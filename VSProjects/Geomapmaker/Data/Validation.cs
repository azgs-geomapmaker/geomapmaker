using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Validation
    {
        public static bool FeatureLayerExists(string layerName)
        {
            if (MapView.Active == null)
            {
                return false;
            }

            Layer layer = MapView.Active?.Map.FindLayers(layerName).FirstOrDefault();

            return layer != null;
        }

        public static async Task<bool> StandaloneTableExistsAsync(string tableName)
        {
            // Check for active map
            if (MapView.Active == null)
            {
                return false;
            }

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            // Check if table was null
            if (standaloneTable == null)
            {
                return false;
            }

            // Check for the underyling table
            bool underlyingTableExists = false;

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    if (table != null)
                    {
                        underlyingTableExists = true;
                    }
                }
            });

            return underlyingTableExists;
        }

        public static async Task<List<string>> StandaloneTableFieldsExistAsync(string tableName, List<string> requiredFields)
        {
            List<string> missingFields = new List<string>();

            // Check for active map
            if (MapView.Active == null)
            {
                // Missing all required fields
                return requiredFields;
            }

            // Get standalone table by name
            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            // Check if table was null
            if (standaloneTable == null)
            {
                // Missing all required fields
                return requiredFields;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    // Underlying table not found
                    if (table == null)
                    {
                        // All fields are missing
                        missingFields = requiredFields;
                    }
                    else
                    {
                        // Get table fields
                        List<Field> tableFields = table.GetDefinition().GetFields().ToList();

                        foreach (string field in requiredFields)
                        {
                            // Check if the field exists. Lowercase both names for case-insensitive check
                            if (!tableFields.Any(a => a.Name.ToLower() == field.ToLower()))
                            {
                                // Add to list of missing fields
                                missingFields.Add(field);
                            }
                        }
                    }
                }
            });

            return missingFields;
        }

        /// <summary>
        /// Verify the required fields are not null
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="requiredFields"></param>
        /// <returns>List of required fields with a null value</returns>
        public static async Task<List<string>> StandaloneTableRequiredNullCountAsync(string tableName, List<string> fieldsToCheck)
        {
            List<string> fieldsWithNull = new List<string>();

            // Check for active map
            if (MapView.Active == null)
            {
                // Missing all required fields
                return fieldsWithNull;
            }

            // Get standalone table by name
            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            // Check if the table was null
            if (standaloneTable == null)
            {
                // Missing all required fields
                return fieldsWithNull;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    // Underlying table found
                    if (table != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            // Limit the query to just the fields we need to check
                            // Join the fields as a comma-delimited string
                            SubFields = string.Join(",", fieldsToCheck) 
                        };

                        using (RowCursor rowCursor = table.Search(queryFilter))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    foreach (string field in fieldsToCheck)
                                    {
                                        // Check if value is empty or null
                                        if (row[field] == null || string.IsNullOrEmpty(row[field].ToString()))
                                        {
                                            // Check if the field is already in the list
                                            if (!fieldsWithNull.Contains(field))
                                            {
                                                // Add field to list
                                                fieldsWithNull.Add(field);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return fieldsWithNull;
        }
    }
}
