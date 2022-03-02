﻿using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace Geomapmaker.ViewModels.DataSources
{
    public class DataSourcesViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

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
        public async void RefreshDataSourcesAsync()
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

        protected override void OnClick()
        {
            if (_datasources != null)
            {
                _datasources.Close();
                return;
            }

            _datasources = new Views.DataSources.DataSources
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _datasources.dataSourcesVM.RefreshDataSourcesAsync();

            _datasources.Closed += (o, e) => { _datasources = null; };

            _datasources.dataSourcesVM.WindowCloseEvent += (s, e) => _datasources.Close();

            _datasources.Show();
        }
    }
}
