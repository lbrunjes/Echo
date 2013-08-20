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
		SettingsForm SettingsForm = new SettingsForm();
		Button SettingsButton = new Button();
		Button StartSync = new Button();
		TextBox Console = new TextBox();
		ProgressBar Progress = new ProgressBar();

		public MainForm ()
		{
			this.SuspendLayout();
			this.Width =512;
			this.Font = new Font("monospaced", 12);

			Settings.ReadConfigFile();

			SettingsButton.Text = "Settings";
			SettingsButton.Location = new Point(192,196);
			SettingsButton.Width = 128;
			SettingsButton.Height = 32;
			SettingsButton.MouseClick += this.ShowSettings;


			StartSync.Text = "Start Scan and Sync";
			StartSync.Location = new Point(128,32);
			StartSync.Width = 256;
			StartSync.Height = 32;
			StartSync.MouseClick += this.RunSyncAndScan;


			Console.Multiline =true;
			Console.Text += "Read Settings file\n";
			Console.Location = new Point(0,64);
			Console.Width = 768;
			Console.Height =128;



			Progress.Location = new Point(0,96);
			Progress.Width =512;
			Progress.Height= 16;

			this.Controls.Add(SettingsButton);
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
			Dictionary<string,string> serverhashes;


			//get the server hashlist
			this.Console.Text += "Getting server hashes\n";
			try {
				serverhashes = Http.GetHashList ();

			} catch (Exception ex) {
				this.Console.Text += "Error Reading Remote Hashes:"+ex.Message+Environment.NewLine;
				return;
			}

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
					if(MessageBox.Show (LocalData.HashList.Count + " Files Are flagged for deletion. Continue?") == System.Windows.Forms.DialogResult.OK){
						shouldDelete =true;
					}


				}


				//remove files
				if(shouldDelete){
					Console.Text+="Deleting files..."+Environment.NewLine;
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

		public void ShowSettings(object a, MouseEventArgs e){
			SettingsForm.ShowDialog();

		}
	}
}

