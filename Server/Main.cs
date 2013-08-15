//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using Shared;
using System.Threading;

namespace Server
{
	public class MainEntry
	{
		public static void Main (String[] args)
		{
			//read teh settings file
			Settings.ReadConfigFile ();
			SyncList Listing = new SyncList (Settings.LocalDirectory);

			//Loop occasionally
			while (true) {

				//write the hashes file to the server directory.
				Listing.saveSyncList (Settings.HashFile);

				//wait a bit.
				Thread.Sleep (Settings.LoopTime);

				//
				Console.WriteLine ("Scanning");

				//reset the hashes
				Listing.loadSyncList (Listing.Path);
			}
		}
	}
}

