using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal class BuildingO : BuildingBase
    {
        public BuildingO()
        {
            Type = ShapeType.O;
        }

        public override void Dispose()
        {
            base.Dispose();

            BaseCenter.Dispose();
        }

        protected override void Setup() { }

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
                
                boundary = Rectangle.ByWidthLength(BaseCenter, Width, Length);

                if (UsesDepth)
                {
                    holes.Add(Rectangle.ByWidthLength(BaseCenter, Width - (2 * Depth), Length - (2 * Depth)));
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
