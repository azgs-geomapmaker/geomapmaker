using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.RibbonElements
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class DataSourceComboBox : ComboBox
    {
        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public DataSourceComboBox()
        {
            UpdateCombo();
        }

        // TODO 
        private bool isValidGemsProject()
        {
            if (MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources") == null)
                return false;
            if (MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits") == null)
                return false;

            return true;
        }

        /// <summary>
        /// Updates the combo box with all the items.
        /// </summary>

        private async void UpdateCombo()
        {
            if (!_isInitialized)
            {
                Clear();

                StandaloneTable dataSources = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

                await QueuedTask.Run(() =>
                {
                    Table enterpriseTable = dataSources.GetTable();

                    using (RowCursor rowCursor = enterpriseTable.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                DataSource dS = new DataSource
                                {
                                    ID = long.Parse(row["objectid"].ToString()),
                                    Source = row["source"]?.ToString(),
                                    DataSource_ID = row["datasources_id"]?.ToString(),
                                    Url = row["url"]?.ToString(),
                                    Notes = row["notes"]?.ToString()
                                };

                                Add(new ComboBoxItem(dS.DataSource_ID));
                            }
                        }
                    }
                });

                _isInitialized = true;
            }

            Enabled = true;
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item)
        {

            if (item == null || string.IsNullOrEmpty(item.Text))
            {
                FrameworkApplication.State.Deactivate("datasource_selected");
                return;
            }

            // Set the user's data source
            Data.DataSources.DataSourceId = item.Text;

            FrameworkApplication.State.Activate("datasource_selected");
        }

    }
}
