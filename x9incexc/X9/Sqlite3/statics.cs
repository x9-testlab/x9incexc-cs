//	Purpose: Lightweight wrapper around Sqlt3Krueger.cs. Can also work directly with that.
//	History:
//		- 20200904 JC: Created.
//		- 20200915 JC: Renamed stuff for reduced confusion about what objects we're working with:
//			- Namespace "X9Tools.DB"          to "X9Db"
//			- Class     "Sqlite3_v1"          to "X9Sqlt3_v1"
//			- Class     "Conn_v1"             to "X9Conn_v1
//			- Property  "Conn_v1.Sqlite3Conn" to "KreugerConn"
//		- 20200924 JC: Refactored into individual class files per standard C# convention.
//	Notes:
//		- Relies on sqlite-net (presumably a wrapper around sqlite3.dll?)
//			- Project page: https://github.com/praeclarum/sqlite-net
//			- But seems to work OK by just including a single source file into the project: https://github.com/praeclarum/sqlite-net/blob/master/src/SQLite.cs
//		- See project notes.txt for sqlite-net reference.

using System.IO;

namespace X9.Sqlite3 {

	public enum ConnTypeFlags {
		TempMem,
		TempSwappable,
		File
	}

	public enum CreateOrOpenFlags {
		OnlyOpenExisting,
		OnlyCreateNew,
		OpenOrCreate
	}

	public class X9Sqlt3 {

		// Static constructor with overloads
		public  static X9Conn GetX9Conn( ConnTypeFlags argType=ConnTypeFlags.TempSwappable )                                                                 { return _getX9Conn( argType, "", CreateOrOpenFlags.OnlyCreateNew ); }
		public  static X9Conn GetX9Conn( in string argFileSpec, CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate )                           { return _getX9Conn( ConnTypeFlags.File, argFileSpec, argCreateOrOpen ); }
		public  static X9Conn GetX9Conn( ConnTypeFlags argType, in string argFileSpec="", CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate ) { return _getX9Conn( argType, argFileSpec, argCreateOrOpen ); }

		private static X9Conn _getX9Conn(ConnTypeFlags argType=ConnTypeFlags.TempSwappable, in string argFileSpec="", CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate ) {

			string sqlite3FileArg;  // By default, empty string will create a swappable temp memory DB

			// Validate
			if ( argType != ConnTypeFlags.File && argFileSpec != "" )  throw new System.ArgumentException("Can't specify a database file if ConnType is not 'File'.");
			if ( argType == ConnTypeFlags.File && argFileSpec == "" )  throw new System.ArgumentException("Can't specify ConnType.File without providing a file location and/or name.");
			if ( argType != ConnTypeFlags.File && argCreateOrOpen == CreateOrOpenFlags.OnlyOpenExisting )  throw new System.ArgumentException("Can't open an existing in-memory database.");
			if ( argType == ConnTypeFlags.File && argCreateOrOpen == CreateOrOpenFlags.OnlyCreateNew    &&  File.Exists(argFileSpec) )  throw new System.ArgumentException($"Sqlite3 database already exists: '{argFileSpec}'.");
			if ( argType == ConnTypeFlags.File && argCreateOrOpen == CreateOrOpenFlags.OnlyOpenExisting && !File.Exists(argFileSpec) )  throw new System.IO.FileNotFoundException($"Could not find or access Sqlite3 database: '{argFileSpec}'.");

			// Figure out what to pass to sqlite3 for file argument
			bool enableWriteAheadLogging = false;
			if      ( argType == ConnTypeFlags.TempSwappable ) { sqlite3FileArg = "";          enableWriteAheadLogging = true; }  // Short-hand in sqlite3 library for temp db.
			else if ( argType == ConnTypeFlags.TempMem )       { sqlite3FileArg = ":memory:";  enableWriteAheadLogging = true; }
			else                                               { sqlite3FileArg = argFileSpec; }

			var retObj = new X9Conn(sqlite3FileArg);
			if (enableWriteAheadLogging) retObj.KreugerConn.EnableWriteAheadLogging();

			return retObj;

		}
	}

}