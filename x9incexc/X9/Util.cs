//	Purpose: Common tools
//	History:
//		- 20200904 JC: Created
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static X9.CrossPlatform;
using static X9.Strings;
using static X9.Util;
using static X9.Terminal;


namespace X9 {


	/// <summary>Misc utilities</summary>
	public static class Util {

		public const int invalidIndex = -1;

		public enum FileTimestampTypes{
			CTime_MetadataChanged,
			MTime_ContentChanged,
			CRTime_Created
		}

		// Try to do something and ignore errors
		public static bool TryAction(Action operation) {
			if (operation == null) return false;
			try { operation.Invoke(); }
			catch { return false; }
			return true;
		}

		public static object ConvertTo_v1 (object obj, Type t)
		{
			Type nut = Nullable.GetUnderlyingType (t);
			if (nut != null) {
				if (obj == null) { return null; }
				else { return Convert.ChangeType (obj, nut); }
			} else { return Convert.ChangeType (obj, t); }
		}


		// Time-related
		public static long     ToDotnetTicksUtc        (DateTime argDt)      { return argDt.ToUniversalTime().Ticks; }
		public static long     ToDotnetTicksLocal      (DateTime argDt)      { return argDt.ToLocalTime().Ticks; }
		public static long     ToUnixTicksUtc          (DateTime argDt)      { return (long)Math.Round( (double)((ToDotnetTicksUtc   (argDt) - UnixEpochToDotnetTicks()) / DotnetTicksPerUnixTicks() ), 0 ); }
		public static long     ToUnixTicksLocal        (DateTime argDt)      { return (long)Math.Round( (double)((ToDotnetTicksLocal (argDt) - UnixEpochToDotnetTicks()) / DotnetTicksPerUnixTicks() ), 0 ); }
		public static string   ToSerialUtc             (DateTime argDt)      { return argDt.ToUniversalTime ().ToString ( "yyyyMMdd-HHmmss-ffff"); }
		public static string   ToSerialLocal           (DateTime argDt)      { return argDt.ToLocalTime     ().ToString ( "yyyyMMdd-HHmmss-ffff"); }
		public static string   ToIso8601Utc            (DateTime argDt)      { return argDt.ToUniversalTime ().ToString (@"yyyy-MM-ddTHH:mm:ssZ"); }
		public static string   ToIso8601Local          (DateTime argDt)      { return argDt.ToLocalTime     ().ToString (@"yyyy-MM-ddTHH:mm:sszzz"); }
		public static DateTime FromDotnetTicksUtc      (long argDotnetTicks) { var tmpDotnet = new DateTime(argDotnetTicks); return DateTime.SpecifyKind( tmpDotnet, DateTimeKind.Utc   ); }
		public static DateTime FromDotnetTicksLocal    (long argDotnetTicks) { var tmpDotnet = new DateTime(argDotnetTicks); return DateTime.SpecifyKind( tmpDotnet, DateTimeKind.Local ); }
		public static DateTime FromUnixTicksUtc        (long argUnixTicks)   { var tmpDotnet = new DateTime( (UnixEpochToDotnetTicks() + ( argUnixTicks * DotnetTicksPerUnixTicks() )) ); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Utc   ); }
		public static DateTime FromUnixTicksLocal      (long argUnixTicks)   { var tmpDotnet = new DateTime( (UnixEpochToDotnetTicks() + ( argUnixTicks * DotnetTicksPerUnixTicks() )) ); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Local ); }
		public static DateTime UnixEpochToDotnetUtc    ()                    { var tmpDotnet = new DateTime(1970, 1, 1, 0, 0, 0); return DateTime.SpecifyKind(tmpDotnet, DateTimeKind.Utc); }
		public static long     UnixEpochToDotnetTicks  ()                    { return UnixEpochToDotnetUtc().Ticks; }
		public static long     DotnetTicksPerUnixTicks ()                    { return TimeSpan.TicksPerSecond; }

		// IsNothing_v1; with overloads
		public static bool IsNothing_v1(in dynamic  argVal, in dynamic  alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(in DateTime argVal, in DateTime alsoTreatAsNothing=default(DateTime) ) { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(in string   argVal, in string   alsoTreatAsNothing="" )                { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(bool?       argVal, bool?       alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(int?        argVal, int?        alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(long?       argVal, long?       alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(float?      argVal, float?      alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(double?     argVal, double?     alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }
		public static bool IsNothing_v1(decimal?    argVal, decimal?    alsoTreatAsNothing=null )              { return ( argVal == null || argVal == alsoTreatAsNothing ); }

		// ValOrDefault_v1; with overloads
		public static dynamic   ValOrDefault_v1(in dynamic   argVal, in dynamic  argDefault, in dynamic  alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return            argVal; }
		public static DateTime  ValOrDefault_v1(in DateTime? argVal, in DateTime argDefault, in DateTime alsoTreatAsNull=default(DateTime) ) { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (DateTime) argVal; }
		public static string    ValOrDefault_v1(in string    argVal, in string   argDefault, in string   alsoTreatAsNull="" )                { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (string)   argVal; }
		public static bool      ValOrDefault_v1(bool?        argVal, bool        argDefault                                 )                { if ( argVal == null )                              return argDefault; else return (bool)     argVal; }
		public static int       ValOrDefault_v1(int?         argVal, int         argDefault, int?        alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (int)      argVal; }
		public static long      ValOrDefault_v1(long?        argVal, in long     argDefault, in long?    alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (long)     argVal; }
		public static float     ValOrDefault_v1(float?       argVal, in float    argDefault, in float?   alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (float)    argVal; }
		public static double    ValOrDefault_v1(double?      argVal, in double   argDefault, in double?  alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (double)   argVal; }
		public static decimal   ValOrDefault_v1(decimal?     argVal, in decimal  argDefault, in decimal? alsoTreatAsNull=null )              { if ( argVal == null || argVal == alsoTreatAsNull ) return argDefault; else return (decimal)  argVal; }


		// Try function, ignore errors, and return only defined type (these are breathtaking not useful)
		public static object   TryFunctionOrDefault(Func<object>   doFunction, object   argDefault, object      alsoTreatAsNull=null )             { object   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static DateTime TryFunctionOrDefault(Func<DateTime> doFunction, DateTime argDefault, DateTime    alsoTreatAsNull=default(DateTime)) { DateTime retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static string   TryFunctionOrDefault(Func<string>   doFunction, string   argDefault, string      alsoTreatAsNull=""   )             { string   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static bool     TryFunctionOrDefault(Func<bool>     doFunction, bool     argDefault, bool?       alsoTreatAsNull=null )             { bool     retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static int      TryFunctionOrDefault(Func<int>      doFunction, int      argDefault, int?        alsoTreatAsNull=null )             { int      retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static long     TryFunctionOrDefault(Func<long>     doFunction, long     argDefault, long?       alsoTreatAsNull=null )             { long     retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static float    TryFunctionOrDefault(Func<float>    doFunction, float    argDefault, float?      alsoTreatAsNull=null )             { float    retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static double   TryFunctionOrDefault(Func<double>   doFunction, double   argDefault, double?     alsoTreatAsNull=null )             { double   retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }
		public static decimal  TryFunctionOrDefault(Func<decimal>  doFunction, decimal  argDefault, decimal?    alsoTreatAsNull=null )             { decimal  retVal=argDefault; if (doFunction != null) { try { retVal=doFunction.Invoke(); } catch { retVal=argDefault; } } return retVal; }

		// Dummy to stand in for things sometimes (which is usually a code sound)
		public static void noop(){ ; }

	}

}