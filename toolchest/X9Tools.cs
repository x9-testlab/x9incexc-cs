//	Purpose: Common tools
//	History:
//		- 20200904 JC: Created

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static X9Tools.OS;
using static X9Tools.Str;
using static X9Tools.Misc;
using static X9Tools.Terminal_v1;


namespace X9Tools {

	/// <summary>OS-specific stuff.</summary>
	public static class OS {

		public static readonly string pathSlash=System.IO.Path.DirectorySeparatorChar.ToString();  // Short-hand for this long-ass thing.

	}


	/// <summary>String utilities</summary>
	public static class Str {

		/// <summary>Purpose: Removes redundant white space in between (replacing with " "), and at the ends of a string.</summary>
		public static string StrNormalize_v1(in string argStr="") {
			return String.Join(" ", argStr.Split((char[])null, StringSplitOptions.RemoveEmptyEntries) );
		}

		public static string EscapeQuotes(in string argStr){
			return argStr.Replace(@"'", @"\'").Replace("\"", "\\\"");
		}

	}

	/// <summary>Misc utilities</summary>
	public static class Misc {

		public const int invalidIndex = -1;

		public enum FileTimestampTypes{
			CTime_MetadataChanged,
			MTime_ContentChanged,
			CRTime_Created
		}

		// Try to do something and ignore errors
		public static bool TryAction(Action operation) {
			if (operation == null) return false;
			try { operation.Invoke(); }
			catch { return false; }
			return true;
		}

		public static object ConvertTo_v1 (object obj, Type t)
		{
			Type nut = Nullable.GetUnderlyingType (t);
			if (nut != null) {
				if (obj == null) { return null; }
				else { return Convert.ChangeType (obj, nut); }
			} else { return Convert.ChangeType (obj, t); }
		}


		// Time-related
		public static long     ToDotnetTicksUtc        (DateTime argDt)      { return argDt.ToUniversalTime().Ticks; }
		public static long     ToDotnetTicksLocal      (DateTime argDt)      { return argDt.ToLocalTime().Ticks; }
		public static long     ToUnixTicksUtc          (DateTime argDt)      { return (long)Math.Round( (double)((ToDotnetTicksUtc   (argDt) - UnixEpochToDotnetTicks()) / DotnetTicksPerUnixTicks() ), 0 ); }
		public static long     ToUnixTicksLocal        (DateTime argDt)      { return (long)Math.Round( (double)((ToDotnetTicksLocal (argDt) - UnixEpochToDotnetTicks()) / DotnetTicksPerUnixTicks() ), 0 ); }
		public static string   ToSerialUtc             (DateTime argDt)      { return argDt.ToUniversalTime ().ToString ( "yyyyMMdd-HHmmss-ffff"); }
		public static string   ToSerialLocal           (DateTime argDt)      { return argDt.ToLocalTime     ().ToString ( "yyyyMMdd-HHmmss-ffff"); }
		public static string   ToIso8601Utc            (DateTime argDt)      { return argDt.ToUniversalTime ().ToString (@"yyyy-MM-ddTHH:mm:ssZ"); }
		public static string   ToIso8601Local          (DateTime argDt)      { return argDt.ToLocalTime     ().ToString (@"yyyy-MM-ddTHH:mm:sszzz"); }
		public static DateTime FromDotnetTicksUtc      (long argDotnetTicks) { var tmpDotnet = new DateTime(argDotnetTicks); return DateTime.SpecifyKind( tmpDotnet, DateTimeKind.Utc   ); }
		public static DateTime FromDotnetTicksLocal    (long argDotnetTicks) { var tmpDotnet = new DateTime(argDotnetTicks); return DateTime.SpecifyKind( tmpDotnet, DateTimeKind.Local ); }
		public static DateTime FromUnixTicksUtc        (long argUnixTicks)   { var tmpDotnet = new DateTime( (UnixEpochToDotnetTicks() + ( argUnixTicks * DotnetTicksPerUnixTicks() )) ); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Utc   ); }
		public static DateTime FromUnixTicksLocal      (long argUnixTicks)   { var tmpDotnet = new DateTime( (UnixEpochToDotnetTicks() + ( argUnixTicks * DotnetTicksPerUnixTicks() )) ); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Local ); }
		public static DateTime UnixEpochToDotnetUtc    ()                    { var tmpDotnet = new DateTime(1970, 1, 1, 0, 0, 0); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Utc); }
		public static long     UnixEpochToDotnetTicks  ()                    { return UnixEpochToDotnetUtc().Ticks; }
		public static long     DotnetTicksPerUnixTicks ()                    { return TimeSpan.TicksPerSecond; }

		// IsNothing_v1; with overloads
		public static bool IsNothing_v1(in dynamic  argVal, in dynamic  alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(in DateTime argVal, in DateTime alsoTreatAsNothing=default(DateTime) ) { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(in string   argVal, in string   alsoTreatAsNothing="" )                { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(bool?       argVal, bool?       alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(int?        argVal, int?        alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(long?       argVal, long?       alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(float?      argVal, float?      alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(double?     argVal, double?     alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(decimal?    argVal, decimal?    alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }

		// ValOrDefault_v1; with overloads
		public static dynamic   ValOrDefault_v1(in dynamic   argVal, in dynamic  argDefault, in dynamic  alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return            argVal; }
		public static DateTime  ValOrDefault_v1(in DateTime? argVal, in DateTime argDefault, in DateTime alsoTreatAsNull=default(DateTime) ) { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (DateTime) argVal; }
		public static string    ValOrDefault_v1(in string    argVal, in string   argDefault, in string   alsoTreatAsNull="" )                { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (string)   argVal; }
		public static bool      ValOrDefault_v1(bool?        argVal, bool        argDefault                                 )                { if ( argVal == null )                              return argDefault; else return (bool)     argVal; }
		public static int       ValOrDefault_v1(int?         argVal, int         argDefault, int?        alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (int)      argVal; }
		public static long      ValOrDefault_v1(long?        argVal, in long     argDefault, in long?    alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (long)     argVal; }
		public static float     ValOrDefault_v1(float?       argVal, in float    argDefault, in float?   alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (float)    argVal; }
		public static double    ValOrDefault_v1(double?      argVal, in double   argDefault, in double?  alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (double)   argVal; }
		public static decimal   ValOrDefault_v1(decimal?     argVal, in decimal  argDefault, in decimal? alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (decimal)  argVal; }


		// Try function, ignore errors, and return only defined type (these are breathtaking not useful)
		public static object   TryFunctionOrDefault(Func<object>   doFunction, object   argDefault, object      alsoTreatAsNull=null )             { object   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static DateTime TryFunctionOrDefault(Func<DateTime> doFunction, DateTime argDefault, DateTime    alsoTreatAsNull=default(DateTime)) { DateTime retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static string   TryFunctionOrDefault(Func<string>   doFunction, string   argDefault, string      alsoTreatAsNull=""   )             { string   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static bool     TryFunctionOrDefault(Func<bool>     doFunction, bool     argDefault, bool?       alsoTreatAsNull=null )             { bool     retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static int      TryFunctionOrDefault(Func<int>      doFunction, int      argDefault, int?        alsoTreatAsNull=null )             { int      retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static long     TryFunctionOrDefault(Func<long>     doFunction, long     argDefault, long?       alsoTreatAsNull=null )             { long     retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static float    TryFunctionOrDefault(Func<float>    doFunction, float    argDefault, float?      alsoTreatAsNull=null )             { float    retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static double   TryFunctionOrDefault(Func<double>   doFunction, double   argDefault, double?     alsoTreatAsNull=null )             { double   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static decimal  TryFunctionOrDefault(Func<decimal>  doFunction, decimal  argDefault, decimal?    alsoTreatAsNull=null )             { decimal  retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }

		// Dummy to stand in for things sometimes (which is usually a code sound)
		public static void noop(){ ; }

	}


	/// <summary>Console wrapper utilities.</summary>
	public static class Terminal_v1 {

		// Echo-related stuff.
		// History:
		// 	- 20200904 JC: Created.
		private static bool wasEmptyLasttime=false;  // TODO: Make thread-safe
		public static void Echo_Reset_v1() { wasEmptyLasttime=false; }
		public static void Echo_v1(in string arg="") {
			if ( arg.Length > 0 ) Echo_Clean_v1($"[ {arg} ]");
			else                  Echo_Clean_v1();
		}
		public static void Echo_Clean_v1(in string arg="") {
			bool isEmpty = ( arg.Length == 0 );
			if ( !isEmpty || !wasEmptyLasttime ) System.Console.WriteLine(arg);
			wasEmptyLasttime=isEmpty;
		}
		public static void EchoIfDebug_v1(in string arg="") {
			#if DEBUG
				if ( arg.Length > 0 ) Echo_v1($"DEBUG: {arg}");
				else                  Echo_v1();
			#endif
			noop();
		}
	}


	//	History:
	//		- 20200913 JC: Created.
	//	TODO: Delete once FsObj_v2 stuff gets deleted.

	public class BetterDateTime_v1 {

		// Private members
		private DateTime dotNetUtc = DateTime.UtcNow;  // Default to now, rather than Jan 1 0001

		// Unix epoch-related (when Unix time started at 0)
		public DateTime UnixEpochToDotnet       { get { return new DateTime(1970, 1, 1, 0, 0, 0); } }
		public long     UnixEpochToDotnetTicks  { get { return UnixEpochToDotnet.Ticks; } }
		public long     UnixEpochUnixTicks      { get { return 0; } }
		public long     DotnetTicksPerUnixTicks { get { return TimeSpan.TicksPerSecond; } }

		public DateTime DotnetUtc {
			get { return dotNetUtc; }
			set { dotNetUtc = value.ToUniversalTime(); }
		}
		public DateTime DotnetLocal {
			get { return dotNetUtc.ToLocalTime() ; }
			set { dotNetUtc = value.ToUniversalTime(); }
		}
		public long DotnetTicksUtc {
			get { return dotNetUtc.Ticks; }
			set { dotNetUtc = new DateTime(value); }
		}
		public long DotnetTicksLocal {
			get { return DotnetLocal.Ticks; }
			set { DotnetLocal = new DateTime(value); }
		}
		public long UnixTicksUtc {
			get { return (long)Math.Round( (double)((DotnetTicksUtc-UnixEpochToDotnetTicks)/DotnetTicksPerUnixTicks), 0 ); }
			set { dotNetUtc = new DateTime( (UnixEpochToDotnetTicks+(value*DotnetTicksPerUnixTicks)) ); }
		}
		public long UnixTicksLocal {
			get { return (long)Math.Round( (double)((DotnetTicksLocal-UnixEpochToDotnetTicks)/DotnetTicksPerUnixTicks), 0 ); }
			set { DotnetLocal = new DateTime( (UnixEpochToDotnetTicks+(value*DotnetTicksPerUnixTicks)) ); }
		}
		public string SerialUtc {
			get { return DotnetUtc.ToString("yyyyMMdd-HHmmss-ffff"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string SerialLocal {
			get { return DotnetLocal.ToString("yyyyMMdd-HHmmss-ffff"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string Iso8601Utc {
			get { return DotnetUtc.ToString(@"yyyy-MM-ddTHH\:mm\:ssZ"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string Iso8601Local {
			get { return DotnetLocal.ToString(@"yyyy-MM-ddTHH\:mm\:sszzz"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}

	}


	//	History:
	//		- 20200907 JC: Created.
	public class DictionaryOfLists : System.Collections.IEnumerable {

		private readonly Dictionary<dynamic, List<dynamic>> _dictionary = new Dictionary<dynamic, List<dynamic>>();

		public long Count { get { return _dictionary?.Count ?? 0; } }

		public bool ContainsKey(dynamic argKey) {
			return _dictionary.ContainsKey(argKey);
		}

		// Add values to a list. List can already exist or not. New values are appended to list.
		public void Add(in dynamic argKey, params dynamic[] argVals) {

			// Either get matching list or create a new one and add it to dictionary.
			if ( ! _dictionary.TryGetValue(argKey, out List<dynamic> list) ) {
				// Key doesn't already exist; Create a new list and add it with the given key
				list = new List<dynamic>();
				_dictionary.Add(argKey, list);
				EchoIfDebug_v1($"{this.GetType()}.Add(): argKey = '{argKey}'");
			}

			// Add the values to the list
			foreach ( var val in argVals ) {
				EchoIfDebug_v1($"{this.GetType()}.Add(): argKey = '{argKey}', val = '{val}'.");
				list.Add(val);
			}

		}

		public List<dynamic> GetValue(in dynamic argKey) {
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> retList) ) {
				return retList;
			} else {
				return null;
			}
		}

		public bool TryGetValue( in dynamic argKey, out List<dynamic> outList ) {
			bool retVal=false;
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> retList) ) {
				outList = retList;
				retVal=true;
			} else {
				outList = null;
			}
			return retVal;
		}

		public long ListCount(in dynamic argKey) {
			long retVal=0;
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> outList) ) {
				retVal = (long)outList.Count;
			}
			return retVal;
		}

		// Required by IEnmerator interface.
		public IEnumerator GetEnumerator() {
			foreach (KeyValuePair<dynamic, List<dynamic>> item in _dictionary) {
				yield return item;
			}
		}
	}


	//	Purpose
	//		- Better management of filesystem objects (e.g. files and folders)
	//		- Can begin object tree at any point, you don't have to start at the top.
	//	History:
	//		- 20200912-13 JC: Created.

	// Collection of FsObjs_v2 with different FilesystemsIdx and/or BaseDirs
	public class TubOfTubOfFsObjs_v2 {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<TubOfFsObjs_v2> list = new List<TubOfFsObjs_v2>();
		public int            Count                                                  { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()    /* TODO: Use <T> version*/         { foreach (TubOfFsObjs_v2 oItem in list) yield return oItem; }
		public TubOfFsObjs_v2 Get           (int argIndex)                           { return list[argIndex]; }
		public void           Add           (TubOfFsObjs_v2 argObj)                  { list.Add(argObj); }
		public TubOfFsObjs_v2 Add           (string argBaseDir, int argFssIdx = -1)  { var oObj = new TubOfFsObjs_v2( argBaseDir, argFssIdx ); Add(oObj); return oObj; }
		public void           Remove        (TubOfFsObjs_v2 oObj)                    { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<TubOfFsObjs_v2> argPred)      { list.RemoveAll(argPred); }
	}

	// Collection of FsObj_v2 with a common FilesystemsIdx and BaseDir; can be used as a top-level object.
	public class TubOfFsObjs_v2 {

		// Public implementation
		public readonly string BaseDir;
		public readonly int    FilesystemsIdx;

		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<FsObj_v2> list = new List<FsObj_v2>();
		public int          Count                                              { get { return list.Count; } }
		public IEnumerator  GetEnumerator ()  /* TODO: Use <T> version*/       { foreach (FsObj_v2 oItem in list) yield return oItem; }
		public FsObj_v2     Get           (int argIndex)                       { return list[argIndex]; }
		public void         Remove        (FsObj_v2 oObj)                      { list.Remove(oObj); }
		public void         RemoveAll     (Predicate<FsObj_v2> argPred )       { list.RemoveAll(argPred); }

		// Boilerplate specifically removed; we don't want this functionality
	//	public void         Add           (FsObj_v2 argObj)                    { list.Add( argObj ); }
	//	public FsObj_v2     Add           (string argPath, int argFssIdx = -1) { var oObj = new FsObj_v2( argPath, argFssIdx ); Add(oObj); return oObj; }

		// Constructor (this is where physical filesystem scanning occurs)
		public TubOfFsObjs_v2(in string argBaseDir, int argFilesystemsIdx = -1){

			// Notes:
			//	- System.IO.Directory.GetFiles() apparently dives into symlinks and that can't be changed; this is more granular.
			//  - How to check if a file is a junction (check reparsepoint attribute, which may be a symlink or ntfs junction):
			// 		https://stackoverflow.com/a/26473940
			// 		https://stackoverflow.com/a/9882819
			//	- Alternate search methods:
			//		https://stackoverflow.com/a/12464711

			// Set enumerate options
			string searchSpec                    = "*";
			var    enumOptions                   = new System.IO.EnumerationOptions();
		//	enumOptions.AttributesToSkip         = System.IO.FileAttributes.ReparsePoint;  // Skips symlinks
			enumOptions.RecurseSubdirectories    = true;
			enumOptions.ReturnSpecialDirectories = false;                                  // 'true' would include '.' and '..'.
			enumOptions.IgnoreInaccessible       = false;                                  // We want an exception for inaccessible dirs.
			enumOptions.MatchCasing              = 0;                                      // 0 = platform default; 1 = case-sensitive; 2 = insensitive
			enumOptions.MatchType                = System.IO.MatchType.Simple;             // 0 = * and ? wildcards; 1 = weird DOS style, ignore.

			IEnumerable<string> filesEnum = System.IO.Directory.EnumerateFiles(argBaseDir, searchSpec, enumOptions);
			foreach (string itemPath in filesEnum) {
				list.Add(new FsObj_v2(itemPath, argFilesystemsIdx));
			}
		}

	}


	// Bottom-layer class in all this (can also be used as lone object[s])
	public class FsObj_v2 {

		// Private properties
		private bool              wasInspected_csFileAttributes = false;

		// Public properties
		public string             Path                          { get; private set; }
		public int                FilesystemsIdx                { get; private set; }
		public long               Inode                         { get; private set; }  // inode info
		public long               RefCount                      { get; private set; }  // How many hardlinks
		public bool               IsFile                        { get; private set; }
		public bool               IsDir                         { get; private set; }
		public bool               IsLink                        { get; private set; }  // Symbolic link, Reparse point; can be both this, and File or Folder.
		public bool               IsNormal                      { get; private set; }
		public bool               IsLinkGood                    { get; private set; }  // Symlinks can be broken.
		public bool               IsVisible                     { get; private set; }  // Hidden attribute in Windows, "." file in *nx.
		public bool               IsMountpoint                  { get; private set; }
		public bool               IsSocket                      { get; private set; }
		public bool               CanList                       { get; private set; }
		public bool               CanRead                       { get; private set; }
		public bool               CanExecute                    { get; private set; }
		public bool               CanWrite                      { get; private set; }
		public bool               CanDelete                     { get; private set; }
		public bool               CanOwn                        { get; private set; }
		public bool               DoesOwn                       { get; private set; }
		public double             SizeMiB                       { get; private set; }
		public string             OwnerName                     { get; private set; }
		public long               OwnerId                       { get; private set; }
		public string             XattrsStr                     { get; private set; }
		public BetterDateTime_v1  MTime_ContentChanged          { get; private set; }
	//	public BetterDateTime_v1  CRTime_Created                { get; private set; }  // Doesn't seem to work on Linux
	//	public BetterDateTime_v1  CTime_MetadataChanged         { get; private set; }  // ctime not directly supported in dotnet core yet.
		public ulong              Content_crc64_binary          { get; private set; }
		public byte[]             Content_blake2b_binary        { get; private set; }

		// Constructor
		public FsObj_v2(in string argPath, int argFilesystemsIdx = -1){

			// The main stuff
			Path           = argPath;
			FilesystemsIdx = argFilesystemsIdx;

			// Ideally these next lines wouldn't run until a property is actually inspected.
			// Unfortunately such lazy get/set complicates properties (can't use syntactic sugar),
			// And causes a stack overflow when self-setting while reading.
			// Once this interface settles, fix that. (By using backing private properties.)
			inspectPropsIfNotYet();

		}

		public Dictionary<string, string> GetXattrsDict() {
			var retVal = new Dictionary<string, string>();

			// Get and parse Xattrs
			if (true) throw new ApplicationException("Method 'GetXattrsDict' not yet implimented.");

			//return retVal;
		}

		// Creates or changes an xattr by keyname.
		public void SetXattr(in string key, in string val){
			throw new ApplicationException("Method 'SetXattr' is not implemented yet.");
		}

		// Inspect file properties (lazy)
		private void inspectPropsIfNotYet(bool force = false ){
			if ( !wasInspected_csFileAttributes || force ){
				wasInspected_csFileAttributes = true;  // Set first so that it also acts as a 'isSetting' flag.

				try {

					// Get file attributes
					var dotnetAttribs  = System.IO.File.GetAttributes(Path);
					var dotnetFileInfo = new System.IO.FileInfo(Path);

					// Set basic props that IO.File.GetAttributes() and IO.FileInfo() can discern.
					CanList                        = true;                                                 // Self-evident
					IsDir                          = dotnetAttribs.HasFlag(FileAttributes.Directory);
					IsFile                         = TryFunctionOrDefault(() => dotnetAttribs.HasFlag(FileAttributes.Directory), false);
					IsLink                         = dotnetAttribs.HasFlag(FileAttributes.ReparsePoint);  // Doesn't tell us if Junction or Symbolic Link.
					IsNormal                       = (IsFile | IsDir ) & !IsLink;  // & dotnetAttribs.HasFlag(FileAttributes.Normal);  // FileAttributes.Normal doesn't seem to do what you'd think.
					IsVisible                      = !dotnetAttribs.HasFlag(FileAttributes.Hidden);
					SizeMiB                        = (double)dotnetFileInfo.Length / Math.Pow(2, 20);  // MiB = 2^20, KiB = 2^10
					MTime_ContentChanged.DotnetUtc = dotnetFileInfo.LastWriteTimeUtc;

				} catch {
					CanList                        = false;                                                 // Self-evident
				}

			}
		}


	}

	// Collection of FileSystem_v2; Is not part of TubOfFsObjs_v2 hiearchy, but can work alongside it if using multiple filesystems.
	public class FileSystems_v2 {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<FileSystem_v2> list = new List<FileSystem_v2>();
		public int            Count                                            { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()  /* TODO: Use <T> version*/     { foreach (FileSystem_v2 oItem in list) yield return oItem; }
		public FileSystem_v2  Get           (int argIndex)                     { return list[argIndex]; }
		public void           Add           (FileSystem_v2 argObj)             { list.Add(argObj); }
		public FileSystem_v2  Add           ()                                 { var oObj = new FileSystem_v2(); Add(oObj); return oObj; }
		public void           Remove        (FileSystem_v2 oObj)               { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<FileSystem_v2> argPred) { list.RemoveAll(argPred); }
	}

	// Describes a filesystem. Not well-fleshed-out yet.
	public class FileSystem_v2{
		public string Type                            { get; private set; }
		public string FilesysId                       { get; private set; }
		public string PartitionId                     { get; private set; }
		public string DiskId                          { get; private set; }
		public string Path                            { get; private set; }
	}

}
