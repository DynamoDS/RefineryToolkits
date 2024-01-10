/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using System;
using Autodesk.RefineryToolkits.Core.Utillites;

namespace Autodesk.RefineryToolkits.Core.Geometry
{
    // Resources:
    // https://www.mathsisfun.com/algebra/vectors-cross-product.html
    // http://www.analyzemath.com/stepbystep_mathworksheets/vectors/vector3D_angle.html
    // https://betterexplained.com/articles/cross-product/
    // http://mathworld.wolfram.com/CrossProduct.html

    public class Vector
    {
        #region Public Properties
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public double Length { get; private set; }
        #endregion


        #region Constructors

        private Vector(double x, double y, double z, double length = double.PositiveInfinity)
        {
            X = x;
            Y = y;
            Z = z;
            Length = (double.IsPositiveInfinity(length)) ? Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)) : length;
        }

        public static Vector ByCoordinates(double x, double y, double z)
        {
            return new Vector(x, y, z);
        }

        public static Vector ByTwoVertices(Vertex start, Vertex end)
        {
            var x = end.X - start.X;
            var y = end.Y - start.Y;
            var z = end.Z - start.Z;
            var length = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return new Vector(x, y, z, length);
        }

        public static Vector XAxis()
        {
            return new Vector(1, 0, 0, 1);
        }

        public static Vector YAxis()
        {
            return new Vector(0, 1, 0, 1);
        }

        public static Vector ZAxis()
        {
            return new Vector(0, 0, 1, 1);
        }
        #endregion

        #region Public Methods
        public double Dot(Vector vector)
        {
            return (X * vector.X) + (Y * vector.Y) + (Z * vector.Z);
        }

        public double Angle(Vector vector)
        {
            double dot = Dot(vector);
            double cos = dot / (Length * vector.Length);
            if (cos > 1)
            {
                return Math.Acos(1).ToDegrees();
            }
            else if (cos < -1)
            {
                return Math.Acos(-1).ToDegrees();
            }
            else
            {
                return Math.Acos(cos).ToDegrees();
            }
        }

        public Vector Cross(Vector vector)
        {
            double x = (Y * vector.Z) - (Z * vector.Y);
            double y = (Z * vector.X) - (X * vector.Z);
            double z = (X * vector.Y) - (Y * vector.X);
            double angle = Angle(vector).ToRadians();
            double length = Length * vector.Length * Math.Sin(angle);
            return new Vector(x, y, z, length);
        }

        public Vector Scale(double factor)
        {
            return new Vector(X * factor, Y * factor, Z * factor);
        }

        public Vector Normalized()
        {
            return new Vector(X / Length, Y / Length, Z / Length, Length / Length);
        }

        public bool IsParallelTo(Vector vector)
        {
            var dot = Math.Abs(Normalized().Dot(vector.Normalized()));
            return dot.AlmostEqualTo(1);
        }

        public Vertex AsVertex()
        {
            return Vertex.ByCoordinates(X, Y, Z);
        }

        public static Vector operator *(Vector vector, double value)
        {
            return new Vector(vector.X * value, vector.Y * value, vector.Z * value);
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
            System.Globalization.NumberFormatInfo inf = new()
            {
                NumberDecimalSeparator = "."
            };
            return string.Format("gVector(X = {0}, Y = {1}, Z = {2}, Length = {3}", X.ToString("0.000", inf), Y.ToString("0.000", inf), Z.ToString("0.000", inf), Length.ToString("0.000", inf));
        }

    }
}
