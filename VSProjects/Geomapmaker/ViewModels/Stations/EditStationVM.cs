using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
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

namespace Geomapmaker.ViewModels.Stations
{
    public class EditStationVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        public StationsViewModel ParentVM { get; set; }

        public EditStationVM(StationsViewModel parentVM)
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
                SpatialReferenceWkid = Selected?.SpatialReferenceWkid;
                XCoordinate = Selected?.XCoordinate;
                YCoordinate = Selected?.YCoordinate;
                TimeDate = Selected?.TimeDate;
                Observer = Selected?.Observer;
                LocationMethod = Selected?.LocationMethod;
                LocationConfidenceMeters = Selected?.LocationConfidenceMeters;
                PlotAtScale = Selected?.PlotAtScale;
                Notes = Selected?.Notes;
                DataSourceId = Selected?.DataSourceId;

                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private string _fieldID;
        public string FieldID
        {
            get => _fieldID;
            set
            {
                SetProperty(ref _fieldID, value, () => FieldID);
                ValidateFieldID(FieldID, "FieldID");
                ValidateChangeWasMade();
            }
        }

        private string _spatialReferenceWkid;
        public string SpatialReferenceWkid
        {
            get => _spatialReferenceWkid;
            set
            {
                SetProperty(ref _spatialReferenceWkid, value, () => SpatialReferenceWkid);
                ValidateWkid(SpatialReferenceWkid, "SpatialReferenceWkid");
                NotifyPropertyChanged("SpatialReferenceName");
                ValidateChangeWasMade();
            }
        }
        private SpatialReference StationSpatialRef;

        public string SpatialReferenceName => StationSpatialRef == null ? "" : StationSpatialRef.Name;

        private string _xCoordinate;
        public string XCoordinate
        {
            get => _xCoordinate;
            set
            {
                SetProperty(ref _xCoordinate, value, () => XCoordinate);
                ValidateXCoordinate(XCoordinate, "XCoordinate");
                ValidateChangeWasMade();
            }
        }
        private double XCoordinateDouble;

        private string _yCoordinate;
        public string YCoordinate
        {
            get => _yCoordinate;
            set
            {
                SetProperty(ref _yCoordinate, value, () => YCoordinate);
                ValidateYCoordinate(YCoordinate, "YCoordinate");
                ValidateChangeWasMade();
            }
        }
        private double YCoordinateDouble;

        private string _timeDate;
        public string TimeDate
        {
            get => _timeDate;
            set
            {
                SetProperty(ref _timeDate, value, () => TimeDate);
                ValidateChangeWasMade();
            }
        }

        private string _observer;
        public string Observer
        {
            get => _observer;
            set
            {
                SetProperty(ref _observer, value, () => Observer);
                ValidateChangeWasMade();
            }
        }

        private string _locationMethod;
        public string LocationMethod
        {
            get => _locationMethod;
            set
            {
                SetProperty(ref _locationMethod, value, () => LocationMethod);
                ValidateChangeWasMade();
            }
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                // Remove " (Unknown)" from the -9 option
                value = value?.Replace(" (Unknown)", "");

                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredNumber(LocationConfidenceMeters, "LocationConfidenceMeters");
                ValidateChangeWasMade();
            }
        }

        private string _plotAtScale;
        public string PlotAtScale
        {
            get => _plotAtScale;
            set
            {
                SetProperty(ref _plotAtScale, value, () => PlotAtScale);
                ValidatePlotAtScale(PlotAtScale, "PlotAtScale");
                ValidateChangeWasMade();
            }
        }
        private int PlotAtScaleInt;

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

        private string _dataSourceId;
        public string DataSourceId
        {
            get => _dataSourceId;
            set
            {
                SetProperty(ref _dataSourceId, value, () => DataSourceId);
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

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                MessageBox.Show("Stations layer not found in active map.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    EditOperation modifyFeature = new EditOperation
                    {
                        Name = "Station Edit",
                    };

                    MapPointBuilder pointBuilder = new MapPointBuilder(XCoordinateDouble, YCoordinateDouble, StationSpatialRef);

                    // Get geometry from point builder
                    Geometry point = pointBuilder.ToGeometry();

                    Inspector modifyInspector = new Inspector();

                    modifyInspector.Load(stationsLayer, Selected.ObjectID);

                    // Create features and set attributes
                    Dictionary<string, object> attributes = new Dictionary<string, object>
                {
                    { "FieldID", FieldID },
                    { "TimeDate", TimeDate },
                    { "Observer", Observer },
                    { "LocationMethod", LocationMethod },
                    { "LocationConfidenceMeters", LocationConfidenceMeters },
                    { "PlotAtScale", PlotAtScale },
                    { "Notes", Notes },
                    { "DataSourceId", DataSourceId },
                };

                    modifyFeature.Modify(stationsLayer, Selected.ObjectID, point, attributes);

                    // Execute to execute the operation
                    modifyFeature.Execute();

                    if (modifyFeature.IsSucceeded)
                    {
                        MapView.Active?.ZoomTo(point);
                    }
                    else
                    {
                        MessageBox.Show(modifyFeature.ErrorMessage, "One or more errors occured.");
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

            if (string.IsNullOrEmpty(errorMessage))
            {
                // Check if FieldID changed
                if (Selected.FieldID != FieldID)
                {
                    // Updated foreign key
                    await QueuedTask.Run(() =>
                    {
                        FeatureLayer op = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

                        // Search by attribute
                        QueryFilter queryFilter = new QueryFilter
                        {
                            // Where StationsID is set to the original StationsID value
                            WhereClause = $"StationsID = '{Selected.FieldID}'"
                        };

                        //Create list of oids to update
                        List<long> oidSet = new List<long>();

                        using (RowCursor rc = op.Search(queryFilter))
                        {
                            while (rc.MoveNext())
                            {
                                using (Row record = rc.Current)
                                {
                                    oidSet.Add(record.GetObjectID());
                                }
                            }
                        }

                        // Edit operation
                        EditOperation modifyFeatures = new EditOperation
                        {
                            Name = "Update OrientationPoints",
                            ShowProgressor = true
                        };

                        Inspector multipleFeaturesInsp = new Inspector();

                        multipleFeaturesInsp.Load(op, oidSet);

                        multipleFeaturesInsp["StationsID"] = FieldID;

                        modifyFeatures.Modify(multipleFeaturesInsp);

                        modifyFeatures.Execute();
                    });
                }

                ParentVM.CloseProwindow();
            }
            else
            {
                MessageBox.Show(errorMessage, "One or more errors occured.");
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

        private void ValidateChangeWasMade()
        {
            // Silent error message
            // Just prevents update until a map unit is selected and a change is made.
            const string propertyKey = "SilentError";

            if (Selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "Select a station unit to edit." };
                RaiseErrorsChanged(propertyKey);
                return;
            }

            if (Selected.FieldID == FieldID &&
                Selected.SpatialReferenceWkid == SpatialReferenceWkid &&
                Selected.XCoordinate == XCoordinate &&
                Selected.YCoordinate == YCoordinate &&
                Selected.TimeDate == TimeDate &&
                Selected.Observer == Observer &&
                Selected.LocationMethod == LocationMethod &&
                Selected.LocationConfidenceMeters == LocationConfidenceMeters &&
                Selected.PlotAtScale == PlotAtScale &&
                Selected.Notes == Notes &&
                Selected.DataSourceId == DataSourceId
                )
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateWkid(string text, string propertyKey)
        {
            if (!int.TryParse(text, out int WKIDint))
            {
                _validationErrors[propertyKey] = new List<string>() { "Invalid WKID." };
                StationSpatialRef = null;
                RaiseErrorsChanged(propertyKey);
                return;
            }

            // Try finding spatial ref by wkid
            try
            {
                StationSpatialRef = SpatialReferenceBuilder.CreateSpatialReference(WKIDint);
            }
            catch
            {
                // Spatial ref not found by wkid
                StationSpatialRef = null;
            }

            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (StationSpatialRef == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "Spatial Reference not found." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateRequiredNumber(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out _))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be numerical." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateFieldID(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.StationOptions.Any(a => a.ObjectID != Selected.ObjectID && a.FieldID.ToLower() == FieldID.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Field ID is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateXCoordinate(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out XCoordinateDouble))
            {
                _validationErrors[propertyKey] = new List<string>() { "Coordinate must be numerical." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateYCoordinate(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out YCoordinateDouble))
            {
                _validationErrors[propertyKey] = new List<string>() { "Coordinate must be numerical." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidatePlotAtScale(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!int.TryParse(text, out PlotAtScaleInt))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be a postive integer." };
            }
            else if (PlotAtScaleInt < 0)
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be a postive integer." };
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
