using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Geomapmaker.ViewModels.MapUnits
{
    internal class MapUnitsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_DescriptionOfMapUnits";

        // This is the only way I found to add a New Line in a label so far
        public string GeoMaterialConfidenceLabel  => "GeoMaterial" + Environment.NewLine + "Confidence:";

        // View models
        public CreateMapUnitVM Create { get; set; } = new CreateMapUnitVM();

        public EditMapUnitVM Edit { get; set; } = new EditMapUnitVM();

        public DeleteMapUnitVM Delete { get; set; } = new DeleteMapUnitVM();

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"CreateHeading", "TODO CreateHeading" },
            {"EditHeading", "TODO EditHeading" },
            {"DeleteHeading", "TODO DeleteHeading" },

            // Control Labels
            {"MapUnit", "TODO MapUnit" },
            {"Name", "TODO Name" },
            {"FullName", "TODO FullName" },
            {"Age", "TODO Age" },
            {"OlderInterval", "TODO OlderInterval" },
            {"YoungerInterval", "TODO YoungerInterval" },
            {"RelativeAge", "TODO RelativeAge" },
            {"Description", "TODO Description" },
            {"Parent", "TODO Parent" },
            {"Label", "TODO Label" },
            {"Color", "TODO Color" },
            {"GeoMaterial", "TODO GeoMaterial" },
            {"GeoMaterialConfidence", "TODO GeoMaterialConfidence" },

            // Map Unit Selection Comboboxes 
            {"Edit", "TODO Edit" },
            {"Delete", "TODO Delete" },

            // Buttons
            {"ClearButton", "TODO ClearButton" },
            {"SaveButton", "TODO SaveButton" },
            {"UpdateButton", "TODO UpdateButton" },
            {"DeleteButton", "TODO DeleteButton" },
        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

        // Convert Hex Color to RGB
        public static string HexToRGB(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);

            return $"{color.R},{color.G},{color.B}";
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class MapUnits_ShowButton : Button
    {
        protected override void OnClick()
        {
            MapUnitsViewModel.Show();
        }
    }
}