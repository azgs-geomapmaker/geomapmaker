using ArcGIS.Core.Data;
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

namespace Geomapmaker.ViewModels
{
    internal class HeadingsDeleteVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandDelete { get; }
        public ICommand CommandReset { get; }

        public HeadingsDeleteVM()
        {
            // Init submit command
            CommandDelete = new RelayCommand(() => DeleteAsync(), () => CanDelete());
            CommandReset = new RelayCommand(() => ResetAsync(), () => CanReset());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(DataHelper.MapUnits.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name));

        /// <summary>
        /// Edit Model
        /// </summary>
        private MapUnit _selectedHeading;
        public MapUnit SelectedHeading
        {
            get => _selectedHeading;
            set
            {
                SetProperty(ref _selectedHeading, value, () => SelectedHeading);

                Name = value?.Name;
                Description = value?.Description;
                Parent = DataHelper.MapUnits.FirstOrDefault(a => a.ID == value?.ParentId)?.Name;

                NotifyPropertyChanged("Name");
                NotifyPropertyChanged("Description");
                NotifyPropertyChanged("Parent");
            }
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Parent { get; set; }

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanDelete()
        {
            return HasErrors;
        }

        private bool CanReset()
        {
            return SelectedHeading != null;
        }

        private async Task ResetAsync()
        {
            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
        private async Task DeleteAsync()
        {
            if (DataHelper.connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

            });

            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
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

        //// Validate the Heading's name
        //private void ValidateHeadingName(string name)
        //{
        //    const string propertyKey = "Name";

        //    if (SelectedHeading != null && string.IsNullOrWhiteSpace(name))
        //    {
        //        _validationErrors[propertyKey] = new List<string>() { "" };
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //    else if (DataHelper.MapUnits.Where(a => a.ID != SelectedHeading?.ID).Any(a => a.Name.ToLower() == name?.ToLower()))
        //    {
        //        _validationErrors[propertyKey] = new List<string>() { "Name is taken." };
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //    else
        //    {
        //        _validationErrors.Remove(propertyKey);
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //}

        #endregion
    }
}
