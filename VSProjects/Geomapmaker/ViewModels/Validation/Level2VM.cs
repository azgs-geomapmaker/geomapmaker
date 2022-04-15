using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels.Validation
{
    public class Level2VM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public Level2VM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async Task Validate()
        {
            Result1 = Check1();
            NotifyPropertyChanged("Result1");

            Result2 = Check2();
            NotifyPropertyChanged("Result2");

            Result3 = Check3();
            NotifyPropertyChanged("Result3");

            Result4 = Check4();
            NotifyPropertyChanged("Result4");

            Result5 = await Check5Async();
            NotifyPropertyChanged("Result5");

            Result6 = Check6();
            NotifyPropertyChanged("Result6");

            Result7 = Check7();
            NotifyPropertyChanged("Result7");

            Result8 = Check8();
            NotifyPropertyChanged("Result8");

            Result9 = await Check9Async();
            NotifyPropertyChanged("Result9");

            ParentVM.UpdateLevel2Results(_validationErrors.Count);
        }

        public string Result1 { get; set; } = "Checking..";
        public string Result2 { get; set; } = "Checking..";
        public string Result3 { get; set; } = "Checking..";
        public string Result4 { get; set; } = "Checking..";
        public string Result5 { get; set; } = "Checking..";
        public string Result6 { get; set; } = "Checking..";
        public string Result7 { get; set; } = "Checking..";
        public string Result8 { get; set; } = "Checking..";
        public string Result9 { get; set; } = "Checking..";

        // 2.1 Has required elements: nonspatial tables DataSources, DescriptionOfMapUnits, GeoMaterialDict;
        // feature dataset GeologicMap with feature classes ContactsAndFaults and MapUnitPolys
        private string Check1()
        {
            List<string> errors = new List<string>();

            if (Data.DataSources.DataSourceTableExists() == false)
            {
                errors.Add("DataSources not found.");
            }

            if (Data.DescriptionOfMapUnits.DmuTableExists() == false)
            {
                errors.Add("DescriptionOfMapUnits not found.");
            }

            if (Data.GeoMaterials.GeoMaterialDictTableExists() == false)
            {
                errors.Add("GeoMaterialDict not found.");
            }

            if (Data.ContactsAndFaults.ContactsAndFaultsExists() == false)
            {
                errors.Add("ContactsAndFaults not found.");
            }

            if (Data.MapUnitPolys.MapUnitPolysExists() == false)
            {
                errors.Add("MapUnitPolys not found.");
            }

            if (errors.Count == 0)
            {
                return "Passed";
            }
            else
            {
                _validationErrors["Result1"] = _helpers.Helpers.ErrorListToTooltip(errors);
                RaiseErrorsChanged("Result1");
                return "Failed";
            }
        }

        // 2.2 Required fields within required elements are present and correctly defined
        private string Check2()
        {
            return "Skipped";
        }

        // 2.3 GeologicMap topology: no internal gaps or overlaps in MapUnitPolys,
        // boundaries of MapUnitPolys are covered by ContactsAndFaults
        private string Check3()
        {
            return "Skipped";
        }

        // 2.4 All map units in MapUnitPolys have entries in DescriptionOfMapUnits table
        private string Check4()
        {
            return "Skipped";
        }

        // 2.5 No duplicate MapUnit values in DescriptionOfMapUnit table
        private async Task<string> Check5Async()
        {
            List<string> duplicates = await Data.DescriptionOfMapUnits.GetDuplicateMapUnitsAsync();

            if (duplicates.Count > 0)
            {
                List<string> errors = new List<string>() { "Duplicated:" };

                foreach (var id in duplicates)
                {
                    errors.Add(id);
                }

                _validationErrors["Result5"] = _helpers.Helpers.ErrorListToTooltip(errors);
                RaiseErrorsChanged("Result5");
                return "Failed";
            }
            else
            {
                return "Passed";
            }
        }

        // 2.6 Certain field values within required elements have entries in Glossary table
        private string Check6()
        {
            return "Skipped";
        }

        // 2.7 No duplicate Term values in Glossary table
        private string Check7()
        {
            return "Skipped";
        }

        // 2.8 All xxxSourceID values in required elements have entries in DataSources table
        private string Check8()
        {
            return "Skipped";
        }

        // 2.9 No duplicate DataSources_ID values in DataSources table
        private async Task<string> Check9Async()
        {
            List<string> duplicates = await Data.DataSources.GetDuplicateIdsAsync();

            if (duplicates.Count > 0)
            {
                List<string> errors = new List<string>() { "Duplicated:" };

                foreach (var id in duplicates)
                {
                    errors.Add(id);
                }

                _validationErrors["Result9"] = _helpers.Helpers.ErrorListToTooltip(errors);
                RaiseErrorsChanged("Result9");
                return "Failed";
            }
            else
            {
                return "Passed";
            }
        }

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
