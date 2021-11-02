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
        private bool _origModuleSetting1 = true;
        private string _origModuleSetting2 = "";

        protected override Task InitializeAsync()
        {
            // Get the settings
            Dictionary<string, string> settings = GeomapmakerModule.Current.Settings;

            //ModuleSetting1 = settings.ContainsKey("Setting1") ? System.Convert.ToBoolean(settings["Setting1"]) : true;

            ModuleSetting2 = settings.ContainsKey("Setting2") ? settings["Setting2"] : "";

            // keep track of the original values (used for comparison when saving)
            _origModuleSetting1 = ModuleSetting1;
            _origModuleSetting2 = ModuleSetting2;

            return Task.FromResult(0);
        }

        // Determines if the current settings are different from the original.
        private bool IsDirty()
        {
            if (_origModuleSetting1 != ModuleSetting1)
            {
                return true;
            }

            if (_origModuleSetting2 != ModuleSetting2)
            {
                return true;
            }

            return false;
        }

        protected override Task CommitAsync()
        {
            if (IsDirty())
            {
                // store the new settings in the dictionary ... save happens in OnProjectSave
                Dictionary<string, string> settings = GeomapmakerModule.Current.Settings;

                if (settings.ContainsKey("Setting1"))
                    settings["Setting1"] = ModuleSetting1.ToString();
                else
                    settings.Add("Setting1", ModuleSetting1.ToString());

                if (settings.ContainsKey("Setting2"))
                    settings["Setting2"] = ModuleSetting2;
                else
                    settings.Add("Setting2", ModuleSetting2);

                // set the project dirty
                Project.Current.SetDirty(true);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called when the page is destroyed.
        /// </summary>
        protected override void Uninitialize()
        {
        }

        private bool _moduleSetting1;
        public bool ModuleSetting1
        {
            get { return _moduleSetting1; }
            set
            {
                if (SetProperty(ref _moduleSetting1, value, () => ModuleSetting1))
                    //You must set "IsModified = true" to have your CommitAsync called
                    base.IsModified = true;
            }
        }

        private string _moduleSetting2;
        public string ModuleSetting2
        {
            get { return _moduleSetting2; }
            set
            {
                if (SetProperty(ref _moduleSetting2, value, () => ModuleSetting2))
                    //You must set "IsModified = true" to have your CommitAsync called
                    base.IsModified = true;
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
