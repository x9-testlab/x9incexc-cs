//	Purpose: Common string tools
//	History:
//		- 20200904 JC: Created
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System;

namespace X9 {

	/// <summary>String utilities</summary>
	public static class Strings {

		/// <summary>Purpose: Removes redundant white space in between (replacing with " "), and at the ends of a string.</summary>
		public static string StrNormalize(in string argStr="") {
			return String.Join(" ", argStr.Split((char[])null, StringSplitOptions.RemoveEmptyEntries) );
		}

		public static string EscapeQuotes(in string argStr){
			return argStr.Replace(@"'", @"\'").Replace("\"", "\\\"");
		}

	}

}