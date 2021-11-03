using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace Geomapmaker
{
    internal class DataSourceButton : Button
    {
        private DataSourceDialogProWindow _dswindow = null;

        protected override async void OnClick()
        {
            //already open?
            if (_dswindow != null)
            {
                return;
            }

            using (var progress = new ProgressDialog("Loading Geomapmaker Project"))
            {
                progress.Show();
                await QueuedTask.Run(async () =>
                {
                    await Data.DescriptionOfMapUnits.GetFields();
                    await DataHelper.PopulateMapUnits();
                    await DataHelper.PopulateContactsAndFaults();
                    await DataHelper.PopulateDataSources();
                    await Data.DataSources.RefreshAsync();
                });
                progress.Hide();
            }

            _dswindow = new DataSourceDialogProWindow();
            _dswindow.Owner = FrameworkApplication.Current.MainWindow;
            _dswindow.Closed += (o, e) => { _dswindow = null; };
            //_datasourcedialogprowindow.Show();
            //uncomment for modal
            _dswindow.ShowDialog();
        }

    }
}
