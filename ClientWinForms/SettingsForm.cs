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
using System.IO;

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

			System.Reflection.FieldInfo[] props = typeof(Shared.Settings).GetFields();

				foreach(System.Reflection.FieldInfo prop in props){
					if(prop.Name != "CONFIG_FILE" &&prop.Name != "HEADER" && prop.Name!= "HaveReadSettingsFile"){
						settingsdata.Add(prop.Name, prop.GetValue(null).ToString());
					}
				}

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
			//set settings using settings data

			Settings.FTPServer = settingsdata["FTPServer"];
			Settings.HashServer = settingsdata["HashServer"];
			Settings.RemoteUser = settingsdata["RemoteUser"];
			Settings.RemotePassword = settingsdata["RemotePassword"];
			Settings.RemoveLocalFileIfNoRemoteFile = settingsdata["RemoveLocalFiles"].ToLower() =="true";
			Settings.LocalDirectory = settingsdata["LocalDirectory"];

			int.TryParse(settingsdata["LoopTime"],out Settings.LoopTime);
			int.TryParse(settingsdata["DeleteWarningLevel"],out Settings.numFilesToRemoveWithNoWarning);



			Settings.WriteConfigFile();
		}
	}
}


