using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels.Validation
{
    public class GeomapmakerVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public GeomapmakerVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async Task Validate()
        {
            Dictionary<string, List<string>> errors = await GeomapmakerValidation.GetErrorsAsync();

            Symbology = errors["Symbology"]?.Count == 0 ? "Passed" : "Failed";
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

            ParentVM.UpdateGeomapmakerResults(_validationErrors.Count);
        }

        // Tooltips
        public string SymbologyTooltip => "Table exists.<br>No duplicate tables.<br>No missing fields.<br>";
        public string DataSourcesTooltip => "";
        public string DescriptionOfMapUnitsTooltip => "Check for toolbar fields.";
        public string GlossaryTooltip => "";
        public string GeoMaterialDictTooltip => "";
        public string Tooltip => "";
        public string MapUnitPolysTooltip => "";
        public string ContactsAndFaultsTooltip => "";
        public string StationsTooltip => "";
        public string OrientationPointsTooltip => "";

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

        #endregion Validation
    }
}