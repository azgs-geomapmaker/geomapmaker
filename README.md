# AZGS GEOMAPMAKER SPECIFICATION 2.0
This is a new design specification for the AZGS toolbar. The desired functionality of this toolbar is now completely reworked to be entirely oriented around supproting compliance with the Geologic Map Schema ([draft version](https://ngmdb.usgs.gov/Info/standards/GeMS/docs/GeMSv2_draft7g_ProvisionalRelease.pdf).

## Required Functionality 
We breakup the description of functionality into a list of specific [buttons and actions](#buttons) for the toolbar and general [broad-strokes requirements](#broad-strokes-requirements).

## Broad-Strokes Requirements

### Critical 
1. Multiple users must be able to simultaneously edit a geologic map project with versioning
2. Toolbar should be compatible with PostgreSQL ArcSDE
3. Maps must be exportable as a GeMs compliant ESRI File Geodatabase

### Optional 
1. Must be able to connect over the internet to REST services with relevant dictionaries or configuration files
2. Ability to transfer, select, and view from a list of symbologies, possibly a style file.
3. The most fundamental GeMS requirement are certain topology rules (no overlapping polygons). However, it is unclear if we should build checks for this into the toolbar or treat this as a separate problem. 

## Buttons and Actions

### Create Initial Database
There are already existing tools for creating databases, and we can also manually create new databases in advance. Therefore, this is not really a needed button. However, I wanted to record the five critical tables that *must* be in the project. Perhaps we could have this button be some sort of limited pre-validate that checks for these tables? Alternatively this could be an autoamtic check by the application that does not require a button.

1. [MapUnitPolys](#map-unit-polys)
2. [ContactsAndFaults](#contacts-and-faults)
3. [DescriptionOfMapUnits](#description-of-map-units) 
4. [DataSources](#datasources)
5. [Glossary](#glossary)

### Login
This button will open a login window. The window should have a dropdown selection to pick an existing user or a text box for a create user option. There will be no passwords or other authentication assocaited this is just for record keeping purposes of who is making other successive edits. For this reason, all other toolbar functionality should be locked until a user has logged in. This login will be recorded in the [DataSources table](data-sources).

### Add/Edit Map Unit
This should trigger a form that corresponds directly to the fields and options in the [DescriptionOfMapUnits table](#description-of-map-units). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table. This should be mostly straightforward as most fields will just require either a picklist or will be an open text field. 

However, special considerations include:
1. `AreaFillRGB` and `hexcolor`. I think users would really appreciate having the ability to use some sort of interactive color-picker as part of the form interface, but we can consider this optional if it is too difficult. At worst case, the users will have to use some other tool to pick the color and will enter in the rgb or hexcode values manually in the form as the appropriate text/numeric string.

2. `Ages`. See the [Ages](#ages) section in the [special considerations](#special-considerations) section.

3. `GeoMaterials`. A new field required by GeMS is the GeoMaterials field, which does not have an analogue in NCGMP09. This is not particularly hard to implement for us, because it is just a picklist from values in a special (pre-defined, static) table called [GeoMaterialsDict](#geomaterialsdict). However, the geologists will probably resent having to fill out this field, so we need to ensure that the form strictly forces them to put this information in.

### AddEdit MapUnitPoly
This should trigger a form that corresponds directly to the fields and options in the [MapUnitPolys table](#map-unit-polys). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table. However, this is overall a very simple feature class with no special considerations.

### AddEdit ContactsAndFault
This should trigger a form that corresponds directly to the fields and options in the [ContactsAndFaults table](#contacts-and-faults). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table. 

This is one of the trickiest forms that will require two special considerations.

Scenario | Type | IsConcealed | LocationConfidenceMeters | Symbol
---- | ---- | ---- | ---- | ---- |
A | approximate contact, concealed | NULL | NULL | 1.0.1
B | contact | Y | 50 | 1.0.1

1. The geologists are currently used to filling out the attribute data in the manner of A, but we want to design the form specifically to encourage them to fill things out in style B. A recommended table of values for LocationConfidenceMeters can be found in the [Appendix](#location-confidence-meters).

2. We need some way of recording styling information for the lines in the `Symbol` field - see [Symbology And Styles](#symbology-and-styles) in [special considerations](#special-considerations). It would be great if the form could actually display the symbols as images to make choosing the best symbol easier, but I don't know if that sort of display can be handled in an ESRI form. The alternative is to refer to the items by name in the form (i.e., values in the `Category` field of the .stylex); however, values in the `Key` field will still have to be what is actually recored in the `Symbol` field of the geodatabase. 

## GeMS feature class and non-spatial table definitions
I have tentatively described these using PostgreSQL data types and not ARC data types, but we should convert or **update this as soon as possible to Arc types**.

#### Map Unit Polys
> Note that GeMS specification is case-sensitive, ugh. Also note that the MapUnitPolys feature class must be part of the GeologicMaps 'feature dataset'

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
> Note that GeMS specification is case-sensitive, ugh. Also note that the ContactsAndFaults feature class must be part of the GeologicMaps 'feature dataset'

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
> Note that GeMS specification is case-sensitive, ugh. Non-spatial tables do not belong to a feature dataset

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
> Note that GeMS specification is case-sensitive, ugh. Non-spatial tables do not belong to a feature dataset

````SQL
DataSources_ID serial PRIMARY KEY
Source text NOT NULL
Notes text
URL text
````

#### Glossary
> Note that GeMS specification is case-sensitive, ugh. Non-spatial tables do not belong to a feature dataset

````SQL
Glossary_ID serial PRIMARY KEY
Term text UNIQUE -- its not clear if this actually can/must be unique, see special considerations FKEYS vs. Check
Definition text
DataSourceID text REFERENCES DataSources(DataSources_ID)
````

## Special Considerations
#### Ages
The GeMS specification asks that the [DescriptionOfMapUnits](#description-of-map-units) table have a field `Age`. There are two problems with the way that this is currently specified.

1. The surficial geologists (Ann Youberg, Joe Cook, Phil Pearthree, Brian Gootee, and Jeri Young) generally do not have precise/absolute ages. They often only know that a particular rock body is <10,000 yrs old or that it is older that Unit A, but younger than Unit B. It is very hard to convey this type of qualitative information in the Ages field as it should be properly defined. Therefore, I have added another field, tentatively titled `RelativeAge`. GeMS does not forbid the inclusion of additional fields, though this field will need to be explained in the [Glossary](#glossary).

2. The current Ages is just an open textbox and tends to have inconsistent formatting. This causes a huge number of headaches. Instead, we want to enforce a consistent structure of "Older Interval - Younger Interval" always. This should probably be handled at the form level, where we will prompt them with two separate boxes `older interval` and `younger interval` and the form will past them together into OI - YI format within the data table.

#### Foreign Keys versus a Check Constraint versus Application Logic
If you look at the structure of the [tables](#gems-feature-class-and-non-spatial-table-definitions) there are a number of fields where I specified a foreign key constraint. However, in some cases, these are not truly foreign keys because the table.field being referenced does not necessarily need to obey a `UNIQUE` constraint, in which case it cannot be used as a foreign key. These scenarios would be better described by using a `CHECK` constraint. 

An obvious altnerative would be not to mess around in the GeoDatabase with either a `CHECK` or `FOREIGN KEY` constraint, but rather enforced these relations through form with application logic. This is probably the way to go, because Arc is probably not going to play nice with a lot of PostgreSQL driven constraints (we've more or less learned that the hard way already). However, it would be AWESOME to be able to add certain topology constraints for example:

````PLpgSQL
ALTER TABLE "MapUnitPolys"
  ADD CONSTRAINT geometry_valid_check
	CHECK (ST_IsValid(geom));
````

#### Symbology and Styles
The old toolbar and ArcMap system was dependent on [Cartographic Representations](https://desktop.arcgis.com/en/arcmap/latest/map/working-with-layers/introduction-to-the-cartographic-representations-tutorial.htm) for handling styling information.

CartoReps are no longer supported in ArcPro, which means we need a different way to handle styling information. The most promising route will be for us to have a .stlyex file (see [stylex files](#stylex-files) in the appendix for a description of how these work). This would probably be the most ESRI way to do things, and I like it for that reason. The Alaska Geological Survey has made a .style file of the FGDC symbology (can be found [here](https://ngmdb.usgs.gov/Info/standards/GeMS/docs/AKDGGS-style_10-2013.zip) which can be converted into a .stylex in ArcPro.

One downside of this, however, is that the .stylex has to be loaded separately from the database. Perhaps we can think of a way to embed the .stylex as part of the toolbar or the database. Otherwise this will potentially be a nightmare for data transfer and updating. You can also only symbolize by matching a stylex to an attribute in arcpro versions >2.5

#### Support for non-core tables
The core tables I [described above](#create-initial-database) are just a small portion of all tables that might be found in GeMS database. However, the majority of those other tables are so rarely used that might as well be created manually on a case-by-case basis. 

There are four tables that the AZGS does sometimes, but not always, use. The question is which of these should also be supported:
1. Stations (should be a fairly straightforward form with no real special considerations)
2. OrientationPoints (Shares the same special considerations as [ContactsAndFaults](#addedit-contactsandfaults)
3. Correlation of Map Units tables
4. Cross Sections

More research will need to be done with the geologists to see which of these they truly need help with. I am also unclear on this time exactly how the cross sectiona and correlation of map units table will need to be implemented.

#### SQL Server vs. PostgreSQL
One of the main goals is to switch to PSQL to save on licensing fees and to gain access to backend PostGIS capabilities, but it might be less performant than SQL Server. If the performance benefits are big enough, it would worth the licensing fees. We can think about a test case using tablet upload case of Brian/Joe.

## Appendix Information
#### Stylex Files
“Styles” are databases that hold symbols, symbol primitives (like colors), and layout elements like north arrows and scale bars. (from the Esri Cartography MOOC)

A .stylx file is a SQLite database containing symbols etc., described in JSON (JavaScript Object Notation). Stylx is an open spec documented in the open Cartographic Information Model (CIM) specification (https://github.com/Esri/cim-spec). You can view a .stylex by just changing its extension to sqlite. Theoretically we might be able to do some sort of PostgreSQL implementation.

Examples of point symbols defined in JSON: https://github.com/cmrRose/cim-spec/blob/master/docs/v2/Example-Symbols.md 

Overview of the symbols in a style: https://github.com/Esri/cim-spec/blob/master/docs/v2/Overview-Symbols.md 

A good starting location if we wanot to build a .stylex: http://dggs.alaska.gov/public_files/Ekberg/FGDC_style_file_fonts/

Full set of notes from [Caroline Rose](https://docs.google.com/document/d/1m3EpqVClDAa6Lyd14uZ_FX20gTbT730n3fDfQLBbhw4/edit#heading=h.4q5u3xtlwbbg) in a google doc. Includes an outline of how to make our own styles.

#### Location Confidence Meters
This is the recommended table of values for the LocationConfidenceMeters field given in the GeMS specification document. The geologists don't like using the field because they don't know what to put, but we can now point them to this recommendation.

Value | Definition
---- | ----
10 | Appropriate for well-defined features located by clear-sky GPS, or by inspection of high-resolution topography (e.g., 1m or 2m lidar DEMs), or by inspection of largescale, well-rectified orthophotographs (e.g., NAIP images)
25 | Reasonable value for locations established by inspection of 1:24,000 scale map, or by digitizing paper source maps of that scale
50 | May be appropriate for some “approximate” lines on 1:24,000 scale maps. Other “approximate” lines on the same map may have value of 100 meters, or larger
100 | Appropriate value for features digitized from 1:100,000 scale paper source maps
250 | You really don’t know where a feature is! Or you captured its location from a 1:250,000 scale source map

#### GeoMaterialsDict
The GeMS specification has a static table called GeoMaterialsDict that links into the GeoMaterials field. This is required by GeMS and is one of the most significant changes from NCGMP09. The geologists will not be happy about this change, so we need to make sure that the form strictly enforces their compliance.

We do not yet have the finalized version of this table, but I will add a link to it once it is released (presumably in July).

#### Existing GeMS tools
https://github.com/usgs/gems-tools-pro
https://pubs.usgs.gov/tm/2006/11A02/ -- FGDC symbology (PDF and Adobe Illustrator Files)
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/FGDCGeoSym_fonts.zip -- Fonts required for the stylex below
https://ngmdb.usgs.gov/Info/standards/GeMS/docs/DGGS_Map_Symbolization_ver3_ArcGISPro.zip -- Stylex file
