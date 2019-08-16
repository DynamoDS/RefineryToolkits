using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.Core.Utillites;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class Openess
    {
        /// <summary>
        /// Calculates an openess percentage based on how enclosed a Surface is. 
        /// </summary>
        /// <param name="surface">Surface of object to check</param>
        /// <param name="searchDistance">(optional) Only takes into account objects that are this given max distance away.</param>
        /// <param name="boundary">(optional) Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstructions">(optional) List of Polygons representing obstacles that might enclose the object to check</param>
        /// <returns>Percentage of openess, from 0 being totally enclosed and 100 being totally open.</returns>
        public static double FromSurface(
            Surface surface,
            [DefaultArgument("0.0")] double searchDistance,
            [DefaultArgument("[]")] List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstructions)
        {
            List<Curve> perimeterCrvs = surface.OffsetPerimeterCurves(searchDistance)["outsetCrvs"].ToList();
            List<Polygon> intersectionPolygons = new List<Polygon>();
            intersectionPolygons.AddRange(boundary);
            intersectionPolygons.AddRange(obstructions);

            double perimeterLength = surface.Perimeter;
            double openessScore = 0;

            for (var i = 0; i < perimeterCrvs.Count; i++)
            {
                var crv = perimeterCrvs[i];
                for (var j = 0; j < intersectionPolygons.Count; j++)
                {
                    var poly = intersectionPolygons[j];
                    if (!crv.DoesIntersect(poly)) continue;

                    try
                    {
                        List<Curve> intersections = crv.Intersect(poly).Cast<Curve>().ToList();
                        for (int k = 0; k < intersections.Count; k++)
                        {
                            var intersection = intersections[k];
                            openessScore += intersection.Length / perimeterLength;
                            intersection.Dispose();
                        }                       
                    }
                    catch (System.InvalidCastException)
                    {
                        continue;
                    }
                }
            }
            // openessScore 0 means totally open and 100 means totally enclosed
            // that does not make sense with node name, so we flip it
            var invertedScore = 1 - openessScore;

            return invertedScore * 100;
        }
    }
}
