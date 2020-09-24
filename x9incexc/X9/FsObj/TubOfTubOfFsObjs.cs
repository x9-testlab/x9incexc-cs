//	Purpose: Part of filesystem object hierarchy.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

using System;
using System.Collections;
using System.Collections.Generic;

namespace X9.FsObj {

	// Collection of FsObjs_v3 with different FilesystemsIdx and/or BaseDirs
	public class TubOfTubOfFsObjs {
		// Boilerplate; TODO: Convert this and other boilerplate that does the same thing, into an inheritable, polymorphable class of type <T>.
		private readonly List<TubOfFsObjs> list = new List<TubOfFsObjs>();
		public int            Count                                                            { get { return list.Count; } }
		public IEnumerator    GetEnumerator ()    /* TODO: Use <T> version*/                   { foreach (TubOfFsObjs oItem in list) yield return oItem; }
		public TubOfFsObjs Get           (int argIndex)                                     { return list[argIndex]; }
		public void           Add           (TubOfFsObjs argObj)                            { list.Add(argObj); }
		public TubOfFsObjs Add           (string argBaseDir, int argFssIdx = -1)            { var oObj = new TubOfFsObjs( argBaseDir, argFssIdx ); Add(oObj); return oObj; }
		public void           Remove        (TubOfFsObjs oObj)                              { list.Remove(oObj); }
		public void           RemoveAll     (Predicate<TubOfFsObjs> argPred)                { list.RemoveAll(argPred); }
	}

}