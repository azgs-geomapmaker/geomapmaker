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
using System.Windows;

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
            MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Are you sure you want to delete {Id}?", $"Delete {Id}?", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string errorMessage = null;

            StandaloneTable ds = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (ds == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("DataSources table not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table table = ds.GetTable())
                    {
                        if (table != null)
                        {
                            EditOperation editOperation = new EditOperation()
                            {
                                Name = "Delete DataSource",
                            };

                            editOperation.Callback(context =>
                            {
                                QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + Selected.ObjectId };

                                using (RowCursor rowCursor = table.Search(filter, false))
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
                            }, table);

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
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(errorMessage, "One or more errors occured.");
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
            bool isError = false;

            if (Selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
                RaiseErrorsChanged(propertyKey);
                return;
            }

            // Remove any existing errors
            _validationErrors.Remove(propertyKey);
            RaiseErrorsChanged(propertyKey);

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = dmu.GetTable())
                    {
                        if (table == null)
                        {
                            return;
                        }

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"DescriptionSourceID = '{Id}'"
                        };

                        long rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} DescriptionOfMapUnit{plural} with this DescriptionSourceID" };
                            RaiseErrorsChanged(propertyKey);

                        }
                    }
                });

                // Stop checking if we've found an error
                if (isError) return;
            }

            FeatureLayer cf = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

            if (cf != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = cf.GetTable())
                    {
                        if (table == null)
                        {
                            return;
                        }

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"DataSourceID = '{Id}'"
                        };

                        long rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} ContactsAndFault{plural} with this DataSourceID" };
                            RaiseErrorsChanged(propertyKey);
                        }
                    }
                });

                // Stop checking if we've found an error
                if (isError) return;
            }

            FeatureLayer mup = MapView.Active?.Map.FindLayers("MapUnitPolys").FirstOrDefault() as FeatureLayer;

            if (mup != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = mup.GetTable())
                    {
                        if (table == null)
                        {
                            return;
                        }

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"DataSourceID = '{Id}'"
                        };

                        long rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} MapUnitPoly{plural} with this DataSourceID" };
                            RaiseErrorsChanged(propertyKey);
                        }
                    }
                });

                // Stop checking if we've found an error
                if (isError) return;
            }

            FeatureLayer station = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (station != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = station.GetTable())
                    {
                        if (table == null)
                        {
                            return;
                        }

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"DataSourceID = '{Id}'"
                        };

                        long rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} Station{plural} with this DataSourceID" };
                            RaiseErrorsChanged(propertyKey);
                        }
                    }
                });

                // Stop checking if we've found an error
                if (isError) return;
            }

            FeatureLayer op = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            if (op != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = op.GetTable())
                    {
                        if (table == null)
                        {
                            return;
                        }

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"LocationSourceID = '{Id}'"
                        };

                        long rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} Orientation Point{plural} with this LocationSourceID" };
                            RaiseErrorsChanged(propertyKey);
                        }

                        // Stop checking if we've found an error
                        if (isError) return;

                        queryFilter = new QueryFilter
                        {
                            WhereClause = $"OrientationSourceID = '{Id}'"
                        };

                        rowCount = table.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            isError = true;
                            string plural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} Orientation Point{plural} with this OrientationSourceID" };
                            RaiseErrorsChanged(propertyKey);
                        }
                    }
                });
            }
        }

        #endregion
    }
}
