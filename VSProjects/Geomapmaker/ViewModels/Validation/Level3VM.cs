using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class Level3VM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public Level3VM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
            Result1 = Check1();
            Result2 = Check2();
            Result3 = Check3();
            Result4 = Check4();
            Result5 = Check5();
            Result6 = Check6();
            Result7 = Check7();
            Result8 = Check8();
            Result9 = Check9();
            Result10 = Check10();
            Result11 = Check11();
            Result12 = Check12();
            Result13 = Check13();

            ParentVM.UpdateLevel3Results(_validationErrors.Count);
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
        public string Result10 { get; set; } = "Checking..";
        public string Result11 { get; set; } = "Checking..";
        public string Result12 { get; set; } = "Checking..";
        public string Result13 { get; set; } = "Checking..";

        // 3.1 Table and field definitions conform to GeMS schema
        private string Check1()
        {
            //List<string> errors = new List<string>();

            //errors.Add("Error 1");
            //errors.Add("Error 2");
            //errors.Add("Error 3");

            //_validationErrors["Result1"] = _helpers.Helpers.ErrorListToTooltip(errors);

            //RaiseErrorsChanged("Result1");

            //return "Failed";
            return "Skipped";
        }

        // 3.2 All map-like feature datasets obey topology rules. No MapUnitPolys gaps or overlaps.
        // No ContactsAndFaults overlaps, self-overlaps, or self-intersections. MapUnitPoly boundaries covered by ContactsAndFaults
        private string Check2()
        {
            return "Skipped";
        }

        // 3.3 No missing required values
        private string Check3()
        {
            return "Skipped";
        }

        // 3.4 No missing terms in Glossary
        private string Check4()
        {
            return "Skipped";
        }

        // 3.5 No unnecessary terms in Glossary
        private string Check5()
        {
            return "Skipped";
        }

        // 3.6 No missing sources in DataSources
        private string Check6()
        {
            return "Skipped";
        }

        // 3.7 No unnecessary sources in DataSources
        private string Check7()
        {
            return "Skipped";
        }

        // 3.8 No map units without entries in DescriptionOfMapUnits
        private string Check8()
        {
            return "Skipped";
        }

        // 3.9 No unnecessary map units in DescriptionOfMapUnits
        private string Check9()
        {
            return "Skipped";
        }

        // 3.10 HierarchyKey values in DescriptionOfMapUnits are unique and well formed
        private string Check10()
        {
            return "Skipped";
        }

        // 3.11 All values of GeoMaterial are defined in GeoMaterialDict. GeoMaterialDict is as specified in the GeMS standard
        private string Check11()
        {
            return "Skipped";
        }

        // 3.12 No duplicate _ID values
        private string Check12()
        {
            return "Skipped";
        }

        // 3.13 No zero-length or whitespace-only strings
        private string Check13()
        {
            return "Skipped";
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
