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

namespace Geomapmaker.ViewModels.Headings
{
    internal class HeadingsDeleteVM : DockPane, INotifyDataErrorInfo
    {
        // Create's save button
        public ICommand CommandDelete { get; }
        public ICommand CommandReset { get; }

        public HeadingsDeleteVM()
        {
            // Init submit command
            CommandDelete = new RelayCommand(() => DeleteHeadingAsync(), () => CanDelete());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> AllHeadings => new ObservableCollection<MapUnit>(DataHelper.MapUnits.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name));

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
                Description = value?.Description;
                Parent = DataHelper.MapUnits.FirstOrDefault(a => a.ID == value?.ParentId)?.Name;
                Tree = value != null ? new List<MapUnit> { new MapUnit { Name = value.Name, Children = GetChildren(value) } } : null;
                NotifyPropertyChanged("Name");
                NotifyPropertyChanged("Description");
                NotifyPropertyChanged("Parent");
                NotifyPropertyChanged("Tree");
                ValidateChildfree(Tree);
            }
        }

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

        public string Name { get; set; }
        public string Description { get; set; }
        public string Parent { get; set; }
        public List<MapUnit> Tree { get; set; }

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
            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("AllHeadings");

            // Reset values
            SelectedHeading = null;
        }

        /// <summary>
        /// Execute the save command
        /// </summary>
        private async Task DeleteHeadingAsync()
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

        // Validate children
        private void ValidateChildfree(List<MapUnit> tree)
        {
            const string propertyKey = "Tree";

            if (tree == null || tree.Count == 0)
            {
                _validationErrors.Remove(propertyKey);
                RaiseErrorsChanged(propertyKey);
                return;
            }

            // First node in the list is the root element
            MapUnit root = tree.First();

            if (root.Children != null && root.Children.Count != 0)
            {
                // Raise the error
                _validationErrors[propertyKey] = new List<string>() { "Can't delete headings with a child node." };
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
