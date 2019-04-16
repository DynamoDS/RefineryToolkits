using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using GTUtil = Autodesk.GenerativeToolkit.Utilities;

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class Openess
    {
        #region Public Methods
        /// <summary>
        /// gives a openess score from 0-1 based on how enclosed a Surface is. 
        /// </summary>
        /// <param name="surface">Surface of object to check</param>
        /// <param name="tolerance">Takes into account objects that are a given distance away</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing obstacles that might enclose the object to check</param>
        /// <returns>Score from 0-1, 1 being totally enclosed and 0 being totally open</returns>
        public static double FromSurface(Surface surface, [DefaultArgument("0.0")] double tolerance, [DefaultArgument("[]")] List<Polygon> boundary, [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            List<Curve> perimeterCrvs = GTUtil.Surface.OffsetPerimeterCurves(surface, tolerance)["outsetCrvs"].ToList();
            List<Polygon> intersectionPolygons = new List<Polygon>();
            intersectionPolygons.AddRange(boundary);
            intersectionPolygons.AddRange(obstacles);

            double perimeterLength = surface.Perimeter;
            double openessScore = 0;
            foreach (Curve crv in perimeterCrvs)
            {
                foreach (Polygon poly in intersectionPolygons)
                {
                    if (crv.DoesIntersect(poly))
                    {
                        try
                        {
                            List<Curve> intersections = crv.Intersect(poly).Cast<Curve>().ToList();
                            intersections.ForEach(c => openessScore += c.Length / perimeterLength);
                        }
                        catch (System.InvalidCastException)
                        {
                            continue;
                        }
                        
                    }
                }
            }

            return openessScore;
        }
        #endregion

        #region Private Methods

        private static double OpenessScore(List<Autodesk.DesignScript.Geometry.Line> lines, List<Polygon> polygons)
        {
            double score = 0.0;
            foreach (var line in lines)
            {
                foreach (var polygon in polygons)
                {
                    if (line.DoesIntersect(polygon))
                    {
                        score += 0.25;
                    }
                }
            }
            return score;
        }
        #endregion
    }
}
