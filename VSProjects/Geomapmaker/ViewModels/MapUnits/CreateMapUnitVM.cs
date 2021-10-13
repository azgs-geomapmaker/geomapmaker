﻿using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class CreateMapUnitVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandSave { get; }
        public ICommand CommandReset { get; }

        public CreateMapUnitVM()
        {
            // Init submit command
            CommandSave = new RelayCommand(() => SubmitAsync(), () => CanSave());
            CommandReset = new RelayCommand(() => ResetAsync());

            // Initialize required values
            MapUnit = null;
            Name = null;
            FullName = null;
            Description = null;
            OlderInterval = null;
            YoungerInterval = null;
            HexColor = null;
            GeoMaterial = null;
            GeoMaterialConfidence = null;
        }

        // MapUnit
        private string _mapUnit;
        public string MapUnit
        {
            get => _mapUnit;
            set
            {
                SetProperty(ref _mapUnit, value, () => MapUnit);
                ValidateMapUnit(MapUnit, "MapUnit");
            }
        }

        // Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                ValidateName(Name, "Name");
            }
        }

        // Full Name
        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                SetProperty(ref _fullName, value, () => FullName);
                ValidateFullName(FullName, "FullName");
            }
        }

        public ObservableCollection<Interval> OlderIntervalOptions { get; set; } = Data.Intervals.IntervalOptions;

        // Older Interval
        private Interval _olderInterval;
        public Interval OlderInterval
        {
            get => _olderInterval;
            set
            {
                SetProperty(ref _olderInterval, value, () => OlderInterval);
                ValidateIntervals(YoungerInterval, OlderInterval);

                if (YoungerInterval == null)
                {
                    YoungerInterval = YoungerIntervalOptions.FirstOrDefault(a => a.Name == OlderInterval?.Name);
                }
            }
        }

        public ObservableCollection<Interval> YoungerIntervalOptions { get; set; } = Data.Intervals.IntervalOptions;

        // Younger Interval
        private Interval _youngerInterval;
        public Interval YoungerInterval
        {
            get => _youngerInterval;
            set
            {
                SetProperty(ref _youngerInterval, value, () => YoungerInterval);
                ValidateIntervals(YoungerInterval, OlderInterval);
            }
        }

        // Convert the two interval names into a string
        public string Age => $"{OlderInterval?.Name}-{YoungerInterval?.Name}";

        // Relative Age
        private string _relativeAge;
        public string RelativeAge
        {
            get => _relativeAge;
            set => SetProperty(ref _relativeAge, value, () => RelativeAge);
        }

        // Description
        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, () => Description);
        }

        // Label
        private string _label;
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, () => Label);
        }

        // Color
        private string _hexColor;
        public string HexColor
        {
            get => _hexColor;
            set
            {
                SetProperty(ref _hexColor, value, () => HexColor);
                ValidateColor(HexColor, "HexColor");
            }
        }

        public string AreaFillRGB => MapUnitsViewModel.HexToRGB(HexColor);

        public ObservableCollection<string> GeoMaterialOptions { get; set; } = Data.GeoMaterials.GeoMaterialOptions;

        // GeoMaterial
        private string _geoMaterial;
        public string GeoMaterial
        {
            get => _geoMaterial;
            set
            {
                SetProperty(ref _geoMaterial, value, () => GeoMaterial);
                ValidateGeoMaterial(GeoMaterial, "GeoMaterial");
            }
        }

        public ObservableCollection<string> GeoMaterialConfidenceOptions { get; set; } = Data.Confidence.ConfidenceOptions;

        // GeoMaterialConfidence
        private string _geoMaterialConfidence;
        public string GeoMaterialConfidence
        {
            get => _geoMaterialConfidence;
            set
            {
                SetProperty(ref _geoMaterialConfidence, value, () => GeoMaterialConfidence);
                ValidateGeoMaterialConfidence(GeoMaterialConfidence, "GeoMaterialConfidence");
            }
        }

        private async Task ResetAsync()
        {
            // Refresh map unit data
            await Data.DescriptionOfMapUnitData.RefreshMapUnitsAsync();
            NotifyPropertyChanged("ParentOptions");

            // Reset values
            MapUnit = null;
            Name = null;
            FullName = null;
            OlderInterval = null;
            YoungerInterval = null;
            RelativeAge = null;
            Description = null;
            Label = null;
            HexColor = null;
            GeoMaterial = null;
            GeoMaterialConfidence = null;
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSave()
        {
            return !HasErrors;
        }

        /// <summary>
        /// Execute the submit command
        /// </summary>
        private async Task SubmitAsync()
        {
            if (DataHelper.connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits"))
                    {

                        editOperation.Callback(context =>
                        {
                            TableDefinition tableDefinition = enterpriseTable.GetDefinition();
                            using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                            {
                                rowBuffer["MapUnit"] = MapUnit;
                                rowBuffer["Name"] = Name;
                                rowBuffer["FullName"] = FullName;
                                rowBuffer["Age"] = Age;
                                rowBuffer["RelativeAge"] = RelativeAge;
                                rowBuffer["Description"] = Description;
                                rowBuffer["Label"] = Label;
                                rowBuffer["AreaFillRGB"] = AreaFillRGB;
                                rowBuffer["HexColor"] = HexColor;
                                rowBuffer["GeoMaterial"] = GeoMaterial;
                                rowBuffer["GeoMaterialConfidence"] = GeoMaterialConfidence;
                                rowBuffer["ParagraphStyle"] = "Standard";
                                rowBuffer["DescriptionSourceID"] = DataHelper.DataSource.DataSource_ID;

                                using (Row row = enterpriseTable.CreateRow(rowBuffer))
                                {
                                    // To Indicate that the attribute table has to be updated.
                                    context.Invalidate(row);
                                }
                            }
                        }, enterpriseTable);

                        bool result = editOperation.Execute();

                        if (!result)
                        {
                            MessageBox.Show(editOperation.ErrorMessage);
                        }

                    }
                }
            });

            // Reset
            await ResetAsync();
        }

        // Validation
        #region INotifyDataErrorInfo members

        // Error collection
        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (System.Collections.IEnumerable)_validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        private void ValidateMapUnit(string mapUnit, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(mapUnit))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            // Name must be unique 
            else if (Data.DescriptionOfMapUnitData.AllDescriptionOfMapUnits.Any(a => a.MU?.ToLower() == MapUnit?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Map Unit is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);

        }

        // Validate the Heading's name
        private void ValidateName(string name, string propertyKey)
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

        // Validate the Heading's Full Name
        private void ValidateFullName(string fullName, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(fullName))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateIntervals(Interval younger, Interval older)
        {
            if (older == null)
            {
                _validationErrors["OlderInterval"] = new List<string>() { "" };
            }
            if (younger == null)
            {
                _validationErrors["YoungerInterval"] = new List<string>() { "" };
            }
            if (younger != null && older != null)
            {
                _validationErrors.Remove("OlderInterval");
                _validationErrors.Remove("YoungerInterval");

                if (younger.Early_Age < older.Early_Age)
                {
                    _validationErrors["OlderInterval"] = new List<string>() { "Swap these comboboxes!" };
                    _validationErrors["YoungerInterval"] = new List<string>() { "" };
                }
                else
                {
                    _validationErrors.Remove("OlderInterval");
                    _validationErrors.Remove("YoungerInterval");
                }
            }

            RaiseErrorsChanged("YoungerInterval");
            RaiseErrorsChanged("OlderInterval");
        }

        private void ValidateColor(string color, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(color))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateGeoMaterial(string geoMaterial, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(GeoMaterial))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateGeoMaterialConfidence(string confidence, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(confidence))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
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
