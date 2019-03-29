using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace GenerativeToolkit
{
    internal enum ShapeType { U, L, H, O, D }

    /// <summary>
    /// Buildings description.
    /// </summary>
    public static class Building
    {
        /// <summary>
        /// Typology selection
        /// </summary>
        /// <param name="ShapeIndex">Select building type by index (U, L, H, O, D).</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        public static string SelectBuildingType(int ShapeIndex = 0)
        {
            return Enum.GetName(typeof(ShapeType), ShapeIndex % Enum.GetValues(typeof(ShapeType)).Length);
        }

        /// <summary>
        /// Generate a building mass.
        /// </summary>
        /// <param name="Type">Building type (U, L, H, O, or D).</param>
        /// <param name="BasePlane">The building base plane.</param>
        /// <param name="Length">Overall building length.</param>
        /// <param name="Width">Overall building width.</param>
        /// <param name="Depth">Building depth.</param>
        /// <param name="BldgArea">Target gross building area.</param>
        /// <param name="FloorHeight">Height of the floor.</param>
        /// <param name="IsCurved">Should sides of building be curved or faceted?</param>
        /// <param name="CreateCore">Create core volumes and subtractions?</param>
        /// <param name="HallwayToDepth">Core sizing logic: ratio between building depth and width of hallways on either side of core.</param>
        /// <param name="CoreSizeFactorFloors">Core sizing logic: Add <code>(# of floors) * CoreSizeFactorFloors</code> area to core footprint.</param>
        /// <param name="CoreSizeFactorArea">Core sizing logic: Add <code>(single floor area) * CoreSizeFactorArea</code> area to core footprint.</param>
        /// <returns name="BuildingSolid">Building volume.</returns>
        /// <returns name="Floors">Building floor surfaces.</returns>
        /// <returns name="NetFloors">Building floor surfaces with core removed.</returns>
        /// <returns name="FloorElevations">Elevation of each floor in building.</returns>
        /// <returns name="Cores">Building core volumes.</returns>
        /// <returns name="TopPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="BuildingVolume">Volume of Mass.</returns>
        /// <returns name="GrossFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="NetFloorArea">Combined area of all floors with core removed.</returns>
        /// <returns name="TotalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "BuildingSolid", "Floors", "NetFloors", "FloorElevations", "Cores", "TopPlane", "BuildingVolume", "GrossFloorArea", "NetFloorArea", "TotalFacadeArea", })]
        public static Dictionary<string, object> BuildingGenerator(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Plane.XY();")]Plane BasePlane = null,
            string Type = "L",
            double Length = 50,
            double Width = 40,
            double Depth = 25,
            double BldgArea = 10000,
            double FloorHeight = 9,
            bool IsCurved = false,
            bool CreateCore = true,
            double HallwayToDepth = 0.1,
            double CoreSizeFactorFloors = 4,
            double CoreSizeFactorArea = 0.1)
        {
            if (Length <= 0) { throw new ArgumentOutOfRangeException(nameof(Length)); }
            if (Width <= 0) { throw new ArgumentOutOfRangeException(nameof(Width)); }
            if (Depth <= 0) { throw new ArgumentOutOfRangeException(nameof(Depth)); }
            if (BldgArea <= 0) { throw new ArgumentOutOfRangeException(nameof(BldgArea)); }
            if (FloorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(FloorHeight)); }

            BuildingBase building = null;

            if (Enum.TryParse(Type, out ShapeType shapeType))
            {
                switch (shapeType)
                {
                    case ShapeType.U:
                        building = new BuildingU();
                        break;
                    case ShapeType.L:
                        building = new BuildingL();
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
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (Type == "A")
            {
                building = new BuildingA();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Type), "Unsupported shape letter.");
            }

            building.CreateBuilding(BasePlane, BldgArea, FloorHeight, Width, Length, Depth, IsCurved, CreateCore, HallwayToDepth, CoreSizeFactorFloors, CoreSizeFactorArea);

            return new Dictionary<string, object>
            {
                {"BuildingSolid", building.Mass},
                {"Floors", building.Floors},
                {"NetFloors", building.NetFloors},
                {"FloorElevations", building.FloorElevations},
                {"Cores", building.Cores},
                {"TopPlane", building.TopPlane},
                {"BuildingVolume", building.TotalVolume},
                {"GrossFloorArea", building.GrossFloorArea},
                {"NetFloorArea", building.NetFloorArea},
                {"TotalFacadeArea", building.FacadeArea},
            };
        }

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

        /// <summary>
        /// Get list of closed polycurve edges of surface. First list item is outside boundary.
        /// </summary>
        /// <param name="Surface">The surface.</param>
        /// <returns name="Edges">Edges of surface.</returns>
        /// <exception cref="ArgumentNullException">Surface</exception>
        public static PolyCurve[] GetSurfaceLoops(Surface Surface)
        {
            if (Surface == null) { throw new ArgumentNullException(nameof(Surface)); }

            var curves = Surface.PerimeterCurves();

            var loops = new List<PolyCurve>();

            foreach (var curve in curves)
            {
                var added = false;

                for (var i = 0; i < loops.Count; i++)
                {
                    var loop = loops[i];

                    if (loop.IsClosed) { continue; }

                    if (loop.StartPoint.IsAlmostEqualTo(curve.StartPoint)
                        || loop.StartPoint.IsAlmostEqualTo(curve.EndPoint)
                        || loop.EndPoint.IsAlmostEqualTo(curve.StartPoint)
                        || loop.EndPoint.IsAlmostEqualTo(curve.EndPoint))
                    {
                        try
                        {
                            loops[i] = loop.Join(new[] { curve });

                            added = true;
                            break;
                        }
                        catch (ApplicationException)
                        {
                            continue;
                        }
                    }
                }

                if (!added)
                {
                    loops.Add(PolyCurve.ByJoinedCurves(new[] { curve }));
                }

                curve.Dispose();
            }

            if (loops.Any(loop => !loop.IsClosed)) { throw new ArgumentException("Created non-closed polycurve."); }

            return loops.OrderByDescending(c => Surface.ByPatch(c).Area).ToArray();
        }

        /// <summary>
        /// Generate a building mass from base curves.
        /// </summary>
        /// <param name="EdgeLoops">Closed curve boundary of building. All curves after first will be treated as holes.</param>
        /// <param name="BldgArea">Target gross building area.</param>
        /// <param name="FloorHeight">Height of the floor.</param>
        /// <returns name="BuildingSolid">Building volume.</returns>
        /// <returns name="Floors">Building floor surfaces.</returns>
        /// <returns name="FloorElevations">Elevation of each floor in building.</returns>
        /// <returns name="TopPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="BuildingVolume">Volume of Mass.</returns>
        /// <returns name="GrossFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="TotalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "BuildingSolid", "Floors", "FloorElevations", "TopPlane", "BuildingVolume", "GrossFloorArea", "TotalFacadeArea", })]
        public static Dictionary<string, object> BuildingGeneratorByCurves(
            List<Curve> EdgeLoops,
            double BldgArea = 1000,
            double FloorHeight = 3)
        {
            if (EdgeLoops == null || EdgeLoops.Count == 0) { throw new ArgumentNullException(nameof(EdgeLoops)); }

            var building = new BuildingFromCurves(EdgeLoops[0], EdgeLoops.Skip(1).ToList(), BldgArea, FloorHeight);

            return new Dictionary<string, object>
            {
                {"BuildingSolid", building.Mass},
                {"Floors", building.Floors},
                {"FloorElevations", building.FloorElevations},
                {"TopPlane", building.TopPlane},
                {"BuildingVolume", building.TotalVolume},
                {"GrossFloorArea", building.GrossFloorArea},
                {"TotalFacadeArea", building.FacadeArea},
            };
        }
    }
}
