using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MinecraftCleanupUtility
{
	class Program
	{
		static string AppData = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
		static string MinecraftPath = AppData + @"\.minecraft";
		static string TechnicPath = AppData + @"\.technic";
		static string MostRecentVersion;
		static string MostRecentSnapshot;
		static List<dynamic> ProfileTable = new List<dynamic>();

		static void GetLatestVersion()
		{
			string path = MinecraftPath + @"\versions\version_manifest_v2.json";

			if ( !File.Exists( path ) )
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine( "Failed to find the file 'version_manifest_v2.json'. Make sure you run the Minecraft launcher at least once before running the cleanup." );
				Console.ResetColor();
				return;
			}

			string data = File.ReadAllText( path );
			dynamic newjson = JObject.Parse( data );
			MostRecentVersion = newjson.latest.release;
			MostRecentSnapshot = newjson.latest.snapshot;
		}

		static void ParseJSON()
		{
			string path = MinecraftPath + @"\launcher_profiles.json";
			string data = File.ReadAllText( path );
			dynamic newjson = JObject.Parse( data );
			dynamic profiles = newjson.profiles;
			foreach ( dynamic profile in profiles )
			{
				foreach ( dynamic version in profile )
					ProfileTable.Add( version.lastVersionId );
			}
		}

		static void DeleteMinecraftLogs( bool technic )
		{
			string logpath = technic ? TechnicPath + @"\logs" : MinecraftPath + @"\logs";
			string name = technic ? "Technic" : "Minecraft";
			bool foundlauncherlog = false;
			Console.WriteLine( "\nChecking for " + name + " log files..." );
			if ( !Directory.Exists( logpath ) )
			{
				Console.WriteLine( "\nNo " + name + " log files found. Skipping..." );
				return;
			}

			if ( !technic )
			{
				string launcherlogpath = MinecraftPath + @"\launcher_log.txt";
				if ( File.Exists( launcherlogpath ) )
				{
					Console.WriteLine( "Deleting launcher_log.txt" );
					File.Delete( launcherlogpath );
					foundlauncherlog = true;
				}
			}

			string[] getfiles = Directory.GetFiles( logpath );
			if ( getfiles.Length > 0 )
			{
				foreach ( string file in getfiles )
				{
					Console.WriteLine( "Deleting " + name + " log file: " + file );
					File.Delete( file );
				}
			}
			else if ( !foundlauncherlog )
			{
				Console.WriteLine( "\nNo " + name + " log files found. Skipping..." );
			}

			if ( technic )
			{
				string modpackpath = TechnicPath + @"\modpacks";
				Console.WriteLine( "\nChecking for Technic modpack log files..." );
				if ( !Directory.Exists( modpackpath ) )
				{
					Console.WriteLine( "\nNo Technic modpacks found. Skipping..." );
					return;
				}

				string[] modpacks = Directory.GetDirectories( modpackpath );
				foreach ( string modpack in modpacks )
				{
					string modpacklogpath = modpack + @"\logs";
					if ( !Directory.Exists( modpacklogpath ) )
					{
						Console.WriteLine( "\nLog folder not found for " + modpack + ". Skipping..." );
						continue;
					}

					string[] logs = Directory.GetFiles( modpacklogpath );
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
			foreach ( string version in getversions )
			{
				bool contains = false;
				string foldername = new DirectoryInfo( version ).Name;
				foreach ( dynamic tableversion in ProfileTable )
				{
					if ( tableversion == foldername )
					{
						contains = true;
						break;
					}
				}
				if ( contains || foldername == MostRecentVersion || foldername == MostRecentSnapshot ) continue;
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
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			Console.WriteLine( "\nMinecraft Cleanup Utility v" + version + " - \u00a9 2021 LambdaGaming\n\nInitializing..." );
			GetLatestVersion();
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
