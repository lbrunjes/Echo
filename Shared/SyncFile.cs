// 
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// 
using System;
using System.IO;
using System.Net.Json;
using System.Globalization;
using System.Collections.Generic;

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
		public SyncFile (string _path, JsonObject obj)
		{
			path = _path;
			foreach (JsonObject field in obj as JsonObjectCollection) {

				if(field.Name == "hash"){
					this.hash = (string)field.GetValue();
				}
				if(field.Name == "time"){

					DateTime.TryParseExact(
						(string)field.GetValue(), 
						"yyyy-MM-dd hh:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeUniversal,
						out this.ModifiedDate);

				}

			}
		}
		public SyncFile (string _path, List<JsonObject> obj)
		{
			path = _path;
		
			foreach (JsonObject field in obj) {

				if (field.Name == "hash") {
					this.hash = (string)field.GetValue ();
				}
				if (field.Name == "time") {

					DateTime.TryParseExact (
						(string)field.GetValue (), 
						"yyyy-MM-dd hh:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeUniversal,
						out this.ModifiedDate);

				}

			}
		}
	}
}

