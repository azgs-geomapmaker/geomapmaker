using Geomapmaker.ViewModels.Validation;

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
