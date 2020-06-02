using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FolderCleanup
{
	class Program
	{
		static string appdata = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

		static void DeleteMinecraftLogs()
		{
			Console.WriteLine( "Checking for Minecraft log files..." );
			string logpath = appdata + @"\.minecraft\logs";
			string[] getfiles = Directory.GetFiles( logpath );
			if ( !Directory.Exists( logpath ) )
			{
				Console.WriteLine( "No Minecraft log files found. Skipping..." );
				return;
			}
			if ( getfiles.Length > 0 )
			{
				foreach ( string file in getfiles )
				{
					Console.WriteLine( "Deleting Minecraft log file: " + file );
					File.Delete( file );
				}
				return;
			}
			Console.WriteLine( "No Minecraft log files found. Skipping..." );
		}

		static void DeleteOldMinecraftVersions()
		{
			Console.WriteLine( "Checking for old minecraft versions..." );
			string versionpath = appdata + @"\.minecraft\versions";
			string[] getdirs = Directory.GetDirectories( versionpath );
			Dictionary<int, string> delete = new Dictionary<int, string>();
			if ( getdirs.Length > 0 )
			{
				string split;
				int converted;
				foreach ( string dir in getdirs )
				{
					if ( dir.Contains( "w" ) )
						split = dir.Split( 'w' )[1].Trim( 'a' ); // Snapshot version format
					else
						split = dir.Split( '.' )[3]; // Regular version format

					converted = int.Parse( split );
					delete.Add( converted, dir );
				}

				var order = delete.OrderBy( k => k.Value ).First(); // Currently only checks for one version
				Console.WriteLine( "Deleting old Minecraft version: " + order.Value );
				Directory.Delete( order.Value );
				return;
			}
			Console.WriteLine( "No old Minecraft versions found. Skipping..." );
		}

		static void DeleteTempFolders()
		{
			string temppath = @"C:\temp";
			string temppathwindows = @"C:\windows\temp";
			if ( Directory.Exists( temppath ) )
			{
				string[] getfiles = Directory.GetFiles( temppath );
				foreach ( string file in getfiles )
				{
					try
					{
						if ( File.GetLastWriteTime( file ) != DateTime.Today ) // Make sure the temporary file wasn't written to today since it might be still in use
						{
							File.Delete( file );
							Console.WriteLine( "Deleting temporary file: " + file );
						}
					}
					catch ( IOException )
					{
						Console.WriteLine( "Found file: " + file + ", but file is still in use. Skipping..." );
					}
				}
			}
			if ( Directory.Exists( temppathwindows ) )
			{
				string[] getfileswindows = Directory.GetFiles( temppathwindows );
				foreach ( string file in getfileswindows )
				{
					try
					{
						if ( File.GetLastWriteTime( file ) != DateTime.Today )
						{
							File.Delete( file );
							Console.WriteLine( "Deleting temporary file: " + file );
						}
					}
					catch ( IOException )
					{
						Console.WriteLine( "Found file: " + file + ", but file is still in use. Skipping..." );
					}
				}
			}
		}

		static void Main( string[] args )
		{
			Console.WriteLine( "FolderCleanup - \u00a9 2020 LambdaGaming\n\nInitializing..." );
			Console.WriteLine( "Check for Minecraft log files?" );
			if ( Console.ReadKey().Key == ConsoleKey.Y )
				DeleteMinecraftLogs();
			Console.WriteLine( "Check for old Minecraft versions?" );
			if ( Console.ReadKey().Key == ConsoleKey.Y )
				DeleteOldMinecraftVersions();
			Console.WriteLine( "Check for temporary files?" );
			if ( Console.ReadKey().Key == ConsoleKey.Y )
				DeleteTempFolders();
			Console.WriteLine( "Process complete. Press any key to continue..." );
			Console.ReadKey();
		}
	}
}
