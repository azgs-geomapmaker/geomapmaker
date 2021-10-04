using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Geomapmaker.ViewModels
{
    public class DMUCreateVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandSave { get; }
        public ICommand CommandReset { get; }

        public DMUCreateVM()
        {
            // Init submit command
            CommandSave = new RelayCommand(() => Submit(), () => CanSave());
            CommandReset = new RelayCommand(() => Reset());
        }

        // Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                ValidateHeadingName(_name);
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
                ValidateFullName(FullName);
            }
        }

        public ObservableCollection<Interval> OlderIntervalOptions { get; set; } = Data.Intervals.IntervalCollection;

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
                    YoungerInterval = YoungerIntervalOptions.FirstOrDefault(a => a.Name == OlderInterval.Name);
                }
            }
        }

        public ObservableCollection<Interval> YoungerIntervalOptions { get; set; } = Data.Intervals.IntervalCollection;

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
        public string Age => $"{OlderInterval?.Name} - {YoungerInterval?.Name}";

        // Relative Age
        private string _relativeAge;
        public string RelativeAge
        {
            get => _relativeAge;
            set
            {
                SetProperty(ref _relativeAge, value, () => RelativeAge);
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
            }
        }

        // Heading parent Id
        private int? _parent;
        public int? Parent
        {
            get => _parent;
            set
            {
                SetProperty(ref _parent, value, () => Parent);
                ValidateParent(Parent);
                NotifyPropertyChanged("RankingOptions");
            }
        }

        /// <summary>
        /// List of parent-options available during create
        /// </summary>
        public ObservableCollection<KeyValuePair<int?, string>> ParentOptions
        {
            get
            {
                // Get Headings/Subheadings from map units.
                // Sort by name
                // Create a int/string kvp for the combobox
                List<KeyValuePair<int?, string>> headingList = Data.DescriptionOfMapUnitData.Headings
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                    .ToList();

                // Initialize a ObservableCollection with the list
                // Note: Casting a list as an OC does not working.
                return new ObservableCollection<KeyValuePair<int?, string>>(headingList);
            }
        }

        public ObservableCollection<KeyValuePair<int?, string>> RankingOptions
        {
            get
            {
                if (Parent == null)
                {
                    return null;
                }

                // Get Headings/Subheadings from map units.
                // Sort by name
                // Create a int/string kvp for the combobox
                //List<KeyValuePair<int?, string>> headingList = Data.DescriptionOfMapUnitData.Headings
                //    .Where(a => a.ParentId == Parent)
                //    .OrderBy(a => a.Name)
                //    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                //    .ToList();


                // SEEDING MOCK DATA
                Random rnd = new Random();
                List<KeyValuePair<int?, string>> headingList = new List<KeyValuePair<int?, string>>
                {
                    new KeyValuePair<int?, string>( 1, "Map Unit " + rnd.Next(1, 2) ),
                    new KeyValuePair<int?, string>( 2, "Map Unit " + rnd.Next(3, 4) ),
                    new KeyValuePair<int?, string>( 3, "Map Unit " + rnd.Next(5, 8) ),
                    new KeyValuePair<int?, string>( 4, "Map Unit " + rnd.Next(8, 10) ),
                };

                headingList.Add(new KeyValuePair<int?, string>(5, "(This Map Unit)"));

                // Initialize a ObservableCollection with the list
                // Note: Casting a list as an OC does not working.
                return new ObservableCollection<KeyValuePair<int?, string>>(headingList);
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
            }
        }

        // Color
        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                SetProperty(ref _color, value, () => Color);
            }
        }

        // DescriptionSource
        private string _descriptionSource;
        public string DescriptionSource
        {
            get => _descriptionSource;
            set
            {
                SetProperty(ref _descriptionSource, value, () => DescriptionSource);
            }
        }

        // GeoMaterial
        private string _geoMaterial;
        public string GeoMaterial
        {
            get => _geoMaterial;
            set
            {
                SetProperty(ref _geoMaterial, value, () => GeoMaterial);
            }
        }

        // GeoMaterialConfidence
        private string _geoMaterialConfidence;
        public string GeoMaterialConfidence
        {
            get => _geoMaterialConfidence;
            set
            {
                SetProperty(ref _geoMaterialConfidence, value, () => GeoMaterialConfidence);
            }
        }

        private void Reset()
        {
            //await DataHelper.PopulateMapUnits();

            //NotifyPropertyChanged("AllHeadings");

            // Reset values
            Name = null;
            FullName = null;
            OlderInterval = null;
            YoungerInterval = null;

            Parent = null;
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
        private void Submit()
        {
            if (Data.DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            return;
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

        // Validate the Heading's name
        private void ValidateHeadingName(string name)
        {
            const string propertyKey = "Name";

            // Required field
            if (string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            // Name must be unique 
            else if (Data.DescriptionOfMapUnitData.AllDescriptionOfMapUnits.Any(a => a.Name.ToLower() == name.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Name is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        // Validate the Heading's Full Name
        private void ValidateFullName(string fullName)
        {
            const string propertyKey = "FullName";

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
            if (younger == null)
            {
                _validationErrors["YoungerInterval"] = new List<string>() { "" };
            }
            if (older == null)
            {
                _validationErrors["OlderInterval"] = new List<string>() { "" };
            }

            if (younger != null && older != null)
            {
                _validationErrors.Remove("YoungerInterval");

                if (younger.Early_Age < older.Early_Age)
                {
                    _validationErrors["OlderInterval"] = new List<string>() { "Swap those comboboxes!" };
                }
                else
                {
                    _validationErrors.Remove("OlderInterval");
                }

            }

            RaiseErrorsChanged("YoungerInterval");
            RaiseErrorsChanged("OlderInterval");
        }

        private void ValidateParent(int? parentid)
        {
            const string propertyKey = "Parent";

            // Required field
            if (parentid == null)
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
