using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class GeomapmakerValidation
    {
        public static async Task<Dictionary<string, List<string>>> GetErrorsAsync()
        {
            Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>
            {
                { "Symbology", await GetSymbologyErrorsAsync() },
                { "DataSources", new List<string>() },
                { "DescriptionOfMapUnits", await GetDescriptionOfMapUnitsErrorsAsync() },
                { "Glossary", new List<string>() },
                { "GeoMaterialDict", new List<string>() },
                { "MapUnitPolys", new List<string>() },
                { "ContactsAndFaults", new List<string>() },
                { "Stations", new List<string>() },
                { "OrientationPoints", new List<string>() }
            };

            return _validationErrors;
        }

        private static async Task<List<string>> GetSymbologyErrorsAsync()
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("Symbology") == false)
            {
                errors.Add("Table not found: Symbology");
            }
            else // Table was found
            {
                //
                // Check for duplicate tables
                //
                int tableCount = General.StandaloneTableCount("Symbology");
                if (tableCount > 1)
                {
                    errors.Add($"Multiple Symbology tables found");
                }

                //
                // Check for any missing fields
                //

                // List of required fields
                List<string> symbologyRequiredFields = new List<string>() { "type", "key_", "description", "symbol" };

                // Get missing fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("Symbology", symbologyRequiredFields);
                // Add errors for any missing fields
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for any missing CF symbols
                //
                List<string> missingCFsymbols = await Symbology.GetMissingContactsAndFaultsSymbologyAsync();
                foreach (string key in missingCFsymbols)
                {
                    errors.Add($"Missing line symbology: {key}");
                }

                //
                // Check for any missing OP symbols
                //
                List<string> missingOPsymbols = await Symbology.GetMissingOrientationPointsSymbologyAsync();
                foreach (string key in missingOPsymbols)
                {
                    errors.Add($"Missing point symbology: {key}");
                }
            }

            return errors;
        }

        private static async Task<List<string>> GetDescriptionOfMapUnitsErrorsAsync()
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("DescriptionOfMapUnits"))
            {
                //
                // Check table for missing toolbar fields
                //

                // List of required fields to check
                List<string> dmuRequiredFields = new List<string>() { "relativeage", "hexcolor" };

                // Get misssing required fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("DescriptionOfMapUnits", dmuRequiredFields);
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }
            }

            return errors;
        }
    }
}
