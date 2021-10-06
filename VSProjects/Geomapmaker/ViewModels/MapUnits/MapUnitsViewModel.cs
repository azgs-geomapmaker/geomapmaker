using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.Generic;
using System.Windows.Media;

namespace Geomapmaker.ViewModels.MapUnits
{
    internal class MapUnitsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_DescriptionOfMapUnits";

        // View models
        public CreateMapUnitVM Create { get; set; } = new CreateMapUnitVM();

        public EditMapUnitVM Edit { get; set; } = new EditMapUnitVM();

        public DeleteMapUnitVM Delete { get; set; } = new DeleteMapUnitVM();

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            {"MapUnitDescription", "TODO" },
            {"MapUnitNotes", "TODO" },

            {"NameDescription", "TODO" },
            {"NameNotes", "TODO" },

            {"FullNameDescription", "TODO" },
            {"FullNameNotes", "TODO" },

            {"AgeDescription", "TODO" },
            {"AgeNotes", "TODO" },

            {"OlderIntervalDescription", "TODO" },
            {"OlderIntervalNotes", "TODO" },

            {"YoungerIntervalDescription", "TODO" },
            {"YoungerIntervalNotes", "TODO" },

            {"RelativeAgeDescription", "TODO" },
            {"RelativeAgeNotes", "TODO" },

            {"DescriptionDescription", "TODO" },
            {"DescriptionNotes", "TODO" },

            {"ParentDescription", "TODO" },
            {"ParentNotes", "TODO" },

            {"LabelDescription", "TODO" },
            {"LabelNotes", "TODO" },

            {"ColorDescription", "TODO" },
            {"ColorNotes", "TODO" },

            {"GeoMaterialDescription", "TODO" },
            {"GeoMaterialNotes", "TODO" },

            {"GeoMaterialConfidenceDescription", "TODO" },
            {"GeoMaterialConfidenceNotes", "TODO" },



            {"Description", "TODO" },
            {"Notes", "TODO" },

        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

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