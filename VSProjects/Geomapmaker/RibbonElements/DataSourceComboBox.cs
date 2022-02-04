using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class DataSourceComboBox : ComboBox
    {
        protected override void OnDropDownOpened()
        {
            StandaloneTable dataSources = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (dataSources == null)
            {
                FrameworkApplication.State.Deactivate("datasource_selected");
                return;
            }

            Clear();

            QueuedTask.Run(() =>
            {
                Table enterpriseTable = dataSources.GetTable();

                if (enterpriseTable == null)
                {
                    return;
                }

                QueryFilter queryFilter = new QueryFilter
                {
                    PostfixClause = "ORDER BY datasources_id"
                };

                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            DataSource dS = new DataSource
                            {
                                ObjectId = long.Parse(row["objectid"].ToString()),
                                Source = row["source"]?.ToString(),
                                DataSource_ID = row["datasources_id"]?.ToString(),
                                Url = row["url"]?.ToString(),
                                Notes = row["notes"]?.ToString()
                            };

                            Add(new ComboBoxItem(dS.DataSource_ID, null, dS.Source));
                        }
                    }
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
