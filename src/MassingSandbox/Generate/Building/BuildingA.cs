using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal class BuildingA : BuildingBase
    {
        public BuildingA()
        {
            UsesDepth = true;
        }

        protected override void Setup() { }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            if (Width <= Depth * 2 || Length <= Depth * 2)
            {
                return default;
            }

            var angle = Math.Asin(Depth / (2 * Math.Sqrt(Math.Pow(Length, 2) + (Math.Pow(Width, 2) / 4)))) + Math.Atan2(2 * Length, Width);
            var DepthX = Depth / Math.Sin(angle);

            if (Width - (2 * (DepthX + (Depth / Math.Tan(angle)))) <= 0)
            {
                return default;
            }

            var points = new[]
            {
                Point.ByCoordinates(DepthX, 0),
                Point.ByCoordinates((Width * 0.999) - DepthX - (Depth / Math.Tan(angle)), Depth),
                Point.ByCoordinates(DepthX + (Depth / Math.Tan(angle)), Depth),
                Point.ByCoordinates(Width / 2, Length - (DepthX* Math.Tan(angle) / 2)),
                Point.ByCoordinates(Width - DepthX, 0),
                Point.ByCoordinates(Width, 0),
                Point.ByCoordinates((Width + DepthX) / 2, Length),
                Point.ByCoordinates((Width - DepthX) / 2, Length),
                Point.ByCoordinates(Depth / Math.Tan(angle), Depth)
            };

            var boundary = PolyCurve.ByPoints(points, connectLastToFirst: true);

            points.ForEach(p => p.Dispose());

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            var boundary = new List<Curve>();

            return boundary;
        }
    }
}
