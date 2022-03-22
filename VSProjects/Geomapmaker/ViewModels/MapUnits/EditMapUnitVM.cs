using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using Geomapmaker.RibbonElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class EditMapUnitVM : DockPane, INotifyDataErrorInfo
    {
        // Edits's save button 
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        public MapUnitsViewModel ParentVM { get; set; }

        public EditMapUnitVM(MapUnitsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                MapUnit = Selected?.MU;
                Name = Selected?.Name;
                FullName = Selected?.FullName;
                OlderInterval = GetOlderIntervalFromAge(Selected?.Age);
                YoungerInterval = GetYoungerIntervalFromAge(Selected?.Age);
                RelativeAge = Selected?.RelativeAge;
                Description = Selected?.Description;
                Label = Selected?.Label;
                Color = _helpers.ColorConverter.RGBtoColor(Selected?.AreaFillRGB);
                GeoMaterial = Selected?.GeoMaterial;
                GeoMaterialConfidence = Selected?.GeoMaterialConfidence;
                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

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
            // TODO: Convert other possible formats such as "Name to Name", "Name - Name", "Name", etc

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
        private Color? color;
        public Color? Color
        {
            get => color;
            set
            {
                SetProperty(ref color, value, () => Color);
                ValidateColor(Color, "Color");
                ValidateChangeWasMade();
            }
        }

        public string AreaFillRGB => _helpers.ColorConverter.ColorToRGB(Color);

        public string HexColor => Color?.ToString();

        public ObservableCollection<Geomaterial> GeoMaterialOptions { get; set; } = Data.GeoMaterials.GeoMaterialOptions;

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
            return Selected != null && !HasErrors;
        }

        private async Task UpdateAsync()
        {
            string errorMessage = null;

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu == null)
            {
                MessageBox.Show("DescriptionOfMapUnits table not found in active map.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    Table enterpriseTable = dmu.GetTable();

                    EditOperation editOperation = new EditOperation();

                    editOperation.Callback(context =>
                    {
                        QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + Selected.ID };

                        using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
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
                                    row["DescriptionSourceID"] = GeomapmakerModule.DataSourceId;

                                    // After all the changes are done, persist it.
                                    row.Store();

                                    // Has to be called after the store too.
                                    context.Invalidate(row);
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
                // Update the MapUnit value in MUPS if it has changed
                if (Selected.MU != MapUnit)
                {

                    await QueuedTask.Run(() =>
                    {
                        FeatureLayer mup = MapView.Active?.Map.FindLayers("MapUnitPolys").FirstOrDefault() as FeatureLayer;

                        // Search by attribute
                        QueryFilter queryFilter = new QueryFilter
                        {
                            // Where MapUnit is set to the original MapUnit value
                            WhereClause = $"mapunit = '{Selected.MU}'"
                        };

                        //Create list of oids to update
                        List<long> oidSet = new List<long>();

                        using (RowCursor rc = mup.Search(queryFilter))
                        {
                            while (rc.MoveNext())
                            {
                                using (Row record = rc.Current)
                                {
                                    oidSet.Add(record.GetObjectID());
                                }
                            }
                        }

                        //create and execute the edit operation
                        EditOperation modifyFeatures = new EditOperation
                        {
                            Name = "Update MapUnitPolys",
                            ShowProgressor = true
                        };

                        Inspector multipleFeaturesInsp = new Inspector();

                        multipleFeaturesInsp.Load(mup, oidSet);

                        multipleFeaturesInsp["mapunit"] = MapUnit;

                        modifyFeatures.Modify(multipleFeaturesInsp);

                        modifyFeatures.Execute();
                    });
                }

                // Add new symbology/templates if needed. Remove old symbology/templates if needed.
                Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();

                ParentVM.CloseProwindow();
            }
        }

        #region ### Validation ####

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
            // Silent error message
            // Just prevents update until a map unit is selected and a change is made.
            const string propertyKey = "SilentError";

            if (Selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "Select a map unit to edit." };
                return;
            }

            if (Selected.MU == MapUnit &&
                Selected.Name == Name &&
                Selected.FullName == FullName &&
                Selected.Age == Age &&
                Selected.RelativeAge == RelativeAge &&
                Selected.Description == Description &&
                Selected.Label == Label &&
                Selected.AreaFillRGB == AreaFillRGB &&
                Selected.GeoMaterial == GeoMaterial &&
                Selected.GeoMaterialConfidence == GeoMaterialConfidence
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
            // Alphabet chars only
            else if (!mapUnit.All(Char.IsLetter))
            {
                _validationErrors[propertyKey] = new List<string>() { "Alphabet characters only." };
            }
            // Name must be unique 
            else if (ParentVM.MapUnits.Where(a => a.ID != Selected?.ID).Any(a => a.MU?.ToLower() == MapUnit?.ToLower()))
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
            // Alphabet chars only
            else if (!name.All(char.IsLetter))
            {
                _validationErrors[propertyKey] = new List<string>() { "Alphabetical letters only." };
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
            // Full Name must be unique 
            else if (ParentVM.MapUnits.Where(a => a.ID != Selected?.ID).Any(a => a.FullName?.ToLower() == FullName?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Full name is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateIntervals(Interval youngerInterval, Interval olderInterval)
        {
            if (olderInterval == null)
            {
                _validationErrors["OlderInterval"] = new List<string>() { "" };
            }
            if (youngerInterval == null)
            {
                _validationErrors["YoungerInterval"] = new List<string>() { "" };
            }
            if (youngerInterval != null && olderInterval != null)
            {
                _validationErrors.Remove("OlderInterval");
                _validationErrors.Remove("YoungerInterval");

                if (youngerInterval.Early_Age > olderInterval.Early_Age)
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

        private void ValidateColor(Color? color, string propertyKey)
        {
            // Required field
            if (color == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            // Color must be unique 
            else if (ParentVM.MapUnits.Where(a => a.ID != Selected?.ID).Any(a => a.AreaFillRGB == AreaFillRGB))
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
            else if (geoMaterial == "Other materials")
            {
                _validationErrors[propertyKey] = new List<string>() { "Please select a subdivision" };
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
