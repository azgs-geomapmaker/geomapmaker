using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace Geomapmaker {
	internal class AddEditMapUnitsDockPaneViewModel : DockPane {
		private const string _dockPaneID = "Geomapmaker_AddEditMapUnitsDockPane";

		protected AddEditMapUnitsDockPaneViewModel() { }

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show() {
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			Debug.WriteLine("VM.Show, mapUnitName = " + DataHelper.MapUnitName);
			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Add/Edit Map Units";
		public string Heading {
			get { return _heading; }
			set {
				SetProperty(ref _heading, value, () => Heading);
			}
		}

		/*
		//private string mapUnitName;
		public string MapUnitName {
			get {
				Debug.WriteLine("property MapUnitName, returning " + DataHelper.mapUnitName);
				return DataHelper.mapUnitName; 
			}
			set {
				SetProperty(ref DataHelper.mapUnitName, value, () => MapUnitName);
			}
		}
		*/

	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class AddEditMapUnitsDockPane_ShowButton : Button {
		protected override void OnClick() {
			AddEditMapUnitsDockPaneViewModel.Show();
		}
	}
}
