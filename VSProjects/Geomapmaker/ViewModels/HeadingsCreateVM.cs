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

namespace Geomapmaker.ViewModels
{
    internal class HeadingsCreateVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandSave { get; }
        public ICommand CommandReset { get; }

        public HeadingsCreateVM()
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
                ValidateHeadingName(_name);
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
                ValidateDescription(_description);
            }
        }

        // Heading parent Id
        private int? _parent;
        public int? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value, () => Parent);
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
                                rowBuffer["Name"] = Name;
                                rowBuffer["Description"] = Description;
                                rowBuffer["ParagraphStyle"] = "Heading";
                                rowBuffer["ParentId"] = Parent;

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
            await DataHelper.PopulateMapUnits();

            // Reset values
            Name = "";
            Description = "";
            Parent = null;

            NotifyPropertyChanged("ParentOptions");
        }

        private async Task ResetAsync()
        {
            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            Name = null;
            Description = null;
            Parent = null;
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
                List<KeyValuePair<int?, string>> headingList = DataHelper.MapUnits
                    .Where(a => a.ParagraphStyle == "Heading")
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                    .ToList();

                headingList.Insert(0, new KeyValuePair<int?, string>(null, ""));

                // Initialize a ObservableCollection with the list
                // Note: Casting a list as an OC does not working.
                return new ObservableCollection<KeyValuePair<int?, string>>(headingList);
            }
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
            else if (DataHelper.MapUnits.Any(a => a.Name.ToLower() == name.ToLower()))
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

        // Validate the Heading's definition
        private void ValidateDescription(string definition)
        {
            const string propertyKey = "Description";

            // Required field
            if (string.IsNullOrWhiteSpace(definition))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
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
