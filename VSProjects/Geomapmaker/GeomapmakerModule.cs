using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Geomapmaker.Models;
using Geomapmaker.RibbonElements;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private static GeomapmakerModule _this = null;

        // User-selected data source (Ribbon Combobox)
        public static string DataSourceId { get; set; }

        public static DataSourceComboBox DataSourceComboBox { get; set; }

        // Line symbol options
        public static List<GemsSymbol> ContactsAndFaultsSymbols { get; set; }

        // Point symbol options
        public static List<GemsSymbol> OrientationPointSymbols { get; set; }

        // Name of the CF Sketch Template
        public const string CF_SketchTemplateName = "Sketch";

        // Name of the MUP Unassigned Template
        public const string MUP_UnassignedTemplateName = "Unassigned";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));

        protected override bool Initialize()
        {
            // Subscribe to project closing
            ProjectClosedEvent.Subscribe(OnProjectClosed);
            LayersAddedEvent.Subscribe(OnLayersAdded);
            StandaloneTablesAddedEvent.Subscribe(OnStandaloneTablesAdded);

            return true;
        }

        private void OnProjectClosed(ProjectEventArgs args)
        {
            // Clear the Datasource combo
            DataSourceComboBox?.ClearSelection();
        }

        private void OnLayersAdded(LayerEventsArgs args)
        {
            QueuedTask.Run(() =>
            {
                var map = MapView.Active?.Map;
                if (map == null) return;

                // Get all layers (flattened to include group layers)
                var allLayers = map.GetLayersAsFlattenedList();

                // Group layers by name and find duplicates
                var duplicateGroups = allLayers
                    .GroupBy(layer => layer.Name)
                    .Where(group => group.Count() > 1)
                    .ToList();

                if (duplicateGroups.Any())
                {
                    // Build the message
                    var duplicateLayerNames = duplicateGroups
                        .Select(group => $"{group.Key} ({group.Count()} instances)")
                        .ToList();

                    string message = "Duplicate layers found:\n\n" + 
                                   string.Join("\n", duplicateLayerNames);

                    MessageBox.Show(message, "Duplicate Layers Detected", 
                                  System.Windows.MessageBoxButton.OK, 
                                  System.Windows.MessageBoxImage.Warning);
                }
            });
        }

        private void OnStandaloneTablesAdded(StandaloneTableEventArgs args)
        {
            QueuedTask.Run(() =>
            {
                var map = MapView.Active?.Map;
                if (map == null) return;

                // Get all standalone tables
                var allTables = map.GetStandaloneTablesAsFlattenedList();

                // Group tables by name and find duplicates
                var duplicateGroups = allTables
                    .GroupBy(table => table.Name)
                    .Where(group => group.Count() > 1)
                    .ToList();

                if (duplicateGroups.Any())
                {
                    // Build the message
                    var duplicateTableNames = duplicateGroups
                        .Select(group => $"{group.Key} ({group.Count()} instances)")
                        .ToList();

                    string message = "Duplicate standalone tables found:\n\n" + 
                                   string.Join("\n", duplicateTableNames);

                    MessageBox.Show(message, "Duplicate Tables Detected", 
                                  System.Windows.MessageBoxButton.OK, 
                                  System.Windows.MessageBoxImage.Warning);
                }
            });
        }
    }
}
