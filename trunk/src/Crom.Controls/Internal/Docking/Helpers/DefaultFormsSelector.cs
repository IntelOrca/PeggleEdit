/***************************************************************************
 *   CopyRight (C) 2009 by Cristinel Mazarine                              *
 *   Author:   Cristinel Mazarine                                          *
 *   Contact:  cristinel@osec.ro                                           *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the Crom Free License as published by           *
 *   the SC Crom-Osec SRL; version 1 of the License                        *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   Crom Free License for more details.                                   *
 *                                                                         *
 *   You should have received a copy of the Crom Free License along with   *
 *   this program; if not, write to the contact@osec.ro                    *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Default forms selector
   /// </summary>
   internal class DefaultFormsSelector : FormsSelector
   {
      #region Embedded types

      /// <summary>
      /// Owner draw form
      /// </summary>
      private class OwnerDrawForm : Form
      {
         #region Instance

         /// <summary>
         /// Default constructor
         /// </summary>
         public OwnerDrawForm()
         {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.Manual;

            SetStyle(ControlStyles.AllPaintingInWmPaint,  true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw,          true);
            SetStyle(ControlStyles.Selectable,            false);
            SetStyle(ControlStyles.UserPaint,             true);
         }

         #endregion Instance

         #region Public section

         /// <summary>
         /// Occurs when escape was pressed
         /// </summary>
         public event EventHandler EscapePressed;

         /// <summary>
         /// Occurs when return was pressed 
         /// </summary>
         public event EventHandler ReturnPressed;

         /// <summary>
         /// Occurs when back was pressed 
         /// </summary>
         public event EventHandler BackPressed;

         /// <summary>
         /// Occurs when next was pressed 
         /// </summary>
         public event EventHandler NextPressed;

         #endregion Public section

         #region Protected section

         /// <summary>
         /// Hide form from Alt-Tab list
         /// </summary>
         protected override CreateParams CreateParams
         {
            get
            {
               CreateParams cp = base.CreateParams;
               // turn on WS_EX_TOOLWINDOW style bit
               cp.ExStyle |= 0x80;
               return cp;
            }
         }

         /// <summary>
         /// Occurs when key down
         /// </summary>
         /// <param name="keyData">key data</param>
         protected override bool ProcessDialogKey(Keys keyData)
         {
            if (keyData == Keys.Escape)
            {
               EventHandler handler = EscapePressed;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }

            if (keyData == Keys.Return)
            {
               EventHandler handler = ReturnPressed;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }

            if (keyData == Keys.Right || keyData == Keys.Up)
            {
               EventHandler handler = BackPressed;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }

            if (keyData == Keys.Left || keyData == Keys.Down)
            {
               EventHandler handler = NextPressed;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }

            return base.ProcessDialogKey(keyData);
         }

         #endregion Protected section
      }

      #endregion Embedded types

      #region Fields

      private Size               _previewSize            = new Size();
      private Form               _previousForm           = null;
      private Form               _nextForm               = null;
      private Form               _selectedForm           = null;

      private OwnerDrawForm      _selectBackPreview      = null;
      private OwnerDrawForm      _selectNextPreview      = null;
      private OwnerDrawForm      _selectCurrentPreview   = null;
      private OwnerDrawForm      _selectFloor            = null;
      private OwnerDrawForm      _selectRoof             = null;

      private PreviewRenderer    _selectBackRenderer     = new PreviewRenderer();
      private PreviewRenderer    _selectNextRenderer     = new PreviewRenderer();
      private PreviewRenderer    _selectCurrentRenderer  = new PreviewRenderer();

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public DefaultFormsSelector()
      {
         _selectBackPreview = new OwnerDrawForm();
         _selectBackPreview.Paint         += OnPaintSelectBackPreview;
         _selectBackPreview.EscapePressed += OnEscapePressed;
         _selectBackPreview.ReturnPressed += OnReturnPressed;
         _selectBackPreview.BackPressed   += OnBackPressed;
         _selectBackPreview.NextPressed   += OnNextPressed;
         _selectBackPreview.MouseClick    += OnBackPreviewClick;
         _selectBackPreview.TopMost = true;

         _selectNextPreview = new OwnerDrawForm();
         _selectNextPreview.Paint         += OnPaintSelectNextPreview;
         _selectNextPreview.EscapePressed += OnEscapePressed;
         _selectNextPreview.ReturnPressed += OnReturnPressed;
         _selectNextPreview.BackPressed   += OnBackPressed;
         _selectNextPreview.NextPressed   += OnNextPressed;
         _selectNextPreview.MouseClick    += OnNextPreviewClick;
         _selectNextPreview.TopMost = true;

         _selectCurrentPreview = new OwnerDrawForm();
         _selectCurrentPreview.Paint         += OnPaintSelectCurrentPreview;
         _selectCurrentPreview.EscapePressed += OnEscapePressed;
         _selectCurrentPreview.ReturnPressed += OnReturnPressed;
         _selectCurrentPreview.BackPressed   += OnBackPressed;
         _selectCurrentPreview.NextPressed   += OnNextPressed;
         _selectCurrentPreview.MouseClick    += OnCurrentPreviewClick;
         _selectCurrentPreview.TopMost = true;

         _selectFloor = new OwnerDrawForm();
         _selectFloor.Paint         += OnPaintFloor;
         _selectFloor.EscapePressed += OnEscapePressed;
         _selectFloor.ReturnPressed += OnReturnPressed;
         _selectFloor.BackPressed   += OnBackPressed;
         _selectFloor.NextPressed   += OnNextPressed;
         _selectFloor.MouseClick    += OnFloorClick;
         _selectFloor.TopMost = true;
         _selectFloor.Enabled = false;

         _selectRoof = new OwnerDrawForm();
         _selectRoof.Opacity   = 0.25;
         _selectRoof.BackColor = Color.FromArgb(0, 0, 20);
         _selectRoof.EscapePressed  += OnEscapePressed;
         _selectRoof.ReturnPressed  += OnReturnPressed;
         _selectRoof.BackPressed    += OnBackPressed;
         _selectRoof.NextPressed    += OnNextPressed;
         _selectRoof.MouseClick     += OnRoofClick;
         _selectRoof.TopMost = true;
         _selectRoof.Enabled = false;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Cancel select
      /// </summary>
      public override void Cancel()
      {
         Hide();
      }

      /// <summary>
      /// Apply select
      /// </summary>
      public override void Apply()
      {
         Hide();

         if (SelectedIndex >= 0 && SelectedIndex < Forms.Length)
         {
            DockableFormInfo info = Forms[SelectedIndex];

            info.IsSelected = true;

            if (info.IsAutoHideMode)
            {
               info.ShowFormAutoPanel();
            }
         }
      }

      /// <summary>
      /// Checks if the selector has focus
      /// </summary>
      /// <returns>true if the selector has focus</returns>
      public override bool HasFocus
      {
         get 
         {
            if (_selectRoof.Focused)
            {
               return true;
            }

            if (_selectFloor.Focused)
            {
               return true;
            }

            if (_selectBackPreview.Focused)
            {
               return true;
            }

            if (_selectCurrentPreview.Focused)
            {
               return true;
            }

            if (_selectNextPreview.Focused)
            {
               return true;
            }

            return false;
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Show the selector
      /// </summary>
      protected override void ShowSelector()
      {
         if (Forms.Length < 2)
         {
            return;
         }

         if (SelectedIndex < 0)
         {
            SelectedIndex = 0;
         }

         _previewSize = new Size(ContainerScreenBounds.Width / 5, ContainerScreenBounds.Height / 3);

         InitializeSelectRoof(ContainerScreenBounds, _selectRoof);
         InitializeSelectFloor(ContainerScreenBounds, _previewSize, _selectFloor);
         InitializeSelectBackPreviewForm(ContainerScreenBounds, _previewSize, _selectBackPreview);
         InitializeSelectNextPreviewForm(ContainerScreenBounds, _previewSize, _selectNextPreview);
         InitializeSelectCurrentPreviewForm(ContainerScreenBounds, _previewSize, _selectCurrentPreview);

         _selectFloor.Show();
         _selectRoof.Show();

         UpdateNeighbours();

         _selectCurrentPreview.Focus();
         _selectCurrentPreview.Select();
      }

      /// <summary>
      /// Accessor of current form preview renderer
      /// </summary>
      protected override PreviewRenderer Renderer
      {
         get { return base.Renderer; }
         set 
         {
            base.Renderer = value;

            if (Renderer != null)
            {
               if (_selectBackRenderer != null)
               {
                  _selectBackRenderer.Invalidated -= OnSelectBackRendererInvalidated;
               }
               if (_selectNextRenderer != null)
               {
                  _selectNextRenderer.Invalidated -= OnSelectNextRendererInvalidated;
               }
               if (_selectCurrentRenderer != null)
               {
                  _selectCurrentRenderer.Invalidated -= OnSelectCurrentRendererInvalidated;
               }

               _selectBackRenderer     = Renderer.Clone();
               _selectNextRenderer     = Renderer.Clone();
               _selectCurrentRenderer  = Renderer.Clone();

               if (_selectBackRenderer != null)
               {
                  _selectBackRenderer.Invalidated += OnSelectBackRendererInvalidated;
               }
               if (_selectNextRenderer != null)
               {
                  _selectNextRenderer.Invalidated += OnSelectNextRendererInvalidated;
               }
               if (_selectCurrentRenderer != null)
               {
                  _selectCurrentRenderer.Invalidated += OnSelectCurrentRendererInvalidated;
               }
            }
         }
      }

      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// On floor click
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnFloorClick(object sender, MouseEventArgs e)
      {
         UpdateNeighbours();
      }

      /// <summary>
      /// On roof click
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnRoofClick(object sender, MouseEventArgs e)
      {
         UpdateNeighbours();
      }


      /// <summary>
      /// On back click
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnBackPreviewClick(object sender, MouseEventArgs e)
      {
         SelectedIndex--;
         Apply();
      }

      /// <summary>
      /// On next click
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnNextPreviewClick(object sender, MouseEventArgs e)
      {
         SelectedIndex++;
         Apply();
      }

      /// <summary>
      /// On current click
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnCurrentPreviewClick(object sender, MouseEventArgs e)
      {
         Apply();
      }


      /// <summary>
      /// On paint select back preview
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPaintSelectBackPreview(object sender, PaintEventArgs e)
      {
         Graphics graphics = e.Graphics;

         int dh = ComputePreviewHeightOffset(_previewSize);
         graphics.TranslateTransform(0, dh);

         Matrix transformMatrix = new Matrix();
         transformMatrix.Shear(0.0F, -0.50F);
         graphics.MultiplyTransform(transformMatrix);

         PreviewRenderer renderer = _selectBackRenderer;
         renderer.SelectedForm    = PreviousForm;
         PreviewRenderer.ShowFormPreview(new Rectangle(0, 0, _previewSize.Width, _previewSize.Height), renderer, graphics);
      }

      /// <summary>
      /// On paint select next preview
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPaintSelectNextPreview(object sender, PaintEventArgs e)
      {
         Graphics graphics = e.Graphics;

         Matrix transformMatrix = new Matrix();
         transformMatrix.Shear(0.0F, 0.50F);
         graphics.MultiplyTransform(transformMatrix);

         PreviewRenderer renderer = _selectNextRenderer;
         renderer.SelectedForm    = NextForm;
         PreviewRenderer.ShowFormPreview(new Rectangle(0, 0, _previewSize.Width, _previewSize.Height), renderer, graphics);
      }

      /// <summary>
      /// On paint select current preview
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPaintSelectCurrentPreview(object sender, PaintEventArgs e)
      {
         Graphics graphics = e.Graphics;

         PreviewRenderer renderer = _selectCurrentRenderer;
         renderer.SelectedForm    = _selectedForm;
         PreviewRenderer.ShowFormPreview(_selectCurrentPreview.ClientRectangle, renderer, graphics);
      }

      /// <summary>
      /// On paint floor
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPaintFloor(object sender, PaintEventArgs e)
      {
         using (LinearGradientBrush brush = new LinearGradientBrush(_selectFloor.ClientRectangle, Color.LightGray, Color.Navy, 90))
         {
            e.Graphics.FillRectangle(brush, _selectFloor.ClientRectangle);
         }
      }



      /// <summary>
      /// Occurs when right or down was pressed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnNextPressed(object sender, EventArgs e)
      {
         SelectedIndex++;
         UpdateNeighbours();
      }

      /// <summary>
      /// Occurs when left or up was pressed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnBackPressed(object sender, EventArgs e)
      {
         SelectedIndex--;
         UpdateNeighbours();
      }

      /// <summary>
      /// Occurs when return was pressed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnReturnPressed(object sender, EventArgs e)
      {
         Apply();
      }

      /// <summary>
      /// Occurs when escape was pressed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnEscapePressed(object sender, EventArgs e)
      {
         Cancel();
      }



      /// <summary>
      /// Occurs when the select back renderer is invalidated
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnSelectBackRendererInvalidated(object sender, InvalidateEventArgs e)
      {
         _selectBackPreview.Invalidate();
      }

      /// <summary>
      /// Occurs when the select next renderer is invalidated
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnSelectNextRendererInvalidated(object sender, InvalidateEventArgs e)
      {
         _selectNextPreview.Invalidate();
      }

      /// <summary>
      /// Occurs when the select current renderer is invalidated
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnSelectCurrentRendererInvalidated(object sender, InvalidateEventArgs e)
      {
         _selectCurrentPreview.Invalidate();
      }

      #endregion Received events

      /// <summary>
      /// Hide forms
      /// </summary>
      private void Hide()
      {
         _selectBackPreview.Hide();
         _selectNextPreview.Hide();
         _selectCurrentPreview.Hide();
         _selectFloor.Hide();
         _selectRoof.Hide();
      }

      /// <summary>
      /// Compute preview height offset
      /// </summary>
      /// <param name="previewSize">preview size</param>
      /// <returns>preview height offset</returns>
      private static int ComputePreviewHeightOffset(Size previewSize)
      {
         return (int)(previewSize.Width * Math.Tan(27.0 * Math.PI / 180.0));
      }

      /// <summary>
      /// Create back region path
      /// </summary>
      /// <param name="previewSize">preview size</param>
      /// <returns>back region path</returns>
      private static GraphicsPath CreateBackRegionPath(Size previewSize)
      {
         int dh = ComputePreviewHeightOffset(previewSize);
         GraphicsPath regionPath = GraphicsUtility.CreateRoundRectPath(0, dh, previewSize.Width, previewSize.Height, 5);

         Matrix transformMatrix  = new Matrix();
         transformMatrix.Shear(0.0F, -0.50F);
         regionPath.Transform(transformMatrix);

         return regionPath;
      }

      /// <summary>
      /// Create forward region path
      /// </summary>
      /// <param name="previewSize">preview size</param>
      /// <returns>back region path</returns>
      private static GraphicsPath CreateNextRegionPath(Size previewSize)
      {
         GraphicsPath regionPath = GraphicsUtility.CreateRoundRectPath(0, 0, previewSize.Width, previewSize.Height, 5);

         Matrix transformMatrix  = new Matrix();
         transformMatrix.Shear(0.0F, 0.50F);
         regionPath.Transform(transformMatrix);

         return regionPath;
      }

      /// <summary>
      /// Create floor region path
      /// </summary>
      /// <param name="floorSize">floor size</param>
      /// <param name="previewSize">preview size</param>
      /// <returns>floor region path</returns>
      private static GraphicsPath CreateFloorRegionPath(Size floorSize, Size previewSize)
      {
         GraphicsPath regionPath = new GraphicsPath();

         regionPath.AddLine(0, floorSize.Height - 30, previewSize.Width, 0);
         regionPath.AddLine(floorSize.Width - previewSize.Width, 0, floorSize.Width, floorSize.Height - 30);
         regionPath.AddLine(floorSize.Width, floorSize.Height, 0, floorSize.Width);

         regionPath.CloseFigure();

         return regionPath;
      }

      /// <summary>
      /// Initialize select back preview
      /// </summary>
      /// <param name="containerBounds">container bounds</param>
      /// <param name="previewSize">preview size</param>
      /// <param name="selectBackPreview">select back preview form</param>
      private static void InitializeSelectBackPreviewForm(Rectangle containerBounds, Size previewSize, Form selectBackPreview)
      {
         int dh = ComputePreviewHeightOffset(previewSize);

         using (GraphicsPath regionPath = CreateBackRegionPath(previewSize))
         {
            selectBackPreview.Visible  = false;
            selectBackPreview.Region   = null;

            selectBackPreview.Bounds   = new Rectangle(containerBounds.Left + 30, containerBounds.Bottom - previewSize.Height - dh - 30, previewSize.Width, previewSize.Height + dh);
            selectBackPreview.Region   = new Region(regionPath);
         }
      }

      /// <summary>
      /// Initialize select forward preview
      /// </summary>
      /// <param name="containerBounds">container bounds</param>
      /// <param name="previewSize">preview size</param>
      /// <param name="selectNextPreview">select forward preview form</param>
      private static void InitializeSelectNextPreviewForm(Rectangle containerBounds, Size previewSize, Form selectNextPreview)
      {
         int dh = ComputePreviewHeightOffset(previewSize);

         using (GraphicsPath regionPath = CreateNextRegionPath(previewSize))
         {
            selectNextPreview.Visible  = false;
            selectNextPreview.Region   = null;

            selectNextPreview.Bounds   = new Rectangle(containerBounds.Right - previewSize.Width - 30, 
                                                       containerBounds.Bottom - previewSize.Height - dh - 30, 
                                                       previewSize.Width, previewSize.Height + dh);
            selectNextPreview.Region   = new Region(regionPath);
         }
      }

      /// <summary>
      /// Initialize select current preview
      /// </summary>
      /// <param name="containerBounds">container bounds</param>
      /// <param name="previewSize">preview size</param>
      /// <param name="selectCurrentPreview">select current preview form</param>
      private static void InitializeSelectCurrentPreviewForm(Rectangle containerBounds, Size previewSize, Form selectCurrentPreview)
      {
         int mainWidth  = containerBounds.Width - 2 * (previewSize.Width + 70);
         int mainHeight = containerBounds.Height / 2;

         int dh = ComputePreviewHeightOffset(previewSize);

         selectCurrentPreview.Location = new Point(containerBounds.Left + previewSize.Width + 70, containerBounds.Bottom - dh - mainHeight - 30);
         selectCurrentPreview.Size     = new Size(mainWidth, mainHeight);

         using (GraphicsPath regionPath = GraphicsUtility.CreateRoundRectPath(0, 0, mainWidth, mainHeight, 5))
         {
            selectCurrentPreview.Visible  = false;
            selectCurrentPreview.Region   = null;

            selectCurrentPreview.Region   = new Region(regionPath);
         }
      }

      /// <summary>
      /// Initialize select floor
      /// </summary>
      /// <param name="containerBounds">container bounds</param>
      /// <param name="previewSize">preview size</param>
      /// <param name="selectFloor">select floor</param>
      private static void InitializeSelectFloor(Rectangle containerBounds, Size previewSize, Form selectFloor)
      {
         int dh = ComputePreviewHeightOffset(previewSize);

         selectFloor.Bounds = new Rectangle(containerBounds.Left, containerBounds.Bottom - dh - 40, containerBounds.Width, dh + 40);
         selectFloor.Region = null;

         using (GraphicsPath regionPath = CreateFloorRegionPath(selectFloor.ClientSize, previewSize))
         {
            selectFloor.Region = new Region(regionPath);
         }
      }

      /// <summary>
      /// Initialize select roof
      /// </summary>
      /// <param name="containerBounds">container bounds</param>
      /// <param name="selectRoof">container roof</param>
      private static void InitializeSelectRoof(Rectangle containerBounds, Form selectRoof)
      {
         selectRoof.Bounds = containerBounds;
      }

      /// <summary>
      /// Update neigbours
      /// </summary>
      private void UpdateNeighbours()
      {
         if (SelectedIndex >= 0 && SelectedIndex < Forms.Length)
         {
            _selectedForm = Forms[SelectedIndex].DockableForm;
            _selectCurrentPreview.Show();
            _selectCurrentPreview.Invalidate();
            _selectCurrentPreview.Activate();
         }

         if (SelectedIndex > 0 && SelectedIndex < Forms.Length)
         {
            PreviousForm = Forms[SelectedIndex - 1].DockableForm;
         }
         else
         {
            PreviousForm = null;
         }

         if (SelectedIndex >= 0 && SelectedIndex < Forms.Length - 1)
         {
            NextForm = Forms[SelectedIndex + 1].DockableForm;
         }
         else
         {
            NextForm = null;
         }
      }


      /// <summary>
      /// Accessor of the previous form
      /// </summary>
      private Form PreviousForm
      {
         get { return _previousForm; }
         set
         {
            if (_previousForm != value)
            {
               _previousForm = value;
            }

            _selectBackPreview.Visible = _previousForm != null;

            if (_selectBackPreview.Visible)
            {
               _selectBackPreview.Invalidate();
               _selectBackPreview.Activate();
            }
         }
      }

      /// <summary>
      /// Accessor of the next form
      /// </summary>
      private Form NextForm
      {
         get { return _nextForm; }
         set
         {
            if (_nextForm != value)
            {
               _nextForm = value;
            }

            _selectNextPreview.Visible = _nextForm != null;

            if (_selectNextPreview.Visible)
            {
               _selectNextPreview.Invalidate();
               _selectNextPreview.Activate();
            }
         }
      }

      #endregion Private section
   }
}
