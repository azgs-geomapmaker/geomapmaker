using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Geomapmaker {
	/// <summary>
	/// Interaction logic for AddEditMapUnitPolysDockPaneView.xaml
	/// </summary>
	public partial class AddEditMapUnitPolysDockPaneView : UserControl {
		public AddEditMapUnitPolysDockPaneView() {
			InitializeComponent();
		}

		private async void NewCommandHandler(object sender, ExecutedRoutedEventArgs e) {
			// Calls a method to close the file and release resources.
			var vm = (AddEditMapUnitPolysDockPaneViewModel)this.DataContext;
			vm.SelectedMapUnitPoly = new MapUnitPoly();
			vm.SelectedMapUnit = null;
			vm.SelectedMapUnitPoly.Shape = null;
		}

		private async void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e) {
			// Calls a method to close the file and release resources.
			var vm = (AddEditMapUnitPolysDockPaneViewModel)this.DataContext;
			await vm.saveMapUnitPoly(/*vm.SelectedMapUnit*/);
			vm.SelectedMapUnitPoly = new MapUnitPoly();
			vm.SelectedMapUnit = null;
			vm.SelectedMapUnitPoly.Shape = null;
		}

		private void SaveCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e) {
			// Call a method to determine if there is a file open.
			// If there is a file open, then set CanExecute to true.
			var vm = (AddEditMapUnitPolysDockPaneViewModel)this.DataContext;
			if (vm != null && vm.IsValid) {
				e.CanExecute = true;
			}
			// if there is not a file open, then set CanExecute to false.
			else {
				e.CanExecute = false;
			}
		}

	}

}
