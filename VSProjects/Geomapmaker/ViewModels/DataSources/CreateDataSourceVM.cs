using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.DataSources
{
    public class CreateDataSourceVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave { get; }

        public DataSourcesViewModel ParentVM { get; set; }

        public CreateDataSourceVM(DataSourcesViewModel parentVM)
        {
            ParentVM = parentVM;
            CommandSave = new RelayCommand(() => SaveAsync(), () => CanSave());
            Id = "";
            Source = "";
        }

        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value, () => Id);
                ValidateId(Id, "Id");
            }
        }

        private string _source;
        public string Source
        {
            get => _source;
            set
            {
                SetProperty(ref _source, value, () => Source);
                ValidateSource(Source, "Source");
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value, () => Url);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value, () => Notes);
        }

        private bool CanSave()
        {
            return !HasErrors;
        }

        private async void SaveAsync()
        {
            string errorMessage = null;

            StandaloneTable ds = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (ds == null)
            {
                MessageBox.Show("DataSources table not found in active map.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    Table enterpriseTable = ds.GetTable();

                    EditOperation editOperation = new EditOperation();

                    editOperation.Callback(context =>
                    {
                        TableDefinition tableDefinition = enterpriseTable.GetDefinition();

                        using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                        {
                            rowBuffer["DataSources_ID"] = Id;
                            rowBuffer["Source"] = Source;
                            rowBuffer["Url"] = Url;
                            rowBuffer["Notes"] = Notes;

                            using (Row row = enterpriseTable.CreateRow(rowBuffer))
                            {
                                // To Indicate that the attribute table has to be updated.
                                context.Invalidate(row);
                            }
                        }
                    }, enterpriseTable);

                    bool result = editOperation.Execute();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();

                    // Trim the stack-trace from the error msg
                    if (errorMessage.Contains("--->"))
                    {
                        errorMessage = errorMessage.Substring(0, errorMessage.IndexOf("--->"));
                    }
                }
            });

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "One or more errors occured.");
            }
            else
            {
                ParentVM.RefreshDataSourcesAsync();

                // Reset values
                Id = "";
                Source = "";
                Url = "";
                Notes = "";
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

        // TODO - Validate unique
        // Validate id
        private void ValidateId(string name, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.DataSources.Any(a => a.DataSource_ID.ToLower() == Id?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "ID is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        // Validate source
        private void ValidateSource(string name, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(name))
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
    }
}
