#region namespaces
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
#endregion

namespace Autodesk.GenerativeToolkit.Generate
{
    public static class AmenitySpace
    {
        private const string amenitySurface = "amenitySrf";
        private const string remainingSurface = "remainSrf";

        /// <summary>
        /// Creates an amentiy space on a given surface, returning both the amenity space and the remaining space within the original surface
        /// </summary>
        /// <param name="surface">Surface to create Amenity Spaces on</param>
        /// <param name="offset">How much to offset to surface perimeter with</param>
        /// <param name="depth"></param>
        /// <search></search>
        [MultiReturn(new[] { amenitySurface, remainingSurface })]
        public static Dictionary<string, Autodesk.DesignScript.Geometry.Surface> Create(Autodesk.DesignScript.Geometry.Surface surface, 
            double offset, 
            double depth)
        {
            List<Curve> inCrvs = Utilities.Surface.OffsetPerimeterCurves(surface, offset)["insetCrvs"].ToList();
            Surface inSrf = Surface.ByPatch(PolyCurve.ByJoinedCurves(inCrvs));

            Curve max;
            List<Curve> others;
            Dictionary<string, dynamic> dict = Utilities.Curve.MaximumLength(inCrvs);
            if (dict["maxCrv"].Count < 1)
            {
                max = dict["otherCrvs"][0] as Curve;
                int count = dict["otherCrvs"].Count;
                List<Curve> rest = dict["otherCrvs"];
                others = rest.GetRange(1, (count - 1));
            }
            else
            {
                max = dict["maxCrv"][0] as Curve;
                others = dict["otherCrvs"];
            }

            List<Curve> perimCrvs = surface.PerimeterCurves().ToList();
            List<Curve> matchCrvs = Utilities.Curve.FindMatchingVectorCurves(max, perimCrvs);


            Curve max2;
            Dictionary<string, dynamic> dict2 = Utilities.Curve.MaximumLength(matchCrvs);
            if (dict2["maxCrv"].Count < 1)
            {
                max2 = dict2["otherCrvs"][0] as Curve;
            }
            else
            {
                max2 = dict2["maxCrv"][0] as Curve;
            }

            Vector vec = Utilities.Vector.ByTwoCurves(max2, max);

            Curve transLine = max.Translate(vec, depth) as Curve;
            Line extendLine = Utilities.Line.ExtendAtBothEnds(transLine, 1);


            List<Curve> crvList = new List<Curve>() { max, extendLine };
            Surface loftSrf = Surface.ByLoft(crvList);

            List<bool> boolLst = new List<bool>();
            foreach (var crv in others)
            {
                bool b = max.DoesIntersect(crv);
                boolLst.Add(b);
            }

            List<Curve> intersectingCurves = others.Zip(boolLst, (name, filter) => new { name, filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Curve> extendCurves = new List<Curve>();
            foreach (Curve crv in intersectingCurves)
            {
                var l = Utilities.Line.ExtendAtBothEnds(crv, 1);
                extendCurves.Add(l);
            }

            List<Surface> split = Utilities.Surface.SplitPlanarSurfaceByMultipleCurves(loftSrf, extendCurves).OfType<Surface>().ToList();

            Surface amenitySurf = Utilities.Surface.MaximumArea(split)["maxSrf"] as Surface;

            Surface remainSurf = inSrf.Split(amenitySurf)[0] as Surface;

            Dictionary<string, Surface> newOutput;
            newOutput = new Dictionary<string, Surface>
            {
                {amenitySurface,amenitySurf},
                {remainingSurface,remainSurf}
            };

            //Dispose redundant geometry
            inCrvs.ForEach(crv => crv.Dispose());
            inSrf.Dispose();
            max.Dispose();
            perimCrvs.ForEach(crv => crv.Dispose());
            matchCrvs.ForEach(crv => crv.Dispose());
            max2.Dispose();
            vec.Dispose();
            transLine.Dispose();
            extendLine.Dispose();
            crvList.ForEach(crv => crv.Dispose());
            loftSrf.Dispose();
            intersectingCurves.ForEach(crv => crv.Dispose());
            extendCurves.ForEach(crv => crv.Dispose());    

            return newOutput;
        }
    }
}
