using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit
{
    /// <summary>
    /// ElementCreation description.
    /// </summary>
    public static class ElementCreation
    {
        private const double footToMm = 12 * 25.4;

        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;

        /// <summary>
        /// Creates Revit floors from building floor surfaces.
        /// </summary>
        /// <param name="Floors">Floor surfaces.</param>
        /// <param name="FloorTypeName">Type of created Revit floors.</param>
        /// <param name="LevelPrefix">Prefix for names of created Revit levels.</param>
        /// <returns name="FloorElements">Revit floor elements.</returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "FloorElements" })]
        public static Dictionary<string, object> CreateRevitFloors(
            Autodesk.DesignScript.Geometry.Surface[] Floors, 
            string FloorTypeName = "Generic 150mm", 
            string LevelPrefix = "Dynamo Level")
        {
            var revitFloors = new List<Floor>();
            var collector = new FilteredElementCollector(Document);

            if (!(collector
                .OfClass(typeof(FloorType))
                .First<Element>(e => e.Name.Equals(FloorTypeName, global::System.StringComparison.Ordinal)) 
                is FloorType floorType))
            {
                throw new ArgumentOutOfRangeException(nameof(FloorTypeName));
            }

            var levels = collector.OfClass(typeof(Level)).ToElements()
                .Where(e => e is Level)
                .Cast<Level>();

            TransactionManager.Instance.EnsureInTransaction(Document);

            for (var i = 0; i < Floors.Length; i++)
            {
                if (Floors[i] == null)
                {
                    throw new ArgumentNullException(nameof(Floors));
                }

                try
                {
                    var levelName = $"{LevelPrefix} {i + 1}";
                    var revitLevel = levels.FirstOrDefault(level => level.Name == levelName);
                    if (revitLevel != null)
                    {
                        revitLevel.Elevation = Floors[i].BoundingBox.MaxPoint.Z / footToMm;
                    }
                    else
                    {
                        revitLevel = Level.Create(Document, Floors[i].BoundingBox.MaxPoint.Z / footToMm);
                        revitLevel.Name = levelName;
                    }

                    var revitCurves = new CurveArray();
                    Floors[i].PerimeterCurves().ForEach(x => revitCurves.Append(x.ToRevitType()));

                    var revitFloor = Document.Create.NewFloor(revitCurves, floorType, revitLevel, true);

                    revitFloors.Add(revitFloor);
                }

                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"FloorElements", revitFloors},
            };
        }

        /// <summary>
        /// Creates a Revit mass as a direct shape from building masser
        /// </summary>
        /// <param name="BuildingSolid">The building volume.</param>
        /// <param name="FloorElevations">Elevation of each floor in building.</param>
        /// <param name="CategoryName">A category for the mass.</param>
        /// <returns name="RevitBuilding">Revit DirectShape element.</returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "RevitBuilding" })]
        public static Dictionary<string, object> CreateRevitMass(Autodesk.DesignScript.Geometry.Solid BuildingSolid, IEnumerable<double> FloorElevations, string CategoryName = "Mass")
        {
            DirectShape shape = null;
            
            if (BuildingSolid == null)
            {
                throw new ArgumentNullException(nameof(BuildingSolid));
            }

            var category = Document.Settings.Categories.get_Item(CategoryName);

            TransactionManager.Instance.EnsureInTransaction(Document);

            shape = DirectShape.CreateElement(Document, category.Id);

            try
            {
                shape.SetShape(new[] { DynamoToRevitBRep.ToRevitType(BuildingSolid) });
            }
            catch (Exception ex)
            {
                try
                {
                    shape.SetShape(BuildingSolid.ToRevitType());
                }
                catch
                {
                    throw new ArgumentException(ex.Message);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"RevitBuilding", shape},
            };
        }
    }
}
