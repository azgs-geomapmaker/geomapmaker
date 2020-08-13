using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Geomapmaker {
	/// <summary>
	/// Represents the ComboBox
	/// </summary>
	internal class MapUnitsComboBox : ComboBox {
		NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=geomapmaker; " +
				   "Password=password;Database=geomapmaker;");

		private bool _isInitialized;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public MapUnitsComboBox() {
			//UpdateCombo();
			DataHelper.ProjectSelectedHandler += onProjectSelected;
		}

		void onProjectSelected() {
			_isInitialized = false;
			UpdateCombo();
		}


		/// <summary>
		/// Updates the combo box with all the items.
		/// </summary>

		private async void UpdateCombo() {
			// TODO – customize this method to populate the combobox with your desired items  
			//if (_isInitialized)
			//	SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


			if (!_isInitialized) {
                Clear();
				await openDatabase();
                /*
                //Add 6 items to the combobox
                for (int i = 0; i < 6; i++) {
					string name = string.Format("Item {0}", i);
					Add(new ComboBoxItem(name));
				}
                */
				_isInitialized = true;
			}


			Enabled = true; //enables the ComboBox
			SelectedItem = null; // ItemCollection.FirstOrDefault(); //set the default item in the comboBox

		}

		/// <summary>
		/// The on comboBox selection change event. 
		/// </summary>
		/// <param name="item">The newly selected combo box item</param>
		protected override void OnSelectionChange(ComboBoxItem item) {

			if (item == null)
				return;

			if (string.IsNullOrEmpty(item.Text))
				return;

			// TODO  Code behavior when selection changes.  
			if (SelectedIndex == -1) {
				MessageBox.Show("Add Map Unit not yet implemented");
			} else {
				MessageBox.Show("Edit Map Unit not yet implemented");
			}
		}

		//The Arc Pro ComboBox has no way to sense when a user has entered a new value. There is OnTextChanged, but this triggers 
		//for each character. Or there is this. It's clunky and sometimes causes ghost events. I'm not too worried about this though, 
		//because we intend to move this combobox into the add/edit dialog, at which point I can switch to a vanilla Windows combobox,
		//which registers an Enter as a selection change.
		protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e) {
			MessageBox.Show("Add Map Unit not yet implemented");
		}
		/*
		protected override void OnTextChange(String text) {
			if (text == null)
				return;

			// TODO  Code behavior when selection changes.   
			MessageBox.Show("Add Map Unit not yet implemented");

		}
		*/

		private async Task openDatabase() {
			if (DataHelper.connectionProperties == null) {
				return;
			}

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {

                    //Table t = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits");
                    QueryDef mapUnitsQDef = new QueryDef {
                        Tables = "DescriptionOfMapUnits",
                        //WhereClause = "ADACOMPLY = 'Yes'",
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false)) {
                        while (rowCursor.MoveNext()) {
                            using (Row row = rowCursor.Current) {
                                Debug.WriteLine(row["Name"].ToString());
                                Add(new ComboBoxItem(row["Name"].ToString()));
                            }
                        }
                    }

                }
            });
        }

    }
}
