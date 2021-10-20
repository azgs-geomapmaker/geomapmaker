# Validation Rules

## Headings

Field | Type | Rules
---- | ---- | ----
Name | Textbox |<ul><li>Required</li><li>Unique</li><li>MaxLength: 254 chars</li></ul>
Description | Textbox | <ul><li>Required</li><li>MaxLength: 3,000 chars</li></ul>

Action | Rules
---- | ----
Save | <ul><li>No validation errors</li></ul>
Edit | <ul><li>No validation errors</li><li>A change was made</li></ul>
Delete | <ul><li>Extra warning if the heading has any children. Headings/mapunits get orphaned if the parent is removed.</li></ul> 

## Map Units
Field | Type | Rules
---- | ---- | ----
Map Unit | Textbox | <ul><li>Required</li><li>Unique</li><li>[a-zA-Z]</li><li>MaxLength: 10 chars</li></ul>
Name | Textbox | <ul><li>Required</li><li>[a-zA-Z]</li><li>MaxLength: 254 chars</li></ul>
Full Name | Textbox | <ul><li>Required</li><li>Unique</li><li>MaxLength: 254 chars</li></ul>
Older Interval | Combobox | <ul><li>Required</li><li>Assert(youngerInterval.Early_Age <= olderInterval.Early_Age)</li></ul>
Younger Interval | Combobox | <ul><li>Required</li><li>Assert(youngerInterval.Early_Age <= olderInterval.Early_Age)</li></ul>
Relative Age | Textbox | <ul><li>MaxLength: 254 chars</li></ul>
Description | Textbox | <ul><li>MaxLength: 3,000 chars</li></ul>
Label | Textbox | <ul><li>MaxLength: 30 chars</li></ul>
Color | Colorpicker | <ul><li>Required</li><li>Unique</li></ul>
GeoMaterial | Combobox | <ul><li>Required</li></ul>
GeoMaterial Confidence | Combobox | <ul><li>Required</li></ul>

Action | Rules
---- | ----
Save | <ul><li>No validation errors</li></ul>
Edit | <ul><li>No validation errors</li><li>A change was made</li></ul>
Delete | <ul><li>Can't delete if the Map Unit has any Map Unit Polys</li></ul> 
