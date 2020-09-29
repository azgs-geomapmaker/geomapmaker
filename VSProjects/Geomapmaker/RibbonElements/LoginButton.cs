using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Npgsql;

namespace Geomapmaker {

    internal class LoginButton : Button {
        private LoginDialogProWindow _logindialogprowindow = null;
        protected override void OnClick() {
            //already open?
            
            if (_logindialogprowindow != null)
                return;
            _logindialogprowindow = new LoginDialogProWindow();
            _logindialogprowindow.Owner = FrameworkApplication.Current.MainWindow;
            _logindialogprowindow.Closed += (o, e) => { _logindialogprowindow = null; };
            _logindialogprowindow.ShowDialog();
            
        }
    }
}
