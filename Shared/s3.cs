//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;

namespace Shared
{
	public class AmazonS3
	{
		public static AmazonS3Client s3 =null;
		public AmazonS3 ()
		{

			s3 = new AmazonS3Client (Settings.s3IDKey, Settings.s3SecretKey);
				ListObjectsRequest lo = new ListObjectsRequest();
				lo.BucketName = Settings.s3Bucket;

				ListObjectsResponse lr =  s3.ListObjects(lo);

				foreach(S3Object obj in lr.S3Objects){
					Console.WriteLine(obj.Key);
				}

		}

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

			using (GetObjectResponse response = s3.GetObject(r)) {
				using(StreamReader reader = new StreamReader(response.ResponseStream)){
				
					File.WriteAllText(Settings.LocalDirectory+ fileName, reader.ReadToEnd());

				
					filesChanged++;
				}
			}


			return filesChanged;
		}
	}
}

