using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Hierarchy
{
    public class HierarchyViewModel : ProWindow, INotifyPropertyChanged, IDropTarget
    {
        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        private bool CanSave()
        {
            return hasChanged;
        }

        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() =>
        {
            WindowCloseEvent(this, new EventArgs());
        });

        // Track if a change was made
        private bool hasChanged = false;

        // Collection for hkey-assigned map units
        public ObservableCollection<MapUnitTreeItem> Tree { get; set; } = new ObservableCollection<MapUnitTreeItem>(new List<MapUnitTreeItem>());

        // Collection for unassigned map units
        public ObservableCollection<MapUnitTreeItem> Unassigned { get; set; } = new ObservableCollection<MapUnitTreeItem>(new List<MapUnitTreeItem>());

        void IDropTarget.DragEnter(IDropInfo dropInfo)
        {
            //throw new NotImplementedException();
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data != null)
            {
                MapUnitTreeItem sourceItem = dropInfo.Data as MapUnitTreeItem;

                MapUnitTreeItem targetItem = dropInfo.TargetItem as MapUnitTreeItem;

                // Target is a Collection
                if (targetItem == null)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
                else if (targetItem.ObjectID == sourceItem.ObjectID)
                {
                    // Can't set a node as a child of itself
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }
                else if (IsTargetADescendantOfSource(sourceItem, targetItem))
                {
                    // Can't set a node as a child of a descendant
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }
                // Target is a MapUnitTreeItem
                else
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                }

                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        // Recursively check if the target is a descendant of the Source
        private bool IsTargetADescendantOfSource(MapUnitTreeItem sourceItem, MapUnitTreeItem targetItem)
        {
            // Base case
            if (sourceItem.Children == null || sourceItem.Children.Count == 0)
            {
                return false;
            }
            else if (sourceItem.Children.Contains(targetItem))
            {
                return true;
            }
            else
            {
                foreach (var child in sourceItem.Children)
                {
                    return (IsTargetADescendantOfSource(child, targetItem));
                }
            }

            return false;
        }

        void IDropTarget.DragLeave(IDropInfo dropInfo)
        {
            //throw new NotImplementedException();
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            // Source
            MapUnitTreeItem sourceItem = dropInfo.Data as MapUnitTreeItem;
            ICollection<MapUnitTreeItem> sourceCollection = dropInfo.DragInfo.SourceCollection as ICollection<MapUnitTreeItem>;

            // Target
            MapUnitTreeItem targetItem = dropInfo.TargetItem as MapUnitTreeItem;
            ICollection<MapUnitTreeItem> targetCollection = dropInfo.TargetCollection as ICollection<MapUnitTreeItem>;
            string targetName = ((FrameworkElement)((DropInfo)dropInfo)?.VisualTarget)?.Name;

            sourceCollection.Remove(sourceItem);

            // Check if the target within the Unassigned listbox
            if (targetName == "Unassigned")
            {
                // Flatten descendents and add to unassigned
                FlattenAndAddToUnassigned(sourceItem);
            }
            else if (targetItem != null)
            {
                // Add to MapUnit child
                targetItem.Children.Add(sourceItem);
            }
            else
            {
                // Add to collection
                targetCollection.Add(sourceItem);
            }

            hasChanged = true;
        }

        // Flatten the tree and add to unassigned
        private void FlattenAndAddToUnassigned(MapUnitTreeItem mapUnit)
        {
            // Check for any children
            if (mapUnit?.Children?.Count > 0)
            {
                foreach (MapUnitTreeItem child in mapUnit?.Children)
                {
                    // Call on each child
                    FlattenAndAddToUnassigned(child);
                }
            }

            // Clear out children
            mapUnit.Children = new ObservableCollection<MapUnitTreeItem>();

            // Add to the flatList
            Unassigned.Add(mapUnit);
        }

        // Build the tree stucture
        public async void BuildTree()
        {
            // Get the hierarchy tree and unassigned list
            Tuple<List<MapUnitTreeItem>, List<MapUnitTreeItem>> hierarchyTuple = await Data.DescriptionOfMapUnits.GetHierarchyTreeAsync();

            Tree = new ObservableCollection<MapUnitTreeItem>(hierarchyTuple.Item1);
            NotifyPropertyChanged("Tree");

            Unassigned = new ObservableCollection<MapUnitTreeItem>(hierarchyTuple.Item2);
            NotifyPropertyChanged("Unassigned");
        }

        private List<MapUnitTreeItem> HierarchyList = new List<MapUnitTreeItem>();

        // Recursively build out Hierarchy Keys
        private void SetHierarchyKeys(ObservableCollection<MapUnitTreeItem> collection, string prefix = "")
        {
            // Base Case: The tree or a MU's children is empty
            if (collection == null || collection.Count == 0)
            {
                return;
            }

            // Start the index at 1
            int index = 1;

            foreach (MapUnitTreeItem mu in collection)
            {
                // Add a dash if the prefix is not empty
                string dash = string.IsNullOrEmpty(prefix) ? "" : "-";

                // Combine prefix, dash, and zero-padded index. For example, "001-001" + "-" + "001"
                string HierarchyKey = prefix + dash + index.ToString("000");

                // Set the HierarchyKey.
                mu.HierarchyKey = HierarchyKey;

                // Add to the list
                HierarchyList.Add(mu);

                // Recursive call to any children
                SetHierarchyKeys(mu.Children, mu.HierarchyKey);

                // Increment index for any siblings
                index++;
            }
        }

        // Write HierarchyKey to database
        private async Task SaveAsync()
        {
            string errorMessage = null;

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu == null)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("DescriptionOfMapUnits table not found in active map.", "One or more errors occured.");
                return;
            }

            HierarchyList = new List<MapUnitTreeItem>();
            SetHierarchyKeys(Tree);

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table enterpriseTable = dmu.GetTable())
                    {
                        if (enterpriseTable != null)
                        {
                            EditOperation editOperation = new EditOperation()
                            {
                                Name = "Update Hierarchy Keys"
                            };

                            editOperation.Callback(context =>
                            {
                                using (RowCursor rowCursor = enterpriseTable.Search(null, false))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            string ID = Helpers.RowValueToString(row["ObjectID"]);

                                            string updatedKey = HierarchyList.FirstOrDefault(a => a.ObjectID == ID)?.HierarchyKey ?? "";

                                            // Has to be called before any changes are made to the row.
                                            context.Invalidate(row);

                                            // Update the HierarchyKey value
                                            row["HierarchyKey"] = updatedKey;

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
                                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(editOperation.ErrorMessage, "One or more errors occured.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string innerEx = ex.InnerException?.ToString();

                    // Trim the stack-trace from the error msg
                    if (innerEx.Contains("--->"))
                    {
                        innerEx = innerEx.Substring(0, innerEx.IndexOf("--->"));
                    }

                    errorMessage = innerEx;
                }
            });

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(errorMessage, "One or more errors occured.");
            }
            else
            {
                // Close window
                WindowCloseEvent(this, new EventArgs());
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }

    internal class ShowHierarchy : Button
    {
        private Views.Hierarchy.Hierarchy _hierarchy = null;

        protected override async void OnClick()
        {
            if (_hierarchy != null)
            {
                _hierarchy.Close();
                return;
            }

            _hierarchy = new Views.Hierarchy.Hierarchy
            {
                Owner = Application.Current.MainWindow
            };

            await QueuedTask.Run(() =>
            {
                _hierarchy.hierarchyVM.BuildTree();
            });

            _hierarchy.Closed += (o, e) => { _hierarchy = null; };

            _hierarchy.hierarchyVM.WindowCloseEvent += (s, e) => _hierarchy.Close();

            _hierarchy.Show();
        }
    }
}