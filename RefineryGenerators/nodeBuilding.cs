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
        /// <param name="BasePlane">The building base plane.</param>
        /// <param name="Length">Overall building length.</param>
        /// <param name="Width">Overall building width.</param>
        /// <param name="Depth">Building depth.</param>
        /// <param name="BldgArea">Target gross building area.</param>
        /// <param name="FloorHeight">Height of the floor.</param>
        /// <param name="CreateCore">Create core volumes and subtractions?</param>
        /// <returns></returns>
        /// <search>building,design,refinery</search>
        [MultiReturn(new[] { "Floors", "Mass", "Cores", "TotalFloorArea", "BuildingVolume", "TopPlane" })]
        public static Dictionary<string, object> BuildingGenerator(string Type, Plane BasePlane, double Length, double Width, double Depth, double BldgArea, double FloorHeight, bool CreateCore)
        {
            var floors = new List<Surface>();
            PolySurface mass = null;
            List<PolySurface> cores = null;
            double totalArea = 0;
            double totalVolume = 0;
            Plane topPlane = null;

            PolyCurve boundary = null;
            var holes = new List<PolyCurve>();
            
            switch (Type)
            {
                case "I":
                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length)
                    }, connectLastToFirst: true);
                    break;

                case "U":
                    // Center-point of the curved parts of the U.
                    var uArcCenter = Point.ByCoordinates(Width / 2, Width / 2);

                    boundary = PolyCurve.ByJoinedCurves(new Curve[]
                    {
                        PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(0, Width / 2),
                            Point.ByCoordinates(0, Length),
                            Point.ByCoordinates(Depth, Length),
                            Point.ByCoordinates(Depth, Width / 2)
                        }),
                        Arc.ByCenterPointStartPointEndPoint(
                            uArcCenter,
                            Point.ByCoordinates(Depth, Width / 2),
                            Point.ByCoordinates(Width - Depth, Width / 2)
                        ),
                        PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(Width - Depth, Width / 2),
                            Point.ByCoordinates(Width - Depth, Length),
                            Point.ByCoordinates(Width, Length),
                            Point.ByCoordinates(Width, Width / 2)
                        }),
                        Arc.ByCenterPointStartPointEndPoint(
                            uArcCenter,
                            Point.ByCoordinates(0, Width / 2),
                            Point.ByCoordinates(Width, Width / 2)
                        )
                    });

                    break;

                case "L":
                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Depth),
                        Point.ByCoordinates(Depth, Depth),
                        Point.ByCoordinates(Depth, Length),
                        Point.ByCoordinates(0, Length)
                    }, connectLastToFirst: true);
                    break;

                case "H":
                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Depth, 0),
                        Point.ByCoordinates(Depth, (Length - Depth) / 2),
                        Point.ByCoordinates(Width - Depth, (Length - Depth) / 2),
                        Point.ByCoordinates(Width - Depth, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(Width - Depth, Length),
                        Point.ByCoordinates(Width - Depth, (Length + Depth) / 2),
                        Point.ByCoordinates(Depth, (Length + Depth) / 2),
                        Point.ByCoordinates(Depth, Length),
                        Point.ByCoordinates(0, Length)
                    }, connectLastToFirst: true);
                    break;

                case "D":
                    // Center-point of the curved parts of the D.
                    var dArcCenter = Point.ByCoordinates(Width / 2, Width / 2);

                    boundary = PolyCurve.ByJoinedCurves(new Curve[]
                    {
                        PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(Width, Width / 2),
                            Point.ByCoordinates(Width, Length),
                            Point.ByCoordinates(0, Length),
                            Point.ByCoordinates(0, Width / 2)
                        }),
                        Arc.ByCenterPointStartPointEndPoint(
                            dArcCenter,
                            Point.ByCoordinates(0, Width / 2),
                            Point.ByCoordinates(Width, Width / 2)
                        )
                    });

                    holes.Add(PolyCurve.ByJoinedCurves(new Curve[]
                    {
                        PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(Width - Depth, Width / 2),
                            Point.ByCoordinates(Width - Depth, Length - Depth),
                            Point.ByCoordinates(Depth, Length - Depth),
                            Point.ByCoordinates(Depth, Width / 2)
                        }),
                        Arc.ByCenterPointStartPointEndPoint(
                            dArcCenter,
                            Point.ByCoordinates(Depth, Width / 2),
                            Point.ByCoordinates(Width - Depth, Width / 2)
                        )
                    }));
                    
                    break;
            }

            if (boundary != null)
            {
                Surface baseSurface = Surface.ByPatch(boundary);

                if (holes.Count > 0)
                {
                    // A bug in Dynamo requires the boundary curve to be included in the trim curves, otherwise it trims the wrong part.
                    holes.Add(boundary);
                    baseSurface = baseSurface.TrimWithEdgeLoops(holes);
                }

                double floorCount = Math.Ceiling(BldgArea / baseSurface.Area);
                Solid solid = baseSurface.Thicken(floorCount * FloorHeight);

                mass = PolySurface.BySolid(solid);

                for (int i = 0; i < floorCount; i++)
                {
                    floors.Add((Surface)baseSurface.Translate(Vector.ByCoordinates(0, 0, i * FloorHeight)));
                }

                totalArea = baseSurface.Area * floorCount;

                topPlane = (Plane)BasePlane.Translate(Vector.ByCoordinates(0, 0, floorCount * FloorHeight));

            }

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
