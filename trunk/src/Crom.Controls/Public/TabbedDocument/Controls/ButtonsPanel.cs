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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Crom.Controls.TabbedDocument
{
   /// <summary>
   /// Implementation of buttons panel object
   /// </summary>
   public abstract partial class ButtonsPanel : UserControl
   {
      #region Fields

      private const int                ButtonMargin                     = 16;
      private const int                ButtonHeight                     = 16;

      private Timer                    _scrollMoveTimer                 = new Timer();

      private int                      _selectedIndex                   = -1;
      private List<TabButton>          _buttons                         = new List<TabButton>();

      private TabButtonRenderer        _buttonsRenderer                 = null;

      private Color                    _backGradient1                   = SystemColors.Control;
      private Color                    _backGradient2                   = SystemColors.ControlLightLight;
      private LinearGradientMode       _backGradientMode                = LinearGradientMode.Horizontal;

      private bool                     _cutRoundRect                    = true;

      private bool                     _isMouseDownInScrollBackButton   = false;
      private bool                     _isMouseDownInScrollNextButton   = false;
      private bool                     _isMouseOverScrollBackButton     = false;
      private bool                     _isMouseOverScrollNextButton     = false;

      private bool                     _isMouseDownInTabButton          = false;
      private bool                     _isMovingTabButton               = false;
      private bool                     _isDraggingTabButton             = false;
      private TabButton                _buttonDisplaced                 = null;

      private Rectangle                _clientBounds                    = new Rectangle();
      private Rectangle                _buttonsPanelBounds              = new Rectangle();
      private Rectangle                _pagesPanelBounds                = new Rectangle();
      private bool                     _updatePositionsOnDraw           = false;
      private bool                     _hasScrolls                      = false;
      private bool                     _canScrollNext                   = false;
      private int                      _firstShownButtonIndex           = 0;

      private Rectangle                _scrollBackBounds                = new Rectangle();
      private Rectangle                _scrollNextBounds                = new Rectangle();

      private Rectangle                _captionButtonsBounds            = new Rectangle();
      private int                      _captionButtonIndexUnderMouse    = -1;

      private bool                     _showOneTabButton                = true;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public ButtonsPanel()
      {
         InitializeComponent();

         SetStyle(ControlStyles.AllPaintingInWmPaint,  true);
         SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
         SetStyle(ControlStyles.ResizeRedraw,          true);
         SetStyle(ControlStyles.Selectable,            true);
         SetStyle(ControlStyles.UserPaint,             true);

         //_scrollMoveTimer.Tick    += OnScrollMoveTimeOccurence;
         //_scrollMoveTimer.Interval = 500;
         //_scrollMoveTimer.Enabled  = true;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Buttons renderer
      /// </summary>
      public TabButtonRenderer ButtonsRenderer
      {
         get 
         {
            if (_buttonsRenderer == null)
            {
               _buttonsRenderer  = new TopTabButtonRenderer();
               OnTabButonRendererChanged();
            }

            return _buttonsRenderer; 
         }
         set 
         {
            if (_buttonsRenderer != value)
            {
               _buttonsRenderer = value;
               OnTabButonRendererChanged();
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Back gradient color 1
      /// </summary>
      public Color BackGradient1
      {
         get { return _backGradient1; }
         set { _backGradient1 = value; }
      }

      /// <summary>
      /// Back gradient color 2
      /// </summary>
      public Color BackGradient2
      {
         get { return _backGradient2; }
         set { _backGradient2 = value; }
      }

      /// <summary>
      /// Back gradient mode
      /// </summary>
      public LinearGradientMode BackGradientMode
      {
         get { return _backGradientMode; }
         set { _backGradientMode = value; }
      }


      /// <summary>
      /// Set this true to show tab buttons when only one button exists
      /// </summary>
      public bool ShowOneTabButton
      {
         get { return _showOneTabButton; }
         set 
         {
            if (_showOneTabButton != value)
            {
               _showOneTabButton      = value;
               _updatePositionsOnDraw = true;
            }
         }
      }


      /// <summary>
      /// Zero based selected button index
      /// </summary>
      public int SelectedIndex
      {
         get { return _selectedIndex; }
         set 
         {
            if (_selectedIndex != value)
            {
               if ((value < 0 || value >= _buttons.Count) && _buttons.Count > 0)
               {
                  throw new IndexOutOfRangeException("Valid values are between 0 and " + _buttons.Count.ToString() + ".");
               }

               _selectedIndex = value;

               Invalidate();
            }

            OnSelectedIndexSet(EventArgs.Empty);
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (_scrollMoveTimer != null)
            {
               _scrollMoveTimer.Enabled = false;
               _scrollMoveTimer.Tick   -= OnScrollMoveTimeOccurence;
               _scrollMoveTimer.Dispose();
               _scrollMoveTimer = null;
            }
         }

         base.Dispose(disposing);
      }

      /// <summary>
      /// Occurs when paint is required
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnPaint(PaintEventArgs e)
      {
         if (_clientBounds != ClientRectangle || _updatePositionsOnDraw)
         {
            _clientBounds = ClientRectangle;
            _updatePositionsOnDraw = false;

            UpdatePositions(e.Graphics, false);
         }

         OnPaintPanelBackground(e);


         if (CaptionButtonsCount > 0)
         {
            OnDrawCaptionButtons(_captionButtonsBounds, e.Graphics);
         }

         DrawRoundBorder(e);

         int buttonsCount = _buttons.Count;
         if (buttonsCount == 1 && ShowOneTabButton == false)
         {
            buttonsCount = 0;
         }

         if (buttonsCount > 0)
         {
            DrawButtonsLine(e);

            RectangleF clip = e.Graphics.ClipBounds;

            e.Graphics.SetClip(_buttonsPanelBounds);

            for (int index = buttonsCount - 1; index >= 0; index--)
            {
               if (index != SelectedIndex)
               {
                  _buttons[index].Draw(ButtonsRenderer, false, Font, e.Graphics);
               }
            }

            _buttons[SelectedIndex].Draw(ButtonsRenderer, true, Font, e.Graphics);

            e.Graphics.SetClip(clip);
         }


         if (_hasScrolls && buttonsCount > 1)
         {
            zButtonState stateBack = zButtonState.Normal;
            zButtonState stateNext = zButtonState.Normal;
            if (_firstShownButtonIndex == 0)
            {
               stateBack = zButtonState.Disabled;
            }
            else if (IsMouseDownInScrollBackButton)
            {
               stateBack = zButtonState.Pressed | zButtonState.UnderMouseCursor;
            }
            else if (IsMouseOverScrollBackButton)
            {
               stateBack = zButtonState.UnderMouseCursor;
            }
            else if (IsMouseDownInScrollNextButton)
            {
               stateNext = zButtonState.Pressed | zButtonState.UnderMouseCursor;
            }
            else if (IsMouseOverScrollNextButton)
            {
               stateNext = zButtonState.UnderMouseCursor;
            }

            if (_canScrollNext == false)
            {
               stateNext = zButtonState.Disabled;
            }

            ButtonsRenderer.DrawScrollBackButton(_scrollBackBounds, stateBack, e.Graphics);
            ButtonsRenderer.DrawScrollNextButton(_scrollNextBounds, stateNext, e.Graphics);
         }
     
         base.OnPaint(e);
      }

      /// <summary>
      /// Draw the round border
      /// </summary>
      /// <param name="e">event args</param>
      protected virtual void DrawRoundBorder(PaintEventArgs e)
      {
         int round  = RoundRadius;
         int width  = Width - 1;
         int height = Height - 1;

         using (GraphicsPath roundRectPath = GraphicsUtility.CreateRoundRectPath(0, 0, width, height, round))
         {
            using (Brush backBrush = new SolidBrush(ButtonsRenderer.SelectedBackGradient2))
            {
               e.Graphics.FillPath(backBrush, roundRectPath);
            }

            using (Pen borderPen = new Pen(ButtonsRenderer.SelectedBorder2))
            {
               e.Graphics.DrawPath(borderPen, roundRectPath);

               if (CutRoundRect)
               {
                  e.Graphics.DrawLine(borderPen, 0, height - 1, width, height - 1);

                  using (GraphicsPath roundRect = GraphicsUtility.CreateRoundRectPath(0, 0, width + 1, height, round))
                  {
                     SetControlRegion(roundRect, this);
                  }
               }
               else
               {
                  SetControlRegion(null, this);
               }
            }
         }
      }

      /// <summary>
      /// Draw buttons line
      /// </summary>
      /// <param name="e">event arg</param>
      protected virtual void DrawButtonsLine(PaintEventArgs e)
      {
         ButtonsRenderer.DrawButtonsLine(e.Graphics, _clientBounds, _buttonsPanelBounds);
      }

      /// <summary>
      /// On paint panel background
      /// </summary>
      /// <param name="e">event arg</param>
      protected virtual void OnPaintPanelBackground(PaintEventArgs e)
      {
         using (LinearGradientBrush backBrush = new LinearGradientBrush(_clientBounds, BackGradient1, BackGradient2, BackGradientMode))
         {
            e.Graphics.FillRectangle(backBrush, _clientBounds);
         }
      }

      /// <summary>
      /// Occurs when size was changed on this control
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnSizeChanged(EventArgs e)
      {
         if (ClientRectangle != _clientBounds)
         {
            _clientBounds = ClientRectangle;

            UpdateSize();
         }

         base.OnSizeChanged(e);
      }

      /// <summary>
      /// Occurs when the mouse id down
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnMouseDown(MouseEventArgs e)
      {
         _isMouseDownInTabButton = false;
         _isMovingTabButton      = false;

         if (IsMouseOverScrollBackButton)
         {
            IsMouseDownInScrollBackButton = true;
            ScrollBack();
         }
         else if (IsMouseOverScrollNextButton)
         {
            IsMouseDownInScrollNextButton = true;
            ScrollNext();
         }
         else if (_captionButtonsBounds.Contains(e.Location))
         {
            int captionIndex = ButtonsRenderer.GetCaptionButtonIndex(_captionButtonsBounds, e.Location);
            OnMouseDownInCaptionButton(captionIndex, e);
         }
         else
         {
            int index = 0;
            TabButton button = GetButtonFromPoint(e.Location, out index);
            if (button != null)
            {
               SelectedIndex = index;

               Invalidate();

               _isMouseDownInTabButton = e.Button == MouseButtons.Left;

               OnMouseDownInTabButton(button);
            }
         }


         base.OnMouseDown(e);
      }

      /// <summary>
      /// Occurs when mouse cursor is moved over the panel
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnMouseMove(MouseEventArgs e)
      {
         Cursor cursor = Cursors.Default;

         if (_isMouseDownInTabButton && e.Button == MouseButtons.Left)
         {
            cursor = Cursors.Hand;

            if (_isMovingTabButton)
            {
               TabButton selected = _buttons[SelectedIndex];
               if (selected.Contains(e.Location) == false)
               {
                  if (_isDraggingTabButton)
                  {
                     ContinueButtonDrag(e.Location, ref cursor);
                  }
                  else
                  {
                     int index = -1;
                     TabButton underMouse = GetButtonFromPoint(e.Location, true, out index);
                     if (underMouse != null)
                     {
                        bool displace  = false;
                        if (underMouse == _buttonDisplaced)
                        {
                           if (index < SelectedIndex)
                           {
                              displace = ButtonsRenderer.CanUndoDisplaceBack(underMouse, selected, e.Location);
                           }
                           else if (index > SelectedIndex)
                           {
                              displace = ButtonsRenderer.CanUndoDisplaceNext(underMouse, selected, e.Location);
                           }
                        }

                        _buttonDisplaced = underMouse;

                        if (displace)
                        {
                           _buttons[index]         = selected;
                           _buttons[SelectedIndex] = underMouse;
                           _updatePositionsOnDraw  = true;
                           SelectedIndex           = index;
                        }
                     }
                     else
                     {
                        _isDraggingTabButton = BeginButtonDrag(selected, e.Location, ref cursor);
                     }
                  }
               }
            }
            else
            {
               _isMovingTabButton   = _buttons[SelectedIndex].Contains(e.Location);
               _isDraggingTabButton = false;
            }
         }
         else if (GetButtonFromPoint(e.Location) != null)
         {
            cursor = Cursors.Hand;
         }

         CheckIfIsMouseOverScrollButtons(e.Location);
         if (IsMouseOverScrollBackButton && _firstShownButtonIndex > 0)
         {
            cursor = Cursors.Hand;
         }
         else if (IsMouseOverScrollNextButton && _canScrollNext)
         {
            cursor = Cursors.Hand;
         }

         int captionIndex = ButtonsRenderer.GetCaptionButtonIndex(_captionButtonsBounds, e.Location);
         if (captionIndex < 0 || captionIndex >= ButtonsCount)
         {
            captionIndex = -1;
         }
         else
         {
            cursor = Cursors.Hand;
         }

         if (_captionButtonIndexUnderMouse != captionIndex)
         {
            _captionButtonIndexUnderMouse = captionIndex;
            OnMouseMoveOverCaptionButton(captionIndex, e);

            Invalidate();
         }

         int buttonIndex = -1;
         TabButton buttonUnderMouse = GetButtonFromPoint(e.Location, false, out buttonIndex);
         if (buttonUnderMouse != null)
         {
            OnMouseMoveOverTabButton(buttonUnderMouse);
         }


         Cursor = cursor;

         base.OnMouseMove(e);
      }

      /// <summary>
      /// Occurs when mouse cursor is released
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnMouseUp(MouseEventArgs e)
      {
         EndMouseAction(false);

         base.OnMouseUp(e);
      }

      /// <summary>
      /// ProcessDialogKey
      /// </summary>
      /// <param name="keyData">keyData</param>
      /// <returns>processing result</returns>
      protected override bool ProcessDialogKey(Keys keyData)
      {
         if (keyData == Keys.Escape)
         {
            EndMouseAction(true);
         }

         return base.ProcessDialogKey(keyData);
      }

      /// <summary>
      /// Occurs when key was pressed inside this control
      /// </summary>
      /// <param name="e">event arg</param>
      protected override void OnKeyDown(KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Escape)
         {
            EndMouseAction(true);
         }

         base.OnKeyDown(e);
      }

      /// <summary>
      /// Occurs after pages panel size was changed
      /// </summary>
      /// <param name="bounds">bounds</param>
      protected virtual void OnPagesPanelBoundsChanged(Rectangle bounds)
      {
      }

      /// <summary>
      /// Occurs when selected index was set
      /// </summary>
      /// <param name="e">event argument</param>
      protected virtual void OnSelectedIndexSet(EventArgs e)
      {
      }

      /// <summary>
      /// Occurs when mouse is moved over caption button
      /// </summary>
      /// <param name="captionIndex">zero-based caption button index</param>
      /// <param name="e">mouse event argument</param>
      protected virtual void OnMouseMoveOverCaptionButton(int captionIndex, MouseEventArgs e)
      {
      }

      /// <summary>
      /// Occurs when mouse button was down inside caption button
      /// </summary>
      /// <param name="captionIndex">zero-based caption button index</param>
      /// <param name="e">mouse event argument</param>
      protected virtual void OnMouseDownInCaptionButton(int captionIndex, MouseEventArgs e)
      {
      }

      /// <summary>
      /// Occurs when mouse button was moved over a tab button
      /// </summary>
      /// <param name="buttonUnderMouse">button under mouse</param>
      protected virtual void OnMouseMoveOverTabButton(TabButton buttonUnderMouse)
      {
      }

      /// <summary>
      /// Occurs when mouse button was pressed while the cursor was over a tab button
      /// </summary>
      /// <param name="buttonUnderMouse">button under mouse</param>
      protected virtual void OnMouseDownInTabButton(TabButton buttonUnderMouse)
      {
      }

      /// <summary>
      /// Occurs when tab button is removed
      /// </summary>
      /// <param name="button">button removed</param>
      protected virtual void OnButtonRemoved(TabButton button)
      {
      }

      /// <summary>
      /// Occurs when tab button is added
      /// </summary>
      /// <param name="button">button added</param>
      protected virtual void OnButtonAdded(TabButton button)
      {
      }

      /// <summary>
      /// Called when tab button renderer is changed
      /// </summary>
      protected virtual void OnTabButonRendererChanged()
      {
      }

      /// <summary>
      /// Gets the margins for pages panel
      /// </summary>
      protected virtual int PagesPanelMargins
      {
         get { return 4; }
      }

      /// <summary>
      /// Gets the radius for round rect
      /// </summary>
      protected virtual int RoundRadius
      {
         get { return 3; }
      }

      /// <summary>
      /// Count of caption buttons
      /// </summary>
      protected virtual int CaptionButtonsCount
      {
         get { return 0; }
      }

      /// <summary>
      /// Draw caption buttons
      /// </summary>
      /// <param name="captionButtonsBounds">caption buttons bounds</param>
      /// <param name="graphics">graphics object</param>
      protected virtual void OnDrawCaptionButtons(Rectangle captionButtonsBounds, Graphics graphics)
      {
      }

      /// <summary>
      /// Update the positions on draw
      /// </summary>
      protected void UpdatePositions()
      {
         _updatePositionsOnDraw = true;
         Invalidate();
      }

      /// <summary>
      /// Gets the count of buttons
      /// </summary>
      protected int ButtonsCount
      {
         get { return _buttons.Count; }
      }

      /// <summary>
      /// Add button
      /// </summary>
      /// <param name="button">button</param>
      protected void AddButton(TabButton button)
      {
         InsertButton(button, _buttons.Count);
      }

      /// <summary>
      /// Insert a button 
      /// </summary>
      /// <param name="button">button to insert</param>
      /// <param name="insertIndex">zero based insert index</param>
      protected void InsertButton(TabButton button, int insertIndex)
      {
         if (button != null && insertIndex >= 0 && insertIndex <= _buttons.Count)
         {
            _buttons.Insert(insertIndex, button);
            OnButtonAdded(button);

            button.TextChanged         += OnButtonTextChanged;
            button.ExplicitDisposing   += OnButtonDisposing;

            SelectedIndex = insertIndex;

            if (_buttons.Count == 1)
            {
               using (Graphics graphics = CreateGraphics())
               {
                  UpdatePositions(graphics, true);
               }
            }
            else
            {
               _updatePositionsOnDraw = true;
            }

            UpdateSize();
            Invalidate();
         }
      }

      /// <summary>
      /// Remove button
      /// </summary>
      /// <param name="button">button</param>
      /// <returns>true if button was removed</returns>
      protected bool RemoveButton(TabButton button)
      {
         if (_buttons.Contains(button) == false)
         {
            return false;
         }

         // If the selected index is the last button index, must decrement the selected index
         // to prevent index out of range exception after the count of buttons is updated.
         if (SelectedIndex > 0 && SelectedIndex == ButtonsCount - 1)
         {
            SelectedIndex--;
         }

         _buttons.Remove(button);
         OnButtonRemoved(button);

         button.TextChanged       -= OnButtonTextChanged;
         button.ExplicitDisposing -= OnButtonDisposing;

         if (_buttons.Count == 0)
         {
            if (Disposing == false && IsDisposed == false)
            {
               using (Graphics graphics = CreateGraphics())
               {
                  UpdatePositions(graphics, false);
               }
            }
         }
         else
         {
            _updatePositionsOnDraw = true;
         }

         if (Disposing == false && IsDisposed == false)
         {
            UpdateSize();
            Invalidate();
         }

         return true;
      }

      /// <summary>
      /// Get the button at given index
      /// </summary>
      /// <param name="buttonIndex">zero based button index</param>
      /// <returns>button at given index</returns>
      protected TabButton GetButtonAt(int buttonIndex)
      {
         if (buttonIndex == -1)
         {
            return null;
         }

         return _buttons[buttonIndex];
      }

      /// <summary>
      /// Selected button
      /// </summary>
      protected TabButton SelectedButton
      {
         get { return GetButtonAt(SelectedIndex); }
         set { SelectedIndex = _buttons.IndexOf(value); }
      }

      /// <summary>
      /// Pages bounds
      /// </summary>
      protected Rectangle PagesBounds
      {
         get { return _pagesPanelBounds; }
      }

      /// <summary>
      /// Validates that current instance is not disposed
      /// </summary>
      protected void ValidateNotDisposed()
      {
         if (IsDisposed)
         {
            throw new ObjectDisposedException(GetType().Name);
         }
      }

      /// <summary>
      /// Set the region of the control
      /// </summary>
      /// <param name="newRegionPath">new region path</param>
      /// <param name="control">control</param>
      protected static void SetControlRegion(GraphicsPath newRegionPath, Control control)
      {
         Region oldRegion = control.Region;
         if (newRegionPath != null)
         {
            control.Region = new Region(newRegionPath);
         }
         else
         {
            if (control.Region == null)
            {
               return;
            }
            control.Region = null;
         }
         if (oldRegion != null)
         {
            oldRegion.Dispose();
         }
      }

      /// <summary>
      /// Cut round rect
      /// </summary>
      protected bool CutRoundRect
      {
         get { return _cutRoundRect; }
         set
         {
            if (_cutRoundRect != value)
            {
               _cutRoundRect = value;
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Update the panel size
      /// </summary>
      protected void UpdateSize()
      {
         if (IsDisposed)
         {
            return;
         }

         if (CutRoundRect)
         {
            using (GraphicsPath roundRect = GraphicsUtility.CreateRoundRectPath(0, 0, Width, Height - 1, RoundRadius))
            {
               SetControlRegion(roundRect, this);
            }
         }
         else
         {
            SetControlRegion(null, this);
         }

         using (Graphics graphics = CreateGraphics())
         {
            UpdatePositions(graphics, false);
         }
      }


      /// <summary>
      /// Begin button drag
      /// </summary>
      /// <param name="selected">selected button</param>
      /// <param name="mousePosition">mouse position</param>
      /// <param name="cursor">cursor</param>
      /// <returns>true if button drag is started</returns>
      protected virtual bool BeginButtonDrag(TabButton selected, Point mousePosition, ref Cursor cursor)
      {
         return false;
      }

      /// <summary>
      /// Continue button drag
      /// </summary>
      /// <param name="mousePosition">mouse position</param>
      /// <param name="cursor">cursor</param>
      protected virtual void ContinueButtonDrag(Point mousePosition, ref Cursor cursor)
      {
      }

      /// <summary>
      /// End button drag
      /// </summary>
      /// <param name="cancel">cancel</param>
      protected virtual void EndButtonDrag(bool cancel)
      {
      }


      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// Event received when button is disposing
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnButtonDisposing(object sender, EventArgs e)
      {
         RemoveButton((TabButton)sender);
      }

      /// <summary>
      /// Event received when button text changed
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnButtonTextChanged(object sender, EventArgs e)
      {
         _updatePositionsOnDraw = true;
         Invalidate();
      }

      /// <summary>
      /// Event received when scroll move time occurs
      /// </summary>
      /// <param name="sender">sender</param>
      /// <param name="e">event args</param>
      private void OnScrollMoveTimeOccurence(object sender, EventArgs e)
      {
         if (_hasScrolls == false)
         {
            return;
         }

         
         if (IsMouseDownInScrollBackButton && _firstShownButtonIndex > 0)
         {
            _firstShownButtonIndex--;
            Invalidate();
         }
         else if (IsMouseDownInScrollNextButton && _canScrollNext)
         {
            _firstShownButtonIndex++;
            Invalidate();
         }
      }

      #endregion Received events

      /// <summary>
      /// Get the button at given point
      /// </summary>
      /// <param name="point">point on which the button is searched</param>
      /// <param name="permisive">permisive search</param>
      /// <param name="buttonIndex">zero based button index if found, -1 if not found</param>
      /// <returns>button at given point or null</returns>
      private TabButton GetButtonFromPoint(Point point, bool permisive, out int buttonIndex)
      {
         if (_buttonsPanelBounds.Contains(point))
         {
            if (permisive)
            {
               int x = _buttonsPanelBounds.Left;
               if (_buttons.Count > 0)
               {
                  x = _buttons[0].Left - 4;
               }

               Rectangle bounds = new Rectangle();
               bounds.Y         = _buttonsPanelBounds.Y - 4;
               bounds.Height    = _buttonsPanelBounds.Height + 8;
               for (int index   = 0; index < _buttons.Count; index++)
               {
                  bounds.X      = x;
                  bounds.Width  = _buttons[index].Width + 4;

                  if (bounds.Contains(point))
                  {
                     buttonIndex = index;
                     return _buttons[index];
                  }

                  x = bounds.Right;
               }
            }
            else
            {
               for (int index = 0; index < _buttons.Count; index++)
               {
                  if (_buttons[index].Contains(point))
                  {
                     buttonIndex = index;
                     return _buttons[index];
                  }
               }
            }
         }

         buttonIndex = -1;
         return null;
      }

      /// <summary>
      /// Get the button at given point
      /// </summary>
      /// <param name="point">point on which the button is searched</param>
      /// <param name="buttonIndex">zero based button index if found, -1 if not found</param>
      /// <returns>button at given point or null</returns>
      private TabButton GetButtonFromPoint(Point point, out int buttonIndex)
      {
         return GetButtonFromPoint(point, false, out buttonIndex);
      }

      /// <summary>
      /// Get the button at given point
      /// </summary>
      /// <param name="point">point on which the button is searched</param>
      /// <returns>button at given point or null</returns>
      private TabButton GetButtonFromPoint(Point point)
      {
         int buttonIndex = -1;
         return GetButtonFromPoint(point, out buttonIndex);
      }

      /// <summary>
      /// Check if the mouse is over scroll button
      /// </summary>
      /// <param name="location">mouse location</param>
      private void CheckIfIsMouseOverScrollButtons(Point location)
      {
         if (_scrollBackBounds.Contains(location))
         {
            IsMouseOverScrollBackButton = true;
            IsMouseOverScrollNextButton = false;
         }
         else if (_scrollNextBounds.Contains(location))
         {
            IsMouseOverScrollBackButton = false;
            IsMouseOverScrollNextButton = true;
         }
         else
         {
            IsMouseOverScrollBackButton = false;
            IsMouseOverScrollNextButton = false;
         }
      }

      /// <summary>
      /// Update positions on panel
      /// </summary>
      /// <param name="graphics">graphics object</param>
      /// <param name="forceChangeNotification">force change notification</param>
      private void UpdatePositions(Graphics graphics, bool forceChangeNotification)
      {
         Rectangle bounds           = new Rectangle();
         Rectangle pagesPanelBounds = _clientBounds;

         int buttonsCount = _buttons.Count;
         if (buttonsCount == 1 && ShowOneTabButton == false)
         {
            buttonsCount  = 0;
         }

         if (buttonsCount != 0)
         {
            bounds = ButtonsRenderer.GetFirstButtonBounds(_clientBounds, false, 0, _buttons[0].Text, Font, _buttons[0].PageIcon, graphics);
            _buttons[0].SetBounds(bounds);

            for (int index = 1; index < _buttons.Count; index++)
            {
               bounds = ButtonsRenderer.GetNextButtonBounds(bounds, _buttons[index].Text, Font, _buttons[index].PageIcon, graphics);
               _buttons[index].SetBounds(bounds);
            }

            _buttonsPanelBounds = ButtonsRenderer.GetButtonsClipRectangle(_clientBounds, false, CaptionButtonsCount);

            _hasScrolls = ButtonsRenderer.HasScroll(_buttonsPanelBounds, bounds);
            if (_hasScrolls)
            {
               _buttonsPanelBounds = ButtonsRenderer.GetButtonsClipRectangle(_clientBounds, true, CaptionButtonsCount);
            }

            if (_hasScrolls)
            {
               int scrollPos = ButtonsRenderer.GetScrollPos(_buttons, _firstShownButtonIndex);

               bounds = ButtonsRenderer.GetFirstButtonBounds(_clientBounds, true, scrollPos, _buttons[0].Text, Font, _buttons[0].PageIcon, graphics);
               _buttons[0].SetBounds(bounds);

               for (int index = 1; index < _buttons.Count; index++)
               {
                  bounds = ButtonsRenderer.GetNextButtonBounds(bounds, _buttons[index].Text, Font, _buttons[index].PageIcon, graphics);
                  _buttons[index].SetBounds(bounds);
               }

               _scrollBackBounds = ButtonsRenderer.GetScrollBackButtonBounds(_buttonsPanelBounds);
               _scrollNextBounds = ButtonsRenderer.GetScrollNextButtonBounds(_buttonsPanelBounds);

               _canScrollNext    = ButtonsRenderer.CanScrollNext(_buttons[ButtonsCount - 1], _scrollNextBounds) && _firstShownButtonIndex < ButtonsCount - 1;
            }

            pagesPanelBounds = ButtonsRenderer.GetClientRectangle(pagesPanelBounds);
         }

         _captionButtonsBounds = ButtonsRenderer.GetCaptionButtonsRectangle(_clientBounds, CaptionButtonsCount);

         if (_pagesPanelBounds != pagesPanelBounds || forceChangeNotification)
         {
            _pagesPanelBounds   = pagesPanelBounds;
            pagesPanelBounds.Height++;

            //pagesPanelBounds.Inflate(-PagesPanelMargins, -PagesPanelMargins);

            OnPagesPanelBoundsChanged(pagesPanelBounds);
         }
      }

      /// <summary>
      /// Accessor of flag indicating if the mouse cursor is over scroll back button
      /// </summary>
      private bool IsMouseOverScrollBackButton
      {
         get { return _isMouseOverScrollBackButton; }
         set
         {
            if (_isMouseOverScrollBackButton != value)
            {
               _isMouseOverScrollBackButton = value;

               if (IsMouseOverScrollBackButton == false)
               {
                  IsMouseDownInScrollBackButton = false;
               }

               Invalidate();
            }
         }
      }

      /// <summary>
      /// Accessor of flag indicating if the mouse cursor is over scroll next button
      /// </summary>
      private bool IsMouseOverScrollNextButton
      {
         get { return _isMouseOverScrollNextButton; }
         set
         {
            if (_isMouseOverScrollNextButton != value)
            {
               _isMouseOverScrollNextButton = value;

               if (IsMouseOverScrollNextButton == false)
               {
                  IsMouseDownInScrollNextButton = false;
               }

               Invalidate();
            }
         }
      }

      /// <summary>
      /// Accessor of flag indicating if the mouse left button is down when the cursor is over scroll back button
      /// </summary>
      private bool IsMouseDownInScrollBackButton
      {
         get { return _isMouseDownInScrollBackButton; }
         set
         {
            if (_isMouseDownInScrollBackButton != value)
            {
               _isMouseDownInScrollBackButton = value;
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Accessor of flag indicating if the mouse left button is down when the cursor is over scroll next button
      /// </summary>
      private bool IsMouseDownInScrollNextButton
      {
         get { return _isMouseDownInScrollNextButton; }
         set
         {
            if (_isMouseDownInScrollNextButton != value)
            {
               _isMouseDownInScrollNextButton = value;
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Scroll next
      /// </summary>
      private void ScrollNext()
      {
         if (_hasScrolls == false)
         {
            return;
         }

         if (IsMouseDownInScrollNextButton && _canScrollNext)
         {
            _firstShownButtonIndex++;
            _updatePositionsOnDraw = true;
            Invalidate();
         }
      }

      /// <summary>
      /// Scroll back
      /// </summary>
      private void ScrollBack()
      {
         if (_hasScrolls == false)
         {
            return;
         }


         if (IsMouseDownInScrollBackButton && _firstShownButtonIndex > 0)
         {
            _firstShownButtonIndex--;
            _updatePositionsOnDraw = true;
            Invalidate();
         }
      }

      /// <summary>
      /// End mouse action
      /// </summary>
      /// <param name="cancel">cancel</param>
      private void EndMouseAction(bool cancel)
      {
         _isMouseDownInTabButton = false;

         if (_isMovingTabButton)
         {
            if (_isDraggingTabButton)
            {
               EndButtonDrag(cancel);
            }

            _isMovingTabButton = false;
            _buttonDisplaced = null;
         }

         IsMouseDownInScrollBackButton = false;
         IsMouseDownInScrollNextButton = false;
      }

      #endregion Private section
   }
}
