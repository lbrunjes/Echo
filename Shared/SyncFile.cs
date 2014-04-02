// 
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// 
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Shared
{
	public class SyncFile:SyncItem
	{
		public SyncFile (string path)
		{
			this.hash = this.CalculateHash (path);
			this.path = path;
			this.ModifiedDate = File.GetLastWriteTimeUtc (path);
            this.FileSize = (int) new FileInfo(path).Length;
		}

		public SyncFile (string _path, JObject obj)
		{
			path = _path;

			foreach (var field in obj.Children<JProperty>()) {

				if(field.Name == "hash"){
					this.hash = (string)field.Value;
				}

				if(field.Name == "time"){
					DateTime.TryParseExact(
                        (string)field.Value,
						"yyyy-MM-dd hh:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeUniversal,
						out this.ModifiedDate);
				}

                if (field.Name == "size"){
                    this.FileSize = (int)field.Value;
                }

			}
		}
	}
}

