using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingL : BuildingBase
    {
        public BuildingL()
        {
            Type = ShapeType.L;
        }

        protected override void Setup()
        {
            UsesDepth = Width <= Depth || Length <= Depth ? false : true;
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;

            if (UsesDepth)
            {
                boundary = PolyCurve.ByPoints(new[]
                {
                    Point.ByCoordinates(0, 0),
                    Point.ByCoordinates(Width, 0),
                    Point.ByCoordinates(Width, Depth),
                    Point.ByCoordinates(Depth, Depth),
                    Point.ByCoordinates(Depth, Length),
                    Point.ByCoordinates(0, Length)
                }, connectLastToFirst: true);
            }
            else
            {
                // L is too chunky - make a box.

                boundary = Rectangle.ByWidthLength(
                    Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis()),
                    Width,
                    Length);
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                // One core along the bottom leg of building.

                double coreHeight = Depth * (1 - (2 * hallwayToDepth));

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
                // Simple box building, core is in the center with the same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
