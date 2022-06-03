using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels.Validation
{
    public class GemsVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public GemsVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async Task Validate()
        {
            Dictionary<string, List<string>> errors = await GemsValidation.GetErrorsAsync();

            Symbology = "Skipped";
            NotifyPropertyChanged("Symbology");

            DataSources = errors["DataSources"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("DataSources");

            DescriptionOfMapUnits = errors["DescriptionOfMapUnits"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("DescriptionOfMapUnits");

            Glossary = errors["Glossary"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("Glossary");

            GeoMaterialDict = errors["GeoMaterialDict"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("GeoMaterialDict");

            MapUnitPolys = errors["MapUnitPolys"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("MapUnitPolys");

            ContactsAndFaults = errors["ContactsAndFaults"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("ContactsAndFaults");

            Stations = errors["Stations"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("Stations");

            OrientationPoints = errors["OrientationPoints"]?.Count == 0 ? "Passed" : "Failed";
            NotifyPropertyChanged("OrientationPoints");

            foreach (string key in errors.Keys)
            {
                if (errors[key]?.Count != 0)
                {
                    _validationErrors[key] = _helpers.Helpers.ErrorListToTooltip(errors[key]);
                }
            }

            ParentVM.UpdateGemsResults(_validationErrors.Count);
        }

        public string SymbologyTooltip => "Symbology table is not in GeMS specification.";

        public string DataSourcesTooltip => "Table exists.<br>" +
                                       "No duplicate tables.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate datasources_id values.<br>" +
                                       "No unused datasources_id values.<br>" +
                                       "No missing datasources_id values.";

        public string DescriptionOfMapUnitsTooltip => "Table exists.<br>" +
                                       "No duplicate tables.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate MapUnit values.<br>" +
                                       "No duplicate Name values.<br>" +
                                       "No duplicate FullName values.<br>" +
                                       "No duplicate AreaFillRGB values.<br>" +
                                       "No duplicate HierarchyKey values.<br>" +
                                       "No duplicate DescriptionOfMapUnits_ID values.<br>" +
                                       "HierarchyKeys unique and well-formed.<br>" +
                                       "GeoMaterial are defined in GeoMaterialDict.";


        public string GlossaryTooltip => "Table exists.<br>" +
                                       "No duplicate tables.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Glossary_ID values.<br>" +
                                       "No duplicate Term values.<br>";


        public string GeoMaterialDictooltip => "Table exists.<br>" +
                                       "No duplicate tables.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "GeoMaterialDict table has not been modified.<br>";

        public string MapUnitPolysTooltip => "Layer exists.<br>" +
                                       "No duplicate layers.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate MapUnitPolys_ID values.<br>";

        public string ContactsAndFaultsTooltip => "Layer exists.<br>" +
                                       "No duplicate layers.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Label values.<br>" +
                                       "No duplicate ContactsAndFaults_ID values.<br>";

        public string StationsTooltip => "Layer exists.<br>" +
                                       "No duplicate layers.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Stations_ID values.<br>";

        public string OrientationPointsTooltip => "Layer exists.<br>" +
                                       "No duplicate layers.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate OrientationPoints_ID values.<br>";

        public string Symbology { get; set; } = "Checking..";
        public string DataSources { get; set; } = "Checking..";
        public string DescriptionOfMapUnits { get; set; } = "Checking..";
        public string Glossary { get; set; } = "Checking..";
        public string GeoMaterialDict { get; set; } = "Checking..";
        public string MapUnitPolys { get; set; } = "Checking..";
        public string ContactsAndFaults { get; set; } = "Checking..";
        public string Stations { get; set; } = "Checking..";
        public string OrientationPoints { get; set; } = "Checking..";

        #region Validation

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public bool HasErrors => _validationErrors.Count > 0;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (IEnumerable)_validationErrors[propertyName];
        }

        #endregion
    }
}
