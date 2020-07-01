using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MinecraftCleanupUtility
{
	class Program
	{
		static string AppData = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
		static string MinecraftPath = AppData + @"\.minecraft";
		static string TechnicPath = AppData + @"\.technic";
		static string MostRecentVersion = GetLatestVersion( false );
		static string MostRecentSnapshot = GetLatestVersion( true );
		static dynamic ProfileTable;
		static bool VersionCheckError = false;

		static string GetLatestVersion( bool snapshot )
		{
			string url = snapshot ? "https://raw.githubusercontent.com/LambdaGaming/MinecraftCleanupUtility/master/latest_snapshot.txt" : "https://raw.githubusercontent.com/LambdaGaming/MinecraftCleanupUtility/master/latest_version.txt";
			WebClient getversion = new WebClient();
			try
			{
				string getdata = getversion.DownloadString( url );
				return getdata;
			}
			catch
			{
				if ( !VersionCheckError )
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine( "\nAn error occurred while getting the latest Minecraft versions. Version cleanup may delete the wrong versions." );
					Console.ResetColor();
					VersionCheckError = true;
				}
			}
			return "";
		}

		static void ParseJSON()
		{
			string path = MinecraftPath + @"\launcher_profiles.json";
			string data = File.ReadAllText( path );
			dynamic newjson = JObject.Parse( data );
			dynamic profiles = newjson.profiles;
			ProfileTable = profiles;
		}

		static void DeleteMinecraftLogs( bool technic )
		{
			string logpath;
			string name;
			logpath = technic ? TechnicPath + @"\logs" : MinecraftPath + @"\logs";
			name = technic ? "Technic" : "Minecraft";
			Console.WriteLine( "\nChecking for " + name + " log files..." );
			string[] getfiles = Directory.GetFiles( logpath );
			if ( !Directory.Exists( logpath ) )
			{
				Console.WriteLine( "\nNo " + name + " log files found. Skipping..." );
				return;
			}

			if ( getfiles.Length > 0 )
			{
				foreach ( string file in getfiles )
				{
					Console.WriteLine( "Deleting " + name + " log file: " + file );
					File.Delete( file );
				}
			}
			else
				Console.WriteLine( "\nNo " + name + " log files found. Skipping..." );

			if ( technic )
			{
				string modpackpath = TechnicPath + @"\modpacks";
				string[] modpacks = Directory.GetDirectories( modpackpath );
				Console.WriteLine( "\nChecking for Technic modpack log files..." );
				if ( !Directory.Exists( modpackpath ) )
				{
					Console.WriteLine( "\nNo Technic modpacks found. Skipping..." );
					return;
				}
				foreach ( string modpack in modpacks )
				{
					string modpacklogpath = modpack + @"\logs";
					string[] logs = Directory.GetFiles( modpacklogpath );
					if ( !Directory.Exists( modpacklogpath ) )
					{
						Console.WriteLine( "\nLog folder not found for " + modpack + ". Skipping..." );
						return;
					}
					foreach( string log in logs )
					{
						Console.WriteLine( "Deleting " + name + " modpack log file: " + log );
						File.Delete( log );
					}
				}
			}
		}

		static void DeleteOldMinecraftVersions()
		{
			Console.WriteLine( "\nChecking for old minecraft versions..." );
			bool foundversions = false;
			string versionpath = MinecraftPath + @"\versions";
			string[] getversions = Directory.GetDirectories( versionpath );
			Dictionary<int, string> delete = new Dictionary<int, string>();
			for ( int i = 1; i < ProfileTable.length; i++ )
			{	
				delete.Add( i, ProfileTable[i].lastVersionId );
			}
			foreach ( string version in getversions )
			{
				string foldername = new DirectoryInfo( version ).Name;
				if ( delete.ContainsValue( version ) || foldername == MostRecentVersion || foldername == MostRecentSnapshot ) continue;
				Console.WriteLine( "Deleting old minecraft version: " + foldername );
				Directory.Delete( version, true );
				foundversions = true;
			}
			if ( !foundversions )
				Console.WriteLine( "\nNo old Minecraft versions found. Skipping..." );
		}

		static void Main( string[] args )
		{
			bool checkoverride = args.Contains( "-checkall" );
			Console.WriteLine( "\nMinecraft Cleanup Utility - \u00a9 2020 LambdaGaming\n\nInitializing..." );
			ParseJSON();
			Console.WriteLine( "\nCheck for Minecraft log files?" );
			if ( checkoverride || Console.ReadKey().Key == ConsoleKey.Y )
				DeleteMinecraftLogs( false );
			Console.WriteLine( "\nCheck for Technic log files?" );
			if ( checkoverride || Console.ReadKey().Key == ConsoleKey.Y )
				DeleteMinecraftLogs( true );
			Console.WriteLine( "\nCheck for old Minecraft versions?" );
			if ( checkoverride || Console.ReadKey().Key == ConsoleKey.Y )
				DeleteOldMinecraftVersions();
			Console.WriteLine( "\nProcess complete. Press any key to continue..." );
			Console.ReadKey();
		}
	}
}
