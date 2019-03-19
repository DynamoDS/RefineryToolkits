using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
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
        internal static Autodesk.Revit.DB.Document Document => DocumentManager.Instance.CurrentDBDocument;

        /// <summary>
        /// Creates a Revit floors from building masser
        /// </summary>
        /// <param name="Floors"></param>
        /// <returns></returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "FloorElement" })]
        public static Dictionary<string, object> CreateRevitFloors(double Floors)
        {
            List<Surface> elements = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"FloorElement", elements},
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
            Autodesk.Revit.DB.DirectShape shape = null;
            
            if (Mass == null)
            {
                throw new ArgumentNullException(nameof(Mass));
            }

            var category = Document.Settings.Categories.get_Item(CategoryName);

            TransactionManager.Instance.EnsureInTransaction(Document);

            shape = Autodesk.Revit.DB.DirectShape.CreateElement(Document, category.Id);

            try
            {
                shape.SetShape(new[] { DynamoToRevitBRep.ToRevitType(Mass) });
            }
            catch (Exception ex)
            {
                try
                {
                    shape.SetShape(Mass.ToRevitType());
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
