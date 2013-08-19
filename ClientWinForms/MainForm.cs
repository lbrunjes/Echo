//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.Drawing;
using System.Windows.Forms;
using Shared;
using System.Collections.Generic;
using System.IO;

namespace ClientWinForms
{
	public class MainForm:Form
	{
		Label HServer = new Label();
		Label FServer = new Label();
		Label Folder = new Label();
		Label ConfigFile = new Label();
		Label DeleteEnabled = new Label();

		Button StartSync = new Button();
		TextBox Console = new TextBox();
		ProgressBar Progress = new ProgressBar();
		public MainForm ()
		{
			this.SuspendLayout();
			this.Width =768;
			this.Font = new Font("monospaced", 12);

			Settings.ReadConfigFile();

			HServer.Text = "HASHES: " + Settings.HashServer;
			HServer.Location = new Point(0,0);
			HServer.Width = 512;

			FServer.Text = "FTP: "+Settings.FTPServer;
			FServer.Location = new Point(0,24);
			FServer.Width = 512;

			Folder.Text = Settings.LocalDirectory;//TODO Support relative directories?
			Folder.Location = new Point(0,48);
			Folder.Width = 768;

			ConfigFile.Text = System.IO.Path.Combine(new string[]{Environment.CurrentDirectory, Settings.CONFIG_FILE});
			ConfigFile.Location = new Point(0,72);
			ConfigFile.Width = 768;

			StartSync.Text = "Start Scan and Sync";
			StartSync.Location = new Point(256,96);
			StartSync.Width = 256;
			StartSync.Height = 32;
			StartSync.MouseClick += this.RunSyncAndScan;


			Console.Multiline =true;
			Console.Text += "Setup completed\n";
			Console.Location = new Point(0,136);
			Console.Width = 768;
			Console.Height =128;

			DeleteEnabled.Text = "DELETE:"+Settings.RemoveLocalFileIfNoRemoteFile;





			this.Controls.Add(HServer);
			this.Controls.Add(FServer);
			this.Controls.Add(Folder);
			this.Controls.Add(ConfigFile);
			this.Controls.Add(StartSync);
			this.Controls.Add(Console);
			this.Controls.Add (Progress);

			this.ResumeLayout();
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Scan and Sync";
		}


		public void RunSyncAndScan (Object o, MouseEventArgs args)
		{
			if (!StartSync.Enabled) {
				return;
			}
			StartSync.Enabled = false;



			//get the server hashlist
			this.Console.Text += "Getting server hashes\n";
			Dictionary<string,string> serverhashes = Http.GetHashList ();


			//hash all teh local files.
			this.Console.Text += "Checking Local hashes\n";
			SyncList LocalData = new SyncList (Settings.LocalDirectory);

			List<string> FilesToDownload = new List<string> ();

			//get the delta list
			foreach(KeyValuePair<string,string> kvp in serverhashes){
			
				//ignore metadata.
				if (kvp.Key == "__Server" || kvp.Key == "__DateGeneratedUTC") {
					;
				} else {
					//remove files that dont exist server side if set to
					if (! LocalData.HashList.ContainsKey (kvp.Key) || kvp.Value != LocalData.HashList [kvp.Key].Hash) {
						FilesToDownload.Add (kvp.Key);
						LocalData.HashList.Remove (kvp.Key);
					}
				}

			}

			//Remove Files if set to
			if(Settings.RemoveLocalFileIfNoRemoteFile){
				this.Console.Text += "Generating list of local files to remove\n";
				bool shouldDelete = true;
				//ensure we didnt delte everything accidentally.
				if (LocalData.HashList.Count > Settings.numFilesToRemoveWithNoWarning) {
					shouldDelete = false;
					this.Console.Text += (LocalData.HashList.Count + " Files Are flagged for deletion. this is greate than the limit of "+Settings.numFilesToRemoveWithNoWarning+" they willl not be removed\n");
				}


				//remove files
				if(shouldDelete){
					foreach(KeyValuePair<string,SyncItem> kvp in LocalData.HashList){
						File.Delete(Settings.LocalDirectory+kvp.Key);
					}
				}
			}


			this.Console.Text += ("Need to download: " + FilesToDownload.Count + " files from " +Settings.FTPServer+Environment.NewLine);
			Progress.Maximum = FilesToDownload.Count;
			int progress = 0;
			//dowlonad htem from ftp
		
			foreach (string fileName in FilesToDownload) {
				progress += Ftp.DownloadFile (fileName);
				Progress.Value =progress;

			}

			Console.Text +=  ("Downloaded "+progress+"/"+ FilesToDownload.Count +" File(s)"+Environment.NewLine);

			if (progress != FilesToDownload.Count) {
				Console.Text +=  ("WARNING: NOT ALL FILES WERE SUCCESSFULLY DOWNLOADED"+Environment.NewLine+Environment.NewLine);
			}

			StartSync.Enabled = true;
		}
	}
}

