// /*
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// */
using System;
using System.IO;
using System.Collections.Generic;
using Shared;

namespace Client
{
	public class MainEntryPoint
	{

		public static void Main (string[] args)
		{
			//read teh settings file
			Settings.ReadConfigFile ();

			//get the server hashlist
			Dictionary<string,string> serverhashes = Http.GetHashList ();

			//hash all teh local files.
			SyncList LocalData = new SyncList (Settings.LocalDirectory);

			List<string> FilesToDownload = new List<string> ();

			//get the delta list
			foreach(KeyValuePair<string,string> kvp in serverhashes){
			
				//remove files that dont exist server side if set to
				if (! LocalData.HashList.ContainsKey (kvp.Key) || kvp.Value != LocalData.HashList [kvp.Key]) {
					FilesToDownload.Add (kvp.Key);
					LocalData.HashList.Remove (kvp.Key);
				}

			}

			//Remove Files if set to
			if(Settings.RemoveLocalFileIfNoRemoteFile){
				foreach(KeyValuePair<string,string> kvp in LocalData.HashList){
					File.Delete(Settings.LocalDirectory+kvp.Key);
				}
			}


			Console.WriteLine ("Need to download: " + FilesToDownload.Count + " files from " +Settings.FTPServer);

			//dowlonad htem from ftp
			Ftp RemoteFtp = new Ftp ();
			foreach (string fileName in FilesToDownload) {
				Ftp.DownloadFile (fileName);

			}

			Console.WriteLine ("Download done");

		}




	}
}

