import json
import os
import platform
import shutil
import sys
from pathlib import Path

AppData = platform.system() == "Linux" and str( Path.home() ) or os.getenv( "APPDATA" )
MinecraftPath = AppData + "/.minecraft"
TechnicPath = AppData + "/.technic"
MostRecentVersion = ""
MostRecentSnapshot = ""
ProfileTable = []

def GetLatestVersion():
	global MostRecentVersion, MostRecentSnapshot
	path = MinecraftPath + "/versions/version_manifest_v2.json"
	if not os.path.exists( path ):
		print( f"Failed to find {path}. Make sure you run the game at least once before running the cleanup." )
		sys.exit()
	data = open( path, "r" )
	newjson = json.loads( data.read() )
	MostRecentVersion = newjson["latest"]["release"]
	MostRecentSnapshot = newjson["latest"]["snapshot"]
	data.close()

def ParseJSON():
	path = MinecraftPath + "/launcher_profiles.json"
	data = open( path )
	newjson = json.loads( data.read() )
	profiles = newjson["profiles"]
	for profile in profiles:
		ProfileTable.append( newjson["profiles"][profile]["lastVersionId"] )

def SafeRemoveFile( file ):
	try:
		os.remove( file )
	except PermissionError:
		print( f"Couldn't delete {file}: Permission denied. Path might be a folder or the file might be in use." )
	except FileNotFoundError:
		print( f"Couldn't delete {file}: File not found." )
	except:
		print( f"Something went wrong while deleting {file}. Skipping..." )

def DeleteMinecraftLogs( technic ):
	logpath = technic and TechnicPath + "/logs" or MinecraftPath + "/logs"
	name = technic and "Technic" or "Minecraft"
	foundlauncherlog = False
	print( f"Checking for {name} log files..." )
	if not os.path.exists( logpath ):
		print( f"No {name} log files found. Skipping..." )
		return

	if not technic:
		launcherlogpath = MinecraftPath + "/launcher_log.txt"
		if os.path.exists( launcherlogpath ):
			print( "Deleting launcher_log.txt" )
			SafeRemoveFile( launcherlogpath )
			foundlauncherlog = True

	getfiles = os.listdir( logpath )
	if len( getfiles ) > 0:
		for file in getfiles:
			print( f"Deleting {name} log file: {file}" )
			SafeRemoveFile( os.path.join( logpath, file ) )
	elif not foundlauncherlog:
		print( f"No {name} log files found. Skipping..." )

	if technic:
		modpackpath = TechnicPath + "/modpacks"
		print( "Checking for Technic modpack log files..." )
		if len( os.listdir( modpackpath ) ) == 0:
			print( "No Technic modpacks found. Skipping..." )
			return
		for modpack in os.listdir( modpackpath ):
			modpacklogpath = f"{modpackpath}/{modpack}/logs"
			if not os.path.exists( modpacklogpath ):
				print( f"Log folder not found for {modpack}. Skipping..." )
				continue
			logs = os.listdir( modpacklogpath )
			if len( logs ) == 0:
				print( f"No log files found for {modpack}. Skipping..." )
				continue
			for log in logs:
				print( f"Deleting {name} modpack log file: {log}" )
				SafeRemoveFile( os.path.join( modpacklogpath, log ) )

def DeleteOldMinecraftVersions():
	print( "Checking for old Minecraft versions..." )
	foundversions = False
	versionpath = MinecraftPath + "/versions"
	getversions = os.listdir( versionpath )
	for version in getversions:
		finalpath = f"{versionpath}/{version}"
		if version in ProfileTable or version == MostRecentVersion or version == MostRecentSnapshot or not os.path.isdir( finalpath ):
			continue
		print( f"Deleting old Minecraft version: {version}" )
		shutil.rmtree( finalpath )
		foundversions = True
	if not foundversions:
		print( "No old Minecraft versions found. Skipping..." )

if __name__ == "__main__":
	checkoverride = "-checkall" in sys.argv
	print( "Minecraft Cleanup Utility | Copyright (c) 2020-2024 OPGman" )
	GetLatestVersion()
	ParseJSON()
	check = input( "Check for Minecraft log files? (y/n)" )
	if checkoverride or check == "y":
		DeleteMinecraftLogs( False )
	check = input( "Check for Technic log files? (y/n)" )
	if checkoverride or check == "y":
		DeleteMinecraftLogs( True )
	check = input( "Check for old Minecraft versions? (y/n)" )
	if checkoverride or check == "y":
		DeleteOldMinecraftVersions()
	print( "Process complete. Press any key to continue..." )
	input()
