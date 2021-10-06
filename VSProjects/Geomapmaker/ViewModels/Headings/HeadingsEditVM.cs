﻿using ArcGIS.Core.Data;
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

namespace Geomapmaker.ViewModels.Headings
{
    internal class HeadingsEditVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandUpdate { get; }
        public ICommand CommandReset { get; }

        public HeadingsEditVM()
        {
            // Init submit command
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(DataHelper.MapUnits.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name));

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

                // Update parent options to remove selected heading
                NotifyPropertyChanged("ParentOptions");

                Name = value?.Name;
                Description = value?.Description;
                Parent = value?.ParentId;

                Tree = value != null ? new List<MapUnit> { new MapUnit { Name = value.Name, Children = GetChildren(value) } } : null;
                NotifyPropertyChanged("Tree");
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
                ValidateIfChangeWasMade();
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
                ValidateIfChangeWasMade();
            }
        }

        // Heading's Parent
        private int? _parent;
        public int? Parent
        {
            get => _parent;
            set
            {
                SetProperty(ref _parent, value, () => Parent);
                ValidateParent(_parent);
                ValidateIfChangeWasMade();
            }
        }

        public List<MapUnit> Tree { get; set; }

        // Recursively look up children
        private List<MapUnit> GetChildren(MapUnit root)
        {
            // Get mapunit's children
            List<MapUnit> children = DataHelper.MapUnits.Where(a => a.ParentId == root?.ID).ToList();

            // If no children
            if (children.Count == 0)
            {
                return null;
            }
            else
            {
                // Get the children of each child
                foreach (MapUnit child in children)
                {
                    child.Children = GetChildren(child);
                }
            }

            return children;
        }
        
        private async Task ResetAsync()
        {
            await DataHelper.PopulateMapUnits();

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
                                        row["Description"] = Description;
                                        row["ParagraphStyle"] = "Heading";
                                        row["ParentId"] = Parent;

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
                    .Where(a => a.ParagraphStyle == "Heading")
                    .Where(a => a.ID != SelectedHeading?.ID)
                    .OrderBy(a => a.Name)
                    .Select(a => new KeyValuePair<int?, string>(a.ID, a.Name))
                    .ToList();

                headingList.Insert(0, new KeyValuePair<int?, string>(null, ""));

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

        private void ValidateIfChangeWasMade()
        {
            // This error isn't display on any field. Prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (SelectedHeading == null)
            {
                _validationErrors.Remove(propertyKey);
                RaiseErrorsChanged(propertyKey);
                return;
            }

            if (SelectedHeading.Name == Name && SelectedHeading.Description == Description && SelectedHeading.ParentId == Parent)
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
                RaiseErrorsChanged(propertyKey);
            }
            else
            {
                _validationErrors.Remove(propertyKey);
                RaiseErrorsChanged(propertyKey);
            }
        }

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
        private void ValidateDescription(string definition)
        {
            const string propertyKey = "Description";

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
        private void ValidateParent(int? checkId)
        {
            const string propertyKey = "Parent";

            // Loop over parents
            while (checkId != null)
            {
                // Look up the parent to grab grandparent id
                checkId = DataHelper.MapUnits.FirstOrDefault(a => a.ID == checkId)?.ParentId;

                // Compare with selected heading
                if (checkId == SelectedHeading.ID)
                {
                    _validationErrors[propertyKey] = new List<string>() { "Parent" };
                    RaiseErrorsChanged(propertyKey);
                    return;
                }
            }

            // Finally remove any error
            _validationErrors.Remove(propertyKey);
            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}