using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
    internal class TestButton : Button
    {
        protected override async void OnClick()
        {
            var undefinedTerms = await Data.Glossary.GetUndefinedGlossaryTerms();
        }
    }
}
