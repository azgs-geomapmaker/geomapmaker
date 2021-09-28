using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.ViewModels;
using System.Collections.Generic;

namespace Geomapmaker
{
    internal class HeadingsViewModel : DockPane
    {

        private const string _dockPaneID = "Geomapmaker_Headings";

        public HeadingsCreateVM Create { get; set; } = new HeadingsCreateVM();

        public HeadingsEditVM Edit { get; set; } = new HeadingsEditVM();

        public HeadingsDeleteVM Delete { get; set; } = new HeadingsDeleteVM();

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            {"NameDescription", "TODO" },
            {"NameNotes", "TODO" },

            {"DescriptionDescription", "TODO" },
            {"DescriptionNotes", "TODO" },

            {"ParentDescription", "TODO" },
            {"ParentNotes", "TODO" },
            
            // Combobox to select a heading to edit
            {"EditDescription", "TODO" },
            {"EditNotes", "TODO" },

            // Combobox to select a heading to delete
            {"DeleteDescription", "TODO" },
            {"DeleteNotes", "TODO" },

            // Tree view
            {"TreeDescription", "TODO" },
            {"TreeNotes", "TODO" },
        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
            {
                return;
            }

            pane.Activate();
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
