using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Extensions;

namespace GenerativeToolkit.Graphs.Geometry
{
    // Resources:
    // https://www.mathsisfun.com/algebra/vectors-cross-product.html
    // http://www.analyzemath.com/stepbystep_mathworksheets/vectors/vector3D_angle.html
    // https://betterexplained.com/articles/cross-product/
    // http://mathworld.wolfram.com/CrossProduct.html

    public class gVector 
    {
        #region Public Properties
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public double Length { get; private set; } 
        #endregion


        #region Constructors

        private gVector(double x, double y, double z, double length = Double.PositiveInfinity)
        {
            X = x;
            Y = y;
            Z = z;
            Length = (Double.IsPositiveInfinity(length)) ? Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)) : length;
        }

        public static gVector ByCoordinates(double x, double y, double z)
        {
            return new gVector(x, y, z);
        }

        public static gVector ByTwoVertices(gVertex start, gVertex end)
        {
            var x = end.X - start.X;
            var y = end.Y - start.Y;
            var z = end.Z - start.Z;
            var length = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return new gVector(x, y, z, length);
        }

        public static gVector XAxis()
        {
            return new gVector(1, 0, 0, 1);
        }

        public static gVector YAxis()
        {
            return new gVector(0, 1, 0, 1);
        }

        public static gVector ZAxis()
        {
            return new gVector(0, 0, 1, 1);
        }
        #endregion

        #region Public Methods
        public double Dot(gVector vector)
        {
            return (this.X * vector.X) + (this.Y * vector.Y) + (this.Z * vector.Z);
        }

        public double Angle(gVector vector)
        {
            double dot = this.Dot(vector);
            double cos = dot / (this.Length * vector.Length);
            if(cos > 1)
            {
                return Math.Acos(1).ToDegrees();
            }
            else if(cos < -1)
            {
                return Math.Acos(-1).ToDegrees();
            }
            else
            {
                return Math.Acos(cos).ToDegrees();
            }
        }

        public gVector Cross(gVector vector)
        {
            double x = (this.Y * vector.Z) - (this.Z * vector.Y);
            double y = (this.Z * vector.X) - (this.X * vector.Z);
            double z = (this.X * vector.Y) - (this.Y * vector.X);
            double angle = this.Angle(vector).ToRadians();
            double length = this.Length * vector.Length * Math.Sin(angle);
            return new gVector(x, y, z, length);
        }

        public gVector Scale(double factor)
        {
            return new gVector(this.X * factor, this.Y * factor, this.Z * factor);
        }

        public gVector Normalized()
        {
            return new gVector(this.X / this.Length, this.Y / this.Length, this.Z / this.Length, this.Length / this.Length);
        }

        public bool IsParallelTo(gVector vector)
        {
            var dot = Math.Abs(this.Normalized().Dot(vector.Normalized()));
            return dot.AlmostEqualTo(1);
        }

        public gVertex AsVertex()
        {
            return gVertex.ByCoordinates(this.X, this.Y, this.Z);
        }

        public static gVector operator *(gVector vector, double value)
        {
            return new gVector(vector.X * value, vector.Y * value, vector.Z * value);
        }
        #endregion

        #region Internal Methods


        #endregion

        /// <summary>
        /// Override of ToStringMethod
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Globalization.NumberFormatInfo inf = new System.Globalization.NumberFormatInfo();
            inf.NumberDecimalSeparator = ".";
            return string.Format("gVector(X = {0}, Y = {1}, Z = {2}, Length = {3}", X.ToString("0.000", inf), Y.ToString("0.000", inf), Z.ToString("0.000", inf), Length.ToString("0.000", inf));
        }

    }
}
