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
using System.Security.Cryptography;
using Microsoft.Web.FtpServer;
using System.Text;
using System.IO;

namespace CampfireFTPAuth
{

	public class Autheticator : BaseProvider,
	IFtpAuthenticationProvider
	{
		struct pwdata{
			public string pwhash;
			public DateTime expires;
			public pwdata(string hash, DateTime Expires){
				pwhash=hash;
				expires =Expires;
			}
		}

		string ns2_campfire_url = "https://ns2.campfirenow.com/users/me.xml";
		Dictionary<string, pwdata> AuthCache = new Dictionary<string,pwdata>();
		int AuthHours =1;


		bool IFtpAuthenticationProvider.AuthenticateUser(
			string sessionId,
			string siteName,
			string userName,
			string userPassword,
			out string canonicalUserName)
		{
			bool successfulAuth =false;
			bool isCached = false;
			//see if the user is cached.
			if (AuthCache.ContainsKey (userName) && AuthCache [userName].expires >= DateTime.Now && AuthCache [userName].pwhash == hashPw(userPassword)) {
				successfulAuth = true;
				isCached = true;

			}


			//connect to the channel using the api id and if we get a 200 return true;
			if(!isCached){
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (ns2_campfire_url);
				req.Credentials = new NetworkCredential (userName, userPassword);
				//req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));;

				HttpWebResponse resp = (HttpWebResponse)req.GetResponse ();

				if(resp.StatusCode == HttpStatusCode.OK){
					successfulAuth = true;
					AuthCache.Add("userName", new pwdata(hashPw(userPassword),DateTime.Now.AddHours(AuthHours)));
				}

			}
			canonicalUserName = userName;
			return successfulAuth;
		}

		private string hashPw(string pw){
			SHA512 sha = SHA512CryptoServiceProvider.Create ();
			MemoryStream ms = new MemoryStream (System.Text.Encoding.UTF8.GetBytes(pw));

			byte[] hash = sha.ComputeHash (ms);

			StringBuilder output = new StringBuilder ();
			foreach(Byte b in hash){
				output.Append (b.ToString ("x2"));
			}

			return output.ToString ();
		}
	}
}

