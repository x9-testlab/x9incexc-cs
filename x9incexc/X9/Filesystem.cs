//	Purpose
//		- Wraps System.IO.Directory.GetFiles(), which isn't great; it ignores symlink dirs and inaccessible files.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

using System.IO;
using System.Collections.Generic;

namespace X9 {

	public static class Filesystem {

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

}