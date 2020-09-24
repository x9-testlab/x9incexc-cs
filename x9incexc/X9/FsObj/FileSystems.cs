//	Purpose: Part of filesystem object hierarchy.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

using System;
using System.Collections;
using System.Collections.Generic;

namespace X9.FsObj {

	// Collection of FileSystem_v3; Is not part of TubOfFsObjs_v3 hiearchy, but can work alongside it if using multiple filesystems.
	public class FileSystems {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<FileSystem> list = new List<FileSystem>();
		public int            Count                                            { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()  /* TODO: Use <T> version*/     { foreach (FileSystem oItem in list) yield return oItem; }
		public FileSystem  Get           (int argIndex)                     { return list[argIndex]; }
		public void           Add           (FileSystem argObj)             { list.Add(argObj); }
		public FileSystem  Add           ()                                 { var oObj = new FileSystem(); Add(oObj); return oObj; }
		public void           Remove        (FileSystem oObj)               { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<FileSystem> argPred) { list.RemoveAll(argPred); }
	}

}