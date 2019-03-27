using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace GenerativeToolkit
{
    /// <summary>
    /// Site description.
    /// </summary>
    public static class Site
    {
        /// <summary>
        /// Use setback and height to create boundary volume for building mass to fit into.
        /// </summary>
        /// <param name="SiteOutline">Site boundary, from Revit.</param>
        /// <param name="Setback">Site setback distance.</param>
        /// <param name="HeightLimit">Maximum building height.</param>
        /// <returns name="SiteMass">Allowable volume for building mass.</returns>
        /// <returns name="SiteOffset">Allowable footprint for building mass.</returns>
        /// <search>site,design,refactory</search>
        [MultiReturn(new[] { "SiteMass", "SiteOffset" })]
        public static Dictionary<string, object> SiteMassGenerator(Curve SiteOutline, double Setback = 0, double HeightLimit = 100)
        {
            Solid siteMass = null;
            Curve siteOffset = null;

            if (SiteOutline == null) { throw new ArgumentNullException(nameof(SiteOutline)); }
            if (Setback <= 0) { throw new ArgumentOutOfRangeException(nameof(Setback), $"{nameof(Setback)} must be greater than 0."); }
            if (HeightLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(HeightLimit), $"{nameof(HeightLimit)} must be greater than 0."); }
            
            var inset1 = SiteOutline.Offset(Setback);
            var inset2 = SiteOutline.Offset(-Setback);

            if (inset1.Length < inset2.Length)
            {
                siteOffset = inset1;
                inset2.Dispose();
            }
            else
            {
                siteOffset = inset2;
                inset1.Dispose();
            }

            // Ensure that the mass is always extruded upwards.
            if (siteOffset.Normal.AngleWithVector(Vector.ZAxis()) > 90)
            {
                HeightLimit = -HeightLimit;
            }

            siteMass = siteOffset.ExtrudeAsSolid(HeightLimit);

            return new Dictionary<string, object>
            {
                {"SiteMass", siteMass},
                {"SiteOffset", siteOffset }
            };
        }

        /// <summary>
        /// Test site boundary against building mass.
        /// </summary>
        /// <param name="BuildingMass">Building mass from the generator</param>
        /// <param name="SiteMass">Site boundary volume.</param>
        /// <returns name="BuildingInside">Volume of building inside of site boundary.</returns>
        /// <returns name="BuildingOutside">Volume of building outside of site boundary.</returns>
        /// <returns name="DoesIntersect">Does the building mass intersect with the site boundary?</returns>
        /// <returns name="Percent">Percent of building volume that is outside the site boundary.</returns>
        /// <search>site,design,refinery</search>
        [MultiReturn(new[] { "BuildingInside", "BuildingOutside", "DoesIntersect", "Percent" })]
        public static Dictionary<string, object> SiteClashTest(Solid BuildingMass, Solid SiteMass)
        {
            Solid insideVolume = null;
            Solid outsideVolume = null;
            bool doesIntersect = false;
            double percent = 0;

            if (BuildingMass == null) { throw new ArgumentNullException(nameof(BuildingMass)); }
            if (SiteMass == null) { throw new ArgumentNullException(nameof(SiteMass)); }

            outsideVolume = BuildingMass.Difference(SiteMass);
            insideVolume = BuildingMass.Difference(outsideVolume);

            if (outsideVolume != null)
            {
                doesIntersect = true;

                percent = outsideVolume.Volume / BuildingMass.Volume;
            }

            return new Dictionary<string, object>
            {
                {"BuildingInside", insideVolume},
                {"BuildingOutside", outsideVolume},
                {"DoesIntersect", doesIntersect},
                {"Percent", percent}
            };
        }

        /// <summary>
        /// Get site components from Revit element.
        /// </summary>
        /// <param name="RevitSite">Referenced site element (usually a selected mass).</param>
        /// <returns name="Elements">Individual solids in site geometry.</returns>
        /// <returns name="BoundingBoxes">Bounding box for each element.</returns>
        /// <returns name="Heights">Height of each element.</returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "Elements", "BoundingBoxes", "Heights" })]
        public static Dictionary<string, object> SiteContext(PolySurface RevitSite)
        {
            Solid[] elements = null;
            List<BoundingBox> boundingBoxes = null;
            List<double> heights = null;

            if (RevitSite == null) { throw new ArgumentNullException(nameof(RevitSite)); }

            elements = RevitSite.ExtractSolids();

            boundingBoxes = elements.Select(e => e.BoundingBox).ToList();

            heights = boundingBoxes.Select(b => b.MaxPoint.Z - b.MinPoint.Z).ToList();

            return new Dictionary<string, object>
            {
                {"Elements", elements},
                {"BoundingBoxes", boundingBoxes},
                {"Heights", heights}
            };
        }

        /// <summary>
        /// Calculate line of sight distances from a grid of points on a surface.
        /// </summary>
        /// <param name="Surface">Surface, such as building's facade, to search from.</param>
        /// <param name="Context">Objects to calculate distance to.</param>
        /// <param name="Resolution">Distance between sampling points.</param>
        /// <param name="MaxDistance">Maximum distance to search.</param>
        /// <returns name="Points">Sampling points.</returns>
        /// <returns name="Distances">Distance from each sampling point along surface normal to the closest context geometry.</returns>
        /// <returns name="Geometry">Colored surfaces mapping view distance.</returns>
        [MultiReturn(new[] { "Points", "Distances", "Geometry" })]
        public static Dictionary<string, object> SiteViewDistance(Surface Surface, Geometry[] Context, double Resolution = 3, double MaxDistance = 50)
        {
            int uCount = (int)Math.Ceiling(Surface.GetIsoline(0, 0.5).Length / Resolution);
            int vCount = (int)Math.Ceiling(Surface.GetIsoline(1, 0.5).Length / Resolution);

            var points = new Point[uCount][];
            var distances = new double[uCount][];
            var colors = new DSCore.Color[uCount][];
            
            Vector normal = null;
            Point point = null;
            Geometry[] projected = null;

            for (var i = 0; i < uCount; i++)
            {
                points[i] = new Point[vCount];
                distances[i] = new double[vCount];
                colors[i] = new DSCore.Color[vCount];

                for (var j = 0; j < vCount; j++)
                {
                    normal = Surface.NormalAtParameter((i + 0.5) / uCount, (j + 0.5) / vCount);
                    point = Surface.PointAtParameter((i + 0.5) / uCount, (j + 0.5) / vCount);

                    var distance = Context.Select(c =>
                    {
                        projected = point.Project(c, normal);

                        if (projected != null && projected.Length > 0)
                        {
                            return projected.Select(p => point.DistanceTo(p)).Min();
                        }
                        else
                        {
                            return MaxDistance;
                        }
                    }).Min();

                    points[i][j] = point;
                    distances[i][j] = distance;

                    int color = (int)(distance / MaxDistance * 255);
                    colors[i][j] = DSCore.Color.ByARGB(255, 255, color, 255);
                }
            }

            var geometry = Modifiers.GeometryColor.BySurfaceColors(Surface, colors);
            
            normal?.Dispose();
            projected?.ForEach(x => x.Dispose());

            return new Dictionary<string, object>
            {
                {"Points", points},
                {"Distances", distances},
                {"Geometry",  geometry}
            };
        }
    }
}
