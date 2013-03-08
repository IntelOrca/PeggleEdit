using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
	public struct PeggleRectangle
	{
		public static PeggleRectangle Empty { get { return new PeggleRectangle(); } }

		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public PeggleRectangle(float x, float y, float width, float height)
			: this()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public static PeggleRectangle FromLTRB(float left, float top, float right, float bottom)
		{
			return new PeggleRectangle(left, top, left - right + 1, bottom - top + 1);
		}

		public void Inflate(float x, float y)
		{
			X -= x;
			Y -= y;
			Width += x * 2.0f;
			Height += y * 2.0f;
		}

		public static implicit operator RectangleF(PeggleRectangle rect)
		{
			return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		}
	}
}
