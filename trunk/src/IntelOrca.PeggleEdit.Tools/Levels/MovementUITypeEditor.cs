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
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents the user interface when editing a Movement property.
	/// </summary>
	class MovementUITypeEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value == null) {
				ILevelChild lc = context.Instance as ILevelChild;
				if (lc == null)
					return null;

				Movement movement = new Movement(lc.Level);

				ILocation location = context.Instance as ILocation;
				if (location != null) {
					movement.Location = location.Location;
				}

				return movement;
			} else {
				DialogResult result = MessageBox.Show("Do you want to delete this movement information?", "Delete Movement", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result == DialogResult.OK)
					return null;
				else
					return value;
			}
		}
	}
}
