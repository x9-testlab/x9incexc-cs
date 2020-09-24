using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static X9Tools.Misc;
using static X9Tools.Terminal_v1;

namespace X9Tools.Filesys {

	//	Purpose
	//		- Better management of filesystem objects (e.g. files and folders)
	//		- Can begin object tree at any point, you don't have to start at the top.
	//	History:
	//		- 20200912-13 JC: Created.

	// Wraps System.IO.Directory.GetFiles(), which isn't great; it ignores symlink dirs and inaccessible files.
	public static class Utils {

		public static bool TryGetFiles(in string argBasedir, in string argSearchSpec, SearchOption argSearchOption, out string[] outResults) {
			bool retVal;
			try {
				outResults = System.IO.Directory.GetFiles(argBasedir, argSearchSpec, argSearchOption);
				retVal = true;
			} catch { outResults = null; retVal = false; }
			return retVal;
		}

		// Same as above but overloaded
		public static bool TryGetFiles(in string argBasedir, in string argSearchSpec, System.IO.EnumerationOptions argEnumOptions, out string[] outResults) {
			bool retVal;
			try {
				outResults = System.IO.Directory.GetFiles(argBasedir, argSearchSpec, argEnumOptions);
				retVal = true;
			} catch { outResults = null; retVal = false; }
			return retVal;
		}

		// Same as above but overloaded
		public static bool TryGetDirs(in string argBasedir, in string argSearchSpec, System.IO.EnumerationOptions argEnumOptions, out string[] outResults) {
			bool retVal;
			try {
				outResults = System.IO.Directory.GetDirectories(argBasedir, argSearchSpec, argEnumOptions);
				retVal = true;
			} catch { outResults = null; retVal = false; }
			return retVal;
		}

		// Wraps System.IO.Directory.EnumerateFiles(), which is flawed; together with custom recursion, it can be better (but much slower).
		public static bool TryEnumerateFiles(in string argBaseDir, in string argSearchSpec, System.IO.EnumerationOptions argEnumOptions, out IEnumerable<string> outResults) {
			bool retVal;
			try {
				outResults = System.IO.Directory.EnumerateFiles(argBaseDir, argSearchSpec, argEnumOptions);
				retVal = true;
			} catch { outResults = null; retVal = false; }
			return retVal;
		}

	}

	// Collection of FsObjs_v3 with different FilesystemsIdx and/or BaseDirs
	public class TubOfTubOfFsObjs_v3 {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<TubOfFsObjs_v3> list = new List<TubOfFsObjs_v3>();
		public int            Count                                                            { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()    /* TODO: Use <T> version*/                   { foreach (TubOfFsObjs_v3 oItem in list) yield return oItem; }
		public TubOfFsObjs_v3 Get           (int argIndex)                                     { return list[argIndex]; }
		public void           Add           (TubOfFsObjs_v3 argObj)                            { list.Add(argObj); }
		public TubOfFsObjs_v3 Add           (string argBaseDir, int argFssIdx = -1)            { var oObj = new TubOfFsObjs_v3( argBaseDir, argFssIdx ); Add(oObj); return oObj; }
		public void           Remove        (TubOfFsObjs_v3 oObj)                              { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<TubOfFsObjs_v3> argPred)                { list.RemoveAll(argPred); }
	}

	// Collection of FsObj_v3 with a common FilesystemsIdx and BaseDir; can be used as a top-level object.
	public class TubOfFsObjs_v3 {

		// Public implementation
		public readonly string BaseDir;
		public readonly int    FilesystemsIdx;

		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<FsObj_v3> fsObjList = new List<FsObj_v3>();
		public int          Count                                              { get { return fsObjList.Count; } }
		public IEnumerator  GetEnumerator ()  /* TODO: Use <T> version*/       { foreach (FsObj_v3 oItem in fsObjList) yield return oItem; }
		public FsObj_v3     Get           (int argIndex)                       { return fsObjList[argIndex]; }
		public void         Remove        (FsObj_v3 oObj)                      { fsObjList.Remove(oObj); }
		public void         RemoveAll     (Predicate<FsObj_v3> argPred )       { fsObjList.RemoveAll(argPred); }

		// Constructor (this is where physical filesystem scanning occurs)
		public TubOfFsObjs_v3(in string argBaseDir, int argFilesystemsIdx = -1){

			// Init public properties
			BaseDir        = argBaseDir;
			FilesystemsIdx = argFilesystemsIdx;

			// Add base dir
			ScanFilesystem_ConsumeFsObj(new FsObj_v3("", BaseDir, invalidIndex, FilesystemsIdx));

			// Common pre-scan stuff
			ScanFS_v3_enumOptions.RecurseSubdirectories    = false;
			ScanFS_v3_enumOptions.ReturnSpecialDirectories = false;                                  // 'true' would include '.' and '..'.
			ScanFS_v3_enumOptions.IgnoreInaccessible       = false;                                  // We want an exception for inaccessible dirs.
			ScanFS_v3_enumOptions.MatchCasing              = 0;                                      // 0 = platform default; 1 = case-sensitive; 2 = insensitive
			ScanFS_v3_enumOptions.MatchType                = System.IO.MatchType.Simple;             // 0 = * and ? wildcards; 1 = weird DOS style, ignore.
			ScanFS_v3_enumOptions.AttributesToSkip         = 0;

			// Scan filesystem for this basedir. Implemented separately so that scanning algorithms can be easily swapped out.
			ScanFilesystem_v3d(BaseDir);   // Custom recursive depth-first search using System.IO.Directory.GetFiles and welcoming but ignoring errors.
		//	ScanFilesystem_v3c(BaseDir);   // Custom recursive depth-first search using System.IO.Directory.GetFiles and welcoming but ignoring errors.
		//	ScanFilesystem_v3b(BaseDir);   // Custom recursive depth-first search using System.IO.Directory.EnumerateFiles and welcoming but ignoring errors.
		//	ScanFilesystem_v3a();          // System.IO.Directory.EnumerateFiles

		}

		// Implemented separately so that scanning algorithms can be easily swapped out.
		private int currentListIndex               = -1;
		private long ScanFilesystem_RecursionLevel =  0;
		private void ScanFilesystem_ConsumeFsObj(FsObj_v3 argObj){
		//	EchoIfDebug_v1($"ScanFilesystem_ConsumeFsObj(): Adding ... '{argObj.Path}'.");
			fsObjList.Add(argObj);
			currentListIndex++;
		}

		// Implemented separately so that they can be easily swapped out.
		// This is recursive, dept-first, using TryGetFiles().
		private void ScanFilesystem_v3d(in string baseDir, long parentIdx = 0) {
			ScanFilesystem_RecursionLevel++;

			// Get non-dir stuff
			if (Utils.TryGetFiles(baseDir, "*", ScanFS_v3_enumOptions, out string[] files)) {
				// Docs seem to suggest this also returns dirs, but it doesn't, at least on Linux; only files or broken dir symlinks.
				foreach (string pathItem in files) {
					var fsObj = new FsObj_v3("", pathItem, parentIdx, FilesystemsIdx);
					ScanFilesystem_ConsumeFsObj(fsObj);
				}
			}

			// Get dirs
			if (Utils.TryGetDirs(baseDir, "*", ScanFS_v3_enumOptions, out string[] dirs)) {
				foreach (string pathItem in dirs) {
					var fsObj = new FsObj_v3("", pathItem, currentListIndex, FilesystemsIdx);
					ScanFilesystem_ConsumeFsObj(fsObj);
					if (fsObj.IsDir && fsObj.IsNormal){
					//	EchoIfDebug_v1($"ScanFilesystem_v3b(): Diving into ....... '{pathItem}'.");
						ScanFilesystem_v3d(pathItem);
					}
				}
			} else {
				fsObjList[currentListIndex].CanList = false;
			}

			ScanFilesystem_RecursionLevel--;
		}

		// Implemented separately so that they can be easily swapped out.
		// This is recursive, dept-first, using TryEnumerateFiles().
		// Set enumerate options
		private readonly System.IO.EnumerationOptions ScanFS_v3_enumOptions = new System.IO.EnumerationOptions();
		private void ScanFilesystem_v3b(in string baseDir) {
			ScanFilesystem_RecursionLevel++;
			if (Utils.TryEnumerateFiles(BaseDir, "*.*", ScanFS_v3_enumOptions, out IEnumerable<string> filesEnum)) {
				foreach (string pathItem in filesEnum) {
					var fsObj = new FsObj_v3("", pathItem, invalidIndex, FilesystemsIdx);
					ScanFilesystem_ConsumeFsObj(fsObj);
					if (fsObj.IsDir && fsObj.IsNormal){
					//	EchoIfDebug_v1($"ScanFilesystem_v3b(): Diving into ....... '{pathItem}'.");
						ScanFilesystem_v3b(pathItem);
					}
				}
			} else {
				fsObjList[currentListIndex].CanList = false;
			}
			ScanFilesystem_RecursionLevel--;
		}

		// Implemented separately so that they can be easily swapped out.
		// This is recursive, dept-first, using TryGetFiles().
		private void ScanFilesystem_v3c(in string baseDir) {
			ScanFilesystem_RecursionLevel++;
			if (Utils.TryGetFiles(baseDir, "*", ScanFS_v3_enumOptions, out string[] list)) {
				foreach (string pathItem in list) {
					var fsObj = new FsObj_v3("", pathItem, invalidIndex, FilesystemsIdx);
					ScanFilesystem_ConsumeFsObj(fsObj);
					if (fsObj.IsDir && fsObj.IsNormal){
					//	EchoIfDebug_v1($"ScanFilesystem_v3b(): Diving into ....... '{pathItem}'.");
						ScanFilesystem_v3c(fsObj.Path);
					}
				}
			} else {
				fsObjList[currentListIndex].CanList = false;
			}
			ScanFilesystem_RecursionLevel--;
		}

		// Implemented separately so that they can be easily swapped out; scans everything at once and is much faster than custom recursion, but can't "see" some important attributes.
		private void ScanFilesystem_v3a() {

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

			enumOptions.AttributesToSkip         = System.IO.FileAttributes.ReparsePoint;  // Skips symlinks
			enumOptions.RecurseSubdirectories    = true;
			enumOptions.ReturnSpecialDirectories = false;                                  // 'true' would include '.' and '..'.
			enumOptions.IgnoreInaccessible       = false;                                  // We want an exception for inaccessible dirs.
			enumOptions.MatchCasing              = 0;                                      // 0 = platform default; 1 = case-sensitive; 2 = insensitive
			enumOptions.MatchType                = System.IO.MatchType.Simple;             // 0 = * and ? wildcards; 1 = weird DOS style, ignore.

			IEnumerable<string> filesEnum = System.IO.Directory.EnumerateFiles(BaseDir, searchSpec, enumOptions);
			foreach (string itemPath in filesEnum) {
				ScanFilesystem_ConsumeFsObj(new FsObj_v3("", itemPath, invalidIndex, FilesystemsIdx) );
			}
		}

	}


	// Bottom-layer class in all this (can also be used as lone object[s])
	public class FsObj_v3 {

		// Private properties
		private string            _path;

		// Public properties ( private setters aren't perfect, but these can't be readonly since these technically aren't set in public constructor)
		public string    Name                    { get; private set; }
		public long      ParentIdx               { get; private set; }
		public int       FilesystemsIdx          { get; private set; }
		public bool      IsFile                  { get; private set; }
		public bool      IsDir                   { get; private set; }
		public bool      IsLink                  { get; private set; }  // Symbolic link, Reparse point; can be both this, and File or Folder.
		public bool      IsNormal                { get; private set; }  // E.g. not a symlink, mountpoint, socket, etc.
		public bool      IsVisible               { get; private set; }  // Hidden attribute in Windows, "." file in *nx.
		public bool      CanList                 { get;         set; }  // Can tell this by failing on directory listing.
	//	public bool      CanRead                 { get; private set; }  // Can't tell this in dotnet, for files, without trying to open it.
	//	public bool      CanExecute              { get; private set; }
	//	public bool      CanWrite                { get; private set; }
		public double    SizeMiB                 { get; private set; }
		public long      MTime_DotnetTicksUtc    { get; private set; }
	//	public long      PrevIdx                 { get; private set; }
	//	public long      NextIdx                 { get; private set; }
	//	public bool      CanList                 { get; private set; }

		// Properties that can't be obtained via dotnet core 3.1
	//	public long      CRTime_Created          { get; private set; }  // Doesn't seem to work on Linux
	//	public long      CTime_MetadataChanged   { get; private set; }  // ctime not directly supported in dotnet core yet.
	//	public long      Inode                   { get; private set; }  // inode info
	//	public long      RefCount                { get; private set; }  // How many hardlinks
	//	public bool      IsMountpoint            { get; private set; }
	//	public bool      IsSocket                { get; private set; }
	//	public bool      IsNamedPipe             { get; private set; }
	//	public bool      IsGood                  { get; private set; }  // Symlinks can be broken.
	//	public bool      CanDelete               { get; private set; }
	//	public bool      CanOwn                  { get; private set; }
	//	public bool      DoesOwn                 { get; private set; }
	//	public string    OwnerName               { get; private set; }
	//	public long      OwnerId                 { get; private set; }
	//	public string    XattrsStr               { get; private set; }
	//	public ulong     Content_crc64_binary    { get; private set; }
	//	public byte[]    Content_blake2b_binary  { get; private set; }

		public string Path { get {
		//	if (_path != "") {
		//		throw new ApplicationException("Property 'Path' not yet implimented.");
		//	}
			return _path;
		}}

		// Constructors (overloaded)
	//	public FsObj_v3       (in string argName,                              long argParentIdx,     int argFilesystemsIdx = invalidIndex){ _fsObj_v3(argName,   ""   , argParentIdx, argFilesystemsIdx); }
	//	public FsObj_v3       (                        in string argPath,                             int argFilesystemsIdx = invalidIndex){ _fsObj_v3(   ""  , argPath,       0     , argFilesystemsIdx); }
		public FsObj_v3       (in string argName = "", in string argPath = "", long argParentIdx = 0, int argFilesystemsIdx = -1){

			// The main stuff
			Name           = argName;
			_path          = argPath;
			ParentIdx      = argParentIdx;
			FilesystemsIdx = argFilesystemsIdx;

			// Ideally the next lines wouldn't run until a property is actually inspected.
			// Unfortunately such lazy get/set complicates properties (can't use syntactic sugar),
			// And causes a stack overflow when self-setting while reading.
			// Once this interface settles, fix that. (By using backing private properties.)

			try {

				// Get attribute inspection objects
				var dotnetAttribs = System.IO.File.GetAttributes(Path);
				var dotnetFileInfo = new System.IO.FileInfo(Path);

				// Inspect
				CanList               = true;                                                // Probably self-evident.
				IsDir                 = dotnetAttribs.HasFlag(FileAttributes.Directory);
				IsFile                = !IsDir;
				IsLink                = dotnetAttribs.HasFlag(FileAttributes.ReparsePoint);  // Doesn't tell us if Junction or Symbolic Link.
				IsNormal              = (IsFile | IsDir ) & !IsLink;                         // & dotnetAttribs.HasFlag(FileAttributes.Normal);  // FileAttributes.Normal doesn't seem to do what you'd think.
				IsVisible             = !dotnetAttribs.HasFlag(FileAttributes.Hidden);
			//	CanWrite              = !dotnetAttribs.HasFlag(FileAttributes.ReadOnly);     // For Linux is supposed to be based on permissions.
				MTime_DotnetTicksUtc  = ToDotnetTicksUtc(dotnetFileInfo.LastWriteTimeUtc);
				if (IsFile) SizeMiB   = (double)dotnetFileInfo.Length / Math.Pow(2, 20);     // MiB = 2^20, KiB = 2^10

			} catch {
				CanList               = false;                                                // Probably self-evident.
			}

		}

		public Dictionary<string, string> GetXattrsDict() {
			var retVal = new Dictionary<string, string>();
			if (true) throw new ApplicationException("Method 'GetXattrsDict' not yet implimented.");
			//return retVal;
		}

		// Creates or changes an xattr by keyname.
		public void SetXattr(in string key, in string val){
			throw new ApplicationException("Method 'SetXattr' is not implemented yet.");
		}

	}

	// Collection of FileSystem_v3; Is not part of TubOfFsObjs_v3 hiearchy, but can work alongside it if using multiple filesystems.
	public class FileSystems_v3 {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<FileSystem_v3> list = new List<FileSystem_v3>();
		public int            Count                                            { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()  /* TODO: Use <T> version*/     { foreach (FileSystem_v3 oItem in list) yield return oItem; }
		public FileSystem_v3  Get           (int argIndex)                     { return list[argIndex]; }
		public void           Add           (FileSystem_v3 argObj)             { list.Add(argObj); }
		public FileSystem_v3  Add           ()                                 { var oObj = new FileSystem_v3(); Add(oObj); return oObj; }
		public void           Remove        (FileSystem_v3 oObj)               { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<FileSystem_v3> argPred) { list.RemoveAll(argPred); }
	}

	// Describes a filesystem. Not well-fleshed-out yet.
	public class FileSystem_v3{
		public string Type                            { get; private set; }
		public string FilesysId                       { get; private set; }
		public string PartitionId                     { get; private set; }
		public string DiskId                          { get; private set; }
		public string Path                            { get; private set; }
	}

}