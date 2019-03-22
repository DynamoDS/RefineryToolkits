using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
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
        /// <param name="FloorType">Type of created Revit floors.</param>
        /// <param name="LevelPrefix">Prefix for names of created Revit levels.</param>
        /// <returns name="FloorElements">Revit floor elements.</returns>
        /// <search>refinery</search>
        public static List<Elements.Floor> CreateRevitFloors(
            Autodesk.DesignScript.Geometry.Surface[] Floors, 
            Elements.FloorType FloorType = null, 
            string LevelPrefix = "Dynamo Level")
        {
            if (Floors == null) { throw new ArgumentNullException(nameof(Floors)); }

            var FloorElements = new List<Elements.Floor>();
            var collector = new FilteredElementCollector(Document);
            
            if (!(FloorType.InternalElement is Autodesk.Revit.DB.FloorType floorType))
            {
                throw new ArgumentOutOfRangeException(nameof(FloorType));
            }

            var levels = collector.OfClass(typeof(Autodesk.Revit.DB.Level)).ToElements()
                .Where(e => e is Autodesk.Revit.DB.Level)
                .Cast<Autodesk.Revit.DB.Level>();

            TransactionManager.Instance.EnsureInTransaction(Document);

            for (var i = 0; i < Floors.Length; i++)
            {
                if (Floors[i] == null) { throw new ArgumentNullException(nameof(Floors)); }
                
                var levelName = $"{LevelPrefix} {i + 1}";
                var revitLevel = levels.FirstOrDefault(level => level.Name == levelName);

                if (revitLevel != null)
                {
                    // Adjust existing level to correct height.
                    revitLevel.Elevation = Floors[i].BoundingBox.MaxPoint.Z / footToMm;
                }
                else
                {
                    // Create new level.
                    revitLevel = Autodesk.Revit.DB.Level.Create(Document, Floors[i].BoundingBox.MaxPoint.Z / footToMm);
                    revitLevel.Name = levelName;
                }

                var revitCurves = new CurveArray();
                Floors[i].PerimeterCurves().ForEach(x => revitCurves.Append(x.ToRevitType()));

                var revitFloor = Document.Create.NewFloor(revitCurves, floorType, revitLevel, true);

                FloorElements.Add(revitFloor.ToDSType(false) as Elements.Floor);
            }

            TransactionManager.Instance.TransactionTaskDone();

            return FloorElements;
        }

        /// <summary>
        /// Creates a Revit mass as a direct shape from building masser
        /// </summary>
        /// <param name="BuildingSolid">The building volume.</param>
        /// <param name="Category">A category for the mass.</param>
        /// <returns name="RevitBuilding">Revit DirectShape element.</returns>
        /// <search>refinery</search>
        public static Elements.DirectShape CreateRevitMass(Autodesk.DesignScript.Geometry.Solid BuildingSolid, Elements.Category Category)
        {
            if (BuildingSolid == null)
            {
                throw new ArgumentNullException(nameof(BuildingSolid));
            }

            var revitCategory = Document.Settings.Categories.get_Item(Category.Name);

            TransactionManager.Instance.EnsureInTransaction(Document);

            var RevitBuilding = Autodesk.Revit.DB.DirectShape.CreateElement(Document, revitCategory.Id);

            try
            {
                RevitBuilding.SetShape(new[] { DynamoToRevitBRep.ToRevitType(BuildingSolid) });
            }
            catch (Exception ex)
            {
                try
                {
                    RevitBuilding.SetShape(BuildingSolid.ToRevitType());
                }
                catch
                {
                    throw new ArgumentException(ex.Message);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();
            
            return RevitBuilding.ToDSType(false) as Elements.DirectShape;
        }
    }
}
