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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Decorator for the group of dock buttons which will be placed in the center of the dock view
   /// </summary>
   internal sealed class CenterDockButtons : Disposable
   {
      #region Fields

      private Rectangle                _rightButtonBounds      = new Rectangle();
      private Rectangle                _leftButtonBounds       = new Rectangle();
      private Rectangle                _topButtonBounds        = new Rectangle();
      private Rectangle                _bottomButtonBounds     = new Rectangle();
      private Rectangle                _fillButtonBounds       = new Rectangle();

      private CenterButtonToDockFill   _dockFillGuider         = new CenterButtonToDockFill();
      private CenterButtonToDockRight  _dockRightGuider        = new CenterButtonToDockRight();
      private CenterButtonToDockBottom _dockBottomGuider       = new CenterButtonToDockBottom();
      private CenterButtonToDockLeft   _dockLeftGuider         = new CenterButtonToDockLeft();
      private CenterButtonToDockUp     _dockTopGuider          = new CenterButtonToDockUp();

      private Rectangle                _guiderBounds           = new Rectangle();

      private Rectangle                _lastViewRectangle      = new Rectangle();
      private zAllowedDock             _lastAllowedDockMode    = zAllowedDock.None;

      private FormWrapper              _host                   = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="host">host</param>
      public CenterDockButtons(FormWrapper host)
      {
         _host = host;

         _leftButtonBounds.Size     = _dockLeftGuider.Size;
         _rightButtonBounds.Size    = _dockRightGuider.Size;
         _topButtonBounds.Size      = _dockTopGuider.Size;
         _bottomButtonBounds.Size   = _dockBottomGuider.Size;
         _fillButtonBounds.Size     = _dockFillGuider.FillSize;

         _dockLeftGuider.Visible    = false;
         _dockRightGuider.Visible   = false;
         _dockTopGuider.Visible     = false;
         _dockBottomGuider.Visible  = false;
         _dockFillGuider.Visible    = false;

         _dockLeftGuider.Text       = "inner left button";
         _dockRightGuider.Text      = "inner right button";
         _dockTopGuider.Text        = "inner top button";
         _dockBottomGuider.Text     = "inner bottom button";
         _dockFillGuider.Text       = "inner fill button";

         _host.AddFirst(_dockLeftGuider);
         _host.AddFirst(_dockRightGuider);
         _host.AddFirst(_dockTopGuider);
         _host.AddFirst(_dockBottomGuider);
         _host.AddFirst(_dockFillGuider);

         _host.KeepFirst[4] = _dockLeftGuider;
         _host.KeepFirst[5] = _dockRightGuider;
         _host.KeepFirst[6] = _dockTopGuider;
         _host.KeepFirst[7] = _dockBottomGuider;
         _host.KeepFirst[8] = _dockFillGuider;

         _host.AddToIgnoreOnGetChildFromPoint(_dockLeftGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockRightGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockTopGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockBottomGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockFillGuider);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Gets the dock for given point
      /// </summary>
      /// <param name="screenPoint">screen point</param>
      /// <returns></returns>
      public DockStyle GetDockAtPoint(Point screenPoint)
      {
         ValidateNotDisposed();

         Point clientPoint = _host.PointToClient(screenPoint);

         if (_dockFillGuider.Bounds.Contains(clientPoint) && _dockFillGuider.Visible)
         {
            return DockStyle.Fill;
         }

         if (_dockLeftGuider.Bounds.Contains(clientPoint) && _dockLeftGuider.Visible)
         {
            return DockStyle.Left;
         }

         if (_dockRightGuider.Bounds.Contains(clientPoint) && _dockRightGuider.Visible)
         {
            return DockStyle.Right;
         }

         if (_dockTopGuider.Bounds.Contains(clientPoint) && _dockTopGuider.Visible)
         {
            return DockStyle.Top;
         }

         if (_dockBottomGuider.Bounds.Contains(clientPoint) && _dockBottomGuider.Visible)
         {
            return DockStyle.Bottom;
         }

         return DockStyle.None;
      }

      /// <summary>
      /// Bounds of the guider
      /// </summary>
      public Rectangle GuiderBounds
      {
         get
         {
            ValidateNotDisposed();

            return _guiderBounds;
         }
      }

      /// <summary>
      /// bounds for the left button
      /// </summary>
      public Rectangle LeftButtonBounds
      {
         get
         {
            ValidateNotDisposed();

            return _leftButtonBounds;
         }
      }

      /// <summary>
      /// bounds for the right button
      /// </summary>
      public Rectangle RightButtonBounds
      {
         get
         {
            ValidateNotDisposed();

            return _rightButtonBounds;
         }
      }

      /// <summary>
      /// bounds for the top button
      /// </summary>
      public Rectangle TopButtonBounds
      {
         get
         {
            ValidateNotDisposed();

            return _topButtonBounds;
         }
      }

      /// <summary>
      /// bounds for the bottom button
      /// </summary>
      public Rectangle BottomButtonBounds
      {
         get
         {
            ValidateNotDisposed();

            return _bottomButtonBounds;
         }
      }

      /// <summary>
      /// bounds for the fill button
      /// </summary>
      public Rectangle FillButtonBounds
      {
         get
         {
            ValidateNotDisposed();

            return _fillButtonBounds;
         }
      }

      /// <summary>
      /// Returns true if the guider is visible
      /// </summary>
      public bool IsVisible
      {
         get { return _dockFillGuider.Visible; }
      }

      /// <summary>
      /// Update the bounds of the buttons
      /// </summary>
      /// <param name="allowedDockMode">allowed dock mode</param>
      /// <param name="viewScreenRectangle">view rectanlge in the center of which the buttons to dock will be shown (in screen coordinates)</param>
      public void UpdateGuiderBounds(zAllowedDock allowedDockMode, Rectangle viewScreenRectangle)
      {
         ValidateNotDisposed();

         Rectangle viewRectangle = _host.RectangleToClient(viewScreenRectangle);

         if (_lastAllowedDockMode == allowedDockMode && _lastViewRectangle == viewRectangle)
         {
            return;
         }

         _lastViewRectangle   = viewRectangle;
         _lastAllowedDockMode = allowedDockMode;

         int width    = viewRectangle.Width;
         int height   = viewRectangle.Height;
         Point center = new Point(width / 2, height / 2);
         Point offset = viewRectangle.Location;

         Point fillPosition      = new Point(center.X       - _dockFillGuider.Width / 2,        center.Y       - _dockFillGuider.Height / 2);

         Point leftPosition      = new Point(fillPosition.X - _dockLeftGuider.Width + 7,        center.Y       - _dockLeftGuider.Height / 2);
         Point topPosition       = new Point(center.X       - _dockTopGuider.Width / 2 - 1,     fillPosition.Y - _dockTopGuider.Height + 7);
         Point rightPosition     = new Point(fillPosition.X + _dockFillGuider.Width - 7,        center.Y       - _dockRightGuider.Height / 2);
         Point bottomPosition    = new Point(center.X       - _dockBottomGuider.Width / 2,      fillPosition.Y + _dockBottomGuider.Height + 7);

         fillPosition.Offset  (offset);
         leftPosition.Offset  (offset);
         rightPosition.Offset (offset);
         topPosition.Offset   (offset);
         bottomPosition.Offset(offset);

         _dockLeftGuider.Location      = leftPosition;
         _dockRightGuider.Location     = rightPosition;
         _dockTopGuider.Location       = topPosition;
         _dockBottomGuider.Location    = bottomPosition;
         _dockFillGuider.Location      = fillPosition;

         bool dockLeftGuiderVisible          = EnumUtility.Contains (allowedDockMode, zAllowedDock.Left);
         bool dockRightGuiderVisible         = EnumUtility.Contains (allowedDockMode, zAllowedDock.Right);
         bool dockTopGuiderVisible           = EnumUtility.Contains (allowedDockMode, zAllowedDock.Top);
         bool dockBottomGuiderVisible        = EnumUtility.Contains (allowedDockMode, zAllowedDock.Bottom);
         bool dockFillGuiderShowFillPreview  = EnumUtility.Contains (allowedDockMode, zAllowedDock.Fill);


         _guiderBounds.X = _dockFillGuider.Left;
         if (dockLeftGuiderVisible)
         {
            _guiderBounds.X = _dockLeftGuider.Left;
         }

         _guiderBounds.Y = _dockFillGuider.Top;
         if (dockTopGuiderVisible)
         {
            _guiderBounds.Y = _dockTopGuider.Top;
         }

         int right = _dockFillGuider.Right;
         if (dockRightGuiderVisible)
         {
            right = _dockRightGuider.Right;
         }
         _guiderBounds.Width = right - _guiderBounds.Left;

         int bottom = _dockFillGuider.Bottom;
         if (dockBottomGuiderVisible)
         {
            bottom = _dockBottomGuider.Bottom;
         }
         _guiderBounds.Height = bottom - _guiderBounds.Top;


         _leftButtonBounds.Location    = leftPosition;
         _rightButtonBounds.Location   = rightPosition;
         _topButtonBounds.Location     = topPosition;
         _bottomButtonBounds.Location  = bottomPosition;

         _fillButtonBounds = _dockFillGuider.FillBounds;
         _fillButtonBounds.X += _dockFillGuider.Left;
         _fillButtonBounds.Y += _dockFillGuider.Top;
      }

      /// <summary>
      /// Shows the buttons to dock in the center of the view rectangle
      /// </summary>
      /// <param name="allowedDockMode">allowed dock mode</param>
      /// <param name="viewScreenRectangle">view rectanlge in the center of which the buttons to dock will be shown (in screen coordinates)</param>
      public void Show(zAllowedDock allowedDockMode, Rectangle viewScreenRectangle)
      {
         ValidateNotDisposed();

         UpdateGuiderBounds(allowedDockMode, viewScreenRectangle);

         _dockLeftGuider.Visible    = EnumUtility.Contains(allowedDockMode, zAllowedDock.Left);
         _dockRightGuider.Visible   = EnumUtility.Contains(allowedDockMode, zAllowedDock.Right);
         _dockTopGuider.Visible     = EnumUtility.Contains(allowedDockMode, zAllowedDock.Top);
         _dockBottomGuider.Visible  = EnumUtility.Contains(allowedDockMode, zAllowedDock.Bottom);

         _dockFillGuider.Visible    = true;
         _dockFillGuider.ShowFillPreview = EnumUtility.Contains(allowedDockMode, zAllowedDock.Fill);
      }

      /// <summary>
      /// Hide the dock buttons
      /// </summary>
      public void Hide()
      {
         ValidateNotDisposed();

         _dockBottomGuider.Visible  = false;;
         _dockLeftGuider.Visible    = false;;
         _dockRightGuider.Visible   = false;;
         _dockTopGuider.Visible     = false;;
         _dockFillGuider.Visible    = false;;
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose the current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">dispose was called from IDisposable.Dispose</param>
      protected override void Dispose(bool fromIDisposableDispose)
      {
         if (fromIDisposableDispose)
         {
            _dockFillGuider.Dispose();
            _dockFillGuider = null;

            _dockRightGuider.Dispose();
            _dockRightGuider = null;

            _dockBottomGuider.Dispose();
            _dockBottomGuider = null;

            _dockLeftGuider.Dispose();
            _dockLeftGuider = null;

            _dockTopGuider.Dispose();
            _dockTopGuider = null;
         }
      }

      #endregion Protected section
   }
}
