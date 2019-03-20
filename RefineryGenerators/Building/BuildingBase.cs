using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal abstract class BuildingBase : IDisposable
    {
        protected double hallwayToDepth = 0.1;
        protected double coreSizeFactorFloors = 10;
        protected double coreSizeFactorArea = 0.1;

        public double Length { get; private set; }
        public double Width { get; private set; }
        public double Depth { get; private set; }
        public Plane BasePlane { get; private set; }
        public double FloorArea { get; private set; }
        public double FloorHeight { get; private set; }
        public bool IsSmooth { get; private set; }
        public Solid Mass { get; private set; }
        public List<Solid> Cores { get; private set; } = new List<Solid>();
        public double TotalVolume { get; private set; }
        public Plane TopPlane { get; private set; }
        public double FacadeArea { get; private set; }
        public List<double> FloorElevations { get; private set; } = new List<double>();
        public List<Surface> Floors { get; private set; } = new List<Surface>();
        public int FloorCount { get; set; }
        public ShapeType Type { get; set; }

        protected double CoreArea => (FloorArea * coreSizeFactorArea) + (FloorCount * coreSizeFactorFloors);
        public double TotalArea => FloorArea * FloorCount;

        public BuildingBase()
        {
        }

        public void CreateBuilding(double length, double width, double depth, Plane basePlane, double targetBuildingArea, double floorHeight, bool createCores = true, bool isSmooth = false)
        {
            Length = length;
            Width = width;
            Depth = depth;
            BasePlane = basePlane;
            FloorHeight = floorHeight;
            IsSmooth = isSmooth;

            Setup();

            Surface baseSurface = MakeBaseSurface();

            if (baseSurface == null) { throw new ArgumentException("Could not create building shape."); }

            // Surface is constructed with lower left corner at (0,0). Move and rotate to given base plane.
            baseSurface = (Surface)baseSurface.Transform(CoordinateSystem.ByOrigin(width / 2, length / 2), BasePlane.ToCoordinateSystem());

            FloorArea = baseSurface.Area;

            FloorCount = (int)Math.Ceiling(targetBuildingArea / FloorArea);

            Mass = baseSurface.Thicken(FloorCount * FloorHeight, both_sides: false);

            TotalVolume = Mass.Volume;

            FacadeArea = Mass.Area - (2 * FloorArea);

            for (int i = 0; i < FloorCount; i++)
            {
                Floors.Add((Surface)baseSurface.Translate(Vector.ByCoordinates(0, 0, i * FloorHeight)));
                FloorElevations.Add(BasePlane.Origin.Z + (i * FloorHeight));
            }

            TopPlane = (Plane)BasePlane.Translate(Vector.ByCoordinates(0, 0, FloorCount * FloorHeight));

            baseSurface.Dispose();

            if (createCores)
            {
                var coreBases = MakeCoreSurface();

                if (coreBases == null || coreBases.Count == 0) { throw new ArgumentException("Could not create core shape."); }

                foreach (var coreBase in coreBases){
                    Cores.Add(coreBase.Thicken(FloorCount * FloorHeight, both_sides: false));

                    coreBase.Dispose();
                }
            }
        }

        protected abstract void Setup();
        protected abstract (Curve boundary, List<Curve> holes) CreateBaseCurves();
        protected abstract List<Curve> CreateCoreCurves();

        public virtual bool IsValid()
        {
            return Length > 0 && Width > 0 && Depth > 0;
        }

        public Surface MakeBaseSurface()
        {
            var (boundary, holes) = CreateBaseCurves();

            if (boundary == null) { return default; }

            Surface baseSurface = null;

            try
            {
                baseSurface = Surface.ByPatch(boundary);
            }
            catch (ApplicationException)
            {
                return default;
            }

            if (holes.Count > 0)
            {
                // A bug in Dynamo requires the boundary curve to be included in the trim curves, otherwise it trims the wrong part.
                holes.Add(boundary);

                // TrimWithEdgeLoops fails occasionally, then succeeds with the same inputs. If at first we don't succeed...
                for (int attempts = 0; attempts < 5; attempts++)
                {
                    try
                    {
                        baseSurface = baseSurface.TrimWithEdgeLoops(holes.Select(c => PolyCurve.ByJoinedCurves(new[] { c })));
                        break;
                    }
                    catch (ApplicationException)
                    { }

                    Thread.Sleep(50);
                }

                holes.ForEach(h => { if (h != null) { h.Dispose(); } });
            }

            boundary.Dispose();

            return baseSurface;
        }

        public List<Surface> MakeCoreSurface()
        {
            List<Curve> boundaries = CreateCoreCurves();

            List<Surface> baseSurfaces = new List<Surface>();

            try
            {
                foreach(var boundary in boundaries)
                {
                    baseSurfaces.Add(Surface.ByPatch(boundary));
                }
            }
            catch (ApplicationException)
            {
                return default;
            }

            boundaries.ForEach(x => x.Dispose());
            
            return baseSurfaces;
        }

        public virtual void Dispose()
        {
            Mass.Dispose();
            Cores.ForEach(x => x.Dispose());
            BasePlane.Dispose();
            TopPlane.Dispose();
            Floors.ForEach(x => x.Dispose());
        }
    }
}
