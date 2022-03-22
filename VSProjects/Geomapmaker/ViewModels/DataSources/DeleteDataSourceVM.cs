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
    public class DeleteDataSourceVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandDelete => new RelayCommand(() => DeleteAsync(), () => CanDelete());

        public DeleteDataSourceVM(DataSourcesViewModel parentVM)
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
                NotifyPropertyChanged("Id");

                Source = Selected?.Source;
                NotifyPropertyChanged("Source");

                Url = Selected?.Url;
                NotifyPropertyChanged("Url");

                Notes = Selected?.Notes;
                NotifyPropertyChanged("Notes");

                NotifyPropertyChanged("Visibility");

                ValidateCanDelete();
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        public string Id { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }

        private bool CanDelete()
        {
            return Selected != null && !HasErrors;
        }

        private async void DeleteAsync()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete {Id}?", $"Delete {Id}?", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

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
                        QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + Selected.ObjectId };

                        using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    context.Invalidate(row);

                                    row.Delete();
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

        private async void ValidateCanDelete()
        {
            const string propertyKey = "Selected";

            if (Selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
                RaiseErrorsChanged(propertyKey);
                return;
            }

            // Remove any existing errors
            _validationErrors.Remove(propertyKey);

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            await QueuedTask.Run(() =>
            {
                Table dmuTable = dmu.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    WhereClause = $"DescriptionSourceID = '{Id}'"
                };

                int rowCount = dmuTable.GetCount(queryFilter);

                if (rowCount > 0)
                {
                    _validationErrors[propertyKey] = new List<string>() { $"{rowCount} DescriptionOfMapUnits with this DescriptionSourceID" };
                }
            });

            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
