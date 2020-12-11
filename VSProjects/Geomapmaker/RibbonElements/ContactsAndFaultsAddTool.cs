using System;
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
using ArcGIS.Desktop.Mapping;

namespace Geomapmaker {
	internal class ContactsAndFaultsAddTool : MapTool {
		public ContactsAndFaultsAddTool() {
			GeomapmakerModule.ContactsAndFaultsAddTool = this;
			IsSketchTool = true;
			SketchType = SketchGeometryType.Line;
			SketchOutputMode = SketchOutputMode.Map;
			ContextToolbarID = "";
		}

		public void Clear() {
			ClearSketchAsync();
		}

		protected override Task OnToolActivateAsync(bool active) {
			AddEditContactsAndFaultsDockPaneViewModel.Show();
			GeomapmakerModule.ContactsAndFaultsVM.Heading = "Add Contacts and Faults";
			return base.OnToolActivateAsync(active);
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
			AddEditContactsAndFaultsDockPaneViewModel.Hide();
			GeomapmakerModule.ContactsAndFaultsVM.Reset();
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}


		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			if (geometry == null) {
				return Task.FromResult(false);
			}

			//TODO: This sets the geom. Need to implement save. Then refresh map
			//GeomapmakerModule.ContactsAndFaultsVM.SelectedCF.Shape = geometry;
			GeomapmakerModule.ContactsAndFaultsVM.Shape = geometry;

			//This is a little janky, but it's the only way I have found to persist the poly and keep it editable
			//until the whole map unit is saved.
			//TODO: Is there a better way?
			//StartSketchAsync(); //Looks like setting current is enough
			SetCurrentSketchAsync(geometry);

			return Task.FromResult(false);

			/*
			if (CurrentTemplate == null || geometry == null)
				return Task.FromResult(false);

			// Create an edit operation
			var createOperation = new EditOperation();
			createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
			createOperation.SelectNewFeatures = true;

			// Queue feature creation
			createOperation.Create(CurrentTemplate, geometry);

			// Execute the operation
			return createOperation.ExecuteAsync();
			*/
		}
	}
}
