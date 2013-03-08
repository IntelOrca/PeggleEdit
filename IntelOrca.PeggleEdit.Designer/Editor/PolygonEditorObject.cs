using IntelOrca.PeggleEdit.Tools.Levels.Children;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace IntelOrca.PeggleEdit.Designer.Editor
{
	class PolygonEditorObject : EditorObject
	{
		public PolygonEditorObject(EditorContext editor, LevelEntry le)
			: base(editor, le)
		{
		}

		public override void RefreshContent()
		{
			Polygon polygon = LevelEntry as Polygon;

			Children.Clear();

			if (!Editor.DisplayOptions.ShowCollision || polygon.Collision) {
				// Get the polygon image
				BitmapSource polygonImage = null;
				if (polygon.ImageFilename != null)
					polygonImage = Editor.GetBitmapImage(String.Format("images\\levels\\{0}", polygon.ImageFilename));

				if (polygonImage == null || Editor.DisplayOptions.ShowCollision) {
					if (!Editor.DisplayOptions.ShowPreview) {

					}
				} else {
					// Image
					Image image = new Image();
					Canvas.SetLeft(image, polygon.ImageDX);
					Canvas.SetTop(image, polygon.ImageDY);
					image.Width = polygonImage.PixelWidth;
					image.Height = polygonImage.PixelHeight;
					image.Source = polygonImage;
					Children.Add(image);

					Width = Height = Math.Max(polygonImage.PixelWidth, polygonImage.PixelHeight);
				}
			}
		}
	}
}
