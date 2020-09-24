//	Purpose: Common tools
//	History:
//		- 20200904 JC: Created
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static X9.CrossPlatform;
using static X9.Strings;
using static X9.Util;
using static X9.Terminal;


namespace X9 {


	/// <summary>Console wrapper utilities.</summary>
	public static class Terminal {

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

}