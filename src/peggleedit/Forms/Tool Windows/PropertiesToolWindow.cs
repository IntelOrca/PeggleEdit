﻿// This file is part of PeggleEdit.
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000002")]
    class PropertiesToolWindow : Form
    {
        MainMDIForm mParent;
        PropertyGrid mPropertyGrid;

        public PropertiesToolWindow(MainMDIForm parent)
        {
            mParent = parent;

            this.Icon = Icon.FromHandle(Resources.properties_16.GetHicon());

            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Text = "Properties";

            mPropertyGrid = new PropertyGrid();
            mPropertyGrid.Dock = DockStyle.Fill;

            mPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(mPropertyGrid_PropertyValueChanged);

            Controls.Add(mPropertyGrid);
        }

        void mPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (mPropertyGrid.SelectedObjects.Length == 0)
                return;

            mParent.GetFocusedLevelEditor().UpdateRedraw();
        }

        public void UpdatePropertyGrid(LevelEntry[] objects)
        {
            mPropertyGrid.SelectedObjects = objects;
            mPropertyGrid.ExpandAllGridItems();
        }
    }
}
