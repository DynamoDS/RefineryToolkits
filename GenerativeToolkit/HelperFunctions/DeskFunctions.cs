using DSGeo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace GenerativeToolkit.HelperFunctions
{
    class DeskFunctions
    {
        [IsVisibleInDynamoLibrary(false)]
        public static List<DSGeo.Polygon> PolygonsFromRooms(List<Room> rooms)
        {
            rooms = FilterRooms(rooms);
            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> segments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                List<DSGeo.Polygon> boundaryPolygon = BoundaryPolygon(segments);

            }
        }



        private static List<Room> FilterRooms(List<Room> rooms)
        {
            List<Room> filteredRooms = new List<Room>();
            foreach (Room room in rooms)
            {
                double roomArea = room.Area;
                if (roomArea > 0)
                {
                    filteredRooms.Add(room);
                }
            }
            return filteredRooms;
        }

        private static List<DSGeo.Polygon> BoundaryPolygon(IList<IList<BoundarySegment>> boundarySegments)
        {
            List<DSGeo.Polygon> polygons = new List<DSGeo.Polygon>();  
            foreach (IList<BoundarySegment> segmentList in boundarySegments)
            {          
                List<DSGeo.Point> boundaryPoints = new List<DSGeo.Point>();  
                foreach (BoundarySegment segment in segmentList)
                {
                    double x = segment.GetCurve().GetEndPoint(0).X;
                    double y = segment.GetCurve().GetEndPoint(0).Y;
                    double z = segment.GetCurve().GetEndPoint(0).Z;
                    boundaryPoints.Add(DSGeo.Point.ByCoordinates(x, y, z));
                }
                polygons.Add(DSGeo.Polygon.ByPoints(boundaryPoints));
                boundaryPoints.ForEach(x => x.Dispose());
            }
            return polygons;
        }

        private static List<DSGeo.Polygon> CombineIntersectingRooms(List<DSGeo.Polygon> polygons)
        {
            List<DSGeo.Solid> solidSurfs = new List<DSGeo.Solid>();
            foreach (DSGeo.Polygon polygon in polygons)
            {
                DSGeo.Solid solid = DSGeo.Surface.ByPatch(polygon).Thicken(1);
                solidSurfs.Add(solid);
            }
            DSGeo.Solid solidUnion = DSGeo.Solid.ByUnion(solidSurfs);

        }
    }
}
