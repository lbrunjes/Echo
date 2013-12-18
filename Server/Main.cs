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
			using (Mutex soloChk = new Mutex(false, "PT_SYNC_SERVER")) {

				if(!soloChk.WaitOne(0, false)){
					Console.WriteLine("ERROR: Another server instance is running. So we are not going to run. sorry.");
					return;
				}
				bool loop = true;
				if (args.Length > 0 && args [0].ToLower () == "once") {
					loop = false;
				}


				//read teh settings file
				Settings.ReadConfigFile (Settings.CONFIG_FILE_SERVER);

				Console.Write ("Starting up");

				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
				sw.Start ();
				SyncList Listing = new SyncList (Settings.LocalDirectory);

				//Loop occasionally
				do {

					//write the hashes file to the server directory.
					Listing.saveSyncList (Settings.HashFile);

					Console.WriteLine (" ... Done (" + sw.ElapsedMilliseconds + " ms)");
					sw.Stop ();

					if (loop) {	
						//wait a bit.
						Thread.Sleep (Settings.LoopTime);
						sw.Reset ();
						sw.Start ();

						//
						Console.Write ("Scanning");

						//reset the hashes
						Listing.loadSyncList (Listing.Path);
					}
				} while(loop);

			}
		}
	}
}

