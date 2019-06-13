using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal class BuildingH : BuildingBase
    {
        public BuildingH()
        {
            Type = ShapeType.H;
        }

        protected override void Setup()
        {
            UsesDepth = Width <= Depth * 2 || Length <= Depth ? false : true;
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            Curve boundary = null;
            
            if (UsesDepth)
            {
                var points = new[]
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
                };

                boundary = PolyCurve.ByPoints(points, connectLastToFirst: true);

                points.ForEach(p => p.Dispose());
            }
            else
            {
                // H is too chunky - make a box.
                boundary = Rectangle.ByWidthLength(BaseCenter, Width, Length);
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                // One core along the center of building.

                double coreHeight = Depth * (1 - (2 * hallwayToDepth));

                return new List<Curve>
                {
                    Rectangle.ByWidthLength(BaseCenter, CoreArea / coreHeight, coreHeight)
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
