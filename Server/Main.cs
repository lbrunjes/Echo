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

			Console.Write ("Starting up");

			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
			sw.Start ();
			SyncList Listing = new SyncList (Settings.LocalDirectory);

			//Loop occasionally
			while (true) {

				//write the hashes file to the server directory.
				Listing.saveSyncList (Settings.HashFile);

				Console.WriteLine (" ... Done ("+ sw.ElapsedMilliseconds+" ms)");
				sw.Stop ();
				//wait a bit.
				Thread.Sleep (Settings.LoopTime);
				sw.Reset ();
				sw.Start ();

				//
				Console.Write ("Scanning");

				//reset the hashes
				Listing.loadSyncList (Listing.Path);
			}
		}
	}
}

