//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.IO;

namespace Shared
{
	public static class Settings
	{
		public static bool HaveReadSettingsFile = false;
	//	public static string FTPServer="ftp://127.0.0.1";
	//	public static string HTTPServer = "http://127.0.0.1";
		public static string HashServer="http://67.180.48.149:81/hashes.txt";
		public static string HashFile="I_did_not_set_the_key_hashfile_in_Settings.ini.txt";
		public static string HashCache ="hashcache.txt";
	//	public static string RemoteUser="ANNON";
	//	public static string RemotePassword ="IM_A_PT_I_SWEAR";
		public static string LocalDirectory = "I_did_not_set__LocalDirectory_in_settings.ini/";
		public static bool RemoveLocalFileIfNoRemoteFile = false;
		public static int numFilesToRemoveWithNoWarning = 0;
		public static int LoopTime = 1000*60 *2;
		public static string s3IDKey = "XXXX";
		public static string s3SecretKey = "XXXXXX";
		public static string s3Bucket = "ns2build";
	//	public static string s3Host = "s3-website-us-east-1.amazonaws.com";
		public enum DownloadTypes{S3,FTP,HTTP};
		public static DownloadTypes DownloadType = DownloadTypes.S3;

		public const string CONFIG_FILE_CLIENT="clientSettings.ini";
		public const string CONFIG_FILE_SERVER="serverSettings.ini";
		public const string HEADER="#Settings for Sync system\n#FILE CREATED BY TOOL AT {0:yyyy MMM dd hh:mm:ss}";

		public static void ReadConfigFile (string configfile)
		{
			string key = "";
			string data = "";
			String[] configData=null;
			int idx;
			try{
				configData = File.ReadAllLines (configfile);
			}
			catch(Exception ex)
            {
				//Console.WriteLine ("WARNING: cannot load config, "+ex.Message+", using defaults");
				return;
			}

			foreach (String line in configData) {
				key = line;
				idx = line.IndexOf ("#");
				if ( idx>= 0) {
					key = line.Substring(0, idx);
				}
				idx = key.IndexOf ("=");

				if (key.Length > 3 && idx >= 0) {

					data = key.Substring (idx+1).Trim();
					key = key.Substring (0, idx).Trim().ToLower();
					ProcessConfig(key,data);
				}



			}

			//Check the paths to ensue they end in /?
			//Console.WriteLine ("Loaded: " + Settings.CONFIG_FILE);
			HaveReadSettingsFile =true;
		}

		static void ProcessConfig(string key, string data){

			//TODO data validation?

			switch (key) {
			
			case "directory":
			case "localdirectory":
				LocalDirectory = data;
				break;
			case "hashurl":
			case "hashserver":
				HashServer = data;
				break;
			case "hashfile":
				HashFile = data;
				break;
	/*		case "ftpserver":
				FTPServer = data;
				break;
			case "httpserver":
				HTTPServer = data;
				break;
			case "remoteuser":
				RemoteUser = data;
				break;
			case "remotepassword":
				RemotePassword = data;
				break;
	*/		case "removeifmissing":
			case "removelocalfiles":
			case "removelocalfileifnoremotefile":
				RemoveLocalFileIfNoRemoteFile = data.ToLower ().Trim () == "true";
				break;
			case "looptime":
				int.TryParse(data,out Settings.LoopTime);
				break;
			case "deletewarninglevel":
			case "numfilestoremovewithnowarning":
				int.TryParse(data,out Settings.numFilesToRemoveWithNoWarning);
				break;
			case "s3authkey":
			case "s3idkey":
				s3IDKey = data.Trim();
				break;
			case "s3secretkey":
				s3SecretKey = data.Trim ();
				break;
			case "s3bucket":
				s3Bucket = data.Trim();
				break;
			/*case "s3host":
				s3Host = data.Trim ();
				break;*/
		/*	case "downloadtype":

				if(data.ToLower() =="ftp"){
					DownloadType = DownloadTypes.FTP;
				}
				if(data.ToLower() =="s3"){
					DownloadType = DownloadTypes.S3;
				}
				if(data.ToLower() == "http"){
					DownloadType = DownloadTypes.HTTP;
				}


				break;*/
			default:
				//Console.WriteLine ("Ignoring Config key: " + key);
				break;


			}
		}

		public static void WriteConfigFile(){
			if(File.Exists(Settings.CONFIG_FILE_CLIENT)){
				File.Delete(Settings.CONFIG_FILE_CLIENT);
			}
			using(StreamWriter sw = new StreamWriter(File.OpenWrite(Settings.CONFIG_FILE_CLIENT))){

				sw.WriteLine(String.Format(Settings.HEADER, DateTime.Now));
				System.Reflection.FieldInfo[] props = typeof(Shared.Settings).GetFields();

				foreach(System.Reflection.FieldInfo prop in props){
					if(!prop.Name.StartsWith("CONFIG_FILE") &&prop.Name != "HEADER" && prop.Name!= "HaveReadSettingsFile"){
						sw.WriteLine(String.Format("{0}={1}",prop.Name, prop.GetValue(null)));
					}
				}

			}
			Settings.ReadConfigFile(Settings.CONFIG_FILE_CLIENT);
		}

	}
}

