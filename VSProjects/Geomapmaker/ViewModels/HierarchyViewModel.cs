﻿using ArcGIS.Core.Data;
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
            {"Heading", "Heading" },
        };


        public ObservableCollection<MapUnit> Tree { get; set; } = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Tree);

        public ObservableCollection<MapUnit> Unassigned { get; set; } = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Unassigned);

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
            Tree = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Tree);
            NotifyPropertyChanged("Tree");
            Unassigned = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Unassigned);
            NotifyPropertyChanged("Unassigned");
        }

        private List<MapUnit> HierarchyList = new List<MapUnit>();
        private void SetHierarchyKeys(ObservableCollection<MapUnit> collection, string prefix = "")
        {
            if (collection == null || collection.Count == 0)
            {
                return;
            }

            int index = 1;

            foreach (MapUnit mu in collection)
            {
                string dash = string.IsNullOrEmpty(prefix) ? "" : "-";

                string HierarchyKey = prefix + dash + index.ToString("000");

                mu.HierarchyKey = HierarchyKey;

                HierarchyList.Add(mu);

                SetHierarchyKeys(mu.Children, HierarchyKey);

                index++;
            }
        }

        private async Task SaveAsync()
        {
            HierarchyList = new List<MapUnit>();
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
            MapUnit sourceItem = dropInfo.Data as MapUnit;
            MapUnit targetItem = dropInfo.TargetItem as MapUnit;

            if (sourceItem != null && targetItem != null && targetItem.CanAcceptChildren)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            MapUnit sourceItem = dropInfo.Data as MapUnit;
            MapUnit targetItem = dropInfo.TargetItem as MapUnit;
            targetItem.Children.Add(sourceItem);
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
