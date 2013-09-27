//
//  PT SYNC
//  2013 Lee Brunjes
//
//  A one way file sync System. 
//
//
using System;
using System.Windows.Forms;

namespace ClientWinForms
{
	public class EntryPoint
	{
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(true);

			Application.Run (new MainForm());


		}
	}
}

