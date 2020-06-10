# AZGS GEOMAPMAKER SPECIFICATION 2.0
This is a new design specification for the AZGS toolbar. The desired functionality of this toolbar is now completely reworked to be entirely oriented around supproting compliance with the Geologic Map Schema ([draft version](https://ngmdb.usgs.gov/Info/standards/GeMS/docs/GeMSv2_draft7g_ProvisionalRelease.pdf).

## Required Functionality
We breakup the description of functionality into a list of specific [buttons and actions](#buttons) for the toolbar and general [broad-strokes requirements](#broad-strokes-requirements).

## Broad-Strokes Requirements

### Critical 
1. Multiple users must be able to simultaneously edit a geologic map project with versioning
2. Toolbar should be compatible with PostgreSQL ArcSDE
3. Maps must be exportable as a GeMs compliant ESRI File Geodatabase
4. Ability to transfer, select, and view from a list of symbologies, possibly a style file.

### Optional 
1. Must be able to connect over the internet to REST services with relevant dictionaries or configuration files

## Buttons and Actions

### Create Initial Database
There are already existing tools for creating databases, and we can also manually create new databases in advance. Therefore, this is not really a needed button. However, I wanted to record the five critical tables that *must* be in the project. Perhaps we could have this button be some sort of limited pre-validate that checks for these tables? Alternatively this could be an autoamtic check by the application that does not require a button.

1. [MapUnitPolys](#map-unit-polys)
2. [ContactsAndFaults](#contacts-and-faults)
3. [DescriptionOfMapUnits](#description-of-map-units) 
4. [DataSources](#data-sources)
5. [Glossary](#glossary)

### Login
This button will open a login window. The window should have a dropdown selection to pick an existing user or a text box for a create user option. There will be no passwords or other authentication assocaited this is just for record keeping purposes of who is making other successive edits. For this reason, all other toolbar functionality should be locked until a user has logged in. This login will be recorded in the [DataSources table](data-sources).

### Add/Edit Map Unit
This should trigger a form that corresponds directly to the fields and options in the [DescriptionOfMapUnits table](#description-of-map-units). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table.

This should be mostly straightforward as most fields will just require either a picklist or will be an open text field. However, special consideration may be needed for inputting color - fields `AreaFillRGB` and `hexcolor`. I think users would really appreciate having the ability to use some sort of interactive color-picker as part of the form interface, but we can consider this optional if it is too difficult. At worst case, the users will have to use some other tool to pick the color and will enter in the rgb or hexcode values manually.

Another special case are the Ages fields. I have deviated a tiny bit from GeMS standard here becuase of a mixture of feedback from the geologists and because of my own biases. See the [Ages](#ages) section in the [special considerations](#special-considerations) section.

### Add/Edit MapUnitPoly
This should trigger a form that corresponds directly to the fields and options in the [MapUnitPolys table](#map-unit-polys). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table. However, this is overall a very simple feature class with no special considerations.

### Add/Edit ContactsAndFault
This should trigger a form that corresponds directly to the fields and options in the [ContactsAndFaults table](#contacts-and-faults). Note that most fields have constraints of some kind that need to be made explicit and be enforced by the form - e.g., unique, not null, or some reference to another table. 

This is one of the trickiest forms that will require quite a few special considerations.

Scenario | Type | IsConcealed | LocationConfidenceMeters | Symbol
---- | ---- | ---- | ---- | ---- |
A | approximate contact, concealed | NULL | NULL | 1.0.1
B | contact | Y | 50 | 1.0.1

1. The geologists are currently used to filling out the attribute data in the manner of A, but we want to design the form specifically to encourage them to fill things out in style B.

2. We need some way of recording styling information for the lines in the symbol field - see [Symbology And Styles](#symbology-and-styles) in [special considerations](#special-considerations).

## GeMS dictionaries
#### GeoMaterials
#### GeoLex
#### Macrostrat Names
#### Macrostrat Ages

## GeMS feature class and non-spatial table definitions
I have tentatively described these using PostgreSQL data types and not ARC data types, but we should convert or update this as soon as possible to Arc language for consistency.

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
GeoMaterial text -- see "special considerations" 
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

#### Foreign Keys versus a Check Constraint
#### Symbology and Styles
#### Support for non-core tables


https://github.com/usgs/gems-tools-pro
