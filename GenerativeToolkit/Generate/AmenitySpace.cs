using System;
using System.Collections.Generic;
using System.Linq;
using DSCore;
using DSCore.Properties;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System.Collections;

namespace Autodesk.GenerativeToolkit.Generate
{
    //[IsVisibleInDynamoLibrary(false)]
    public static class AmenitySpace
    {
        #region Create
        /// <summary>
        /// Creates an amentiy space on a given surface, returning both the amenity space and the remaining space within the original surface
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="offset"></param>
        /// <param name="depth"></param>
        /// <search></search>
        //[IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "amenitySrf", "remainSrf" })]
        public static Dictionary<string, Autodesk.DesignScript.Geometry.Surface> Create(Autodesk.DesignScript.Geometry.Surface surface, double offset, double depth)
        {
            List<Curve> inCrvs = Autodesk.GenerativeToolkit.Utilities.Surface.OffsetPerimeterCurves(surface, offset)["insetCrvs"].ToList();
            Autodesk.DesignScript.Geometry.Surface inSrf = Autodesk.DesignScript.Geometry.Surface.ByPatch(PolyCurve.ByJoinedCurves(inCrvs));

            Curve max;
            List<Curve> others;
            Dictionary<string, dynamic> dict = Autodesk.GenerativeToolkit.Utilities.Curve.MaximumLength(inCrvs);
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
            List<Curve> matchCrvs = Autodesk.GenerativeToolkit.Utilities.Curve.FindMatchingVectorCurves(max, perimCrvs);


            Curve max2;
            Dictionary<string, dynamic> dict2 = Autodesk.GenerativeToolkit.Utilities.Curve.MaximumLength(matchCrvs);
            if (dict2["maxCrv"].Count < 1)
            {
                max2 = dict2["otherCrvs"][0] as Curve;
            }
            else
            {
                max2 = dict2["maxCrv"][0] as Curve;
            }

            Vector vec = Autodesk.GenerativeToolkit.Utilities.Vector.ByTwoCurves(max2, max);

            Autodesk.DesignScript.Geometry.Curve transLine = max.Translate(vec, depth) as Curve;
            Line extendLine = Autodesk.GenerativeToolkit.Utilities.Line.ExtendAtBothEnds(transLine, 5000);


            List<Curve> crvList = new List<Curve>() { max, extendLine };
            Autodesk.DesignScript.Geometry.Surface loftSrf = Autodesk.DesignScript.Geometry.Surface.ByLoft(crvList);

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
                var l = Autodesk.GenerativeToolkit.Utilities.Line.ExtendAtBothEnds(crv, 5000);
                extendCurves.Add(l);
            }

            List<Autodesk.DesignScript.Geometry.Surface> split = Autodesk.GenerativeToolkit.Utilities.Surface.SplitPlanarSurfaceByMultipleCurves(loftSrf, extendCurves).OfType<Autodesk.DesignScript.Geometry.Surface>().ToList();

            Autodesk.DesignScript.Geometry.Surface amenitySurf = Autodesk.GenerativeToolkit.Utilities.Surface.MaximumArea(split)["maxSrf"] as Autodesk.DesignScript.Geometry.Surface;

            Autodesk.DesignScript.Geometry.Surface remainSurf = inSrf.Split(amenitySurf)[0] as Autodesk.DesignScript.Geometry.Surface;

            Dictionary<string, Autodesk.DesignScript.Geometry.Surface> newOutput;
            newOutput = new Dictionary<string, Autodesk.DesignScript.Geometry.Surface>
            {
                {"amenitySrf",amenitySurf},
                {"remainSrf",remainSurf}
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
        #endregion 
    }
}
