using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    internal class BuildingFromCurves : BuildingBase
    {
        private readonly Curve boundary;
        private readonly List<Curve> holes;

        public BuildingFromCurves(Curve boundary, List<Curve> holes, double floorHeight, double? targetBuildingArea = null, int? floorCount = null)
        {
            BoundingBox bounds = boundary.BoundingBox;

            using (var v1 = bounds.MaxPoint.AsVector())
            using (var v2 = bounds.MinPoint.AsVector())
            using (var diagonal = v1.Subtract(v2))
            using (var scaledDiagonal = diagonal.Scale(0.5))
            using (Point center = bounds.MinPoint.Add(scaledDiagonal))
            using (var zAxis = Vector.ZAxis())
            using (Plane basePlane = Plane.ByOriginNormal(center, zAxis))
            using (var v3 = bounds.MinPoint.AsVector())
            using (Vector locationReset = v3.Scale(-1))
            {
                this.boundary = (Curve)boundary.Translate(locationReset);
                this.holes = holes.Select(h => h.Translate(locationReset)).Cast<Curve>().ToList();

                CreateBuilding(basePlane, floorHeight, targetBuildingArea, floorCount, diagonal.X, diagonal.Y);
            }
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            return (boundary, holes);
        }

        protected override void Setup()
        {
        }

        public override void DisposeNonExports()
        {
            base.DisposeNonExports();

            boundary.Dispose();
            holes.ForEach(x => x.Dispose());
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
