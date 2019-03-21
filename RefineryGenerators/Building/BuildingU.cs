using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingU : BuildingBase
    {
        public BuildingU()
        {
            Type = ShapeType.U;
        }

        protected override void Setup()
        {
            UsesDepth = Width > 2 * Depth && Length > Depth ? true : false;
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;

            if (IsCurved)
            {
                var holes = new List<Curve>();
                var boundaryCurves = new List<Curve>();

                var arcHeight = Math.Min(Length, Width / 2);

                using (Plane arcCenter = Plane.ByOriginNormal(
                    Point.ByCoordinates(Width / 2, arcHeight),
                    Vector.ZAxis()))
                {
                    boundaryCurves.Add(EllipseArc.ByPlaneRadiiAngles(arcCenter, Width / 2, arcHeight, 180, 180));

                    if (UsesDepth)
                    {
                        if (arcHeight < Length)
                        {
                            // Top of U has straight parts.

                            boundaryCurves.Add(PolyCurve.ByPoints(new[]
                                {
                                    Point.ByCoordinates(Width, arcHeight),
                                    Point.ByCoordinates(Width, Length),
                                    Point.ByCoordinates(Width - Depth, Length),
                                    Point.ByCoordinates(Width - Depth, arcHeight)
                                }));
                        }
                        else
                        {
                            // Top of U has no straight parts.
                            boundaryCurves.Add(Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(Width - Depth, Length)));
                        }

                        boundaryCurves.Add(EllipseArc.ByPlaneRadiiAngles(arcCenter, (Width / 2) - Depth, arcHeight - Depth, 0, -180));

                        if (arcHeight < Length)
                        {
                            // Top of U has straight parts.
                            boundaryCurves.Add(PolyCurve.ByPoints(new[]
                                {
                            Point.ByCoordinates(Depth, arcHeight),
                            Point.ByCoordinates(Depth, Length),
                            Point.ByCoordinates(0, Length),
                            Point.ByCoordinates(0, arcHeight)
                        }));
                        }
                        else
                        {
                            // Top of U has no straight parts.
                            boundaryCurves.Add(Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Depth, Length),
                                Point.ByCoordinates(0, Length)));
                        }
                    }
                    else
                    {
                        // U has no interior.

                        if (arcHeight < Length)
                        {
                            boundaryCurves.Add(PolyCurve.ByPoints(new[]
                            {
                                Point.ByCoordinates(Width, arcHeight),
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(0, Length),
                                Point.ByCoordinates(0, arcHeight)
                            }));
                        }
                        else
                        {
                            boundaryCurves.Add(Line.ByStartPointEndPoint(
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(0, Length)));
                        }
                    }
                }

                boundary = PolyCurve.ByJoinedCurves(boundaryCurves);
            }
            else
            {
                // Straight U

                if (UsesDepth)
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
                else
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
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                var coreHeight = Depth * (1 - (2 * hallwayToDepth));

                return new List<Curve>
                {
                    Rectangle.ByWidthLength(
                        Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Depth / 2), Vector.ZAxis()),
                        CoreArea / coreHeight,
                        coreHeight)
                };
            }
            else
            {
                // Simple box building, core has same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
