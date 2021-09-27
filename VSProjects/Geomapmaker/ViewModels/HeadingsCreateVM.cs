using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels
{
    internal class HeadingsCreateVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandSubmit { get; }

        public HeadingsCreateVM()
        {
            // Init submit command
            CommandSubmit = new RelayCommand(() => SubmitAsync(), () => CanSubmit());

            // Initializing as empty strings to trigger validation
            Name = "";
            Definition = "";
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
        private string _definition;
        public string Definition
        {
            get => _definition;
            set
            {
                SetProperty(ref _definition, value, () => Definition);
                ValidateDefinition(_definition);
            }
        }

        // Heading parent Id
        private KeyValuePair<int, string>? _parent;
        public KeyValuePair<int, string>? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value, () => Parent);
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSubmit()
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
                                rowBuffer["Description"] = Definition;

                                if (Parent == null || Parent.Value.Key == -1)
                                {
                                    rowBuffer["ParagraphStyle"] = "Root";
                                    rowBuffer["ParentId"] = null;
                                    rowBuffer["Type"] = 0;
                                }
                                else
                                {
                                    rowBuffer["ParagraphStyle"] = Parent.Value.Value;
                                    rowBuffer["ParentId"] = Parent.Value.Key;
                                    rowBuffer["Type"] = 1;
                                }

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

            await DataHelper.PopulateMapUnits();

            // Reset values
            Name = "";
            Definition = "";
            Parent = null;

            NotifyPropertyChanged("ParentOptions");
        }

        /// <summary>
        /// List of parent-options available during create
        /// </summary>
        public ObservableCollection<KeyValuePair<int, string>> ParentOptions
        {
            get
            {
                // Get Headings/Subheadings from map units.
                // Sort by name
                // Create a int/string kvp for the combobox
                List<KeyValuePair<int, string>> headingList = DataHelper.MapUnits
                    .Where(a => a.Type == 0 || a.Type == 1)
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int, string>(a.ID, a.Name))
                    .ToList();

                // Insert the no parent option (-1) at the top of the list.
                // Needed an explicit option incase user opens the combobox. 
                // If they don't open the comboobox then parent is null.
                headingList.Insert(0, new KeyValuePair<int, string>(-1, "(No Parent)"));

                // Initialize a ObservableCollection with the list
                // Note: Casting a list as an OC does not working.
                return new ObservableCollection<KeyValuePair<int, string>>(headingList);
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

            if (string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
                RaiseErrorsChanged(propertyKey);
            }
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
        private void ValidateDefinition(string definition)
        {
            const string propertyKey = "Definition";

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
