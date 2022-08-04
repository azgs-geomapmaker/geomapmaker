using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class AnyStandaloneTable
    {
        /// <summary>
        /// Get the number of tables by name
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <returns>Returns the number of tables</returns>
        public static int GetTableCount(string tableName)
        {
            // Check for active map
            if (MapView.Active == null)
            {
                return 0;
            }

            IEnumerable<StandaloneTable> tables = MapView.Active?.Map.StandaloneTables.Where(a => a.Name == tableName);

            return tables.Count();
        }

        /// <summary>
        /// Check if standalone table exists
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <returns>Returns true if table exists</returns>
        public static async Task<bool> DoesTableExistsAsync(string tableName)
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

            // Check for underyling table
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

        /// <summary>
        /// Check if a standalone table has the required fields.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="requiredFields">List of fields to check</param>
        /// <returns>Returns list of missing fields</returns>
        public static async Task<List<string>> GetMissingFieldsAsync(string tableName, List<string> requiredFields)
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
                        List<Field> tableFields = table.GetDefinition()?.GetFields()?.ToList();

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
        /// <param name="tableName">Name of table</param>
        /// <param name="fieldsToCheck">List of fields to check</param>
        /// <returns>List of required fields with a null value</returns>
        public static async Task<List<string>> GetRequiredFieldIsNullAsync(string tableName, List<string> fieldsToCheck, string whereClause = "")
        {
            List<string> fieldsWithNull = new List<string>();

            // Check for active map
            if (MapView.Active == null)
            {
                // Missing all required fields
                return fieldsWithNull;
            }

            // Avoid any errors from trying to check fields that don't exist 
            List<string> missingFields = await GetMissingFieldsAsync(tableName, fieldsToCheck);
            if (missingFields.Count > 0)
            {
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
                            SubFields = string.Join(",", fieldsToCheck),

                            WhereClause = whereClause
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

        /// <summary>
        /// Find duplicate values in a standalone table
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <param name="fieldName">Name of field</param>
        /// <param name="whereClause">Where-clause</param>
        /// <returns>Returns list of duplicate values</returns>
        public static async Task<List<string>> GetDuplicateValuesInFieldAsync(string tableName, string fieldName, string whereClause = "")
        {
            List<string> allValues = new List<string>();

            // Check for active map
            if (MapView.Active == null)
            {
                return allValues;
            }

            // Get standalone table by name
            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            // Check if the table was null
            if (standaloneTable == null)
            {
                return allValues;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    // Underlying table found
                    if (table != null)
                    {
                        // Get fields for the table
                        List<Field> tableFields = table.GetDefinition()?.GetFields()?.ToList();

                        // Check if the table has the field
                        if (tableFields.Any(a => a.Name.ToLower() == fieldName.ToLower()))
                        {
                            QueryFilter queryFilter = new QueryFilter
                            {
                                SubFields = fieldName,
                                WhereClause = whereClause
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        // Check if value is empty or null
                                        if (!string.IsNullOrWhiteSpace(row[fieldName]?.ToString()))
                                        {
                                            // Add field to list
                                            allValues.Add(row[fieldName].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            // Return duplicates
            return allValues.GroupBy(a => a).Where(b => b.Count() > 1).Select(c => c.Key).ToList();
        }

        /// <summary>
        /// Get dictinct values for a specific field from a standalone table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>List of distinct values</returns>
        public static async Task<List<string>> GetDistinctValuesForFieldAsync(string tableName, string fieldName)
        {
            List<string> uniqueValues = new List<string>();

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            if (standaloneTable != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = standaloneTable.GetTable())
                    {
                        if (table != null)
                        {
                            // Get fields for the table
                            List<Field> tableFields = table.GetDefinition()?.GetFields()?.ToList();

                            // Check if the table has the field
                            if (tableFields.Any(a => a.Name.ToLower() == fieldName.ToLower()))
                            {
                                QueryFilter queryFilter = new QueryFilter
                                {
                                    PrefixClause = "DISTINCT",
                                    // Where not null or empty
                                    WhereClause = $"{fieldName} <> ''",
                                    SubFields = fieldName
                                };

                                using (RowCursor rowCursor = table.Search(queryFilter))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            string fieldValue = row[fieldName]?.ToString();

                                            uniqueValues.Add(fieldValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return uniqueValues;
        }

        public static async Task<string> GetValueFromWhereClauseAsync(string tableName, string whereClause, string returnField)
        {
            string returnValue = "";

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            if (standaloneTable != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = standaloneTable.GetTable())
                    {
                        if (table != null)
                        {
                            // Get fields for the table
                            List<Field> tableFields = table.GetDefinition()?.GetFields()?.ToList();

                            // Check if the table has the return field
                            if (tableFields.Any(a => a.Name.ToLower() == returnField.ToLower()))
                            {
                                QueryFilter queryFilter = new QueryFilter
                                {
                                    WhereClause = whereClause,
                                };

                                try
                                {
                                    using (RowCursor rowCursor = table.Search(queryFilter))
                                    {
                                        while (rowCursor.MoveNext())
                                        {
                                            using (Row row = rowCursor.Current)
                                            {
                                                returnValue = row[returnField]?.ToString();
                                                return;
                                            }
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "Something went wrong.");
                                }
                            }
                        }
                    }
                });
            }

            return returnValue;
        }

        public static async Task<int> SetPrimaryKeys(string tableName, string fieldName)
        {
            int count = 0;

            List<string> uniqueValues = new List<string>();

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            if (standaloneTable != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = standaloneTable.GetTable())
                    {
                        if (table != null)
                        {
                            // Get fields for the table
                            List<Field> tableFields = table.GetDefinition()?.GetFields()?.ToList();

                            // Check if the table has the field
                            if (tableFields.Any(a => a.Name.ToLower() == fieldName.ToLower()))
                            {
                                QueryFilter queryFilter = new QueryFilter
                                {
                                    // Where field is null or empty
                                    WhereClause = $"{fieldName} = '' or {fieldName} is null",
                                    SubFields = fieldName
                                };

                                EditOperation editOperation = new EditOperation()
                                {
                                    Name = $"Set Primary Keys for {tableName}"
                                };

                                editOperation.Callback(context =>
                                {
                                    using (RowCursor rowCursor = table.Search(queryFilter, false))
                                    {
                                        while (rowCursor.MoveNext())
                                        {
                                            using (Row row = rowCursor.Current)
                                            {
                                                // Increment
                                                count++;

                                                // In order to update the Map and/or the attribute table.
                                                // Has to be called before any changes are made to the row.
                                                context.Invalidate(row);

                                                // Assign a new guid
                                                row[fieldName] = Guid.NewGuid().ToString();

                                                // After all the changes are done, persist it.
                                                row.Store();

                                                // Has to be called after the store too.
                                                context.Invalidate(row);
                                            }
                                        }
                                    }
                                }, table);

                                bool result = editOperation.Execute();
                            }
                        }
                    }
                });
            }

            return count;
        }
    }
}
