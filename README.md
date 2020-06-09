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

## Buttons and Actions

### Create Initial Database
This button should create the following five feature classes according to the GeMS specification: 2 feature classes and 3 non-spatial tables.

1. [MapUnitPolys](map-unit-polys)
2. [ContactsAndFaults](contacts-and-faults)
3. [DescriptionOfMapUnits](description-of-map-units) 
4. [DataSources]()
5. [Glossary]()

### Login
This button will open a login window. The window should have a dropdown selection to pick an existing user or a text box for a create user option. There will be no passwords or other authentication assocaited this is just for record keeping purposes of who is making other successive edits. For this reason, all other toolbar functionality should be locked until a user has logged in. This login will be recorded in the DataSources table.

## GeMS dictionaries
#### GeoMaterials
#### GeoLex
#### Macrostrat Names
#### Macrostrat Ages

## GeMS feature class and non-spatial table definitions
I have tentatively described these using PostgreSQL data types and not ARC data types, but we should convert or update this as soon as possible to Arc language.

#### Map Unit Polys
> Note that GeMS specification is case-sensitive, ugh. Also note that the MapUnitPolys feature class must be part of the GeologicMaps 'feature dataset'

````SQL
MapUnitPolys_ID integer PRIMARY KEY -- Not sure how this will conflict witht he arc OBJECTID, ugh
MapUnit text REFERENCES DescriptionOfMapUnits(MapUnit) NOT NULL
IdentityConfidence text REFERENCES Glossary(term) NOT NULL
Label text -- This is a duplicate of the MapUnit field, not sure why it exists, can be left blank, but the field should be there
Symbol text REFERENCES DescriptionOfMapUnits(rgb_values) -- redundant because you can ge this from a simple JOIN with DMU table
Notes text
DataSourceID text REFERENCES DataSourceID(datasourceid) NOT NULL
````

#### Contacts And Faults
> Note that GeMS specification is case-sensitive, ugh. Also note that the ContactsAndFaults feature class must be part of the GeologicMaps 'feature dataset'

````SQL
ContactsAndFaults_ID serial PRIMARY KEY
Type text REFERENCES glossary(term) NOT NULL
IsConcealed CREATE TYPE yesno AS (Y text, N text) NOT NULL -- the 'Y' 'N' is part of the spec
LocationConfidenceMeters numeric NOT NULL -- there is a recommended table of values for this, see "special considerations section"
ExistenceConfidence text REFERENCES glossary(term) NOT NULL
IdentityConfidence text REFERENCES glossary(term) NOT NULL
Label text
Symbol text NOT NULL -- This will have to be handled in a special way, see "special considerations section"
DataSourceID text REFERENCES DataSourceID(datasourceid) NOT NULL
Notes text
````

#### Description Of Map Units
> Note that GeMS specification is case-sensitive, ugh. Non-spatial tables do not belong to a feature dataset

````SQL
DescriptionOfMapUnits_ID serial PRIMARY KEY
MapUnit text UNIQUE NOT NULL
Name text UNIQUE NOT NULL -- This needs to be compared against geolex and macrostrt, see "NAME CHECK BUTTON" section.
FullName text UNIQUE
OldAge text NOT NULL -- This needs to be handled in a special way, see "special considerations"
YoungAge text NOT NULL -- This needs to be handled ina special way, see "special considerations"
RelativeAge text -- Off specification
Description text
HierarchyKey text NOT NULL -- I'm not actually clear what this is. I will have to look at some practical examples.
ParagraphStyle  CREATE TYPE headings (heading1 text, heading2 text, etc.) NOT NULL
Label text -- This is a duplicate of the MapUnit field, not sure why it exists, can be left blank, but the field should be there
Symbol text NOT NULL -- This will have to be handled in a special way, see "special considerations section"
AreaFillRGB text -- Specifically must be of format 'xxx;xxx;xxx'
colorhex text -- this is not part of the specification, but I find it more useful than RGB so I've added it as an optional field
DescriptionSourceID text REFERENCES DataSoruceID(datasoruceid) NOT NULL
GeoMaterial text -- see "special considerations" 
GeoMaterialConfidence text REFERENCES Glossary(terms)
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

````SQL
````

https://github.com/usgs/gems-tools-pro
