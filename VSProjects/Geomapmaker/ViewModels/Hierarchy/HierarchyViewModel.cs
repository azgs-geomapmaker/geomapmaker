﻿using ArcGIS.Core.Data;
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
        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"Heading", "TODO Heading" },

            {"HierarchyTree", "TODO HierarchyTree" },
            {"Unassigned", "TODO Unassigned" },
        };

        public ObservableCollection<MapUnitTreeItem> Tree { get; set; } = new ObservableCollection<MapUnitTreeItem>(new List<MapUnitTreeItem>());

        public ObservableCollection<MapUnitTreeItem> Unassigned { get; set; } = new ObservableCollection<MapUnitTreeItem>(new List<MapUnitTreeItem>());

        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        private bool CanSave()
        {
            // TODO Check if a change was made!
            return true;
        }

        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() =>
        {
            WindowCloseEvent(this, new EventArgs());
        });

        // Build the tree stucture
        public async Task BuildTree()
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
                MessageBox.Show("DescriptionOfMapUnits table not found in active map.");
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
                            EditOperation editOperation = new EditOperation();

                            editOperation.Callback(context =>
                            {
                                using (RowCursor rowCursor = enterpriseTable.Search(null, false))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            string ID = Helpers.RowValueToString(row["ObjectID"]);

                                            // In order to update the Map and/or the attribute table.
                                            // Has to be called before any changes are made to the row.
                                            context.Invalidate(row);

                                            row["HierarchyKey"] = HierarchyList.FirstOrDefault(a => a.ObjectID == ID)?.HierarchyKey ?? "";

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
                MessageBox.Show(errorMessage, "One or more errors occured.");
            }
            else
            {
                // Close window
                WindowCloseEvent(this, new EventArgs());
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            MapUnitTreeItem sourceItem = dropInfo.Data as MapUnitTreeItem;
            MapUnitTreeItem targetItem = dropInfo.TargetItem as MapUnitTreeItem;

            bool isItemDropValid = sourceItem != null && targetItem != null && targetItem.CanAcceptChildren;
            bool isCollectionDropValid = sourceItem != null && dropInfo.TargetCollection is ICollection<MapUnitTreeItem> targetCollection;

            if (sourceItem != null && targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            MapUnitTreeItem sourceItem = dropInfo.Data as MapUnitTreeItem;

            MapUnitTreeItem targetItem = dropInfo.TargetItem as MapUnitTreeItem;

            ICollection<MapUnitTreeItem> sourceCollection = dropInfo.DragInfo.SourceCollection as ICollection<MapUnitTreeItem>;

            targetItem.Children.Add(sourceItem);
            sourceCollection.Remove(sourceItem);
        }

        void IDropTarget.DragEnter(IDropInfo dropInfo)
        {
            // TODO
        }

        void IDropTarget.DragLeave(IDropInfo dropInfo)
        {
            // TODO
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
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

            // Tree takes a few seconds to load. Display progress dialog
            ProgressorSource ps = new ProgressorSource("Building hierarchy tree from DMU");

            await QueuedTask.Run(async () =>
            {
                await _hierarchy.hierarchyVM.BuildTree();
            }, ps.Progressor);

            _hierarchy.Closed += (o, e) => { _hierarchy = null; };

            _hierarchy.hierarchyVM.WindowCloseEvent += (s, e) => _hierarchy.Close();

            _hierarchy.Show();
        }
    }
}
