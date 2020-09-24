//	History:
//		- 20200904 JC: Created.
//		- 20200915 JC: Renamed stuff for reduced confusion about what objects we're working with:
//			- Namespace "X9Tools.DB"          to "X9Db"
//			- Class     "Sqlite3_v1"          to "X9Sqlt3_v1"
//			- Class     "Conn_v1"             to "X9Conn_v1
//			- Property  "Conn_v1.Sqlite3Conn" to "KreugerConn"
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System.Collections.Generic;
using Sqlt3Krueger;  // sqlite-net, in "./toolchest/3rd-party/Sqlt3Krueger.cs" as of 20200915.

namespace X9.Sqlite3 {

	public class X9PreparedInsertCmd {

		// Private members
		private readonly PreparedSqlLiteInsertCommand KreugerPIC;

		// Constructor
		public X9PreparedInsertCmd(SQLiteConnection argKreugerConn, in string argSQL) {
			KreugerPIC = new PreparedSqlLiteInsertCommand(argKreugerConn, argSQL);
		}

		// Do the insert (with overloads)
		public int InsertValues(List<dynamic> argValues){
			return KreugerPIC.ExecuteNonQuery_x9v1(argValues);
		}
		public int InsertValues(params dynamic[] argValues){
			return KreugerPIC.ExecuteNonQuery_x9v1(argValues);
		}

	}
}