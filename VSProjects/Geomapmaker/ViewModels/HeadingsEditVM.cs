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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels
{
    internal class HeadingsEditVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandUpdate { get; }

        public HeadingsEditVM()
        {
            // Init submit command
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(DataHelper.MapUnits.Where(a => a.Type == 0 || a.Type == 1).OrderBy(a => a.Name));

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
                Definition = value?.Description;

                // TODO. Double check this conversion is working
                // Try converting ParagraphStyle to int, -1 if fails 
                Parent = int.TryParse(value?.ParagraphStyle, out int i) ? i : -1;

                // Update parent options to remove selected heading
                NotifyPropertyChanged("ParentOptions");
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
        private int? _parent;
        public int? Parent
        {
            get => _parent;
            set
            {
                SetProperty(ref _parent, value, () => Parent);
                ValidateParent(_parent);
            }
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanUpdate()
        {
            // Can't submit if are any errors
            if (HasErrors || SelectedHeading == null)
            {
                return false;
            }

            // Convert null or empty to an empty string (Or the string value)
            string originalParent = string.IsNullOrEmpty(SelectedHeading.ParagraphStyle) ? "" : SelectedHeading.ParagraphStyle;

            // Convert null or -1 to an empty string (Or the string value)
            string newParent = Parent == null || Parent == -1 ? "" : Parent.ToString();

            // Compare original values to updated values
            return SelectedHeading.Name != Name || SelectedHeading.Description != Definition || originalParent != newParent;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
        private async Task UpdateAsync()
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
                            QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + SelectedHeading.ID };

                            using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                            {

                                while (rowCursor.MoveNext())
                                { //TODO: Anything? Should be only one
                                    using (Row row = rowCursor.Current)
                                    {
                                        // In order to update the Map and/or the attribute table.
                                        // Has to be called before any changes are made to the row.
                                        context.Invalidate(row);

                                        row["Name"] = Name;
                                        row["Description"] = Definition;







                                        row["ParagraphStyle"] = Parent == null || Parent == -1 ? null : Parent.ToString();
                                        row["Type"] = Parent == null || Parent == -1 ? 0 : 1;

                                        // After all the changes are done, persist it.
                                        row.Store();

                                        // Has to be called after the store too.
                                        context.Invalidate(row);
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

            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("AllHeadings");
            NotifyPropertyChanged("ParentOptions");

            // Reset values
            SelectedHeading = null;
            Parent = null;
        }

        /// <summary>
        /// List of parent-options available during create
        /// </summary>
        public ObservableCollection<KeyValuePair<int?, string>> ParentOptions
        {
            get
            {




                // Remove selected heading from parent options
                List<KeyValuePair<int?, string>> headingList = DataHelper.MapUnits
                    .Where(a => a.Type == 0 || a.Type == 1)
                    .Where(a => a.ID != SelectedHeading?.ID)
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                    .ToList();

                var foo = headingList;

                headingList.Insert(0, new KeyValuePair<int?, string>(-1, "(No Parent)"));

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

            if (SelectedHeading != null && string.IsNullOrWhiteSpace(name))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
                RaiseErrorsChanged(propertyKey);
            }
            else if (DataHelper.MapUnits.Where(a => a.ID != SelectedHeading?.ID).Any(a => a.Name.ToLower() == name?.ToLower()))
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

            if (SelectedHeading != null && string.IsNullOrWhiteSpace(definition))
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

        // Validate the Heading's definition
        private void ValidateParent(int? parent)
        {
            const string propertyKey = "Parent";

            int? checkId = parent;

            while (checkId != null && checkId != -1)
            {
                // Look up the parent
                MapUnit current = DataHelper.MapUnits.FirstOrDefault(a => a.ID == checkId);

                if (current != null)
                {
                    var parentId = int.Parse(current.ParagraphStyle);

                    if (parentId == SelectedHeading.ID)
                    {
                        _validationErrors[propertyKey] = new List<string>() { "Yikes!" };
                        RaiseErrorsChanged(propertyKey);
                        return;
                    }
                    checkId = parentId;

                }
            }

            // Finally remove any error
            _validationErrors.Remove(propertyKey);
            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
