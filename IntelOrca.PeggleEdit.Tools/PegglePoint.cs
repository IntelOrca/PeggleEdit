using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
	public struct PegglePoint
	{
		public PegglePoint(float x, float y)
			: this()
		{
			X = x; Y = y;
		}

		public static PegglePoint Empty
		{
			get { return new PegglePoint(); }
		}

		public float X { get; set; }
		public float Y { get; set; }

		public override bool Equals(object obj)
		{
			PegglePoint p = (PegglePoint)obj;
			return (X == p.X && Y == p.Y);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("X = {0} Y = {1}", X, Y);
		}

		public static implicit operator PointF(PegglePoint p)
		{
			return new PointF(p.X, p.Y);
		}

		public static implicit operator PegglePoint(PointF p)
		{
			return new PegglePoint(p.X, p.Y);
		}
	}
}
