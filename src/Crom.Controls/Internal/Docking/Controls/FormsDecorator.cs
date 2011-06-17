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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Implementation of forms decorator
   /// </summary>
   internal class FormsDecorator : Control
   {
      #region Fields

      private const int             DefaultTopTitleOffset               = 2;
      private const int             DefaultTopMargin                    = 4;
      private const int             DefaultLeftMargin                   = 4;
      private const int             DefaultRightMargin                  = 4;
      private const int             DefaultBottomMargin                 = 4;
      private const int             DefaultTitleHeight                  = 20;

      private FormsDecoratorControlCollection _controls                 = null;

      private TitleBarRenderer      _titleRenderer                      = new TitleBarRenderer();

      private ControlPositioner     _positioner                         = null;
      private bool                  _isFocused                          = false;

      private Point                 _mouseDownScreenPos                 = new Point();
      private Point                 _positionerPositionOnMouseDown      = new Point();
      private Size                  _positionerSizeOnMouseDown          = new Size();
      private zSizeMode             _sizeMode                           = zSizeMode.None;
      private bool                  _moving                             = false;

      private Timer                 _unhighlightTimer                   = new Timer();
      private bool                  _canResizeByMouse                   = true;
      private bool                  _canMoveByMouse                     = true;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public FormsDecorator()
      {
         FormsPanel.Bounds      = ClientRectangle;
         FormsPanel.Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

         TitleBar.Height        = 24;
         TitleBar.Cursor        = Cursors.SizeAll;
         TitleBar.Paint        += OnPaintTitleBar;
         TitleBar.MouseDown    += OnMouseDownInTitleBar;
         TitleBar.MouseMove    += OnMouseMoveInTitleBar;
         TitleBar.MouseUp      += OnMouseUpFromTitleBar;

         TopMargin.Height       = 4;
         TopMargin.Cursor       = Cursors.SizeNS;
         TopMargin.MouseDown   += OnMouseDownInTopMargin;
         TopMargin.MouseMove   += OnMouseMoveInTopMargin;
         TopMargin.MouseUp     += OnMouseUpFromTopMargin;

         LeftMargin.Width       = 4;
         LeftMargin.Cursor      = Cursors.SizeWE;
         LeftMargin.MouseDown  += OnMouseDownInLeftMargin;
         LeftMargin.MouseMove  += OnMouseMoveInLeftMargin;
         LeftMargin.MouseUp    += OnMouseUpFromLeftMargin;

         RightMargin.Width      = 4;
         RightMargin.Cursor     = Cursors.SizeWE;
         RightMargin.MouseDown += OnMouseDownInRightMargin;
         RightMargin.MouseMove += OnMouseMoveInRightMargin;
         RightMargin.MouseUp   += OnMouseUpFromRightMargin;

         BottomMargin.Height    = 4;
         BottomMargin.Cursor    = Cursors.SizeNS;
         BottomMargin.MouseDown += OnMouseDownInBottomMargin;
         BottomMargin.MouseMove += OnMouseMoveInBottomMargin;
         BottomMargin.MouseUp   += OnMouseUpFromBottomMargin;


         TitleBar.BackColor     = SystemColors.Control;
         LeftMargin.BackColor   = SystemColors.Control;
         RightMargin.BackColor  = SystemColors.Control;
         BottomMargin.BackColor = SystemColors.Control;

         FormsPanel.BackColor   = SystemColors.Control;
         FormsPanel.Visible     = false;

         FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;
         forms.TopControlChanged += OnTopFormChanged;

         _unhighlightTimer.Tick    += OnUnhighlightTimer;
         _unhighlightTimer.Interval = 200;
         _unhighlightTimer.Enabled  = true;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when the context button was clicked
      /// </summary>
      public event EventHandler ContextButtonClick;

      /// <summary>
      /// Occurs when the auto-hide button was clicked
      /// </summary>
      public event EventHandler AutohideButtonClick;

      /// <summary>
      /// Occurs when the close button was clicked
      /// </summary>
      public event EventHandler CloseButtonClick;

      /// <summary>
      /// Occurs when the IsFocused property has changed
      /// </summary>
      public event EventHandler IsFocusedChanged;

      /// <summary>
      /// Occurs when colors were changed in the decorator schema
      /// </summary>
      public event EventHandler ColorsChanged;

      /// <summary>
      /// Change is focused state
      /// </summary>
      public bool IsFocused
      {
         get { return _isFocused; }
         set
         {
            if (_isFocused != value)
            {
               _isFocused = value;
               TitleBar.Invalidate();

               if (IsFocusedChanged != null)
               {
                  IsFocusedChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Positioner
      /// </summary>
      public ControlPositioner Positioner
      {
         get
         {
            if (_positioner == null)
            {
               _positioner = new ControlPositioner(this);
               _positioner.Disposed += OnPositionerDisposed;
            }

            return _positioner;
         }
         set
         {
            if (_positioner != null)
            {
               _positioner.Disposed             -= OnPositionerDisposed;
               _positioner.CanMoveChanged       -= OnPositionerCanMoveChanged;
               _positioner.CanSizeLeftChanged   -= OnPositionerCanSizeLeftChanged;
               _positioner.CanSizeRightChanged  -= OnPositionerCanSizeRightChanged;
               _positioner.CanSizeTopChanged    -= OnPositionerCanSizeTopChanged;
               _positioner.CanSizeBottomChanged -= OnPositionerCanSizeBottomChanged;
            }

            _positioner = value;

            if (_positioner != null)
            {
               _positioner.Disposed             += OnPositionerDisposed;
               _positioner.CanMoveChanged       += OnPositionerCanMoveChanged;
               _positioner.CanSizeLeftChanged   += OnPositionerCanSizeLeftChanged;
               _positioner.CanSizeRightChanged  += OnPositionerCanSizeRightChanged;
               _positioner.CanSizeTopChanged    += OnPositionerCanSizeTopChanged;
               _positioner.CanSizeBottomChanged += OnPositionerCanSizeBottomChanged;
            }

            OnPositionerCanMoveChanged(null, EventArgs.Empty);
            OnPositionerCanSizeLeftChanged(null, EventArgs.Empty);
            OnPositionerCanSizeRightChanged(null, EventArgs.Empty);
            OnPositionerCanSizeTopChanged(null, EventArgs.Empty);
            OnPositionerCanSizeBottomChanged(null, EventArgs.Empty);
         }
      }

      /// <summary>
      /// Get the count of forms inside this control
      /// </summary>
      public int FormsCount
      {
         get { return FormsPanel.Controls.Count; }
      }

      /// <summary>
      /// Add form
      /// </summary>
      /// <param name="form">form</param>
      public void Add(Form form)
      {
         form.TopLevel      = false;
         form.ShowInTaskbar = false;
         FormsPanel.Controls.Add(form);
         form.Visible       = true;
      }

      /// <summary>
      /// Remove a form from decorator
      /// </summary>
      /// <param name="form">form to remove</param>
      /// <returns>true if form was removed</returns>
      public bool Remove(Form form)
      {
         if (FormsPanel.Controls.Contains(form))
         {
            FormsPanel.Controls.Remove(form);
            return true;
         }

         return false;
      }

      /// <summary>
      /// Get the form at zero based index
      /// </summary>
      /// <param name="index">zero based form index</param>
      /// <returns>form at index</returns>
      public Form GetFormAt(int index)
      {
         return (Form)FormsPanel.Controls[index];
      }

      /// <summary>
      /// Select a form
      /// </summary>
      /// <param name="form">form to select</param>
      public void SelectForm(Form form)
      {
         FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;
         forms.SetChildIndex(form, 0);
      }


      /// <summary>
      /// Flag indicating if can move the control
      /// </summary>
      public bool CanMoveByMouse
      {
         get { return _canMoveByMouse; }
         set
         {
            if (_canMoveByMouse != value)
            {
               _canMoveByMouse = value;

               OnPositionerCanMoveChanged(null, EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// Flag indicating if can resize the control
      /// </summary>
      public bool CanResizeByMouse
      {
         get { return _canResizeByMouse; }
         set 
         {
            if (_canResizeByMouse != value)
            {
               _canResizeByMouse = value;

               OnPositionerCanSizeLeftChanged(null, EventArgs.Empty);
               OnPositionerCanSizeRightChanged(null, EventArgs.Empty);
               OnPositionerCanSizeTopChanged(null, EventArgs.Empty);
               OnPositionerCanSizeBottomChanged(null, EventArgs.Empty);

               SetFormsPanelBounds();
               ApplyTopFormMargins();
            }
         }
      }

      /// <summary>
      /// Begin movement by mouse
      /// </summary>
      /// <param name="mouseScreenPos">mouse down screen position</param>
      public void BeginMovementByMouse(Point mouseScreenPos)
      {
         if (Positioner.CanMove)
         {
            _mouseDownScreenPos            = mouseScreenPos;
            _positionerPositionOnMouseDown = Positioner.Location;
            _sizeMode                      = zSizeMode.Move;
            _moving                        = false;
         }
      }

      /// <summary>
      /// Continue movement by mouse
      /// </summary>
      /// <param name="mouseScreenPos">mouse screen position</param>
      /// <returns>true if movement is continued</returns>
      public bool ContinueMovementByMouse(Point mouseScreenPos)
      {
         if (_sizeMode != zSizeMode.Move)
         {
            return false;
         }

         int dx = mouseScreenPos.X - _mouseDownScreenPos.X;
         int dy = mouseScreenPos.Y - _mouseDownScreenPos.Y;

         if (_moving == false)
         {
            _moving = true;
            Positioner.StartMoveByMouse();
            _positionerPositionOnMouseDown = Positioner.Location;
            _positionerSizeOnMouseDown     = Positioner.Size;
         }

         Positioner.PerformMoveByMouse(_positionerPositionOnMouseDown.X + dx, _positionerPositionOnMouseDown.Y + dy);

         return true;
      }

      /// <summary>
      /// End movement by mouse
      /// </summary>
      public void EndMovementByMouse()
      {
         if (_moving)
         {
            Positioner.StopMoveByMouse();
            _moving = false;
         }

         _sizeMode = zSizeMode.None;
      }

      /// <summary>
      /// Flag indicating if the form is in auto-hidden mode or not
      /// </summary>
      public bool AutoHidden
      {
         get { return _titleRenderer.Autohide; }
         set
         {
            if (_titleRenderer.Autohide != value)
            {
               _titleRenderer.Autohide = value;
               _controls.TitleBar.Invalidate();
            }
         }
      }

      /// <summary>
      /// Show close button
      /// </summary>
      public bool ShowCloseButton
      {
         get { return _titleRenderer.ShowCloseButton; }
         set
         {
            if (_titleRenderer.ShowCloseButton != value)
            {
               _titleRenderer.ShowCloseButton = value;
               _controls.TitleBar.Invalidate();
            }
         }
      }

      /// <summary>
      /// Show autohide button
      /// </summary>
      public bool ShowAutohideButton
      {
         get { return _titleRenderer.ShowAutohideButton; }
         set
         {
            if (_titleRenderer.ShowAutohideButton != value)
            {
               _titleRenderer.ShowAutohideButton = value;
               _controls.TitleBar.Invalidate();
            }
         }
      }

      /// <summary>
      /// Show context menu button
      /// </summary>
      public bool ShowContextMenuButton
      {
         get { return _titleRenderer.ShowContextMenuButton; }
         set
         {
            if (_titleRenderer.ShowContextMenuButton != value)
            {
               _titleRenderer.ShowContextMenuButton = value;
               _controls.TitleBar.Invalidate();
            }
         }
      }

      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color Color1
      {
         get { return _titleRenderer.Color1; }
         set 
         {
            if (_titleRenderer.Color1 != value)
            {
               _titleRenderer.Color1 = value;

               Invalidate();

               if (ColorsChanged != null)
               {
                  ColorsChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color Color2
      {
         get { return _titleRenderer.Color2; }
         set
         {
            if (_titleRenderer.Color2 != value)
            {
               _titleRenderer.Color2 = value;

               Invalidate();

               if (ColorsChanged != null)
               {
                  ColorsChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color SelectedColor1
      {
         get { return _titleRenderer.SelectedColor1; }
         set
         {
            if (_titleRenderer.SelectedColor1 != value)
            {
               _titleRenderer.SelectedColor1 = value;

               Invalidate();

               if (ColorsChanged != null)
               {
                  ColorsChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color SelectedColor2
      {
         get { return _titleRenderer.SelectedColor2; }
         set
         {
            if (_titleRenderer.SelectedColor2 != value)
            {
               _titleRenderer.SelectedColor2 = value;

               Invalidate();

               if (ColorsChanged != null)
               {
                  ColorsChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Text color
      /// </summary>
      public Color TextColor
      {
         get { return _titleRenderer.TextColor; }
         set
         {
            if (_titleRenderer.TextColor != value)
            {
               _titleRenderer.TextColor = value;

               Invalidate();

               if (ColorsChanged != null)
               {
                  ColorsChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">called from IDisposable.Dispose</param>
      protected override void Dispose(bool fromIDisposableDispose)
      {
         if (fromIDisposableDispose)
         {
            if (_unhighlightTimer != null)
            {
               _unhighlightTimer.Enabled  = false;
               _unhighlightTimer.Tick    -= OnUnhighlightTimer;
               _unhighlightTimer.Dispose();
               _unhighlightTimer          = null;
            }
         }

         base.Dispose(fromIDisposableDispose);
      }

      /// <summary>
      /// Occurs when visible changed
      /// </summary>
      /// <param name="e">event args</param>
      protected override void OnVisibleChanged(EventArgs e)
      {
         base.OnVisibleChanged(e);

         FormsPanel.Visible = Visible;
      }

      /// <summary>
      /// Occurs when the size of this control is changed
      /// </summary>
      /// <param name="e"></param>
      protected override void OnSizeChanged(EventArgs e)
      {
         SetFormsPanelBounds();
         ApplyTopFormMargins();

         base.OnSizeChanged(e);
      }

      /// <summary>
      /// Create controls
      /// </summary>
      /// <returns>controls</returns>
      protected override ControlCollection CreateControlsInstance()
      {
         return InternalControls;
      }

      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// Occurs when the positioner was disposed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnPositionerDisposed(object sender, EventArgs e)
      {
         Positioner = null;
      }


      /// <summary>
      /// On top form changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">e</param>
      private void OnTopFormChanged(object sender, ControlSwitchedEventArgs e)
      {
         if (e.OldControl != null)
         {
            e.OldControl.Resize -= OnTopFormResize;
         }

         SetFormsPanelBounds();
         ApplyTopFormMargins();

         if (e.NewControl != null)
         {
            e.NewControl.Resize += OnTopFormResize;
         }

         return;
      }

      /// <summary>
      /// On top form resize
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnTopFormResize(object sender, EventArgs e)
      {
         ApplyTopFormMargins();
      }

      /// <summary>
      /// On paint title bar
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnPaintTitleBar(object sender, PaintEventArgs e)
      {
         Font font = Font;
         Color color1 = _titleRenderer.Color1;
         Color color2 = _titleRenderer.Color2;
         if (IsFocused == false)
         {
            if (font.Bold)
            {
               font = new Font(font, FontStyle.Regular);
            }
         }
         else
         {
            if (font.Bold == false)
            {
               font = new Font(font, FontStyle.Bold);
            }

            color1 = _titleRenderer.SelectedColor1;
            color2 = _titleRenderer.SelectedColor2;
         }

         _titleRenderer.Draw(font, e.Graphics, color1, color2);
      }


      /// <summary>
      /// On un-hightlight timer
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">even argument</param>
      private void OnUnhighlightTimer(object sender, EventArgs e)
      {
         int buttonIndex  = _titleRenderer.TitleBarButtonIndexUnderMouse;
         if (buttonIndex >= 0)
         {
            Point mousePos = TitleBar.PointToClient(Control.MousePosition);

            _titleRenderer.UpdateTitleBarButtonIndexUnderMouse(mousePos);

            if (_titleRenderer.TitleBarButtonIndexUnderMouse != buttonIndex)
            {
               TitleBar.Invalidate();
            }
         }
      }



      /// <summary>
      /// On mouse down in title bar
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseDownInTitleBar(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;

         if (e.Button == MouseButtons.Left)
         {
            if (_titleRenderer.TitleBarButtonIndexUnderMouse >= 0)
            {
               EventHandler handler = null;

               int index = -1;
               if (_titleRenderer.ShowContextMenuButton)
               {
                  index++;
                  if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                  {
                     handler = ContextButtonClick;
                  }
               }
               
               if (_titleRenderer.ShowAutohideButton)
               {
                  index++;
                  if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                  {
                     handler = AutohideButtonClick;
                  }
               }
               
               if (_titleRenderer.ShowCloseButton)
               {
                  index++;
                  if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                  {
                     handler = CloseButtonClick;
                  }
               }

               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
            else if (CanMoveByMouse)
            {
               BeginMovementByMouse(TitleBar.PointToScreen(e.Location));
            }
         }
      }

      /// <summary>
      /// On mouse move over title bar
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseMoveInTitleBar(object sender, MouseEventArgs e)
      {
         if (_sizeMode == zSizeMode.Move)
         {
            Point location = TitleBar.PointToScreen(e.Location);
            ContinueMovementByMouse(location);

            return;
         }

         int buttonIndex  = _titleRenderer.TitleBarButtonIndexUnderMouse;
         _titleRenderer.UpdateTitleBarButtonIndexUnderMouse(e.Location);
         if (buttonIndex != _titleRenderer.TitleBarButtonIndexUnderMouse)
         {
            TitleBar.Invalidate();
         }

         Cursor cursor = Cursors.Default;
         if (buttonIndex >= 0)
         {
            cursor = Cursors.Hand;
         }
         else if (_positioner != null)
         {
            if (_positioner.CanMove && CanMoveByMouse)
            {
               cursor = Cursors.SizeAll;
            }
         }

         TitleBar.Cursor = cursor;
      }

      /// <summary>
      /// On mouse released from bar
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseUpFromTitleBar(object sender, MouseEventArgs e)
      {
         EndMovementByMouse();
      }


      /// <summary>
      /// On mouse down in title top margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseDownInTopMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;

         if (e.Button == MouseButtons.Left && Positioner.CanSizeTop && CanResizeByMouse)
         {
            _mouseDownScreenPos            = TopMargin.PointToScreen(e.Location);
            _positionerPositionOnMouseDown = Positioner.Location;
            _positionerSizeOnMouseDown     = Positioner.Size;
            _sizeMode                      = zSizeMode.Top;
         }
      }

      /// <summary>
      /// On mouse move over title top margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseMoveInTopMargin(object sender, MouseEventArgs e)
      {
         if (_sizeMode == zSizeMode.Top)
         {
            Point location = LeftMargin.PointToScreen(e.Location);
            int dy = location.Y - _mouseDownScreenPos.Y;

            Rectangle bounds = new Rectangle();
            bounds.X       = _positionerPositionOnMouseDown.X;
            bounds.Y       = _positionerPositionOnMouseDown.Y + dy;
            bounds.Width   = _positionerSizeOnMouseDown.Width;
            bounds.Height  = _positionerSizeOnMouseDown.Height - dy;

            Positioner.Bounds = bounds;
         }
      }

      /// <summary>
      /// On mouse released from top margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseUpFromTopMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;
      }


      /// <summary>
      /// On mouse down in title left margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseDownInLeftMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;

         if (e.Button == MouseButtons.Left && Positioner.CanSizeLeft && CanResizeByMouse)
         {
            _mouseDownScreenPos            = LeftMargin.PointToScreen(e.Location);
            _positionerPositionOnMouseDown = Positioner.Location;
            _positionerSizeOnMouseDown     = Positioner.Size;
            _sizeMode                      = zSizeMode.Left;
         }
      }

      /// <summary>
      /// On mouse move over title left margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseMoveInLeftMargin(object sender, MouseEventArgs e)
      {
         if (_sizeMode == zSizeMode.Left)
         {
            Point location = LeftMargin.PointToScreen(e.Location);
            int dx = location.X - _mouseDownScreenPos.X;

            Rectangle bounds = new Rectangle();
            bounds.X       = _positionerPositionOnMouseDown.X + dx;
            bounds.Width   = _positionerSizeOnMouseDown.Width - dx;
            bounds.Y       = _positionerPositionOnMouseDown.Y;
            bounds.Height  = _positionerSizeOnMouseDown.Height;

            Positioner.Bounds = bounds;
         }
      }

      /// <summary>
      /// On mouse released from left margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseUpFromLeftMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;
      }


      /// <summary>
      /// On mouse down in title right margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseDownInRightMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;

         if (e.Button == MouseButtons.Left && Positioner.CanSizeRight && CanResizeByMouse)
         {
            _mouseDownScreenPos            = RightMargin.PointToScreen(e.Location);
            _positionerPositionOnMouseDown = Positioner.Location;
            _positionerSizeOnMouseDown     = Positioner.Size;
            _sizeMode                      = zSizeMode.Right;
         }
      }

      /// <summary>
      /// On mouse move over title right margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseMoveInRightMargin(object sender, MouseEventArgs e)
      {
         if (_sizeMode == zSizeMode.Right)
         {
            Point location = RightMargin.PointToScreen(e.Location);
            int dx = location.X - _mouseDownScreenPos.X;

            Positioner.Size = new Size(
               _positionerSizeOnMouseDown.Width + dx,
               Positioner.Size.Height);
         }
      }

      /// <summary>
      /// On mouse released from right margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseUpFromRightMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;
      }


      /// <summary>
      /// On mouse down in title bottom margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseDownInBottomMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;

         if (e.Button == MouseButtons.Left && Positioner.CanSizeBottom && CanResizeByMouse)
         {
            _mouseDownScreenPos            = BottomMargin.PointToScreen(e.Location);
            _positionerPositionOnMouseDown = Positioner.Location;
            _positionerSizeOnMouseDown     = Positioner.Size;
            _sizeMode                      = zSizeMode.Bottom;
         }
      }

      /// <summary>
      /// On mouse move over title bottom margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseMoveInBottomMargin(object sender, MouseEventArgs e)
      {
         if (_sizeMode == zSizeMode.Bottom)
         {
            Point location = BottomMargin.PointToScreen(e.Location);
            int dy = location.Y - _mouseDownScreenPos.Y;

            Positioner.Size = new Size(
               Positioner.Size.Width,
               _positionerSizeOnMouseDown.Height + dy);
         }
      }

      /// <summary>
      /// On mouse released from bottom margin
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arg</param>
      private void OnMouseUpFromBottomMargin(object sender, MouseEventArgs e)
      {
         _sizeMode = zSizeMode.None;
      }




      /// <summary>
      /// Occurs when positioner can move changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnPositionerCanMoveChanged(object sender, EventArgs e)
      {
         Cursor cursor = Cursors.Default;
         if (_positioner != null)
         {
            if (_positioner.CanMove && CanMoveByMouse)
            {
               cursor = Cursors.SizeAll;
            }
         }

         TitleBar.Cursor = cursor;
      }

      /// <summary>
      /// Occurs when positioner can size left changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnPositionerCanSizeLeftChanged(object sender, EventArgs e)
      {
         Cursor cursor = Cursors.Default;
         if (_positioner != null)
         {
            if (_positioner.CanSizeLeft && CanResizeByMouse)
            {
               cursor = Cursors.SizeWE;
            }
         }

         LeftMargin.Cursor = cursor;
      }

      /// <summary>
      /// Occurs when positioner can size right changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnPositionerCanSizeRightChanged(object sender, EventArgs e)
      {
         Cursor cursor = Cursors.Default;
         if (_positioner != null)
         {
            if (_positioner.CanSizeRight && CanResizeByMouse)
            {
               cursor = Cursors.SizeWE;
            }
         }

         RightMargin.Cursor = cursor;
      }

      /// <summary>
      /// Occurs when positioner can size top changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnPositionerCanSizeTopChanged(object sender, EventArgs e)
      {
         Cursor cursor = Cursors.Default;
         if (_positioner != null)
         {
            if (_positioner.CanSizeTop && CanResizeByMouse)
            {
               cursor = Cursors.SizeNS;
            }
         }

         TopMargin.Cursor = cursor;
      }

      /// <summary>
      /// Occurs when positioner can size bottom changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnPositionerCanSizeBottomChanged(object sender, EventArgs e)
      {
         Cursor cursor = Cursors.Default;
         if (_positioner != null)
         {
            if (_positioner.CanSizeBottom && CanResizeByMouse)
            {
               cursor = Cursors.SizeNS;
            }
         }

         BottomMargin.Cursor = cursor;
      }


      #endregion Received events

      /// <summary>
      /// Accessor of the controls
      /// </summary>
      private FormsDecoratorControlCollection InternalControls
      {
         get 
         {
            if (_controls == null)
            {
               _controls = new FormsDecoratorControlCollection(this);
            }

            return _controls; 
         }
      }

      /// <summary>
      /// Accessor of the forms panel
      /// </summary>
      private FormsContainer FormsPanel
      {
         get { return InternalControls.FormsPanel; }
      }

      /// <summary>
      /// Accessor of the forms title bar
      /// </summary>
      private Control TitleBar
      {
         get { return InternalControls.TitleBar; }
      }

      /// <summary>
      /// Accessor of the forms top margin
      /// </summary>
      private Control TopMargin
      {
         get { return InternalControls.TopMargin; }
      }

      /// <summary>
      /// Accessor of the forms left margin
      /// </summary>
      private Control LeftMargin
      {
         get { return InternalControls.LeftMargin; }
      }

      /// <summary>
      /// Accessor of the forms right margin
      /// </summary>
      private Control RightMargin
      {
         get { return InternalControls.RightMargin; }
      }

      /// <summary>
      /// Accessor of the forms bottom margin
      /// </summary>
      private Control BottomMargin
      {
         get { return InternalControls.BottomMargin; }
      }


      /// <summary>
      /// Apply top form margins
      /// </summary>
      private void ApplyTopFormMargins()
      {
         int topOffset        = 0;
         Margins margins      = GetMargins(out topOffset);

         if (topOffset == 0)
         {
            TopMargin.Top        = FormsPanel.Top;
            TopMargin.Left       = FormsPanel.Left;
            TopMargin.Width      = FormsPanel.Width;
            TopMargin.Height     = margins.Bottom;

            TitleBar.Top         = TopMargin.Bottom;
            TitleBar.Left        = TopMargin.Left;
            TitleBar.Width       = TopMargin.Width;
            TitleBar.Height      = margins.Top - margins.Bottom;

            BottomMargin.Top     = FormsPanel.Bottom - margins.Bottom;
            BottomMargin.Left    = TopMargin.Left;
            BottomMargin.Width   = TopMargin.Width;
            BottomMargin.Height  = margins.Bottom;


            LeftMargin.Top       = TitleBar.Bottom;
            LeftMargin.Left      = FormsPanel.Left;
            LeftMargin.Width     = margins.Left;
            LeftMargin.Height    = BottomMargin.Top - TitleBar.Bottom;

            RightMargin.Top      = TitleBar.Bottom;
            RightMargin.Left     = FormsPanel.Right - margins.Right;
            RightMargin.Width    = margins.Right;
            RightMargin.Height   = BottomMargin.Top - TitleBar.Bottom;

            _titleRenderer.ContentTopOffset = 0;
         }
         else
         {
            TopMargin.Top        = FormsPanel.Top - margins.Top;
            TopMargin.Left       = 0;
            TopMargin.Width      = Width;
            TopMargin.Height     = DefaultTopMargin;

            TitleBar.Top         = TopMargin.Bottom;
            TitleBar.Left        = TopMargin.Left;
            TitleBar.Width       = TopMargin.Width;
            TitleBar.Height      = DefaultTitleHeight;

            BottomMargin.Top     = FormsPanel.Bottom;
            BottomMargin.Left    = TopMargin.Left;
            BottomMargin.Width   = TopMargin.Width;
            BottomMargin.Height  = margins.Bottom;


            LeftMargin.Top       = TitleBar.Bottom;
            LeftMargin.Left      = FormsPanel.Left - margins.Left;
            LeftMargin.Width     = margins.Left;
            LeftMargin.Height    = BottomMargin.Top - TitleBar.Bottom;

            RightMargin.Top      = TitleBar.Bottom;
            RightMargin.Left     = FormsPanel.Right;
            RightMargin.Width    = margins.Right;
            RightMargin.Height   = BottomMargin.Top - TitleBar.Bottom;

            _titleRenderer.ContentTopOffset = DefaultTopTitleOffset;
         }


         FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;

         Form selectedForm = forms.TopControl as Form;
         if (selectedForm != null)
         {
            _titleRenderer.Icon = selectedForm.Icon;
            _titleRenderer.Text = selectedForm.Text;

            selectedForm.Bounds = FormsPanel.ClientRectangle;
         }

         _titleRenderer.TitleBarBounds = TitleBar.ClientRectangle;


         TitleBar.Invalidate();
      }

      /// <summary>
      /// Set the bounds of the forms panel
      /// </summary>
      private void SetFormsPanelBounds()
      {
         int topOffset     = 0;
         Margins margins   = GetMargins(out topOffset);

         Rectangle bounds  = new Rectangle();
         if (CanResizeByMouse)
         {
            if (topOffset == 0)
            {
               bounds.X       = 0;
               bounds.Y       = 0;
               bounds.Width   = ClientSize.Width;
               bounds.Height  = ClientSize.Height;
            }
            else
            {
               bounds.X       = margins.Left;
               bounds.Y       = margins.Top;
               bounds.Width   = ClientSize.Width - margins.Left - margins.Right;
               bounds.Height  = ClientSize.Height - margins.Top - margins.Bottom;
            }
         }
         else
         {
            if (topOffset == 0)
            {
               bounds.X       = -margins.Left;
               bounds.Y       = -margins.Bottom;
               bounds.Width   = ClientSize.Width + margins.Left + margins.Right;
               bounds.Height  = ClientSize.Height + 2 * margins.Bottom;
            }
            else
            {
               bounds.X       = 0;
               bounds.Y       = topOffset;
               bounds.Width   = ClientSize.Width;
               bounds.Height  = ClientSize.Height - topOffset;
            }
         }

         FormsPanel.Bounds = bounds;
      }

      /// <summary>
      /// Gets the margins for the top form
      /// </summary>
      /// <param name="topOffset">top offset (title height when border style is none or zero when border style is provided)</param>
      /// <returns>margins</returns>
      private Margins GetMargins(out int topOffset)
      {
         FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;

         Margins margins = FormWrapper.GetMargins(forms.TopControl);

         topOffset    = 0;
         Form topForm = forms.TopControl as Form;
         if (topForm != null)
         {
            if (topForm.FormBorderStyle == FormBorderStyle.None)
            {
               margins.Top    = DefaultTopMargin + DefaultTitleHeight;
               margins.Left   = DefaultLeftMargin;
               margins.Right  = DefaultRightMargin;
               margins.Bottom = DefaultBottomMargin;

               topOffset      = DefaultTitleHeight;
            }
         }

         return margins;
      }

      #endregion Private section
   }
}
