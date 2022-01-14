using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class CFSymbols
    {
        public static CIMUniqueValueRenderer cfRenderer;

        public static async Task<List<string>> GetContactsAndFaultsSymbolsAsync()
        {
            List<string> cfInFeatureClass = new List<string>();

            if (!(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "ContactsAndFaults") is FeatureLayer cfFeatureLayer))
            {
                return cfInFeatureClass;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Table enterpriseTable = cfFeatureLayer.GetTable();

                using (RowCursor rowCursor = enterpriseTable.Search())
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            cfInFeatureClass.Add(row["symbol"].ToString());
                        }
                    }
                }

            });

            return cfInFeatureClass;
        }

        public static async Task<List<CFSymbol>> GetCFSymbolList()
        {
            List<string> cfInFeatureClass = await GetContactsAndFaultsSymbolsAsync();

            List<CFSymbol> cfSymbols = new List<CFSymbol>();

            StandaloneTable CFSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "cfsymbology");

            if (CFSymbologyTable == null)
            {
                return cfSymbols;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Table enterpriseTable = CFSymbologyTable.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    PostfixClause = "ORDER BY key"
                };

                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                {
                    List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();

                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            // Create and load map unit
                            CFSymbol cfS = new CFSymbol
                            {
                                Key = row["key"].ToString(),
                                Description = row["description"] == null ? "" : row["description"].ToString(),
                                SymbolJson = row["symbol"].ToString()
                            };

                            // TODO: preprocess all this stuff
                            cfS.SymbolJson = cfS.SymbolJson.Replace("'", "\"");

                            // Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
                            cfS.SymbolJson = cfS.SymbolJson.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
                            cfS.SymbolJson = cfS.SymbolJson.Insert(cfS.SymbolJson.Length, "}");

                            // Create the preview image used in the ComboBox
                            SymbolStyleItem sSI = new SymbolStyleItem()
                            {
                                Symbol = CIMSymbolReference.FromJson(cfS.SymbolJson).Symbol,
                                PatchWidth = 50,
                                PatchHeight = 25
                            };
                            cfS.Preview = sSI.PreviewImage;

                            // Add it to our list
                            cfSymbols.Add(cfS);

                            // Only add to renderer if present in the feature class
                            if (cfInFeatureClass.Contains(cfS.Key))
                            {
                                // Create a "CIMUniqueValueClass" for the cf and add it to the list of unique values.
                                // This is what creates the mapping from cf derived attribute to symbol
                                List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
                                        new CIMUniqueValue {
                                            FieldValues = new string[] { cfS.Key }
                                        }
                                    };

                                CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                                {
                                    Editable = true,
                                    Label = cfS.Key,
                                    Patch = PatchShape.AreaPolygon,
                                    Symbol = CIMSymbolReference.FromJson(cfS.SymbolJson, null),
                                    Visible = true,
                                    Values = listUniqueValues.ToArray()
                                };
                                listUniqueValueClasses.Add(uniqueValueClass);
                            }
                        }

                        //Create a list of CIMUniqueValueGroup
                        CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                        {
                            Classes = listUniqueValueClasses.ToArray(),
                        };
                        List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

                        //Use the list to create the CIMUniqueValueRenderer
                        cfRenderer = new CIMUniqueValueRenderer
                        {
                            UseDefaultSymbol = false,
                            Groups = listUniqueValueGroups.ToArray(),
                            Fields = new string[] { "symbol" }
                        };

                        if ((MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "ContactsAndFaults") is FeatureLayer cfLayer))
                        {
                            cfLayer.SetRenderer(cfRenderer);
                        }
                    }
                }
            });

            return cfSymbols;
        }
    }
}
