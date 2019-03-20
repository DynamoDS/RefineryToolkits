using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingO : BuildingBase
    {
        private Plane centerPlane;

        public BuildingO()
        {
            Type = ShapeType.O;
        }

        protected override void Setup()
        {
            centerPlane = Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis());
        }

        public override void Dispose()
        {
            base.Dispose();

            centerPlane.Dispose();
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;
            var holes = new List<Curve>();

            UsesDepth = Width <= Depth * 2 || Length <= Depth * 2 ? false : true;

            if (IsCurved)
            {
                using (Point centerPoint = Point.ByCoordinates(Width / 2, Length / 2))
                {
                    boundary = Ellipse.ByOriginRadii(centerPoint, Width / 2, Length / 2);

                    if (UsesDepth)
                    {
                        holes.Add(Ellipse.ByOriginRadii(centerPoint, (Width / 2) - Depth, (Length / 2) - Depth));
                    }
                }
            }
            else
            {
                // Faceted O (box with courtyard)
                
                boundary = Rectangle.ByWidthLength(centerPlane, Width, Length);

                if (UsesDepth)
                {
                    holes.Add(boundary.Offset(Depth));
                }
            }

            return (boundary, holes);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                // One core along the bottom edge of building.

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
                // Simple box building, core has same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
