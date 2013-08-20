//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Shared;

namespace ClientWinForms
{
	public class SettingsForm:Form
	{
		Dictionary<String,String> settingsdata = new Dictionary<String,String>();
		DataGridView SettingsList = new DataGridView();

		Button SaveButton =new Button();

		public SettingsForm ()
		{
			this.SuspendLayout();
			this.Width=512;
			this.Height=340;

			settingsdata.Add("HashServer",Settings.HashServer);
			settingsdata.Add("HashFile",Settings.HashFile);
			settingsdata.Add("FTPServer",Settings.FTPServer);
			settingsdata.Add("LocalDirectory",Settings.LocalDirectory);
			settingsdata.Add("LoopTime",Settings.LoopTime.ToString());
			settingsdata.Add("DeleteWarningLevel",Settings.numFilesToRemoveWithNoWarning.ToString());
			settingsdata.Add("RemotePassword",Settings.RemotePassword);
			settingsdata.Add("RemoteUser",Settings.RemoteUser);
			settingsdata.Add("RemoveLocalFiles",Settings.RemoveLocalFileIfNoRemoteFile.ToString());	
			settingsdata.Add("ConfigFile",Settings.CONFIG_FILE);	

			SettingsList.DataSource = new BindingSource(settingsdata,null);
			SettingsList.Width =512;
			SettingsList.Height=256;

		
			SettingsList.RowHeadersVisible =false;
			SettingsList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		

			SaveButton.Text = "Save";
		
			SaveButton.Location = new System.Drawing.Point(128,272);
			SaveButton.Width =256;
			SaveButton.Height =32;
			SaveButton.MouseClick += SaveSettings;

			this.Controls.Add(SettingsList);
			this.Controls.Add (SaveButton);
			this.StartPosition = FormStartPosition.CenterParent;

			this.ResumeLayout();
			this.Text = "Scan and Sync Settings";

		}



		public void SaveSettings (Object o, MouseEventArgs e)
		{
			Console.WriteLine(Settings.HEADER);
			foreach (KeyValuePair<string,string> kvp in settingsdata) {
				Console.WriteLine(kvp.Key+"="+kvp.Value);
			}
		}
	}
}

