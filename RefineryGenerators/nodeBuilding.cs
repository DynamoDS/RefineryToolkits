using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    public static class Creation
    {
        /// <summary>
        /// Typology selection
        /// </summary>
        /// <param name="selection">Select building type by index (ex. U, L, I, H, O, D)</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "SelectedType" })]
        public static Dictionary<string, object> SelectBuildingType(int selection)
        {
            string selected = "H";

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"SelectedType", selected},
            };
        }

        /// <summary>
        /// Generates a building mass
        /// </summary>
        /// <param name="Type">Building type (ex. U, L, I, H, O, D)</param>
        /// <param name="Length">Overall building length</param>
        /// <param name="Width">Overall building width</param>
        /// <param name="Offset">Building Floor offset</param>
        /// <param name="BldgArea">Target gross building area</param>
        /// <param name="CreateCore">Create core volumes and subtractions</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "Floors", "Mass", "Cores", "TotalFloorArea", "BuildingVolume", "TopPlane" })]
        public static Dictionary<string, object> BuildingGenerator(string Type, Plane BasePlane, double Length, double Width, double Depth, double BldgArea, double FloorHeight, bool CreateCore)
        {
            List<Surface> floors = null;
            PolySurface mass = null;
            List<PolySurface> cores = null;
            double totalArea = 0;
            double totalVolume = 0;
            Plane topPlane = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Floors", floors},
                {"Mass", mass},
                {"Cores", cores},
                {"TotalFloorArea", totalArea},
                {"BuildingVolume", totalVolume},
                {"TopPlane", topPlane}
            };
        }
    }
    public static class Analysis
    {


        /// <summary>
        /// Deconstructs a building mass into component horizontal and vertical parts 
        /// </summary>
        /// <param name="Mass">Building mass</param>
        /// <param name="tolerance">Tolerance for vertical and horizontal classification</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "VerticalSurfaces", "HoriztonalSurfaces" })]
        public static Dictionary<string, object> DeceonstructFacadeShell(PolySurface Mass, double tolerance)
        {
            List<Surface> horizontal = null;
            List<Surface> vertical = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"VerticalSurfaces", horizontal},
                {"HorizontalSurfaces", vertical}
            };
        }
    }
}
