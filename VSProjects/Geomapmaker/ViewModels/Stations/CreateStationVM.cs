using ArcGIS.Core.Geometry;
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

namespace Geomapmaker.ViewModels.Stations
{
    public class CreateStationVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        public StationsViewModel ParentVM { get; set; }

        public CreateStationVM(StationsViewModel parentVM)
        {
            ParentVM = parentVM;

            // Get the wkid for the current Spatial Reference
            string currentWkid = MapView.Active?.Map.SpatialReference.Wkid.ToString();

            // Set as the current spaital ref
            SpatialReferenceWkid = currentWkid;

            // Trigger validation
            FieldID = "";
            XCoordinate = "";
            YCoordinate = "";
            LocationConfidenceMeters = "";
            PlotAtScale = "0";
        }

        private string _fieldID;
        public string FieldID
        {
            get => _fieldID;
            set
            {
                SetProperty(ref _fieldID, value, () => FieldID);
                ValidateFieldID(FieldID, "FieldID");
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
            }
        }
        private double YCoordinateDouble;

        private string _timeDate;
        public string TimeDate
        {
            get => _timeDate;
            set => SetProperty(ref _timeDate, value, () => TimeDate);
        }

        private string _observer;
        public string Observer
        {
            get => _observer;
            set => SetProperty(ref _observer, value, () => Observer);
        }

        private string _locationMethod;
        public string LocationMethod
        {
            get => _locationMethod;
            set => SetProperty(ref _locationMethod, value, () => LocationMethod);
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredString(LocationConfidenceMeters, "LocationConfidenceMeters");
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
            }
        }
        // Holds the converted int value 
        private int PlotAtScaleInt;

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value, () => Notes);
        }

        private string _dataSourceId = GeomapmakerModule.DataSourceId;
        public string DataSourceId
        {
            get => _dataSourceId;
            set => SetProperty(ref _dataSourceId, value, () => DataSourceId);
        }

        private bool CanSave()
        {
            return !HasErrors;
        }

        private async void SaveAsync()
        {
            bool IsSucceeded = false;

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                MessageBox.Show("Stations layer not found in active map.");
                return;
            }

            await QueuedTask.Run(async () =>
            {
                EditOperation createFeatures = new EditOperation
                {
                    Name = "Create Station",
                    SelectNewFeatures = true
                };

                MapPointBuilder pointBuilder = new MapPointBuilder(XCoordinateDouble, YCoordinateDouble, StationSpatialRef);

                // Get geometry from point builder
                Geometry point = pointBuilder.ToGeometry();

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

                RowToken token = createFeatures.CreateEx(stationsLayer, point, attributes);

                // Execute to execute the operation
                createFeatures.Execute();

                IsSucceeded = createFeatures.IsSucceeded;

                if (IsSucceeded)
                {
                    MapView.Active?.ZoomTo(point);
                }

                //
                // Validate X, Y Coordinates by checking if a feature was actually created at that geometry point. 
                // I haven't found a way to check prior to executing the editOperation
                //
                if ((bool)!MapView.Active?.GetFeatures(point).ContainsKey(stationsLayer))
                {
                    IsSucceeded = false;

                    // Undo the edit operation
                    await createFeatures.UndoAsync();
                }

            });

            if (IsSucceeded)
            {
                ParentVM.CloseProwindow();
            }
            else
            {
                // Raise error
                _validationErrors["SpatialReferenceWkid"] = new List<string>() { "Coordinates not valid for Spatial Reference." };
                RaiseErrorsChanged("SpatialReferenceWkid");
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


        private void ValidateFieldID(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.StationFieldIdOptions.Any(a => a.ToLower() == FieldID.ToLower()))
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
                _validationErrors[propertyKey] = new List<string>() { "Scale must be a postive integer." };
            }
            else if (PlotAtScaleInt < 0)
            {
                _validationErrors[propertyKey] = new List<string>() { "Scale must be a postive integer." };
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
