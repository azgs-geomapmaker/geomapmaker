using Geomapmaker.ViewModels.Glossary;

namespace Geomapmaker.Views.Glossary
{
    /// <summary>
    /// Interaction logic for Glossary.xaml
    /// </summary>
    public partial class Glossary : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GlossaryVM glossaryVM = new GlossaryVM();

        public Glossary()
        {
            InitializeComponent();
            DataContext = glossaryVM;
        }
    }
}
