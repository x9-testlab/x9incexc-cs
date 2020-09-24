//	History:
//		- 20200904 JC: Created.
//		- 20200915 JC: Renamed stuff for reduced confusion about what objects we're working with:
//			- Namespace "X9Tools.DB"          to "X9Db"
//			- Class     "Sqlite3_v1"          to "X9Sqlt3_v1"
//			- Class     "Conn_v1"             to "X9Conn_v1
//			- Property  "Conn_v1.Sqlite3Conn" to "KreugerConn"
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System;
using Sqlt3Krueger;  // sqlite-net, in "./toolchest/3rd-party/Sqlt3Krueger.cs" as of 20200915.

namespace X9.Sqlite3 {

	public class X9Conn : IDisposable {

		// Public fields
		public readonly SQLiteConnection KreugerConn;
		public          bool             IsInTransaction { get; private set; }

		// Constructor
		public X9Conn( in string sqlite3FileArg="" ) {
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
			if (IsInTransaction) X9.Util.TryAction(() => { KreugerConn.Commit(); } );
			IsInTransaction = false;
		}

		// Roll back transaction
		public void Transaction_Rollback_TryIfBegan() {
			if (IsInTransaction) X9.Util.TryAction(() => { KreugerConn.Rollback(); } );
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
		~X9Conn() => Dispose();  // Standard
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }  // Standard
		protected virtual void Dispose(bool disposing) {
			if      (_disposed) return;
			else if (disposing) {
				// PUT CODE HERE: Clean up managed resources
				KreugerConn?.Dispose();
			}
			_disposed = true;
		}

	}
}