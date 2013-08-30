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
		public static Dictionary<string,SyncItem> GetHashList(){
			Dictionary<string,SyncItem> Hashlist = new Dictionary<string,SyncItem> ();

			//get remote hash data
			string HashJSON = getHashData ();

			//read JSON
			JsonTextParser parser = new JsonTextParser ();
			JsonObject hashes = parser.Parse(HashJSON);

			foreach (JsonObject field in hashes as JsonObjectCollection) {
		
				if(field.GetValue().GetType().Name !="String" ){

					List<JsonObject> obj  = (List<JsonObject>)field.GetValue();



					SyncFile sf =new SyncFile(field.Name.ToString(), obj);		                                       
					Hashlist.Add (field.Name, sf);
				}
				else{
					Console.WriteLine(field.Name, field.GetValue().ToString());
				}
			}

			return Hashlist;
		}

		private static string getHashData(){
			string data = "";

				HttpWebRequest req =  (HttpWebRequest)WebRequest.Create(Settings.HashServer);
				HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
				StreamReader read = new StreamReader( resp.GetResponseStream());
				data = read.ReadToEnd();
				//Console.WriteLine(data);
				resp.Close();
			
			return data;
		}



	}
}

