using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows;

namespace Geomapmaker
{
    internal class DataSourceButton : Button
    {
        protected override async void OnClick()
        {
            if (string.IsNullOrEmpty(DataHelper.connectionProperties.Password))
            {
                MessageBox.Show("Project connection properties is null", "Error");
                return;
            }

            using (ProgressDialog progress = new ProgressDialog("Loading Geomapmaker Project"))
            {
                progress.Show();
                await QueuedTask.Run(async () =>
                {
                    await DataHelper.PopulateMapUnits();
                    await DataHelper.PopulateContactsAndFaults();
                    await DataHelper.PopulateDataSources();
                });
                progress.Hide();
                FrameworkApplication.State.Activate("connectPropsSet");
            }
        }
    }
}
