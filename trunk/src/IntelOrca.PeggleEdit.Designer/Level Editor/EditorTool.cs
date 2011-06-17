// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
	abstract class EditorTool : ICloneable
	{
		private CallbackMethod mFinishCallback;
		private LevelEditor mEditor;

		public void Finish()
		{
			Deactivate();

			if (mFinishCallback != null)
				mFinishCallback.Invoke();
		}

		public virtual void Activate()
		{
		}

		public virtual void Deactivate()
		{
		}

		public virtual void Draw(Graphics g)
		{
		}

		public virtual void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
		{
		}

		public virtual void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
		{
		}

		public virtual void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
		{
		}

		public virtual object Clone()
		{
			return null;
		}

		protected void CloneTo(EditorTool tool)
		{
			tool.mEditor = mEditor;
		}

		public virtual LevelEditor Editor
		{
			get
			{
				return mEditor;
			}
			set
			{
				mEditor = value;
			}
		}

		public CallbackMethod FinishCallback
		{
			get
			{
				return mFinishCallback;
			}
			set
			{
				mFinishCallback = value;
			}
		}
	}
}
