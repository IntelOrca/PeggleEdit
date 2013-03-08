using IntelOrca.PeggleEdit.Tools.Levels.Children;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IntelOrca.PeggleEdit.Designer.Editor
{
	class EditorObject : Canvas
	{
		private EditorContext mEditor;
		private LevelEntry mLevelEntry;
		private bool mSelected;

		public EditorObject(EditorContext editor, LevelEntry le)
		{
			mEditor = editor;
			mLevelEntry = le;

			RefreshContent();
		}

		public virtual void RefreshContent()
		{
			
		}

		public virtual Canvas GetSelectionObject()
		{
			Canvas canvas = new Canvas();
			canvas.Width = Width + 10;
			canvas.Height = Height + 10;

			Rectangle rectangle = new Rectangle();
			rectangle.Width = canvas.Width;
			rectangle.Height = canvas.Height;
			rectangle.Stroke = new SolidColorBrush(Colors.White);
			rectangle.StrokeDashArray = new DoubleCollection(new double[] { 1, 1 });
			canvas.Children.Add(rectangle);

			return canvas;
		}

		public EditorContext Editor
		{
			get { return mEditor; }
		}

		public LevelEntry LevelEntry
		{
			get { return mLevelEntry; }
		}

		public bool Selected
		{
			get { return mSelected; }
			set { mSelected = value; }
		}

		public Rect Bounds
		{
			get { return new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), Width, Height); }
		}

		protected Color OuterPegColour
		{
			get {
				if (!mLevelEntry.HasPegInfo)
					return new Color();

				if (mLevelEntry.PegInfo.CanBeOrange && (!Editor.DisplayOptions.ShowPreview)) {
					if (mLevelEntry.PegInfo.QuickDisappear)
						return Color.FromRgb(234, 140, 22);
					else
						return Color.FromRgb(234, 140, 22);
				} else {
					if (mLevelEntry.PegInfo.QuickDisappear && (!Editor.DisplayOptions.ShowPreview))
						return Color.FromRgb(83, 124, 217);
					else
						return Color.FromRgb(83, 124, 217);
				}
			}
		}

		protected Color InnerPegColour
		{
			get {
				if (!mLevelEntry.HasPegInfo)
					return new Color();

				if (mLevelEntry.PegInfo.CanBeOrange && (!Editor.DisplayOptions.ShowPreview)) {
					if (mLevelEntry.PegInfo.QuickDisappear)
						return Color.FromRgb(255, 250, 202);
					else
						return Color.FromRgb(131, 35, 6);
				} else {
					if (mLevelEntry.PegInfo.QuickDisappear && (!Editor.DisplayOptions.ShowPreview))
						return Color.FromRgb(214, 254, 255);
					else
						return Color.FromRgb(13, 50, 167);
				}
			}
		}
	}
}
