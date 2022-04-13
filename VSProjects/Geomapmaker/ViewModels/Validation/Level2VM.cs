using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class Level2VM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public Level2VM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
            Check1 = "Skipped";
            Check2 = "Skipped";
            Check3 = "Skipped";
            Check4 = "Skipped";
            Check5 = "Skipped";
            Check6 = "Skipped";
            Check7 = "Skipped";
            Check8 = "Skipped";
            Check9 = "Skipped";
        }

        private string _check1 = "Checking..";
        public string Check1
        {
            get => _check1;
            set => SetProperty(ref _check1, value, () => Check1);
        }

        private string _check2 = "Checking..";
        public string Check2
        {
            get => _check2;
            set => SetProperty(ref _check2, value, () => Check2);
        }

        private string _check3 = "Checking..";
        public string Check3
        {
            get => _check3;
            set => SetProperty(ref _check3, value, () => Check3);
        }

        private string _check4 = "Checking..";
        public string Check4
        {
            get => _check4;
            set => SetProperty(ref _check4, value, () => Check4);
        }

        private string _check5 = "Checking..";
        public string Check5
        {
            get => _check5;
            set => SetProperty(ref _check5, value, () => Check5);
        }

        private string _check6 = "Checking..";
        public string Check6
        {
            get => _check6;
            set => SetProperty(ref _check6, value, () => Check6);
        }

        private string _check7 = "Checking..";
        public string Check7
        {
            get => _check7;
            set => SetProperty(ref _check7, value, () => Check7);
        }

        private string _check8 = "Checking..";
        public string Check8
        {
            get => _check8;
            set => SetProperty(ref _check8, value, () => Check8);
        }

        private string _check9 = "Checking..";
        public string Check9
        {
            get => _check9;
            set => SetProperty(ref _check9, value, () => Check9);
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
