//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.IO;
using System.Net;
using Shared;

namespace Shared
{
	public class Ftp
	{

		public static int DownloadFile (string fileName)
		{
			int filesChanged = 0;
			string dir = "";
			//remove file if it exists.
			if (File.Exists (Settings.LocalDirectory + fileName)) {
				File.Delete (Settings.LocalDirectory + fileName);
			}

				//make sure the requred directoreis exist
				dir = Path.GetDirectoryName(Settings.LocalDirectory + fileName);
				if(!Directory.Exists(dir)){
					Console.WriteLine("Createing dir");
					Directory.CreateDirectory(dir);
				}

				using (FileStream file  = File.OpenWrite(Settings.LocalDirectory+fileName)) {


					FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create (Settings.FTPServer + fileName.Replace('\\','/'));
					ftp.Method = WebRequestMethods.Ftp.DownloadFile;
					ftp.UseBinary =true;

					ftp.Credentials = new NetworkCredential (Settings.RemoteUser, Settings.RemotePassword);
					FtpWebResponse response = (FtpWebResponse)ftp.GetResponse ();

					Stream ftpdata = response.GetResponseStream();
					ftpdata.CopyTo (file);
					filesChanged ++;
					
				}


			return filesChanged;
		}
	}
}

