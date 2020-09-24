//	Purpose: Lightweight wrapper around Sqlt3Krueger.cs. Can also work directly with that.
//	History:
//		- 20200904 JC: Created.
//		- 20200915 JC: Renamed stuff for reduced confusion about what objects we're working with:
//			- Namespace "X9Tools.DB"          to "X9Db"
//			- Class     "Sqlite3_v1"          to "X9Sqlt3_v1"
//			- Class     "Conn_v1"             to "X9Conn_v1
//			- Property  "Conn_v1.Sqlite3Conn" to "KreugerConn"
//	Notes:
//		- Relies on sqlite-net (presumably a wrapper around sqlite3.dll?)
//			- Project page: https://github.com/praeclarum/sqlite-net
//			- But seems to work OK by just including a single source file into the project: https://github.com/praeclarum/sqlite-net/blob/master/src/SQLite.cs
//		- See project notes.txt for sqlite-net reference.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static X9Tools.Str;
using Sqlt3Krueger;  // sqlite-net, in "./toolchest/3rd-party/Sqlt3Krueger.cs" as of 20200915.

namespace X9Db {

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

	public class X9Sqlt3_v1 {

		// Static constructor with overloads
		public  static X9Conn_v1 GetX9Conn( ConnTypeFlags argType=ConnTypeFlags.TempSwappable )                                                                 { return _getX9Conn( argType, "", CreateOrOpenFlags.OnlyCreateNew ); }
		public  static X9Conn_v1 GetX9Conn( in string argFileSpec, CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate )                           { return _getX9Conn( ConnTypeFlags.File, argFileSpec, argCreateOrOpen ); }
		public  static X9Conn_v1 GetX9Conn( ConnTypeFlags argType, in string argFileSpec="", CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate ) { return _getX9Conn( argType, argFileSpec, argCreateOrOpen ); }

		private static X9Conn_v1 _getX9Conn(ConnTypeFlags argType=ConnTypeFlags.TempSwappable, in string argFileSpec="", CreateOrOpenFlags argCreateOrOpen=CreateOrOpenFlags.OpenOrCreate ) {

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

			var retObj = new X9Conn_v1(sqlite3FileArg);
			if (enableWriteAheadLogging) retObj.KreugerConn.EnableWriteAheadLogging();

			return retObj;
		}


		// Inner class
		public class X9Conn_v1 : IDisposable {

			// Public fields
			public readonly SQLiteConnection KreugerConn;
			public          bool             IsInTransaction { get; private set; }

			// Constructor
			public X9Conn_v1( in string sqlite3FileArg="" ) {
				// Create/open DB and return connection object
				KreugerConn = new SQLiteConnection(sqlite3FileArg);
			}

			public string GetSqlite3Ver(){
				return KreugerConn.LibVersionNumber.ToString();
			}

			// Start transactions
			public void Transaction_Begin() {
				KreugerConn.BeginTransaction();
				IsInTransaction = true;
			}

			// Commit transaction
			public void Transaction_Commit_TryIfBegan() {
				if (IsInTransaction) X9Tools.Misc.TryAction(() => { KreugerConn.Commit(); } );
				IsInTransaction = false;
			}

			// Roll back transaction
			public void Transaction_Rollback_TryIfBegan() {
				if (IsInTransaction) X9Tools.Misc.TryAction(() => { KreugerConn.Rollback(); } );
				IsInTransaction = false;
			}

			// Execute and return nothing; .ExecuteNonQuery
			public int RunSql_NoResults_v1(in string argSQL) {
				return KreugerConn.Execute(argSQL);
			}

			// Wraps Kreuger functionality in a more OO way (don't have to pass connection object, which we already have internally).
			public X9PreparedInsertCmd PrepareInsertCmd(in string argSQL){
				var retObj = new X9PreparedInsertCmd(KreugerConn, argSQL);
				return retObj;
			}

			public void Close() {
				KreugerConn.Close();
			}

			// Standard Deconstructor, IDisoposable (https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose, https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/using-objects)
			private bool _disposed = false;
			~X9Conn_v1() => Dispose();  // Standard
			public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }  // Standard
			protected virtual void Dispose(bool disposing) {
				if      (_disposed) return;
				else if (disposing) {
					// PUT CODE HERE: Clean up managed resources
					KreugerConn?.Dispose();
				}
				_disposed = true;
			}

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

	}


	public class X9SqlBuilder_v1 {
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
					sqlUseful = StrNormalize_v1(sqlUseful);                             // Convert all whitespace to a single space.
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
