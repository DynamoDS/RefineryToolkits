using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DSVector = Autodesk.DesignScript.Geometry.Vector;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using DSLine = Autodesk.DesignScript.Geometry.Line;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using GenerativeToolkit.Graphs.Geometry;
using GenerativeToolkit.Graphs.Extensions;

namespace Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry
{

    /// <summary>
    /// Static class extending Point functionality
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class Points
    {

        #region Internal Methods
        [IsVisibleInDynamoLibrary(false)]
        internal static GeometryVertex ToVertex(this DSPoint point)
        {
            return GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);
        }

        internal static DSPoint ToPoint(this GeometryVertex vertex)
        {
            return DSPoint.ByCoordinates(vertex.X, vertex.Y, vertex.Z);
        }

        #endregion

    }
}
