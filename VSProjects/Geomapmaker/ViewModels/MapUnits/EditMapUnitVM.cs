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
    public class EditMapUnitVM : DockPane, INotifyDataErrorInfo
    {
        // Edits's save button
        public ICommand CommandUpdate { get; }
        public ICommand CommandReset { get; }

        public EditMapUnitVM()
        {
            // Init commands
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        public ObservableCollection<MapUnit> AllMapUnits => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnitData.MapUnits);

        private MapUnit _selectedMapUnit;
        public MapUnit SelectedMapUnit
        {
            get => _selectedMapUnit;
            set
            {
                SetProperty(ref _selectedMapUnit, value, () => SelectedMapUnit);

                if (SelectedMapUnit != null)
                {
                    MapUnit = SelectedMapUnit.MU;
                    Name = SelectedMapUnit.Name;
                    FullName = SelectedMapUnit.FullName;
                    OlderInterval = GetOlderIntervalFromAge(SelectedMapUnit.Age);
                    YoungerInterval = GetYoungerIntervalFromAge(SelectedMapUnit.Age);
                    RelativeAge = SelectedMapUnit.RelativeAge;
                    Description = SelectedMapUnit.Description;
                    Label = SelectedMapUnit.Label;
                    HexColor = SelectedMapUnit.Hexcolor;
                    GeoMaterial = SelectedMapUnit.GeoMaterial;
                    GeoMaterialConfidence = SelectedMapUnit.GeoMaterialConfidence;
                }
            }
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
                ValidateChangeWasMade();
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
                ValidateChangeWasMade();
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
                ValidateChangeWasMade();
            }
        }

        public ObservableCollection<Interval> OlderIntervalOptions { get; set; } = Data.Intervals.IntervalOptions;

        private Interval GetOlderIntervalFromAge(string age)
        {
            // TODO: Convert other possible formats such as "Name to Name", "Name - Name", "Name", etc

            if (string.IsNullOrEmpty(age) || !age.Contains('-'))
            {
                return null;
            }

            string olderName = age.Substring(0, age.IndexOf('-')).Trim();

            return OlderIntervalOptions.FirstOrDefault(a => a.Name == olderName);
        }

        // Older Interval
        private Interval _olderInterval;
        public Interval OlderInterval
        {
            get => _olderInterval;
            set
            {
                SetProperty(ref _olderInterval, value, () => OlderInterval);
                ValidateIntervals(YoungerInterval, OlderInterval);
                ValidateChangeWasMade();
            }
        }

        public ObservableCollection<Interval> YoungerIntervalOptions { get; set; } = Data.Intervals.IntervalOptions;

        private Interval GetYoungerIntervalFromAge(string age)
        {
            if (string.IsNullOrEmpty(age) || !age.Contains('-'))
            {
                return null;
            }

            string youngerName = age.Substring(age.IndexOf('-') + 1).Trim();

            return YoungerIntervalOptions.FirstOrDefault(a => a.Name == youngerName);
        }

        // Younger Interval
        private Interval _youngerInterval;
        public Interval YoungerInterval
        {
            get => _youngerInterval;
            set
            {
                SetProperty(ref _youngerInterval, value, () => YoungerInterval);
                ValidateIntervals(YoungerInterval, OlderInterval);
                ValidateChangeWasMade();
            }
        }

        // Convert the two interval names into a string
        public string Age => $"{OlderInterval?.Name}-{YoungerInterval?.Name}";

        // Relative Age
        private string _relativeAge;
        public string RelativeAge
        {
            get => _relativeAge;
            set
            {
                SetProperty(ref _relativeAge, value, () => RelativeAge);
                ValidateChangeWasMade();
            }
        }

        // Description
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value, () => Description);
                ValidateChangeWasMade();
            }
        }

        // Label
        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                SetProperty(ref _label, value, () => Label);
                ValidateChangeWasMade();
            }
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
                ValidateChangeWasMade();
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
                ValidateChangeWasMade();
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
                ValidateChangeWasMade();
            }
        }

        private bool CanUpdate()
        {
            return !HasErrors;
        }

        private async Task ResetAsync()
        {
            // Refresh map unit data
            await Data.DescriptionOfMapUnitData.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllMapUnits");

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
            SelectedMapUnit = null;
        }

        private async Task UpdateAsync()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(Data.DbConnectionProperties.GetProperties()))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits"))
                    {

                        editOperation.Callback(context =>
                        {
                            QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + SelectedMapUnit.ID };

                            using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                            {

                                while (rowCursor.MoveNext())
                                { //TODO: Anything? Should be only one
                                    using (Row row = rowCursor.Current)
                                    {
                                        // In order to update the Map and/or the attribute table.
                                        // Has to be called before any changes are made to the row.
                                        context.Invalidate(row);

                                        row["MapUnit"] = MapUnit;
                                        row["Name"] = Name;
                                        row["FullName"] = FullName;
                                        row["Age"] = Age;
                                        row["RelativeAge"] = RelativeAge;
                                        row["Description"] = Description;
                                        row["Label"] = Label;
                                        row["AreaFillRGB"] = AreaFillRGB;
                                        row["HexColor"] = HexColor;
                                        row["GeoMaterial"] = GeoMaterial;
                                        row["GeoMaterialConfidence"] = GeoMaterialConfidence;
                                        row["ParagraphStyle"] = "Standard";
                                        row["DescriptionSourceID"] = DataHelper.DataSource.DataSource_ID;

                                        // After all the changes are done, persist it.
                                        row.Store();

                                        // Has to be called after the store too.
                                        context.Invalidate(row);
                                    }
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

        private void ValidateChangeWasMade()
        {
            // Error message isn't display on a field. Just prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (SelectedMapUnit == null)
            {
                _validationErrors.Remove(propertyKey);
                return;
            }

            if (SelectedMapUnit.MU == MapUnit &&
                SelectedMapUnit.Name == Name &&
                SelectedMapUnit.FullName == FullName &&
                SelectedMapUnit.Age == Age &&
                SelectedMapUnit.RelativeAge == RelativeAge &&
                SelectedMapUnit.Description == Description &&
                SelectedMapUnit.Label == Label &&
                SelectedMapUnit.Hexcolor == HexColor &&
                SelectedMapUnit.GeoMaterial == GeoMaterial &&
                SelectedMapUnit.GeoMaterialConfidence == GeoMaterialConfidence
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

        private void ValidateMapUnit(string mapUnit, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(mapUnit))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            // Name must be unique 
            else if (Data.DescriptionOfMapUnitData.AllDescriptionOfMapUnits.Where(a => a.ID != SelectedMapUnit?.ID).Any(a => a.MU?.ToLower() == MapUnit?.ToLower()))
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
            // Color must be unique 
            else if (Data.DescriptionOfMapUnitData.AllDescriptionOfMapUnits.Where(a => a.ID != SelectedMapUnit?.ID).Any(a => a.Hexcolor == color))
            {
                _validationErrors[propertyKey] = new List<string>() { "Color is taken." };
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