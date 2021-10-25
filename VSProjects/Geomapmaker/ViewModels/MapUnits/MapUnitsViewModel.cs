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
        public string GeoMaterialConfidenceLabel => "GeoMaterial" + Environment.NewLine + "Confidence:";

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

        // Max length of the field's string
        public Dictionary<string, int> MaxLength => new Dictionary<string, int>
        {
            {"MapUnit", 10 },
            {"Name", 254 },
            {"FullName", 254 },
            {"RelativeAge", 254 },
            {"Description", 3000 },
            {"Label", 30 },
        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

        // Convert Color to RGB
        public static string ColorToRGB(Color? color)
        {
            if (color == null)
            {
                return "#00000000";
            }

            // GeMS: (1) each RGB color value is integer between 0 and 255; (2) values are left - padded with zeroes so that each consists of 3 digits; (3) values are separated by commas with no spaces(for example, nnn,nnn,nnn).

            return $"{color.Value.R:000},{color.Value.G:000},{color.Value.B:000}";
        }

        // Convert RGB string to System Color
        public static Color? RGBtoColor(string rgb)
        {
            // Null if the string is empty
            if (string.IsNullOrEmpty(rgb))
            {
                return null;
            }

            // Split by comma 
            string[] strArray = rgb.Split(',');

            // Color from RGB bytes
            return strArray.Length != 3
                ? null
                : (Color?)Color.FromRgb(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2]));
        }

        // Convert RGB string to Hex
        public static string RGBtoHex(string rgb)
        {
            // Null if the string is empty
            if (string.IsNullOrEmpty(rgb))
            {
                return "#00000000";
            }

            // Split by comma 
            string[] strArray = rgb.Split(',');

            if (strArray.Length != 3)
            {
                return "#00000000";
            }

            return Color.FromRgb(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2])).ToString();
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