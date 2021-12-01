using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    public class CreateHeadingVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave { get; }

        public HeadingsViewModel ParentVM { get; set; }

        public CreateHeadingVM(HeadingsViewModel parentVM)
        {
            CommandSave = new RelayCommand(() => SaveAsync(), () => CanSave());

            ParentVM = parentVM;

            // Initialize required values
            Name = null;
            Description = null;
        }

        // Heading Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                ValidateHeadingName(Name, "Name");
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
                ValidateDescription(Description, "Description");
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
        private async void SaveAsync()
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
                        TableDefinition tableDefinition = enterpriseTable.GetDefinition();

                        using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                        {
                            rowBuffer["Name"] = Name;
                            rowBuffer["FullName"] = Name;
                            rowBuffer["Description"] = Description;
                            rowBuffer["ParagraphStyle"] = "Heading";
                            rowBuffer["DescriptionSourceID"] = GeomapmakerModule.DataSourceId;

                            using (Row row = enterpriseTable.CreateRow(rowBuffer))
                            {
                                // To Indicate that the attribute table has to be updated.
                                context.Invalidate(row);
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
                await ParentVM.RefreshMapUnitsAsync();

                // Reset values
                Name = "";
                Description = "";
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
            else if (ParentVM.MapUnits.Any(a => a.Name.ToLower() == name.ToLower()))
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
