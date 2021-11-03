using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.RibbonElements;
using Geomapmaker.ViewModels;

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
            // Reset the flag
            hasSettings = false;
        }

        private void OnProjectOpen(ProjectEventArgs args)
        {
            // If flag has not been set then we didn't enter OnReadSettingsAsync
            if (!hasSettings)
            {
                Settings.Clear();
            }

            DataHelper.SetConnectionProperties(GetConnectionPropertiesFromSettings());
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

        public DatabaseConnectionProperties GetConnectionPropertiesFromSettings()
        {
            string Instance = Settings.ContainsKey("Instance") ? Settings["Instance"] : "";
            string Database = Settings.ContainsKey("Database") ? Settings["Database"] : "";
            string Version = Settings.ContainsKey("Version") ? Settings["Version"] : "";
            string Username = Settings.ContainsKey("Username") ? Settings["Username"] : "";
            string Password = Settings.ContainsKey("Password") ? Settings["Password"] : "";

            DatabaseConnectionProperties dbConnectionProps = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
            {
                AuthenticationMode = AuthenticationMode.DBMS,
                Instance = Instance,
                Database = Database,
                Version = Version,
                User = Username,
                Password = Password
            };

            return dbConnectionProps;
        }

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

            // Get the property names from the setting view model
            List<string> settingPropertyNames = typeof(ProjectSettingsViewModel).GetProperties().Select(f => f.Name).ToList();

            foreach (string key in settingPropertyNames)
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
