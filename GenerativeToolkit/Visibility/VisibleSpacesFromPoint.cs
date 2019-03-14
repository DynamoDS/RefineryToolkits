using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;

namespace GenerativeToolkit.Visibility
{
    class VisibleSpacesFromPoint
    {
        [MultiReturn(new[] { "visGraph", "factors" })]
        public static Dictionary<string, object> VisibleSpaces(List<Room> rooms, List<Point> points )
        {

        }

    }
}
