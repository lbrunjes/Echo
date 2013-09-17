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
			Dictionary<string,SyncItem> serverhashes =null;
			SyncList LocalData= null;
		


			//read teh settings file
			Settings.ReadConfigFile (Settings.CONFIG_FILE_CLIENT);

			//get the server hashlist
			try {
				serverhashes = Http.GetHashList ();
			} catch (Exception ex) {
				Console.WriteLine ("Something went wrong gettin'  our remote hashes. Oops.\n" + ex.Message );
				return;
			}
			//hash all teh local files.
			try {
				LocalData = new SyncList (Settings.LocalDirectory);
				Console.WriteLine ("hashed:"+LocalData.HashList.Count);
			} catch (Exception ex) {
				Console.WriteLine("Problems readin' local directory. Oops\n"+ex.Message);
				return;
			}


			List<string> FilesToDownload = new List<string> ();

			//get the delta list
			foreach(KeyValuePair<string,SyncItem> kvp in serverhashes){
			
				//ignore metadata.
				if (kvp.Key == "__Server" || kvp.Key == "__DateGeneratedUTC") {
					;
				} else {
					//Queue downloads
					if (! LocalData.HashList.ContainsKey (kvp.Key) || 
					    kvp.Value.Hash != LocalData.HashList [kvp.Key].Hash) {
						FilesToDownload.Add (kvp.Key);
						LocalData.HashList.Remove (kvp.Key);
					}
					else{
						if(LocalData.HashList.ContainsKey (kvp.Key)){
							LocalData.HashList.Remove (kvp.Key);
						}
					}
				}

			}

			//Remove Files if set to
			if(Settings.RemoveLocalFileIfNoRemoteFile){

			bool shouldDelete = true;
				//ensure we didnt delte everything accidentally.
				if (LocalData.HashList.Count > Settings.numFilesToRemoveWithNoWarning) {
					shouldDelete = false;
					Console.Write (LocalData.HashList.Count + " Files Are flagged for deletion, Remove them (Y/N)");
					char key = (char)Console.Read ();

					if( key == 'Y' || key == 'y'){
						shouldDelete =true;
					}

				}


				//remove files
				if(shouldDelete){
					foreach(KeyValuePair<string,SyncItem> kvp in LocalData.HashList){
						File.Delete(Settings.LocalDirectory+kvp.Key);
						Console.WriteLine  ("removed"+kvp.Key);
					}
				}
			}


			Console.WriteLine ("Need to download: " + FilesToDownload.Count +" from " +Settings.DownloadType );
			int progress = 0;
			int fileCount = 0;
			switch(Settings.DownloadType){

				//dowl	onad htem from s3
			case Settings.DownloadTypes.S3:
				AmazonS3 s3 =new AmazonS3();

				foreach (string fileName in FilesToDownload) {
					fileCount++;
					Console.WriteLine(String.Format("downloading: {0}/{1} ({2})",fileCount,FilesToDownload.Count, fileName));
					try{
						progress += AmazonS3.DownloadFile(fileName);
					
					}
					catch(Exception ex){
						Console.WriteLine (String.Format("Couldn't download file:{1}\n {2}",Environment.NewLine, ex.Message, ex.StackTrace));
					}

				}
				break;
			case Settings.DownloadTypes.FTP:
				Ftp RemoteFtp = new Ftp ();
				foreach (string fileName in FilesToDownload) {
					fileCount++;
					Console.WriteLine(String.Format("downloading: {0}/{1} ({2})",fileCount,FilesToDownload.Count, fileName));

					try{
					progress += Ftp.DownloadFile (fileName);
					}
					catch(Exception ex){
						Console.WriteLine (String.Format("Couldn't download file:{1}",Environment.NewLine, ex.Message, ex.StackTrace));
					}

				}
				break;
			
			case Settings.DownloadTypes.HTTP:
			
				foreach (string fileName in FilesToDownload) {
					fileCount++;
					Console.WriteLine(String.Format("downloading: {0}/{1} ({2})",fileCount,FilesToDownload.Count, fileName));

					try{
					progress += Http.DownloadFile (fileName);
					}
					catch(Exception ex){
						Console.WriteLine (String.Format("Couldn't download file:{1}",Environment.NewLine, ex.Message, ex.StackTrace));
					}

				}

				break;

			}
			Console.WriteLine ("Downloaded "+progress+"/"+ FilesToDownload.Count +" File");

			if (progress != FilesToDownload.Count) {
				Console.WriteLine ("WARNING: NOT ALL FILES WERE SUCCESSFULLY DOWNLOADED");
			}

		}




	}
}

