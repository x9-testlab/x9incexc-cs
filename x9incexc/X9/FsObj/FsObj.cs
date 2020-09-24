//	Purpose: Part of filesystem object hierarchy.
//	History:
//		- 20200912 JC: Created.
//		- 20200924 JC: Refactored this class into it's own file per standard C# convention.

using System;
using System.IO;
using System.Collections.Generic;
using static X9.Util;

namespace X9.FsObj {

	// Bottom-layer class in all this (can also be used as lone object[s])
	public class FsObj_v3 {

		// Private properties
		private string            _path;

		// Public properties ( private setters aren't perfect, but these can't be readonly since these technically aren't set in public constructor)
		public string    Name                    { get; private set; }
		public long      ParentIdx               { get; private set; }
		public int       FilesystemsIdx          { get; private set; }
		public bool      IsFile                  { get; private set; }
		public bool      IsDir                   { get; private set; }
		public bool      IsLink                  { get; private set; }  // Symbolic link, Reparse point; can be both this, and File or Folder.
		public bool      IsNormal                { get; private set; }  // E.g. not a symlink, mountpoint, socket, etc.
		public bool      IsVisible               { get; private set; }  // Hidden attribute in Windows, "." file in *nx.
		public bool      CanList                 { get;         set; }  // Can tell this by failing on directory listing.
	//	public bool      CanRead                 { get; private set; }  // Can't tell this in dotnet, for files, without trying to open it.
	//	public bool      CanExecute              { get; private set; }
	//	public bool      CanWrite                { get; private set; }
		public double    SizeMiB                 { get; private set; }
		public long      MTime_DotnetTicksUtc    { get; private set; }
	//	public long      PrevIdx                 { get; private set; }
	//	public long      NextIdx                 { get; private set; }
	//	public bool      CanList                 { get; private set; }

		// Properties that can't be obtained via dotnet core 3.1
	//	public long      CRTime_Created          { get; private set; }  // Doesn't seem to work on Linux
	//	public long      CTime_MetadataChanged   { get; private set; }  // ctime not directly supported in dotnet core yet.
	//	public long      Inode                   { get; private set; }  // inode info
	//	public long      RefCount                { get; private set; }  // How many hardlinks
	//	public bool      IsMountpoint            { get; private set; }
	//	public bool      IsSocket                { get; private set; }
	//	public bool      IsNamedPipe             { get; private set; }
	//	public bool      IsGood                  { get; private set; }  // Symlinks can be broken.
	//	public bool      CanDelete               { get; private set; }
	//	public bool      CanOwn                  { get; private set; }
	//	public bool      DoesOwn                 { get; private set; }
	//	public string    OwnerName               { get; private set; }
	//	public long      OwnerId                 { get; private set; }
	//	public string    XattrsStr               { get; private set; }
	//	public ulong     Content_crc64_binary    { get; private set; }
	//	public byte[]    Content_blake2b_binary  { get; private set; }

		public string Path { get {
		//	if (_path != "") {
		//		throw new ApplicationException("Property 'Path' not yet implimented.");
		//	}
			return _path;
		}}

		public FsObj_v3       (in string argName = "", in string argPath = "", long argParentIdx = 0, int argFilesystemsIdx = -1){

			// The main stuff
			Name           = argName;
			_path          = argPath;
			ParentIdx      = argParentIdx;
			FilesystemsIdx = argFilesystemsIdx;

			// Ideally the next lines wouldn't run until a property is actually inspected.
			// Unfortunately such lazy get/set complicates properties (can't use syntactic sugar),
			// And causes a stack overflow when self-setting while reading.
			// Once this interface settles, fix that. (By using backing private properties.)

			try {

				// Get attribute inspection objects
				var dotnetAttribs = System.IO.File.GetAttributes(Path);
				var dotnetFileInfo = new System.IO.FileInfo(Path);

				// Inspect
				CanList               = true;                                                // Probably self-evident.
				IsDir                 = dotnetAttribs.HasFlag(FileAttributes.Directory);
				IsFile                = !IsDir;
				IsLink                = dotnetAttribs.HasFlag(FileAttributes.ReparsePoint);  // Doesn't tell us if Junction or Symbolic Link.
				IsNormal              = (IsFile | IsDir ) & !IsLink;                         // & dotnetAttribs.HasFlag(FileAttributes.Normal);  // FileAttributes.Normal doesn't seem to do what you'd think.
				IsVisible             = !dotnetAttribs.HasFlag(FileAttributes.Hidden);
			//	CanWrite              = !dotnetAttribs.HasFlag(FileAttributes.ReadOnly);     // For Linux is supposed to be based on permissions.
				MTime_DotnetTicksUtc  = ToDotnetTicksUtc(dotnetFileInfo.LastWriteTimeUtc);
				if (IsFile) SizeMiB   = (double)dotnetFileInfo.Length / Math.Pow(2, 20);     // MiB = 2^20, KiB = 2^10

			} catch {
				CanList               = false;                                                // Probably self-evident.
			}

		}

		public Dictionary<string, string> GetXattrsDict() {
			var retVal = new Dictionary<string, string>();
			if (true) throw new ApplicationException("Method 'GetXattrsDict' not yet implimented.");
			//return retVal;
		}

		// Creates or changes an xattr by keyname.
		public void SetXattr(in string key, in string val){
			throw new ApplicationException("Method 'SetXattr' is not implemented yet.");
		}

	}

}