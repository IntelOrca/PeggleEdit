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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Designer.Forms
{
    /// <summary>
    /// Sets a level's background from an image: kept at its own size, or cut to the size of the screen or of the opening
    /// in the frame, which it covers or fits in, or is at its own size in; and saved as a PNG or a JPG. A preview shows it
    /// behind the frame, as the game will, where it can be moved and zoomed by hand.
    /// </summary>
    sealed class SetBackgroundForm : Form
    {
        private enum BackgroundSize
        {
            Keep,
            Screen,
            Peggle,
        }

        // How the image goes in its size, or placed by hand
        private enum BackgroundFit
        {
            Cover,
            Contain,
            Actual,
            Custom,
        }

        // The largest side of the copy of the image drawn while it is being changed, which is quicker than the image
        private const int ProxyLimit = 1600;
        private const float MinZoomedSide = 16;
        private const float MaxZoomedSide = 16000;
        // How much + and - zoom by
        private const double KeyZoom = 1.02;
        private const string Title = "Import Background";

        private readonly Level mLevel;
        private readonly string mBaseName;
        private readonly byte[] mSourceData;
        private readonly Bitmap mSource;
        private readonly Bitmap mProxy;
        private readonly bool mSourceJpeg;
        private readonly bool mSourcePng;
        // Whether the file can be saved as it is, for an image kept at its size in the format it is in
        private readonly bool mSourceKeepable;

        private BackgroundSize mSize;
        private BackgroundFit mFit;
        // Where the image is on the screen
        private RectangleF mPlacement;
        // The size and format chosen when the window was opened, which Reset goes back to
        private BackgroundSize mInitialSize;
        private bool mInitialJpg;
        private bool mSettingOptions;

        // The background as it will be saved
        private byte[] mResultData;
        private Bitmap mResultImage;
        private Rectangle mResultBounds;
        // Whether the result is of the image where it is now (else the image is drawn where it is), and whether it is
        // to be made again
        private bool mResultPlaced;
        private bool mResultPending = true;
        private string mResultError;
        private readonly Timer mResultTimer = new Timer { Interval = 150 };

        private Bitmap mOverObjects;
        private Bitmap mOverFrame;
        private Bitmap mScreenBuffer;

        private bool mDragging;
        private Point mDragStart;
        private RectangleF mDragPlacement;

        private readonly PreviewBox mPreview;
        private readonly RadioButton mKeepSize;
        private readonly RadioButton mScreenSize;
        private readonly RadioButton mPeggleSize;
        private readonly GroupBox mFitGroup;
        private readonly RadioButton mCover;
        private readonly RadioButton mContain;
        private readonly RadioButton mActual;
        private readonly Button mReset;
        private readonly RadioButton mPng;
        private readonly RadioButton mJpg;
        private readonly Label mQualityLabel;
        private readonly NumericUpDown mQuality;
        private readonly Label mSavedAs;
        private readonly PictureBox mWarningIcon;
        private readonly Label mWarning;
        private readonly CheckBox mShowObjects;

        /// <summary>
        /// Gets the background chosen, once the window is closed with OK.
        /// </summary>
        public PakImage Background { get; private set; }

        private SetBackgroundForm(Level level, string path, byte[] data, Bitmap source, ImageFormat format, bool keepable)
        {
            mLevel = level;
            mBaseName = Path.GetFileNameWithoutExtension(path);
            mSourceData = data;
            mSource = source;
            mSourceJpeg = format.Equals(ImageFormat.Jpeg);
            mSourcePng = format.Equals(ImageFormat.Png);
            mSourceKeepable = keepable;
            mProxy = MakeProxy(source);
            mResultTimer.Tick += (s, e) => MakeResult();

            SuspendLayout();
            Text = Title;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;

            // Laid out in panels, which size each control to its text in the window's font
            int margin = Dpi.Scale(12);
            int gap = Dpi.Scale(6);
            int columnWidth = Dpi.Scale(290);
            var buttonSize = new Size(Dpi.Scale(75), Math.Max(Dpi.Scale(23), Font.Height + Dpi.Scale(10)));

            // The preview is as big as fits on the screen, up to a size that shows the screen at its own size
            var workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
            var minPreview = new Size(Dpi.Scale(320), Dpi.Scale(240));
            var previewWidth = Math.Min(Dpi.Scale(560), workingArea.Width - columnWidth - (margin * 3) - Dpi.Scale(40));
            // Room for the title bar, the margins and the buttons under it
            previewWidth = Math.Min(previewWidth, (workingArea.Height - Dpi.Scale(130)) * 4 / 3);
            previewWidth = Math.Max(minPreview.Width, previewWidth);
            var previewHeight = previewWidth * 3 / 4;
            mPreview = new PreviewBox
            {
                Dock = DockStyle.Fill,
                Size = new Size(previewWidth, previewHeight),
                MinimumSize = minPreview,
                Margin = Padding.Empty,
                Cursor = Cursors.SizeAll,
            };
            mPreview.Paint += OnPreviewPaint;
            mPreview.MouseDown += OnPreviewMouseDown;
            mPreview.MouseMove += OnPreviewMouseMove;
            mPreview.MouseUp += OnPreviewMouseUp;
            mPreview.MouseCaptureChanged += (s, e) => mDragging &= mPreview.Capture;
            mPreview.MouseWheel += OnPreviewMouseWheel;
            mPreview.KeyDown += OnPreviewKeyDown;
            mPreview.GotFocus += (s, e) => mPreview.Invalidate();
            mPreview.LostFocus += (s, e) => mPreview.Invalidate();

            var hint = new Label
            {
                Text = "Use mouse to pan && zoom image",
                AutoSize = true,
                Anchor = AnchorStyles.Top,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(0, gap, 0, 0),
            };

            // Size, which is what is cut from the screen and saved
            mScreenSize = NewRadio("&800 × 600, the whole screen");
            mPeggleSize = NewRadio("&646 × 543, just inside the frame");
            mKeepSize = NewRadio($"&Keep its size, {source.Width} × {source.Height}");
            var sizeGroup = NewGroup("Size", columnWidth, mScreenSize, mPeggleSize, mKeepSize);

            // How the image goes in its size
            mCover = NewRadio("Cov&er");
            mContain = NewRadio("Co&ntain");
            mActual = NewRadio("&100%");
            mCover.Margin = mContain.Margin = new Padding(0, 0, Dpi.Scale(16), 0);
            mActual.Margin = Padding.Empty;
            mFitGroup = NewGroup("Fit", columnWidth, NewRow(mCover, mContain, mActual));

            // Format
            mPng = NewRadio("&PNG, which keeps every pixel");
            mJpg = NewRadio("&JPG, which is smaller but loses detail");
            mQualityLabel = new Label { Text = "&Quality:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, gap, 0) };
            mQuality = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100,
                Value = LevelBackground.DefaultJpegQuality,
                Width = Dpi.Scale(60),
                Margin = Padding.Empty,
            };
            var quality = NewRow(mQualityLabel, mQuality);
            quality.Margin = new Padding(Dpi.Scale(18), 0, 0, gap);
            var textWidth = columnWidth - Dpi.Scale(20);
            mSavedAs = new Label
            {
                AutoSize = true,
                // Room for two lines, so that the window does not change as it does
                MinimumSize = new Size(textWidth, (Font.Height * 2) + Dpi.Scale(2)),
                MaximumSize = new Size(textWidth, 0),
                Margin = new Padding(0, gap, 0, 0),
            };
            var formatGroup = NewGroup("Format", columnWidth, mPng, mJpg, quality, mSavedAs);

            mShowObjects = new CheckBox
            {
                Text = "Show the level's &objects",
                Checked = true,
                AutoSize = true,
                Margin = new Padding(0, gap, 0, gap),
            };
            mShowObjects.CheckedChanged += (s, e) => mPreview.Invalidate();

            var iconSize = Dpi.Scale(16);
            mWarningIcon = new PictureBox
            {
                Image = SystemIcons.Warning.ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(iconSize, iconSize),
                Margin = new Padding(0, Dpi.Scale(1), gap, 0),
            };
            mWarning = new Label
            {
                Text = "Black will show around it in the game, where it does not reach the frame.",
                AutoSize = true,
                MaximumSize = new Size(columnWidth - iconSize - gap, 0),
                Margin = Padding.Empty,
            };
            var warning = NewRow(mWarningIcon, mWarning);

            var side = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(margin, 0, 0, 0),
            };
            side.Controls.AddRange(new Control[] { sizeGroup, mFitGroup, formatGroup, mShowObjects, warning });

            // OK closes the window itself, once the background is made
            var ok = new Button { Text = "OK", Size = buttonSize, UseVisualStyleBackColor = true, Margin = new Padding(gap, 0, 0, 0), TabIndex = 0 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = buttonSize, UseVisualStyleBackColor = true, Margin = new Padding(gap, 0, 0, 0), TabIndex = 2 };
            ok.Click += (s, e) => Accept();
            mReset = new Button { Text = "&Reset", Size = buttonSize, UseVisualStyleBackColor = true, Margin = new Padding(gap, 0, 0, 0), TabIndex = 1 };
            mReset.Click += (s, e) => ResetSettings();
            // OK, Reset, Cancel, from the left
            var buttons = NewRow(cancel, mReset, ok);
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttons.Margin = new Padding(margin, gap, 0, 0);

            // The preview, which takes what room the window has, with the hint centred under it, and the options and
            // buttons beside them
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(margin),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(mPreview, 0, 0);
            table.Controls.Add(hint, 0, 1);
            table.Controls.Add(side, 1, 0);
            table.Controls.Add(buttons, 1, 1);
            Controls.Add(table);

            AcceptButton = ok;
            CancelButton = cancel;
            // Big enough for the preview at the size it was given and for the options, whichever is taller
            var sideSize = side.GetPreferredSize(Size.Empty);
            var bottom = Math.Max(hint.PreferredHeight, buttonSize.Height) + gap;
            ClientSize = new Size(
                (margin * 3) + previewWidth + sideSize.Width,
                (margin * 2) + Math.Max(previewHeight, sideSize.Height) + bottom);
            // As small as the preview's smallest, with the options and buttons still shown
            var chrome = Size - ClientSize;
            MinimumSize = new Size(
                (margin * 3) + minPreview.Width + sideSize.Width,
                (margin * 2) + Math.Max(minPreview.Height, sideSize.Height) + bottom) + chrome;
            ActiveControl = mPreview;

            // An image the size of the screen or of Peggle's backgrounds is kept as it is; any other covers the screen
            var made = source.Size == LevelBackground.ScreenSize || source.Size == LevelBackground.PeggleBounds.Size;
            mInitialSize = made ? BackgroundSize.Keep : BackgroundSize.Screen;
            SetOptions(mInitialSize, BackgroundFit.Cover);
            // A photo stays a JPG; anything else, which may have sharp edges or few colours, is kept exact as a PNG
            mInitialJpg = mSourceJpeg;
            (mInitialJpg ? mJpg : mPng).Checked = true;

            mKeepSize.CheckedChanged += (s, e) => OnSizeChecked(mKeepSize, BackgroundSize.Keep);
            mScreenSize.CheckedChanged += (s, e) => OnSizeChecked(mScreenSize, BackgroundSize.Screen);
            mPeggleSize.CheckedChanged += (s, e) => OnSizeChecked(mPeggleSize, BackgroundSize.Peggle);
            mCover.CheckedChanged += (s, e) => OnFitChecked(mCover, BackgroundFit.Cover);
            mContain.CheckedChanged += (s, e) => OnFitChecked(mContain, BackgroundFit.Contain);
            mActual.CheckedChanged += (s, e) => OnFitChecked(mActual, BackgroundFit.Actual);
            mPng.CheckedChanged += (s, e) => { if (mPng.Checked) Changed(false); };
            mJpg.CheckedChanged += (s, e) => { if (mJpg.Checked) Changed(false); };
            mQuality.ValueChanged += (s, e) => Changed(false);
            ResumeLayout(false);
            Changed(true);
        }

        private static RadioButton NewRadio(string text) => new RadioButton
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, Dpi.Scale(4)),
        };

        /// <summary>
        /// Makes a group of controls one above the other, at least the given width.
        /// </summary>
        private static GroupBox NewGroup(string text, int width, params Control[] controls)
        {
            var padding = Dpi.Scale(8);
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
            };
            panel.Controls.AddRange(controls);
            var group = new GroupBox
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(width, 0),
                Padding = new Padding(padding, padding / 2, padding, padding),
                Margin = new Padding(0, 0, 0, Dpi.Scale(6)),
            };
            group.Controls.Add(panel);
            return group;
        }

        /// <summary>
        /// Makes a row of controls side by side.
        /// </summary>
        private static FlowLayoutPanel NewRow(params Control[] controls)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty,
            };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Bitmap MakeProxy(Bitmap source)
        {
            var scale = Math.Min(1f, (float)ProxyLimit / Math.Max(source.Width, source.Height));
            var size = new Size(Math.Max(1, (int)Math.Round(source.Width * scale)), Math.Max(1, (int)Math.Round(source.Height * scale)));
            var proxy = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(proxy))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(source, new Rectangle(Point.Empty, size), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel);
            }
            return proxy;
        }

        #region Size and place

        /// <summary>
        /// Gets where on the screen the saved background goes, and so its size.
        /// </summary>
        private Rectangle OutputBounds
        {
            get
            {
                switch (mSize)
                {
                    case BackgroundSize.Keep:
                        return LevelBackground.GetBounds(mSource.Size);
                    case BackgroundSize.Peggle:
                        return LevelBackground.PeggleBounds;
                    default:
                        return new Rectangle(Point.Empty, LevelBackground.ScreenSize);
                }
            }
        }

        /// <summary>
        /// Gets whether the image's own file is saved, unchanged: kept at its size, in the format it is in.
        /// </summary>
        private bool KeepsFile => mSize == BackgroundSize.Keep && mSourceKeepable && (mJpg.Checked ? mSourceJpeg : mSourcePng);

        /// <summary>
        /// Sets the size and how the image goes in it, which puts it there, unless it is placed by hand, when it stays
        /// where it is. A kept image is where the game puts it, at its own size.
        /// </summary>
        private void SetOptions(BackgroundSize size, BackgroundFit fit)
        {
            mSize = size;
            mFit = fit;
            var bounds = OutputBounds;
            if (size == BackgroundSize.Keep)
                mPlacement = bounds;
            else if (fit == BackgroundFit.Cover)
                mPlacement = LevelBackground.Fill(mSource.Size, bounds);
            else if (fit == BackgroundFit.Contain)
                mPlacement = LevelBackground.Contain(mSource.Size, bounds);
            else if (fit == BackgroundFit.Actual)
                // Where the game would show it at its own size, cut to the size
                mPlacement = LevelBackground.GetBounds(mSource.Size);

            mSettingOptions = true;
            mScreenSize.Checked = size == BackgroundSize.Screen;
            mPeggleSize.Checked = size == BackgroundSize.Peggle;
            mKeepSize.Checked = size == BackgroundSize.Keep;
            mCover.Checked = fit == BackgroundFit.Cover;
            mContain.Checked = fit == BackgroundFit.Contain;
            mActual.Checked = fit == BackgroundFit.Actual;
            mSettingOptions = false;
            // Checking a radio button takes the others out of the tab order, so with none checked one is put back
            if (fit == BackgroundFit.Custom)
                mCover.TabStop = true;
            // A kept image is not scaled
            mFitGroup.Enabled = size != BackgroundSize.Keep;
        }

        private void OnSizeChecked(RadioButton radio, BackgroundSize size)
        {
            if (mSettingOptions || !radio.Checked)
                return;
            // A kept image is not placed by hand, so it is not where it was placed once it is cut to a size again
            SetOptions(size, size == BackgroundSize.Keep && mFit == BackgroundFit.Custom ? BackgroundFit.Cover : mFit);
            Changed(true);
        }

        private void OnFitChecked(RadioButton radio, BackgroundFit fit)
        {
            if (mSettingOptions || !radio.Checked)
                return;
            SetOptions(mSize, fit);
            Changed(true);
        }

        /// <summary>
        /// Starts placing the image by hand, from where it is, in the size chosen; a kept image is then 800 x 600.
        /// </summary>
        private void BeginPlacing()
        {
            if (mFit == BackgroundFit.Custom && mSize != BackgroundSize.Keep)
                return;
            SetOptions(mSize == BackgroundSize.Keep ? BackgroundSize.Screen : mSize, BackgroundFit.Custom);
        }

        /// <summary>
        /// Goes back to the settings the window opened with, and the image to where they put it.
        /// </summary>
        private void ResetSettings()
        {
            SetOptions(mInitialSize, BackgroundFit.Cover);
            (mInitialJpg ? mJpg : mPng).Checked = true;
            mQuality.Value = LevelBackground.DefaultJpegQuality;
            mShowObjects.Checked = true;
            Changed(true);
            mPreview.Focus();
        }

        private void MoveBy(float dx, float dy)
        {
            BeginPlacing();
            mPlacement.Offset(dx, dy);
            SnapToPixels();
            Changed(true);
        }

        /// <summary>
        /// Puts an image that is at its own size, or as near as zooming back to it leaves it, on whole pixels, so that
        /// it is sharp.
        /// </summary>
        private void SnapToPixels()
        {
            const float tolerance = 0.01f;
            if (Math.Abs(mPlacement.Width - mSource.Width) < tolerance && Math.Abs(mPlacement.Height - mSource.Height) < tolerance)
            {
                mPlacement = new RectangleF(
                    (float)Math.Round(mPlacement.X), (float)Math.Round(mPlacement.Y), mSource.Width, mSource.Height);
            }
        }

        /// <summary>
        /// Zooms the image about a point on the screen, which stays where it is.
        /// </summary>
        private void Zoom(double factor, PointF about)
        {
            var width = mPlacement.Width * factor;
            var largest = Math.Max(mPlacement.Width, mPlacement.Height) * factor;
            var smallest = Math.Min(mPlacement.Width, mPlacement.Height) * factor;
            if ((factor > 1 && largest > MaxZoomedSide) || (factor < 1 && smallest < MinZoomedSide))
                return;
            BeginPlacing();
            var f = (float)factor;
            mPlacement = new RectangleF(
                about.X - ((about.X - mPlacement.X) * f), about.Y - ((about.Y - mPlacement.Y) * f),
                (float)width, mPlacement.Height * f);
            SnapToPixels();
            Changed(true);
        }

        #endregion

        #region The saved background

        /// <summary>
        /// Notes that what is saved has changed, to be made again shortly.
        /// </summary>
        private void Changed(bool placementChanged)
        {
            if (placementChanged)
                mResultPlaced = false;
            mResultPending = true;
            mResultTimer.Stop();
            mResultTimer.Start();
            UpdateInfo();
            mPreview.Invalidate();
        }

        private void MakeResult()
        {
            mResultTimer.Stop();
            if (!mResultPending)
                return;
            mResultPending = false;
            mResultError = null;
            try
            {
                var bounds = OutputBounds;
                byte[] data;
                if (KeepsFile)
                {
                    data = mSourceData;
                }
                else if (mSize == BackgroundSize.Keep)
                {
                    data = LevelBackground.Encode(mSource, mJpg.Checked, (int)mQuality.Value);
                }
                else
                {
                    var placement = mPlacement;
                    placement.Offset(-bounds.X, -bounds.Y);
                    using (var image = LevelBackground.Draw(mSource, bounds.Size, placement))
                        data = LevelBackground.Encode(image, mJpg.Checked, (int)mQuality.Value);
                }

                // Shown as it will be read back, so that what a JPG loses is seen
                Bitmap decoded;
                using (var ms = new MemoryStream(data))
                using (var image = Image.FromStream(ms))
                {
                    decoded = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                    using (var g = Graphics.FromImage(decoded))
                        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                }
                mResultImage?.Dispose();
                mResultImage = decoded;
                mResultData = data;
                mResultBounds = bounds;
                mResultPlaced = true;
            }
            catch (Exception ex) when (ex is ExternalException || ex is ArgumentException || ex is OutOfMemoryException ||
                ex is InvalidOperationException)
            {
                mResultData = null;
                mResultError = ex.Message;
            }
            UpdateInfo();
            mPreview.Invalidate();
        }

        private void UpdateInfo()
        {
            var bounds = OutputBounds;
            var format = mJpg.Checked ? "JPG" : "PNG";
            var keeps = KeepsFile;
            string text;
            if (mResultError != null)
                text = "It can not be saved: " + mResultError;
            else
            {
                text = keeps ? $"The file is kept as it is: {format}, {bounds.Width} × {bounds.Height}" :
                    $"Saved as {format}, {bounds.Width} × {bounds.Height}";
                if (!mResultPending && mResultData != null)
                    text += ", " + FormatSize(mResultData.Length);
            }
            mSavedAs.Text = text;
            mQuality.Enabled = mQualityLabel.Enabled = mJpg.Checked && !keeps;

            // What is saved is only what is in its bounds
            var shown = RectangleF.Intersect(mPlacement, bounds);
            var covers = LevelBackground.CoversFrameOpening(shown);
            mWarning.Visible = mWarningIcon.Visible = !covers;
        }

        private static string FormatSize(int bytes)
        {
            if (bytes < 1024 * 1024)
                return $"{Math.Max(1, (bytes + 512) / 1024)} KB";
            return $"{bytes / (1024.0 * 1024.0):0.0} MB";
        }

        private void Accept()
        {
            // A quality typed and not yet taken, as when Enter is pressed straight after, is taken now
            _ = mQuality.Value;
            if (mResultPending)
                MakeResult();
            if (mResultData == null)
            {
                MessageBox.Show(this, mResultError ?? "The background could not be made.", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var extension = mJpg.Checked ? ".jpg" : ".png";
            Background = new PakImage(mBaseName + extension, mResultData);
            DialogResult = DialogResult.OK;
        }

        #endregion

        #region Preview

        /// <summary>
        /// Gets where the screen is drawn in the preview, as big as fits.
        /// </summary>
        private RectangleF ScreenRect
        {
            get
            {
                var client = mPreview.ClientRectangle;
                var screen = LevelBackground.ScreenSize;
                var scale = Math.Min((float)client.Width / screen.Width, (float)client.Height / screen.Height);
                var width = screen.Width * scale;
                var height = screen.Height * scale;
                return new RectangleF((client.Width - width) / 2, (client.Height - height) / 2, width, height);
            }
        }

        private float PreviewScale => ScreenRect.Width / LevelBackground.ScreenSize.Width;

        private PointF ToScreen(Point point)
        {
            var rect = ScreenRect;
            var scale = PreviewScale;
            return new PointF((point.X - rect.X) / scale, (point.Y - rect.Y) / scale);
        }

        private Bitmap GetOverlay()
        {
            if (mShowObjects.Checked)
                return mOverObjects ?? (mOverObjects = mLevel.RenderOverBackground(true));
            return mOverFrame ?? (mOverFrame = mLevel.RenderOverBackground(false));
        }

        /// <summary>
        /// Draws the screen as the game will show it, at its own size: the background, and the frame and the level's
        /// objects over it.
        /// </summary>
        private Bitmap DrawScreen()
        {
            var size = LevelBackground.ScreenSize;
            if (mScreenBuffer == null)
                mScreenBuffer = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(mScreenBuffer))
            {
                g.Clear(Color.Black);
                if (mResultPlaced && mResultImage != null)
                {
                    g.DrawImage(mResultImage, mResultBounds, 0, 0, mResultImage.Width, mResultImage.Height, GraphicsUnit.Pixel);
                }
                else
                {
                    // Until it is made, the image is drawn where it is, cut to what is saved. At its own size it is copied
                    // pixel for pixel, as it will be saved; bigger than the copy of it, it is drawn from the image itself,
                    // so that it is as sharp as it will be
                    g.SetClip(OutputBounds);
                    if (LevelBackground.IsUnscaled(mSource.Size, mPlacement))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(mSource, Rectangle.Round(mPlacement), 0, 0, mSource.Width, mSource.Height, GraphicsUnit.Pixel);
                    }
                    else
                    {
                        // The image itself only when it is not made much smaller, which, as it is drawn quickly while
                        // it is dragged, would make it shimmer and be slow
                        var image = mPlacement.Width > mProxy.Width && mPlacement.Width >= mSource.Width / 2f ? mSource : mProxy;
                        g.InterpolationMode = mDragging ? InterpolationMode.Bilinear : InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        using (var attributes = new ImageAttributes())
                        {
                            attributes.SetWrapMode(WrapMode.TileFlipXY);
                            var points = new[]
                            {
                                mPlacement.Location,
                                new PointF(mPlacement.Right, mPlacement.Top),
                                new PointF(mPlacement.Left, mPlacement.Bottom),
                            };
                            g.DrawImage(image, points, new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel, attributes);
                        }
                    }
                    g.ResetClip();
                }
                g.DrawImage(GetOverlay(), 0, 0, size.Width, size.Height);
            }
            return mScreenBuffer;
        }

        private void OnPreviewPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(SystemColors.AppWorkspace);
            var rect = ScreenRect;
            var screen = DrawScreen();
            // Made smaller smoothly; made bigger by whole pixels, so that each can be seen
            g.InterpolationMode = PreviewScale >= 2 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(screen, rect, new RectangleF(0, 0, screen.Width, screen.Height), GraphicsUnit.Pixel);
            if (mPreview.Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(g, mPreview.ClientRectangle);
        }

        private void OnPreviewMouseDown(object sender, MouseEventArgs e)
        {
            mPreview.Focus();
            if (e.Button != MouseButtons.Left)
                return;
            mDragging = true;
            mDragStart = e.Location;
            mDragPlacement = mPlacement;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!mDragging || e.Location == mDragStart)
                return;
            var scale = PreviewScale;
            // By whole pixels, from where it was, so that an image at its own size stays sharp
            var dx = (float)Math.Round(mDragPlacement.X + ((e.X - mDragStart.X) / scale)) - mDragPlacement.X;
            var dy = (float)Math.Round(mDragPlacement.Y + ((e.Y - mDragStart.Y) / scale)) - mDragPlacement.Y;
            // A jiggle of the mouse too small to move it leaves the settings as they are
            if (Math.Abs(dx) < 0.5f && Math.Abs(dy) < 0.5f && mFit != BackgroundFit.Custom)
                return;
            BeginPlacing();
            mPlacement = mDragPlacement;
            mPlacement.Offset(dx, dy);
            Changed(true);
            // Drawn as it moves, as the mouse moving would otherwise hold it up
            mPreview.Update();
        }

        private void OnPreviewMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mDragging = false;
        }

        private void OnPreviewMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0)
                return;
            Zoom(Math.Pow(1.1, e.Delta / 120.0), ToScreen(e.Location));
            mPreview.Update();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var step = e.Shift ? 10 : 1;
            // About the middle of the opening in the frame
            var middle = new PointF(
                LevelBackground.FrameOpening.X + (LevelBackground.FrameOpening.Width / 2f),
                LevelBackground.FrameOpening.Y + (LevelBackground.FrameOpening.Height / 2f));
            switch (e.KeyCode)
            {
                case Keys.Left:
                    MoveBy(-step, 0);
                    break;
                case Keys.Right:
                    MoveBy(step, 0);
                    break;
                case Keys.Up:
                    MoveBy(0, -step);
                    break;
                case Keys.Down:
                    MoveBy(0, step);
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    Zoom(KeyZoom, middle);
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    Zoom(1 / KeyZoom, middle);
                    break;
                default:
                    return;
            }
            e.Handled = true;
        }

        /// <summary>
        /// The preview, which takes the arrow keys to move the image.
        /// </summary>
        private sealed class PreviewBox : Control
        {
            public PreviewBox()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
                TabStop = true;
            }

            protected override bool IsInputKey(Keys keyData)
            {
                switch (keyData & ~Keys.Shift)
                {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                        return true;
                }
                return base.IsInputKey(keyData);
            }

            // Otherwise a letter or number typed with it focused picks the option it is the access key of, as if Alt
            // were held
            protected override bool IsInputChar(char charCode) => true;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mResultTimer.Dispose();
                mSource.Dispose();
                mProxy.Dispose();
                mResultImage?.Dispose();
                mOverObjects?.Dispose();
                mOverFrame?.Dispose();
                mScreenBuffer?.Dispose();
                mWarningIcon.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Asks how to set a level's background from an image file.
        /// </summary>
        /// <returns>The background, or null if the window was cancelled or the file could not be read.</returns>
        public static PakImage Show(IWin32Window owner, Level level, string path)
        {
            byte[] data;
            Bitmap source;
            ImageFormat format;
            bool keepable;
            try
            {
                data = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(data))
                using (var image = Image.FromStream(ms))
                {
                    format = image.RawFormat;
                    keepable = IsKeepable(image);
                    // A copy, as the image needs its stream for as long as it is used
                    source = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                    using (var g = Graphics.FromImage(source))
                        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException ||
                ex is OutOfMemoryException || ex is ExternalException)
            {
                // Image.FromStream throws ArgumentException for a file that is not an image it can read, and the copy
                // OutOfMemoryException for one too big
                string message;
                if (ex is ArgumentException || ex is ExternalException)
                    message = "The file is not an image that can be read.";
                else if (ex is OutOfMemoryException)
                    message = "The image is too big to be read.";
                else
                    message = ex.Message;
                MessageBox.Show(owner, message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            SetBackgroundForm form;
            try
            {
                form = new SetBackgroundForm(level, path, data, source, format, keepable);
            }
            catch
            {
                source.Dispose();
                throw;
            }
            using (form)
                return form.ShowDialog(owner) == DialogResult.OK ? form.Background : null;
        }

        /// <summary>
        /// Gets whether an image's file is of a kind the game reads as it is: a PNG or a JPG in red, green and blue of 8
        /// bits each, rather than e.g. a CMYK JPG or a PNG of 16 bits a colour, which are saved again as one that is.
        /// </summary>
        private static bool IsKeepable(Image image)
        {
            if (image.RawFormat.Equals(ImageFormat.Jpeg))
                return (image.Flags & (int)(ImageFlags.ColorSpaceCmyk | ImageFlags.ColorSpaceYcck)) == 0;
            if (image.RawFormat.Equals(ImageFormat.Png))
            {
                switch (image.PixelFormat)
                {
                    case PixelFormat.Format16bppGrayScale:
                    case PixelFormat.Format48bppRgb:
                    case PixelFormat.Format64bppArgb:
                    case PixelFormat.Format64bppPArgb:
                        return false;
                }
                return true;
            }
            return false;
        }
    }
}
