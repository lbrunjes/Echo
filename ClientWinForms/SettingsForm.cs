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
using System.ComponentModel;
using Shared;
using System.IO;

namespace ClientWinForms
{
	public class SettingsForm:Form
	{
        private class SettingPair
        {
            public SettingPair(String setKey, String setValue)
            {
                key = setKey;
                val = setValue;
            }
            public String key = "";
            public String val = "";

            public string Key
            {
                get { return key; }
                set { key = value; }
            }

            public string Value
            {
                get { return val; }
                set { val = value; }
            }
        }

        private String GetSetting(BindingList<SettingPair> settingList, String settingKey)
        {
            foreach (SettingPair setting in settingList)
            {
                if (setting.key == settingKey)
                {
                    return setting.val;
                }
            }
            return null;
        }

        BindingList<SettingPair> settingsdata = new BindingList<SettingPair>();
		DataGridView SettingsList = new DataGridView();

		Button SaveButton = new Button();

		public SettingsForm ()
		{
			this.SuspendLayout();
			this.Width=530;
			this.Height=344;

			System.Reflection.FieldInfo[] props = typeof(Shared.Settings).GetFields();

				foreach(System.Reflection.FieldInfo prop in props){
			//	Console.WriteLine(prop.Name);
					if(prop.Name == "HashServer" || 
					   	prop.Name == "LocalDirectory" || 
						prop.Name == "s3IDKey" || 
						prop.Name == "s3SecretKey" || 
						prop.Name == "s3Bucket"){					   
						settingsdata.Add(new SettingPair(prop.Name, prop.GetValue(null).ToString()));
					}
				}

			SettingsList.DataSource = new BindingSource(settingsdata, null);
            
			SettingsList.Width =512;
			SettingsList.Height=256;

					this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.StartPosition = FormStartPosition.CenterScreen;
		
			SettingsList.RowHeadersVisible = false;
            SettingsList.ReadOnly = false;
            SettingsList.Enabled = true;
            SettingsList.EditMode = DataGridViewEditMode.EditOnEnter;
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
			this.Text = "Echo - Settings";

		}

		public void SaveSettings (Object o, MouseEventArgs e)
		{

				//set settings using settings data

				//	Settings.FTPServer = GetSetting(settingsdata, "FTPServer");
				Settings.HashServer = GetSetting (settingsdata, "HashServer");
				//    Settings.RemoteUser = GetSetting(settingsdata, "RemoteUser");
				//    Settings.RemotePassword = GetSetting(settingsdata, "RemotePassword");
		//		Settings.RemoveLocalFileIfNoRemoteFile = GetSetting (settingsdata, "RemoveLocalFileIfNoRemoteFile").ToLower () == "true";
				Settings.LocalDirectory = GetSetting (settingsdata, "LocalDirectory");
				Settings.HashServer = GetSetting (settingsdata, "HashServer");
				Settings.LocalDirectory = GetSetting (settingsdata, "LocalDirectory");
				Settings.s3IDKey = GetSetting (settingsdata, "s3IDKey");
				Settings.s3SecretKey = GetSetting (settingsdata, "s3SecretKey");
				Settings.s3Bucket = GetSetting (settingsdata, "s3Bucket");
		//		int.TryParse (GetSetting (settingsdata, "LoopTime"), out Settings.LoopTime);
		//		int.TryParse (GetSetting (settingsdata, "DeleteWarningLevel"), out Settings.numFilesToRemoveWithNoWarning);

				Settings.WriteConfigFile ();
		
		}
	}
}


