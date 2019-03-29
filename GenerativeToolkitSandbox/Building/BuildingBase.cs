using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.DesignScript.Geometry;

namespace GenerativeToolkit
{
    internal abstract class BuildingBase : IDisposable
    {
        protected double hallwayToDepth;
        protected double coreSizeFactorFloors;
        protected double coreSizeFactorArea;

        public double Length { get; protected set; }
        public double Width { get; protected set; }
        public double Depth { get; protected set; }
        public Plane BasePlane { get; protected set; }
        public double FloorArea { get; protected set; }
        public double FloorHeight { get; protected set; }
        public bool IsCurved { get; protected set; }
        public Solid Mass { get; protected set; }
        public List<Solid> Cores { get; protected set; } = new List<Solid>();
        public double TotalVolume { get; protected set; }
        public Plane TopPlane { get; protected set; }
        public double FacadeArea { get; protected set; }
        public List<double> FloorElevations { get; protected set; } = new List<double>();
        public List<Surface[]> Floors { get; protected set; } = new List<Surface[]>();
        public List<Surface[]> NetFloors { get; protected set; }
        public int FloorCount { get; set; }
        public ShapeType Type { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this building has a hole in the center.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a hole; otherwise, <c>false</c>.
        /// </value>
        public bool UsesDepth { get; protected set; }

        protected double CoreArea => (FloorArea * coreSizeFactorArea) + (FloorCount * coreSizeFactorFloors);
        public double GrossFloorArea => FloorArea * FloorCount;
        public double NetFloorArea => (FloorArea - CoreArea) * FloorCount;

        public BuildingBase()
        {
        }

        public void CreateBuilding(
            Plane basePlane, double targetBuildingArea, double floorHeight,
            double width = 0, double length = 0, double depth = 0,
            bool isCurved = false, bool createCores = false,
            double hallwayToDepth = 0, double coreSizeFactorFloors = 0, double coreSizeFactorArea = 0)
        {
            Length = length;
            Width = width;
            Depth = depth;
            BasePlane = basePlane;
            FloorHeight = floorHeight;
            IsCurved = isCurved;
            this.hallwayToDepth = hallwayToDepth;
            this.coreSizeFactorFloors = coreSizeFactorFloors;
            this.coreSizeFactorArea = coreSizeFactorArea;

            Setup();

            Surface baseSurface = MakeBaseSurface();

            if (baseSurface == null) { throw new ArgumentException("Could not create building shape."); }

            // Surface is constructed with lower left corner at (0,0). Move and rotate to given base plane.
            baseSurface = (Surface)baseSurface.Transform(
                CoordinateSystem.ByOrigin(width / 2, length / 2), 
                BasePlane.ToCoordinateSystem());

            FloorArea = baseSurface.Area;

            FloorCount = (int)Math.Ceiling(targetBuildingArea / FloorArea);

            Mass = baseSurface.Thicken(FloorCount * FloorHeight, both_sides: false);

            TotalVolume = Mass.Volume;

            FacadeArea = Mass.Area - (2 * FloorArea);

            for (int i = 0; i < FloorCount; i++)
            {
                Floors.Add(new[] { (Surface)baseSurface.Translate(Vector.ByCoordinates(0, 0, i * FloorHeight)) });
                FloorElevations.Add(BasePlane.Origin.Z + (i * FloorHeight));
            }

            TopPlane = (Plane)BasePlane.Translate(Vector.ByCoordinates(0, 0, FloorCount * FloorHeight));

            baseSurface.Dispose();

            if (createCores)
            {
                var coreBases = MakeCoreSurface();

                if (coreBases == null || coreBases.Count == 0) { return; }

                foreach (var coreBase in coreBases){
                    var core = coreBase.Thicken(FloorCount * FloorHeight, both_sides: false);

                    Cores.Add((Solid)core.Transform(
                        CoordinateSystem.ByOrigin(width / 2, length / 2), 
                        BasePlane.ToCoordinateSystem()));

                    coreBase.Dispose();
                    core.Dispose();
                }

                NetFloors = new List<Surface[]>();

                foreach (var floor in Floors)
                {
                    NetFloors.Add(Cores.Aggregate(floor, 
                        (f, core) => f.SelectMany(
                                surface => surface.SubtractFrom(core))
                            .Cast<Surface>()
                            .ToArray()));
                }
            }
            else
            {
                NetFloors = Floors;
            }
        }

        protected abstract void Setup();
        protected abstract (Curve boundary, List<Curve> holes) CreateBaseCurves();

        /// <summary>
        /// Creates boundary curves at the base of all cores. Defaults to creating a centered rectangle of the same aspect as the building's bounds.
        /// </summary>
        protected virtual List<Curve> CreateCoreCurves()
        {
            // Simple box building, core has same aspect ratio as floorplate.
            return new List<Curve>
            {
                Rectangle.ByWidthLength(
                    Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length / 2), Vector.ZAxis()),
                    Width * Math.Sqrt(CoreArea / FloorArea),
                    Length * Math.Sqrt(CoreArea / FloorArea))
            };
        }

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

            if (holes != null && holes.Count > 0)
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
            Floors.ForEach(x => x.ForEach(y => y.Dispose()));
            NetFloors.ForEach(x => x.ForEach(y => y.Dispose()));
        }
    }
}
