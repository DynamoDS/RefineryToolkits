using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Buildings
{
    /// <summary>
    /// Creation description.
    /// </summary>
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
        /// Generate a building mass.
        /// </summary>
        /// <param name="Type">Building type (ex. U, L, I, H, O, D)</param>
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
            var floors = new List<Surface>();
            var floorElevations = new List<double>();
            Solid mass = null;
            List<Solid> cores = null;
            double totalArea = 0;
            double totalVolume = 0;
            double facadeArea = 0;
            Plane topPlane = null;

            if (Length <= 0 || Width <= 0 || Depth <= 0 || BldgArea <= 0 || FloorHeight <= 0)
            {
                return new Dictionary<string, object>();
            }

            if (BasePlane == null)
            {
                BasePlane = Plane.XY();
            }

            Surface baseSurface = MakeBaseSurface(Type, Length, Width, Depth);

            if (baseSurface != null)
            {
                // Surface is constructed with lower left corner at (0,0). Move and rotate to given base plane.
                baseSurface = (Surface)baseSurface.Transform(CoordinateSystem.ByOrigin(Width / 2, Length / 2), BasePlane.ToCoordinateSystem());

                double floorCount = Math.Ceiling(BldgArea / baseSurface.Area);

                mass = baseSurface.Thicken(floorCount * FloorHeight, both_sides: false);

                totalVolume = mass.Volume;

                facadeArea = mass.Area - (2 * baseSurface.Area);

                for (int i = 0; i < floorCount; i++)
                {
                    floors.Add((Surface)baseSurface.Translate(Vector.ByCoordinates(0, 0, i * FloorHeight)));
                    floorElevations.Add(BasePlane.Origin.Z + (i * FloorHeight));
                }

                totalArea = baseSurface.Area * floorCount;

                topPlane = (Plane)BasePlane.Translate(Vector.ByCoordinates(0, 0, floorCount * FloorHeight));

                baseSurface.Dispose();
            }

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"BuildingSolid", mass},
                {"Floors", floors},
                {"FloorElevations", floorElevations},
                {"Cores", cores},
                {"TopPlane", topPlane},
                {"BuildingVolume", totalVolume},
                {"TotalFloorArea", totalArea},
                {"TotalFacadeArea", facadeArea},
            };
        }

        private static Surface MakeBaseSurface(string Type, double Length, double Width, double Depth)
        {
            Curve boundary = null;
            var holes = new List<Curve>();
            Surface baseSurface = null;

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
                    if (Length <= Depth || Width <= Depth * 2)
                    {
                        break;
                    }

                    if (Length > Width / 2)
                    {
                        // Enough room to make the curved part of the U an arc.

                        // Center-point of the curved parts of the U.
                        using (var arcCenter = Point.ByCoordinates(Width / 2, Width / 2))
                        {
                            boundary = PolyCurve.ByJoinedCurves(new Curve[]
                            {
                                PolyCurve.ByPoints(new[]
                                {
                                    Point.ByCoordinates(Depth, Width / 2),
                                    Point.ByCoordinates(Depth, Length),
                                    Point.ByCoordinates(0, Length),
                                    Point.ByCoordinates(0, Width / 2)
                                }),
                                Arc.ByCenterPointStartPointEndPoint(
                                    arcCenter,
                                    Point.ByCoordinates(0, Width / 2),
                                    Point.ByCoordinates(Width, Width / 2)
                                ),
                                PolyCurve.ByPoints(new[]
                                {
                                    Point.ByCoordinates(Width, Width / 2),
                                    Point.ByCoordinates(Width, Length),
                                    Point.ByCoordinates(Width - Depth, Length),
                                    Point.ByCoordinates(Width - Depth, Width / 2)
                                }),
                                Arc.ByCenterPointStartPointSweepAngle(
                                    arcCenter,
                                    Point.ByCoordinates(Width - Depth, Width / 2),
                                    -180,
                                    Vector.ZAxis()
                                )
                            });
                        }
                    }
                    else
                    {
                        // Short U. Use ellipses and no straight part.
                        using (var ellipseCenter = Plane.ByOriginNormal(
                            Point.ByCoordinates(Width / 2, Length),
                            Vector.ZAxis()))
                        {
                            boundary = PolyCurve.ByJoinedCurves(new Curve[]
                            {
                            Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(Width - Depth, Length)),
                            EllipseArc.ByPlaneRadiiAngles(ellipseCenter, (Width / 2) - Depth, Length - Depth, 180, 180),
                            Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Depth, Length),
                                Point.ByCoordinates(0, Length)),
                            EllipseArc.ByPlaneRadiiAngles(ellipseCenter, Width / 2, Length, 180, 180)
                            });
                        }
                    }

                    break;

                case "L":
                    if (Width <= Depth || Length <= Depth)
                    {
                        break;
                    }

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
                    if (Width <= Depth * 2 || Length <= Depth)
                    {
                        break;
                    }

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
                    if (Width <= Depth * 2 || Length <= Depth * 2)
                    {
                        break;
                    }

                    // The D is pointing "down" so that it matches with the U.

                    if (Length > (Width / 2) + Depth)
                    {
                        // Enough room to make the curved part of the D an arc.

                        // Center-point of the curved parts of the D.
                        using (var arcCenter = Point.ByCoordinates(Width / 2, Width / 2))
                        {
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
                                arcCenter,
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
                                    arcCenter,
                                    Point.ByCoordinates(Depth, Width / 2),
                                    Point.ByCoordinates(Width - Depth, Width / 2)
                                )
                            }));
                        }
                    }
                    else
                    {
                        // Short D. Use ellipses and no straight part.
                        using (var ellipseCenter = Plane.ByOriginNormal(
                            Point.ByCoordinates(Width / 2, Length - Depth),
                            Vector.ZAxis()))
                        {
                            boundary = PolyCurve.ByJoinedCurves(new Curve[]
                            {
                            PolyCurve.ByPoints(new[]
                            {
                                Point.ByCoordinates(Width, Length - Depth),
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(0, Length),
                                Point.ByCoordinates(0, Length - Depth)
                            }),
                            EllipseArc.ByPlaneRadiiAngles(ellipseCenter, Width / 2, Length - Depth, 180, 180)
                            });

                            holes.Add(PolyCurve.ByJoinedCurves(new Curve[]
                            {
                                Line.ByStartPointEndPoint(
                                    Point.ByCoordinates(Width - Depth, Length - Depth),
                                    Point.ByCoordinates(Depth, Length - Depth)),
                                EllipseArc.ByPlaneRadiiAngles(ellipseCenter, (Width / 2) - Depth, Length - (2 * Depth), 180, 180)
                            }));
                        }
                    }

                    break;

                case "O":
                    if (Width <= Depth * 2 || Length <= Depth * 2)
                    {
                        break;
                    }

                    using (Point centerPoint = Point.ByCoordinates(Width / 2, Length / 2))
                    {
                        boundary = Ellipse.ByOriginRadii(centerPoint, Width / 2, Length / 2);
                        holes.Add(Ellipse.ByOriginRadii(centerPoint, (Width / 2) - Depth, (Length / 2) - Depth));
                    }

                    break;

                case "A":
                    if (Width <= Depth * 2 || Length <= Depth * 2)
                    {
                        break;
                    }

                    var angle = Math.Asin(Depth / (2 * Math.Sqrt(Math.Pow(Length, 2) + (Math.Pow(Width, 2) / 4)))) + Math.Atan2(2 * Length, Width);
                    var DepthX = Depth / Math.Sin(angle);

                    if (Width - (2 * (DepthX + (Depth / Math.Tan(angle)))) <= 0)
                    {
                        break;
                    }

                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(DepthX, 0),
                        Point.ByCoordinates((Width * 0.999) - DepthX - (Depth / Math.Tan(angle)), Depth),
                        Point.ByCoordinates(DepthX + (Depth / Math.Tan(angle)), Depth),
                        Point.ByCoordinates(Width / 2, Length - (DepthX * Math.Tan(angle) / 2)),
                        Point.ByCoordinates(Width - DepthX, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates((Width + DepthX) / 2, Length),
                        Point.ByCoordinates((Width - DepthX) / 2, Length),
                        Point.ByCoordinates(Depth / Math.Tan(angle), Depth)
                    }, connectLastToFirst: true);

                    break;

                default:
                    break;
            }

            if (boundary != null)
            {
                try
                {
                    baseSurface = Surface.ByPatch(boundary);
                }
                catch (ApplicationException)
                {
                    return default;
                }

                if (holes.Count > 0)
                {
                    // A bug in Dynamo requires the boundary curve to be included in the trim curves, otherwise it trims the wrong part.
                    holes.Add(boundary);

                    // TrimWithEdgeLoops fails occasionally, then succeeds with the same inputs. If at first we don't succeed...
                    for (int attempts = 0; attempts < 5; attempts++)
                    {
                        try
                        {
                            baseSurface = baseSurface.TrimWithEdgeLoops(holes.Select(c => PolyCurve.ByJoinedCurves(new[] { c })));
                            break;
                        }
                        catch (ApplicationException)
                        { }

                        Thread.Sleep(50);
                    }

                    holes.ForEach(h => { if (h != null) { h.Dispose(); } });
                }

                boundary.Dispose();
            }

            return baseSurface;
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

            if (Mass != null)
            {
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
            }

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"VerticalSurfaces", vertical},
                {"HorizontalSurfaces", horizontal}
            };
        }
    }
}
