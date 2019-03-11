using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DSCurve = Autodesk.DesignScript.Geometry.Curve;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Graphical.Geometry;
using Graphical.Extensions;
using GenerativeToolkit.Visibility;

namespace GenerativeToolkit.Geometry
{
    
    /// <summary>
    /// Static class extending Point functionality
    /// </summary>
    public static class Curves
    {
        #region Internal Methods

        internal static gEdge ToEdge(this Line line)
        {
            return gEdge.ByStartVertexEndVertex(line.StartPoint.ToVertex(), line.EndPoint.ToVertex());
        }

        internal static Dictionary<gVertex,List<DSCurve>> CurvesDependency(List<DSCurve> curves)
        {
            Dictionary<gVertex, List<DSCurve>> graph = new Dictionary<gVertex, List<DSCurve>>();

            foreach (DSCurve curve in curves)
            {
                gVertex start = Points.ToVertex(curve.StartPoint);
                gVertex end = Points.ToVertex(curve.EndPoint);
                List<DSCurve> startList = new List<DSCurve>();
                List<DSCurve> endList = new List<DSCurve>();
                if (graph.TryGetValue(start, out startList))
                {
                    startList.Add(curve);
                }
                else
                {
                    graph.Add(start, new List<DSCurve>() { curve });
                }

                if (graph.TryGetValue(end, out endList))
                {
                    endList.Add(curve);
                }
                else
                {
                    graph.Add(end, new List<DSCurve>() { curve });
                }
            }
            return graph;
        }

        internal static bool PolygonContainsPoint(Polygon polygon, DSPoint point)
        {
            gVertex vertex = Points.ToVertex(point);
            var vertices = polygon.Points.Select(p => Points.ToVertex(p)).ToList();
            gPolygon gPolygon = gPolygon.ByVertices(vertices, false);

            return gPolygon.ContainsVertex(vertex);
        }

        internal static bool DoesIntersect(Line line1, Line line2)
        {
            gEdge edge1 = gEdge.ByStartVertexEndVertex(Points.ToVertex(line1.StartPoint), Points.ToVertex(line1.EndPoint));
            gEdge edge2 = gEdge.ByStartVertexEndVertex(Points.ToVertex(line2.StartPoint), Points.ToVertex(line2.EndPoint));

            if (edge1.Intersects(edge2))
            {
                if (edge2.StartVertex.OnEdge(edge1)) { return false; }
                if (edge2.EndVertex.OnEdge(edge1)) { return false; }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool DoesIntersect(Line line, DSPoint point)
        {
            gEdge edge = gEdge.ByStartVertexEndVertex(Points.ToVertex(line.StartPoint), Points.ToVertex(line.EndPoint));
            gVertex vertex = Points.ToVertex(point);

            return vertex.OnEdge(edge);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates polygons from a list of lines. Lines are returned as ungrouped if not connected or
        /// not forming a closed polygon.
        /// </summary>
        /// <param name="lines">List of connected lines</param>
        /// <returns name="polygons">Polygons created from connected lines</returns>
        /// <returns name="ungrouped">Lines not forming a closed polygon</returns>
        [MultiReturn(new[] { "polygons", "ungrouped" })]
        public static Dictionary<string, object> BuildPolygons(List<Line> lines)
        {
            if (lines == null) { throw new ArgumentNullException("lines"); }
            if (lines.Count < 2) { throw new ArgumentException("Needs 2 or more lines", "lines"); }

            BaseGraph g = BaseGraph.ByLines(lines);
            g.graph.BuildPolygons();

            var gPolygons = g.graph.Polygons;
            List<Polygon> dsPolygons = new List<Polygon>();
            List<Line> dsLines = new List<Line>();
            List<gEdge> polygonEdges = new List<gEdge>();
            foreach (gPolygon gP in gPolygons)
            {
                var points = gP.Vertices.Select(v => DSPoint.ByCoordinates(v.X, v.Y, v.Z)).ToList();
                if (gP.IsClosed)
                {
                    dsPolygons.Add(Polygon.ByPoints(points));
                }
                else if (gP.Edges.Count > 1)
                {
                    foreach(gEdge edge in gP.Edges)
                    {
                        DSPoint start = Points.ToPoint(edge.StartVertex);
                        DSPoint end = Points.ToPoint(edge.EndVertex);
                        dsLines.Add(Line.ByStartPointEndPoint(start, end));
                    }
                }
                else
                {
                    DSPoint start = Points.ToPoint(gP.Edges.First().StartVertex);
                    DSPoint end = Points.ToPoint(gP.Edges.First().EndVertex);
                    dsLines.Add(Line.ByStartPointEndPoint(start, end));
                }
            }

            return new Dictionary<string, object>()
            {
                {"polygons", dsPolygons },
                {"ungrouped",  dsLines}
            };
        }

        /// <summary>
        /// Groups connected curves into polycurves. Curves are returned as ungrouped if not connected to any other curve.
        /// </summary>
        /// <param name="curves">Curves to group</param>
        /// <returns name="polycurves">Polycurves grouped</returns>
        /// <returns name="ungrouped">Lines not grouped</returns>
        [MultiReturn(new[] { "polycurves", "ungrouped" })]
        public static Dictionary<string, object> GroupCurves(List<DSCurve> curves)
        {
            if(curves == null) { throw new ArgumentNullException("lines"); }
            if(curves.Count < 2) { throw new ArgumentException("Needs 2 or more lines", "lines"); }

            Dictionary<gVertex, List<DSCurve>> graph = CurvesDependency(curves);
            Dictionary<int, List<DSCurve>> grouped = new Dictionary<int, List<DSCurve>>();
            Dictionary<gVertex, int> vertices = new Dictionary<gVertex, int>();

            foreach(gVertex v in graph.Keys)
            {

                // If already belongs to a polygon or is not a polygon vertex or already computed
                if (vertices.ContainsKey(v)|| graph[v].Count > 2) { continue; }

                // grouped.Count() translates to the number of different groups created
                vertices.Add(v, grouped.Count());
                grouped.Add(vertices[v], new List<DSCurve>());

                foreach(DSCurve curve in graph[v])
                {
                    var startVertex = Points.ToVertex(curve.StartPoint);
                    var endVertex = Points.ToVertex(curve.EndPoint);
                    DSCurve nextCurve = curve;
                    gVertex nextVertex = (startVertex.Equals(v)) ? endVertex : startVertex;
                
                    while(!vertices.ContainsKey(nextVertex))
                    {
                        vertices.Add(nextVertex, vertices[v]);
                        grouped[vertices[v]].Add(nextCurve);

                        // Next vertex doesn't have any other curve connected.
                        if(graph[nextVertex].Count < 2) { break; }

                        nextCurve = graph[nextVertex].Where(c => !c.Equals(nextCurve)).First();
                        startVertex = Points.ToVertex(nextCurve.StartPoint);
                        endVertex = Points.ToVertex(nextCurve.EndPoint);
                        nextVertex = (startVertex.Equals(nextVertex)) ? endVertex : startVertex;

                    }
                    if (!grouped[vertices[v]].Last().Equals(nextCurve))
                    {
                        grouped[vertices[v]].Add(nextCurve);
                    }

                }

            }
            
            List<PolyCurve> polyCurves = new List<PolyCurve>();
            List<DSCurve> ungrouped = new List<DSCurve>();
            foreach(var group in grouped.Values)
            {
                if(group.Count > 1)
                {
                    polyCurves.Add(PolyCurve.ByJoinedCurves(group));
                }
                else
                {
                    ungrouped.Add(group.First());
                }
            }

            return new Dictionary<string, object>()
            {
                {"polycurves", polyCurves },
                {"ungrouped",  ungrouped}
            };
        }

        /// <summary>
        /// Creates a simplified version of the curve by creating lines with a maximum length defined.
        /// </summary>
        /// <param name="curve">Curve to polygonize</param>
        /// <param name="maxLength">Maximum length of subdivisions</param>
        /// <param name="asPolycurve">If true returns a Polycurve or a list of lines otherwise.</param>
        /// <returns></returns>
        public static object Polygonize(DSCurve curve, double maxLength, bool asPolycurve = false)
        {
            //TODO : Look into http://www.antigrain.com/research/adaptive_bezier/index.html
            if (curve == null) { throw new ArgumentNullException("curve"); }
            List<DSCurve> lines = new List<DSCurve>();
            bool isStraight = curve.Length.AlmostEqualTo(curve.StartPoint.DistanceTo(curve.EndPoint));
            if (isStraight)
            {
                lines.Add(curve);
            }
            else
            {
                int divisions = (int)Math.Ceiling(curve.Length / maxLength);
                if(divisions > 1)
                {
                    var points = curve.PointsAtEqualSegmentLength(divisions);
                    lines.Add(Line.ByStartPointEndPoint(curve.StartPoint, points.First()));
                    for (var i = 0; i < points.Count() - 1; i++)
                    {
                        lines.Add(Line.ByStartPointEndPoint(points[i], points[i + 1]));
                    }
                    lines.Add(Line.ByStartPointEndPoint(points.Last(), curve.EndPoint));
                }
                else
                {
                    lines.Add(Line.ByStartPointEndPoint(curve.StartPoint, curve.EndPoint));
                }
            }

            if (asPolycurve)
            {
                return PolyCurve.ByJoinedCurves(lines);
            }
            else
            {
                return lines;
            }

        }

        #endregion
    }
}
