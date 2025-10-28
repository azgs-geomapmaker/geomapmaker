using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CsvHelper;
using Geomapmaker.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Geomapmaker.RibbonElements
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class DataSourceComboBox : ComboBox
    {
        public DataSourceComboBox()
        {
            GeomapmakerModule.DataSourceComboBox = this;
        }

        public void ClearSelection()
        {
            Clear();
        }

        protected override void OnDropDownOpened()
        {
            // Clear existing datasource options
            ClearSelection();

            QueuedTask.Run(async () =>
            {
                List<string> DataSourceIds = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DataSources", "datasources_id");

                foreach (string id in DataSourceIds)
                {
                    Add(new ComboBoxItem(id, null, id));
                }
            });
        }

        protected override async void OnTextChange(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                GeomapmakerModule.DataSourceId = null;
                FrameworkApplication.State.Deactivate("datasource_selected");
            }
            else
            {
                GeomapmakerModule.DataSourceId = text;

                ProgressDialog progDialog = new ProgressDialog("Refreshing templates");
                progDialog.Show();
                //await Templates.RefreshCFTemplates();
                await ContactsAndFaults.RefreshCFTemplates();
                progDialog.Hide();

                FrameworkApplication.State.Activate("datasource_selected");
                FrameworkApplication.State.Deactivate("cfsymbols_generated");
            }
        }

    }
}
