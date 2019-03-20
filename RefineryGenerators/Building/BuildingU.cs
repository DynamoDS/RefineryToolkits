using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingU : BuildingBase
    {
        private double facetLength;

        public BuildingU()
        {
            Type = ShapeType.U;
        }

        protected override void Setup()
        {
            if (!IsSmooth)
            {
                facetLength = Width / (1 + Math.Sqrt(2));
            }
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;

            if (IsSmooth)
            {
                if (Length > Width / 2)
                {
                    // Enough room to make the curved part of the U an arc.

                    // Center-point of the curved parts of the U.
                    using (var arcCenter = Point.ByCoordinates(Width / 2, Width / 2))
                    {
                        var boundaryCurves = new List<Curve>()
                        {
                            Line.ByStartPointEndPoint(
                                Point.ByCoordinates(0, Length),
                                Point.ByCoordinates(0, Width / 2)),
                            Arc.ByCenterPointStartPointEndPoint(
                                arcCenter,
                                Point.ByCoordinates(0, Width / 2),
                                Point.ByCoordinates(Width, Width / 2)),
                            Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Width, Width / 2),
                                Point.ByCoordinates(Width, Length))
                        };

                        if (Width > 2 * Depth && Length > Depth)
                        {
                            // Big enough to have an interior part of the U.

                            boundaryCurves.AddRange(new Curve[]
                            {
                                PolyCurve.ByPoints(new[]
                                {
                                    Point.ByCoordinates(Width, Length),
                                    Point.ByCoordinates(Width - Depth, Length),
                                    Point.ByCoordinates(Width - Depth, Width / 2)
                                }),
                                Arc.ByCenterPointStartPointSweepAngle(
                                    arcCenter,
                                    Point.ByCoordinates(Width - Depth, Width / 2),
                                    -180,
                                    Vector.ZAxis()),
                                PolyCurve.ByPoints(new[]
                                {
                                    Point.ByCoordinates(Depth, Width / 2),
                                    Point.ByCoordinates(Depth, Length)
                                })
                            });
                        }

                        boundary = PolyCurve.ByJoinedCurves(boundaryCurves);
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
            }
            else
            {
                // Straight U

                if (Width <= 2 * Depth || Length <= Depth)
                {
                    // Solid straight U (rectangle)
                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length)
                    }, connectLastToFirst: true);
                }
                else
                {
                    boundary = PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(Width - Depth, Length),
                        Point.ByCoordinates(Width - Depth, Depth),
                        Point.ByCoordinates(Depth, Depth),
                        Point.ByCoordinates(Depth, Length),
                        Point.ByCoordinates(0, Length)
                    }, connectLastToFirst: true);
                }
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            var boundary = new List<Curve>();

            var coreWidth = Depth * (1 - (2 * hallwayToDepth));

            using (Plane center = (Plane)Plane.XY().Translate(Width / 2, Depth / 2, 0))
            {
                boundary.Add(Rectangle.ByWidthLength(center, coreWidth, CoreArea / coreWidth));
            }

            Rectangle asdf = null;

            return boundary;
        }
    }
}
