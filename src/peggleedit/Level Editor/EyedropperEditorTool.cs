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

using System.Drawing;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    internal class EyedropperEditorTool : EditorTool
    {
        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            var level = Editor.Level;
            var pos = level.GetVirtualXY(location);
            var obj = level.GetObjectAt(pos.X, pos.Y);
            switch (obj)
            {
                case Circle circle when circle.HasPegInfo:
                {
                    var template = new Circle(null);
                    template.PegInfo = (PegInfo)circle.PegInfo.Clone();
                    SetTool(new DrawEditorTool(template, true, 30, 30));
                    break;
                }
                case Brick brick:
                {
                    SetTool(new BrickEditorTool(brick));
                    break;
                }
                case LevelEntry entry:
                {
                    var template = (LevelEntry)entry.Clone();
                    template.MovementLink = null;
                    SetTool(new DrawEditorTool(template, false));
                    break;
                }
            }
        }

        private void SetTool(EditorTool tool)
        {
            MainMDIForm.Instance.SetEditorTool(tool);
        }

        public override object Clone()
        {
            var tool = new EyedropperEditorTool();
            CloneTo(tool);
            return tool;
        }
    }
}
