// /*
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// */
using System;
using System.Collections.Generic;
using System.IO;

namespace Shared
{
	public class SyncList:IDisposable
	{
		protected string path = "";
		public string Path{ get { return path; } }

		protected string serverName = "unknown";
		public string ServerName{ get { return serverName; } }

		public Dictionary<string,string> HashList = new Dictionary<string, string> ();


		public SyncList (string directory)
		{
			path = directory;
			this.loadSyncList (path);

		}

		public void loadSyncList (string directory){

			SyncFile file;

			foreach (String filename  in Directory.GetFiles (directory)) {
				try{
				file = new SyncFile (filename);

				HashList.Add (filename.Replace (path, ""), file.Hash);
				}
				catch(Exception ex){
					//Console.WriteLine ("skipping" + filename);
				}
			}

			foreach (String dir in Directory.GetDirectories(directory)) {
				loadSyncList (dir);
			}

		}

		public void saveSyncList (string filename){

			StreamWriter writer = new StreamWriter(File.OpenWrite (filename));

			writer.WriteLine ("{");

			foreach (KeyValuePair<string,string> kvp in HashList) {
				writer.WriteLine(String.Format("\"{0}\":\"{1}\",", kvp.Key.Replace("\\","/"), kvp.Value));
			}
			writer.WriteLine(String.Format("\"Server\":\"{0}\"", serverName));
			writer.Write ("}");
            writer.Flush();
			writer.Close ();


		}

	
	
		public void Dispose ()
		{

		}

	}
}

