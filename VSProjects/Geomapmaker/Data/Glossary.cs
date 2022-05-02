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
    }
}
