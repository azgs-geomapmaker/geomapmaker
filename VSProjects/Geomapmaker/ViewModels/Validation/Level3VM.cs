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

        private string Check1()
        {
            List<string> errors = new List<string>();

            errors.Add("Error 1");
            errors.Add("Error 2");
            errors.Add("Error 3");

            _validationErrors["Result1"] = _helpers.Helpers.ErrorListToTooltip(errors);

            RaiseErrorsChanged("Result1");

            return "Failed";
        }

        private string Check2()
        {
            return "Skipped";
        }

        private string Check3()
        {
            return "Skipped";
        }

        private string Check4()
        {
            return "Skipped";
        }

        private string Check5()
        {
            return "Skipped";
        }

        private string Check6()
        {
            return "Skipped";
        }

        private string Check7()
        {
            return "Skipped";
        }

        private string Check8()
        {
            return "Skipped";
        }

        private string Check9()
        {
            return "Skipped";
        }

        private string Check10()
        {
            return "Skipped";
        }
        private string Check11()
        {
            return "Skipped";
        }
        private string Check12()
        {
            return "Skipped";
        }
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
