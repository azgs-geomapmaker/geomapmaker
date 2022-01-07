using Geomapmaker.ViewModels.ContactsFaults;
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
