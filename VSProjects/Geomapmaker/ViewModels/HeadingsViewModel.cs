using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker
{
    internal class HeadingsViewModel : DockPane
    {
        // TODO
        // Add a null option for parent (create and edit)
        // Enable Submit Button on key down instad of off-focus
        // Prevent duplicate heading names for create/edit

        // Edit needs some changes. Need to save a copy of the original Heading 

        // Ask Andrew about deletion

        // DONE
        // Remove current heading from parent-options in edit
        // Update parent-options when a new heading is created

        private const string _dockPaneID = "Geomapmaker_Headings";

        // Create's save button
        public ICommand CommandSubmit { get; }

        // Edit's update button
        public ICommand CommandUpdate { get; }

        protected HeadingsViewModel()
        {
            // Init command relays
            CommandSubmit = new RelayCommand(() => SubmitAsync(), () => CanSubmit());
            CommandUpdate = new RelayCommand(() => UpdateAsync(), () => CanUpdate());
        }

        /// <summary>
        /// Create Model
        /// </summary>
        private MapUnit _createModel = new MapUnit();
        public MapUnit CreateModel
        {
            get => _createModel;
            set => SetProperty(ref _createModel, value, () => CreateModel);
        }

        /// <summary>
        /// Edit Model
        /// </summary>
        private MapUnit _selectedModel;
        public MapUnit SelectedModel
        {
            get => _selectedModel;
            set
            {
                SetProperty(ref _selectedModel, value, () => SelectedModel);
                NotifyPropertyChanged("EditParents");
                EditModel.Name = value?.Name;
                EditModel.Description = value?.Description;
                EditModel.ParagraphStyle = value?.ParagraphStyle;
                NotifyPropertyChanged("EditModel");
            }
        }

        /// <summary>
        /// Edit Model
        /// </summary>
        private MapUnit _editModel = new MapUnit();
        public MapUnit EditModel
        {
            get => _editModel;
            set
            {
                SetProperty(ref _editModel, value, () => EditModel);
            }
        }

        /// <summary>
        /// List of all Headings/Subheadings
        /// </summary>
        public ObservableCollection<MapUnit> HeadingsList => new ObservableCollection<MapUnit>(DataHelper.MapUnits.Where(a => a.Type != 2).OrderBy(a => a.Name));

        /// <summary>
        /// List of parent-options during edit
        /// </summary>
        public ObservableCollection<MapUnit> EditParents
        {
            get
            {
                // Remove selected heading as a possible parent
                IOrderedEnumerable<MapUnit> headingList = DataHelper.MapUnits
                    .Where(a => a.Type != 2)
                    .Where(a => a.ID != SelectedModel?.ID)
                    .OrderBy(a => a.Name);

                return new ObservableCollection<MapUnit>(headingList);
            }
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSubmit()
        {
            return CreateModel != null && !string.IsNullOrWhiteSpace(CreateModel.Name) && !string.IsNullOrWhiteSpace(CreateModel.Description);
        }

        /// <summary>
        /// Execute the submit command
        /// </summary>
        private async Task SubmitAsync()
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
                            TableDefinition tableDefinition = enterpriseTable.GetDefinition();
                            using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                            {

                                rowBuffer["Name"] = CreateModel.Name;
                                rowBuffer["Description"] = CreateModel.Description;
                                rowBuffer["ParagraphStyle"] = CreateModel.ParagraphStyle;
                                rowBuffer["Type"] = string.IsNullOrWhiteSpace(CreateModel.ParagraphStyle) ? 0 : 1;

                                using (Row row = enterpriseTable.CreateRow(rowBuffer))
                                {
                                    // To Indicate that the attribute table has to be updated.
                                    context.Invalidate(row);
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

            CreateModel = new MapUnit();

            await DataHelper.PopulateMapUnits();

            NotifyPropertyChanged("HeadingsList"); 
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanUpdate()
        {
            if (SelectedModel == null || EditModel == null)
            {
                return false;
            }

            // Validate required inputs
            if (string.IsNullOrWhiteSpace(EditModel.Name) && string.IsNullOrWhiteSpace(EditModel.Description))
            {
                return false;
            }

            // Compare edit values with original values. Can update if the values changed.
            return !(SelectedModel.Name.Equals(EditModel.Name) && SelectedModel.Description.Equals(EditModel.Description));
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
                            QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + SelectedModel.ID };

                            using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                            {

                                while (rowCursor.MoveNext())
                                { //TODO: Anything? Should be only one
                                    using (Row row = rowCursor.Current)
                                    {
                                        // In order to update the Map and/or the attribute table.
                                        // Has to be called before any changes are made to the row.
                                        context.Invalidate(row);

                                        row["Name"] = EditModel.Name;
                                        row["Description"] = EditModel.Description;
                                        row["ParagraphStyle"] = EditModel.ParagraphStyle;
                                        row["Type"] = string.IsNullOrWhiteSpace(EditModel.ParagraphStyle) ? 0 : 1;

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

            NotifyPropertyChanged("HeadingsList");

            SelectedModel = null;
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
            {
                return;
            }

            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Headings_ShowButton : Button
    {
        protected override void OnClick()
        {
            HeadingsViewModel.Show();
        }
    }
}
