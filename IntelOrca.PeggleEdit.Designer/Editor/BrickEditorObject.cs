using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IntelOrca.PeggleEdit.Designer.Editor
{
	class BrickEditorObject : EditorObject
	{
		public BrickEditorObject(EditorContext editor, LevelEntry le)
			: base(editor, le)
		{
		}

		public override void RefreshContent()
		{
			Brick brick = LevelEntry as Brick;
			TransformGroup transformGroup = new TransformGroup();

			//Show only if its collidable
			if (brick.HasPegInfo || (Editor.DisplayOptions.ShowCollision && brick.Collision)) {
				Rect bounds = new Rect();

				double rotation = brick.Rotation;
				if (brick.HasMovementInfo)
					rotation = brick.MovementInfo.GetEstimatedMoveAngle(brick.Rotation);

				//Calculate the brick destination rectangle
				float height = brick.GetHeight();
				if (brick.Curved) {

					// Method to add a polygon to the content control
					Action<Point, double, Color> AddCurvedBrick = (loc, width, c) => {
						// Small offset
						loc.X -= 10;
						double outerRadius = brick.InnerRadius + width;						
						Point circleCentre = new Point(loc.X - brick.InnerRadius, loc.Y);
						double div_angle = brick.SectorAngle / (brick.CurvePoints - 1);
						double cur_angle = -(brick.SectorAngle / 2.0f);

						// Function to calculate a point along a circle
						Func<double, Point> GetBrickAngularPoint = (radius) => {
							return new Point(Math.Cos(MathExt.ToRadians(cur_angle)) * radius + circleCentre.X,
								Math.Sin(MathExt.ToRadians(cur_angle)) * radius + circleCentre.Y);
						};

						// Calculate the polygon points
						Point[] o_pnts = new Point[brick.CurvePoints];
						Point[] i_pnts = new Point[brick.CurvePoints];
						for (int i = 0; i < brick.CurvePoints; i++) {
							o_pnts[i] = GetBrickAngularPoint(outerRadius);
							i_pnts[i] = GetBrickAngularPoint(brick.InnerRadius);
							cur_angle += div_angle;
						}

						// Merge points together
						Point[] pnts = new Point[o_pnts.Length + i_pnts.Length];
						Array.Copy(o_pnts, 0, pnts, 0, o_pnts.Length);

						//Inner points need to be reversed
						for (int i = 0; i < i_pnts.Length; i++)
							pnts[pnts.Length - i - 1] = i_pnts[i];

						// Increase bounds to fit all the points
						Array.ForEach(pnts, p => bounds.Union(p));

						// Create the polygon
						var polygon = new System.Windows.Shapes.Polygon();
						Canvas.SetLeft(polygon, brick.Width / 2);
						Canvas.SetTop(polygon, brick.Width / 2);
						polygon.Stroke = null;
						polygon.Fill = new SolidColorBrush(c);
						polygon.Points = new PointCollection(pnts);
						Children.Add(polygon);
					};

					if (Editor.DisplayOptions.ShowCollision) {
						AddCurvedBrick(new Point(0, 0), brick.Width, Colors.White);
					} else {
						float shadingOffset = 8.0f;
						if (brick.TextureFlip)
							shadingOffset = 2.0f;

						AddCurvedBrick(new Point(), brick.Width, OuterPegColour);
						AddCurvedBrick(new Point(shadingOffset, 0), brick.Width / 2.0, InnerPegColour);
					}

					transformGroup.Children.Add(new RotateTransform(-rotation, brick.Width / 2, brick.Width / 2));

					Width = Height = brick.Width;
				} else {
					// Method to add a rectangle to the content control
					Action<Rect, Color> AddStraightBrick = (r, c) => {
						Rectangle rect = new Rectangle();
						rect.Stroke = null;
						rect.Fill = new SolidColorBrush(c);
						rect.Margin = new Thickness(r.X, r.Y, 0, 0);
						rect.Width = r.Width;
						rect.Height = r.Height;
						Children.Add(rect);
					};

					Rect dest = new Rect(-brick.Length / 2.0, -brick.Width / 2.0, brick.Length, brick.Width);
					AddStraightBrick(dest, OuterPegColour);
					dest.Inflate(-2, -5);
					dest.Y += (brick.TextureFlip ? 3.0 : -3.0);
					AddStraightBrick(dest, InnerPegColour);

					transformGroup.Children.Add(new RotateTransform(-rotation + 90.0));
				}
			}

			RenderTransform = transformGroup;

			Width = 50;
			Height = 50;
		}
	}
}
