using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IntelOrca.PeggleEdit.Designer.Editor
{
	class CircleEditorObject : EditorObject
	{
		public CircleEditorObject(EditorContext editor, LevelEntry le)
			: base(editor, le)
		{
		}

		public override void RefreshContent()
		{
			Circle circle = LevelEntry as Circle;

			Children.Clear();

			// Get the circle image
			BitmapSource circleImage = null;
			if (circle.ImageFilename != null)
				circleImage = Editor.GetBitmapImage(String.Format("images\\levels\\{0}", circle.ImageFilename));

			if (Editor.DisplayOptions.ShowCollision || (!circle.HasPegInfo && circleImage == null)) {
				if (!Editor.DisplayOptions.ShowPreview) {
					// White collision circle
					Ellipse ellipse = new Ellipse();
					ellipse.Width = ellipse.Height = circle.Radius * 2;
					ellipse.Fill = new SolidColorBrush(OuterPegColour);
					Children.Add(ellipse);

					Width = Height = circle.Radius * 2;
				}
			} else {
				if (circle.HasPegInfo) {
					// Outer circle
					Ellipse outerEllipse = new Ellipse();
					outerEllipse.Width = circle.Radius * 2;
					outerEllipse.Height = circle.Radius * 2;
					outerEllipse.Fill = new SolidColorBrush(OuterPegColour);
					Children.Add(outerEllipse);
					
					// Inner circle
					Ellipse innerEllipse = new Ellipse();
					Canvas.SetLeft(innerEllipse, 2);
					Canvas.SetTop(innerEllipse, 2);
					innerEllipse.Width = circle.Radius * 2 - 4;
					innerEllipse.Height = circle.Radius * 2 - 4;
					innerEllipse.Fill = new SolidColorBrush(InnerPegColour);
					Children.Add(innerEllipse);

					Width = Height = circle.Radius * 2;
				} else {
					// Image
					Image image = new Image();
					image.Width = circleImage.PixelWidth;
					image.Height = circleImage.PixelHeight;
					image.Source = circleImage;
					Children.Add(image);

					Width = Height = Math.Max(circleImage.PixelWidth, circleImage.PixelHeight);
				}
			}
		}
	}
}
