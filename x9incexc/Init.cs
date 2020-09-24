//	History:
//		- 20200924 JC: Moved to it's own class file per standard C# convention.

using System.Collections.Generic;
using X9;
using static X9.Util;

namespace X9IncExc {
	public class Init {

		// Public members
		public readonly ValidSwitches   ValidSwitches;
		public readonly ParsedArgs      ParsedArgs;
		public readonly SettingsClass   Settings;

		// Constructor
		public Init(string[] origArgs) {

			// Command-line syntax
			ValidSwitches = new ValidSwitches();
			ValidSwitches.Add( "newer-than",            @"oldest|after"           , 1, 1);
			ValidSwitches.Add( "older-than",            @"newest|before"          , 1, 1);
			ValidSwitches.Add( "larger-than",           @"bigger-than|smallest"   , 1, 1);
			ValidSwitches.Add( "smaller-than",          @"largest|biggest"        , 1, 1);
			ValidSwitches.Add( "patterns-from",         ""                        , 1, 1);
			ValidSwitches.Add( "filter-macros",         ""                        , 1, 1);
			ValidSwitches.Add( "output-file",           ""                        , 1, 1);
			ValidSwitches.Add( "output-format",         ""                        , 1, 1);
			ValidSwitches.Add( "output-file_excluded",  ""                        , 1, 1);
			ValidSwitches.Add( "output-file_debug",     ""                        , 1, 1);

			// Parse args
			ParsedArgs = new ParsedArgs(origArgs, ValidSwitches);

			// Process args (specific)
			Settings = new SettingsClass(ParsedArgs);

		}
	}

	public class SettingsClass {

		// Public members
		public readonly string PatternsFromFile;
		public readonly List<string> SourceDirs;

		// Constructor
		public SettingsClass(in ParsedArgs argParsedArgs) {

			// Processs parsed switches and parameters
			foreach (KeyValuePair<dynamic, List<dynamic>> item in argParsedArgs.SwitchesAndParams) {

				// Get key (argument switch) and value (list)
				string argSwitch=item.Key.ToString();
				List<dynamic> list = item.Value;

				// For each possible parsed arg, validate and populate specific typesafe settings (this is cleaner than doing the same number of 'TryGetValue' on the dictionary.
				if ( argSwitch == "newer-than" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "older-than" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "larger-than" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "smaller-than" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "patterns-from" ){
					if (list.Count <= 0)                           throw new System.ArgumentException($"No file seems to have been specified for switch '{argSwitch}'.");
					PatternsFromFile=list[0];
					if (PatternsFromFile == "")                    throw new System.ArgumentException($"No file seems to have been specified for switch '{argSwitch}'.");
					if (! System.IO.File.Exists(PatternsFromFile)) throw new System.ArgumentException($"Filespec for '{argSwitch}' either doesn't exist, or is inaccessible: '{PatternsFromFile}'");

				} else if ( argSwitch == "filter-macros" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "output-file" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "output-format" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "output-file_excluded" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else if ( argSwitch == "output-file_debug" ){
					throw new System.ArgumentException($"Sorry, no implementation for '{argSwitch}' exists yet.");

				} else {
					throw new System.ArgumentException($"Unknown switch '{argSwitch}' encountered. This is probably a bug.");

				}
			}

			// Validate source directories
			foreach ( string item in argParsedArgs.StandaloneArgs){
				// Remove empty items
				if (IsNothing_v1(item)) argParsedArgs.StandaloneArgs.Remove(item);
				if (!System.IO.Directory.Exists(item))  throw new System.ArgumentException($"Could not find or access specified directory '{item}'.");
			}
			if (argParsedArgs.StandaloneArgs.Count <=0) throw new System.ArgumentException($"No source folder[s] were specified to scan for files in.");
			SourceDirs = argParsedArgs.StandaloneArgs;

		}
	}
}
