using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace Site
{
    public static class Creation
    {
        /// <summary>
        /// Site setback
        /// </summary>
        /// <param name="SiteObject">Site object reference from Revit</param>
        /// <param name="OffsetAmnt">Site setback amount</param>
        /// <param name="HeightLimit">Height limitation</param>
        /// <returns></returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "SiteMass" })]
        public static Dictionary<string, object> SiteMassGenerator(PolySurface SiteObject, double SetbackAmnt, double HeightLimit)
        {
            PolySurface siteMass = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"SiteMass", siteMass},
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
        public static Dictionary<string, object> SiteClashTest(PolySurface BuildingMass, double SiteMass)
        {
            PolySurface volume = null;
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
        [MultiReturn(new[] { "Solids", "BoundingBoxes", "Heights" })]
        public static Dictionary<string, object> SiteContext(PolySurface RevitSite)
        {
            List<Solid> solids = null;
            List<BoundingBox> boundingBoxes = null;
            List<double> heights = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Solids", solids},
                {"BoundingBoxes", boundingBoxes},
                {"Heights", heights}
            };
        }
    }
}
