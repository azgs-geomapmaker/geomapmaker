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


namespace Geomapmaker
{
    /// <summary>
    /// Interaction logic for AddEditContactsAndFaultsDockPaneView.xaml
    /// </summary>
    public partial class AddEditContactsAndFaultsDockPaneView : UserControl
    {
        public AddEditContactsAndFaultsDockPaneView()
        {
            InitializeComponent();
        }

        private void NewCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // Calls a method to close the file and release resources.
            var vm = (AddEditContactsAndFaultsDockPaneViewModel)this.DataContext;
            vm.Reset();
        }

        private async void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // Calls a method to close the file and release resources.
            var vm = (AddEditContactsAndFaultsDockPaneViewModel)this.DataContext;
            await vm.saveCF();
            vm.Reset();
            //ContactsAndFaultsComboBox.SelectedIndex = -1;
        }

        private void SaveCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            // Call a method to determine if there is a file open.
            // If there is a file open, then set CanExecute to true.
            var vm = (AddEditContactsAndFaultsDockPaneViewModel)this.DataContext;
            if (vm != null && vm.IsValid)
            {
                e.CanExecute = true;
            }
            // if there is not a file open, then set CanExecute to false.
            else
            {
                e.CanExecute = false;
            }
        }

    }
}
