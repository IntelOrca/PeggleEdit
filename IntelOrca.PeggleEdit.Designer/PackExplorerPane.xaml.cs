using IntelOrca.PeggleEdit.Tools.Pack;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace IntelOrca.PeggleEdit.Designer
{
	/// <summary>
	/// Interaction logic for PackExplorer.xaml
	/// </summary>
	internal partial class PackExplorer : UserControl
	{
		private EditorContext mEditor;

		public PackExplorer()
		{
			InitializeComponent();
		}

		private void SetEditorEvents()
		{
			mEditor.OnPakCollectionChanged += mEditor_OnPakCollectionChanged;
		}

		void mEditor_OnPakCollectionChanged(object sender, EventArgs e)
		{
			PopulateTree();
		}

		private void PopulateTree()
		{
			mTreeView.Items.Clear();

			TreeViewItem head = new TreeViewItem();
			head.Header = System.IO.Path.GetFileNameWithoutExtension(mEditor.PakCollection.Filename);
			head.Tag = "\\";

			mTreeView.Items.Add(head);
			foreach (PakRecord record in mEditor.PakCollection.Records)
				AddTreeItem(record);

			head.IsExpanded = true;
		}

		private void AddTreeItem(PakRecord record)
		{
			TreeViewItem currentNode = mTreeView.Items[0] as TreeViewItem;
			string filename = record.FileName;
			int slashIndex;

			while ((slashIndex = filename.IndexOf('\\')) != -1) {
				string directory = filename.Substring(0, slashIndex);
				filename = filename.Remove(0, slashIndex + 1);

				var directoryItem = currentNode.Items.Cast<object>().SingleOrDefault(i => {
					var di = i as PackExplorerTreeViewItemDirectory;
					if (di == null)
						return false;
					return (String.Compare(di.DirectoryName, directory, true) == 0);
				}) as PackExplorerTreeViewItemDirectory;

				if (directoryItem == null) {
					directoryItem = new PackExplorerTreeViewItemDirectory(this, directory);
					currentNode.Items.Add(directoryItem);
				}

				currentNode = directoryItem;
			}

			currentNode.Items.Add(new PackExplorerTreeViewItemFile(this, record));
		}

		public EditorContext Editor
		{
			get { return mEditor; }
			set {
				mEditor = value;
				SetEditorEvents();
			}
		}

		class PackExplorerTreeViewItem : TreeViewItem
		{
			PackExplorer mPackExplorer;

			public PackExplorerTreeViewItem(PackExplorer packExplorer)
			{
				mPackExplorer = packExplorer;
			}

			public PackExplorer PackExplorer
			{
				get { return mPackExplorer; }
			}
		}

		class PackExplorerTreeViewItemDirectory : PackExplorerTreeViewItem
		{
			private string mDirectoryName;

			public PackExplorerTreeViewItemDirectory(PackExplorer packExplorer, string name)
				: base(packExplorer)
			{
				mDirectoryName = name;
				Header = mDirectoryName;
			}

			public string DirectoryName
			{
				get { return mDirectoryName; }
			}
		}

		class PackExplorerTreeViewItemFile : PackExplorerTreeViewItem
		{
			private PakRecord mRecord;

			public PackExplorerTreeViewItemFile(PackExplorer packExplorer, PakRecord record)
				: base(packExplorer)
			{
				mRecord = record;
				Header = System.IO.Path.GetFileName(mRecord.FileName);
			}

			protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
			{
				base.OnMouseDoubleClick(e);
				PackExplorer.Editor.LoadLevel(mRecord);
			}

			public PakRecord Record
			{
				get { return mRecord; }
			}
		}
	}
}
