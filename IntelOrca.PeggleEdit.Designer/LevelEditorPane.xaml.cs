using IntelOrca.PeggleEdit.Designer.Editor;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IntelOrca.PeggleEdit.Designer
{
	/// <summary>
	/// Interaction logic for LevelEditorPane.xaml
	/// </summary>
	internal partial class LevelEditorPane : UserControl
	{
		private EditorContext mEditor;

		private bool mCentreScroll;

		public LevelEditorPane()
		{
			InitializeComponent();
		}

		private void SetEditorEvents()
		{
			mEditor.OnLevelChanged += mEditor_OnLevelChanged;
		}

		private void mEditor_OnLevelChanged(object sender, EventArgs e)
		{
			RefreshBackgroundLayer();
			RefreshObjectLayer();
		}

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			if (mCentreScroll) {
				mCentreScroll = false;
				mScrollViewer.ScrollToVerticalOffset(mScrollViewer.ScrollableWidth / 2.0);
				mScrollViewer.ScrollToHorizontalOffset(mScrollViewer.ScrollableHeight / 2.0);
			}
		}

		private void RefreshBackgroundLayer()
		{
			mLayerBackground.Source = mEditor.GetBitmapImage(mEditor.LevelPakRecord.FileName);
		}

		private void RefreshObjectLayer()
		{
			mLayerObjects.Children.Clear();
			if (mEditor.Level == null)
				return;

			foreach (LevelEntry le in mEditor.Level.Entries) {
				EditorObject eo = EditorObjectFactory.FromLevelEntry(mEditor, le);
				Canvas.SetLeft(eo, le.DrawLocation.X - (eo.Width / 2.0));
				Canvas.SetTop(eo, le.DrawLocation.Y - (eo.Height / 2.0));
				mLayerObjects.Children.Add(eo);
			}

			mCentreScroll = true;
		}

		private Point mSelectionStart;

		private void mCanvasBorder_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mSelectionStart = e.GetPosition(mLayerSelection);
			Canvas.SetLeft(mSelectionRectangle, mSelectionStart.X);
			Canvas.SetTop(mSelectionRectangle, mSelectionStart.Y);
			mSelectionRectangle.Visibility = Visibility.Visible;
		}

		private void mCanvasBorder_MouseMove(object sender, MouseEventArgs e)
		{
			Point selectionEnd = e.GetPosition(mLayerSelection);
			if (mSelectionRectangle.Visibility != Visibility.Hidden) {
				if (e.LeftButton == MouseButtonState.Released) {
					mSelectionRectangle.Visibility = Visibility.Hidden;
					mSelectionRectangle.Width = mSelectionRectangle.Height = 0;
				} else {
					Rect rect = new Rect(mSelectionStart, selectionEnd);
					Canvas.SetLeft(mSelectionRectangle, rect.X);
					Canvas.SetTop(mSelectionRectangle, rect.Y);
					mSelectionRectangle.Width = rect.Width;
					mSelectionRectangle.Height = rect.Height;


					foreach (EditorObject eo in mLayerObjects.Children) {
						if (rect.IntersectsWith(eo.Bounds))
							eo.Selected = true;
						else
							eo.Selected = false;
					}
				}
			}

			mLayerSelection.Children.RemoveRange(1, mLayerSelection.Children.Count - 1);
			foreach (EditorObject eo in mLayerObjects.Children) {
				if (eo.Selected) {
					Canvas canvas = eo.GetSelectionObject();
					Canvas.SetLeft(canvas, eo.LevelEntry.DrawLocation.X - (canvas.Width / 2.0));
					Canvas.SetTop(canvas, eo.LevelEntry.DrawLocation.Y - (canvas.Height / 2.0));
					mLayerSelection.Children.Add(canvas);
				}
			}
		}

		private void mCanvasBorder_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mSelectionRectangle.Visibility = Visibility.Hidden;
			mSelectionRectangle.Width = mSelectionRectangle.Height = 0;
		}

		public EditorContext Editor
		{
			get { return mEditor; }
			set {
				mEditor = value;
				SetEditorEvents();
			}
		}
	}
}
