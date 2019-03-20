using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingD : BuildingBase
    {
        public BuildingD()
        {
            Type = ShapeType.D;
        }

        protected override void Setup()
        {
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            // The D is pointing "down" so that it matches with the U.

            Curve boundary = null;
            var holes = new List<Curve>();

            if (IsSmooth)
            {
                return default;
            }
            else
            {
                if (Width <= Depth * 2 || Length <= Depth * 2)
                {
                    return default;
                }

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
            }

            return (boundary, holes);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            var boundary = new List<Curve>();

            return boundary;
        }
    }
}
