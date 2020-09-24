//	History:
//		- 20200904 JC: Created.
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static X9.Strings;

namespace X9 {

	public class X9SqlBuilder {
		private string sqlPretty;
		private string sqlUseful;
		private bool doStrsNeedRecalc_Pretty=false;
		private bool doStrsNeedRecalc_Useful=false;
		List<string> rawList = new List<string>();

		public string Pretty {
			get {
				if (doStrsNeedRecalc_Pretty) {

					// Concatenate the list into a newline-delimited string
					sqlPretty = String.Join("\n", rawList);

					// Clean stuff up to avoid SQL errors (e.g. extra chars that allow easier initial building)
					sqlPretty = Regex.Replace(sqlPretty, @",(\s*\))",    @"$1");      // Remove last comma before closing parenthesis, which would cause statement to fail (but makes editing SQL statements much easier)
					sqlPretty = Regex.Replace(sqlPretty, @",(\s*;)",     @"$1");      // Remove last comma before semicolon, which would cause statement to fail (but makes editing SQL statements much easier)
					sqlPretty = Regex.Replace(sqlPretty, @",(\s*FROM)",  @"$1");      // Remove last comma before 'FROM', which would cause statement to fail (but makes editing SQL statements much easier)
					sqlPretty = Regex.Replace(sqlPretty, @",(\s*WHERE)", @"");        // Remove last comma before 'WHERE', which would cause statement to fail (but makes editing SQL statements much easier)

					// Make prettier
//					sqlPretty = Regex.Replace(sqlPretty, @"\(\s*",       @"(");       // Remove space after opening opening paren.
//					sqlPretty = Regex.Replace(sqlPretty, @"\s*\)",       @")");       // Remove space before closing paren.
					sqlPretty = Regex.Replace(sqlPretty, @"\s*;",        @";");       // Remove space before semicolon.
					sqlPretty = Regex.Replace(sqlPretty, @"\t",          @"    ");    // Replace tabs with 4 spaces

					doStrsNeedRecalc_Pretty=false;
				}
				return sqlPretty;
			}
		}

		public string Useful {
			get {
				if (doStrsNeedRecalc_Useful) {

					// Start with pretty (easier to clean up than raw)
					sqlUseful = Pretty;

					// Normalize into a regular string
					sqlUseful = StrNormalize(sqlUseful);                             // Convert all whitespace to a single space.
					sqlUseful = Regex.Replace(sqlUseful, @"\(\s*",       @"(");      // Remove space after opening opening paren.
					sqlUseful = Regex.Replace(sqlUseful, @"\s*\)",       @")");      // Remove space before closing paren.

					doStrsNeedRecalc_Useful=false;
				}
				return sqlUseful;
			}
		}

		public void AddLine(in string argStr) {
			rawList.Add(argStr);
			doStrsNeedRecalc_Pretty=true;
			doStrsNeedRecalc_Useful=true;
		}

	}

}