# AZGS GEOMAPMAKER v1.0 RELEASE
This is the design specification for the [AZGS](https://azgs.arizona.edu/)'s geologic mapping toolbar for ***ESRI ArcPro (v2.9x)***. This primary purpose of this toolbar is to seamlessly (and discretely) integrate compliance with the 2020 USGS [Geologic Map Schema](https://pubs.er.usgs.gov/publication/tm11B10) into the map digitization process.

## Core Requirements
The following *minimum* features are required for a functional toolbar. The toolbar may in many cases exceed the capabilities listed below.  

- [x] Toolbar should be compatible with Arc Server Enterprise (ArcSDE) and flat ESRI File Geodatabase workflows.
- [x] Must generate a GeMS validation REPORT.
- [x] Maps must be exportable as a GeMs compliant ESRI File Geodatabase.
- [x] Forms for adding features enforce appropriate constraints and vocabularies.
- [x] Ability to transfer, select, and view from a pre-made list of symbologies.

## Table of Contents
- [AZGS GEOMAPMAKER v1.0 RELEASE](#azgs-geomapmaker-v10-release)
  - [Core Requirements](#core-requirements)
  - [Table of Contents](#table-of-contents)
    - [Installation Instructions](#installation-instructions)
  - [Buttons and Workflow Overview](#buttons-and-workflow-overview)
    - [Data Source (Dropdown)](#data-source-dropdown)
    - [Data Sources (Button)](#data-sources-button)
    - [Headings (Button)](#headings-button)
    - [Map Units (Button)](#map-units-button)
    - [Hierarchy (Button)](#hierarchy-button)
    - [Contacts and Faults (Workflow)](#contacts-and-faults-workflow)
    - [Map Unit Polygons (Workflow)](#map-unit-polygons-workflow)
    - [Orientation Points and Stations (Workflow)](#orientation-points-and-stations-workflow)
    - [Glossary (Button)](#glossary-button)
    - [Validation Report (Button)](#validation-report-button)
    - [Export (Button)](#export-button)
  - [Table Definitions](#table-definitions)
    - [Map Unit Polys](#map-unit-polys)
    - [Contacts And Faults](#contacts-and-faults)
    - [Description Of Map Units](#description-of-map-units)
    - [DataSources](#datasources)
    - [Glossary](#glossary)
  - [Special Considerations](#special-considerations)
    - [Symbology](#symbology)
    - [Glossary Template](#glossary-template)
    - [Handling Ages](#handling-ages)
    - [Location Confidence Meters](#location-confidence-meters)
    - [Future Work](#future-work)

### Installation Instructions
Users can currently download geomapmaker from the [/installation](/installation) fodler in this repository. This folder consists of two subdirectories: [/geomapmaker_addin](/installation/geomapmaker_addin) and [/geomapmaker_geodatabase](/installation/geomapaker_geodatabase). The addin subdirectory holds the actual toolbar and can be installed into your ESRI ArcPRO environment by going to the addin-manager in Settings.

![Screenshot 2022-05-27 at 12 39 44 PM](https://user-images.githubusercontent.com/10422595/170779059-11e84426-28e1-40a3-a458-556def262871.png)

The addin manager will automatically work with any tables loaded into the currently active map that follow the GeMS namespace (e.g., [ContactsAndFaults](#contacts-and-faults), [MapUnitPolys](#map-unit-polys)) - **though see the [Symbology Table](#symbology) and [Glossary Templates](#glossary-templates) section for special considerations.** 

For convenience, however, we do provide a blank GeMS geodatabase with all the core tables provided as part of the installation in the [/geomapmaker_geodatabase](/installation/geomapaker_geodatabase) subdirectory.

> Please remember that this toolbar will *only* work for ESRI ArcPro 2.9x. 

## Buttons and Workflow Overview

### Data Source (Dropdown)
Users must select a data source from the dropdown menu before being able to proceed with other actions (other than creating a new data source user). The list of selectable datasources is drawn directly from the [DATA SOURCES](#datasources) table.

### Data Sources (Button)
Users can add, edit, or delete data sources to the [DATA SOURCES](#datasources) table from the data sources button. 

![Screenshot 2022-05-24 at 10 05 41 AM](https://user-images.githubusercontent.com/10422595/170092556-72e3aefa-5ae2-4263-afe0-f675571bb122.png)

### Headings (Button)
Users can add, edit, or delete new headers or subheaders to the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table.

![Screenshot 2022-05-24 at 10 08 27 AM](https://user-images.githubusercontent.com/10422595/170093044-4f2de052-d101-42f7-9825-b1dffa6e9408.png)

### Map Units (Button)
Users can add, edit, or delete map unit descriptions to the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table.

![Screenshot 2022-05-24 at 10 09 46 AM](https://user-images.githubusercontent.com/10422595/170093285-32495be8-6ddc-47be-aaa8-47d025a06445.png)

### Hierarchy (Button)
Users can assign the hierarchy of units and headers using the hiearchy assignment tool. Changes made using the hierarchy assignment tool will automatically updated the `HierarchyKey` of the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table.

![Screenshot 2022-05-24 at 10 13 40 AM](https://user-images.githubusercontent.com/10422595/170093913-72053579-e65b-4e8a-8312-7b3509e21402.png)

### Contacts and Faults (Workflow)
The process for creating contacts and faults (linework) using geomapmaker consists of 1) line template creation and management and 2) editing the [CONTACTS AND FAULTS](#contacts-and-faults) table.

The Contacts and Faults button in the toolbar will open a form for creating a new TEMPLATE.

![Screenshot 2022-05-24 at 10 27 37 AM](https://user-images.githubusercontent.com/10422595/170096235-c741e9a3-7c21-44e3-8ba0-19ec630e4512.png)

Once a template is created will appear in the ESRI ArcGIS Pro CREATE FEATURES pane. This pane can be accessed by going to EDIT in the top ribbon and selecting any of the buttons in the FEATURES group.

![Screenshot 2022-05-24 at 10 30 20 AM](https://user-images.githubusercontent.com/10422595/170096892-c1f74e67-20d7-4a78-aabd-8ea64010ac29.png)

Once a template is selected from the CREATE FEATURES pane it can then be used to draw lines on the map. Lines added in this way will automatically write to the [CONTACTS AND FAULTS](#contacts-and-faults) table.

### Map Unit Polygons (Workflow)
As a method of passively enforcing GeMS topology rules and consistency with the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table, the geomapmaker workflow purposefully discourages the hand-drawing of polygons. Rather, users are encouraged to follow a linear workflow where they 1) draw all of their contacts (and faults) using the [CONTACTS AND FAULTS](#contacts-and-faults-workflow) workflow and 2) also registered and attributed all of their map units using the [MAP UNITS (BUTTON)](#map-units-button).

Once these preliminary steps are completed, users can use the relevant polygon borders and map unit using the CREATE POLYGON(S)FROM CONTACTS button. This will open a form where you can create one or more valid polygons based on selected contacts. The form is rather simple and really only has two major components 1) a SELECT LINES button that will activate the mouse-click select tool so that the lines making the polygon boundary can be selected and 2) a picklist list of valid map units registered in the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table to associate with the created polygon. The tool should not duplicate/overwrite existing polygons, but caution is recommended.

![Screenshot 2022-08-19 at 10 54 33 AM](https://user-images.githubusercontent.com/10422595/185679102-4f4bbec8-081b-45bf-85da-58a0eef96c04.png)

Users may want to all valid polygons at once rather than manually selecting boundaries. In this case, users can use the GENERATE POLYGONS button in the same section of the toolbar. This tool should not duplicate/overwrite existing polygons, but caution is recommended. The polygons created by this option will not be assigned to a specific map unit, but rather will be universally labelled as UNASSIGNED and have a characteristic white and red hatched appearance. Users can then select one or more polygons at a time and assign them to the correct unit using the EDIT OF MAP UNIT POLYGONS TOOL.

![Screenshot 2022-08-19 at 11 36 23 AM](https://user-images.githubusercontent.com/10422595/185689196-53866127-ad8c-4ebe-8048-f2c7879acf01.png)

### Orientation Points and Stations (Workflow)
The Stations button in the toolbar opens a form for creating a new station. Users can choose to enter all of their stations (localities) first using this form and then associate one or more Orientation Points (measurements) with each station. This approach is recommended in situations where many orientation measurements have been taken from the same locality. 

![Screenshot 2022-08-19 at 12 08 15 PM](https://user-images.githubusercontent.com/10422595/185690079-c19df099-dfea-4827-9719-513460f666bc.png)

Alternatively, users can bypass the stations field entirely as it is **not** a required GeMS table. In this case, users can click directly on the Orientation Points button and add new points by manually clicking on the map or entering coordinates. 

![Screenshot 2022-08-19 at 12 17 49 PM](https://user-images.githubusercontent.com/10422595/185691475-637ed28d-4399-4879-993c-eebde1089390.png)

Users wanting to add multiple stations or orientation points at once from an external table such as an excel file or CSV should use the native import tools provided by ArcPro.

### Glossary (Button)
The glossary button provides several different options for managing the [GLOSSARY table](#glossary), which is a required table in the GeMS specification. The most unique of these options is the Undefined Terms tab. Each time the glossary button is activated in the toolbar it will conduct an automatic check of all fields and data values that require a definition according to the GeMS specification. Users will then be prompted to enter a definition for each undefined term. 

![Screenshot 2022-08-19 at 12 32 25 PM](https://user-images.githubusercontent.com/10422595/185693908-afac9217-73f7-4d52-8edd-49c3f9f8525a.png)

Many terms will be reused across geologic maps. Users looking to do a bulk edit/upload to the Glossary table can add a table named PredefinedTerms to their ArcPro project that uses the following format.

![Screenshot 2022-08-19 at 12 41 10 PM](https://user-images.githubusercontent.com/10422595/185695214-c9dd0822-0d55-4e22-8171-96b2ffd6bcca.png)

Users can create their own table of PredefinedTerms or they can download a pre-made form used by the Arizona Geological Survey directly through geomapmaker by going to TOOLS -> ADD PREDEFINED TERMS. Once this table (whether user-generated or downloaded through geomapmaker) users can then populate the glossary table using TOOLS -> INSERT GLOSSARY TERMS 

![Screenshot 2022-08-19 at 12 42 27 PM](https://user-images.githubusercontent.com/10422595/185695722-bd954651-a920-43cc-ab99-82979b24866e.png)

### Validation Report (Button)
The (validation) report button will automatically generate an html report detailing whether the current GeMS geodatabase currently meets the official [Geologic Map Schema](https://pubs.er.usgs.gov/publication/tm11B10). This report is a slightly modified port of the [USGS validation report tool](https://github.com/usgs/gems-tools-pro). The main difference between the two being that the geomapmaker report **does not conduct topology checks**. Geomapmaker will likely add separate tools for topology checking in v1.1. Users are encouraged to use the geompamaker report during their primary quality control phase, butrun the USGS tool as a final step before publication.

Common difficulties encountered during validation include:
1. Missing ID field values, this can be corrected by going to TOOLS -> SET ALL PRIMARY KEYS
2. Orientation Points not matched to underlying map unit, this can be corrected by going to TOOLS -> SET MAP UNIT VALUES FOR POINTS

### Export (Button)
The official [Geologic Map Schema](https://pubs.er.usgs.gov/publication/tm11B10) requires a number of different file types as part of a publication pacakge. While these files can be generated using native tools built in ArcPro and do not require the geomapmaker toolbar, an export interface has been included in the toolbar for convenience. 

![Screenshot 2022-08-19 at 12 55 47 PM](https://user-images.githubusercontent.com/10422595/185697352-50aff863-4a8f-4b5c-8ecc-d2721bf539b0.png)

## Table Definitions

### Map Unit Polys
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc.

````SQL
MapUnitPolys_ID integer PRIMARY KEY -- Not sure how this will conflict witht he arc OBJECTID, ugh
MapUnit text REFERENCES DescriptionOfMapUnits(MapUnit) NOT NULL
IdentityConfidence text REFERENCES Glossary(Term) NOT NULL
Label text -- This is a duplicate of the MapUnit field, not sure why it exists, can be left blank, but the field should be there
Symbol text REFERENCES DescriptionOfMapUnits(AreaFillRGB) -- redundant with DMU, but required by spec
Notes text
DataSourceID text REFERENCES DataSources(DataSources_ID) NOT NULL
geom geometry -- Polygon only (multi-polygon not allowed), other checks constraints may be appropriate, see special considerations
````

### Contacts And Faults
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc.

````SQL
ContactsAndFaults_ID serial PRIMARY KEY
Type text REFERENCES glossary(term) NOT NULL
IsConcealed CREATE TYPE yesno AS (Y text, N text) NOT NULL -- the 'Y' 'N' is part of the spec
LocationConfidenceMeters numeric NOT NULL -- there is a recommended table of values for this, see "special considerations section"
ExistenceConfidence text REFERENCES Glossary(Term) NOT NULL
IdentityConfidence text REFERENCES Glossary(Term) NOT NULL
Label text
Symbol text NOT NULL -- This will have to be handled in a special way, see "special considerations section"
DataSourceID text REFERENCES DataSources(DataSources_ID) NOT NULL
Notes text
geom geometry -- line only (multi-line not allowed), other checks constraints may be appropriate, see special considerations
````

### Description Of Map Units
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc. Non-spatial tables do not belong to an Arc feature dataset

````SQL
DescriptionOfMapUnits_ID serial PRIMARY KEY
MapUnit text UNIQUE NOT NULL
Name text UNIQUE NOT NULL -- This needs to be compared against geolex and macrostrt, see "NAME CHECK BUTTON" section.
FullName text UNIQUE
Age text NOT NULL -- This needs to be handled in a special way, see "special considerations, Ages"
RelativeAge text -- Off specification, see special considerations Ages
Description text
HierarchyKey text NOT NULL -- I'm not actually clear what this is. I will have to look at some practical examples.
ParagraphStyle  CREATE TYPE headings (heading1 text, heading2 text, etc.) NOT NULL
Label text -- This is a duplicate of the MapUnit field, not sure why it exists, can be left blank, but the field should be there
Symbol text NOT NULL -- This will have to be handled in a special way, see "special considerations section"
AreaFillRGB text -- Specifically must be of format 'xxx;xxx;xxx'
hexcolor text -- this is not part of the specification, but I find it more useful than RGB so I've added it as another field
DescriptionSourceID text REFERENCES  DataSources(DataSources_ID) NOT NULL
GeoMaterial text REFERENCES GeomaterialDict
GeoMaterialConfidence text REFERENCES Glossary(Term)
````

### DataSources
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc. Non-spatial tables do not belong to an Arc feature dataset

````SQL
DataSources_ID serial PRIMARY KEY
Source text NOT NULL
Notes text
URL text
````

### Glossary
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc.

````SQL
Glossary_ID serial PRIMARY KEY
Term text UNIQUE -- its not clear if this actually can/must be unique, see special considerations FKEYS vs. Check
Definition text
DataSourceID text REFERENCES DataSources(DataSources_ID)
````

## Special Considerations

### Symbology
The previous AZGS workflow was dependent on [Cartographic Representations](https://desktop.arcgis.com/en/arcmap/latest/map/working-with-layers/introduction-to-the-cartographic-representations-tutorial.htm) for handling styling information. CartoReps are no longer supported in ArcPro and have been superseded by .stylex files). 

There are two downsides of .stylex from the perspective of the toolbar.
1. No robust .stylex exists of FGDC symbology (though several attempts by other surveys can be found at the NGMDB website[(https://ngmdb.usgs.gov) and the quality of these will likely improve over time.
2. A .stylex is loaded/transferred separately from a geodatabase, which increases the likelihood of data loss.

We have solved both of these problems by creating an [ESRI compatible](https://github.com/Esri/cim-spec) symbology table that can be downloaded directly through the geomapmaker toolbar and can be exported as part of the final geodatabase as a standard non-spatial table. This ensures that the symbology information will always travel with the geodatabase. 

We do also provide an equivalent .stylex file that can be [downloaded manually](/SetUp/SourceMaterials/Geomapmaker.stylx) from this repository, but the *toolbar requires* the aforementioned non-spatial table version to work properly.

### Glossary Template
We currently provide a CSV of pre-defined terms that commonly require definition according to GeMS rules. Users can download this CSV directly through the toolbar by going to TOOLS -> ADD PREDEFINED TERMS. Our current list of predefined terms is fairly small, but we are working with the [Alaska Department of Natural Resources: Geological & Geophysical Survey](https://dggs.alaska.gov/maps-data/enterprise-db.html) to provide a more comprehensive table of terms in the future.

![Screenshot 2022-08-19 at 12 42 27 PM](https://user-images.githubusercontent.com/10422595/185695722-bd954651-a920-43cc-ab99-82979b24866e.png)

This step is optional, however, and users can create their own table (following the same structure) or skip this step entirely and fill in the glossary definitions manually using the glossary tool. 

### Handling Ages
The GeMS specification asks that the [DescriptionOfMapUnits](#description-of-map-units) table have a field `Age`. There are two problems with the way that this is currently specified.

1. The surficial geologists generally do not have precise/absolute ages. They often only know that a particular rock body is <10,000 yrs old or that it is older that Unit A, but younger than Unit B. It is very hard to convey this type of informal, qualitative information in the Ages field as it should be properly defined. Therefore, I have added another field, tentatively titled `RelativeAge`. 

2. The GeMS `Ages` field is just an open textbox and tends to have inconsistent formatting. This causes a huge number of headaches. Instead, we want to enforce a consistent structure of "Older Interval - Younger Interval" always. This is now fixed at the form level, where we prompt them with two separate boxes `older interval` and `younger interval`. These entries are then pasted into the format OI - YI format within the data table to maintain GeMS compliance.

### Location Confidence Meters
This is the recommended table of values for the LocationConfidenceMeters field given in the GeMS specification document. The mappers don't like using the field because they don't know what to put, but we can now point them to this recommendation by using these as default values. However, the toolbar will accept other numeric values. 

*Note, the template GeMS geodatabase created by the USGS GeMS toolbox enforces these values through the domain.*

Value | Definition
---- | ----
10 | Appropriate for well-defined features located by clear-sky GPS, or by inspection of high-resolution topography (e.g., 1m or 2m lidar DEMs), or by inspection of largescale, well-rectified orthophotographs (e.g., NAIP images)
25 | Reasonable value for locations established by inspection of 1:24,000 scale map, or by digitizing paper source maps of that scale
50 | May be appropriate for some “approximate” lines on 1:24,000 scale maps. Other “approximate” lines on the same map may have value of 100 meters, or larger
100 | Appropriate value for features digitized from 1:100,000 scale paper source maps
250 | You really don’t know where a feature is! Or you captured its location from a 1:250,000 scale source map

### Future Work
The toolbar in its current form is focused around the core-tables *required* to produce a GeMS geologic map. There are many other *optional* tables associated with the [GeMS specification]() that mapper's may also want to include in their products - e.g., GeologicLines, OverlayPolygons. AZGS has no plans to support these tables *through the toolbar* at this time or in future updates. An important exception to this, however, is our desire to build-in greater support for geologic cross-sections. The exact nature of these cross-section tools and the proposed development time-line have not yet been finalized.

In addition, it is our expectation to conduct the first round of significant maintenance on this toolbar starting March 2023, with a target release of the first *major patch* in September 2023. Hotfixes may still be periodically released prior to this date and users are encouraged to periodically check here for updates. The most significantly update as part of the next patch will be switching the toolbar over to the ESRI ArcPro 3.x API. Please see (or add to) the [issues](/issues) section of this GitHub repository for other enhancements or bug-fixes that will be includedin v.1.1 of this toolbar.