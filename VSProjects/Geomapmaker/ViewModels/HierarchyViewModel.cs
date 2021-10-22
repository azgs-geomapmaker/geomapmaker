using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels
{
    internal class HierarchyViewModel : DockPane, IDropTarget
    {
        private const string _dockPaneID = "Geomapmaker_Hierarchy";

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"Heading", "TODO Heading" },

            {"HierarchyTree", "TODO HierarchyTree" },
            {"Unassigned", "TODO Unassigned" },
        };

        public ObservableCollection<MapUnitTreeItem> Tree { get; set; } = new ObservableCollection<MapUnitTreeItem>(Data.DescriptionOfMapUnits.Tree);

        public ObservableCollection<MapUnitTreeItem> Unassigned { get; set; } = new ObservableCollection<MapUnitTreeItem>(Data.DescriptionOfMapUnits.Unassigned);

        public ICommand CommandSave { get; }
        public ICommand CommandReset { get; }

        protected HierarchyViewModel()
        {
            // Init submit command
            CommandSave = new RelayCommand(() => SaveAsync(), () => CanSave());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        private bool CanSave()
        {
            return true;
        }

        private async Task ResetAsync()
        {
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            Tree = new ObservableCollection<MapUnitTreeItem>(Data.DescriptionOfMapUnits.Tree);
            NotifyPropertyChanged("Tree");

            Unassigned = new ObservableCollection<MapUnitTreeItem>(Data.DescriptionOfMapUnits.Unassigned);
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
            HierarchyList = new List<MapUnitTreeItem>();
            SetHierarchyKeys(Tree);

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(Data.DbConnectionProperties.GetProperties()))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits"))
                    {
                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = enterpriseTable.Search(null, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        int ID = int.Parse(row["ObjectID"].ToString());

                                        // In order to update the Map and/or the attribute table.
                                        // Has to be called before any changes are made to the row.
                                        context.Invalidate(row);

                                        row["HierarchyKey"] = HierarchyList.FirstOrDefault(a => a.ID == ID)?.HierarchyKey ?? "";

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

            await ResetAsync();

        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            MapUnitTreeItem sourceItem = dropInfo.Data as MapUnitTreeItem;
            MapUnitTreeItem targetItem = dropInfo.TargetItem as MapUnitTreeItem;
            ICollection<MapUnitTreeItem> targetCollection = dropInfo.TargetCollection as ICollection<MapUnitTreeItem>;

            bool isItemDropValid = sourceItem != null && targetItem != null && targetItem.CanAcceptChildren;
            bool isCollectionDropValid = sourceItem != null && targetCollection != null;


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
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Hierarchy_ShowButton : Button
    {
        protected override void OnClick()
        {
            HierarchyViewModel.Show();
        }
    }
}
