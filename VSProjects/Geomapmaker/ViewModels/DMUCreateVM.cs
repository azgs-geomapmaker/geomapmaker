using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
            }
        }

        // Younger Interval
        private string _youngerInterval;
        public string YoungerInterval
        {
            get => _youngerInterval;
            set
            {
                SetProperty(ref _youngerInterval, value, () => YoungerInterval);
            }
        }

        // Older Interval
        private string _olderInterval;
        public string OlderInterval
        {
            get => _olderInterval;
            set
            {
                SetProperty(ref _olderInterval, value, () => OlderInterval);
            }
        }

        // Relative Age
        private string _relativeAge;
        public string RelativeAge {
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
            //Description = null;
            //Parent = null;
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSave()
        {
            // Can't submit if are any errors
            return !HasErrors;
        }

        /// <summary>
        /// Execute the submit command
        /// </summary>
        private void Submit()
        {
            if (DataHelper.connectionProperties == null)
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
                RaiseErrorsChanged(propertyKey);
            }
            // Name must be unique 
            else if (Data.DescriptionOfMapUnitData.AllDescriptionOfMapUnits.Any(a => a.Name.ToLower() == name.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Name is taken." };
                RaiseErrorsChanged(propertyKey);
            }
            else
            {
                _validationErrors.Remove(propertyKey);
                RaiseErrorsChanged(propertyKey);
            }
        }

        #endregion
    }
}
