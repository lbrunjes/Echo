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
		SettingsForm SettingsForm =null;
		Button SettingsButton = new Button();
		Button StartSync = new Button();
		TextBox Console = new TextBox();
		ProgressBar Progress = new ProgressBar();

		public MainForm ()
		{
			this.SuspendLayout();
			this.Width =512;
			this.Font = new Font("monospaced", 12);

			Settings.ReadConfigFile(Settings.CONFIG_FILE_CLIENT);

			SettingsForm = new SettingsForm();

			SettingsButton.Text = "Settings";
			SettingsButton.Location = new Point(382,32);
			SettingsButton.Width = 128;
			SettingsButton.Height = 32;
			SettingsButton.MouseClick += this.ShowSettings;


			StartSync.Text = "Start Scan and Sync";
			StartSync.Location = new Point(0,32);
			StartSync.Width = 256;
			StartSync.Height = 32;
			StartSync.MouseClick += this.RunSyncAndScan;


			Console.Multiline =true;
			Console.Text += "Read Settings file" +Environment.NewLine;
			Console.Location = new Point(0,64);
			Console.Width = 768;
			Console.Height =256;
			Console.ScrollBars = ScrollBars.Both;



			Progress.Location = new Point(0,8);
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
				this.Console.Text += "cannot sync now";
				return;
			}

			StartSync.Enabled = false;
			Dictionary<string,SyncItem> serverhashes;


			//get the server hashlist
			this.Console.Text += "Getting server hashes ("+Settings.HashServer+")" +Environment.NewLine;
			try {
				serverhashes = Http.GetHashList ();

			} catch (Exception ex) {
				this.Console.Text += "Error Reading Remote Hashes:"+ex.Message+Environment.NewLine;
				StartSync.Enabled = true;
				return;
			}

			//hash all teh local files.
			this.Console.Text += "Checking Local hashes" +Environment.NewLine;
			SyncList LocalData = new SyncList (Settings.LocalDirectory);

			List<string> FilesToDownload = new List<string> ();

			//get the delta list
			foreach(KeyValuePair<string,SyncItem> kvp in serverhashes){
			
				//ignore metadata.
				if (kvp.Key == "__Server" || kvp.Key == "__DateGeneratedUTC") {
					;
				} else {
					//remove files that dont exist server side if set to
					if (! LocalData.HashList.ContainsKey (kvp.Key) || kvp.Value.Hash != LocalData.HashList [kvp.Key].Hash) {
						FilesToDownload.Add (kvp.Key);
						LocalData.HashList.Remove (kvp.Key);
					}
					else{
						if(LocalData.HashList.ContainsKey (kvp.Key)){
							LocalData.HashList.Remove (kvp.Key);
						}
					}
				}

			}

			//Remove Files if set to
			if(Settings.RemoveLocalFileIfNoRemoteFile){
				this.Console.Text += "Generating list of local files to remove" +Environment.NewLine;
				bool shouldDelete = true;
				//ensure we didnt delte everything accidentally.
				if (LocalData.HashList.Count > Settings.numFilesToRemoveWithNoWarning) {
					shouldDelete = false;
					if(MessageBox.Show (LocalData.HashList.Count + " Files Are flagged for deletion."+Environment.NewLine+" Delete them?","Continue?",MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes){
						shouldDelete =true;
							this.Console.Text += "Deleting Files" +Environment.NewLine;
					}
					else{
							this.Console.Text += "Files Will NOT be Removed" +Environment.NewLine;
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


			this.Console.Text += ("Need to download: " + FilesToDownload.Count + " files from " +Settings.DownloadType+Environment.NewLine);
			Progress.Maximum = FilesToDownload.Count;
			Progress.Value =0;
			int progress = 0;

			switch(Settings.DownloadType){
				//dowlonad htem from ftp
			case  Settings.DownloadTypes.FTP:
				foreach (string fileName in FilesToDownload) {
					try{
					progress += Ftp.DownloadFile (fileName);

					Progress.Value =progress;
						Progress.Value ++;
					}
					catch(Exception ex){
						this.Console.Text += String.Format("Error Getting file {1}: {0} ",
						                                   ex.Message, fileName);

					}
				}
				break;
				case Settings.DownloadTypes.HTTP:

				foreach (string fileName in FilesToDownload) {
					try{
						progress += Http.DownloadFile (fileName);
					}
					catch(Exception ex){
						Console.Text +=  (String.Format("Couldn't download file:{1}{0}",Environment.NewLine, ex.Message, ex.StackTrace));
					}
				}

				break;
			case Settings.DownloadTypes.S3:
				AmazonS3 s3 = new AmazonS3();
				//try to test atuh so we dont hang and time out after forever.
			 	if(s3.testAuth()){
					foreach (string fileName in FilesToDownload) {
						try{
							progress += AmazonS3.DownloadFile(fileName);
						}
						catch(Exception ex){
							Console.Text +=  (String.Format("Couldn't download file:{1}{0}",Environment.NewLine, ex.Message, ex.StackTrace));
						}

					}	
				}
				else{
					Console.Text += "Could Not authenticate. Downloads skipped"+Environment.NewLine;
				}
				break;
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

