using System;
using System.Collections.Generic;
using System.Linq;
using DynamoElements = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitElements = Autodesk.Revit.DB;
using DynamoRevitElements = Revit.Elements;
using RevitReferences = Revit.References;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Autodesk.DesignScript.Geometry;

namespace GenerativeToolkit
{
    /// <summary>
    /// Revit description.
    /// </summary>
    public static class Revit
    {
        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;

        /// <summary>
        /// Creates Revit floors from building floor surfaces.
        /// </summary>
        /// <param name="Floors">Floor surfaces.</param>
        /// <param name="FloorType">Type of created Revit floors.</param>
        /// <param name="LevelPrefix">Prefix for names of created Revit levels.</param>
        /// <returns name="FloorElements">Revit floor elements.</returns>
        /// <search>refinery</search>
        public static List<List<DynamoRevitElements.Floor>> CreateRevitFloors(
            DynamoElements.Surface[][] Floors,
            DynamoRevitElements.FloorType FloorType = null, 
            string LevelPrefix = "Dynamo Level")
        {
            if (Floors == null) { throw new ArgumentNullException(nameof(Floors)); }
            
            if (!(FloorType.InternalElement is RevitElements.FloorType floorType))
            {
                throw new ArgumentOutOfRangeException(nameof(FloorType));
            }

            DisplayUnitType unitType = Document.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            
            var FloorElements = new List<List<DynamoRevitElements.Floor>>();
            var collector = new FilteredElementCollector(Document);

            var levels = collector.OfClass(typeof(RevitElements.Level)).ToElements()
                .Where(e => e is RevitElements.Level)
                .Cast<RevitElements.Level>();

            TransactionManager.Instance.EnsureInTransaction(Document);

            for (var i = 0; i < Floors.Length; i++)
            {

                if (Floors[i] == null) { throw new ArgumentNullException(nameof(Floors)); }

                FloorElements.Add(new List<DynamoRevitElements.Floor>());
                
                string levelName = $"{LevelPrefix} {i + 1}";
                var revitLevel = levels.FirstOrDefault(level => level.Name == levelName);

                if (revitLevel != null)
                {
                    // Adjust existing level to correct height.
                    revitLevel.Elevation = UnitUtils.ConvertToInternalUnits(
                        BoundingBox.ByGeometry(Floors[i]).MaxPoint.Z,
                        unitType);
                }
                else
                {
                    // Create new level.
                    revitLevel = RevitElements.Level.Create(Document, UnitUtils.ConvertToInternalUnits(
                        BoundingBox.ByGeometry(Floors[i]).MaxPoint.Z,
                        unitType));
                    revitLevel.Name = levelName;
                }

                var revitCurves = new CurveArray();

                foreach (var surface in Floors[i])
                {
                    var loops = Building.GetSurfaceLoops(surface);

                    revitCurves.Clear();

                    loops[0].Curves().ForEach(curve => revitCurves.Append(curve.ToRevitType()));
                    
                    var revitFloor = Document.Create.NewFloor(revitCurves, floorType, revitLevel, true);

                    FloorElements.Last().Add(revitFloor.ToDSType(false) as DynamoRevitElements.Floor);

                    // Need to finish creating the floor before we add openings in it.
                    TransactionManager.Instance.ForceCloseTransaction();
                    TransactionManager.Instance.EnsureInTransaction(Document);

                    loops.Skip(1).ToArray().ForEach(loop =>
                    {
                        revitCurves.Clear();

                        loop.Curves().ForEach(curve => revitCurves.Append(curve.ToRevitType()));

                        Document.Create.NewOpening(revitFloor, revitCurves, true);
                    });

                    loops.ForEach(x => x.Dispose());
                    revitFloor.Dispose();
                }
                
                revitCurves.Dispose();
            }

            TransactionManager.Instance.TransactionTaskDone();
            
            collector.Dispose();

            return FloorElements;
        }

        /// <summary>
        /// Creates a Revit mass as a direct shape from building masser
        /// </summary>
        /// <param name="BuildingSolid">The building volume.</param>
        /// <param name="Category">A category for the mass.</param>
        /// <returns name="RevitBuilding">Revit DirectShape element.</returns>
        /// <search>refinery</search>
        public static DynamoRevitElements.DirectShape CreateRevitMass(DynamoElements.Solid BuildingSolid, DynamoRevitElements.Category Category)
        {
            if (BuildingSolid == null)
            {
                throw new ArgumentNullException(nameof(BuildingSolid));
            }

            var revitCategory = Document.Settings.Categories.get_Item(Category.Name);

            TransactionManager.Instance.EnsureInTransaction(Document);

            var revitBuilding = RevitElements.DirectShape.CreateElement(Document, revitCategory.Id);

            try
            {
                revitBuilding.SetShape(new[] { DynamoToRevitBRep.ToRevitType(BuildingSolid) });
            }
            catch (Exception ex)
            {
                try
                {
                    revitBuilding.SetShape(BuildingSolid.ToRevitType());
                }
                catch
                {
                    throw new ArgumentException(ex.Message);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            revitCategory.Dispose();

            var directShape = revitBuilding.ToDSType(false) as DynamoRevitElements.DirectShape;

            revitBuilding.Dispose();

            return directShape;
        }
    }
}
