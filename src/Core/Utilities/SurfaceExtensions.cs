using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    [IsVisibleInDynamoLibrary(false)]
    public static class SurfaceExtension
    {

        #region BoundingSurface
        /// <summary>
        /// Return the bounding surface of the input surface
        /// </summary>
        /// <param name="surface"></param>
        /// <search></search>
        public static Autodesk.DesignScript.Geometry.Surface BoundingSurface(this Autodesk.DesignScript.Geometry.Surface surface)
        {
            Autodesk.DesignScript.Geometry.BoundingBox bb = surface.BoundingBox;
            Cuboid c = bb.ToCuboid();
            Autodesk.DesignScript.Geometry.Surface srf = (c.Explode())[0] as Autodesk.DesignScript.Geometry.Surface;

            bb.Dispose();
            c.Dispose();

            return srf;
        }
        #endregion

        #region MaximumArea
        /// <summary>
        /// Return the maximum area surface from a list of surfaces and all the other surfaces
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search></search>
        [MultiReturn(new[] { "maxSrf", "otherSrfs" })]
        public static Dictionary<string, dynamic> MaximumArea(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> areas = new List<double>();
            for (int i = 0; i < surfaces.Count; i++)
            {
                areas.Add(surfaces[i].Area);
            }


            List<Autodesk.DesignScript.Geometry.Surface> otherSurfaces = new List<Autodesk.DesignScript.Geometry.Surface>();
            List<int> maxID = new List<int>();
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == areas.Max())
                {
                    maxID.Add(i);
                }
                else
                {
                    otherSurfaces.Add(surfaces[i]);
                }
            }

            Autodesk.DesignScript.Geometry.Surface maxSurface = surfaces[maxID[0]];

            Dictionary<string, dynamic> newOutput;
            newOutput = new Dictionary<string, dynamic>
            {
                {"maxSrf",maxSurface},
                {"otherSrfs",otherSurfaces}
            };

            return newOutput;
        }
        #endregion

        #region OffsetPerimeterCurves
        /// <summary>
        /// Creates a amentiy space on a given surface
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="offset"></param>
        /// <search></search>
        [MultiReturn(new[] { "insetCrvs", "outsetCrvs" })]
        public static Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]> OffsetPerimeterCurves(this Autodesk.DesignScript.Geometry.Surface surface, double offset)
        {
            List<Autodesk.DesignScript.Geometry.Curve> srfPerimCrvs = surface.PerimeterCurves().ToList();

            Autodesk.DesignScript.Geometry.PolyCurve plyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(srfPerimCrvs);

            double inOffset;
            double outOffset;

            if (offset < 0)
            {
                inOffset = offset;
                outOffset = -offset;
            }
            else
            {
                inOffset = -offset;
                outOffset = offset;
            }

            Autodesk.DesignScript.Geometry.Curve[] inPerimCrvs;
            try
            {
                List<Autodesk.DesignScript.Geometry.Curve> inOffsetCrv = new List<Autodesk.DesignScript.Geometry.Curve>() { (plyCrv.Offset(inOffset)) };
                Autodesk.DesignScript.Geometry.PolyCurve inOffsetPolyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(inOffsetCrv);
                List<Autodesk.DesignScript.Geometry.Curve> inOffsetCrvList = inOffsetPolyCrv.Curves().ToList();
                List<Autodesk.DesignScript.Geometry.Point> inPts = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach (Autodesk.DesignScript.Geometry.Curve c in inOffsetCrvList)
                {
                    inPts.Add(c.StartPoint);
                }
                Autodesk.DesignScript.Geometry.PolyCurve inOffsetPolyCrv2 = PolyCurve.ByPoints(inPts, true);
                Autodesk.DesignScript.Geometry.Surface inOffsetSrf = Autodesk.DesignScript.Geometry.Surface.ByPatch(inOffsetPolyCrv2);
                inPerimCrvs = inOffsetSrf.PerimeterCurves();
                inOffsetSrf.Dispose();
                inOffsetPolyCrv.Dispose();
                inOffsetPolyCrv2.Dispose();
            }
            catch (Exception)
            {
                inPerimCrvs = null;
            }

            Autodesk.DesignScript.Geometry.Curve[] outPerimCrvs;
            try
            {
                List<Autodesk.DesignScript.Geometry.Curve> outOffsetCrv = new List<Autodesk.DesignScript.Geometry.Curve>() { (plyCrv.Offset(outOffset)) };
                Autodesk.DesignScript.Geometry.PolyCurve outOffsetPolyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(outOffsetCrv);
                List<Autodesk.DesignScript.Geometry.Curve> outOffsetCrvList = outOffsetPolyCrv.Curves().ToList();
                List<Autodesk.DesignScript.Geometry.Point> outPts = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach (Autodesk.DesignScript.Geometry.Curve c in outOffsetCrvList)
                {
                    outPts.Add(c.StartPoint);
                }
                Autodesk.DesignScript.Geometry.PolyCurve outOffsetPolyCrv2 = PolyCurve.ByPoints(outPts, true);
                Autodesk.DesignScript.Geometry.Surface outOffsetSrf = Autodesk.DesignScript.Geometry.Surface.ByPatch(outOffsetPolyCrv2);
                outPerimCrvs = outOffsetSrf.PerimeterCurves();
                outOffsetSrf.Dispose();
                outOffsetPolyCrv.Dispose();
                outOffsetPolyCrv2.Dispose();

            }
            catch (Exception)
            {
                outPerimCrvs = null;
            }


            Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]> newOutput;
            newOutput = new Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]>
            {
                {"insetCrvs",inPerimCrvs},
                {"outsetCrvs",outPerimCrvs}
            };

            //Dispose all redundant geometry

            plyCrv.Dispose();
            foreach (Autodesk.DesignScript.Geometry.Curve c in srfPerimCrvs)
            {
                c.Dispose();
            }

            return newOutput;

        }
        #endregion

        #region PlanarBase
        /// <summary>
        /// Extract the planar base surface/s from a list of surfaces.
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search>geometry,base,planar,surface,surfaces,bottom</search>
        [MultiReturn(new[] { "baseSurfaces", "otherSurfaces" })]
        public static Dictionary<string, object> PlanarBase(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> zValues = new List<double>();
            foreach (var srf in surfaces)
            {
                double z = DSCore.Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double min = zValues.Min();

            List<bool> boolList = new List<bool>();
            foreach (double z in zValues)
            {
                if (z == min)
                {
                    boolList.Add(true);
                }
                else
                {
                    boolList.Add(false);
                }
            }
            List<Autodesk.DesignScript.Geometry.Surface> baseSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Autodesk.DesignScript.Geometry.Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput;
            newOutput = new Dictionary<string, object>
            {
                {"baseSurfaces",baseSrfs},
                {"otherSurfaces",otherSrfs}

            };
            return newOutput;
        }
        #endregion

        #region PlanarTop
        /// <summary>
        /// Extract the planar top surface/s from a list of surfaces.
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search>geometry,top,planar,surface,surfaces,ceiling</search>
        [MultiReturn(new[] { "topSurfaces", "otherSurfaces" })]
        public static Dictionary<string, object> PlanarTop(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> zValues = new List<double>();
            foreach (var srf in surfaces)
            {
                double z = System.Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double max = zValues.Max();

            List<bool> boolList = new List<bool>();
            foreach (double z in zValues)
            {
                if (z == max)
                {
                    boolList.Add(true);
                }
                else
                {
                    boolList.Add(false);
                }
            }
            List<Autodesk.DesignScript.Geometry.Surface> topSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Autodesk.DesignScript.Geometry.Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput;
            newOutput = new Dictionary<string, object>
            {
                {"topSurfaces",topSrfs},
                {"otherSurfaces",otherSrfs}

            };
            return newOutput;
        }
        #endregion

        #region SplitPlanarSurfaceByMultipleCurves
        /// <summary>
        /// Splits a surface by multiple curves.
        /// </summary>
        /// <param name="srf"></param>
        /// <param name="crvs"></param>
        /// <search></search>
        public static Autodesk.DesignScript.Geometry.Geometry[] SplitPlanarSurfaceByMultipleCurves(this Autodesk.DesignScript.Geometry.Surface srf, List<Autodesk.DesignScript.Geometry.Curve> crvs)
        {
            Autodesk.DesignScript.Geometry.Vector vec = srf.NormalAtParameter(0.5, 0.5);

            List<Autodesk.DesignScript.Geometry.Surface> srfLst = new List<Autodesk.DesignScript.Geometry.Surface>();
            foreach (Autodesk.DesignScript.Geometry.Curve crv in crvs)
            {
                Autodesk.DesignScript.Geometry.Surface splitSrf = crv.Extrude(vec, 5000);
                srfLst.Add(splitSrf);
            }

            Autodesk.DesignScript.Geometry.PolySurface polysrf = Autodesk.DesignScript.Geometry.PolySurface.ByJoinedSurfaces(srfLst);

            Autodesk.DesignScript.Geometry.Geometry[] geo = srf.Split(polysrf);

            vec.Dispose();
            polysrf.Dispose();
            foreach (Autodesk.DesignScript.Geometry.Surface s in srfLst)
            {
                s.Dispose();
            }

            return geo;

        }
        #endregion
    }
}
