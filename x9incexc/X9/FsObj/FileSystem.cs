//	Purpose: Part of filesystem object hierarchy.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

namespace X9.FsObj {

	// Describes a filesystem. Not well-fleshed-out yet.
	public class FileSystem{
		public string Type                            { get; private set; }
		public string FilesysId                       { get; private set; }
		public string PartitionId                     { get; private set; }
		public string DiskId                          { get; private set; }
		public string Path                            { get; private set; }
	}

}