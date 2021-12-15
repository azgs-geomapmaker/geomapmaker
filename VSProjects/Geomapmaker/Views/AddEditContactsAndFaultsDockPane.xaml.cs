using System.Windows.Controls;

namespace Geomapmaker
{
    /// <summary>
    /// Interaction logic for AddEditContactsAndFaultsDockPaneView.xaml
    /// </summary>
    public partial class AddEditContactsAndFaultsDockPaneView : UserControl
    {
        public AddEditContactsAndFaultsDockPaneViewModel contactsAndFaultsVM = new AddEditContactsAndFaultsDockPaneViewModel();

        public AddEditContactsAndFaultsDockPaneView()
        {
            InitializeComponent();
            DataContext = contactsAndFaultsVM;
        }
    }
}
