using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
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
    internal class CreateHeadingsVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandSave { get; }
        public ICommand CommandReset { get; }

        public CreateHeadingsVM()
        {
            // Init submit command
            CommandSave = new RelayCommand(() => SubmitAsync(), () => CanSave());
            CommandReset = new RelayCommand(() => ResetAsync());

            // Initialize to trigger validation
            Name = "";
            Description = "";
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
            }
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
        private async Task SubmitAsync()
        {
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
                            TableDefinition tableDefinition = enterpriseTable.GetDefinition();
                            using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                            {
                                rowBuffer["Name"] = Name;
                                rowBuffer["FullName"] = Name;
                                rowBuffer["Description"] = Description;
                                rowBuffer["ParagraphStyle"] = "Heading";
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

            // Update mapunits
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            // Reset values
            Name = "";
            Description = "";

            NotifyPropertyChanged("ParentOptions");
        }

        private async Task ResetAsync()
        {
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("ParentOptions");

            // Reset values
            Name = null;
            Description = null;
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
                List<KeyValuePair<int?, string>> headingList = Data.DescriptionOfMapUnits.Headings
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                    .ToList();

                headingList.Insert(0, new KeyValuePair<int?, string>(null, ""));

                // Initialize a ObservableCollection with the list
                // Note: Casting a list as an OC does not working.
                return new ObservableCollection<KeyValuePair<int?, string>>(headingList);
            }
        }

        #region Validation

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
        private void ValidateHeadingName(string name, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            // Name must be unique 
            else if (Data.DescriptionOfMapUnits.DMUs.Any(a => a.Name.ToLower() == name.ToLower()))
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
            // Required field
            if (string.IsNullOrWhiteSpace(definition))
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
