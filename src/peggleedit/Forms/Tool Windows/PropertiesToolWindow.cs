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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000002")]
    class PropertiesToolWindow : Form
    {
        private MainMDIForm _parent;
        private PropertyGrid _propertyGrid;
        private int _lastScroll;

        public PropertiesToolWindow(MainMDIForm parent)
        {
            _parent = parent;

            this.Icon = Icon.FromHandle(Resources.properties_16.GetHicon());

            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Text = "Properties";

            _propertyGrid = new PropertyGrid();
            _propertyGrid.Dock = DockStyle.Fill;

            _propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(mPropertyGrid_PropertyValueChanged);

            Controls.Add(_propertyGrid);
        }

        void mPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (_propertyGrid.SelectedObjects.Length == 0)
                return;

            _parent.GetFocusedLevelEditor().UpdateRedraw();
        }

        public void UpdatePropertyGrid(LevelEntry[] objects)
        {
            if (_propertyGrid.SelectedObjects.Length != 0)
            {
                _lastScroll = GetScrollPosition();
            }

            _propertyGrid.SelectedObjects = objects;
            _propertyGrid.ExpandAllGridItems();

            SetScrollPosition(_lastScroll);
        }

        private int GetScrollPosition()
        {
            var vScroll = GetScrollBar();
            return vScroll?.Value ?? 0;
        }

        private void SetScrollPosition(int value)
        {
            var vScroll = GetScrollBar();
            vScroll.Value = Math.Min(value, vScroll.Maximum);
        }

        private VScrollBar GetScrollBar()
        {
            var propGrid = _propertyGrid.Controls
                .OfType<Control>()
                .Where(ctl => ctl.AccessibilityObject.Role == AccessibleRole.Table)
                .First();
            var vScroll = propGrid.Controls
                .OfType<VScrollBar>()
                .FirstOrDefault();
            return vScroll;
        }
    }
}
