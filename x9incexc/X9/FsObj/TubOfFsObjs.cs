//	Purpose: Part of filesystem object hierarchy.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

using System;
using System.Collections;
using System.Collections.Generic;
using static X9.Util;

namespace X9.FsObj {

	// Collection of FsObj_v3 with a common FilesystemsIdx and BaseDir; can be used as a top-level object.
	public class TubOfFsObjs {

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
		public TubOfFsObjs(in string argBaseDir, int argFilesystemsIdx = -1){

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
			if (Filesystem.TryGetFiles(baseDir, "*", ScanFS_v3_enumOptions, out string[] files)) {
				// Docs seem to suggest this also returns dirs, but it doesn't, at least on Linux; only files or broken dir symlinks.
				foreach (string pathItem in files) {
					var fsObj = new FsObj_v3("", pathItem, parentIdx, FilesystemsIdx);
					ScanFilesystem_ConsumeFsObj(fsObj);
				}
			}

			// Get dirs
			if (Filesystem.TryGetDirs(baseDir, "*", ScanFS_v3_enumOptions, out string[] dirs)) {
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
			if (Filesystem.TryEnumerateFiles(BaseDir, "*.*", ScanFS_v3_enumOptions, out IEnumerable<string> filesEnum)) {
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
			if (Filesystem.TryGetFiles(baseDir, "*", ScanFS_v3_enumOptions, out string[] list)) {
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

}