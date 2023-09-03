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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Represents a form for editing the collision points in a polygon.
    /// </summary>
    public partial class PolygonEditor : Form
    {
        Image mImage;
        List<PointF> mPoints = new List<PointF>();

        public PolygonEditor(Image img, PointF[] points)
        {
            InitializeComponent();
            mImage = img;
            mPoints = points.ToList();
            RefreshPoints();
        }

        private void pnlPointEditor_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pnlPointEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (mPoints.Count > 0)
                {
                    mPoints.RemoveAt(mPoints.Count - 1);
                    RefreshPoints();
                }

                return;
            }


            if (IsComplete())
                return;

            //Check if its near first point
            if (mPoints.Count > 1)
            {
                var p = mPoints[0];
                var area = new RectangleF(p.X - 2, p.Y - 2, 4, 4);
                if (area.Contains(e.X, e.Y))
                {
                    mPoints.Add(p);
                    RefreshPoints();
                    return;
                }
            }

            mPoints.Add(new Point(e.X, e.Y));

            RefreshPoints();
        }

        private void pnlPointEditor_Paint(object sender, PaintEventArgs e)
        {
            var canvasSize = pnlPointEditor.Size;
            e.Graphics.TranslateTransform(canvasSize.Width / 2, canvasSize.Height / 2);

            if (mImage != null)
                e.Graphics.DrawImage(mImage, new Rectangle(0, 0, mImage.Width, mImage.Height), new Rectangle(0, 0, mImage.Width, mImage.Height), GraphicsUnit.Pixel);

            if (IsComplete())
            {
                e.Graphics.FillPolygon(Brushes.White, mPoints.ToArray());
                e.Graphics.DrawPolygon(Pens.Black, mPoints.ToArray());
            }
            else
            {
                for (int i = 1; i < mPoints.Count; i++)
                {
                    var a = mPoints[i - 1];
                    var b = mPoints[i];

                    e.Graphics.DrawLine(Pens.White, a, b);
                }

                foreach (var p in mPoints)
                {
                    var rect = new RectangleF(p.X - 2, p.Y - 2, 4, 4);
                    e.Graphics.FillRectangle(Brushes.White, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        private void pnlPointEditor_SizeChanged(object sender, EventArgs e)
        {
            pnlPointEditor.Invalidate();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            mPoints.Clear();

            RefreshPoints();
        }

        private void btnCentre_Click(object sender, EventArgs e)
        {
            if (mPoints.Count < 1)
                return;

            var minX = mPoints.Min(p => p.X);
            var minY = mPoints.Min(p => p.Y);
            var maxX = mPoints.Max(p => p.X);
            var maxY = mPoints.Max(p => p.Y);
            var currentOrigin = new PointF(
                minX + ((maxX - minX) / 2),
                minY + ((maxY - minY) / 2));

            for (var i = 0; i < mPoints.Count; i++)
            {
                mPoints[i] = mPoints[i].Subtract(currentOrigin);
            }
            RefreshPoints();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            if (!IsComplete())
            {
                MessageBox.Show("There is not a complete polygon yet.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void RefreshPoints()
        {
            lstPoints.Items.Clear();
            foreach (var p in mPoints)
            {
                lstPoints.Items.Add(p.ToString());
            }

            RefreshSelectedPoint();

            pnlPointEditor.Invalidate();
        }

        private bool IsComplete()
        {
            if (mPoints.Count > 1)
            {
                if (mPoints[0] == mPoints[mPoints.Count - 1])
                    return true;
            }

            return false;
        }

        public PointF[] GetFinalPoints()
        {
            return mPoints.Select(p => (PointF)p).ToArray();
        }

        public void PointsFromPolygon(PointF[] pnts)
        {
            Point centreOfImage = new Point(mImage.Width / 2, mImage.Height / 2);

            mPoints.Clear();
            foreach (PointF p in pnts)
            {
                mPoints.Add(new Point((int)p.X + centreOfImage.X, (int)p.Y + centreOfImage.Y));
            }

            RefreshPoints();
        }

        private void RefreshSelectedPoint()
        {
            if (disableTxtEvent)
                return;

            if (lstPoints.SelectedIndex == -1)
            {
                txtX.Enabled = false;
                txtY.Enabled = false;
                txtX.Text = String.Empty;
                txtY.Text = String.Empty;
                return;
            }

            var pnt = mPoints[lstPoints.SelectedIndex];

            txtX.Enabled = true;
            txtY.Enabled = true;
            txtX.Text = pnt.X.ToString();
            txtY.Text = pnt.Y.ToString();
        }

        private void lstPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSelectedPoint();
        }

        bool disableTxtEvent = false;
        private void txtX_TextChanged(object sender, EventArgs e)
        {
            if (disableTxtEvent)
                return;

            disableTxtEvent = true;

            if (int.TryParse(txtX.Text, out var value))
            {
                var pnt = mPoints[lstPoints.SelectedIndex];
                pnt.X = value;
                mPoints[lstPoints.SelectedIndex] = pnt;

                lstPoints.Items[lstPoints.SelectedIndex] = pnt.ToString();

                int selStart = txtX.SelectionStart;
                txtX.Focus();
                txtX.Select(selStart, 0);

                pnlPointEditor.Invalidate();
            }

            disableTxtEvent = false;
        }

        private void txtY_TextChanged(object sender, EventArgs e)
        {
            if (disableTxtEvent)
                return;

            disableTxtEvent = true;

            if (int.TryParse(txtY.Text, out var value))
            {
                var pnt = mPoints[lstPoints.SelectedIndex];
                pnt.Y = value;
                mPoints[lstPoints.SelectedIndex] = pnt;

                lstPoints.Items[lstPoints.SelectedIndex] = pnt.ToString();

                int selStart = txtY.SelectionStart;
                txtY.Focus();
                txtY.Select(selStart, 0);

                pnlPointEditor.Invalidate();
            }

            disableTxtEvent = false;
        }
    }
}
