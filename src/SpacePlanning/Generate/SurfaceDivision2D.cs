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
        public static List<Geometry> DivideSurface(
            Surface surface,
            List<double> U,
            List<double> V)
        {
            List<IDisposable> disposables = [];
            List<Geometry> dividedSurfaces = [];

            List<PolySurface> polySurfaces = [];
            List<List<double>> UV = [U, V];
            Curve uCurve = Curve.ByIsoCurveOnSurface(surface, 1, 0);
            for (int i = 0; i <= 1; i++)
            {
                List<Surface> crvSurf = [];
                foreach (double item in UV[i])
                {
                    Curve crv = Curve.ByIsoCurveOnSurface(surface, i, item);
                    crvSurf.Add(crv.Extrude(Vector.ByCoordinates(0, 0, 1)));
                    crv.Dispose();
                }
                polySurfaces.Add(PolySurface.ByJoinedSurfaces(crvSurf));
                disposables.AddRange(crvSurf);
            }
            var splitSurfaces = surface.Split(polySurfaces[1]);
            var sortedSurfaces = splitSurfaces.OrderBy(uCurve.DistanceTo);
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
