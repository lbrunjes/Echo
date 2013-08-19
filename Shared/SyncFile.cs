// 
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// 
using System;
using System.IO;

namespace Shared
{
	public class SyncFile:SyncItem
	{
		public SyncFile (string path)
		{
			this.hash = this.CalculateHash (path);
			this.path = path;
			this.ModifiedDate = File.GetLastWriteTimeUtc (path);
		}
	}
}

