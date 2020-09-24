//	Purpose: Common cross-platform tools.
//	History:
//		- 20200904 JC: Created.
//		- 20200924 JC: Moved to it's own class file per standard C# convention.

namespace X9 {

	/// <summary>OS-specific stuff.</summary>
	public static class CrossPlatform {

		public static readonly string pathSlash=System.IO.Path.DirectorySeparatorChar.ToString();  // Short-hand for this long-ass thing.

	}

}