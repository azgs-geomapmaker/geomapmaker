using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace Geomapmaker.ViewModels
{
    internal class HierarchyViewModel : DockPane, IDropTarget
    {
        private const string _dockPaneID = "Geomapmaker_Hierarchy";

        public ObservableCollection<MapUnit> Tree { get; set; }

        public ObservableCollection<MapUnit> Orphans { get; set; }

        protected HierarchyViewModel() 
        {

            MapUnit heading1 = new MapUnit() { Name = "Heading1" };
            heading1.Children.Add(new MapUnit() { Name = "SubHeading1" });
            heading1.Children.Add(new MapUnit() { Name = "SubHeading2" });

            MapUnit heading2 = new MapUnit() { Name = "Heading2" };
            heading2.Children.Add(new MapUnit() { Name = "SubHeading3" });
            heading2.Children.Add(new MapUnit() { Name = "SubHeading4" });
            Tree = new ObservableCollection<MapUnit> { heading1, heading2 };

            MapUnit orphan1 = new MapUnit() { Name = "Orphan1" };
            MapUnit orphan2 = new MapUnit() { Name = "Orphan2" };
            MapUnit orphan3 = new MapUnit() { Name = "Orphan3" };
            MapUnit orphan4 = new MapUnit() { Name = "Orphan4" };

            Orphans = new ObservableCollection<MapUnit> { orphan1, orphan2, orphan3, orphan4 };

            //Tree = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Headings);
            //Orphans = new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.MapUnits);
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
