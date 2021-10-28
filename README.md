# AZGS GEOMAPMAKER SPECIFICATION v2.1
This is the design specification for the [AZGS](https://azgs.arizona.edu/)'s geologic mapping toolbar for ESRI ArcPro (v2.x). This primary purpose of this toolbar is to seamlessly (and discretely) integrate compliance with the 2020 USGS [Geologic Map Schema](https://pubs.er.usgs.gov/publication/tm11B10) into the map digitization process.

## Required Functionality 
We breakup the description of functionality into a list of specific [buttons and actions](#buttons) for the toolbar and general [broad-strokes requirements](#broad-strokes-requirements).

## Broad-Strokes Requirements

### Critical 
1. Multiple users must be able to simultaneously edit a geologic map project with versioning
2. Toolbar should be compatible with PostgreSQL ArcSDE
3. Maps must be exportable as a GeMs compliant ESRI File Geodatabase
4. That when users fill out a feature through the toolbar form that form is enforcing vocabularies and `NOT NULL` constraints according to GeMS.
5. Ability to transfer, select, and view from a pre-made list of symbologies.

### Optional 
1. Able to connect over the internet to REST services with relevant dictionaries or configuration files.
2. The toolbar should fully control the ArcGIS Pro environment to prevent users from using non-toolbar approved functions, which may potentially lead to violations of GeMS or otherwise break the geodatabase.
3. Integrated topology checks during feature creation (e.g., no overlapping polygons).


## Table of Contents
1. [Installation](#installation)
2. BUTTONS AND ACTIONS
    1. [LOGIN](#login)
    2. PROJECTS
        1. [PROJECT CREATION](#project-creation)
        2. [SELECT PROJECT](#select-project)
    3. [SELECT DATA SOURCE](#select-data-source)
    4. [MANAGE MAP UNITS](#add-and-edit-a-map-unit)
    5. [DRAW CONTACTS](#draw-contacts)
    6. [DRAW MAP UNIT POLYGONS](#draw-map-unit-polygons)
3. GEMS TABLE DEFINITIONS
    1. [MAP UNIT POLYS](#map-unit-polys)
    2. [CONTACTS AND FAULTS](#contacts-and-faults)
    3. [DESCRIPTION OF MAP UNITS](#description-of-map-units)
    4. [DATA SOURCES](#datasources)
    5. [GLOSSARY](#glossary)
4. APPENDIX
    1. [AGES](#ages)
    3. [SYMBOLOGY AND STYLES](#symbology-and-styles)
    4. [LOCATION CONFIDENCE METERS](#location-confidence-meters)
    5. [EXISTING GEMS TOOLS](#existing-gems-tools)
    6. [TOPOLOGY CHECKS](#topology-checks)
    7. [HIERARCHY KEYS](#hierarchy-keys)
    8. [AUTOMATED GLOSSARY](#autoamted-glossary)
    9. [HELP BUTTONS](#help-buttons)
    10. [UNIT IMPORT](#unit-import)

### Installation
Working documentation for installation can be found in the [SetUp.md](/SetUp.md) markdown file in the repository.

### Login
This button opens a login window to select or create a new user option. It includes a free-text field to add user-related notes. All other toolbar functionality is locked until a user has logged in.

The 2.0 version of the toolbar did not include any authentication protocol as part of the login (i.e., no passwords), as it was assumed that an honor system would be sufficient for AZGS purposes. However, now that toolbar development is being funded by the USGS, it is possible that other surveys may wish for authentication to be included, and we leave this issue open-ended in v2.1 until feedback from other surveys can be obtained. If authentication is added as a feature in 2.1 then additional logic will likely be necessary to limit [project selection](#select-project) based on user permissions.

Note that login is *distinct* from [data source](#select-data-source) as a user may conceivably enter data from multiple sources and wish to make a distinction.

### Project Creation
There are [existing tools](https://github.com/usgs/gems-tools-pro) for creating GeMS databases and the v2.x implementation of the AZGS toolbar assumes that new geologic mapping projects are added to the PostgreSQL database [in advance](/SetUp.md#to-create-new-azgsgems-db). 

However, it is important to record the five critical tables that *must* be in any valid GeMS project. A project cannot be selected/loaded into the toolbar if it is missing these tables. 

1. [MapUnitPolys](#map-unit-polys)
2. [ContactsAndFaults](#contacts-and-faults)
3. [DescriptionOfMapUnits](#description-of-map-units) 
4. [DataSources](#datasources)
5. [Glossary](#glossary)

Additionally, the toolbar also assumes the following special tables that are not part of the GeMS specification. These tables are not included when exporting a project as an ESRI File Geodatabase, but are used internally by the toolbar to handle styling information.

1. [Symbology](/sql/CFSymbology.sql)

### Select Project
This is a dropdown menu that allows user to select an existing project. Note that there is a known delay between selecting a project and the start of a load indicator that we have not been able to eliminate (issue #27). All other toolbar functionality is locked until a user has logged AND THEN selected a project.

All projects are currenty visible to all users, but this may change if a system of user-based permissions is introduced (see [login](#login))

### Select Data Source
This is not yet implemented. However, it is expected to operate in the same manner as the [login button](#login), but with the following differences.

 1. Authentication will never be a part of the data sources field, regardless of whether it is eventually  added to the main login button.
 2. New DataSources will always be written to the GeMS [DataSources](#datasources) table
 3. A login does not apply to a specific project, but a datasource is always *project specific*.
 4. Once a DataSource is chosen, it should automatically populate to all other database entries (e.g., [adding a new contact](#draw-contacts)).
 
 All other toolbar functionality is locked until a user has logged in AND THEN selected a project AND THEN selected a data source.

### Manage Map Units
This trigger a form-pane that corresponds directly to the fields and options in the [DescriptionOfMapUnits table](#description-of-map-units). However, a few special considerations and modifications to the base table that we have made.

1. `AreaFillRGB` and `hexcolor`. We provide a color-picker rather than requesting a text rgb. We also record both hexcode (better) and rgb in separate columsn on the back end. 

2. `Ages`. See the [Ages](#ages) section in the appendix. To briefly summarize the solution, we now provide an explicit free-text 'relative-age' field AND an explicit picklist of authorized ICS intervals. This includes Eras, Periods, Epochs, and Stages. Substages and regional stages are not included, but could be supported if desired by the community. In addition, we enforce the separation of early age and late age into separate fields (as GeMS should have done!! 🤬)

3. `GeoMaterials`. A new field required by GeMS is the GeoMaterials field, which does not have an analogue in NCGMP09. This is not particularly hard to implement for us, because it is just a picklist from values in a special (pre-defined, static) table called GeoMaterialsDict (latest example from https://github.com/usgs/gems-tools-pro). However, some mappers resent having to fill out this field, so the form forces them to put this information in.

4. `Hierarchy Key`. This issue has not yet been resolved. See the [Hierarchy keys](#hierarchy-keys) section in the appendix.

### Draw Contacts
This triggers a form that corresponds directly to the fields and options in the [ContactsAndFaults](#contacts-and-faults) table of GeMS. However, a few special considerations and modifications to the base table that we have made are listed below.

Scenario | Type | IsConcealed | LocationConfidenceMeters | Symbol
---- | ---- | ---- | ---- | ---- |
A | approximate contact, concealed | NULL | NULL | 1.0.1
B | contact | Y | 50 | 1.0.1

1. The mappers are currently used to filling out  attribute data in the manner of A, but we have designed the form to require that informaton be filled out in accord with scenario B. A recommended table of values for [LocationConfidenceMeters](#location-confidence-meters) can be found in the appendix.

2. The inconsistent (and terrible 🤬) manner in which ESRI has handled styling and symbology information makes it impossible to support symbol selection using base ESRI tools (i.e., stylex). We have resolved this by creating a custom set of symbology tables (modified from the uderlying SQLite design of stylex) that live on the PostgreSQL backend. For more information on how these are handled, see [Symbology And Styles](#symbology-and-styles) in the appendix.

3. Different surveys handle the relationship between lines and polygons differently, but our toolbar currently enforces that linework be completed before map unit polygons can be added. This is because we create map unit polygons *from* lines (see [Draw Map Unit Polygons](#draw-map-unit-polygons))

4. Users can select existing contacts both with a click-select tool (implemented) and from a dropdown (not yet implemented) to 'copy' the attributes of a previous contact. This is a heavily requested feature from the mappers.

5. `Glossary`. Whether definitions should be checked for and/entered when entering a new contact type OR if this process should simply be done at the end of the mapmaking process by running the [USGS validation tool](https://github.com/usgs/gems-tools-pro) has not been resolved. See the [Automated Glossary](#automated-glossary) section in the appendix.

### Draw Map Unit Polygons
This button works as a simple form where users select an existing map unit (generated from the [manage map units](#manage-map-units) button) and then select the enclosing lines (contacts) and convert into a polygon. There are very few fields in this form, which are all taken straight from GeMS.

We have determined that overlapping polygons should be forbidden at this stage, but it has not yet been determined what, if any, topology checks should be included (see [toplogy checks](#topology-checks) in the appendix for further discussion)

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

### Support for non-core tables
The [core tables](#project-creation) are just a small portion of all tables that might be found in GeMS database. However, the majority of those other tables are so rarely used that might as well be created manually on a case-by-case basis. 

There are four tables that the AZGS does sometimes, but not always, use. The question is which of these should also be supported:
1. Stations
2. OrientationPoints
3. Correlation of Map Units tables
4. Cross Sections

We have already determined that Stations and OrientationPoints will be supported via an implementation that is essentially a direct mirror of the [draw contacts](#draw-contacts) button. No determination has been made as to how or if correlations and/or cross-sections will be supported by the toolbar.

### Location Confidence Meters
This is the recommended table of values for the LocationConfidenceMeters field given in the GeMS specification document. The mappers don't like using the field because they don't know what to put, but we can now point them to this recommendation.

Value | Definition
---- | ----
10 | Appropriate for well-defined features located by clear-sky GPS, or by inspection of high-resolution topography (e.g., 1m or 2m lidar DEMs), or by inspection of largescale, well-rectified orthophotographs (e.g., NAIP images)
25 | Reasonable value for locations established by inspection of 1:24,000 scale map, or by digitizing paper source maps of that scale
50 | May be appropriate for some “approximate” lines on 1:24,000 scale maps. Other “approximate” lines on the same map may have value of 100 meters, or larger
100 | Appropriate value for features digitized from 1:100,000 scale paper source maps
250 | You really don’t know where a feature is! Or you captured its location from a 1:250,000 scale source map

### Hierarchy Keys
Both the `HierarchyKeys` and `ParagraphStyles` fields (moreso ParagraphStyles) are really legacies of the layout design process and complicate the role of the [DescriptionOfMapUnits](#description-of-map-units) table as the location of primarily geologic information. 

We set a two zero minimum of left padding on hierarchy indexes. The ParagraphStyle field is used to distinguish Headings and Map Units.  

(Truncated DMU)
Name | HierarchyKey | ParagraphStyles
---- | ---- | ----
Miscellaneous Units | 001 | Heading
Marcellus Shale | 001-004 | Standard
Skaneateles  | 001-003 | Standard
Ludlowville  | 001-002 | Standard
Moscow | 001-001 | Standard

> Note that the convention is that 001-002 is older than 001-001. Also note that older AZGS databases use the 1.1 format, but GeMS demands using `-` instead of `.` as the separator.

### Topology Checks
It may be prudent to build certain topology checks into toolbar so that invalid features cannot be entered at all. 

There are some downsides to this approach.

1. Some topology rules are not always hard and fast, for example, hanging faults are sometimes permitted. Banning them from the get-go would make it impossible to put a geologically valid data point into the map.
2. However strict our checks are, the checks that really matter will come from the [USGS validation tool](https://github.com/usgs/gems-tools-pro). This may make our checks redundant or misleading.

The pros to this approach are.
1. Would be far less likely to have to retroactively correct dozens of topological errors after running [USGS validation tool](https://github.com/usgs/gems-tools-pro).

The latest thinking is overlapping polygons will be forbidded under [draw map unit polygons](#draw-map-unit-polygons) tool, but other topology checks will not be baked into the toolbar, and users will have to correct topologies at the end of the map making process using some other external tool such as the [USGS validation tool](https://github.com/usgs/gems-tools-pro).

### Automated Glossary
Whether definitions should be checked for and/entered when entering a new contact type OR if this process should simply be done at the end of the mapmaking process by running the [USGS validation tool](https://github.com/usgs/gems-tools-pro) has not been resolved. 

> discussion TBD

### Help Buttons
It may be desirable to add severall "lookup" buttons to the ribbon.

1. Lookup formation names
2. Lookup geologic intervals
3. Lookup FGDC symbology by name rather than number

### Unit Import
The former AZGS toolbar has (not sure if it is still working) a button that let users import Map Units from older maps into a new map project. It has not yet been decided if this feature should be included or not, and if it is included, if it should be expanded to include glossaries and other tables besides the DescriptionOfMapUnits. Feedback from the mappers is needed. 

### Existing GeMS tools
https://github.com/usgs/gems-tools-pro
https://pubs.usgs.gov/tm/2006/11A02/ -- FGDC symbology (PDF and Adobe Illustrator Files)
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/FGDCGeoSym_fonts.zip -- Fonts required for the stylex below
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/DGGS_Map_Symbolization_ver3_ArcGISPro.zip -- Stylex file
