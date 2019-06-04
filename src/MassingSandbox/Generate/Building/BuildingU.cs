using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
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

            Point[] points;

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
                            points = new[]
                            {
                                Point.ByCoordinates(Width, arcHeight),
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(Width - Depth, Length),
                                Point.ByCoordinates(Width - Depth, arcHeight)
                            };

                            boundaryCurves.Add(PolyCurve.ByPoints(points));

                            points.ForEach(p => p.Dispose());
                        }
                        else
                        {
                            points = new[]
                            {
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(Width - Depth, Length)
                            };

                            // Top of U has no straight parts.
                            boundaryCurves.Add(Line.ByStartPointEndPoint(points[0], points[1]));

                            points.ForEach(p => p.Dispose());
                        }

                        boundaryCurves.Add(EllipseArc.ByPlaneRadiiAngles(arcCenter, (Width / 2) - Depth, arcHeight - Depth, 0, -180));

                        if (arcHeight < Length)
                        {
                            // Top of U has straight parts.

                            points = new[]
                            {
                                Point.ByCoordinates(Depth, arcHeight),
                                Point.ByCoordinates(Depth, Length),
                                Point.ByCoordinates(0, Length),
                                Point.ByCoordinates(0, arcHeight)
                            };
                            
                            boundaryCurves.Add(PolyCurve.ByPoints(points));

                            points.ForEach(p => p.Dispose());
                        }
                        else
                        {
                            // Top of U has no straight parts.

                            points = new[]
                            {
                                Point.ByCoordinates(Depth, Length),
                                Point.ByCoordinates(0, Length)
                            };

                            boundaryCurves.Add(Line.ByStartPointEndPoint(points[0], points[1]));

                            points.ForEach(p => p.Dispose());
                        }
                    }
                    else
                    {
                        // U has no interior.

                        if (arcHeight < Length)
                        {
                            points = new[]
                                {
                                Point.ByCoordinates(Width, arcHeight),
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(0, Length),
                                Point.ByCoordinates(0, arcHeight)
                            };

                            boundaryCurves.Add(PolyCurve.ByPoints(points));

                            points.ForEach(p => p.Dispose());
                        }
                        else
                        {
                            points = new[]
                            {
                                Point.ByCoordinates(Width, Length),
                                Point.ByCoordinates(0, Length)
                            };

                            boundaryCurves.Add(Line.ByStartPointEndPoint(points[0], points[1]));

                            points.ForEach(p => p.Dispose());
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
                    points = new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(Width - Depth, Length),
                        Point.ByCoordinates(Width - Depth, Depth),
                        Point.ByCoordinates(Depth, Depth),
                        Point.ByCoordinates(Depth, Length),
                        Point.ByCoordinates(0, Length)
                    };

                    boundary = PolyCurve.ByPoints(points, connectLastToFirst: true);

                    points.ForEach(p => p.Dispose());
                }
                else
                {
                    // Solid straight U (rectangle)

                    points = new[]
                    {
                        Point.ByCoordinates(0, 0),
                        Point.ByCoordinates(Width, 0),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length)
                    };

                    boundary = PolyCurve.ByPoints(points, connectLastToFirst: true);

                    points.ForEach(p => p.Dispose());
                }
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                var coreHeight = Depth * (1 - (2 * hallwayToDepth));

                using (var point = Point.ByCoordinates(Width / 2, Depth / 2))
                using (var zAxis = Vector.ZAxis())
                using (var plane = Plane.ByOriginNormal(point, zAxis))
                {
                    return new List<Curve>
                    {
                        Rectangle.ByWidthLength(plane, CoreArea / coreHeight, coreHeight)
                    };
                }
            }
            else
            {
                // Simple box building, core has same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
