//	Purpose: Date/time object with properties that converts between dotnet time, unix ticks, and dotnet ticks.
//	TODO:
//		- Refactor to use static functions from Util.cs.
//	History:
//		- 20200904 JC: Created
//		- 20200924 JC: Moved to it's own class file per standard C# convention.

using System;

namespace X9 {

	public class BetterDateTime_v1 {

		// Private members
		private DateTime dotNetUtc = DateTime.UtcNow;  // Default to now, rather than Jan 1 0001

		// Unix epoch-related (when Unix time started at 0)
		public DateTime UnixEpochToDotnet       { get { return new DateTime(1970, 1, 1, 0, 0, 0); } }
		public long     UnixEpochToDotnetTicks  { get { return UnixEpochToDotnet.Ticks; } }
		public long     UnixEpochUnixTicks      { get { return 0; } }
		public long     DotnetTicksPerUnixTicks { get { return TimeSpan.TicksPerSecond; } }

		public DateTime DotnetUtc {
			get { return dotNetUtc; }
			set { dotNetUtc = value.ToUniversalTime(); }
		}
		public DateTime DotnetLocal {
			get { return dotNetUtc.ToLocalTime() ; }
			set { dotNetUtc = value.ToUniversalTime(); }
		}
		public long DotnetTicksUtc {
			get { return dotNetUtc.Ticks; }
			set { dotNetUtc = new DateTime(value); }
		}
		public long DotnetTicksLocal {
			get { return DotnetLocal.Ticks; }
			set { DotnetLocal = new DateTime(value); }
		}
		public long UnixTicksUtc {
			get { return (long)Math.Round( (double)((DotnetTicksUtc-UnixEpochToDotnetTicks)/DotnetTicksPerUnixTicks), 0 ); }
			set { dotNetUtc = new DateTime( (UnixEpochToDotnetTicks+(value*DotnetTicksPerUnixTicks)) ); }
		}
		public long UnixTicksLocal {
			get { return (long)Math.Round( (double)((DotnetTicksLocal-UnixEpochToDotnetTicks)/DotnetTicksPerUnixTicks), 0 ); }
			set { DotnetLocal = new DateTime( (UnixEpochToDotnetTicks+(value*DotnetTicksPerUnixTicks)) ); }
		}
		public string SerialUtc {
			get { return DotnetUtc.ToString("yyyyMMdd-HHmmss-ffff"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string SerialLocal {
			get { return DotnetLocal.ToString("yyyyMMdd-HHmmss-ffff"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string Iso8601Utc {
			get { return DotnetUtc.ToString(@"yyyy-MM-ddTHH\:mm\:ssZ"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}
		public string Iso8601Local {
			get { return DotnetLocal.ToString(@"yyyy-MM-ddTHH\:mm\:sszzz"); }
		//	set { throw new ApplicationException("Not yet implemented: 'BetterDateTime_v1.SerialLocal{set}'.") ; }
		}

	}

}