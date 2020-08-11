# -*- coding: utf-8 -*-

import arcpy
import xml.etree.ElementTree as ET
import os
import os.path as path 

#I was hoping to use xpath on gdb in an xml file, but it does not work properly.
#This is unused.
def fixDataTypes(xmlFile, messages):
    messages.AddMessage("xmlFile = " + xmlFile);
    tree = ET.parse(xmlFile)
    #tables = tree.findall(".//DataElement[Fields/FieldArray/Field/Type='esriFieldTypeSingle']")
    #tables = tree.findall(".//DataElement[Name='ContactsAndFaults']")
    tables = tree.findall(".//DataElement[Name='ContactsAndFaults' or Name='Stations']")
    #tables = tree.findall(".//DataElement")
    
    for table in tables:
        messages.AddMessage(table.find('Name').text)
    return

#Collect fields with wrong type
def collectProblemFields(sourceGDB, messages):
    messages.AddMessage("sourceGDB = " + sourceGDB);
    arcpy.env.workspace = sourceGDB

    problemFields = []

    #feature classes in features datasets
    #TODO: do we care about feature datasets other than GeologicMap?
    featureDatasets = arcpy.ListDatasets(feature_type="Feature")
    for fd in featureDatasets: #loop through each feature class
        #fd = targetGDB + "\\" + fd
        messages.addMessage("fd = " + fd);
        featureClasses = arcpy.ListFeatureClasses(feature_dataset=fd)
        for fc in featureClasses: #loop through each feature class
            fc = sourceGDB + "\\" + fc
            messages.addMessage("fc = " + fc);          
            fieldList = arcpy.ListFields(fc)  #get a list of fields for each feature class
            for field in fieldList: #loop through each field
                messages.addMessage("field = " + field.name);
                if "Single" == field.type:
                    problemFields.append({
                        "featureClass" : fc,
                        "field" : field.name
                    })

        #Standalone feature classes
        #TODO: Do we care about these?
        featureClasses = arcpy.ListFeatureClasses()
        for fc in featureClasses: #loop through each feature class
            fc = sourceGDB + "\\" + fc
            messages.addMessage("fc = " + fc);
            fieldList = arcpy.ListFields(fc)  #get a list of fields for each feature class
            for field in fieldList: #loop through each field
                messages.addMessage("field = " + field.name);
                if "Single" == field.type:
                    problemFields.append({
                        "featureClass" : fc,
                        "field" : field.name
                    })

        tables = arcpy.ListTables()
        for t in tables: #loop through each feature class
            t = sourceGDB + "\\" + t
            messages.addMessage("t = " + t);
            fieldList = arcpy.ListFields(t)  #get a list of fields for each feature class
            for field in fieldList: #loop through each field
                messages.addMessage("field = " + field.name);
                if "Single" == field.type:
                    problemFields.append({
                        "featureClass" : fc,
                        "field" : field.name
                    })

    return problemFields                
    

class Toolbox(object):
    def __init__(self):
        """Define the toolbox (the name of the toolbox is the name of the
        .pyt file)."""
        self.label = "Toolbox"
        self.alias = ""

        # List of tool classes associated with this toolbox
        self.tools = [Tool]


class Tool(object):
    def __init__(self):
        """Define the tool (tool name is the name of the class)."""
        self.label = "Tool"
        self.description = ""
        self.canRunInBackground = False

    def getParameterInfo(self):
        """Define parameter definitions"""
        # First parameter
        param0 = arcpy.Parameter(
            displayName="Source Gems File Geodatabase",
            name="in_egdb",
            #datatype="DEFile",
            datatype="DEWorkspace",
            parameterType="Required",
            direction="Input")
        
        # Second parameter
        param1 = arcpy.Parameter(
            displayName="Target Enterprise Geodatabase",
            name="out_egdb",
            datatype="DEWorkspace",
            parameterType="Required",
            direction="Input")

        params = [param0, param1]
        
        #params = [param0]
        return params

    def isLicensed(self):
        """Set whether tool is licensed to execute."""
        return True

    def updateParameters(self, parameters):
        """Modify the values and properties of parameters before internal
        validation is performed.  This method is called whenever a parameter
        has been changed."""
        return

    def updateMessages(self, parameters):
        """Modify the messages created by internal validation for each tool
        parameter.  This method is called after internal validation."""
        return

    def execute(self, parameters, messages):
        """The source code of the tool."""
        #First, get list of problem fields from source gdb for use later
        pFs = collectProblemFields(parameters[0].valueAsText, messages)
        for field in pFs:
            messages.addMessage("feature class = " + field['featureClass'] + ", field = " + field['field'])

        '''
        #Temp? Create enterprise gdb
        arcpy.CreateEnterpriseGeodatabase_management(
            "POSTGRESQL",
            "127.0.0.1",
            "gems03",
            "DATABASE_AUTH",
            "postgres",
            "password", 
            "",
            "sde",
            "password",
            "", 
            "C:\keycodes.ecp")
        '''
        '''
        #Next, create xml from source gdb
        messages.addMessage("creating tmp xml file");
        arcpy.ExportXMLWorkspaceDocument_management(
            parameters[0].valueAsText,
            path.join(path.dirname(parameters[0].valueAsText), "tmp.xml"),
            "SCHEMA_ONLY"
        )
        
        #Next, import xml to target gdb
        messages.addMessage("importing tmp xml file");
        messages.addMessage("target = " + parameters[1].valueAsText);
        arcpy.ImportXMLWorkspaceDocument_management(
            parameters[1].valueAsText, 
            path.join(path.dirname(parameters[0].valueAsText), "tmp.xml"), 
            "SCHEMA_ONLY"
        )

        #Next, remove the xml file
        messages.addMessage("deleting tmp xml file");
        os.remove(path.join(path.dirname(parameters[0].valueAsText), "tmp.xml"))
        '''
        #Finally, fix all problem fields in target gdb
        messages.addMessage("fixing fields");
        for field in pFs:
            messages.addMessage(
                "Altering type for feature class = " +
                path.join(parameters[1].valueAsText, path.basename(field['featureClass'])) +
                ", field = " + field['field']
            )
            arcpy.AlterField_management(
                path.join(parameters[1].valueAsText, path.basename(field['featureClass'])),
                field['field'],
                field_type="Float"
            ) 
       
        #messages.addMessage("path dirname = " + path.dirname(parameters[0].valueAsText))
        #messages.addMessage("path basename= " + path.basename(parameters[0].valueAsText))
        return
