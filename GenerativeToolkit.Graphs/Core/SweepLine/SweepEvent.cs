using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Geometry;
using GenerativeToolkit.Graphs.DataStructures;
using GenerativeToolkit.Graphs.Extensions;


namespace GenerativeToolkit.Graphs.Core
{
    /// <summary>
    /// Class to hold information about Vertex and Edges on 
    /// the SweepLine algorithm. SweepEvents are compared by X, then Y coordinates
    /// of the vertex. If same vertex, Pairs are compared insted.
    /// </summary>
    public class SweepEvent : IEquatable<SweepEvent>, IComparable<SweepEvent>
    {
        internal PolygonType polygonType;

        #region Public Properties
        /// <summary>
        /// Vertex associated with the event
        /// </summary>
        public gVertex Vertex { get; set; }

        /// <summary>
        /// SweepEvent pair
        /// </summary>
        public SweepEvent Pair { get; set; }

        /// <summary>
        /// Edge associated with the event
        /// </summary>
        public gEdge Edge { get; set; }

        /// <summary>
        /// Determines if SweepEvent comes first on a left to right direction
        /// </summary>
        public bool IsLeft { get; set; }

        /// <summary>
        /// Flags if associated edge is inside other polygon on boolean operations
        /// </summary>
        public bool? IsInside { get; set; }


        /// <summary>
        /// Flags if the associated edge represents an in-out transition into the polygon
        /// in an upwards, y-axis direction.
        /// </summary>
        public bool? InOut { get; set; }

        /// <summary>
        /// Label to define how a SweepEvent contributes to a polygon boolean operation.
        /// </summary>
        public SweepEventLabel Label;

        #endregion

        #region Constructor
        /// <summary>
        /// SweepEvent default constructor
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        public SweepEvent(gVertex vertex, gEdge edge)
        {
            this.Vertex = vertex;
            this.Edge = edge;
        }
        #endregion

        /// <summary>
        /// Updates the edge and Pair event with a new gVertex
        /// </summary>
        /// <param name="newPairVertex"></param>
        public void UpdatePairVertex(gVertex newPairVertex)
        {
            this.Edge = gEdge.ByStartVertexEndVertex(this.Vertex, newPairVertex);
            this.Pair = new SweepEvent(newPairVertex, this.Edge)
            {
                Pair = this,
                IsLeft = !this.IsLeft,
                polygonType = this.polygonType
            };
        }

        /// <summary>
        /// SweepEvent comparer.
        /// A SweepEvent is considered less than other if having smaller X, then Y and then Z.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(SweepEvent other)
        {
            if(other == null) { return -1; }
            // If same Vertex, compare pairs
            if (this.Vertex.Equals(other.Vertex))
            {
                if (this.Pair.Vertex.Equals(other.Pair.Vertex))
                {
                    return 0;
                }
                else
                {
                    return this.Pair.CompareTo(other.Pair);
                }
            }
            // If same X
            else if(this.Vertex.X.AlmostEqualTo(other.Vertex.X))
            {
                // If same Y
                if(this.Vertex.Y.AlmostEqualTo(other.Vertex.Y))
                {
                    return this.Vertex.Z.CompareTo(other.Vertex.Z);
                }
                else
                {
                    return this.Vertex.Y.CompareTo(other.Vertex.Y);
                }
            }else
            {
                return this.Vertex.X.CompareTo(other.Vertex.X);
            }
        }

        /// <summary>
        /// SweepEvent equality comparer. SweepEvents are considered equals if have the same edge.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SweepEvent other)
        {
            return this.Edge.Equals(other.Edge);
        }

        /// <summary>
        /// Less Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator <(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == -1;
        }

        /// <summary>
        /// Greater Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator >(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == 1;
        }

        /// <summary>
        /// Less or Equal Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator <=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) <= 0;
        }

        /// <summary>
        /// Greater or Equal Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator >=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) >= 0;
        }

        /// <summary>
        /// SweepEvent string override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("(Vertex:{0}, Pair:{1})", this.Vertex.ToString(), this.Pair.Vertex.ToString());
        }
    }

    /// <summary>
    /// Custom Vertical Ascending IComparer for SweepEvent.
    /// Lower SweepEvent has lowest X. At same X, lowest Y and finally lowest Z.
    /// </summary>
    internal class SortEventsVerticalAscendingComparer : IComparer<SweepEvent>
    {
        /// <summary>
        /// Custom SweepEvent Vertical Ascending Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(SweepEvent x, SweepEvent y)
        {
            if (x.Vertex.Equals(y.Vertex))
            {
                if (x.Pair.Vertex.Equals(y.Pair.Vertex))
                {
                    return 0;
                }
                else
                {
                    return Compare(x.Pair, y.Pair);
                }
            }
            // If same Y, below is the one with lower X
            else if (x.Vertex.Y.AlmostEqualTo(y.Vertex.Y))
            {
                // If same X, below is the one with lower Z
                if (x.Vertex.X.AlmostEqualTo(y.Vertex.X))
                {
                    return x.Vertex.Z.CompareTo(y.Vertex.Z);
                }
                else
                {
                    return x.Vertex.X.CompareTo(y.Vertex.X);
                }
            }
            else
            {
                return x.Vertex.Y.CompareTo(y.Vertex.Y);
            }
        }
    }

}
