using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.DataSources
{
    public class EditDataSourceVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandUpdate { get; }

        public EditDataSourceVM(DataSourcesViewModel parentVM)
        {
            ParentVM = parentVM;
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
        }

        public DataSourcesViewModel ParentVM { get; set; }

        private DataSource _selected;
        public DataSource Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);
                Id = Selected?.DataSource_ID;
                Source = Selected?.Source;
                Url = Selected?.Url;
                Notes = Selected?.Notes;

                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value, () => Id);
                ValidateId(Id, "Id");
                ValidateChangeWasMade();
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
                ValidateChangeWasMade();
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                SetProperty(ref _url, value, () => Url);
                ValidateChangeWasMade();
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                SetProperty(ref _notes, value, () => Notes);
                ValidateChangeWasMade();
            }
        }

        private bool CanUpdate()
        {
            return Selected != null && !HasErrors;
        }

        private async void UpdateAsync()
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
                        QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + Selected.ObjecttId };

                        using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    // In order to update the Map and/or the attribute table.
                                    // Has to be called before any changes are made to the row.
                                    context.Invalidate(row);

                                    row["DataSources_ID"] = Id;
                                    row["Source"] = Source;
                                    row["Url"] = Url;
                                    row["Notes"] = Notes;

                                    // After all the changes are done, persist it.
                                    row.Store();

                                    // Has to be called after the store too.
                                    context.Invalidate(row);
                                }
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
                Selected = null;

                await ParentVM.RefreshDataSourcesAsync();

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

        // Validate id
        private void ValidateId(string name, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.DataSources.Where(a => a.ObjecttId != Selected?.ObjecttId).Any(a => a.DataSource_ID.ToLower() == Id?.ToLower()))
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

        private void ValidateChangeWasMade()
        {
            const string propertyKey = "SilentError";

            if (Selected != null && Id == Selected.DataSource_ID &&
                Source == Selected.Source &&
                Url == Selected.Url &&
                Notes == Selected.Notes)
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
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
