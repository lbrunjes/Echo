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
using System.ComponentModel;

namespace ClientWinForms
{
	public class MainForm:Form
	{
		SettingsForm SettingsForm = null;
		Button SettingsButton = new Button();
		Button StartSync = new Button();
		TextBox Console = new TextBox();
		ProgressBar Progress = new ProgressBar();
        private BackgroundWorker backgroundWorker = null;

		public MainForm ()
		{
			this.SuspendLayout();
			this.Width = 530;
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
			Progress.Width = 512;
			Progress.Height= 16;

			this.Controls.Add(SettingsButton);
			this.Controls.Add(StartSync);
			this.Controls.Add(Console);
			this.Controls.Add(Progress);

			this.ResumeLayout();
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Scan and Sync";

            InitializeBackgroundWorker();
		}

        // Set up the BackgroundWorker object by attaching event handlers.
        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(ThreadedSyncAndScan);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SyncComplete);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(SyncProgressChanged);
        }

		public void RunSyncAndScan(Object o, MouseEventArgs args)
		{
            if (!StartSync.Enabled)
            {
                this.Console.Text += "cannot sync now";
                return;
            }

            backgroundWorker.RunWorkerAsync();
            StartSync.Enabled = false;
            Progress.Maximum = 100;
            Progress.Value = 0;
        }

        private delegate void StringDelegate(string s);
        private void AddTextToConsole(string text)
        {
            if (this.Console.InvokeRequired)
            {
                StringDelegate sd = new StringDelegate(AddTextToConsole);
                this.Console.Invoke(sd, new object[] { text });
                return;
            }

            this.Console.Text += text;
        }

        private void ThreadedSyncAndScan(object sender, DoWorkEventArgs e)
        {   
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            Dictionary<string, SyncItem> serverhashes;

            //get the server hashlist
            AddTextToConsole("Getting server hashes (" + Settings.HashServer + ")" + Environment.NewLine);
            try
            {
                serverhashes = Http.GetHashList();

            }
            catch (Exception ex)
            {
                AddTextToConsole("Error Reading Remote Hashes:" + ex.Message + Environment.NewLine);
                return;
            }

            //hash all teh local files.
            AddTextToConsole("Checking Local hashes" + Environment.NewLine);
            SyncList LocalData = new SyncList(Settings.LocalDirectory);

            List<string> FilesToDownload = new List<string>();

            //get the delta list
            foreach (KeyValuePair<string, SyncItem> kvp in serverhashes)
            {

                //ignore metadata.
                if (kvp.Key == "__Server" || kvp.Key == "__DateGeneratedUTC")
                {
                    ;
                }
                else
                {
                    //Queue downloads
                    if (!LocalData.HashList.ContainsKey(kvp.Key))
                    {
                        //downlaod files that dont exist.
                        FilesToDownload.Add(kvp.Key);
                    }
                    else
                    {
                        if (kvp.Value.Hash != LocalData.HashList[kvp.Key].Hash)
                        {
                            //download files the dont mathc the hash
                            FilesToDownload.Add(kvp.Key);
                        }
                        else
                        {
                            //if we get here the file exist and is right.
                            //get it out of the hash list so we dont delrte it.
                            bool okay = LocalData.HashList.Remove(kvp.Key);
                        }
                    }
                }

            }

            //Remove Files if set to
            if (Settings.RemoveLocalFileIfNoRemoteFile)
            {
                AddTextToConsole("Generating list of local files to remove" + Environment.NewLine);
                bool shouldDelete = true;
                //ensure we didnt delte everything accidentally.
                if (LocalData.HashList.Count > Settings.numFilesToRemoveWithNoWarning)
                {
                    shouldDelete = false;
                    if (MessageBox.Show(LocalData.HashList.Count + " Files Are flagged for deletion." + Environment.NewLine + " Delete them?", "Continue?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        shouldDelete = true;
                        AddTextToConsole("Deleting Files" + Environment.NewLine);
                    }
                    else
                    {
                        AddTextToConsole("Files Will NOT be Removed" + Environment.NewLine);
                    }


                }


                //remove files
                if (shouldDelete)
                {
                    AddTextToConsole("Deleting files..." + Environment.NewLine);
                    foreach (KeyValuePair<string, SyncItem> kvp in LocalData.HashList)
                    {
                        File.Delete(Settings.LocalDirectory + kvp.Key);
                    }
                }
            }


            AddTextToConsole("Need to download: " + FilesToDownload.Count + " files from " + Settings.DownloadType + Environment.NewLine);
            int progress = 0;

            Boolean authSuccess = true;
            AmazonS3 s3 = null;
            if (Settings.DownloadType == Settings.DownloadTypes.S3)
            {
                s3 = new AmazonS3();
                // Try to test auth so we dont hang and time out after forever.
                if (!s3.testAuth())
                {
                    authSuccess = false;
                    AddTextToConsole("Could not authenticate S3 key. Downloads skipped." + Environment.NewLine);
                }
            }

            if (authSuccess)
            {
                foreach (string fileName in FilesToDownload)
                {
                    switch (Settings.DownloadType)
                    {
                        //dowlonad htem from ftp
                    /*    case Settings.DownloadTypes.FTP:
                            try
                            {
                                progress += Ftp.DownloadFile(fileName);
                            }
                            catch (Exception ex)
                            {
                                AddTextToConsole(String.Format("Error Getting file {1}: {0} ", ex.Message, fileName));

                            }
                            break;

                        case Settings.DownloadTypes.HTTP:
                            try
                            {
                                progress += Http.DownloadFile(fileName);
                            }
                            catch (Exception ex)
                            {
                                AddTextToConsole(String.Format("Couldn't download file:{1}{0}", Environment.NewLine, ex.Message, ex.StackTrace));
                            }
                            break;*/

                        case Settings.DownloadTypes.S3:
                            try
                            {
						     
                                progress += AmazonS3.DownloadFile(fileName);
							}
                            catch (Exception ex)
                            {
                                AddTextToConsole(String.Format("Couldn't download file:{1}{0}", Environment.NewLine, ex.Message, ex.StackTrace));
                            }
                            break;
                    }
                    worker.ReportProgress((int)(((float)progress) / ((float)FilesToDownload.Count) * 100.0f));
                }
            }

            AddTextToConsole("Downloaded " + progress + "/" + FilesToDownload.Count + " File(s)" + Environment.NewLine);

            if (progress != FilesToDownload.Count)
            {
                AddTextToConsole("WARNING: NOT ALL FILES WERE SUCCESSFULLY DOWNLOADED" + Environment.NewLine + Environment.NewLine);
            }
        }

        private void SyncComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation. 
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
            }

            StartSync.Enabled = true;
        }

        private void SyncProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
        }

		public void ShowSettings(object a, MouseEventArgs e)
        {
			SettingsForm.ShowDialog();
		}
	}
}

