﻿using System;
using System.Collections.Generic;
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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace Geomapmaker {
	internal class DataSourceButton : Button {

		private DataSourceDialogProWindow _datasourcedialogprowindow = null;

		protected override void OnClick() {
			//already open?
			if (_datasourcedialogprowindow != null)
				return;
			_datasourcedialogprowindow = new DataSourceDialogProWindow();
			_datasourcedialogprowindow.Owner = FrameworkApplication.Current.MainWindow;
			_datasourcedialogprowindow.Closed += (o, e) => { _datasourcedialogprowindow = null; };
			//_datasourcedialogprowindow.Show();
			//uncomment for modal
			_datasourcedialogprowindow.ShowDialog();
		}

	}
}
