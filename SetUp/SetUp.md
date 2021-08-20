# To set up geomapmaker toolbar in ArcGIS Pro 
1. Install Postgres 
  
    Overall instructions at [https://pro.arcgis.com/en/pro-app/latest/help/data/geodatabases/manage-postgresql/get-started-gdb-in-postgresql.htm](https://pro.arcgis.com/en/pro-app/latest/help/data/geodatabases/manage-postgresql/get-started-gdb-in-postgresql.htm).
    But I used a Postgres version obtained from ESRI (currently E:\geomapmaker-stuff\PostgreSQL_DBMS_for_Windows_115_172304.exe). This bundle includes pgAdmin4.

2. Copy st_geometry.dll to postgres lib. 
    
    This dll must correspond to the versions of Postgres and ArcGIS Pro you are running. Obtaining this dll is left as an exercise for the reader. 

3. Copy Add-In directory (built by VisualStudio) to Add-In directory (currently E:\ArcGISPro-AddIns). 
    
    Follow the "Load an add-in from a well-known folder" instructions at [https://pro.arcgis.com/en/pro-app/latest/get-started/manage-add-ins.htm](https://pro.arcgis.com/en/pro-app/latest/get-started/manage-add-ins.htm) to set up ArcGIS Pro to access this directory. It's not clear to me whether this must be done for each user on a shared machine.

4. Copy keycodes file to keycode directory (currently E:\geomapmaker-stuff\ArcGIS-keycodes). 
    
    This is used when running Create Enterprise Geodatabase from ArcGIS Pro. Once again, obtaining this file is left as an exercise for the reader.

5. In Postgres, create geomapmaker user with password "password" (This is currently hard-coded in the Add-In--I know, I know) 
  
    This user must be made superuser, though I don't know why.

6. In Postgres, create database geomapmaker with schema geomapmaker. Make user geomapmaker the owner.

7. In Postgres, run geomapmaker.sql. This is a dump file. 

    I chose to just run it by pasting into a pgAdmin query window. To do this, I had to comment out the DROP/ADD database lines and the /connect line. Also, the ALTER OWNER statement did not appear to work. I did this manually afterwards.

8. At this point, you should be able to run ArcGIS Pro, click the AZGS tab, click the Login button and complete the login, populating the Project combobox with some test data. Don't try to click these, as the connection data is still bogus.

9. Create azgs/gems db's as described [below](#create-gems). In order for the test data to be able to access the gems dbs, call one gems01, the other gems02, and specify the sde user password to be "password". Otherwise, delete the test data from the geomapmaker db and add entries that correspond to what you create.

 

# <a id="create-gems"></a>To create new azgs/gems db
1. Run Geoprocessing>Create Enterprise Geodatabase

    Database Platform: PostgreSQL
    
    Instance: typically 127.0.0.1
    
    Database: whatever you want to call it
    
    Database Administrator: typically postgres
    
    Database Administrator Password: the password for previous
    
    Geodatabase Administrator Password: running this tool creates a user called sde, if one does not already exist. This password is for that user.
    
    Authorization File: the keycodes file

2. Run Catalog>Databases>New Database Connection 

3. Run Geoprocessing>Import XML Workspace Document 

    Target Geodatabase: the connection from #2
    
    Import File: gems xml template (see [below](#create-template) for how to create this template if necessary) 
    
    Import Options: Import schema only 

4. In Postgres, run azgs sql touchup

    This is currently in file sql\AZGS-customizations.sql. 
    
    It may be possible to make these changes part of the template, but I haven't figured out how yet.

5. To enable this, run the following commands (substituting values corresponding to the above)
	```
	insert into 
		geomapmaker.projects (
			name,
			notes,
			connection_properties
		)
	values (
		'Project 04', 
		'Notes for Project 04', 
		'{"user": "sde", "database": "gems03", "instance": "127.0.0.1", "password": "password"}'
	)
	returning
		id;

	insert into
		geomapmaker.user_project_links (
			user_id, 
			project_id 
		)
	values (<the id from users>, <the id from projects>);
	```

# <a id="create-template"></a> To create Gems template in xml (only necessary if you need to create the template)
1. Install USGS Gems tools in ArcGIS Pro per [https://github.com/usgs/gems-tools-pro
](https://github.com/usgs/gems-tools-pro)

2. Run Catalog>Favorites>gems-tools-pro-master>GEMS_Tools_AGP2.tbx>Create New Database 
    
    Exact path may change with different versions.
    
    This creates a file gdb

3. Run Geoprocessing>Export XML Workspace Document

    This creates the xml template


		
