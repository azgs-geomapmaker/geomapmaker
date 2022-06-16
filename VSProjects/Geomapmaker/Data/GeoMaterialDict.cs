using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class GeoMaterialDict
    {
        /// <summary>
        /// Get validation report for table
        /// </summary>
        /// <returns>List of Validation results</returns>
        public static async Task<List<ValidationRule>> GetValidationResultsAsync()
        {
            List<ValidationRule> results = new List<ValidationRule>
            {
                new ValidationRule{ Description="Table exists."},
                new ValidationRule{ Description="No duplicate tables."},
                new ValidationRule{ Description="No missing fields."},
                new ValidationRule{ Description="No empty/null values in required fields."},
                new ValidationRule{ Description="GeoMaterialDict table has not been modified."},
            };

            if (await AnyStandaloneTable.DoesTableExistsAsync("GeoMaterialDict") == false)
            {
                results[0].Status = ValidationStatus.Failed;
                results[0].Errors.Add("Table not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate tables
                //
                int tableCount = AnyStandaloneTable.GetTableCount("GeoMaterialDict");
                if (tableCount > 1)
                {
                    results[1].Status = ValidationStatus.Failed;
                    results[1].Errors.Add($"{tableCount} tables found");
                }
                else
                {
                    results[1].Status = ValidationStatus.Passed;
                }

                //
                // Check table for any missing fields 
                //

                // List of required fields
                List<string> geoMaterialRequiredFields = new List<string>() { "hierarchykey", "geomaterial", "indentedname", "definition" };

                // Get list of missing required fields
                List<string> missingFields = await AnyStandaloneTable.GetMissingFieldsAsync("GeoMaterialDict", geoMaterialRequiredFields);
                if (missingFields.Count == 0)
                {
                    results[2].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[2].Status = ValidationStatus.Failed;
                    foreach (string field in missingFields)
                    {
                        results[2].Errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for empty/null values in required fields
                //

                // List of fields to check for null values
                List<string> geoMaterialNotNull = new List<string>() { "hierarchykey", "geomaterial", "indentedname" };

                // Check the required fields for any missing values.
                List<string> fieldsWithMissingValues = await AnyStandaloneTable.GetRequiredFieldIsNullAsync("GeoMaterialDict", geoMaterialNotNull);
                if (fieldsWithMissingValues.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string field in fieldsWithMissingValues)
                    {
                        results[3].Errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check if the GeoMaterialDict table was modified
                //
                List<string> modifiedGeoMaterials = await GeoMaterialDict.GetModifiedGeoMaterials();
                if (modifiedGeoMaterials.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string geomaterial in modifiedGeoMaterials)
                    {
                        results[4].Errors.Add($"Geomaterial Modified: {geomaterial}");
                    }
                }

            }

            return results;
        }

        /// <summary>
        /// Compares the hard-coded Geomaterials values to the GeoMaterialDict table values.
        /// </summary>
        /// <returns>Returns true if the table is modified.</returns>
        public static async Task<List<string>> GetModifiedGeoMaterials()
        {
            // List of Geomaterials added from the table
            List<Geomaterial> geomaterialTable = new List<Geomaterial>();

            // List of modified HierarchyKey for Geomaterials
            List<string> modifiedHierarchyKey = new List<string>();

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "GeoMaterialDict");

            if (standaloneTable == null)
            {
                // Table not found.
                return modifiedHierarchyKey;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    if (table != null)
                    {
                        using (RowCursor rowCursor = table.Search())
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    Geomaterial newGeo = new Geomaterial
                                    {
                                        HierarchyKey = Helpers.RowValueToString(row["hierarchykey"]),
                                        GeoMaterial = Helpers.RowValueToString(row["geomaterial"]),
                                        IndentedName = Helpers.RowValueToString(row["indentedname"]),
                                        Definition = Helpers.RowValueToString(row["definition"]),
                                    };

                                    geomaterialTable.Add(newGeo);
                                }
                            }
                        }
                    }
                }
            });

            // Compare all the hard-coded values to the table
            foreach (Geomaterial geoHardcode in GeoMaterialOptions)
            {
                // Find the hardcoded geomaterial in the geomaterial table by HierarchyKey
                Geomaterial lookup = geomaterialTable.FirstOrDefault(a => a.HierarchyKey == geoHardcode.HierarchyKey);

                // Check if the geomaterial wasn't found or if the values have changed 
                if (lookup == null || geoHardcode.GeoMaterial != lookup.GeoMaterial || geoHardcode.IndentedName != lookup.IndentedName || geoHardcode.Definition != lookup.Definition)
                {
                    // Change found. Add HierarchyKey to list.
                    modifiedHierarchyKey.Add(geoHardcode.HierarchyKey);
                }
            }

            // Return list of modified GeoMaterial hkeys
            return modifiedHierarchyKey;
        }

        public static ObservableCollection<Geomaterial> GeoMaterialOptions => new ObservableCollection<Geomaterial>()
        {
            new Geomaterial { HierarchyKey="01", GeoMaterial="Sedimentary material", IndentedName="Sedimentary material", Definition="An aggregation of particles deposited by gravity, air, water, or ice, or as accumulated by other natural agents operating at Earths surface such as chemical precipitation or secretion by organisms. May include unconsolidated material (sediment) and (or) sedimentary rock. Does not include sedimentary material directly deposited as a result of volcanic activity." },
            new Geomaterial { HierarchyKey="01.01", GeoMaterial="Sediment", IndentedName="--Sediment", Definition="Unconsolidated material (sediment) composed of particles deposited by gravity, air, water, or ice, or as accumulated by other natural agents operating at Earths surface such as chemical precipitation or secretion by organisms. Does not include sedimentary material directly deposited as a result of volcanic activity." },
            new Geomaterial { HierarchyKey="01.01.01", GeoMaterial="Clastic sediment", IndentedName="----Clastic sediment", Definition="Sediment formed by weathering and erosion of preexisting rocks or minerals; eroded particles or clasts are transported and deposited by gravity, air, water, or ice." },
            new Geomaterial { HierarchyKey="01.01.01.01", GeoMaterial="Sand and gravel of unspecified origin", IndentedName="------Sand and gravel of unspecified origin", Definition="Sediment composed mostly of sand and (or) gravel, formed by weathering and erosion of preexisting rocks or minerals; eroded particles or clasts are transported and deposited by gravity, air, water, or ice." },
            new Geomaterial { HierarchyKey="01.01.01.02", GeoMaterial="Silt and clay of unspecified origin", IndentedName="------Silt and clay of unspecified origin", Definition="Sediment composed mostly of silt and (or) clay, formed by weathering and erosion of preexisting rocks or minerals; eroded particles or clasts are transported and deposited by gravity, air, water, or ice." },
            new Geomaterial { HierarchyKey="01.01.01.03", GeoMaterial="Alluvial sediment", IndentedName="------Alluvial sediment", Definition="Unconsolidated material deposited by streams or other bodies of running water as sorted or semisorted sediment in streambed, or on its floodplain or delta, or as cone or fan at base of mountain slope. Grain size varies from clay to gravel." },
            new Geomaterial { HierarchyKey="01.01.01.03.01", GeoMaterial="Alluvial sediment, mostly coarse-grained", IndentedName="--------Alluvial sediment, mostly coarse-grained", Definition="Unconsolidated material deposited by streams or other bodies of running water as sorted or semisorted sediment in streambed, or on its floodplain or delta, or as cone or fan at base of mountain slope. Sediment is mostly sand, gravel, and coarser material but may also contain some silt and clay." },
            new Geomaterial { HierarchyKey="01.01.01.03.02", GeoMaterial="Alluvial sediment, mostly fine-grained", IndentedName="--------Alluvial sediment, mostly fine-grained", Definition="Unconsolidated material deposited by streams or other bodies of running water as sorted or semisorted sediment in streambed, or on its floodplain or delta, or as cone or fan at base of mountain slope. Sediment is mostly silt and clay but may also contain some sand and gravel." },
            new Geomaterial { HierarchyKey="01.01.01.04", GeoMaterial="Glacial till", IndentedName="------Glacial till", Definition="Mostly unsorted and unstratified material, generally unconsolidated, deposited directly by and underneath or adjacent to glacier without subsequent reworking by meltwater. Consists of heterogeneous mixture of clay, silt, sand, gravel, and boulders, ranging widely in size and shape." },
            new Geomaterial { HierarchyKey="01.01.01.04.01", GeoMaterial="Glacial till, mostly sandy", IndentedName="--------Glacial till, mostly sandy", Definition="Mostly unsorted and unstratified material, generally unconsolidated, deposited directly by and underneath or adjacent to glacier without subsequent reworking by meltwater. Consists of heterogeneous mixture of clay, silt, sand, gravel, and boulders, ranging widely in size and shape. Relatively sandy in texture." },
            new Geomaterial { HierarchyKey="01.01.01.04.02", GeoMaterial="Glacial till, mostly silty", IndentedName="--------Glacial till, mostly silty", Definition="Mostly unsorted and unstratified material, generally unconsolidated, deposited directly by and underneath or adjacent to glacier without subsequent reworking by meltwater. Consists of heterogeneous mixture of clay, silt, sand, gravel, and boulders, ranging widely in size and shape. Relatively loamy (silty) in texture." },
            new Geomaterial { HierarchyKey="01.01.01.04.03", GeoMaterial="Glacial till, mostly clayey", IndentedName="--------Glacial till, mostly clayey", Definition="Mostly unsorted and unstratified material, generally unconsolidated, deposited directly by and underneath or adjacent to glacier without subsequent reworking by meltwater. Consists of heterogeneous mixture of clay, silt, sand, gravel, and boulders, ranging widely in size and shape. Relatively clayey in texture." },
            new Geomaterial { HierarchyKey="01.01.01.05", GeoMaterial="Ice-contact and ice-marginal sediment", IndentedName="------Ice-contact and ice-marginal sediment", Definition="Mostly sand-, silt-, and gravel-sized particles or clasts derived from rock or preexisting sediment that has been eroded and transported by glaciers. As glacier melted, material was deposited by running water essentially in contact with glacial ice or was transported and deposited by glacially fed streams. Includes sediment deposited into water bodies adjacent to glacier." },
            new Geomaterial { HierarchyKey="01.01.01.05.01", GeoMaterial="Ice-contact and ice-marginal sediment, mostly coarse-grained", IndentedName="--------Ice-contact and ice-marginal sediment, mostly coarse-grained", Definition="Mostly sand- and gravel-sized particles or clasts, with lesser amounts of silt and clay, derived from rock or preexisting sediment that has been eroded and transported by glaciers. As glacier melted, material was deposited by running water essentially in contact with glacial ice or was transported and deposited by glacially fed streams. Includes sediment deposited into water bodies adjacent to glacier." },
            new Geomaterial { HierarchyKey="01.01.01.05.02", GeoMaterial="Ice-contact and ice-marginal sediment, mostly fine-grained", IndentedName="--------Ice-contact and ice-marginal sediment, mostly fine-grained", Definition="Mostly silt- and clay-sized particles or clasts, with lesser amounts of sand and gravel, derived from rock or preexisting sediment that has been eroded and transported by glaciers. As glacier melted, material was deposited by running water essentially in contact with glacial ice or was transported and deposited by glacially fed streams. Includes sediment deposited into water bodies adjacent to glacier." },
            new Geomaterial { HierarchyKey="01.01.01.06", GeoMaterial="Eolian sediment", IndentedName="------Eolian sediment", Definition="Silt- and sand-sized sediment, deposited by wind." },
            new Geomaterial { HierarchyKey="01.01.01.06.01", GeoMaterial="Dune sand", IndentedName="--------Dune sand", Definition="Mostly sand-sized sediment, deposited by wind. Typically characterized by various dune landforms." },
            new Geomaterial { HierarchyKey="01.01.01.06.02", GeoMaterial="Loess", IndentedName="--------Loess", Definition="Silty sediment, deposited by wind, commonly near glacial margin." },
            new Geomaterial { HierarchyKey="01.01.01.07", GeoMaterial="Lacustrine sediment", IndentedName="------Lacustrine sediment", Definition="Mostly well-sorted and well-bedded material that ranges in grain size from clay to gravel, deposited in perennial to intermittent lakes. Much of sediment is derived from material eroded and transported by streams. Includes deposits of lake-marginal beaches and deltas." },
            new Geomaterial { HierarchyKey="01.01.01.07.01", GeoMaterial="Lacustrine sediment, mostly coarse-grained", IndentedName="--------Lacustrine sediment, mostly coarse-grained", Definition="Mostly well-sorted and well-bedded material, generally sand and gravel sized, with lesser amounts of silt and clay, deposited in perennial to intermittent lakes. Much of sediment is derived from material eroded and transported by streams. Mostly deposits of lake-marginal beaches and deltas." },
            new Geomaterial { HierarchyKey="01.01.01.07.02", GeoMaterial="Lacustrine sediment, mostly fine-grained", IndentedName="--------Lacustrine sediment, mostly fine-grained", Definition="Mostly well-sorted and well-bedded material, generally silt and clay sized, with lesser amounts of sand, deposited in perennial to intermittent lakes. " },
            new Geomaterial { HierarchyKey="01.01.01.08", GeoMaterial="Playa sediment", IndentedName="------Playa sediment", Definition="Fine-grained clastic sediment and evaporitic salts, deposited in ephemeral lakes in centers of undrained basins. Includes material deposited in playas, mud flats, salt flats, and adjacent saline marshes. Generally interbedded with eolian sand and with lacustrine sediment deposited during wetter climatic periods; commonly intertongues upslope with sediment deposited by alluvial fans." },
            new Geomaterial { HierarchyKey="01.01.01.09", GeoMaterial="Coastal zone sediment", IndentedName="------Coastal zone sediment", Definition="Mud and sand, with lesser amounts of gravel, deposited on beaches, on barrier islands, or in nearshore-marine, deltaic, or various low-energy shoreline (mud flat, tidal flat, sabka, algal flat) environments." },
            new Geomaterial { HierarchyKey="01.01.01.09.01", GeoMaterial="Coastal zone sediment, mostly coarse-grained", IndentedName="--------Coastal zone sediment, mostly coarse-grained", Definition="Mostly sand, silt, and gravel, deposited on beaches, in dunes, and in shallow-marine and related alluvial environments." },
            new Geomaterial { HierarchyKey="01.01.01.09.02", GeoMaterial="Coastal zone sediment, mostly fine-grained", IndentedName="--------Coastal zone sediment, mostly fine-grained", Definition="Mostly clay and silt, deposited in lagoons, tidal flats, backbarriers, and coastal marshes." },
            new Geomaterial { HierarchyKey="01.01.01.10", GeoMaterial="Marine sediment", IndentedName="------Marine sediment", Definition="Mud and sand, deposited in various marine environments. May originate from erosion of rocks and sediment or may be derived from marine organisms (of carbonate or siliceous composition)." },
            new Geomaterial { HierarchyKey="01.01.01.10.01", GeoMaterial="Marine sediment, mostly coarse-grained", IndentedName="--------Marine sediment, mostly coarse-grained", Definition="Mud, sand, and gravel eroded from rocks and sediment on land, transported by streams, and deposited in marine deltas and basins. Mostly siliceous in composition." },
            new Geomaterial { HierarchyKey="01.01.01.10.02", GeoMaterial="Marine sediment, mostly fine-grained", IndentedName="--------Marine sediment, mostly fine-grained", Definition="Mostly clay- and silt-sized sediment, deposited in relatively deep, quiet water, far removed from areas where coarser grained clastic sediments are washed into marine environment. Includes sediment derived from marine organisms." },
            new Geomaterial { HierarchyKey="01.01.01.11", GeoMaterial="Mass movement sediment", IndentedName="------Mass movement sediment", Definition="Sediment formed by downslope transport of particles or clasts produced by weathering and breakdown of underlying rock, sediment, and (or) soil. Composed of poorly sorted and poorly stratified material that ranges in size from clay to boulders. Includes colluvial, landslide, talus, and rock-avalanche deposits." },
            new Geomaterial { HierarchyKey="01.01.01.11.01", GeoMaterial="Colluvium and other widespread mass-movement sediment", IndentedName="--------Colluvium and other widespread mass-movement sediment", Definition="Sediment formed by slow, relatively widespread, downslope transport of particles or clasts produced by weathering and breakdown of underlying rock, sediment, and (or) soil. Composed of poorly sorted and poorly stratified material that ranges in size from clay to boulders." },
            new Geomaterial { HierarchyKey="01.01.01.11.02", GeoMaterial="Debris flows, landslides, and other localized mass-movement sediment", IndentedName="--------Debris flows, landslides, and other localized mass-movement sediment", Definition="Sediment formed by relatively localized, downslope transport of particles or clasts produced by weathering and breakdown of underlying rock, sediment, and (or) soil. Composed of poorly sorted and poorly stratified material that ranges in size from clay to boulders. Speed of downslope transport ranges from rapid to imperceptible." },
            new Geomaterial { HierarchyKey="01.01.02", GeoMaterial="Residual material", IndentedName="----Residual material", Definition="Unconsolidated material, developed in place by weathering of underlying rock or sediment. Usually forms relatively thin surface layer that conceals unweathered or partly altered source material. Material from which soils are formed." },
            new Geomaterial { HierarchyKey="01.01.03", GeoMaterial="Carbonate sediment", IndentedName="----Carbonate sediment", Definition="Sediment formed by biotic or abiotic precipitation from aqueous solution of carbonates of calcium, magnesium, or iron (for example, limestone, dolomite)." },
            new Geomaterial { HierarchyKey="01.01.04", GeoMaterial="Peat and muck", IndentedName="----Peat and muck", Definition="Unconsolidated material, principally composed of plant remains, with lesser amounts of fine-grained clastic sediment. Deposited in water-saturated environment such as swamp, marsh, or bog. With lithification, such material becomes coal." },
            new Geomaterial { HierarchyKey="01.02", GeoMaterial="Sedimentary rock", IndentedName="--Sedimentary rock", Definition="Consolidated material (rock) composed of particles transported and deposited by gravity, air, water, or ice, or accumulated by other natural agents operating at Earths surface, such as chemical precipitation or secretion by organisms. Does not here include sedimentary material directly deposited as result of volcanic activity." },
            new Geomaterial { HierarchyKey="01.02.01", GeoMaterial="Clastic sedimentary rock", IndentedName="----Clastic sedimentary rock", Definition="Sedimentary rock, composed predominantly of particles or clasts derived by erosion, weathering, or mass-wasting of preexisting rock and deposited by gravity, air, water, or ice." },
            new Geomaterial { HierarchyKey="01.02.01.01", GeoMaterial="Conglomerate", IndentedName="------Conglomerate", Definition="Sedimentary rock, composed predominantly of particles or clasts derived by erosion and weathering of preexisting rock; contains more than 30 percent gravel-sized clasts." },
            new Geomaterial { HierarchyKey="01.02.01.02", GeoMaterial="Sandstone", IndentedName="------Sandstone", Definition="Sedimentary rock, composed predominantly of particles or clasts derived by erosion and weathering of preexisting rock; consists mostly of sand-sized particles, with or without fine-grained matrix of silt or clay." },
            new Geomaterial { HierarchyKey="01.02.01.03", GeoMaterial="Mostly sandstone", IndentedName="------Mostly sandstone", Definition="Mostly sandstone, interbedded with other sedimentary rocks that locally may include conglomerate and finer grained clastic rocks (mudstone), carbonates, and (or) coal." },
            new Geomaterial { HierarchyKey="01.02.01.04", GeoMaterial="Sandstone and mudstone", IndentedName="------Sandstone and mudstone", Definition="Sandstone and mudstone (including shale and siltstone), in approximately equal (or unspecified) proportions. " },
            new Geomaterial { HierarchyKey="01.02.01.05", GeoMaterial="Mudstone", IndentedName="------Mudstone", Definition="Sedimentary rock, composed predominantly of particles or clasts derived by erosion and weathering of preexisting rock; consists mostly of mud (that is, silt- and clay-sized particles). Includes shale and siltstone." },
            new Geomaterial { HierarchyKey="01.02.01.06", GeoMaterial="Mostly mudstone", IndentedName="------Mostly mudstone", Definition="Mostly mudstone, interbedded with other sedimentary rocks that locally may include coarser grained clastic rocks (sandstone, conglomerate), carbonates, and (or) coal." },
            new Geomaterial { HierarchyKey="01.02.02", GeoMaterial="Carbonate rock", IndentedName="----Carbonate rock", Definition="Sedimentary rock, consisting chiefly of carbonate minerals such as limestone or dolomite. " },
            new Geomaterial { HierarchyKey="01.02.02.01", GeoMaterial="Limestone", IndentedName="------Limestone", Definition="Carbonate sedimentary rock, consisting chiefly of calcite." },
            new Geomaterial { HierarchyKey="01.02.02.02", GeoMaterial="Dolomite", IndentedName="------Dolomite", Definition="Carbonate sedimentary rock, consisting chiefly of dolomite. Although dolostone is the proper analog to limestone, it has not often been applied to dolomitic units." },
            new Geomaterial { HierarchyKey="01.02.03", GeoMaterial="Mostly carbonate rock", IndentedName="----Mostly carbonate rock", Definition="Mostly carbonate rock, interbedded with other sedimentary rock types." },
            new Geomaterial { HierarchyKey="01.02.04", GeoMaterial="Chert", IndentedName="----Chert", Definition="Sedimentary rock, composed chiefly of microcrystalline or cryptocrystalline quartz." },
            new Geomaterial { HierarchyKey="01.02.05", GeoMaterial="Evaporitic rock", IndentedName="----Evaporitic rock", Definition="Sedimentary rock, composed primarily of minerals produced by evaporation of saline solution. Examples include gypsum, anhydrite, other diverse sulfates, halite (rock salt), primary dolomite, and rocks composed of various nitrates and borates." },
            new Geomaterial { HierarchyKey="01.02.06", GeoMaterial="Iron-rich sedimentary rock", IndentedName="----Iron-rich sedimentary rock", Definition="Sedimentary rock, in which at least half (by volume) of observed minerals are iron bearing (hematite, magnetite, limonite group minerals, siderite, iron sulfides)." },
            new Geomaterial { HierarchyKey="01.02.07", GeoMaterial="Coal and lignite", IndentedName="----Coal and lignite", Definition="Organic-rich sedimentary rock, formed from compaction and alteration of plant remains. Coal is consolidated, harder, black rock. Lignite is semiconsolidated, brown to black, earthy material that may contain large particles of recognizable plant parts and tends to crack upon drying." },
            new Geomaterial { HierarchyKey="02", GeoMaterial="Sedimentary and extrusive igneous material", IndentedName="Sedimentary and extrusive igneous material", Definition="Either (1) sedimentary rock and (or) unconsolidated material (sediment) and extrusive igneous material (volcanic rock and [or] sediment) or (2) volcanic rock and (or) sediment and such material after erosion and redeposition." },
            new Geomaterial { HierarchyKey="03", GeoMaterial="Igneous rock", IndentedName="Igneous rock", Definition="Rock and fragmental material that solidified from molten or partly molten material (magma). " },
            new Geomaterial { HierarchyKey="03.01", GeoMaterial="Extrusive igneous material", IndentedName="--Extrusive igneous material", Definition="Molten material that was erupted onto Earths surface, fusing into rock or remaining as unconsolidated particles. Includes pyroclastic flows, air-fall tephra, lava flows, and volcanic mass flows." },
            new Geomaterial { HierarchyKey="03.01.01", GeoMaterial="Volcaniclastic (fragmental) material", IndentedName="----Volcaniclastic (fragmental) material", Definition="Rock and unconsolidated material consisting of particles or clasts that were formed by volcanic explosion or aerial expulsion from volcanic vent." },
            new Geomaterial { HierarchyKey="03.01.01.01", GeoMaterial="Pyroclastic flows", IndentedName="------Pyroclastic flows", Definition="Hot ash, pumice, and rock fragments erupted from volcano or caldera. Material moves downslope commonly in chaotic flows; once deposited, hot fragments may compact under their own weight and weld together." },
            new Geomaterial { HierarchyKey="03.01.01.01.01", GeoMaterial="Felsic-composition pyroclastic flows", IndentedName="--------Felsic-composition pyroclastic flows", Definition="Hot ash, pumice, and rock fragments erupted from volcano or caldera. Material moves downslope commonly in chaotic flows; once deposited, hot fragments may compact under their own weight and weld together. Because of their high-silica content and resulting high viscosity, parental magmas tend to erupt explosively. Includes rhyolite, dacite, trachyte, latite; rocks are commonly light-colored." },
            new Geomaterial { HierarchyKey="03.01.01.01.02", GeoMaterial="Intermediate-composition pyroclastic flows", IndentedName="--------Intermediate-composition pyroclastic flows", Definition="Hot ash, pumice, and rock fragments erupted from volcano. Material moves downslope commonly in chaotic flows; once deposited, hot fragments may compact under their own weight and weld together. Parental magma commonly erupts from stratovolcanoes as thick lava flows but also can generate strong explosive eruptions to form pyroclastic flows. Includes rocks that are, in color and mineral composition, intermediate between felsic and mafic rocks (for example, andesite)." },
            new Geomaterial { HierarchyKey="03.01.01.01.03", GeoMaterial="Mafic-composition pyroclastic flows", IndentedName="--------Mafic-composition pyroclastic flows", Definition="Hot ash, pumice, and rock fragments erupted from volcano. Material moves downslope commonly in chaotic flows; once deposited, hot fragments may compact under their own weight and weld together. Because of their low silica content and resulting low viscosity, parental magmas tend to erupt gently as lava flows rather than more forcefully as pyroclastic flows. Includes basalt; rocks are commonly dark-colored. " },
            new Geomaterial { HierarchyKey="03.01.01.02", GeoMaterial="Air-fall tephra", IndentedName="------Air-fall tephra", Definition="Fragments of volcanic rock and lava, of various sizes, carried into air by explosions and by hot gases in eruption columns or lava fountains; known as tephra. As tephra falls to ground, with increasing distance from volcano, average size of individual rock particles and thickness of resulting deposit decrease. Fine tephra deposited at some distance from volcano is known as volcanic ash." },
            new Geomaterial { HierarchyKey="03.01.01.02.01", GeoMaterial="Felsic-composition air-fall tephra", IndentedName="--------Felsic-composition air-fall tephra", Definition="Fragments of volcanic rock and lava, of various sizes, carried into air by explosions and by hot gases in eruption columns or lava fountains; known as tephra. As tephra falls to ground, with increasing distance from volcano, average size of individual rock particles and thickness of resulting deposit decrease. Because of their high silica content and resulting high viscosity, felsic-composition magmas tend to erupt explosively, readily forming pumice and volcanic ash. Composed of light-colored rocks (for example, rhyolite, dacite, trachyte, latite). " },
            new Geomaterial { HierarchyKey="03.01.01.02.02", GeoMaterial="Intermediate-composition air-fall tephra", IndentedName="--------Intermediate-composition air-fall tephra", Definition="Fragments of volcanic rock and lava, of various sizes, carried into the air by explosions and by hot gases in eruption columns or lava fountains; known as tephra. As tephra falls to ground, with increasing distance from volcano, average size of individual rock particles and thickness of resulting deposit decrease. Parental magma commonly erupts from stratovolcanoes as thick lava flows but also can generate strong explosive eruptions to form pyroclastic flows. Includes rocks that are, in color and mineral composition, intermediate between felsic and mafic rocks (for example, andesite). " },
            new Geomaterial { HierarchyKey="03.01.01.02.03", GeoMaterial="Mafic-composition air-fall tephra", IndentedName="--------Mafic-composition air-fall tephra", Definition="Fragments of volcanic rock and lava, of various sizes, carried into the air by explosions and by hot gases in eruption columns or lava fountains; known as tephra. As tephra falls to ground, with increasing distance from volcano, average size of individual rock particles and thickness of resulting deposit decrease. Because of their low silica content and resulting low viscosity, parental magmas tend to erupt gently as lava flows, and so these deposits are uncommon. Includes basalt; rocks are commonly dark-colored. " },
            new Geomaterial { HierarchyKey="03.01.02", GeoMaterial="Lava flows", IndentedName="----Lava flows", Definition="Lateral, surficial outpourings of molten lava from vent or fissure; also, solidified bodies of rock that form when they cool. Composed generally of fine-grained, dark-colored rocks (for example, basalt), which tend to form extensive sheets that have generally low relief, except in vent areas where cinder cones or shield volcanoes may form. Includes basaltic shield volcanoes, which may become very large (for example, Hawaii)." },
            new Geomaterial { HierarchyKey="03.01.02.01", GeoMaterial="Felsic-composition lava flows", IndentedName="------Felsic-composition lava flows", Definition="Lateral, surficial outpourings of molten lava from vent or fissure; also, solidified bodies of rock that form when they cool. Because of their high silica content and resulting high viscosity, parental magmas tend to erupt explosively, and so these deposits are uncommon. Includes fine-grained, light-colored rock with rhyolitic, dacitic, trachytic, and latitic composition." },
            new Geomaterial { HierarchyKey="03.01.02.02", GeoMaterial="Intermediate-composition lava flows", IndentedName="------Intermediate-composition lava flows", Definition="Lateral, surficial outpourings of molten lava from vent or fissure; also, solidified bodies of rock that form when they cool. Parental magma commonly erupts from stratovolcanoes as thick lava flows. Includes rocks that are, in color and in mineral composition, intermediate between felsic and mafic rocks (for example, andesite)." },
            new Geomaterial { HierarchyKey="03.01.02.03", GeoMaterial="Mafic-composition lava flows", IndentedName="------Mafic-composition lava flows", Definition="Lateral, surficial outpourings of molten lava from vent or fissure; also, solidified bodies of rock that form when they cool. Low-silica parental magmas have low viscosity and tend to form extensive sheets that have generally low relief. Includes basaltic shield volcanoes, which may become very large (for example, in Hawaii). Composed of fine-grained, dark rocks, including basaltic." },
            new Geomaterial { HierarchyKey="03.01.03", GeoMaterial="Volcanic mass flow", IndentedName="----Volcanic mass flow", Definition="Volcanic deposits formed by mass movement (for example, debris avalanches, debris flows, lahar deposits), in many cases triggered by volcanic eruption. Debris avalanches that occur on volcanoes clearly without eruptive trigger may be classified as sedimentary (for example, as Debris flows, landslides, and other localized mass-movement sediment)." },
            new Geomaterial { HierarchyKey="03.02", GeoMaterial="Intrusive igneous rock", IndentedName="--Intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) below Earths surface." },
            new Geomaterial { HierarchyKey="03.02.01", GeoMaterial="Coarse-grained intrusive igneous rock", IndentedName="----Coarse-grained intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at some depth beneath Earths surface, thereby cooling slowly enough for mineral crystals to grow large enough to be visible to naked eye." },
            new Geomaterial { HierarchyKey="03.02.01.02", GeoMaterial="Coarse-grained, felsic-composition intrusive igneous rock", IndentedName="------Coarse-grained, felsic-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at some depth beneath Earths surface, thereby cooling slowly enough for mineral crystals to grow large enough to be visible to naked eye. Composed mostly of light-colored minerals (for example, feldspar, quartz). Includes granitic, syenitic, and monzonitic rock." },
            new Geomaterial { HierarchyKey="03.02.01.03", GeoMaterial="Coarse-grained, intermediate-composition intrusive igneous rock", IndentedName="------Coarse-grained, intermediate-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at some depth beneath Earths surface, thereby cooling slowly enough for mineral crystals to grow large enough to be visible to naked eye. Intermediate in color and in mineral composition (between felsic and mafic igneous rock). Includes dioritic rock." },
            new Geomaterial { HierarchyKey="03.02.01.04", GeoMaterial="Coarse-grained, mafic-composition intrusive igneous rock", IndentedName="------Coarse-grained, mafic-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at some depth beneath Earths surface, thereby cooling slowly enough for mineral crystals to grow large enough to be visible to naked eye. Composed mostly of feldspar and dark-colored minerals. Includes gabbroic rock." },
            new Geomaterial { HierarchyKey="03.02.01.05", GeoMaterial="Ultramafic intrusive igneous rock", IndentedName="------Ultramafic intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at some depth beneath Earths surface, thereby cooling slowly enough for mineral crystals to grow large enough to be visible to naked eye. Composed almost entirely of mafic minerals (for example, hypersthene, augite, olivine)." },
            new Geomaterial { HierarchyKey="03.02.02", GeoMaterial="Fine-grained intrusive igneous rock", IndentedName="----Fine-grained intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at shallow depth beneath Earths surface, thereby cooling quickly. Generally fine grained but may contain large mineral crystals (phenocrysts). Mostly found as tabular dikes or sills." },
            new Geomaterial { HierarchyKey="03.02.02.01", GeoMaterial="Fine-grained, felsic-composition intrusive igneous rock", IndentedName="------Fine-grained, felsic-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at shallow depth beneath Earths surface, thereby cooling quickly. Generally fine grained but may contain large mineral crystals (phenocrysts). Mostly found as tabular dikes or sills. Composed mostly of light-colored minerals. Includes rhyolitic, dacitic, trachytic, and latitic rock." },
            new Geomaterial { HierarchyKey="03.02.02.02", GeoMaterial="Fine-grained, intermediate-composition intrusive igneous rock", IndentedName="------Fine-grained, intermediate-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at shallow depth beneath Earths surface, thereby cooling quickly. Generally fine grained but may contain large mineral crystals (phenocrysts). Mostly found as tabular dikes or sills. Intermediate in color and in mineral composition (between felsic and mafic igneous rock). Includes andesitic rock." },
            new Geomaterial { HierarchyKey="03.02.02.03", GeoMaterial="Fine-grained, mafic-composition intrusive igneous rock", IndentedName="------Fine-grained, mafic-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) at shallow depth beneath Earths surface, thereby cooling quickly. Generally fine grained but may contain large mineral crystals (phenocrysts). Mostly found as tabular dikes or sills. Composed mostly of dark-colored minerals. Includes basaltic rock." },
            new Geomaterial { HierarchyKey="03.02.03", GeoMaterial="Exotic-composition intrusive igneous rock", IndentedName="----Exotic-composition intrusive igneous rock", Definition="Rock that solidified from molten or partly molten material (magma) below Earths surface that has exotic mineralogical, textural, or field setting characteristics. Typically dark colored with abundant phenocrysts. Includes kimberlite, lamprophyre, lamproite, and foiditic rocks." },
            new Geomaterial { HierarchyKey="04", GeoMaterial="Igneous and metamorphic rock", IndentedName="Igneous and metamorphic rock", Definition="Consists of coarse-grained intrusive igneous rock and generally medium- to high-grade metamorphic rock." },
            new Geomaterial { HierarchyKey="05", GeoMaterial="Metamorphic rock", IndentedName="Metamorphic rock", Definition="Rock derived from preexisting rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust." },
            new Geomaterial { HierarchyKey="05.01", GeoMaterial="Regional metamorphic rock, of unspecified origin", IndentedName="--Regional metamorphic rock, of unspecified origin", Definition="Rock derived from preexisting rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked regional changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Origin of preexisting rock is mixed (for example, igneous and sedimentary) or is not known. " },
            new Geomaterial { HierarchyKey="05.01.01", GeoMaterial="Lower-grade metamorphic rock, of unspecified origin", IndentedName="----Lower-grade metamorphic rock, of unspecified origin", Definition="Rock derived from preexisting rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to relatively mild regional changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Origin of preexisting rock is mixed (for example, igneous and sedimentary) or is not known. Includes slate and phyllite." },
            new Geomaterial { HierarchyKey="05.01.02", GeoMaterial="Medium and high-grade regional metamorphic rock, of unspecified origin", IndentedName="----Medium and high-grade regional metamorphic rock, of unspecified origin", Definition="Rock derived from preexisting rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to relatively intense regional changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Origin of preexisting rock is mixed (for example, igneous and sedimentary) or is not known. Includes amphibolite, granulite, schist, and gneiss." },
            new Geomaterial { HierarchyKey="05.01.03", GeoMaterial="Contact-metamorphic rock", IndentedName="----Contact-metamorphic rock", Definition="Altered rock that originated by local processes of thermal metamorphism, genetically related to intrusion and extrusion of magmas and taking place at or near contact with body of igneous rock. Metamorphic changes are effected by heat and fluids emanating from magma and by some deformation because of emplacement of igneous mass." },
            new Geomaterial { HierarchyKey="05.01.04", GeoMaterial="Deformation-related metamorphic rock", IndentedName="----Deformation-related metamorphic rock", Definition="Rock derived from preexisting rocks by essentially solid-state mineralogical, chemical, or structural changes in response to strong deformation, commonly in association with marked changes in temperature, pressure, and (or) chemical environment. Generally forms in narrow, planar zones of local deformation (for example, along faults); characterized by foliation or alignment of mineral grains. Includes mylonite and cataclasite." },
            new Geomaterial { HierarchyKey="05.02", GeoMaterial="Metasedimentary rock", IndentedName="--Metasedimentary rock", Definition="Rock derived from preexisting sedimentary rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust." },
            new Geomaterial { HierarchyKey="05.02.01", GeoMaterial="Slate and phyllite, of sedimentary-rock origin", IndentedName="----Slate and phyllite, of sedimentary-rock origin", Definition="Fine-grained rock derived from preexisting sedimentary rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Includes phyllite and slate (compact, fine-grained rock that possesses strong cleavage and, hence, can be split into slabs and thin plates). Mostly formed from fine-grained material such as mudstone." },
            new Geomaterial { HierarchyKey="05.02.02", GeoMaterial="Schist and gneiss, of sedimentary-rock origin", IndentedName="----Schist and gneiss, of sedimentary-rock origin", Definition="Foliated rock derived from preexisting sedimentary rocks by essentially solid-state mineralogical, chemical, or structural changes in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Includes schist (characterized by such strong foliation or alignment of minerals that it readily splits into flakes or slabs) and gneiss (characterized by alternating, irregular bands of different mineral composition). Mostly formed from fine-grained material such as mudstone." },
            new Geomaterial { HierarchyKey="05.02.03", GeoMaterial="Meta-carbonate rock", IndentedName="----Meta-carbonate rock", Definition="Rock derived from preexisting carbonate sedimentary rocks and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Characterized by recrystallization of carbonate minerals in source rock. Includes marble (for which preexisting rock was dominantly limestone or other rock composed of calcite), dolomitic marble, meta-dolostone, and meta-dolomite (for which preexisting rock contained appreciable amount of magnesium)." },
            new Geomaterial { HierarchyKey="05.02.04", GeoMaterial="Quartzite", IndentedName="----Quartzite", Definition="Rock derived from preexisting quartz-rich sedimentary rocks (commonly sandstone) and altered by essentially solid-state mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, shear stress, and (or) chemical environment, generally at depth in Earths crust. " },
            new Geomaterial { HierarchyKey="05.03", GeoMaterial="Metaigneous rock", IndentedName="--Metaigneous rock", Definition="Rock derived from preexisting igneous rocks and altered by essentially solid-state, mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, shear stress, and (or) chemical environment, generally at depth in Earths crust. " },
            new Geomaterial { HierarchyKey="05.03.01", GeoMaterial="Meta-ultramafic rock", IndentedName="----Meta-ultramafic rock", Definition="Rock derived from preexisting ultramafic rocks by essentially solid-state, mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Composed mostly of magnesium-bearing minerals (for example, serpentine, talc, magnesite)." },
            new Geomaterial { HierarchyKey="05.03.02", GeoMaterial="Meta-mafic rock", IndentedName="----Meta-mafic rock", Definition="Rock derived from preexisting mafic rocks by essentially solid-state, mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Composed mostly of iron- and magnesium-bearing, dark-colored and (or) green minerals. Includes greenstone, amphibolite, and metagabbro." },
            new Geomaterial { HierarchyKey="05.03.03", GeoMaterial="Meta-felsic and intermediate rock", IndentedName="----Meta-felsic and intermediate rock", Definition="Rock derived from preexisting felsic and intermediate-composition rocks by essentially solid-state, mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Composed mostly of light-colored minerals; relatively enriched in silica. Includes metagranite, metadiorite, and meta-andesite." },
            new Geomaterial { HierarchyKey="05.03.04", GeoMaterial="Meta-volcaniclastic rock", IndentedName="----Meta-volcaniclastic rock", Definition="Rock derived from preexisting volcaniclastic rocks by essentially solid-state, mineralogical, chemical, or structural changes, in response to marked changes in temperature, pressure, deformation, and (or) chemical environment, generally at depth in Earths crust. Composed of deformed but recognizable particles or clasts of volcanic explosive material." },
            new Geomaterial { HierarchyKey="06", GeoMaterial="Other materials", IndentedName="Other materials:", Definition="" },
            new Geomaterial { HierarchyKey="06.01", GeoMaterial="Rock and sediment", IndentedName="--Rock and sediment", Definition="Various rocks and sediment, not differentiated." },
            new Geomaterial { HierarchyKey="06.02", GeoMaterial="Rock", IndentedName="--Rock", Definition="Various rock types, not differentiated." },
            new Geomaterial { HierarchyKey="06.03", GeoMaterial="\"Made\" or human-engineered land", IndentedName="--\"Made\" or human-engineered land", Definition="Modern, unconsolidated material known to have human-related origin." },
            new Geomaterial { HierarchyKey="06.04", GeoMaterial="Water or ice", IndentedName="--Water or ice", Definition="" },
            new Geomaterial { HierarchyKey="06.05", GeoMaterial="Unmapped area", IndentedName="--Unmapped area", Definition="" },
        };
    }
}
