using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal class BuildingD : BuildingBase
    {
        private double facetLength;

        public BuildingD()
        {
            Type = ShapeType.D;
        }

        protected override void Setup()
        {
            if (!IsCurved)
            {
                facetLength = Width / (1 + Math.Sqrt(2));
            }

            UsesDepth = Width <= Depth * 2 || Length <= Depth * 2 ? false : true;
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            // The D is pointing "down" so that it matches with the U.

            var holes = new List<Curve>();
            var boundaryCurves = new List<Curve>();

            var arcHeight = Math.Min(
                Length - (UsesDepth ? Depth : 0),
                Width / 2);

            Point[] points;

            if (IsCurved)
            {
                Plane arcCenter = null;

                using (var point = Point.ByCoordinates(Width / 2, arcHeight))
                using (var zAxis = Vector.ZAxis())
                {
                    arcCenter = Plane.ByOriginNormal(point, zAxis);
                }

                boundaryCurves.Add(EllipseArc.ByPlaneRadiiAngles(arcCenter, Width / 2, arcHeight, 180, 180));

                if (arcHeight < Length)
                {
                    points = new[]
                    {
                        Point.ByCoordinates(Width, arcHeight),
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length),
                        Point.ByCoordinates(0, arcHeight)
                    };

                    // Outside of D has a square back.
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

                    // Outside of D is half an ellipse (or circle).
                    boundaryCurves.Add(Line.ByStartPointEndPoint(points[0], points[1]));

                    points.ForEach(p => p.Dispose());
                }

                if (UsesDepth)
                {
                    var curves = new List<Curve>
                    {
                        EllipseArc.ByPlaneRadiiAngles(arcCenter, (Width / 2) - Depth, arcHeight - Depth, 0, -180)
                    };

                    if (arcHeight < Length - Depth)
                    {
                        points = new[]
                        {
                            Point.ByCoordinates(Depth, arcHeight),
                            Point.ByCoordinates(Depth, Length - Depth),
                            Point.ByCoordinates(Width - Depth, Length - Depth),
                            Point.ByCoordinates(Width - Depth, arcHeight)
                        };

                        curves.Add(PolyCurve.ByPoints(points));

                        points.ForEach(p => p.Dispose());
                    }
                    else
                    {
                        points = new[]
                        {
                            Point.ByCoordinates(Depth, arcHeight),
                            Point.ByCoordinates(Width - Depth, arcHeight)
                        };

                        curves.Add(Line.ByStartPointEndPoint(points[0], points[1]));

                        points.ForEach(p => p.Dispose());
                    }

                    holes.Add(PolyCurve.ByJoinedCurves(curves));

                    curves.ForEach(x => x.Dispose());
                }

                arcCenter.Dispose();
            }
            else
            {
                // Faceted D.

                double baseWidth = Width * Math.Tan(Math.PI / 8);
                double sideWidth = baseWidth * arcHeight / (2 * Width);

                points = new[]
                {
                    Point.ByCoordinates(Width, Length),
                    Point.ByCoordinates(0, Length),
                    Point.ByCoordinates(0, arcHeight - sideWidth),
                    Point.ByCoordinates((Width - baseWidth) / 2, 0),
                    Point.ByCoordinates((Width + baseWidth) / 2, 0),
                    Point.ByCoordinates(Width, arcHeight - sideWidth)
                };

                boundaryCurves.Add(PolyCurve.ByPoints(points, connectLastToFirst: true));

                points.ForEach(p => p.Dispose());
                
                if (UsesDepth)
                {
                    double angleA = Math.Atan2(2 * (arcHeight - sideWidth), Width - baseWidth);
                    double offsetBaseWidth = baseWidth - (2 * Depth / Math.Tan((Math.PI - angleA) / 2));
                    double offsetSideWidth = sideWidth - (Depth / Math.Tan((angleA / 2) + (Math.PI / 4)));

                    points = new[]
                    {
                        Point.ByCoordinates(Width - Depth, Length - Depth),
                        Point.ByCoordinates(Width - Depth, arcHeight - offsetSideWidth),
                        Point.ByCoordinates((Width + offsetBaseWidth) / 2, Depth),
                        Point.ByCoordinates((Width - offsetBaseWidth) / 2, Depth),
                        Point.ByCoordinates(Depth, arcHeight - offsetSideWidth),
                        Point.ByCoordinates(Depth, Length - Depth)

                    };

                    holes.Add(PolyCurve.ByPoints(points, connectLastToFirst: true));

                    points.ForEach(p => p.Dispose());
                }

            }

            var boundary = PolyCurve.ByJoinedCurves(boundaryCurves);

            return (boundary, holes);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                // One core along the top leg of building.

                double coreHeight = Depth * (1 - (2 * hallwayToDepth));

                using (var point = Point.ByCoordinates(Width / 2, Length - (Depth / 2)))
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
                // Simple box building, core is in the center with the same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
