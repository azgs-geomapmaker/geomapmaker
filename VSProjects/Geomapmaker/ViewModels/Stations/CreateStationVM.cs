using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            string currentWkid = MapView.Active?.Map?.SpatialReference?.Wkid.ToString();

            // Set as the current spaital ref
            SpatialReferenceWkid = currentWkid;

            // Trigger validation
            FieldID = "";
            XCoordinate = "";
            YCoordinate = "";
            LocationConfidenceMeters = "";
            PlotAtScale = "0";
        }

        public void PopulateCoordinates(MapPoint mp)
        {
            SpatialReferenceWkid = mp?.SpatialReference?.Wkid.ToString();
            XCoordinate = mp?.X.ToString(); ;
            YCoordinate = mp?.Y.ToString(); ;

            // Turn off the toggle button
            Populate = false;
        }

        private bool _populate;
        public bool Populate
        {
            get => _populate;
            set
            {
                SetProperty(ref _populate, value, () => Populate);

                // if the toggle-btn is active
                if (value)
                {
                    // Active the populate tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateStationCoordinate");
                }
                else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
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
                // Remove " (Unknown)" from the -9 option
                value = value?.Replace(" (Unknown)", "");

                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredNumber(LocationConfidenceMeters, "LocationConfidenceMeters");
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

        private string _label;
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, () => Label);
        }

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
            string errorMessage = null;

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                MessageBox.Show("Stations layer not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    EditOperation createFeatures = new EditOperation
                    {
                        Name = "Create Station",
                        SelectNewFeatures = true
                    };

                    MapPointBuilderEx pointBuilder = new MapPointBuilderEx(XCoordinateDouble, YCoordinateDouble, StationSpatialRef);

                    // Get geometry from point builder
                    Geometry point = pointBuilder.ToGeometry();

                    // Create features and set attributes
                    Dictionary<string, object> attributes = new Dictionary<string, object>
                   {
                        { "FieldID", FieldID },
                        { "TimeDate", TimeDate },
                        { "Observer", Observer },
                        { "LocationConfidenceMeters", LocationConfidenceMeters },
                        { "PlotAtScale", PlotAtScale },
                        { "Label", Label },
                        { "Notes", Notes },
                        { "DataSourceId", DataSourceId },
                   };

                    RowToken token = createFeatures.Create(stationsLayer, point, attributes);

                    // Execute to execute the operation
                    createFeatures.Execute();

                    if (createFeatures.IsSucceeded)
                    {
                        MapView.Active?.ZoomTo(point);
                    }
                    else
                    {
                        errorMessage = createFeatures.ErrorMessage;
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
            else if (ParentVM.StationOptions.Any(a => a.FieldID.ToLower() == FieldID.ToLower()))
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
