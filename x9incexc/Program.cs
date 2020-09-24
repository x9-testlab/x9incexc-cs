//	Purpose: Creates a list of files, with include/exclude logic.
//	History:
//		- 20200901 JC: Created.
//	Notes:
//		- For SQL logic to create filesystem-related tables, see "../0_similar-projects/jcfilesys_2of4_combine-vertically_20191008 [copy].py".
//		- For flag-processing logic, see "../0_similar-projects/x9incexc.sh".

using static X9.Terminal;
using X9.FsObj;

namespace X9IncExc {

	class Program {

		// Return values
		enum ExitCode : int {
			Success = 0,
			General = 1
		}

		static int Main(string[] args) {
			try {
				Echo_Clean_v1();

				// Create init object, which contains parsed and validated settings.
				var oInit = new X9IncExc.Init(args);

				// DB: Create our app-specific db object, and schema
				var oX9incexcDb = new X9IncExc.Db();

				// Show the system's Sqlite3 version number
				Echo_v1();
				Echo_v1($"FYI: System-installed Sqlite version number: {oX9incexcDb.DbConn_v1.GetSqlite3Ver()}");

				// For each base directory specified on command-line, scan the file system from there and add each file to DB
				foreach ( string baseDir in oInit.Settings.SourceDirs ){

					// This is where filesystem scanning occurs; could take a while
					Echo_v1();
					Echo_v1($"Processing dir: '{baseDir}' ...");
					var oFsObjs = new TubOfFsObjs(baseDir);

					// Add scanned files to DB; could take a while
					Echo_v1($"Adding {oFsObjs.Count.ToString("N0")} entries to the database ...");  // Depending on scan method, accessing .Count may require loading them all into memory at once first.
					oX9incexcDb.AddFilesToDb(oFsObjs);

				}

				// Now add indexes and calculated ranking colum values
				oX9incexcDb.IndexAndRank();

				// Close DB
				oX9incexcDb.DbConn_v1.Close();

				Echo_v1();
				Echo_v1("Done.");

				// Exit with code
				return (int)ExitCode.Success;

			} catch (System.Exception ex) {

				Echo_v1();
				Echo_v1($"Error: in '{ex.Source}': '{ex.Message}'");
				Echo_v1();
				Echo_Clean_v1("Stack trace:");
				Echo_v1();
				Echo_Clean_v1(ex.StackTrace);

				// Exit with code
				return (int)ExitCode.General;

			} finally {
				Echo_Clean_v1();
			}
		}

	}

}
