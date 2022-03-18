using ArcGIS.Core.Geometry;
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
using System.Windows.Input;

namespace Geomapmaker.ViewModels.OrientationPoints
{
    public class CreateOrientationPointVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        public OrientationPointsViewModel ParentVM { get; set; }

        public CreateOrientationPointVM(OrientationPointsViewModel parentVM)
        {
            ParentVM = parentVM;

            // Get the wkid for the current Spatial Reference
            string currentWkid = MapView.Active?.Map.SpatialReference.Wkid.ToString();

            // Set as the current spaital ref
            SpatialReferenceWkid = currentWkid;

            // Trigger validation
            Type = "";
            Symbol = null;
            XCoordinate = "";
            YCoordinate = "";
            PlotAtScale = "0";
            LocationConfidenceMeters = "";
        }

        private bool CanSave()
        {
            return !HasErrors;
        }

        private async void SaveAsync()
        {
            bool IsSucceeded = false;

            FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            if (opLayer == null)
            {
                MessageBox.Show("OrientationPoints layer not found in active map.");
                return;
            }

            await QueuedTask.Run(async () =>
            {
                EditOperation createFeatures = new EditOperation
                {
                    Name = "Create Orientation Point",
                    SelectNewFeatures = true
                };

                MapPointBuilder pointBuilder = new MapPointBuilder(XCoordinateDouble, YCoordinateDouble, StationSpatialRef);

                // Get geometry from point builder
                Geometry point = pointBuilder.ToGeometry();

                // Create features and set attributes
                Dictionary<string, object> attributes = new Dictionary<string, object>
                {
                    { "StationsID", StationFieldId },
                    { "Type", Type },
                    { "Symbol", Symbol.Key },
                    { "LocationConfidenceMeters", LocationConfidenceMeters },
                    { "PlotAtScale", PlotAtScale },
                    { "Notes", Notes },
                };

                RowToken token = createFeatures.CreateEx(opLayer, point, attributes);

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
                if ((bool)!MapView.Active?.GetFeatures(point).ContainsKey(opLayer))
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

        private string _stationFieldId;
        public string StationFieldId
        {
            get => _stationFieldId;
            set
            {
                SetProperty(ref _stationFieldId, value, () => StationFieldId);
            }
        }

        private GemsSymbol _symbol;
        public GemsSymbol Symbol
        {
            get => _symbol;
            set
            {
                SetProperty(ref _symbol, value, () => Symbol);
                ValidateSymbol(Symbol, "Symbol");
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

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value, () => Type);
                ValidateRequiredString(Type, "Type");
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

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                SetProperty(ref _notes, value, () => Notes);
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

        // Validate symbol
        private void ValidateSymbol(GemsSymbol symbol, string propertyKey)
        {
            // Required field
            if (symbol == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
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
