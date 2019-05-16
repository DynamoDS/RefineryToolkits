using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.GenerativeToolkit.Core.Geometry.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.GenerativeToolkit.Analyze
{
    public static class Openess
    {
        /// <summary>
        /// gives a openess score from 0-1 based on how enclosed a Surface is. 
        /// </summary>
        /// <param name="surface">Surface of object to check</param>
        /// <param name="tolerance">Takes into account objects that are a given distance away</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing obstacles that might enclose the object to check</param>
        /// <returns>Score from 0-1, 1 being totally enclosed and 0 being totally open</returns>
        public static double FromSurface(
            Surface surface,
            [DefaultArgument("0.0")] double tolerance,
            [DefaultArgument("[]")] List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            List<Curve> perimeterCrvs = surface.OffsetPerimeterCurves(tolerance)["outsetCrvs"].ToList();
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
    }
}
