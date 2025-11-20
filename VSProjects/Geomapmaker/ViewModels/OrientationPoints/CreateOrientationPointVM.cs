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
            string currentWkid = MapView.Active?.Map?.SpatialReference?.Wkid.ToString();

            // Set as the current spaital ref
            SpatialReferenceWkid = currentWkid;

            // Trigger validation
            Azimuth = "";
            Inclination = "";
            Type = "";
            Label = "";
            Symbol = null;
            XCoordinate = "";
            YCoordinate = "";
            PlotAtScale = "0";
            LocationConfidenceMeters = "";
            OrientationConfidenceDegrees = "-9";
            IdentityConfidence = "";
        }

        public void PopulateCoordinates(MapPoint mp)
        {
            SpatialReferenceWkid = mp?.SpatialReference?.Wkid.ToString();
            XCoordinate = mp?.X.ToString(); ;
            YCoordinate = mp?.Y.ToString(); ;

            // Turn off the toggle button
            Populate = false;
        }

        public void PopulateAzimuth(double azimuth) {
            Azimuth = azimuth.ToString();

            // Turn off the toggle button
            PopulateAZ = false;
        }

        private bool _populateaz;
        public bool PopulateAZ {
            get => _populateaz;
            set {
                SetProperty(ref _populateaz, value, () => PopulateAZ);

                // if the toggle-btn is active
                if (value) {
                    SetProperty(ref _populate, false, () => Populate);
                    // Active the populate tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateAzimuth");
                    //FrameworkApplication.SetCurrentToolAsync("esri_editing_measureAngleTool");
                } else {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
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
                    SetProperty(ref _populateaz, false, () => PopulateAZ);
                    // Active the populate tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateOPCoordinate");
                } else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
        }

        private bool CanSave()
        {
            return !HasErrors;
        }

        private async void SaveAsync()
        {
            string errorMessage = null;

            FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            if (opLayer == null)
            {
                MessageBox.Show("OrientationPoints layer not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    EditOperation createFeatures = new EditOperation
                    {
                        Name = "Create Orientation Point",
                        SelectNewFeatures = true
                    };

                    MapPointBuilderEx pointBuilder = new MapPointBuilderEx(XCoordinateDouble, YCoordinateDouble, StationSpatialRef);

                    // Get geometry from point builder
                    Geometry point = pointBuilder.ToGeometry();

                    // Create features and set attributes
                    Dictionary<string, object> attributes = new Dictionary<string, object>
                    {
                        { "StationsID", StationFieldId },
                        { "Azimuth", Azimuth },
                        { "Inclination", Inclination },
                        { "Type", Type },
                        { "Label", Label },
                        { "Symbol", Symbol.Key },
                        { "LocationConfidenceMeters", LocationConfidenceMeters },
                        { "OrientationConfidenceDegrees", OrientationConfidenceDegrees },
                        { "PlotAtScale", PlotAtScale },
                        { "Notes", Notes },
                        { "LocationSourceID", LocationSourceID },
                        { "OrientationSourceID", OrientationSourceID },
                        { "IdentityConfidence", IdentityConfidence },
                    };

                    RowToken token = createFeatures.Create(opLayer, point, attributes);

                    // Execute to execute the operation
                    createFeatures.Execute();

                    if (createFeatures.IsSucceeded)
                    {
                        // Zoom into point
                        MapView.Active?.ZoomTo(point);
                    }
                    else
                    {
                        MessageBox.Show(createFeatures.ErrorMessage, "One or more errors occured.");
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
                //We have to rebuild symbology here in case the user chose a symbol that has not been used yet.
                Data.OrientationPoints.RebuildOrientationPointsSymbology();
                ParentVM.CloseProwindow();
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

        private string _azimuth;
        public string Azimuth
        {
            get => _azimuth;
            set
            {
                SetProperty(ref _azimuth, value, () => Azimuth);
                ValidateAzimuth(Azimuth, "Azimuth");
            }
        }

        // Holds the converted double value 
        private double AzimuthDouble;

        private string _inclination;
        public string Inclination
        {
            get => _inclination;
            set
            {
                SetProperty(ref _inclination, value, () => Inclination);
                ValidateInclination(Inclination, "Inclination");
            }
        }

        // Holds the converted double value 
        private double InclinationDouble;

        private string _keyFilter;
        public string KeyFilter
        {
            get => _keyFilter;
            set
            {
                SetProperty(ref _keyFilter, value, () => KeyFilter);
                FilterSymbols(KeyFilter, DescriptionFilter);
            }
        }

        private string _descriptionFilter;
        public string DescriptionFilter
        {
            get => _descriptionFilter;
            set
            {
                SetProperty(ref _descriptionFilter, value, () => DescriptionFilter);
                FilterSymbols(KeyFilter, DescriptionFilter);
            }
        }

        // Filter the Symbol options by key and description
        private void FilterSymbols(string keyFilter, string DescriptionFilter)
        {
            // Start with all the symbols from the parent vm
            List<GemsSymbol> filteredSymbols = ParentVM.SymbolOptions;

            // Filter by key
            if (!string.IsNullOrEmpty(keyFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Key.ToLower().StartsWith(keyFilter.ToLower())).ToList();
            }

            // Filter by description
            if (!string.IsNullOrEmpty(DescriptionFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Description != null && a.Description.ToLower().Contains(DescriptionFilter.ToLower())).ToList();
            }

            // Update options
            SymbolOptions = filteredSymbols;
        }

        private void UpdateFilteredMessage()
        {
            int totalSymbolsCount = ParentVM.SymbolOptions.Count();

            int filteredSymbolsCount = SymbolOptions.Count();

            string plural = totalSymbolsCount == 1 ? "" : "s";

            // Update messsage
            SymbolsFilteredMessage = totalSymbolsCount != filteredSymbolsCount ? $"{filteredSymbolsCount} of {totalSymbolsCount} symbol{plural}" : $"{totalSymbolsCount} symbol{plural}";
        }

        private string _symbolsFilteredMessage;
        public string SymbolsFilteredMessage
        {
            get => _symbolsFilteredMessage;
            set => SetProperty(ref _symbolsFilteredMessage, value, () => SymbolsFilteredMessage);
        }

        private List<GemsSymbol> _symbolOptions;
        public List<GemsSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set
            {
                SetProperty(ref _symbolOptions, value, () => SymbolOptions);
                UpdateFilteredMessage();
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

        private string _label;
        public string Label {
            get => _label;
            set {
                SetProperty(ref _label, value, () => Label);
                //ValidateRequiredString(Label, "Label");
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
                // Remove " (Unknown)" from the -9 option
                value = value?.Replace(" (Unknown)", "");

                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredNumber(LocationConfidenceMeters, "LocationConfidenceMeters");
            }
        }

        private string _orientationConfidenceDegrees;
        public string OrientationConfidenceDegrees
        {
            get => _orientationConfidenceDegrees;
            set
            {
                // Remove " (Unknown)" from the -9 option
                value = value?.Replace(" (Unknown)", "");

                SetProperty(ref _orientationConfidenceDegrees, value, () => OrientationConfidenceDegrees);
                ValidateOrientationConfidenceDegrees(OrientationConfidenceDegrees, "OrientationConfidenceDegrees");
            }
        }

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set
            {
                SetProperty(ref _identityConfidence, value, () => IdentityConfidence);
                ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
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

        private string _locationSourceID = GeomapmakerModule.DataSourceId;
        public string LocationSourceID
        {
            get => _locationSourceID;
            set => SetProperty(ref _locationSourceID, value, () => LocationSourceID);
        }

        private string _orientationSourceID = GeomapmakerModule.DataSourceId;
        public string OrientationSourceID
        {
            get => _orientationSourceID;
            set => SetProperty(ref _orientationSourceID, value, () => OrientationSourceID);
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

        private void ValidateAzimuth(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out AzimuthDouble))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be numerical." };
            }
            else if (AzimuthDouble < 0 || AzimuthDouble > 360)
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be between 0 and 360." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateInclination(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out InclinationDouble))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be numerical." };
            }
            else if (InclinationDouble < -90 || InclinationDouble > 90)
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be between -90 and 90." };
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

        private void ValidateOrientationConfidenceDegrees(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out double degreesDouble))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be numerical." };
            }
            else if (degreesDouble > 90)
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be less than 90." };
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
