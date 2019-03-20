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
            UsesDepth = Width <= Depth * 2 || Length <= Depth ? false : true;
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
                if (UsesDepth)
                {
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
                else
                {
                    // H is too chunky - make a box.

                    boundary = Rectangle.ByWidthLength(
                        Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis()),
                        Width,
                        Length);
                }
            }

            return (boundary, default);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            var boundary = new List<Curve>();

            using (Plane centerPlane = Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis()))
            {
                if (UsesDepth)
                {
                    // One core along the center of building.

                    double coreHeight = Depth * (1 - (2 * hallwayToDepth));

                    boundary.Add(Rectangle.ByWidthLength(
                        centerPlane,
                        CoreArea / coreHeight,
                        coreHeight));
                }
                else
                {
                    // Simple box building, core has same aspect ratio as floorplate.
                    boundary.Add(Rectangle.ByWidthLength(
                        centerPlane, 
                        Width * (CoreArea / FloorArea), 
                        Length * (CoreArea / FloorArea)));
                }
            }

            return boundary;
        }
    }
}
