# -*- coding: utf-8 -*-

import arcpy, os, shutil, time
import logging as log
from datetime import datetime

##########################################################################
#export stuff from https://gis.stackexchange.com/questions/67529/copying-arcsde-geodatabase-to-file-geodatabase-using-arcpy
def formatTime(x):
    minutes, seconds_rem = divmod(x, 60)
    if minutes >= 60:
        hours, minutes_rem = divmod(minutes, 60)
        return "%02d:%02d:%02d" % (hours, minutes_rem, seconds_rem)
    else:
        minutes, seconds_rem = divmod(x, 60)
        return "00:%02d:%02d" % (minutes, seconds_rem)

def getDatabaseItemCount(workspace):
    arcpy.env.workspace = workspace
    feature_classes = []
    for dirpath, dirnames, filenames in arcpy.da.Walk(workspace,datatype="Any",type="Any"):
        for filename in filenames:
            feature_classes.append(os.path.join(dirpath, filename))
    return feature_classes, len(feature_classes)

def replicateDatabase(dbConnection, targetGDB, messages):
    startTime = time.time()

    featSDE,cntSDE = getDatabaseItemCount(dbConnection)
    featGDB,cntGDB = getDatabaseItemCount(targetGDB)

    now = datetime.now()
    #logName = now.strftime("SDE_REPLICATE_SCRIPT_%Y-%m-%d_%H-%M-%S.log")
    #log.basicConfig(datefmt='%m/%d/%Y %I:%M:%S %p', format='%(asctime)s %(message)s',\
    #filename=logName,level=log.INFO)

    messages.addMessage("Old Target Geodatabase: {0} -- Feature Count: {1}".format(targetGDB, cntGDB))
    #log.info("Old Target Geodatabase: %s -- Feature Count: %s" %(targetGDB, cntGDB))
    messages.addMessage("Geodatabase being copied: {0} -- Feature Count: {1}".format(dbConnection, cntSDE))
    #log.info("Geodatabase being copied: %s -- Feature Count: %s" %(dbConnection, cntSDE))

    arcpy.env.workspace = dbConnection

    #deletes old targetGDB
    try:
        shutil.rmtree(targetGDB)
        messages.addMessage("Deleted Old {0}".format(os.path.split(targetGDB)[-1]))
        #log.info("Deleted Old %s" %(os.path.split(targetGDB)[-1]))
    except Exception as e:
        messages.addMessage(e)
        #log.info(e)

    #creates a new targetGDB
    GDB_Path, GDB_Name = os.path.split(targetGDB)
    messages.addMessage("Now Creating New {0}".format(GDB_Name))
    #log.info("Now Creating New %s" %(GDB_Name))
    arcpy.CreateFileGDB_management(GDB_Path, GDB_Name)

    datasetList = [arcpy.Describe(a).name for a in arcpy.ListDatasets()]
    featureClasses = [arcpy.Describe(a).name for a in arcpy.ListFeatureClasses()]
    tables = [arcpy.Describe(a).name for a in arcpy.ListTables()]

    #Compiles a list of the previous three lists to iterate over
    allDbData = datasetList + featureClasses + tables

    for sourcePath in allDbData:
        targetName = sourcePath.split('.')[-1]
        targetPath = os.path.join(targetGDB, targetName)
        if arcpy.Exists(targetPath)==False:
            try:
                messages.addMessage("Atempting to Copy {0} to {1}".format(targetName, targetPath))
                #log.info("Atempting to Copy %s to %s" %(targetName, targetPath))
                arcpy.Copy_management(sourcePath, targetPath)
                messages.addMessage("Finished copying {0} to {1}".format(targetName, targetPath))
                #log.info("Finished copying %s to %s" %(targetName, targetPath))
            except Exception as e:
                messages.addMessage("Unable to copy {0} to {1}".format(targetName, targetPath))
                messages.addMessage(e)
                #log.info("Unable to copy %s to %s" %(targetName, targetPath))
                #log.info(e)
        else:
            messages.addMessage("{0} already exists....skipping.....".format(targetName))
            #log.info("%s already exists....skipping....." %(targetName))
    featGDB,cntGDB = getDatabaseItemCount(targetGDB)
    messages.addMessage("Completed replication of {0} -- Feature Count: {1}".format(dbConnection, cntGDB))
    #log.info("Completed replication of %s -- Feature Count: %s" %(dbConnection, cntGDB))
    totalTime = (time.time() - startTime)
    totalTime = formatTime(totalTime)
    #log.info("Script Run Time: %s" %(totalTime))
########################################################################


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
            displayName="Enterprise geodatabase",
            name="in_egdb",
            datatype="DEWorkspace",
            parameterType="Required",
            direction="Input")

        # Second parameter
        param1 = arcpy.Parameter(
            displayName="File geodatabase",
            name="out_fgdb",
            datatype="DEWorkspace",
            parameterType="Required",
            direction="Output")

        params = [param0, param1]
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
        databaseConnection = parameters[0].valueAsText
        targetGDB = parameters[1].valueAsText
        replicateDatabase(databaseConnection, targetGDB, messages)

        #Set workspace
        arcpy.env.workspace = targetGDB

        #feature classes in feature datasets
        #TODO: Do we care about feature datasets other than GeologicMap?
        featureDatasets = arcpy.ListDatasets(feature_type="Feature")
        for fd in featureDatasets: #loop through each feature class
            messages.addMessage("fd = " + fd);
            featureClasses = arcpy.ListFeatureClasses(feature_dataset=fd)
            for fc in featureClasses: #loop through each feature class
                fc = targetGDB + "\\" + fc
                messages.addMessage("fc = " + fc);          
                fieldList = arcpy.ListFields(fc)  #get a list of fields for each feature class
                for field in fieldList: #loop through each field
                    messages.addMessage("field = " + field.name);
                    messages.addMessage("field alias = " + field.aliasName);
                    messages.addMessage("field required = " + str(field.required));
                    if not field.required:
                        try:
                            arcpy.AlterField_management(fc, field.name, field.name + "x") #rename field to tmp val. Lame, but necessary to convince arc that we really do want to rename the field to itself in mixed case.
                            messages.addMessage("tmp rename over");
                            arcpy.AlterField_management(fc, field.name + "x", field.aliasName) #rename field from alias
                            messages.addMessage("alias over");
                        except:
                            messages.addMessage("failed to rename field " + field.name)

        #Standalone feature classes
        #TODO: Do we care about these?
        featureClasses = arcpy.ListFeatureClasses()
        for fc in featureClasses: #loop through each feature class
            fc = targetGDB + "\\" + fc
            messages.addMessage("fc = " + fc);
            fieldList = arcpy.ListFields(fc)  #get a list of fields for each feature class
            for field in fieldList: #loop through each field
                if not field.required:
                    messages.addMessage("field = " + field.name);
                    messages.addMessage("field alias = " + field.aliasName);
                    try:
                        arcpy.AlterField_management(fc, field.name, field.name + "x") #rename field to tmp val
                        arcpy.AlterField_management(fc, field.name + "x", field.aliasName) #rename field from alias
                    except:
                        messages.addMessage("failed to rename field " + field.name)

        tables = arcpy.ListTables()
        for t in tables: #loop through each feature class
            t = targetGDB + "\\" + t
            messages.addMessage("t = " + t);
            fieldList = arcpy.ListFields(t)  #get a list of fields for each feature class
            for field in fieldList: #loop through each field
                if not field.required:
                    messages.addMessage("field = " + field.name);
                    messages.addMessage("field alias = " + field.aliasName);
                    try:
                        arcpy.AlterField_management(t, field.name, field.name + "x") #rename field from alias
                        arcpy.AlterField_management(t, field.name + "x", field.aliasName) #rename field from alias
                    except:
                        messages.addMessage("failed to rename field " + field.name)
        
        return

