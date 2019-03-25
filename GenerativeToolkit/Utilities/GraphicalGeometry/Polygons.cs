using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using GenerativeToolkit.Graphs.Geometry;
using DSPoint = Autodesk.DesignScript.Geometry.Point;

namespace Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry
{
    public static class Polygons
    {
        #region Public Methods

        /// <summary>
        /// Method to check if a polygon contains a point.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool ContainsPoint(Polygon polygon, DSPoint point)
        {
            gPolygon gPol = gPolygon.ByVertices(polygon.Points.Select(p => gVertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());
            gVertex vertex = gVertex.ByCoordinates(point.X, point.Y, point.Z);

            return gPol.ContainsVertex(vertex);
        }

        #endregion
    }
}
