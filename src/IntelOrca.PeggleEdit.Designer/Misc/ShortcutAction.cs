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
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
	delegate void CallbackMethod();

	class ShortcutAction
	{
		public Keys Key;
		public bool Ctrl;
		public bool EditorMustBeFocused;
		EventHandler fEventHandler;
		MethodInvoker fMethodInvoker;

		public ShortcutAction(Keys k, bool ctrl, EventHandler func) :
			this(k, ctrl, false, func)
		{
		}

		public ShortcutAction(Keys k, bool ctrl, bool editorMustBeFocused, EventHandler func)
		{
			Key = k;
			Ctrl = ctrl;
			EditorMustBeFocused = editorMustBeFocused;
			fEventHandler = func;
		}

		public ShortcutAction(Keys k, bool ctrl, MethodInvoker func) :
			this(k, ctrl, false, func)
		{
		}

		public ShortcutAction(Keys k, bool ctrl, bool editorMustBeFocused, MethodInvoker func)
		{
			Key = k;
			Ctrl = ctrl;
			EditorMustBeFocused = editorMustBeFocused;
			fMethodInvoker = func;
		}

		public void CallMethod()
		{
			if (fEventHandler != null)
				fEventHandler.Invoke(this, EventArgs.Empty);
			else if (fMethodInvoker != null)
				fMethodInvoker.Invoke();
		}
	}
}
