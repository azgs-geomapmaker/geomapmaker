using Geomapmaker.ViewModels.ContactsFaults;

namespace Geomapmaker.Views.ContactsFaults
{
    /// <summary>
    /// Interaction logic for ContactsFaults.xaml
    /// </summary>
    public partial class ContactsFaults : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ContactsFaultsViewModel contactsFaultsVM = new ContactsFaultsViewModel();

        public ContactsFaults()
        {
            InitializeComponent();
            DataContext = contactsFaultsVM;
        }
    }
}
