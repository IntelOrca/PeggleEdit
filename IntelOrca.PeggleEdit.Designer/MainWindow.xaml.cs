using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IntelOrca.PeggleEdit.Designer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	internal partial class MainWindow : Window
	{
		private readonly EditorContext mEditor = new EditorContext();

		public MainWindow()
		{
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			mPackExplorer.Editor = mEditor;
			mLevelEditorPane.Editor = mEditor;

			mEditor.PakCollection = new PegglePakCollection(@"C:\Program Files (x86)\PopCap Games\Peggle Nights\main.pak");
			mEditor.LoadLevel(mEditor.PakCollection.GetRecord("levels\\cinder4.dat"));
		}

		public void PostStatusUpdate(string status)
		{
			mStatusbar.SetStatus(status);
		}
	}
}
