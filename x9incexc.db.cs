using System;
using X9Db;
using static X9Tools.Str;
using static X9Tools.Misc;
using static X9Tools.Terminal_v1;
using X9Tools.Filesys;


namespace X9IncExc {

	public class Db {

		// Public members
		public readonly X9Sqlt3_v1.X9Conn_v1 DbConn_v1;

		// Constructor
		public Db(){

			// Create new database
			DbConn_v1 = X9Sqlt3_v1.GetX9Conn(
				ConnTypeFlags.File,
				$@"/tmp/test/{ToIso8601Local(DateTime.Now)}.sqlite3",
				CreateOrOpenFlags.OnlyCreateNew
			);

			// Set properties
			DbConn_v1.KreugerConn.EnableWriteAheadLogging();  // Appropriate for temporary stufff (but by default for file-based dbs, is off).

			// DB: Create schema
			DbConn_v1.RunSql_NoResults_v1( GetSql_010_CreateInitialSchema() );

		}


		public void AddFilesToDb(TubOfFsObjs_v3 argFsObjs){

			// DB: Get prepared SQL insert command object
			var oPreparedCmd = DbConn_v1.PrepareInsertCmd( GetSql_020_InsertCommand() );

			// Begin transaction
			DbConn_v1.Transaction_Begin();
			try {

				// Add each one into the DB
				foreach (FsObj_v3 oFsObj in argFsObjs){
					oPreparedCmd.InsertValues(
						oFsObj.Path,
						oFsObj.SizeMiB,
						oFsObj.MTime_DotnetTicksUtc,
						//ToIso8601Local(FromDotnetTicksUtc(oFsObj.MTime_DotnetTicksUtc)),
						oFsObj.IsDir,
						oFsObj.IsFile,
						oFsObj.IsLink,
						oFsObj.IsNormal,
						oFsObj.CanList
					);
				}

				// Commit transaction
				DbConn_v1.Transaction_Commit_TryIfBegan();

			} catch (Exception ex) {
				DbConn_v1.Transaction_Rollback_TryIfBegan();
				throw ex;
			}

		}


		// We have to go row-by-row, because although we can calc magnitude scores in SQL,
		//     and even though recent version of sqlite3 support 'ROW_NUMBER() OVER(...)',
		//     the SQL to do it is either not possible, or exceedingly difficult/brittle.
		public void IndexAndRank(){

			// Now add indexes and calculated ranking colum values
			Echo_v1();
			Echo_v1($"Creating first batch of DB indexes ...");
			DbConn_v1.RunSql_NoResults_v1( GetSql_030_AddIndexes_1() );

			// Populate calculated fields
			Echo_v1($"Calculating rankings ...");

			// Get values needed for calculating
			long   mtime_dotnet_ticks_utc_Min = DbConn_v1.KreugerConn.FindWithQuery(@"SELECT MIN(mtime_dotnet_ticks_utc) FROM fsobj WHERE ( COALESCE(mtime_dotnet_ticks_utc, 0) > 0 )");
			long   mtime_dotnet_ticks_utc_Max = 0;
			double filesize_megabytes_Min     = 0;
			double filesize_megabytes_Max     = 0;
			long   ord_Min                    = 1;
			long   ord_Max                    = 0;




			try {



				DbConn_v1.Transaction_Commit_TryIfBegan();


				DbConn_v1.Transaction_Commit_TryIfBegan();
			} catch (Exception ex) {
				DbConn_v1.Transaction_Rollback_TryIfBegan();
				throw ex;
			}

			// Create remaining indexes
			Echo_v1($"Creating second batch of DB indexes ...");
			DbConn_v1.RunSql_NoResults_v1( GetSql_050_AddIndexes_2() );

		}


		public static string GetSql_010_CreateInitialSchema(){
			X9SqlBuilder_v1 sqlStr = new X9SqlBuilder_v1();

			sqlStr.AddLine(@"CREATE TABLE fsobj (" );
			sqlStr.AddLine(@"    id                      INTEGER PRIMARY KEY AUTOINCREMENT,"   );
		//	sqlStr.AddLine(@"    id                      TEXT PRIMARY,"                        ); // Base64URL-encoded UUID, e.g.: N_Fuxr57rUWptJ3mn3EmDg
		//	sqlStr.AddLine(@"    filesys_id              INTEGER NOT NULL,"                    ); // Unique ID across hosts and filesystems.
		//	sqlStr.AddLine(@"    batch_id                INTEGER NOT NULL,"                    );
			sqlStr.AddLine(@"    path                    TEXT NOT NULL,"                       );
			sqlStr.AddLine(@"    filesize_megabytes      REAL NOT NULL DEFAULT 0,"             );
			sqlStr.AddLine(@"    mtime_dotnet_ticks_utc  INTEGER,"                             );
		//	sqlStr.AddLine(@"    mtime_local             STRING,"                              );
			sqlStr.AddLine(@"    is_dir                  INTEGER,"                             );
			sqlStr.AddLine(@"    is_file                 INTEGER,"                             );
			sqlStr.AddLine(@"    is_link                 INTEGER,"                             );
			sqlStr.AddLine(@"    is_normal               INTEGER,"                             );
			sqlStr.AddLine(@"    can_list                INTEGER,"                             );
		//	sqlStr.AddLine(@"    xattrs                  TEXT,"                                );
			sqlStr.AddLine(@"    row_inserted_utc        INTEGER DEFAULT CURRENT_TIMESTAMP,"      );
		//	sqlStr.AddLine(@"    xattrs_set_utc          TEXT,"                                );
			sqlStr.AddLine(@"    score_ord_mtime         REAL,"                                ); // Counting order converted into percentile = (order - 1) / (max -1)
			sqlStr.AddLine(@"    score_ord_filesize      REAL"                                 );
			sqlStr.AddLine(@"    score_mag_mtime         REAL,"                                ); // Percentile 0-1 = (size-min) / (max-min)
			sqlStr.AddLine(@"    score_mag_filesize      REAL,"                                );
			sqlStr.AddLine(@"    score_combo_mtime       REAL,"                                ); // Weighted (or just regular) average of score_ord_* and score_mag_*
			sqlStr.AddLine(@"    score_combo_filesize    REAL,"                                );
		//	sqlStr.AddLine(@"    content_id              INTEGER,"                             );
		//	sqlStr.AddLine(@"    FOREIGN KEY(filesys_id) REFERENCES filesys(id) ON UPDATE RESTRICT ON DELETE RESTRICT,"  );
		//	sqlStr.AddLine(@"    FOREIGN KEY(batch_id)   REFERENCES batch(id)   ON UPDATE RESTRICT ON DELETE RESTRICT,"  );
		//	sqlStr.AddLine(@"    FOREIGN KEY(content_id) REFERENCES content(id) ON UPDATE RESTRICT ON DELETE RESTRICT,"  );
			sqlStr.AddLine(@");"  );

		//	sqlStr.AddLine(@"CREATE TABLE content (" );
		//	sqlStr.AddLine(@"    id                       INTEGER PRIMARY KEY AUTOINCREMENT,"   );
		//	sqlStr.AddLine(@"    crc64                    TEXT"                                 );  // Base64URL-encoded CRC-64, e.g.: PhFULcK8l50=
		//	sqlStr.AddLine(@"    row_inserted_utc         TEXT DEFAULT CURRENT_TIMESTAMP,"      );
		//	sqlStr.AddLine(@"    collision_resolution_id  TEXT,"                                );
		//	sqlStr.AddLine(@"    FOREIGN KEY(collision_resolution_id) REFERENCES collision_resolution(id) ON UPDATE RESTRICT ON DELETE RESTRICT,"  );
		//	sqlStr.AddLine(@");"  );
		//	sqlStr.AddLine(@"CREATE UNIQUE INDEX uidxD010 ON content ( crc64 );" );

		//	sqlStr.AddLine(@"CREATE TABLE collision_resolution (" );
		//	sqlStr.AddLine(@"    id                       INTEGER PRIMARY KEY AUTOINCREMENT,"            );
		//	sqlStr.AddLine(@"    blake2b                  TEXT,"                                         );  // Base64URL-encoded Blake2b, e.g. ...: IJzUU6tnzZhdL4c8TpD8XSSocTqaQ61EHTm1XLRS6OXBJvE4G_2I02ow1h_m49gtmxo5KhzHw3QdqA6_I-p3YA
		//	sqlStr.AddLine(@"    row_inserted_utc         TEXT DEFAULT CURRENT_TIMESTAMP,"               );
		//	sqlStr.AddLine(@");"  );
		//	sqlStr.AddLine(@"CREATE UNIQUE INDEX uidxE010 ON collision_resolution ( blake2b );" );

		//	Echo_Clean_v1(sqlStr.Pretty);
		//	Echo_Clean_v1(sqlStr.Useful);
			return sqlStr.Useful;
		}

		public static string GetSql_020_InsertCommand(){
			X9SqlBuilder_v1 sqlStr = new X9SqlBuilder_v1();

			sqlStr.AddLine(@"INSERT INTO fsobj ("            );
			sqlStr.AddLine(@"    path,"                      );
			sqlStr.AddLine(@"    filesize_megabytes,"        );
			sqlStr.AddLine(@"    mtime_dotnet_ticks_utc,"    );
		//	sqlStr.AddLine(@"    mtime_local,"               );
			sqlStr.AddLine(@"    is_dir,"                    );
			sqlStr.AddLine(@"    is_file,"                   );
			sqlStr.AddLine(@"    is_link,"                   );
			sqlStr.AddLine(@"    is_normal,"                 );
			sqlStr.AddLine(@"    can_list,"                  );
			sqlStr.AddLine(@")"                              );
			sqlStr.AddLine(@"VALUES (?,?,?,?,?,?,?,?);"    );

		//	X9Tools.Terminal_v1.Echo_Clean_v1(sqlStr.Pretty);
		//	X9Tools.Terminal_v1.Echo_Clean_v1(sqlStr.Useful);
			return sqlStr.Useful;
		}

		public static string GetSql_030_AddIndexes_1(){
			X9SqlBuilder_v1 sqlStr = new X9SqlBuilder_v1();
			sqlStr.AddLine(@"CREATE UNIQUE INDEX uidxC010 ON fsobj   ( path                        );"  );
		//	sqlStr.AddLine(@"CREATE UNIQUE INDEX uidxC010 ON fsobj   ( filesys_id, batch_id, path  );"  );
		//	sqlStr.AddLine(@"CREATE        INDEX  idxC010 ON fsobj   ( filesys_id                  );"  );
		//	sqlStr.AddLine(@"CREATE        INDEX  idxC020 ON fsobj   ( batch_id                    );"  );
		//	sqlStr.AddLine(@"CREATE        INDEX  idxC030 ON fsobj   ( path                        );"  );
			sqlStr.AddLine(@"CREATE        INDEX  idxC040 ON fsobj   ( filesize_megabytes          );"  );
			sqlStr.AddLine(@"CREATE        INDEX  idxC050 ON fsobj   ( mtime_dotnet_ticks_utc      );"  );
		//	sqlStr.AddLine(@"CREATE        INDEX  idxC060 ON fsobj   ( xattrs                      );"  );
			return sqlStr.Useful;
		}

		public static string GetSql_041_Calculate(){
			X9SqlBuilder_v1 sqlStr = new X9SqlBuilder_v1();
			sqlStr.AddLine(@"UPDATE * FROM fsobj        );"                         );
			sqlStr.AddLine( @")"                                                    );
			sqlStr.AddLine( @";"                                                    );
			return sqlStr.Useful;
		}

		public static string GetSql_050_AddIndexes_2(){
			X9SqlBuilder_v1 sqlStr = new X9SqlBuilder_v1();
			sqlStr.AddLine(@"CREATE INDEX idxC210 ON fsobj ( score_ord_mtime        );"  ); // Counting order converted into percentile = (order - 1) / (max -1)
			sqlStr.AddLine(@"CREATE INDEX idxC220 ON fsobj ( score_ord_filesize     );"  );
			sqlStr.AddLine(@"CREATE INDEX idxC230 ON fsobj ( score_mag_mtime        );"  ); // Percentile 0-1 = (size-min) / (max-min)
			sqlStr.AddLine(@"CREATE INDEX idxC240 ON fsobj ( score_mag_filesize     );"  );
			sqlStr.AddLine(@"CREATE INDEX idxC250 ON fsobj ( score_combo_mtime      );"  ); // Weighted (or just regular) average of score_ord_* and score_mag_*
			sqlStr.AddLine(@"CREATE INDEX idxC260 ON fsobj ( score_combo_filesize   );"  );
			return sqlStr.Useful;
		}


	}
}