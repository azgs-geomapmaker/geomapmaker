using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker
{
    internal class HeadingsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_Headings";

        public ObservableCollection<MapUnit> HeadingOptions { get; set; }

        public ObservableCollection<KeyValuePair<int, string>> ParentOptions { get; set; }

        protected HeadingsViewModel()
        {
            // Init command relays
            CommandSubmit = new RelayCommand(() => SubmitAsync(), () => CanSubmit());

            HeadingOptions = DataHelper.MapUnits;

            ParentOptions = new ObservableCollection<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>( 1, "A"),
                new KeyValuePair<int, string>( 2, "B"),
                new KeyValuePair<int, string>( 3, "C"),
                new KeyValuePair<int, string>( 4, "D"),
            };

        }

        /// <summary>
        /// Map Unit Model
        /// </summary>
        private MapUnit _model = new MapUnit();
        public MapUnit Model
        {
            get => _model;
            set => SetProperty(ref _model, value, () => Model);
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

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Headings";
        public string Heading
        {
            get { return _heading; }
            set => SetProperty(ref _heading, value, () => Heading);
        }

        public ICommand CommandSubmit { get; }

        /// <summary>
        /// Determines the visibility (enabled state) of the submit button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSubmit()
        {
            return Model != null && !string.IsNullOrWhiteSpace(Model.Name) && !string.IsNullOrWhiteSpace(Model.Description);
        }

        /// <summary>
        /// Execute the submit command
        /// </summary>
        private async Task SubmitAsync()
        {
            if (Model.ID == 0)
            {
                // Save Heading
                await SaveHeadingAsync(Model);
            }
            else
            {
                // Update Heading
            }
        }

        private async Task SaveHeadingAsync(MapUnit model)
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

                                rowBuffer["Name"] = model.Name;
                                rowBuffer["Description"] = model.Description;
                                rowBuffer["ParagraphStyle"] = model.ParagraphStyle;
                                rowBuffer["Type"] = string.IsNullOrWhiteSpace(model.ParagraphStyle) ? 0 : 1;

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

                        Model = null;

                    }
                }
            });
        }

        public int ModelId
        {
            set
            {
                Model = DataHelper.MapUnits.FirstOrDefault(a => a.ID == value) ?? new MapUnit();
            }
        }

        public ObservableCollection<KeyValuePair<int, string>> MapUnitsList
        {
            get
            {
                var headings = DataHelper.MapUnits.Where(a => a.Type == 0 || a.Type == 1).OrderBy(a => a.Name).Select(a => new KeyValuePair<int, string>(a.ID, a.Name));

                var collection = new ObservableCollection<KeyValuePair<int, string>>(headings);

                collection.Insert(0, new KeyValuePair<int, string>(-1, "Add Heading"));

                return collection;
            }
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
