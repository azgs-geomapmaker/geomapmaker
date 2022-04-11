using Geomapmaker.ViewModels.Validation;
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

namespace Geomapmaker.Views.Validation
{
    /// <summary>
    /// Interaction logic for Validation.xaml
    /// </summary>
    public partial class Validation : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ValidationViewModel validationVM = new ValidationViewModel();

        public Validation()
        {
            InitializeComponent();
            DataContext = validationVM;
        }
    }
}
