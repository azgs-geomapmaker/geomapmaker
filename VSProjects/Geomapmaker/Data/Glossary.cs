using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Glossary
    {
        /// <summary>
        /// Check if the DataSources table exists
        /// </summary>
        /// <returns>Returns true if the table exists</returns>
        public static async Task<bool> TableExistsAsync()
        {
            return await General.StandaloneTableExistsAsync("Glossary");
        }

        /// <summary>
        /// Check the table for any missing fieldss
        /// </summary>
        /// <returns>Returns a list of fieldnames missing from the table</returns>
        public static async Task<List<string>> GetMissingFieldsAsync()
        {
            // List of fields to check for
            List<string> requiredFields = new List<string>() { "term", "definition", "definitionsourceid", "glossary_id" };

            return await General.StandaloneTableFieldsExistAsync("Glossary", requiredFields);
        }

        /// <summary>
        /// Check the required fields for any missing values.
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value</returns>
        public static async Task<List<string>> GetRequiredFieldsWithNullValues()
        {
            List<string> fieldsToCheck = new List<string>() { "term", "definition", "definitionsourceid", "glossary_id" };

            return await General.StandaloneTableRequiredFieldIsNullAsync("Glossary", fieldsToCheck);
        }

    }
}
