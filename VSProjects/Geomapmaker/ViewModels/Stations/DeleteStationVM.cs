using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
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

namespace Geomapmaker.ViewModels.Stations
{
    public class DeleteStationVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandDelete => new RelayCommand(() => DeleteAsync(), () => CanDelete());

        public StationsViewModel ParentVM { get; set; }

        public DeleteStationVM(StationsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        private Station _selected;
        public Station Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                FieldID = Selected?.FieldID;
                NotifyPropertyChanged("FieldID");

                SpatialReferenceWkid = Selected?.SpatialReferenceWkid;
                NotifyPropertyChanged("SpatialReferenceWkid");

                XCoordinate = Selected?.XCoordinate;
                NotifyPropertyChanged("XCoordinate");

                YCoordinate = Selected?.YCoordinate;
                NotifyPropertyChanged("YCoordinate");

                TimeDate = Selected?.TimeDate;
                NotifyPropertyChanged("TimeDate");

                Observer = Selected?.Observer;
                NotifyPropertyChanged("Observer");

                LocationMethod = Selected?.LocationMethod;
                NotifyPropertyChanged("LocationMethod");

                LocationConfidenceMeters = Selected?.LocationConfidenceMeters;
                NotifyPropertyChanged("LocationConfidenceMeters");

                PlotAtScale = Selected?.PlotAtScale;
                NotifyPropertyChanged("PlotAtScale");

                Notes = Selected?.Notes;
                NotifyPropertyChanged("Notes");

                DataSourceId = Selected?.DataSourceId;
                NotifyPropertyChanged("DataSourceId");

                NotifyPropertyChanged("Visibility");

                ValidateCanDelete();
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        public string FieldID { get; set; }
        public string SpatialReferenceWkid { get; set; }
        public string SpatialReferenceName { get; set; }
        public string XCoordinate { get; set; }
        public string YCoordinate { get; set; }
        public string TimeDate { get; set; }
        public string Observer { get; set; }
        public string LocationMethod { get; set; }
        public string LocationConfidenceMeters { get; set; }
        public string PlotAtScale { get; set; }
        public string Notes { get; set; }
        public string DataSourceId { get; set; }

        private bool CanDelete()
        {
            return Selected != null && !HasErrors;
        }

        private async void DeleteAsync()
        {
            MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Are you sure you want to delete {FieldID}?", $"Delete {FieldID}?", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string errorMessage = null;

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Stations layer not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table enterpriseTable = stationsLayer.GetTable())
                    {
                        if (enterpriseTable != null)
                        {
                            EditOperation editOperation = new EditOperation()
                            {
                                Name = "Delete Station"
                            };

                            editOperation.Callback(context =>
                            {

                                QueryFilter filter = new QueryFilter { ObjectIDs = new List<long> { Selected.ObjectID } };

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

            if (Selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
                RaiseErrorsChanged(propertyKey);
                return;
            }

            FeatureLayer op = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            await QueuedTask.Run(() =>
            {
                using (Table OrientationPointsTable = op.GetTable())
                {
                    if (OrientationPointsTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = $"StationsID = '{FieldID}'"
                        };

                        int rowCount = OrientationPointsTable.GetCount(queryFilter);

                        if (rowCount > 0)
                        {
                            string pural = rowCount == 1 ? "" : "s";
                            _validationErrors[propertyKey] = new List<string>() { $"{rowCount} OrientationPoint{pural} with this StationsID" };
                        }
                        else
                        {
                            _validationErrors.Remove(propertyKey);
                        }
                    }
                }
            });

            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
