//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.IO;
using System.Security;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Shared
{


	[SecurityCriticalAttribute]
	public class AmazonS3
	{
		public static AmazonS3Client s3 =null;
		public AmazonS3 ()
		{

			s3 = new  AmazonS3Client (Settings.s3IDKey, Settings.s3SecretKey);	

			/*ListObjectsRequest req = new ListObjectsRequest ();
			req.BucketName = Settings.s3Bucket;
			Console.Write("listing objects: ");
			using (ListObjectsResponse r = s3.ListObjects(req)) {
				Console.WriteLine(r.S3Objects.Count);

			}*/

		}
		public bool testAuth(){

			return true;
		}
		[SecurityCriticalAttribute]
		public static int DownloadFile (string fileName)
		{

			int filesChanged = 0;
			string dir = "";
			//remove file if it exists.
			if (File.Exists (Settings.LocalDirectory + fileName)) {
				File.Delete (Settings.LocalDirectory + fileName);
			}

			//make sure the requred directoreis exist
			dir = Path.GetDirectoryName (Settings.LocalDirectory + fileName);
			if (!Directory.Exists (dir)) {
				Console.WriteLine ("Creating dir");
				Directory.CreateDirectory (dir);
			}
			GetObjectRequest r = new GetObjectRequest ();
			r.BucketName = Settings.s3Bucket;
			r.Key = fileName.Substring (1);//use substring so we elminate the /
			r.Timeout  =1000;//wait 1 minute fora  response.

			using (GetObjectResponse response = s3.GetObject(r)) {
				using (FileStream file  = File.OpenWrite(Settings.LocalDirectory+fileName)) {
					using (Stream reader = response.ResponseStream) {
					
						reader.CopyTo (file);

						filesChanged++;
					}
				}
			}


			return filesChanged;
		}
	}
}

