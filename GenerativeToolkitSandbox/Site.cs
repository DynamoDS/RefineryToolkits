using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace GenerativeToolkit
{
    /// <summary>
    /// Site description.
    /// </summary>
    public static class Site
    {
        /// <summary>
        /// Use setback and height to create boundary volume for building solid to fit into.
        /// </summary>
        /// <param name="siteOutline">Site boundary, from Revit.</param>
        /// <param name="setback">Site setback distance.</param>
        /// <param name="heightLimit">Maximum building height.</param>
        /// <returns name="siteSolid">Allowable volume for building mass.</returns>
        /// <returns name="siteOffset">Allowable footprint for building mass.</returns>
        /// <search>site,design,refactory</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "siteSolid", "siteOffset" })]
        public static Dictionary<string, object> SiteVolumeGenerator(Curve siteOutline, double setback = 0, double heightLimit = 100)
        {
            Solid siteMass;
            Curve siteOffset;

            if (siteOutline == null) { throw new ArgumentNullException(nameof(siteOutline)); }
            if (setback < 0) { throw new ArgumentOutOfRangeException(nameof(setback), $"{nameof(setback)} must be greater than or equal to 0."); }
            if (heightLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(heightLimit), $"{nameof(heightLimit)} must be greater than 0."); }
            
            var inset1 = siteOutline.Offset(setback);
            var inset2 = siteOutline.Offset(-setback);

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
                heightLimit = -heightLimit;
            }

            siteMass = siteOffset.ExtrudeAsSolid(heightLimit);

            return new Dictionary<string, object>
            {
                {"siteSolid", siteMass},
                {"siteOffset", siteOffset }
            };
        }

        /// <summary>
        /// Test site boundary against building mass.
        /// </summary>
        /// <param name="buildingSolid">Building solid from the generator.</param>
        /// <param name="siteSolid">Site boundary volume.</param>
        /// <returns name="buildingInside">Volume of building inside of site boundary.</returns>
        /// <returns name="buildingOutside">Volume of building outside of site boundary.</returns>
        /// <returns name="intersects">Does the building mass intersect with the site boundary?</returns>
        /// <returns name="percentOutside">Percent of building volume that is outside the site boundary.</returns>
        /// <search>site,design,refinery</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "buildingInside", "buildingOutside", "intersects", "percentOutside" })]
        public static Dictionary<string, object> SiteClashTest(Solid buildingSolid, Solid siteSolid)
        {
            bool intersects = false;
            double percentOutside = 0;

            if (buildingSolid == null) { throw new ArgumentNullException(nameof(buildingSolid)); }
            if (siteSolid == null) { throw new ArgumentNullException(nameof(siteSolid)); }

            Solid outsideVolume = buildingSolid.Difference(siteSolid);
            Solid insideVolume = buildingSolid.Difference(outsideVolume);

            if (outsideVolume != null)
            {
                intersects = true;

                percentOutside = outsideVolume.Volume / buildingSolid.Volume;
            }

            return new Dictionary<string, object>
            {
                {"buildingInside", insideVolume},
                {"buildingOutside", outsideVolume},
                {"intersects", intersects},
                {"percentOutside", percentOutside}
            };
        }

        /// <summary>
        /// Get site components from Revit element.
        /// </summary>
        /// <param name="revitSite">Referenced site element (usually a selected mass).</param>
        /// <returns name="solidList">Individual solids in site geometry.</returns>
        /// <returns name="boundingBoxList">Bounding box for each element.</returns>
        /// <returns name="heightList">Height of each element.</returns>
        /// <search>refinery</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "solidList", "boundingBoxList", "heightList" })]
        public static Dictionary<string, object> SiteContext(PolySurface revitSite)
        {
            Solid[] solidList = null;
            List<BoundingBox> boundingBoxList = null;
            List<double> heightList = null;

            if (revitSite == null) { throw new ArgumentNullException(nameof(revitSite)); }

            solidList = revitSite.ExtractSolids();

            boundingBoxList = solidList.Select(e => e.BoundingBox).ToList();

            heightList = boundingBoxList.Select(b => b.MaxPoint.Z - b.MinPoint.Z).ToList();

            return new Dictionary<string, object>
            {
                {"solidList", solidList},
                {"boundingBoxList", boundingBoxList},
                {"heightList", heightList}
            };
        }

        /// <summary>
        /// Calculate line of sight distances from a grid of points on a surface.
        /// </summary>
        /// <param name="surface">Surface, such as building's facade, to search from.</param>
        /// <param name="contextGeomList">Geometry to calculate distance to.</param>
        /// <param name="startColor">Color of start of preview color range.</param>
        /// <param name="endColor">Color of end of preview color range.</param>
        /// <param name="resolution">Distance between sampling points.</param>
        /// <param name="maxDistance">Maximum distance to search.</param>
        /// <returns name="pointList">Sampling points.</returns>
        /// <returns name="distanceList">Distance from each sampling point along surface normal to the closest context geometry.</returns>
        /// <returns name="geometryColor">Colored surfaces mapping view distance.</returns>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "pointList", "distanceList", "geometryColor" })]
        public static Dictionary<string, object> SiteViewDistance(
            Surface surface, Geometry[] contextGeomList,
            [DefaultArgument("DSCore.Color.ByARGB(255, 255, 255, 255);")]DSCore.Color startColor,
            [DefaultArgument("DSCore.Color.ByARGB(255, 255, 0, 115);")]DSCore.Color endColor,
            double resolution = 3, double maxDistance = 50)
        {
            if (resolution <= 0) { throw new ArgumentNullException(nameof(resolution)); }
            if (maxDistance <= 0) { throw new ArgumentNullException(nameof(maxDistance)); }

            int uCount = (int)Math.Ceiling(surface.GetIsoline(0, 0.5).Length / resolution);
            int vCount = (int)Math.Ceiling(surface.GetIsoline(1, 0.5).Length / resolution);

            var points = new Point[uCount][];
            var distances = new double[uCount][];
            var colors = new DSCore.Color[uCount][];
            
            Vector normal = null;
            Point point = null;
            Geometry[] projected = null;

            // Collide with self as well as all of the context.
            var allBuildings = contextGeomList.Append(surface);

            for (var i = 0; i < uCount; i++)
            {
                points[i] = new Point[vCount];
                distances[i] = new double[vCount];
                colors[i] = new DSCore.Color[vCount];

                for (var j = 0; j < vCount; j++)
                {
                    normal = surface.NormalAtParameter((i + 0.5) / uCount, (j + 0.5) / vCount);
                    point = surface.PointAtParameter((i + 0.5) / uCount, (j + 0.5) / vCount).Add(normal.Scale(0.001));

                    // Find the distance to the closest building, maxing out at MaxDistance.
                    var distance = allBuildings.Select(c =>
                    {
                        projected = point.Project(c, normal);

                        if (projected != null && projected.Length > 0)
                        {
                            return projected
                                .Select(p => point.DistanceTo(p))
                                .Min();
                        }
                        else
                        {
                            return maxDistance;
                        }
                    }).Append(maxDistance).Min();

                    points[i][j] = point;
                    distances[i][j] = distance;

                    colors[i][j] = DSCore.Color.Lerp(startColor, endColor, distance / maxDistance);
                }
            }

            var geometry = Modifiers.GeometryColor.BySurfaceColors(surface, colors);
            
            normal?.Dispose();
            projected?.ForEach(x => x.Dispose());

            return new Dictionary<string, object>
            {
                {"pointList", points},
                {"distanceList", distances},
                {"geometryColor",  geometry}
            };
        }
    }
}
