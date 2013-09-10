//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.IO;
using LitS3;

namespace Shared
{
	public class AmazonS3
	{
		public static S3Service s3 =null;

		public static int DownloadFile (string fileName)
		{
			if (s3 == null) {
				s3 = new S3Service();
				s3.Host = Settings.s3Host;
				s3.AccessKeyID = Settings.s3Authkey;
				s3.BeforeAuthorize += (sender, e) =>
				{
				    e.Request.ServicePoint.ConnectionLimit = int.MaxValue;
				};
			}


			int filesChanged = 0;
			string dir = "";
			//remove file if it exists.
			if (File.Exists (Settings.LocalDirectory + fileName)) {
				File.Delete (Settings.LocalDirectory + fileName);
			}

			//make sure the requred directoreis exist
			dir = Path.GetDirectoryName(Settings.LocalDirectory + fileName);
			if(!Directory.Exists(dir)){
				Console.WriteLine("Creating dir");
				Directory.CreateDirectory(dir);
			}


			s3.GetObject(Settings.s3Bucket, fileName, Settings.LocalDirectory + fileName);
				
			filesChanged++;



			return filesChanged;
		}
	}
}

