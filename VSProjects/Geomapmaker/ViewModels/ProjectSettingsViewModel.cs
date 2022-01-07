using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels
{
    internal class ProjectSettingsViewModel : Page
    {
        private string _origInstance;
        private string _origDatabase;
        private string _origVersion;
        private string _origUsername;
        private string _origPassword;

        protected override Task InitializeAsync()
        {
            // Get the settings
            Dictionary<string, string> settings = GeomapmakerModule.Current.Settings;

            Instance = settings.ContainsKey("Instance") ? settings["Instance"] : "";
            Database = settings.ContainsKey("Database") ? settings["Database"] : "";
            Version = settings.ContainsKey("Version") ? settings["Version"] : "";
            Username = settings.ContainsKey("Username") ? settings["Username"] : "";
            Password = settings.ContainsKey("Password") ? settings["Password"] : "";

            // keep track of the original values (used for comparison when saving)
            _origInstance = Instance;
            _origDatabase = Database;
            _origVersion = Version;
            _origUsername = Username;
            _origPassword = Password;

            return Task.FromResult(0);
        }

        // Determines if the current settings are different from the original.
        private bool IsDirty()
        {
            if (_origInstance != Instance)
            {
                return true;
            }

            if (_origDatabase != Database)
            {
                return true;
            }

            if (_origVersion != Version)
            {
                return true;
            }

            if (_origUsername != Username)
            {
                return true;
            }

            if (_origPassword != Password)
            {
                return true;
            }

            return false;
        }

        protected override Task CommitAsync()
        {
            if (IsDirty())
            {
                AddUpdateSetting("Instance", Instance);
                AddUpdateSetting("Database", Database);
                AddUpdateSetting("Version", Version);
                AddUpdateSetting("Username", Username);
                AddUpdateSetting("Password", Password);

                DatabaseConnectionProperties dbConnectionProps = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
                {
                    AuthenticationMode = AuthenticationMode.DBMS,
                    Instance = Instance,
                    Database = Database,
                    Version = Version,
                    User = Username,
                    Password = Password
                };

                DataHelper.SetConnectionProperties(dbConnectionProps);

                // set the project dirty
                Project.Current.SetDirty(true);
            }
            return Task.FromResult(0);
        }

        private void AddUpdateSetting(string settingName, string settingValue)
        {
            Dictionary<string, string> settings = GeomapmakerModule.Current.Settings;

            if (settings.ContainsKey(settingName))
            {
                settings[settingName] = settingValue;
            }
            else
            {
                settings.Add(settingName, settingValue);
            }
        }

        private string instance;
        public string Instance
        {
            get => instance;
            set
            {
                if (SetProperty(ref instance, value, () => Instance))
                {
                    IsModified = true;
                }
            }
        }

        private string database;
        public string Database
        {
            get => database;
            set
            {
                if (SetProperty(ref database, value, () => Database))
                {
                    IsModified = true;
                }
            }
        }

        private string version;
        public string Version
        {
            get => version;
            set
            {
                if (SetProperty(ref version, value, () => Version))
                {
                    IsModified = true;
                }
            }
        }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                if (SetProperty(ref username, value, () => Username))
                {
                    IsModified = true;
                }
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                if (SetProperty(ref password, value, () => Password))
                {
                    IsModified = true;
                }
            }
        }

    }

    /// <summary>
    /// Button implementation to show the property sheet.
    /// </summary>
    internal class ProjectSettings_ShowButton : Button
    {
        protected override void OnClick()
        {
            // collect data to be passed to the page(s) of the property sheet
            Object[] data = new object[] { "Page UI content" };

            if (!PropertySheet.IsVisible)
                PropertySheet.ShowDialog("Geomapmaker_ProjectSettings", "Geomapmaker_ProjectSettings", data);

        }
    }
}
