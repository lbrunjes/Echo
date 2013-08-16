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

namespace Client
{
	public class Ftp
	{

		public static void DownloadFile (string fileName)
		{
			string dir = "";
			//remove file if it exists.
			if (File.Exists (Settings.LocalDirectory + fileName)) {
				File.Delete (Settings.LocalDirectory + fileName);
			}
			try{
				//make sure the requred directoreis exist
				dir = Path.GetDirectoryName(Settings.LocalDirectory + fileName);
				if(!Directory.Exists(dir)){
					Console.WriteLine("Createing dir");
					Directory.CreateDirectory(dir);
				}

				using (StreamWriter sw = new StreamWriter(File.OpenWrite(Settings.LocalDirectory+fileName))) {


					FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create (Settings.FTPServer + fileName.Replace('\\','/'));
					ftp.Method = WebRequestMethods.Ftp.DownloadFile;

					ftp.Credentials = new NetworkCredential (Settings.RemoteUser, Settings.RemotePassword);
					FtpWebResponse response = (FtpWebResponse)ftp.GetResponse ();

					StreamReader sr = new StreamReader (response.GetResponseStream());



					while (sr.Peek() >0) {
						sw.Write (sr.Read());
					}
					sw.Flush ();
				}
			}
			catch(Exception ex){
				Console.WriteLine ("Cannot download file " + fileName + " because " + ex.Message);
			}
		}
	}
}

