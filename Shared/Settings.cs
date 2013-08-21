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
		public bool HaveReadSettingsFile = false;
		public static string FTPServer="ftp://127.0.0.1";
		public static string HashServer="http://127.0.0.1/hashes.txt";
		public static string HashFile="C:/inetpub/stuff/hashes.txt";
		public static string RemoteUser="ANNON";
		public static string RemotePassword ="IM_A_PT_I_SWEAR";
		public static string LocalDirectory = "C:\\Users\\Confused\\Dropbox\\inprogress\\BeatDown";
		public static bool RemoveLocalFileIfNoRemoteFile = false;
		public static int numFilesToRemoveWithNoWarning = 100;
		public static int LoopTime = 1000*60 *2;

		public const string CONFIG_FILE="../../Settings.ini";
		public const string HEADER="#Seettings for Sync system\n#FILE CREATED BY TOOL AT {0:yyyy MMM dd hh:mm:ss}";

		public static void ReadConfigFile ()
		{
			string key = "";
			string data = "";
			String[] configData=null;
			int idx;
			try{
				configData = File.ReadAllLines (CONFIG_FILE);
			}
			catch(Exception ex){
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
			//Console.WriteLine ("Loaded: " + Settings.CONFIG_FILE);
			HaveReadSettingsFile =true;
		}

		static void ProcessConfig(string key, string data){

			//TODO data validation?

			switch (key) {
			case "looptime":
				int.TryParse(data, out LoopTime);
				break;
			case "directory":
				LocalDirectory = data;
				break;
			case "hashurl":
				HashServer = data;
				break;
			case "ftpserver":
				FTPServer = data;
				break;
			case "remoteuser":
				RemoteUser = data;
				break;
			case "remotepassword":
				RemotePassword = data;
				break;
			case "removeifmissing":
				RemoveLocalFileIfNoRemoteFile = data.ToLower ().Trim () == "true";
				break;
			default:
				Console.WriteLine ("Invalid Config key: " + key);
				break;


			}
		}

	}
}

