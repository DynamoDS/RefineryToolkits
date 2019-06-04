using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static class SurfaceDivision2D
    {
        /// <summary>
        /// Divides surface based on U and V parameters
        /// </summary>
        /// <param name="surface">Surface to divide</param>
        /// <param name="U">U parameter</param>
        /// <param name="V">V parameter</param>
        /// <returns>List of individual surfaces</returns>
        [NodeCategory("Create")]
        public static List<Autodesk.DesignScript.Geometry.Geometry> DivideSurface(
            Surface surface,
            List<double> U,
            List<double> V)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            List<Autodesk.DesignScript.Geometry.Geometry> dividedSurfaces = new List<Autodesk.DesignScript.Geometry.Geometry>();

            List<PolySurface> polySurfaces = new List<PolySurface>();
            List<List<double>> UV = new List<List<double>> { U, V };
            Curve uCurve = Curve.ByIsoCurveOnSurface(surface, 1, 0);
            for (int i = 0; i <= 1; i++)
            {
                List<Surface> crvSurf = new List<Surface>();
                foreach (double item in UV[i])
                {
                    Curve crv = Curve.ByIsoCurveOnSurface(surface, i, item);
                    crvSurf.Add(crv.Extrude(Vector.ByCoordinates(0, 0, 1)));
                    crv.Dispose();
                }
                polySurfaces.Add(PolySurface.ByJoinedSurfaces(crvSurf));
                disposables.AddRange(crvSurf);
            }
            List<Autodesk.DesignScript.Geometry.Geometry> splitSurfaces = surface.Split(polySurfaces[1]).ToList();
            List<Autodesk.DesignScript.Geometry.Geometry> sortedSurfaces = splitSurfaces.OrderBy(x => uCurve.DistanceTo(x)).ToList();
            disposables.AddRange(splitSurfaces);

            foreach (var surf in sortedSurfaces)
            {
                dividedSurfaces.AddRange(surf.Split(polySurfaces[0]));
            }
            disposables.AddRange(sortedSurfaces);
            disposables.AddRange(polySurfaces);

            disposables.ForEach(x => x.Dispose());
            return dividedSurfaces;
        }
    }
}
