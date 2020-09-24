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
using static X9.Strings;
using Sqlt3Krueger;  // sqlite-net, in "./toolchest/3rd-party/Sqlt3Krueger.cs" as of 20200915.
