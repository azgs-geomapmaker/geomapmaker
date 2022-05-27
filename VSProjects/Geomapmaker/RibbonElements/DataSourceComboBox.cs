using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Data;
using System.Collections.Generic;

namespace Geomapmaker.RibbonElements
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class DataSourceComboBox : ComboBox
    {
        protected override void OnDropDownOpened()
        {
            // Clear existing datasource options
            Clear();

            QueuedTask.Run(async () =>
            {
                List<string> DataSourceIds = await General.StandaloneTableGetDistinctValuesForFieldAsync("DataSources", "datasources_id");

                foreach (string id in DataSourceIds)
                {
                    Add(new ComboBoxItem(id, null, id));
                }
            });
        }

        protected override void OnTextChange(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                FrameworkApplication.State.Deactivate("datasource_selected");
            }
            else
            {
                GeomapmakerModule.DataSourceId = text;
                FrameworkApplication.State.Activate("datasource_selected");
            }
        }

    }
}
