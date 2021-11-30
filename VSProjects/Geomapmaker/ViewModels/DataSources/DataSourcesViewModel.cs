using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using Geomapmaker.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Geomapmaker.ViewModels.DataSources
{
    public class DataSourcesViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public CreateDataSourceVM Create { get; set; }
        public EditDataSourceVM Edit { get; set; }
        public DeleteDataSourceVM Delete { get; set; }

        public DataSourcesViewModel()
        {
            Create = new CreateDataSourceVM(this);
            Edit = new EditDataSourceVM(this);
            Delete = new DeleteDataSourceVM(this);
        }

        private List<DataSource> _dataSources { get; set; }
        public List<DataSource> DataSources
        {
            get => _dataSources;
            set
            {
                _dataSources = value;
                NotifyPropertyChanged();
            }
        }

        // Update collection of data sources
        public async Task RefreshDataSourcesAsync()
        {
            DataSources = await Data.DataSources.GetDataSourcesAsync();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowDataSources : Button
    {
        private Views.DataSources.DataSources _datasources = null;

        protected override async void OnClick()
        {
            if (_datasources != null)
            {
                return;
            }

            _datasources = new Views.DataSources.DataSources
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            await _datasources.dataSourcesVM.RefreshDataSourcesAsync();

            _datasources.Closed += (o, e) => { _datasources = null; };

            _datasources.Show();
        }
    }
}
