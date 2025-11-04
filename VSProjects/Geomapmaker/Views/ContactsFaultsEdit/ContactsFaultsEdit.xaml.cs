using Geomapmaker.ViewModels.ContactsFaultsEdit;

namespace Geomapmaker.Views.ContactsFaultsEdit {
    /// <summary>
    /// Interaction logic for ContactsFaultsEdit.xaml
    /// </summary>
    public partial class ContactsFaultsEdit : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ContactsFaultsEditVM contactsFaultsEditVM = new ContactsFaultsEditVM();

        public ContactsFaultsEdit()
        {
            InitializeComponent();
            DataContext = contactsFaultsEditVM;
        }
    }
}
