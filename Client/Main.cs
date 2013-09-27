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
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Client
{
	public class MainEntryPoint
	{
		public static int DownloadCount =0;
		public static int ErrorCount =0;
		public static void Main (string[] args)
		{

			Dictionary<string,SyncItem> serverhashes =null;
			SyncList LocalData= null;
		
			Console.WriteLine ("Startup.  ");

			//read teh settings file
			Settings.ReadConfigFile (Settings.CONFIG_FILE_CLIENT);

			Console.WriteLine("Getting file list");
			//get the server hashlist
			try {
				serverhashes = Http.GetHashList ();
			} catch (Exception ex) {
				Console.WriteLine ("Something went wrong gettin'  our remote hashes. Oops.\n" + ex.Message );
				return;
			}

			Console.WriteLine ("getting local files");
			//hash all teh local files.
			try {
				LocalData = new SyncList (Settings.LocalDirectory);
				Console.WriteLine ("hashed:"+LocalData.HashList.Count);

				LocalData.saveSyncList (Settings.HashCache);
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
					if (! LocalData.HashList.ContainsKey (kvp.Key)){
						//downlaod files that dont exist.
						FilesToDownload.Add (kvp.Key);
						Console.WriteLine ("New File: " + kvp.Key);
					}
					else{
						if( kvp.Value.Hash != LocalData.HashList [kvp.Key].Hash) {
							//download files the dont mathc the hash
							FilesToDownload.Add (kvp.Key);
							Console.WriteLine (String.Format ("hash mismatch: {0}  ;local{2}, remote {1}", kvp.Key, kvp.Value.Hash, LocalData.HashList [kvp.Key].Hash));
						}
						else{
							//if we get here the file exist and is right.
							//get it out of the hash list so we dont delrte it.
							bool okay = LocalData.HashList.Remove (kvp.Key);
							//Console.WriteLine ("File OK: " + kvp.Key+ "okay"+okay);
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
						Console.WriteLine  ("removed: "+kvp.Key);
					}
				}
			}

			//DOWNLOADS
			Console.WriteLine ("Need to download: " + FilesToDownload.Count +" from " +Settings.DownloadType );
			int progress = 0;



			int tasklimit =10;

			int nextFile =0;
			GetObjectRequest request;
			AmazonS3Client s3 = new  AmazonS3Client (Settings.s3IDKey, Settings.s3SecretKey);
			while(nextFile < FilesToDownload.Count || DownloadCount >0 ){
				if(DownloadCount < tasklimit && nextFile<FilesToDownload.Count){
					request = new GetObjectRequest();
					request.BucketName = Settings.s3Bucket;
					request.Key = FilesToDownload[nextFile].Substring (1);//use substring so we elminate the /
					request.Timeout  =1000;//wait 1 minute for a  response.
					Console.WriteLine(nextFile+"/"+FilesToDownload.Count +" "+request.Key);

					s3.BeginGetObject(request,DownloadFile,s3);
					nextFile++;
					DownloadCount++;
				}

			}
               

            
			Console.WriteLine ("Downloaded "+(nextFile-ErrorCount)+"/"+ FilesToDownload.Count +" File");

			if (nextFile-ErrorCount != FilesToDownload.Count) {
				Console.WriteLine ("WARNING: NOT ALL FILES WERE SUCCESSFULLY DOWNLOADED");
				Console.WriteLine ("         YOU SHOULD RUN THIS TOOL AGAIN.");
			}
			Console.WriteLine("Press any key to close.");
			Console.ReadKey();
		}

		protected static void DownloadFile (IAsyncResult Result)
		{

			try {
				AmazonS3 s3 = Result.AsyncState as AmazonS3;
				GetObjectResponse r = s3.EndGetObject (Result);
				string dir = Path.GetDirectoryName (Settings.LocalDirectory +"/" + r.Key);
				if (!Directory.Exists (dir)) {

					Directory.CreateDirectory (dir);
				}
				r.WriteResponseStreamToFile (Settings.LocalDirectory +"/"+ r.Key);
			} catch (Exception ex) {
				ErrorCount++;
				Console.WriteLine(ex.Message);
			}
			DownloadCount--;

		}

	}
}

