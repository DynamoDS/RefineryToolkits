﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Autodesk.RefineryToolkits.MassingSandbox.Generate
{
    /// <summary>
    /// Site description.
    /// </summary>
    public static class Site
    {
        /// <summary>
        /// Uses setback and height to create boundary volume for building solid to fit into.
        /// </summary>
        /// <param name="siteOutline">Site boundary, from Revit.</param>
        /// <param name="setback">Site setback distance.</param>
        /// <param name="heightLimit">Maximum building height.</param>
        /// <returns name="siteSolid">Allowable volume for building mass.</returns>
        /// <returns name="siteOffset">Allowable footprint for building mass.</returns>
        /// <search>site,design,refactory</search>
        [NodeCategory("Create")]
        [MultiReturn(["siteSolid", "siteOffset"])]
        public static Dictionary<string, object> VolumeByOutlineSetback(Curve siteOutline, double setback = 0, double heightLimit = 100)
        {
            Solid siteMass;
            Curve siteOffset;

            ArgumentNullException.ThrowIfNull(siteOutline);
            if (setback < 0) { throw new ArgumentOutOfRangeException(nameof(setback), $"{nameof(setback)} must be greater than or equal to 0."); }
            if (heightLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(heightLimit), $"{nameof(heightLimit)} must be greater than 0."); }

            var inset1 = siteOutline.OffsetMany(setback, siteOutline.Normal);
            var inset2 = siteOutline.OffsetMany(-setback, siteOutline.Normal);

            if (inset1[0].Length < inset2[0].Length)
            {
                siteOffset = inset1[0];
                inset1.ForEach(c => c.Dispose());
            }
            else
            {
                siteOffset = inset2[0];
            }

            using (var zAxis = Vector.ZAxis())
            {
                // Ensure that the mass is always extruded upwards.
                if (siteOffset.Normal.AngleWithVector(zAxis) > 90)
                {
                    heightLimit = -heightLimit;
                }
            }

            siteMass = siteOffset.ExtrudeAsSolid(heightLimit);

            inset1.ForEach(c => c.Dispose());
            inset2.ForEach(c => c.Dispose());

            return new Dictionary<string, object>
            {
                {"siteSolid", siteMass},
                {"siteOffset", siteOffset }
            };
        }

        /// <summary>
        /// Gets site components from polysurface (e.g. Revit mass).
        /// </summary>
        /// <param name="polysurface">Referenced site element.</param>
        /// <returns name="solidList">Individual solids in site geometry.</returns>
        /// <returns name="boundingBoxList">Bounding box for each element.</returns>
        /// <returns name="heightList">Height of each element.</returns>
        /// <search>refinery</search>
        [NodeCategory("Query")]
        [MultiReturn(["solidList", "boundingBoxList", "heightList"])]
        public static Dictionary<string, object> ContextByElement(PolySurface polysurface)
        {
            Solid[] solidList = null;
            List<BoundingBox> boundingBoxList = null;
            List<double> heightList = null;

            ArgumentNullException.ThrowIfNull(polysurface);

            solidList = polysurface.ExtractSolids();

            boundingBoxList = solidList.Select(e => e.BoundingBox).ToList();

            heightList = boundingBoxList.Select(b => b.MaxPoint.Z - b.MinPoint.Z).ToList();

            return new Dictionary<string, object>
            {
                {"solidList", solidList},
                {"boundingBoxList", boundingBoxList},
                {"heightList", heightList}
            };
        }
    }
}
