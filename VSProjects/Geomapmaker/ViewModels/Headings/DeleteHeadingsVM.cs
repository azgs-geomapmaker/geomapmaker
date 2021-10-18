using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    internal class DeleteHeadingsVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandDelete { get; }
        public ICommand CommandReset { get; }

        public DeleteHeadingsVM()
        {
            // Init submit command
            CommandDelete = new RelayCommand(() => DeleteHeadingAsync(), () => CanDelete());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Headings);

        /// <summary>
        /// Map Unit selected to be deleted
        /// </summary>
        private MapUnit _selectedHeading;
        public MapUnit SelectedHeading
        {
            get => _selectedHeading;
            set
            {
                SetProperty(ref _selectedHeading, value, () => SelectedHeading);

                Name = value?.Name;
                NotifyPropertyChanged("Name");
                Description = value?.Description;
                NotifyPropertyChanged("Description");
            }
        }

        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanDelete()
        {
            return SelectedHeading != null && !HasErrors;
        }

        private async Task ResetAsync()
        {
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
        private async Task DeleteHeadingAsync()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you want to delete {Name}?", $"Delete {Name}?", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            if (Data.DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(Data.DbConnectionProperties.GetProperties()))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits"))
                    {

                        editOperation.Callback(context =>
                        {
                            QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + SelectedHeading.ID };

                            using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                            {

                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        context.Invalidate(row);

                                        row.Delete();
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

            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

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

        #endregion
    }
}
