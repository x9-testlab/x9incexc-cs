using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using X9Tools;
//using static X9Tools.Str;
//using static X9Tools.OS;
using static X9Tools.Terminal_v1;
using static X9Tools.Misc;

namespace X9Tools.Args {

	public class ParsedArgs_v1 {

		// Public members
		public readonly string[]          ArgArray;
		public readonly string            ArgStr;
		public readonly DictionaryOfLists SwitchesAndParams;
		public readonly List<string>      StandaloneArgs;

		// Constructor
		public ParsedArgs_v1( in string[] argArray, in ValidSwitches_v1 argValidSwitches ) {

			// Populate member variables/properties
			ArgArray=argArray;
			ArgStr=String.Join(" ", ArgArray).ToString();

			// Init member variables
			SwitchesAndParams = new DictionaryOfLists();
			StandaloneArgs    = new List<string>();

			// Loop processing variables
			bool           didProcessArg                     = false ;
			bool           nextCanBeSwitch                   = true  ;
			bool           nextCanBeSwitchParam              = false ;
			bool           nextMustBeSwitchParam             = false ;
			bool           nextCanBeSwitchless               = true  ;
			bool           nextMustBeSwitchlessOrNothing     = false ;
			bool           nextCanBeNothing                  = true  ;
			string         currentCanonicalSwitchName        = ""    ;  // Available as currentCanonicalSwitchSpec.canonicalName, but you have to check for currentCanonicalSwitchSpec is null first.
			string         currentSwitchAsPassed             = ""    ;  // Available as currentCanonicalSwitchSpec.canonicalName, but you have to check for currentCanonicalSwitchSpec is null first.
			string         currentSwitchNormalized           = ""    ;  // '--' stripped off, lower-cased.
			string         lastArg                           = ""    ;  //
			long           currentMinParamCount              =  0    ;  // Another shortcut.
			long           currentMAXParamCount              =  0    ;  // Another shortcut.
			ValidSwitch_v1 currentCanonicalSwitchSpec;

			// Loop through array and set up stuff
			foreach ( string arg in argArray ) {
				if ( arg == "" ) continue;  // Skip the rest for this loop
				didProcessArg=false;

				// Determine if it's a switch
				if ( Regex.IsMatch( arg, @"^--[^ \-]*" ) ) {
					// It's a switch.

					// Validate
					if ( ! nextCanBeSwitch ) {
						if ( nextMustBeSwitchParam )         throw new System.ArgumentException($"Cannot process switch '{arg}', as a[nother] parameter for {lastArg} was still expected.");
						if ( nextMustBeSwitchlessOrNothing ) throw new System.ArgumentException($"Cannot process switch '{arg}', as it occurs after '{lastArg}', which was interpreted as the end of switch processing.");
						throw new System.ArgumentException($"Did not expect switch '{arg}' at this point.");
					}

					// Get switch specification based on regex match on passed switch (lower case, stripped of '--').
					currentSwitchAsPassed=arg;
					currentSwitchNormalized=Regex.Replace( arg.ToLower(), @"-{1,2}(.*)", @"$1");

					// Check to see if the "switch" was an "end-of-switch-processing" flag
					if ( currentSwitchNormalized == "" ) {
						// Set flags
						didProcessArg                  = true  ;
						nextCanBeSwitch                = false ;
						nextCanBeSwitchParam           = false ;
						nextMustBeSwitchParam          = false ;
						nextCanBeSwitchless            = true  ;
						nextCanBeNothing               = true  ;
						nextMustBeSwitchlessOrNothing  = false ;
					} else {

						currentCanonicalSwitchName=argValidSwitches.GetKeyFromRegexMatch( currentSwitchNormalized );
						currentCanonicalSwitchSpec=argValidSwitches.GetValue( currentCanonicalSwitchName );

						// Validate
						if ( currentCanonicalSwitchName == "" || currentCanonicalSwitchSpec is null ) throw new System.ArgumentException($"Unknown command-line switch passed; '{arg}'.");
						if ( SwitchesAndParams.ContainsKey(currentCanonicalSwitchName) )              throw new System.ArgumentException($"Can't specify canonical switch (or alias) more than once: '{currentCanonicalSwitchName}'.");

						// Get shortcut variables for quicker/easier reference
						currentMinParamCount=currentCanonicalSwitchSpec.paramCountMin;
						currentMAXParamCount=currentCanonicalSwitchSpec.paramCountMax;

						// Add flag now in case it's a unitary flag (and it's no problem later if it already exists to add parameters to)
						SwitchesAndParams.Add(currentCanonicalSwitchName);

						// Set flags
						didProcessArg                  = true;
						nextCanBeSwitch                = (currentMinParamCount == 0);
						nextCanBeSwitchParam           = (currentMAXParamCount > 0);
						nextMustBeSwitchParam          = (currentMinParamCount > 0);
						nextCanBeSwitchless            = (currentMinParamCount == 0);
						nextCanBeNothing               = (currentMinParamCount == 0);
						nextMustBeSwitchlessOrNothing  = false ;

					}
				} else if (nextCanBeSwitchParam && currentCanonicalSwitchName != "") {
					// It's a parameter to an existing switch.

					// Validate
					if ( ! nextCanBeSwitchParam ) throw new System.ArgumentException($"Cannot accept parameter '{arg}' to switch '{currentSwitchAsPassed}' at this point.");

					// Get variables
					long currentParamListCount=SwitchesAndParams.ListCount(currentCanonicalSwitchName);

					// Validate
					if ( currentMAXParamCount > 0 && currentParamListCount >= currentMAXParamCount ) throw new ArgumentException($"Too many parameters specified for flag '{currentSwitchAsPassed}' (specifically, '{arg}').");

					// Add param arg to the list belonging to dictionary item currentCanonicalSwitchName.
					SwitchesAndParams.Add(currentCanonicalSwitchName, arg);
					currentParamListCount++;

					// Update flags
					didProcessArg                  = true;
					nextCanBeSwitch                = (currentParamListCount >= currentMinParamCount);
					nextCanBeSwitchParam           = (currentParamListCount < currentMAXParamCount);
					nextMustBeSwitchParam          = (currentParamListCount < currentMinParamCount);
					nextCanBeSwitchless            = (currentParamListCount >= currentMinParamCount);
					nextCanBeNothing               = (currentParamListCount >= currentMinParamCount);
					nextMustBeSwitchlessOrNothing  = false;

				} else {
					// It's a switchless argument (e.g. list of folders or files at the end).

					// Validate
					if ( ! nextCanBeSwitchless ) throw new System.ArgumentException($"Cannot accept switchless parameter '{arg}' at this point; it/they should go at the end of the command line.");

					StandaloneArgs.Add(arg);

					#if DEBUG
						Echo_v1($"DEBUG: {this.GetType()}.parseArgs(): standaloneArgs.Add(): '{arg}'");
					#endif

					// Flags
					didProcessArg                  = true  ;
					nextCanBeSwitch                = false ;  // Standalone stuff has to be at the end
					nextCanBeSwitchParam           = false ;
					nextMustBeSwitchParam          = false ;
					nextCanBeSwitchless            = true  ;
					nextCanBeNothing               = true  ;
					nextMustBeSwitchlessOrNothing  = true  ;

				}

				// Prepare for next loop around
				lastArg=arg;
				if ( ! nextCanBeSwitchParam ) {
					// Clear curent switch stuff
					currentSwitchAsPassed="";
					currentCanonicalSwitchSpec=null;
					currentCanonicalSwitchName="";
					currentMinParamCount=0;
					currentMAXParamCount=0;
				}

			}

			// Final validation
			if ( ! nextCanBeNothing ) throw new System.ArgumentException($"Expected something more after last arg '{lastArg}'.");
			if ( ! didProcessArg    ) throw new System.ArgumentException($"Unexpected argument '{lastArg}'.");

		}
	}


	// Innner class
	public class ValidSwitches_v1 {
		private Dictionary<string, ValidSwitch_v1> dictionary;
		public ValidSwitches_v1(){
			dictionary = new Dictionary<string, ValidSwitch_v1>();
		}
		public void Add(in string argCanonicalSwitchName, in string argAliases="", long argParamCountMin=0, long argParamCountMax=0 ) {
			// Build regex
			string regex=argCanonicalSwitchName;
			if ( argAliases != "" ) regex = $@"{regex}|{argAliases}";
			regex = $@"^(?:{regex})$";
			// Create new ValidSwitchFormat object
			var vsf = new ValidSwitch_v1(argCanonicalSwitchName, regex, argParamCountMin, argParamCountMax);
			// Add to dictionary
			dictionary.Add(argCanonicalSwitchName, vsf);
		}

		public bool TryGetValue(in string argKey, out ValidSwitch_v1 validSwitchObj) {
			bool retVal=false;
			if ( dictionary.TryGetValue(argKey, out ValidSwitch_v1 retObj) ) {
				validSwitchObj = retObj;
				retVal=true;
			} else {
				validSwitchObj = null;
			}
			return retVal;
		}
		public ValidSwitch_v1 GetValue(in string argKey) {
			if ( dictionary.TryGetValue(argKey, out ValidSwitch_v1 retObj) ) {
				return retObj;
			} else {
				return null;
			}
		}

		public string GetKeyFromRegexMatch(in string argSwitchName){
			string retVal="";
			foreach (KeyValuePair<string, ValidSwitch_v1> item in dictionary) {
				if ( Regex.IsMatch(argSwitchName, item.Value.regex) ) {
					retVal=item.Key;
					break;
				}
			}
			return retVal;
		}

		public bool IsSwitchValid(in string argSwitchName){
			bool retVal=false;
			foreach (KeyValuePair<string, ValidSwitch_v1> item in dictionary) {
				if ( Regex.IsMatch(argSwitchName, item.Value.regex) ) {
					retVal=true;
					break;
				}
			}
			return retVal;
		}
	}
	public class ValidSwitch_v1 {
		public readonly string canonicalName;
		public readonly string regex;
		public readonly long paramCountMin;
		public readonly long paramCountMax;
		public ValidSwitch_v1(in string argCanonicalName, in string argRegex, long argParamCountMin, long argParamCountMax ) {
			if ( argCanonicalName == "")              throw new System.ArgumentException($"Canonical switch name cannot be empty.");
			if ( argParamCountMin < 0  )              throw new System.ArgumentException($"Min parameter count must be >= 0.");
			if ( argParamCountMax < 0  )              throw new System.ArgumentException($"Max parameter count must be >= 0.");
			if ( argParamCountMin > argParamCountMax) throw new System.ArgumentException($"Min parameter count must be less than max.");
			canonicalName = argCanonicalName;
			regex         = argRegex;
			paramCountMin = argParamCountMin;
			paramCountMax = argParamCountMax;
		}
	}

}
