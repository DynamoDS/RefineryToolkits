using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingO : BuildingBase
    {
        private double facetLength;

        public BuildingO()
        {
            Type = ShapeType.O;
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
            var holes = new List<Curve>();

            if (IsSmooth)
            {
                if (Width <= Depth * 2 || Length <= Depth * 2)
                {
                    return default;
                }

                using (Point centerPoint = Point.ByCoordinates(Width / 2, Length / 2))
                {
                    boundary = Ellipse.ByOriginRadii(centerPoint, Width / 2, Length / 2);
                    holes.Add(Ellipse.ByOriginRadii(centerPoint, (Width / 2) - Depth, (Length / 2) - Depth));
                }
            }
            else
            {
                // Faceted O (box with courtyard)

                using (Plane centerPlane = Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis()))
                {
                    boundary = Rectangle.ByWidthLength(centerPlane, Width, Length);
                    holes.Add(Rectangle.ByWidthLength(centerPlane, Width - (2 * Depth), Length - (2 * Depth));
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
