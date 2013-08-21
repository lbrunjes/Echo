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
		
		public string ServerName{ get { return Environment.MachineName; } }

		public Dictionary<string,SyncItem> HashList = new Dictionary<string, SyncItem> ();


		public SyncList (string directory)
		{
			path = directory;
			this.loadSyncList (path);

		}

		public void loadSyncList (string directory){

			SyncFile file;
			string key;

			foreach (String filename  in Directory.GetFiles (directory)) {
				try{
				

					key =filename.Replace (path, "");

					//is teh file known?
					if(!HashList.ContainsKey(key)){
						//new files get added
						file = new SyncFile (filename);
						HashList.Add (key, file);
					}
					else{
						//does it still exist
						if(HashList[key].CheckForRemoval()){
							HashList.Remove(key);
						}
						else{
							//only update if the file changed since last run.
							if(HashList[key].CheckForModified()){
								file = new SyncFile (filename);
								HashList[key] = file;
							}
						}
					}

				}
				catch(Exception ex){
					Console.WriteLine ("skipped: " + filename + " because :" +ex.Message);
				}
			}

			foreach (String dir in Directory.GetDirectories(directory)) {
				loadSyncList (dir);
			}

		}

		public void saveSyncList (string filename){

			StreamWriter writer = new StreamWriter(File.OpenWrite (filename));

			writer.WriteLine ("{");

			foreach (KeyValuePair<string,SyncItem> kvp in HashList) {
				writer.WriteLine(String.Format("\"{0}\":\"{1}\",", kvp.Key.Replace("\\","/"), kvp.Value.Hash));
			}
			writer.WriteLine(String.Format("\"__Server\":\"{0}\",", ServerName));
			writer.WriteLine(String.Format("\"__DateGeneratedUTC\":\"{0:yy-MM-dd-hh-mm-ss}\"", DateTime.UtcNow));
			writer.Write ("}");
            writer.Flush();
			writer.Close ();


		}

	
	
		public void Dispose ()
		{

		}

	}
}

