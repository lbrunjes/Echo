//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Json;
using System.IO;
using Shared;

namespace Shared
{
	public static class Http
	{
		public static Dictionary<string,string> GetHashList(){
			Dictionary<string,string> Hashlist = new Dictionary<string,string> ();

			//get remote hash data
			string HashJSON = getHashData ();

			//read JSON
			JsonObject hashes = ParseObject (HashJSON);

			foreach (JsonObject field in hashes as JsonObjectCollection) {
				Hashlist.Add (field.Name, field.GetValue().ToString());
			}

			return Hashlist;
		}

		private static string getHashData(){
			string data = "";
			try{
				HttpWebRequest req =  (HttpWebRequest)WebRequest.Create(Settings.HashServer);
				HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
				StreamReader read = new StreamReader( resp.GetResponseStream());
				data = read.ReadToEnd();
				//Console.WriteLine(data);
				resp.Close();
			}
			catch(Exception ex){
				Console.WriteLine ("ERROR: CANNOT READ HASHLIST: " + ex.Message + "\n" + ex.StackTrace);
			}
			return data;
		}

		private static JsonObject ParseObject(string data){
			JsonTextParser parser = new JsonTextParser ();
				return parser.Parse(data);

		}


	}
}

