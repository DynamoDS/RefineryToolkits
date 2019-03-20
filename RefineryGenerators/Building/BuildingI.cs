using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingI : BuildingBase
    {
        private Plane center;

        public BuildingI()
        {
            Type = ShapeType.I;
        }

        protected override void Setup()
        {
            center = Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis());
        }

        public override void Dispose()
        {
            base.Dispose();

            center.Dispose();
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            var boundary = Rectangle.ByWidthLength(center, Width, Length);

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            // Core has same aspect ratio as floorplate.
            return new List<Curve>
            {
                Rectangle.ByWidthLength(center, Width * (CoreArea / FloorArea), Length * (CoreArea / FloorArea))
            };
        }
    }
}
