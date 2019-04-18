using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace GenerativeToolkit
{
    internal class BuildingFromCurves : BuildingBase
    {
        private readonly Curve boundary;
        private readonly List<Curve> holes;

        public BuildingFromCurves(Curve boundary, List<Curve> holes, double floorHeight, double? targetBuildingArea = null, int? floorCount = null)
        {
            BoundingBox bounds = boundary.BoundingBox;

            Vector diagonal = bounds.MaxPoint.AsVector().Subtract(bounds.MinPoint.AsVector());

            Point center = bounds.MinPoint.Add(diagonal.Scale(0.5));

            Plane basePlane = Plane.ByOriginNormal(center, Vector.ZAxis());

            Vector locationReset = bounds.MinPoint.AsVector().Scale(-1);

            this.boundary = (Curve)boundary.Translate(locationReset);
            this.holes = holes.Select(h => h.Translate(locationReset)).Cast<Curve>().ToList();

            CreateBuilding(basePlane, floorHeight, targetBuildingArea, floorCount, diagonal.X, diagonal.Y);

            diagonal.Dispose();
            center.Dispose();
            locationReset.Dispose();
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            return (boundary, holes);
        }

        protected override void Setup()
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            boundary.Dispose();
            holes.ForEach(x => x.Dispose());
        }
    }
}
