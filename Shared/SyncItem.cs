//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
// 
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Shared
{
	public abstract class SyncItem
	{

		protected string hash = "";
		public string Hash{ get { return hash; } }


		public string ServerHash="";

		public DateTime ModifiedDate;
		public DateTime ServerModifiedDate;

		protected string path = "";
		public string Path{ get { return path; } }



		protected bool isDownloading = false;
		public bool IsDownloading{ get { return isDownloading; } }

		public bool CheckForModified(){
			return File.GetLastWriteTimeUtc (Path) != ModifiedDate;
		}
		public bool CheckForRemoval(){
			return !File.Exists (Path);
		}

		protected string CalculateHash (string filename){
			using (FileStream stream = new FileStream( filename, FileMode.Open)) {
				return CalculateHash (stream);
			}
		}

		protected string  CalculateHash(Stream stream){
			StringBuilder output = new StringBuilder ();
			if (stream != null) {
				stream.Seek (0, SeekOrigin.Begin);

				MD5 md5 = MD5CryptoServiceProvider.Create ();
				byte[] hash = md5.ComputeHash (stream);
				foreach (byte b in hash) {
					output.Append (b.ToString ("x2"));
				}

				stream.Seek (0, SeekOrigin.Begin);

			}
			return output.ToString ();
		}




	}
}

