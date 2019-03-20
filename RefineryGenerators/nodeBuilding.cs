using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Buildings
{
    internal enum ShapeType { U, L, I, H, O, D, A }

    /// <summary>
    /// Creation description.
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Typology selection
        /// </summary>
        /// <param name="ShapeIndex">Select building type by index (U, L, I, H, O, D).</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        public static string SelectBuildingType(int ShapeIndex = 0)
        {
            return Enum.GetName(typeof(ShapeType), ShapeIndex % Enum.GetValues(typeof(ShapeType)).Length);
        }

        /// <summary>
        /// Generate a building mass.
        /// </summary>
        /// <param name="Type">Building type (U, L, I, H, O, or D).</param>
        /// <param name="BasePlane">The building base plane.</param>
        /// <param name="Length">Overall building length.</param>
        /// <param name="Width">Overall building width.</param>
        /// <param name="Depth">Building depth.</param>
        /// <param name="BldgArea">Target gross building area.</param>
        /// <param name="FloorHeight">Height of the floor.</param>
        /// <param name="CreateCore">Create core volumes and subtractions?</param>
        /// <returns name="BuildingSolid">Building volume.</returns>
        /// <returns name="Floors">Building floor surfaces.</returns>
        /// <returns name="FloorElevations">Elevation of each floor in building.</returns>
        /// <returns name="Cores">Building core volumes.</returns>
        /// <returns name="TopPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="BuildingVolume">Volume of Mass.</returns>
        /// <returns name="TotalFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="TotalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "BuildingSolid", "Floors", "FloorElevations", "Cores", "TopPlane", "BuildingVolume", "TotalFloorArea", "TotalFacadeArea", })]
        public static Dictionary<string, object> BuildingGenerator(
            Plane BasePlane = null, 
            string Type = "L",
            double Length = 40, 
            double Width = 40, 
            double Depth = 6,
            double BldgArea = 1000, 
            double FloorHeight = 3,
            bool CreateCore = true)
        {
            if (Length <= 0) { throw new ArgumentOutOfRangeException(nameof(Length)); }
            if (Width <= 0) { throw new ArgumentOutOfRangeException(nameof(Width)); }
            if (Depth <= 0) { throw new ArgumentOutOfRangeException(nameof(Depth)); }
            if (BldgArea <= 0) { throw new ArgumentOutOfRangeException(nameof(BldgArea)); }
            if (FloorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(FloorHeight)); }

            if (BasePlane == null)
            {
                BasePlane = Plane.XY();
            }

            if (!Enum.TryParse(Type, out ShapeType shapeType)) { throw new ArgumentOutOfRangeException(nameof(Type)); }

            BuildingBase building = null;

            switch (shapeType)
            {
                case ShapeType.U:
                    building = new BuildingU();
                    break;
                case ShapeType.L:
                    building = new BuildingL();
                    break;
                case ShapeType.I:
                    building = new BuildingI();
                    break;
                case ShapeType.H:
                    building = new BuildingH();
                    break;
                case ShapeType.O:
                    building = new BuildingO();
                    break;
                case ShapeType.D:
                    building = new BuildingD();
                    break;
                case ShapeType.A:
                    building = new BuildingA();
                    break;
                default:
                    throw new NotImplementedException();
            }

            building.CreateBuilding(Length, Width, Depth, BasePlane, BldgArea, FloorHeight, CreateCore);

            return new Dictionary<string, object>
            {
                {"BuildingSolid", building.Mass},
                {"Floors", building.Floors},
                {"FloorElevations", building.FloorElevations},
                {"Cores", building.Cores},
                {"TopPlane", building.TopPlane},
                {"BuildingVolume", building.TotalVolume},
                {"TotalFloorArea", building.TotalArea},
                {"TotalFacadeArea", building.FacadeArea},
            };
        }
    }

    /// <summary>
    /// Analysis description.
    /// </summary>
    public static class Analysis
    {
        /// <summary>
        /// Deconstruct a building mass into component horizontal and vertical surfaces.
        /// </summary>
        /// <param name="Mass">Building mass.</param>
        /// <param name="AngleThreshold">Threshold for classification. 0 (more vertical surfaces) - 90 (more horizontal surfaces).</param>
        /// <returns name="VerticalSurfaces">Vertical surfaces.</returns>
        /// <returns name="HorizontalSurfaces">Horizontal surfaces.</returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "VerticalSurfaces", "HorizontalSurfaces" })]
        public static Dictionary<string, object> DeceonstructFacadeShell(Topology Mass, double AngleThreshold = 45)
        {
            List<Surface> horizontal = new List<Surface>();
            List<Surface> vertical = new List<Surface>();

            if (Mass == null) { throw new ArgumentNullException(nameof(Mass)); }
            if (AngleThreshold < 0 || AngleThreshold > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(AngleThreshold), "AngleThreshold must be between 0 and 90.");
            }

            foreach (var surface in Mass.Faces.Select(f => f.SurfaceGeometry()))
            {
                var angle = surface.NormalAtParameter(0.5, 0.5).AngleWithVector(Vector.ZAxis());
                if (angle < AngleThreshold || angle > 180 - AngleThreshold)
                {
                    horizontal.Add(surface);
                }
                else
                {
                    vertical.Add(surface);
                }
            }

            return new Dictionary<string, object>
            {
                {"VerticalSurfaces", vertical},
                {"HorizontalSurfaces", horizontal}
            };
        }
    }
}
