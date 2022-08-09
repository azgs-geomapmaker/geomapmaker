# AZGS GEOMAPMAKER v1.0 RELEASE
This is the design specification for the [AZGS](https://azgs.arizona.edu/)'s geologic mapping toolbar for ESRI ArcPro (v2.9x). This primary purpose of this toolbar is to seamlessly (and discretely) integrate compliance with the 2020 USGS [Geologic Map Schema](https://pubs.er.usgs.gov/publication/tm11B10) into the map digitization process.

## Core Requirements
The following *minimum* features are required for a functional toolbar. The toolbar may in many cases exceed the capabilities listed below.  

- [x] Toolbar should be compatible with Arc Server Enterprise (ArcSDE) and flat ESRI File Geodatabase workflows.
- [x] Must generate a GeMS validation REPORT.
- [x] Maps must be exportable as a GeMs compliant ESRI File Geodatabase.
- [x] Forms for adding features enforce appropriate constraints and vocabularies.
- [x] Ability to transfer, select, and view from a pre-made list of symbologies.

## Table of Contents
1. [Installation](#installation)
2. Toolbar Actions
    1. [DATA SOURCE (DROPDOWN)](#data-source-dropdown)
    2. [DATA SOURCES (BUTTON)](#data-sources-button)
    3. [HEADINGS (BUTTON)](#headings-button)
    4. [MAP UNITS (BUTTON)](#map-units-button)
    5. [HIERARCHY (BUTTON)](#hierarchy-button)
    6. [CONTACTS AND FAULTS (WORKFLOW)](#contacts-and-faults-workflow)
    7. [MAP UNIT POLYGONS (WORKFLOW)](#map-unit-polygons-workflow)
        1. CREATE POLYGON(S) FROM CONTACTS
        2. REASSIGN MAP UNIT OF POLYGON(S)
    9. [ORIENTATION POINTS AND STATIONS (WORKFLOW)](#orientation-points-and-stations-workflow)
    10.[GLOSSARY (BUTTON)](#glossary-button)
    11.[TOOLS (BUTTON)](#tools-button)
        1. [SET ALL PRIMARY KEYS]
        2. [INSERT GLOSSARY TERMS]
        3. [SET MAPUNIT VALUE FOR POINTS]
        4. [ZERO-PAD SYMBOLS]
        5. [ZERO-PAD HIERARCHY KEYS]
        6. [GEOPACKAGE DATASET RENAME]
        7. [LINE/POINT SYMBOL OPTIONS]
        8. [MAPUNITPOLYS SYMBOLOGY AND TEMPLATES]
        9. [CONTACTS FAULTS SYMBOLOGY]
        10. [ORIENTATION POINTS SYMBOLOGY]
    12. [REPORT]
    13. [EXPORT]     
3. GEMS TABLE DEFINITIONS
    1. [MAP UNIT POLYS](#map-unit-polys)
    2. [CONTACTS AND FAULTS](#contacts-and-faults)
    3. [DESCRIPTION OF MAP UNITS](#description-of-map-units)
    4. [DATA SOURCES](#datasources)
    5. [GLOSSARY](#glossary)
    6. [STATIONS](#stations)
    7. [ORIENTATION POINTS](#orientation-points)
4. SUPPORTING TABLES
    1. [SYMBOLOGY]
    2. [GEOMATERIALSDICT]
    3. [GLOSSARY TEMPLATES]
5. SPECIAL CONSIDERATIONS
    1. [HANDLING AGES](#ages)
    3. [SYMBOLOGY AND STYLES](#symbology-and-styles)
    4. [LOCATION CONFIDENCE METERS](#location-confidence-meters)
    5. [EXISTING GEMS TOOLS](#existing-gems-tools)
    6. [TOPOLOGY CHECKS](#topology-checks)
    7. [OTHER GEMS TABLES](#other-gems-tables)
    8. [FUTURE WORK](#future-work)

## Installation
A simple demonstration installation is availabe in [/demo_installation](/demo_installation). This demo currently includes only the geomapmaker addin installation file (in [/geomapmaker_addin](/geomapmaker_addin). The addin can be installed by going to the addin-manager in the ArcPro Settings

![Screenshot 2022-05-27 at 12 39 44 PM](https://user-images.githubusercontent.com/10422595/170779059-11e84426-28e1-40a3-a458-556def262871.png)

The addin manager will automatically work with any tables loaded into the currently active map that follow the GeMS namespace - though see the (Symbology Table)[#] section for special considerations.

### Data Source (Dropdown)
Users must select a data source from the dropdown menu before being able to proceed with other actions (other than creating a new data source user). The list of selectable datasources is drawn from the [DATA SOURCES](#datasources) table.

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

### MAP UNIT POLYGONS (WORKFLOW)
As a method of passively enforcing GeMS topology rules and consistency with the [DESCRIPTION OF MAP UNITS](#description-of-map-units) table, the geomapmaker workflow purposefully discourages the hand-drawing of polygons. Rather, users are encouraged to follow a linear workflow where they 1) draw all of their contacts (and faults) using the [CONTACTS AND FAULTS](#contacts-and-faults-workflow) workflow and 2) also created and attributed all of their map units using the [MAP UNITS (BUTTON)](#map-units-button).

Once these preliminary steps are completed, users can use the relevant polygon borders and map unit using the CREATE POLYGON(S)FROM CONTACTS button. Simply select the borders surrounding the polygons 

[INSERT SCREENSHOT AFTER NAME CHANGE IN LATEST VERSION]


### Stations (Button)
The Stations button in the toolbar opens a form for creating a new station. 

![Screenshot 2022-05-27 at 12 19 34 PM](https://user-images.githubusercontent.com/10422595/170776573-7f2ebd0d-ea53-4b67-961f-8ac3c488927d.png)

### Orientation Points (Workflow)
The process for creating Orientation Points (pointwork) using geomapmaker consists of 1) line template creation and management and 2) editing the [CONTACTS AND FAULTS](#contacts-and-faults) table.

The Contacts and Faults button in the toolbar will open a form for creating a new TEMPLATE.

### Validation (Workflow)
*This workflow is currenlty being revised.)

### Update Fields (Button)
*This button is not yet implemented.*

#### Map Unit Polys
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

#### Contacts And Faults
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

#### Description Of Map Units
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

#### DataSources
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc. Non-spatial tables do not belong to an Arc feature dataset

````SQL
DataSources_ID serial PRIMARY KEY
Source text NOT NULL
Notes text
URL text
````

#### Glossary
> Note that GeMS specification is case-sensitive, We use the proper SQL convention of all lowercase for the PostgreSQL backend, but alias to camelCase or PascalCase within Arc.

````SQL
Glossary_ID serial PRIMARY KEY
Term text UNIQUE -- its not clear if this actually can/must be unique, see special considerations FKEYS vs. Check
Definition text
DataSourceID text REFERENCES DataSources(DataSources_ID)
````

### Ages
The GeMS specification asks that the [DescriptionOfMapUnits](#description-of-map-units) table have a field `Age`. There are two problems with the way that this is currently specified.

1. The surficial geologists generally do not have precise/absolute ages. They often only know that a particular rock body is <10,000 yrs old or that it is older that Unit A, but younger than Unit B. It is very hard to convey this type of informal, qualitative information in the Ages field as it should be properly defined. Therefore, I have added another field, tentatively titled `RelativeAge`. GeMS does not forbid the inclusion of additional fields, though this field will need to be explained in the [Glossary](#glossary).

2. The current Ages is just an open textbox and tends to have inconsistent formatting. This causes a huge number of headaches. Instead, we want to enforce a consistent structure of "Older Interval - Younger Interval" always. This is now fixed at the form level, where we prompt them with two separate boxes `older interval` and `younger interval`. These entries are then pasted into the format OI - YI format within the data table.

### Symbology and Styles
The old toolbar and ArcMap system was dependent on [Cartographic Representations](https://desktop.arcgis.com/en/arcmap/latest/map/working-with-layers/introduction-to-the-cartographic-representations-tutorial.htm) for handling styling information. CartoReps are no longer supported in ArcPro and have been superseded by .stylex files (see [stylex files](#stylex-files) in the appendix for a description of how these work). 

There are two downsides of .stylex from the perspective of the toolbar.
1. No robust .stylex exists of FGDC symbology (though several attempts by other surveys can be found at the NGMDB website[(https://ngmdb.usgs.gov).

2. A .stylex has to be loaded separately from a geodatabase, which increases the likelihood of data loss.

We have solved both of these problems by creating an ESRI compatible style table for FGDC symbols that lives within the backend ArcSDE PostgreSQL database. 

### Help Buttons
It may be desirable to add severall "lookup" buttons to the ribbon.

1. Lookup formation names
2. Lookup geologic intervals
3. Lookup FGDC symbology by name rather than number

### Existing GeMS tools
https://github.com/usgs/gems-tools-pro
https://pubs.usgs.gov/tm/2006/11A02/ -- FGDC symbology (PDF and Adobe Illustrator Files)
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/FGDCGeoSym_fonts.zip -- Fonts required for the stylex below
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/DGGS_Map_Symbolization_ver3_ArcGISPro.zip -- Stylex file
