using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingH : BuildingBase
    {
        public BuildingH()
        {
            Type = ShapeType.H;
        }

        protected override void Setup()
        {
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;

            if (IsSmooth)
            {
                return default;
            }
            else
            {
                if (Width <= Depth * 2 || Length <= Depth)
                {
                    return default;
                }

                boundary = PolyCurve.ByPoints(new[]
                {
                    Point.ByCoordinates(0, 0),
                    Point.ByCoordinates(Depth, 0),
                    Point.ByCoordinates(Depth, (Length - Depth) / 2),
                    Point.ByCoordinates(Width - Depth, (Length - Depth) / 2),
                    Point.ByCoordinates(Width - Depth, 0),
                    Point.ByCoordinates(Width, 0),
                    Point.ByCoordinates(Width, Length),
                    Point.ByCoordinates(Width - Depth, Length),
                    Point.ByCoordinates(Width - Depth, (Length + Depth) / 2),
                    Point.ByCoordinates(Depth, (Length + Depth) / 2),
                    Point.ByCoordinates(Depth, Length),
                    Point.ByCoordinates(0, Length)
                }, connectLastToFirst: true);
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            var boundary = new List<Curve>();

            return boundary;
        }
    }
}
