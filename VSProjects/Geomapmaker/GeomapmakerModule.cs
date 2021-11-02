﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.RibbonElements;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private GeomapmakerModule()
        {
            ProjectOpenedEvent.Subscribe(OnProjectOpen);
            ProjectClosedEvent.Subscribe(OnProjectClosed);
        }

        private void OnProjectClosed(ProjectEventArgs args)
        {
            // reset the flag
            hasSettings = false;
        }

        private void OnProjectOpen(ProjectEventArgs args)
        {
            // if flag has not been set then we didn't enter OnReadSettingsAsync
            if (!hasSettings)
            {
                Settings.Clear();
            }
        }

        internal Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        private static GeomapmakerModule _this = null;

        //public DataHelper helper = new DataHelper();
        internal static AddEditContactsAndFaultsDockPaneViewModel ContactsAndFaultsVM { get; set; }
        internal static ContactsAndFaultsAddTool ContactsAndFaultsAddTool { get; set; }
        internal static ContactsAndFaultsEditTool ContactsAndFaultsEditTool { get; set; }

        internal static AddEditMapUnitPolysDockPaneViewModel MapUnitPolysVM { get; set; }
        internal static MapUnitPolyAddTool AddMapUnitPolyTool { get; set; }
        internal static MapUnitPolyEditTool EditMapUnitPolyTool { get; set; }

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));

        #region Overrides

        private bool hasSettings = false;
        protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
        {
            // set the flag
            hasSettings = true;

            // clear existing setting values
            Settings.Clear();

            if (settings == null)
            {
                return Task.FromResult(0);
            }

            // Settings defined in the Property sheet’s viewmodel.	
            string[] keys = new string[] { "Setting1", "Setting2" };

            foreach (string key in keys)
            {
                object value = settings.Get(key);

                if (value != null)
                {
                    if (Settings.ContainsKey(key))
                    {
                        Settings[key] = value.ToString();
                    }
                    else
                    {
                        Settings.Add(key, value.ToString());
                    }
                }
            }

            return Task.FromResult(0);
        }

        protected override Task OnWriteSettingsAsync(ModuleSettingsWriter settings)
        {
            foreach (string key in Settings.Keys)
            {
                settings.Add(key, Settings[key]);
            }
            return Task.FromResult(0);
        }

        protected override bool Initialize()
        {
            HideDockPanes();

            return true;
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            HideDockPanes();

            return true;
        }

        // Hide geomapmaker dockpanes when loading/unloading
        private void HideDockPanes()
        {
            // When the app starts up, there will be no user logged in. Clean up dockpanes to reflect this
            List<string> DockPaneIds = new List<string>
            {
                "Geomapmaker_Headings",
                "Geomapmaker_DescriptionOfMapUnits",
                "Geomapmaker_Hierarchy"
            };

            foreach (string dockId in DockPaneIds)
            {
                DockPane pane = FrameworkApplication.DockPaneManager.Find(dockId);
                pane?.Hide();
            }
        }

        #endregion Overrides
    }
}
