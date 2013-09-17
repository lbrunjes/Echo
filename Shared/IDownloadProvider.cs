//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;

namespace Shared
{
	public interface IDownloadProvider
	{
		 bool testAuth();
		 int DownloadFile (string fileName);
	}
}

