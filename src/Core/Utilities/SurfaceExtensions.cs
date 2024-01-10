using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static Surface BoundingSurface(this Surface surface)
        {
            BoundingBox bb = surface.BoundingBox;
            Cuboid c = bb.ToCuboid();
            Surface srf = (c.Explode())[0] as Surface;

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
        [MultiReturn(["maxSrf", "otherSrfs"])]
        public static Dictionary<string, dynamic> MaximumArea(this List<Surface> surfaces)
        {
            List<double> areas = [];
            for (int i = 0; i < surfaces.Count; i++)
            {
                areas.Add(surfaces[i].Area);
            }


            List<Surface> otherSurfaces = [];
            List<int> maxID = [];
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

            Surface maxSurface = surfaces[maxID[0]];

            Dictionary<string, dynamic> newOutput = new()
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
        [MultiReturn(["insetCrvs", "outsetCrvs"])]
        public static Dictionary<string, Curve[]> OffsetPerimeterCurves(this Surface surface, double offset)
        {
            var srfPerimCrvs = surface.PerimeterCurves();

            PolyCurve plyCrv = PolyCurve.ByJoinedCurves(srfPerimCrvs, 0.001, false);

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

            Curve[] inPerimCrvs;
            try
            {
                var inOffsetCrv = plyCrv.OffsetMany(inOffset, false, plyCrv.Normal);
                PolyCurve inOffsetPolyCrv = PolyCurve.ByJoinedCurves(inOffsetCrv, 0.001, false);
                var inOffsetCrvList = inOffsetPolyCrv.Curves();
                List<Point> inPts = [];
                foreach (Curve c in inOffsetCrvList)
                {
                    inPts.Add(c.StartPoint);
                }
                PolyCurve inOffsetPolyCrv2 = PolyCurve.ByPoints(inPts, true);
                Surface inOffsetSrf = Surface.ByPatch(inOffsetPolyCrv2);
                inPerimCrvs = inOffsetSrf.PerimeterCurves();
                inOffsetSrf.Dispose();
                inOffsetPolyCrv.Dispose();
                inOffsetPolyCrv2.Dispose();
            }
            catch (Exception)
            {
                inPerimCrvs = null;
            }

            Curve[] outPerimCrvs;
            try
            {
                var outOffsetCrv = plyCrv.OffsetMany(inOffset, false, plyCrv.Normal.Reverse());
                var outOffsetPolyCrv = PolyCurve.ByJoinedCurves(outOffsetCrv, 0.001, false);
                var outOffsetCrvList = outOffsetPolyCrv.Curves();
                List<Point> outPts = [];
                foreach (Curve c in outOffsetCrvList)
                {
                    outPts.Add(c.StartPoint);
                }
                PolyCurve outOffsetPolyCrv2 = PolyCurve.ByPoints(outPts, true);
                Surface outOffsetSrf = Surface.ByPatch(outOffsetPolyCrv2);
                outPerimCrvs = outOffsetSrf.PerimeterCurves();
                outOffsetSrf.Dispose();
                outOffsetPolyCrv.Dispose();
                outOffsetPolyCrv2.Dispose();

            }
            catch (Exception)
            {
                outPerimCrvs = null;
            }


            Dictionary<string, Curve[]> newOutput = new()
            {
                {"insetCrvs",inPerimCrvs},
                {"outsetCrvs",outPerimCrvs}
            };

            //Dispose all redundant geometry

            plyCrv.Dispose();
            foreach (Curve c in srfPerimCrvs)
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
        [MultiReturn(["baseSurfaces", "otherSurfaces"])]
        public static Dictionary<string, object> PlanarBase(this List<Surface> surfaces)
        {
            List<double> zValues = [];
            foreach (var srf in surfaces)
            {
                double z = DSCore.Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double min = zValues.Min();

            List<bool> boolList = [];
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
            List<Surface> baseSrfs = surfaces.Zip(boolList, (name, filter) => new { name, filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name, filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput = new()
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
        [MultiReturn(["topSurfaces", "otherSurfaces"])]
        public static Dictionary<string, object> PlanarTop(this List<Surface> surfaces)
        {
            List<double> zValues = [];
            foreach (var srf in surfaces)
            {
                double z = Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double max = zValues.Max();

            List<bool> boolList = [];
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
            List<Surface> topSrfs = surfaces.Zip(boolList, (name, filter) => new { name, filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name, filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput = new()
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
        public static DesignScript.Geometry.Geometry[] SplitPlanarSurfaceByMultipleCurves(this Surface srf, List<Curve> crvs)
        {
            Vector vec = srf.NormalAtParameter(0.5, 0.5);

            List<Surface> srfLst = [];
            foreach (Curve crv in crvs)
            {
                Surface splitSrf = crv.Extrude(vec, 5000);
                srfLst.Add(splitSrf);
            }

            PolySurface polysrf = PolySurface.ByJoinedSurfaces(srfLst);

            DesignScript.Geometry.Geometry[] geo = srf.Split(polysrf);

            vec.Dispose();
            polysrf.Dispose();
            foreach (Surface s in srfLst)
            {
                s.Dispose();
            }

            return geo;

        }
        #endregion
    }
}
