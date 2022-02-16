using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    public class EditHeadingVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        public HeadingsViewModel ParentVM { get; set; }

        public EditHeadingVM(HeadingsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        /// <summary>
        /// Map Unit selected for edit
        /// </summary>
        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);
                Name = Selected?.Name;
                Description = Selected?.Description;
                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        // Heading Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                ValidateHeadingName(Name, "Name");
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
                ValidateDescription(Description, "Description");
                ValidateChangeWasMade();
            }
        }

        private bool CanUpdate()
        {
            return Selected != null && !HasErrors;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
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

                                    row["Name"] = Name;
                                    row["FullName"] = Name;
                                    row["Description"] = Description;
                                    row["ParagraphStyle"] = "Heading";
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
            // Error message isn't display on a field. Just prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (Selected == null)
            {
                _validationErrors.Remove(propertyKey);
                return;
            }

            if (Selected.Name == Name && Selected.Description == Description)
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
            if (Selected != null && string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.MapUnits.Where(a => a.ID != Selected?.ID).Any(a => a.Name.ToLower() == name?.ToLower()))
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
            if (Selected != null && string.IsNullOrWhiteSpace(definition))
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
