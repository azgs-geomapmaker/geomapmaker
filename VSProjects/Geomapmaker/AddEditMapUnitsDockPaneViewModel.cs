using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Geomapmaker.Models;

namespace Geomapmaker {
	internal class AddEditMapUnitsDockPaneViewModel : DockPane {
		private const string _dockPaneID = "Geomapmaker_AddEditMapUnitsDockPane";

		protected  AddEditMapUnitsDockPaneViewModel() { 
			Debug.WriteLine("AddEditMapUnitsDockPaneViewModel constructor enter");
		}

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

		internal static void Hide() {
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;
			pane.Hide();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private const string GENERIC_HEADING = "Add/Edit Map Units";
		private const string ADD_HEADING = "Add Map Unit";
		public const string EDIT_HEADING = "Edit Map Unit";
		private string _heading = GENERIC_HEADING;
		public string Heading {
			get { return _heading; }
			set {
				SetProperty(ref _heading, value, () => Heading);
			}
		}
		private string subHeading = GENERIC_HEADING;
		public string SubHeading {
			get { return subHeading; }
			set {
				SetProperty(ref subHeading, value, () => SubHeading);
			}
		}

		private MapUnit selectedMapUnit;
		public MapUnit SelectedMapUnit {
			get => selectedMapUnit;
			set {
				if (value == null) return;
				//selectedMapUnit = value;
				SetProperty(ref selectedMapUnit, value, () => SelectedMapUnit); //Have to do this to trigger stuff, I guess.

				//Set heading and populate form, depending on whether this is a new map unit or not 
				//if (!DataHelper.MapUnits.Any(p => p.Text == value)) {
				if (!DataHelper.MapUnits.Any(mu => mu.MU == value.MU)) {
					SubHeading = ADD_HEADING;
					//populate form with defaults
				} else {
					SubHeading = EDIT_HEADING;
					// populate form from db

				}
			}
		}

		//This is to capture the user entering a new map unit (not in the list)
		private string userEnteredMapUnit;
		public string UserEnteredMapUnit {
			get {
				return userEnteredMapUnit;
			}
			set {
				if (userEnteredMapUnit != value) {
					userEnteredMapUnit = value;
					var mapUnit = new MapUnit();
					mapUnit.MU = userEnteredMapUnit;
					SelectedMapUnit = mapUnit;
				}
			}
		}
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
