﻿using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    public class HeadingsViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateHeadingVM Create { get; set; }
        public EditHeadingVM Edit { get; set; }
        public DeleteHeadingVM Delete { get; set; }

        public HeadingsViewModel()
        {
            Create = new CreateHeadingVM(this);
            Edit = new EditHeadingVM(this);
            Delete = new DeleteHeadingVM(this);
        }

        private List<MapUnit> _mapUnits { get; set; }
        public List<MapUnit> MapUnits
        {
            get => _mapUnits;
            set
            {
                _mapUnits = value;
                NotifyPropertyChanged();
            }
        }

        private List<MapUnit> _headings { get; set; }
        public List<MapUnit> Headings
        {
            get => _headings;
            set
            {
                _headings = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of dmu
        public async void RefreshMapUnitsAsync()
        {
            MapUnits = await Data.DescriptionOfMapUnits.GetMapUnitsAsync();
            Headings = MapUnits.Where(a => string.IsNullOrEmpty(a.MU)).OrderBy(a => a.Name).ToList();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowHeadings : Button
    {
        private Views.Headings.Headings _headings = null;

        protected override void OnClick()
        {
            if (_headings != null)
            {
                _headings.Close();
                return;
            }

            _headings = new Views.Headings.Headings
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _headings.headingsVM.RefreshMapUnitsAsync();

            _headings.Closed += (o, e) => { _headings = null; };

            _headings.headingsVM.WindowCloseEvent += (s, e) => _headings.Close();

            _headings.Show();
        }
    }
}