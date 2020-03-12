# Design for GEOMAPMAKER
This document outlines the basic design principles and specifications for the new AZGS GEOMAPMAKER toolbar. This toolbar is **not** meant to be tied to any specific database format, such as GeMS. Rather, this toolbar is to be based on an API-like structure where there are a series of core `POST` and `GET` functions that can be layered together to create more complex clients. This 'atomic' approach was necessary because the many different schemas and workflows used by different geologic mappers necessitates an underlying database structure that can handle many-to-many relationships.

## TOOLBAR EVENTS
The following are the core functions of the toolbar, and should be thought of as having a direct correspondence to the final toolbar UI - i.e., a literal one-to-one relationship between a back-end database table and a respective toolbar button/action/event.

### LOGIN 
This button will open a login window. The window should have a dropdown selection to pick an existing user or a text box for a create user option. There will be no passwords or other authentication assocaited. This is just for record keeping purposes of who is making other successive edits. For this reason, all other toolbar functionality should be locked until a user has logged in.

The textbar should correspond to a `INSERT` to the [public.users](#publicusers-table) table.
The dropdown should correspond to a `SELECT DISTINCT user_name FROM public.users`.

### DRAW FEATURE 
This button should trigger the ability to add, edit, or remove features to the map. Ideally, this should make `INSERTS` or `UPDATES` to a single [public.features](#publicfeatures-table) table in the underlying database, but it is possible that we will have to have multiple tables for different classes of geometry - e.g., point, line, polygon - because these 'feature classes' are generally enforced by ESRI workflows. In this latter, more likely case, there would be separate public.point_features, public.poly_features, and public.line_features tables.

### ADD, EDIT, DELETE FEATURE TYPE
This button should trigger the ability to add, edit, or remove a "feature type". A "feature type" is a subset of a feature class such as "points". For example, some points on the map may represents "wells" others may represent "outcrops", each of the different types of point is a "feature type"

### ADD, EDIT, DELETE ATTRIBUTE 
This button should trigger the ability to add, edit, or remove attributes to the database that might want to be added to a feature_type. Examples of frequently used attributes by AZGS are `citation`, `line_type`, `geologic_age`, `stratigraphic_name`.

The form will need to both display existing attributes and have entry for new fields. Therefore, we should expect support for `SELECT`, `INSERTS` and `UPDATES` to the [public.attributes](#publicattributes-table) table. 

### ADD, EDIT, DELETE POSSIBLE VALUES 
This button should initiate a form with a dropdown menu of existing attributes to choose from - i.e., `SELECT DISTINCT attribute_name FROM public.attributes`. Once selected it should offer a form/textbox for adding or editing possible values for the new attribute - i.e., an `INSERT` into the [public.possible_values](#publicpossible_values-table) table.

We can expect that there may be many existing attributes and vocabularies that a user will have to browse against to see if what they wish to enter already exists. We will need to make sure that the various UI options handle this browse correctly.

### ASSIGN VALUE TO GEOMETRY
[insert into public.feature_value_links]

### INHERIT ATTRIBUTES FROM GEOMETRY
[insert into public.feature_value_links]
relations of geom A are now copied for geom B

### DISASSEMBLE ESRI/OPEN FILE GEODATABASE
[deconstruct (normalize or denormalize, depending) an existing file geodatabase into our schema]

### CREATE NEW TOOLBAR-COMPLIANT DATABASE
[create a new, blank database following our schema AND potentially pre-populate elements of the public.users, public.attributes, and public.vocabularies table based on some additional configuration file].

## DATABASE SCHEMA
The following outlines the expected structure of each table and implicitly serves as the ER model description becuase I have stated the primary key and foreign key relationships. Note that I was very lax about `NOT NULL`, `UNIQUE`, and other constraints that may need to be added. 

### public.users table
````SQL
CREATE TABLE public.users (
        id serial PRIMARY KEY, 
        name text UNIQUE NOT NULL, 
        notes text
);
````

### public.features table
````SQL
CREATE TABLE public.features (
        id serial PRIMARY KEY, 
        geom geometry, 
        removed boolean, 
        preceded_by integer REFERENCES public.features(id), 
        superceded_by integer REFERENCES public.features(id), 
        user_id integer REFERENCES public.users(id)
);
````

### public.feature_types table
````SQL
CREATE TABLE public.feature_types (
        id serial PRIMARY KEY,
        name text,
        notes text,
        user_id integer REFERENCES public.users(id)
);
 ````

### public.attributes table
````SQL
CREATE TABLE public.attributes (
        id serial PRIMARY KEY,
        name text,
        data_type text, -- any constraints assumed for the field... for example should it only be integers only or is a long text string expected field
        notes text,
        feature_type_id integer REFERENCES public.feature_types(id),
        user_id integer REFERENCES public.users(id)
);
 ````
 
 ### public.possible_values table
````SQL
CREATE TABLE public.possible_values (
        id serial PRIMARY KEY,
        name text,
        definition text,
        notes text,
        attribute_id integer REFERENCES public.attributes(id) NOT NULL
        user_id integer REFERENCES public.users(id)
);
 ````
 
 ### public.feature_value_links table
 ````SQL
CREATE TABLE public.feature_value_links (
       id serial PRIMARY KEY,
       feature_id integer REFERENCES public.features(id), 
       possible_value_id integer REFERENCES public.possible_values(id),
       user_id integer REFERENCES public.users(id)
);
````

## CLIENT/VIEWS
Buttons for more complex views of our tables will also (i.e., denormalizations of our database) need to be supported both for users and for export purposes.

### GEMS EXPORT

## ADDITIONAL CONSIDERATIONS
1. It is unknown how style/symbology - e.g., polygon color, line dashes - information should be stored in the database.
2. It may be best to work in SQLlite rather than Postgres as PostGIS functionality is not really necessary.
3. We will want to track - probably in the geometry table, but possibly across all tables - either timestamps or some form of version tracking.
