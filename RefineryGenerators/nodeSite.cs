using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Site
{
    public static class Creation
    {
        /// <summary>
        /// Site setback
        /// </summary>
        /// <param name="SiteOutline">Site boundary, from Revit.</param>
        /// <param name="OffsetAmnt">Site setback amount.</param>
        /// <param name="HeightLimit">Height limitation.</param>
        /// <returns name="SiteMass">Allowable volume for building mass.</returns>
        /// <returns name="SiteOffset">Allowable footprint for building mass.</returns>
        /// <search>site,design,refactory</search>
        [MultiReturn(new[] { "SiteMass", "SiteOffset" })]
        public static Dictionary<string, object> SiteMassGenerator(Curve SiteOutline, double SetbackAmnt, double HeightLimit)
        {
            Solid siteMass = null;
            Curve siteOffset = null;

            if (SetbackAmnt >= 0 && HeightLimit > 0 && SiteOutline != null)
            {
                var inset1 = SiteOutline.Offset(SetbackAmnt);
                var inset2 = SiteOutline.Offset(-SetbackAmnt);

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
        /// Test site boundary against building mass
        /// </summary>
        /// <param name="BuildingMass">Building mass from the generator</param>
        /// <param name="BuildingMass">Site volume</param>
        /// <returns>c, d</returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "IntersectionVolume", "DoesIntersect", "Percent" })]
        public static Dictionary<string, object> SiteClashTest(Solid BuildingMass, Solid SiteMass)
        {
            Solid volume = null;
            bool doesIntersect = false;
            double percent = 0;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"IntersectionVolume", volume},
                {"DoesIntersect", doesIntersect},
                {"Percent", percent}
            };
        }

        /// <summary>
        /// Get site components from Revit element
        /// </summary>
        /// <param name="RevitSite">Reference site element (usually a selected mass)</param>
        /// <returns></returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "Elements", "BoundingBoxes", "Heights" })]
        public static Dictionary<string, object> SiteContext(PolySurface RevitSite)
        {
            List<PolySurface> elements = null;
            List<BoundingBox> boundingBoxes = null;
            List<double> heights = null;

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
