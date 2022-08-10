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
using ArcGIS.Desktop.Framework.Dialogs;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.DataSources
{
    public class EditDataSourceVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        public EditDataSourceVM(DataSourcesViewModel parentVM)
        {
            ParentVM = parentVM;
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
                ValidateRequiredString(Source, "Source");
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
                MessageBox.Show("DataSources table not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table enterpriseTable = ds.GetTable())
                    {
                        if (enterpriseTable != null)
                        {
                            EditOperation editOperation = new EditOperation()
                            {
                                Name = "Edit DataSource",
                            };

                            editOperation.Callback(context =>
                            {
                                QueryFilter filter = new QueryFilter { ObjectIDs = new List<long> { Selected.ObjectId } };

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
                    }
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
                // Check if the Data Source ID has changed
                if (Selected.DataSource_ID != Id)
                {
                    Data.DataSources.UpdateDataSourceForeignKeys(Selected.DataSource_ID, Id);

                    GeomapmakerModule.DataSourceComboBox.ClearSelection();
                }

                ParentVM.CloseProwindow();
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
            else if (ParentVM.DataSources.Where(a => a.ObjectId != Selected?.ObjectId).Any(a => a.DataSource_ID?.ToLower() == Id?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "ID is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        // Validate required field
        private void ValidateRequiredString(string name, string propertyKey)
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
