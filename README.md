## TOOLBAR EVENTS
The following are the core functions of the toolbar, and should be thought of as having a direct correspondence to the final toolbar UI - i.e., a literal one-to-one relationship between a back-end database table and a respective toolbar button/action/event.

### LOGIN 
This button will open a login window. The window should have a dropdown selection to pick an existing user or a text box for a create user option. There will be no passwords or other authentication assocaited this is just for record keeping purposes of who is making other successive edits. For this reason, all other toolbar functionality should be locked until a user has logged in.

### DRAW FEATURE 
This button should trigger the ability to add, edit, or remove features to the map. For example, to draw a new rock fromation polygon.

### ADD, EDIT, DELETE FEATURE CLASS

### ADD, EDIT, DELETE ATTRIBUTE(S) OF A FEATURECLASS

### ADD, EDIT, DELETE POSSIBLE VALUES 

### INHERIT ATTRIBUTES FROM OTHER GEOMETRY
[insert into public.feature_value_links]
relations of geom A are now copied for geom B

### GEMS EXPORT

## ADDITIONAL CONSIDERATIONS
1. It is unknown how style/symbology - e.g., polygon color, line dashes - information should be stored in the database.
2. It may be best to work in SQLlite rather than Postgres as PostGIS functionality is not really necessary.
3. We will want to track - probably in the geometry table, but possibly across all tables - either timestamps or some form of version tracking.
