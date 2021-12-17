using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;

namespace Geomapmaker.RibbonElements
{
    internal class ContactsAndFaultsEditTool : MapTool
    {
        AddEditContactsAndFaultsDockPaneViewModel ContactAndFaultsVM { get; set; }

        public ContactsAndFaultsEditTool()
        {
            ContactAndFaultsVM = GeomapmakerModule.ContactsAndFaultsVM;

            GeomapmakerModule.ContactsAndFaultsEditTool = this;
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            UseSelection = false;
        }

        public void Clear()
        {
            ClearSketchAsync();
            UseSelection = false;
            SketchType = SketchGeometryType.Point;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            AddEditContactsAndFaultsDockPaneViewModel.Show();
            ContactAndFaultsVM.Heading = "Edit Map Contacts and Faults";
            return base.OnToolActivateAsync(active);
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            AddEditContactsAndFaultsDockPaneViewModel.Hide();
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mv = MapView.Active;
            var identifyResult = await QueuedTask.Run(() =>
            {

                //mv.SelectFeatures(geometry);

                if (UseSelection == false)
                {
                    //User just selected a cf to edit. Populate form and set up to allow geom editing

                    // Get the features that intersect the sketch geometry.
                    var features = mv.GetFeatures(geometry);
                    //Only interested in MapUnitPolys
                    var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");
                    if (cfFeatures.Count() > 0)
                    {
                        //Get cf objectid
                        //TODO: I am only pulling the first from the list. Might need to present some sort of selector to the user. 
                        if (cfFeatures.First().Value.Count() > 0)
                        {
                            var cfID = cfFeatures.First().Value.First();

                            //Using the objectid, get the cf record from the database
                            using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                            {
                                using (Table cfTable = geodatabase.OpenDataset<Table>("ContactsAndFaults"))
                                {
                                    QueryFilter queryFilter = new QueryFilter
                                    {
                                        WhereClause = "objectid = " + cfID
                                    };
                                    var cf = new CF();
                                    using (RowCursor rowCursor = cfTable.Search(queryFilter, false))
                                    {
                                        if (rowCursor.MoveNext())
                                        {
                                            using (Row row = rowCursor.Current)
                                            {
                                                cf.ID = Int32.Parse(Convert.ToString(row["objectid"]));
                                                cf.symbol = DataHelper.CFSymbols.Where(x => x.key == Convert.ToString(row["symbol"])).First();
                                                //cf.description = Convert.ToString(row["description"]);
                                                //cf.symbol = Convert.ToString(row["symbol"]);
                                                cf.Notes = Convert.ToString(row["notes"]);
                                                cf.IdentityConfidence = Convert.ToString(row["identityconfidence"]);
                                                cf.ExistenceConfidence = Convert.ToString(row["existenceconfidence"]);
                                                cf.LocationConfidenceMeters = Convert.ToString(row["locationconfidencemeters"]);
                                                cf.IsConcealed = Convert.ToString(row["isconcealed"]) == "Y";
                                                cf.DataSource = Convert.ToString(row["datasourceid"]);
                                                cf.Shape = (Geometry)row["shape"]; //TODO: no idea
                                            }
                                        }

                                    }

                                    // Use the map unit to populate the view model for the form
                                    ContactAndFaultsVM.SelectedCF = cf;
                                    ContactAndFaultsVM.SelectedCFSymbol = cf.symbol;

                                    //TODO: The line below forces a refresh on the object the UI is bound to for shape. But other than that,
                                    //it's redundant. There should be a better way to handle this.
                                    ContactAndFaultsVM.Shape = ContactAndFaultsVM.Shape;

                                    UseSelection = true;
                                    SketchType = SketchGeometryType.Line;
                                    ContextToolbarID = "";
                                    SetCurrentSketchAsync(cf.Shape);

                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    //User has just modified the geometery, stick it on the form
                    ContactAndFaultsVM.Shape = geometry;
                    SetCurrentSketchAsync(geometry);
                    //UseSelection = false;
                    //SketchType = SketchGeometryType.Point;
                    //TODO: more stuff?
                    return true;
                }
            });
            //MessageBox.Show(identifyResult);
            return true;
        }
    }
}
