using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    internal class EditHeadingsVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandUpdate { get; }
        public ICommand CommandReset { get; }

        public EditHeadingsVM()
        {
            // Init submit command
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Headings);

        /// <summary>
        /// Map Unit selected for edit
        /// </summary>
        private MapUnit _selectedHeading;
        public MapUnit SelectedHeading
        {
            get => _selectedHeading;
            set
            {
                SetProperty(ref _selectedHeading, value, () => SelectedHeading);

                Name = SelectedHeading?.Name;
                Description = SelectedHeading?.Description;
            }
        }

        // Heading Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                ValidateHeadingName(_name, "Name");
                ValidateChangeWasMade();
            }
        }

        // Heading Definition
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value, () => Description);
                ValidateDescription(_description, "Description");
                ValidateChangeWasMade();
            }
        }

        private async Task ResetAsync()
        {
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanUpdate()
        {
            return SelectedHeading != null && !HasErrors;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
        private async Task UpdateAsync()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu == null)
                {
                    MessageBox.Show("DescriptionOfMapUnits table not found in active map.");
                    return Task.CompletedTask;
                }

                Table enterpriseTable = dmu.GetTable();

                EditOperation editOperation = new EditOperation();

                editOperation.Callback(context =>
                {
                    QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + SelectedHeading.ID };

                    using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // In order to update the Map and/or the attribute table.
                                // Has to be called before any changes are made to the row.
                                context.Invalidate(row);

                                row["Name"] = Name;
                                row["FullName"] = Name;
                                row["Description"] = Description;
                                row["ParagraphStyle"] = "Heading";
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
                return Task.CompletedTask;
            });

            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
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
            // Error message isn't display on a field. Just prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (SelectedHeading == null)
            {
                _validationErrors.Remove(propertyKey);
                return;
            }

            if (SelectedHeading.Name == Name && SelectedHeading.Description == Description)
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        // Validate the Heading's name
        private void ValidateHeadingName(string name, string propertyKey)
        {
            if (SelectedHeading != null && string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (Data.DescriptionOfMapUnits.DMUs.Where(a => a.ID != SelectedHeading?.ID).Any(a => a.Name.ToLower() == name?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Name is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        // Validate the Heading's definition
        private void ValidateDescription(string definition, string propertyKey)
        {
            if (SelectedHeading != null && string.IsNullOrWhiteSpace(definition))
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
