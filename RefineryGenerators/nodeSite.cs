using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Site
{
    public static class Creation
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

            if (Setback >= 0 && HeightLimit > 0 && SiteOutline != null)
            {
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
            }

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"SiteMass", siteMass},
                {"SiteOffset", siteOffset }
            };
        }
    }

    public static class Analysis
    {
        /// <summary>
        /// Test site boundary against building mass.
        /// </summary>
        /// <param name="BuildingMass">Building mass from the generator</param>
        /// <param name="SiteMass">Site boundary volume.</param>
        /// <returns name="IntersectionVolume">Volume of building outside of site boundary.</returns>
        /// <returns name="DoesIntersect">Does the building mass intersect with the site boundary?</returns>
        /// <returns name="Percent">Percent of building volume that is outside the site boundary.</returns>
        /// <search>site,design,refinery</search>
        [MultiReturn(new[] { "IntersectionVolume", "DoesIntersect", "Percent" })]
        public static Dictionary<string, object> SiteClashTest(Solid BuildingMass, Solid SiteMass)
        {
            Solid volume = null;
            bool doesIntersect = false;
            double percent = 0;

            if (BuildingMass != null && SiteMass != null)
            {
                volume = BuildingMass.Difference(SiteMass);

                if (volume != null)
                {
                    doesIntersect = true;

                    percent = volume.Volume / BuildingMass.Volume;
                }
            }

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"IntersectionVolume", volume},
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

            elements = RevitSite.ExtractSolids();

            boundingBoxes = elements.Select(e => e.BoundingBox).ToList();

            heights = boundingBoxes.Select(b => b.MaxPoint.Z - b.MinPoint.Z).ToList();

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Elements", elements},
                {"BoundingBoxes", boundingBoxes},
                {"Heights", heights}
            };
        }
    }
}
