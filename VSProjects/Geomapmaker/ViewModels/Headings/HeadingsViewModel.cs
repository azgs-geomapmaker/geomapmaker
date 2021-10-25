using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.Generic;

namespace Geomapmaker.ViewModels.Headings
{
    internal class HeadingsViewModel : DockPane
    {

        private const string _dockPaneID = "Geomapmaker_Headings";

        // View models
        public CreateHeadingsVM Create { get; set; } = new CreateHeadingsVM();

        public EditHeadingsVM Edit { get; set; } = new EditHeadingsVM();

        public DeleteHeadingsVM Delete { get; set; } = new DeleteHeadingsVM();

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"CreateHeading", "TODO CreateHeading" },
            {"EditHeading", "TODO EditHeading" },
            {"DeleteHeading", "TODO DeleteHeading" },

            // Control Labels
            {"Name", "TODO Name" },
            {"Description", "TODO Description" },
            
            // Heading Selection Comboboxes
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
            {"Name", 254 },
            {"Description", 3000 },
        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Headings_ShowButton : Button
    {
        protected override void OnClick()
        {
            HeadingsViewModel.Show();
        }
    }
}
