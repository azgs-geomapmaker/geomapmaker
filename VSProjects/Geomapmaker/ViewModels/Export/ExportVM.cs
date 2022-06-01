using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Export
{
    public class ExportVM : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandExport => new RelayCommand(() => Export());

        public ExportVM()
        {

        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public void Export()
        {
            // Get the project name
            string projectName = Project.Current.Name;

            // Remove extension
            projectName = projectName.Replace(".aprx", "");

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = projectName,
                DefaultExt = ".gdb",
                Filter = "File geodatabase (.gdb)|*.gdb"
            };

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dialog.FileName;
            }
        }

        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (System.Collections.IEnumerable)_validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        private void ValidateRequiredString(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowExport : Button
    {
        private Views.Export.Export _export = null;

        protected override void OnClick()
        {
            //already open?
            if (_export != null)
            {
                _export.Close();
                return;
            }

            _export = new Views.Export.Export
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _export.Closed += (o, e) =>
            {
                _export = null;
            };

            _export.exportVM.WindowCloseEvent += (s, e) => _export.Close();

            _export.Show();
        }
    }
}
