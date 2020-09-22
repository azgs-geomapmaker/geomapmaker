using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	/// Interaction logic for AddEditMapUnitsDockPaneView.xaml
	/// </summary>
	public partial class AddEditMapUnitsDockPaneView : UserControl {
		public AddEditMapUnitsDockPaneView() {
			InitializeComponent();
		}


		/** I'm using a hybrid (half-assed?) MVVM approach and putting some handlers here in the code-behind
		 * because have you seen the contortions necessary to implement button handlers in MVVM?
		 * Someday, take a look at https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/may/mvvm-commands-relaycommands-and-eventtocommand
		 * for a start. In the meantime, I'm grabbing a handle on the view model here when I need it (gasp!) and going for it.
		 **/
		/*
		private void ResetButton_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("Reset");
			var vm = (AddEditMapUnitsDockPaneViewModel)this.DataContext;
			vm.SelectedMapUnit = null;
			//vm.UserEnteredMapUnit = null;
		}

		private async void SubmitButton_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("Submit");
			var vm = (AddEditMapUnitsDockPaneViewModel)this.DataContext;
			await vm.saveMapUnit(vm.SelectedMapUnit);
			MapUnitTextBox.SelectedIndex = -1;
			MapUnitTextBox.Text = null;
		}
		*/

		private async void NewCommandHandler(object sender, ExecutedRoutedEventArgs e) {
			// Calls a method to close the file and release resources.
			var vm = (AddEditMapUnitsDockPaneViewModel)this.DataContext;
			vm.SelectedMapUnit = null;
		}

		private async void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e) {
			// Calls a method to close the file and release resources.
			var vm = (AddEditMapUnitsDockPaneViewModel)this.DataContext;
			await vm.saveMapUnit(vm.SelectedMapUnit);
		}

		private void SaveCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e) {
			// Call a method to determine if there is a file open.
			// If there is a file open, then set CanExecute to true.
			var vm = (AddEditMapUnitsDockPaneViewModel)this.DataContext;
			if (vm != null && vm.IsValid) {
				e.CanExecute = true;
			}
			// if there is not a file open, then set CanExecute to false.
			else {
				e.CanExecute = false;
			}
		}

		/*
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Debug.WriteLine("ComboBox_SelectionChanged enter, " + e.ToString());
		}
		*/
	}
}
