using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;

namespace Autodesk.GenerativeToolkit.Analyse
{
    [IsVisibleInDynamoLibrary(false)]
    public class VisibleSpacesFromPoint
    {
        [IsVisibleInDynamoLibrary(true)]
        public static List<List<Room>> VisibleRoomsFromDesk(List<Room> rooms, List<Point> points )
        {
            List<Polygon> roomPolygons = HelperFunctions.DeskFunctions.RoomPolygons(rooms);
            List<Polygon> spacePolygons = HelperFunctions.DeskFunctions.PolygonsFromSpaces(roomPolygons);
            
            GraphicalDynamo.Graphs.BaseGraph baseGraph = GraphicalDynamo.Graphs.BaseGraph.ByPolygons(spacePolygons);

            List<List<Room>> visibleRooms = new List<List<Room>>();
            foreach (Point point in points)
            {
                Surface isovist = GraphicalDynamo.Graphs.BaseGraph.IsovistFromPoint(baseGraph, point);
                visibleRooms.Add(new List<Room> (VisibleSpaces(isovist, roomPolygons, rooms)));
                isovist.Dispose();
                point.Dispose();
            }

            spacePolygons.ForEach(poly => poly.Dispose());
            roomPolygons.ForEach(roomPoly => roomPoly.Dispose());

            return visibleRooms;

        }

        private static List<Room> VisibleSpaces(Surface isovist, List<Polygon> roomPolygons, List<Room> rooms)
        {
            List<Room> visibleRooms = new List<Room>();

            for (int i = 0; i < rooms.Count-1; i++)
            {
                if (isovist.DoesIntersect(roomPolygons[i]))
                {
                    visibleRooms.Add(rooms[i]);
                }
            }

            return visibleRooms;
        } 
    }
}
