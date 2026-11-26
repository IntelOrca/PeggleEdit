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

namespace IntelOrca.PeggleEdit.Designer.Forms
{
    /// <summary>
    /// Asks whether to save the changes to the pack before it is closed: Save, Don't Save or Cancel.
    /// </summary>
    sealed class SaveChangesForm : Form
    {
        private const int MaxLevelsListed = 5;

        private SaveChangesForm(string packName, IReadOnlyList<string> changedLevels)
        {
            SuspendLayout();
            Text = Program.AppTitle;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;

            var message = $"Do you want to save the changes to '{packName}'?";
            if (changedLevels.Count > 0)
            {
                var names = changedLevels.Take(MaxLevelsListed).ToList();
                if (changedLevels.Count > MaxLevelsListed)
                    names.Add($"{changedLevels.Count - MaxLevelsListed} more");
                message += Environment.NewLine + Environment.NewLine +
                    (changedLevels.Count == 1 ? "Changed level: " : "Changed levels: ") + String.Join(", ", names);
            }

            // Laid out by hand, which Mono does as Windows does
            int margin = Dpi.Scale(12);
            int gap = Dpi.Scale(7);
            var label = new Label
            {
                AutoSize = false,
                Text = message,
                Location = new Point(margin, margin),
            };
            var textSize = TextRenderer.MeasureText(message, Font, new Size(Dpi.Scale(380), 0), TextFormatFlags.WordBreak);
            label.Size = new Size(textSize.Width + Dpi.Scale(4), textSize.Height + Dpi.Scale(2));

            var buttons = new[]
            {
                NewButton("&Save", DialogResult.Yes),
                NewButton("Do&n't Save", DialogResult.No),
                NewButton("Cancel", DialogResult.Cancel),
            };
            var buttonSize = new Size(
                Math.Max(Dpi.Scale(75), buttons.Max(x => TextRenderer.MeasureText(x.Text, Font).Width) + Dpi.Scale(16)),
                Math.Max(Dpi.Scale(23), Font.Height + Dpi.Scale(10)));
            var buttonsWidth = (buttons.Length * buttonSize.Width) + ((buttons.Length - 1) * gap);

            var width = Math.Max(label.Width, buttonsWidth);
            var buttonTop = label.Bottom + margin + Dpi.Scale(4);
            var buttonLeft = margin + width - buttonsWidth;
            foreach (var button in buttons)
            {
                button.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), buttonSize);
                buttonLeft += buttonSize.Width + gap;
            }

            Controls.Add(label);
            Controls.AddRange(buttons);
            AcceptButton = buttons[0];
            CancelButton = buttons[2];
            ClientSize = new Size(width + (margin * 2), buttonTop + buttonSize.Height + margin);
            ResumeLayout(false);
        }

        private static Button NewButton(string text, DialogResult result) => new Button
        {
            Text = text,
            DialogResult = result,
            UseVisualStyleBackColor = true,
        };

        /// <summary>
        /// Asks whether to save the changes to the pack.
        /// </summary>
        /// <returns><see cref="DialogResult.Yes"/> to save, <see cref="DialogResult.No"/> not to, or
        /// <see cref="DialogResult.Cancel"/> to keep the pack open.</returns>
        public static DialogResult Show(IWin32Window owner, string packName, IReadOnlyList<string> changedLevels)
        {
            using (var form = new SaveChangesForm(packName, changedLevels))
            {
                var result = form.ShowDialog(owner);
                return result == DialogResult.Yes || result == DialogResult.No ? result : DialogResult.Cancel;
            }
        }
    }
}
