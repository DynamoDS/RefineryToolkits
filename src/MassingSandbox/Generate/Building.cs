using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal enum ShapeType { U, L, H, O, D }

    /// <summary>
    /// Buildings description.
    /// </summary>
    public static class Building
    {
        /// <summary>
        /// Initializes a building object.
        /// </summary>
        /// <returns></returns>
        private static BuildingBase InitializeBuilding(string shape)
        {
            BuildingBase building;

            if (shape == "A")
            {
                building = new BuildingA();
            }
            else if (Enum.TryParse(shape, out ShapeType shapeType))
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
            else
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "Unsupported shape letter.");
            }

            return building;
        }

        /// <summary>
        /// Uses an integer index to select a building type letter from the list of possible options ("U", "L", "H", "O", or "D").
        /// </summary>
        /// <param name="shapeIndex">Index of building type to select from list of possible types.</param>
        /// <returns name="buildingTypeStr">Name of building type. One of "U", "L", "H", "O", or "D".</returns>
        /// <search>building,design,refinery</search>
        public static string SelectBuildingType(int shapeIndex = 0)
        {
            return Enum.GetName(typeof(ShapeType), shapeIndex % Enum.GetValues(typeof(ShapeType)).Length);
        }

        /// <summary>
        /// Generates a building solid by shape type and target gross area.
        /// </summary>
        /// <param name="typeStr">Building type name ("U", "L", "H", "O", or "D").</param>
        /// <param name="basePlane">The building base plane.</param>
        /// <param name="length">Overall building length.</param>
        /// <param name="width">Overall building width.</param>
        /// <param name="depth">Building depth.</param>
        /// <param name="buildingArea">Target gross building area.</param>
        /// <param name="floorHeight">Height of the floors.</param>
        /// <param name="curvedBool">Should sides of building be curved or faceted?</param>
        /// <param name="createCoreBool">Create core volumes and subtractions?</param>
        /// <param name="hallwayToDepth">Core sizing logic: ratio between building depth and width of hallways on either side of core.</param>
        /// <param name="coreSizeFactorFloors">Core sizing logic: Add <code>(# of floors) * coreSizeFactorFloors</code> area to core footprint.</param>
        /// <param name="coreSizeFactorArea">Core sizing logic: Add <code>(single floor area) * coreSizeFactorArea</code> area to core footprint.</param>
        /// <returns name="buildingSolid">Building volume.</returns>
        /// <returns name="floorSrfList">Building floor surfaces.</returns>
        /// <returns name="netFloorSrfList">Building floor surfaces with core removed.</returns>
        /// <returns name="floorElevationList">Elevation of each floor in building.</returns>
        /// <returns name="coreSolidList">Building core volumes.</returns>
        /// <returns name="topPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="buildingVolume">Volume of entire building solid.</returns>
        /// <returns name="grossFloorArea">Combined area of all floors. Will be greater than or equal to input buildingArea.</returns>
        /// <returns name="netFloorArea">Combined area of all floors with core removed.</returns>
        /// <returns name="totalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "buildingSolid", "floorSrfList", "netFloorSrfList", "floorElevationList", "coreSolidList", "topPlane", "buildingVolume", "grossFloorArea", "netFloorArea", "totalFacadeArea", })]
        public static Dictionary<string, object> ByTypeArea(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Plane.XY();")]Plane basePlane = null,
            string typeStr = "L",
            double length = 50,
            double width = 40,
            double depth = 25,
            double buildingArea = 10000,
            double floorHeight = 9,
            bool curvedBool = false,
            bool createCoreBool = true,
            double hallwayToDepth = 0.1,
            double coreSizeFactorFloors = 4,
            double coreSizeFactorArea = 0.1)
        {
            if (length <= 0) { throw new ArgumentOutOfRangeException(nameof(length)); }
            if (width <= 0) { throw new ArgumentOutOfRangeException(nameof(width)); }
            if (depth <= 0) { throw new ArgumentOutOfRangeException(nameof(depth)); }
            if (buildingArea <= 0) { throw new ArgumentOutOfRangeException(nameof(buildingArea)); }
            if (floorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(floorHeight)); }

            var building = InitializeBuilding(typeStr);

            building.CreateBuilding(basePlane, floorHeight, 
                buildingArea, floorCount: null, 
                width, length, depth, curvedBool, createCoreBool, hallwayToDepth, coreSizeFactorFloors, coreSizeFactorArea);

            building.DisposeNonExports();

            return new Dictionary<string, object>
            {
                {"buildingSolid", building.Mass},
                {"floorSrfList", building.Floors},
                {"netFloorSrfList", building.NetFloors},
                {"floorElevationList", building.FloorElevations},
                {"coreSolidList", building.Cores},
                {"topPlane", building.TopPlane},
                {"buildingVolume", building.TotalVolume},
                {"grossFloorArea", building.GrossFloorArea},
                {"netFloorArea", building.NetFloorArea},
                {"totalFacadeArea", building.FacadeArea},
            };
        }

        /// <summary>
        /// Generates a building solid by shape type and number of floors.
        /// </summary>
        /// <param name="typeStr">Building type (U, L, H, O, or D).</param>
        /// <param name="basePlane">The building base plane.</param>
        /// <param name="length">Overall building length.</param>
        /// <param name="width">Overall building width.</param>
        /// <param name="depth">Building depth.</param>
        /// <param name="floorCount">Number of building floors.</param>
        /// <param name="floorHeight">Height of the floors.</param>
        /// <param name="curvedBool">Should sides of building be curved or faceted?</param>
        /// <param name="createCoreBool">Create core volumes and subtractions?</param>
        /// <param name="hallwayToDepth">Core sizing logic: ratio between building depth and width of hallways on either side of core.</param>
        /// <param name="coreSizeFactorFloors">Core sizing logic: Add <code>(# of floors) * CoreSizeFactorFloors</code> area to core footprint.</param>
        /// <param name="coreSizeFactorArea">Core sizing logic: Add <code>(single floor area) * CoreSizeFactorArea</code> area to core footprint.</param>
        /// <returns name="buildingSolid">Building volume.</returns>
        /// <returns name="floorSrfList">Building floor surfaces.</returns>
        /// <returns name="netFloorSrfList">Building floor surfaces with core removed.</returns>
        /// <returns name="floorElevationList">Elevation of each floor in building.</returns>
        /// <returns name="coreSolidList">Building core volumes.</returns>
        /// <returns name="topPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="buildingVolume">Volume of entire building solid.</returns>
        /// <returns name="grossFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="netFloorArea">Combined area of all floors with core removed.</returns>
        /// <returns name="totalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "buildingSolid", "floorSrfList", "netFloorSrfList", "floorElevationList", "coreSolidList", "topPlane", "buildingVolume", "grossFloorArea", "netFloorArea", "totalFacadeArea", })]
        public static Dictionary<string, object> ByTypeFloors(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Plane.XY();")]Plane basePlane = null,
            string typeStr = "L",
            double length = 50,
            double width = 40,
            double depth = 25,
            int floorCount = 10,
            double floorHeight = 9,
            bool curvedBool = false,
            bool createCoreBool = true,
            double hallwayToDepth = 0.1,
            double coreSizeFactorFloors = 4,
            double coreSizeFactorArea = 0.1)
        {
            if (length <= 0) { throw new ArgumentOutOfRangeException(nameof(length)); }
            if (width <= 0) { throw new ArgumentOutOfRangeException(nameof(width)); }
            if (depth <= 0) { throw new ArgumentOutOfRangeException(nameof(depth)); }
            if (floorCount <= 0) { throw new ArgumentOutOfRangeException(nameof(floorCount)); }
            if (floorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(floorHeight)); }

            var building = InitializeBuilding(typeStr);

            building.CreateBuilding(basePlane, floorHeight, 
                targetBuildingArea: null, floorCount: floorCount, 
                width, length, depth, curvedBool, createCoreBool, hallwayToDepth, coreSizeFactorFloors, coreSizeFactorArea);

            building.DisposeNonExports();

            return new Dictionary<string, object>
            {
                {"buildingSolid", building.Mass},
                {"floorSrfList", building.Floors},
                {"netFloorSrfList", building.NetFloors},
                {"floorElevationList", building.FloorElevations},
                {"coreSolidList", building.Cores},
                {"topPlane", building.TopPlane},
                {"buildingVolume", building.TotalVolume},
                {"grossFloorArea", building.GrossFloorArea},
                {"netFloorArea", building.NetFloorArea},
                {"totalFacadeArea", building.FacadeArea},
            };
        }

        /// <summary>
        /// Deconstructs a building solid into component horizontal and vertical surfaces.
        /// </summary>
        /// <param name="solid">Building solid.</param>
        /// <param name="angleThreshold">Threshold for classification. 0 (more vertical surfaces) - 90 (more horizontal surfaces).</param>
        /// <returns name="verticalSrfList">Vertical surfaces.</returns>
        /// <returns name="horizontalSrfList">Horizontal surfaces.</returns>
        /// <search>building,design,refinery</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "verticalSrfList", "horizontalSrfList" })]
        public static Dictionary<string, object> DeconstructFacadeShell(Topology solid, double angleThreshold = 45)
        {
            List<Surface> horizontal = new List<Surface>();
            List<Surface> vertical = new List<Surface>();

            if (solid == null) { throw new ArgumentNullException(nameof(solid)); }
            if (angleThreshold < 0 || angleThreshold > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(angleThreshold), $"{nameof(angleThreshold)} must be between 0 and 90.");
            }

            foreach (var surface in solid.Faces.Select(f => f.SurfaceGeometry()))
            {
                using (var zAxis = Vector.ZAxis())
                using (var surfaceNormal = surface.NormalAtParameter(0.5, 0.5))
                {
                    var angle = surfaceNormal.AngleWithVector(zAxis);
                    if (angle < angleThreshold || angle > 180 - angleThreshold)
                    {
                        horizontal.Add(surface);
                    }
                    else
                    {
                        vertical.Add(surface);
                    }
                }
            }

            return new Dictionary<string, object>
            {
                {"verticalSrfList", vertical},
                {"horizontalSrfList", horizontal}
            };
        }

        /// <summary>
        /// Gets a list of closed polycurve edges of surface. First list item is outside boundary.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <returns name="edgeCrvList">Edges of surface.</returns>
        /// <exception cref="ArgumentNullException">Surface</exception>
        public static PolyCurve[] GetSurfaceLoops(Surface surface)
        {
            if (surface == null) { throw new ArgumentNullException(nameof(surface)); }

            var curves = surface.PerimeterCurves();

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

            return loops.OrderByDescending(c =>
            {
                using (var s = Surface.ByPatch(c))
                {
                    return s.Area;
                }
            }).ToArray();
        }

        /// <summary>
        /// Generates a building solid from base curves and target gross area.
        /// </summary>
        /// <param name="edgeLoopCrvList">Closed curve boundaries of building. All curves after first will be treated as holes.</param>
        /// <param name="buildingArea">Target gross building area.</param>
        /// <param name="floorHeight">Height of the floors.</param>
        /// <returns name="buildingSolid">Building volume.</returns>
        /// <returns name="floorSrfList">Building floor surfaces.</returns>
        /// <returns name="floorElevationList">Elevation of each floor in building.</returns>
        /// <returns name="topPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="buildingVolume">Volume of entire building solid.</returns>
        /// <returns name="grossFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="totalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "buildingSolid", "floorSrfList", "floorElevationList", "topPlane", "buildingVolume", "grossFloorArea", "totalFacadeArea", })]
        public static Dictionary<string, object> ByOutlineArea(
            List<Curve> edgeLoopCrvList,
            double buildingArea = 10000,
            double floorHeight = 3)
        {
            if (edgeLoopCrvList == null || edgeLoopCrvList.Count == 0) { throw new ArgumentNullException(nameof(edgeLoopCrvList)); }
            if (buildingArea <= 0) { throw new ArgumentOutOfRangeException(nameof(buildingArea)); }
            if (floorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(floorHeight)); }

            var building = new BuildingFromCurves(edgeLoopCrvList[0], edgeLoopCrvList.Skip(1).ToList(), floorHeight, buildingArea, floorCount: null);

            building.DisposeNonExports();
            building.DisposeCores();

            return new Dictionary<string, object>
            {
                {"buildingSolid", building.Mass},
                {"floorSrfList", building.Floors},
                {"floorElevationList", building.FloorElevations},
                {"topPlane", building.TopPlane},
                {"buildingVolume", building.TotalVolume},
                {"grossFloorArea", building.GrossFloorArea},
                {"totalFacadeArea", building.FacadeArea},
            };
        }

        /// <summary>
        /// Generates a building solid from base curves and number of floors.
        /// </summary>
        /// <param name="edgeLoopCrvList">Closed curve boundaries of building. All curves after first will be treated as holes.</param>
        /// <param name="floorCount">Target gross building area.</param>
        /// <param name="floorHeight">Height of the floors.</param>
        /// <returns name="buildingSolid">Building volume.</returns>
        /// <returns name="floorSrfList">Building floor surfaces.</returns>
        /// <returns name="floorElevationList">Elevation of each floor in building.</returns>
        /// <returns name="topPlane">A plane at the top of the building volume. Use this for additional volumes to create a stacked building.</returns>
        /// <returns name="buildingVolume">Volume of entire building solid.</returns>
        /// <returns name="grossFloorArea">Combined area of all floors. Will be at least equal to BldgArea.</returns>
        /// <returns name="totalFacadeArea">Combined area of all facades (vertical surfaces).</returns>
        /// <search>building,design,refinery</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] { "buildingSolid", "floorSrfList", "floorElevationList", "topPlane", "buildingVolume", "grossFloorArea", "totalFacadeArea", })]
        public static Dictionary<string, object> ByOutlineFloors(
            List<Curve> edgeLoopCrvList,
            int floorCount = 10,
            double floorHeight = 3)
        {
            if (edgeLoopCrvList == null || edgeLoopCrvList.Count == 0) { throw new ArgumentNullException(nameof(edgeLoopCrvList)); }
            if (floorCount <= 0) { throw new ArgumentOutOfRangeException(nameof(floorCount)); }
            if (floorHeight <= 0) { throw new ArgumentOutOfRangeException(nameof(floorHeight)); }

            var building = new BuildingFromCurves(edgeLoopCrvList[0], edgeLoopCrvList.Skip(1).ToList(), floorHeight, targetBuildingArea: null, floorCount);

            building.DisposeNonExports();
            building.DisposeCores();

            return new Dictionary<string, object>
            {
                {"buildingSolid", building.Mass},
                {"floorSrfList", building.Floors},
                {"floorElevationList", building.FloorElevations},
                {"topPlane", building.TopPlane},
                {"buildingVolume", building.TotalVolume},
                {"grossFloorArea", building.GrossFloorArea},
                {"totalFacadeArea", building.FacadeArea},
            };
        }
    }
}
