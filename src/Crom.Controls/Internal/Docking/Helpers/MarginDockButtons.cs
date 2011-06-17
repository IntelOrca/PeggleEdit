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

using System.Drawing;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Decorator for the group of dock buttons which will be placed on the sides of the dock view
   /// </summary>
   internal class MarginDockButtons : Disposable
   {
      #region Fields

      private Rectangle                _rightButtonBounds      = new Rectangle();
      private Rectangle                _leftButtonBounds       = new Rectangle();
      private Rectangle                _topButtonBounds        = new Rectangle();
      private Rectangle                _bottomButtonBounds     = new Rectangle();

      private ButtonToDockRight        _dockRightGuider        = new ButtonToDockRight();
      private ButtonToDockBottom       _dockBottomGuider       = new ButtonToDockBottom();
      private ButtonToDockLeft         _dockLeftGuider         = new ButtonToDockLeft();
      private ButtonToDockUp           _dockTopGuider          = new ButtonToDockUp();

      private FormWrapper              _host                   = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="host">host</param>
      public MarginDockButtons(FormWrapper host)
      {
         _host = host;

         _leftButtonBounds.Size     = _dockLeftGuider.Size;
         _rightButtonBounds.Size    = _dockRightGuider.Size;
         _topButtonBounds.Size      = _dockTopGuider.Size;
         _bottomButtonBounds.Size   = _dockBottomGuider.Size;

         _dockLeftGuider.Visible    = false;
         _dockRightGuider.Visible   = false;
         _dockTopGuider.Visible     = false;
         _dockBottomGuider.Visible  = false;

         _dockLeftGuider.Text       = "outer left button";
         _dockRightGuider.Text      = "outer right button";
         _dockTopGuider.Text        = "outer top button";
         _dockBottomGuider.Text     = "outer bottom button";

         _host.AddFirst(_dockLeftGuider);
         _host.AddFirst(_dockRightGuider);
         _host.AddFirst(_dockTopGuider);
         _host.AddFirst(_dockBottomGuider);

         _host.KeepFirst[0] = _dockLeftGuider;
         _host.KeepFirst[1] = _dockRightGuider;
         _host.KeepFirst[2] = _dockTopGuider;
         _host.KeepFirst[3] = _dockBottomGuider;

         _host.AddToIgnoreOnGetChildFromPoint(_dockLeftGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockRightGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockTopGuider);
         _host.AddToIgnoreOnGetChildFromPoint(_dockBottomGuider);
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
      /// Update the bounds of the buttons
      /// </summary>
      public void UpdateButtonsBounds()
      {
         ValidateNotDisposed();

         int width    = _host.ClientSize.Width;
         int height   = _host.ClientSize.Height;
         Point center = new Point(width / 2, height / 2);

         Point leftPosition      = new Point(24,                                       center.Y - _dockLeftGuider.Height / 2);
         Point topPosition       = new Point(center.X - _dockTopGuider.Width / 2,      24);
         Point rightPosition     = new Point(width    - _dockRightGuider.Width - 24,   center.Y - _dockRightGuider.Height / 2);
         Point bottomPosition    = new Point(center.X - _dockBottomGuider.Width / 2,   height   - _dockBottomGuider.Height - 24);

         _leftButtonBounds.Location    = leftPosition;
         _rightButtonBounds.Location   = rightPosition;
         _topButtonBounds.Location     = topPosition;
         _bottomButtonBounds.Location  = bottomPosition;
      }


      /// <summary>
      /// Shows the buttons to dock in the center of the view rectangle
      /// </summary>
      /// <param name="allowedDockMode">allowed dock mode</param>
      /// <param name="viewScreenRectangle">view rectangle in which the buttons to dock will be shown (in screen coordinates)</param>
      public void Show(zAllowedDock allowedDockMode)
      {
         UpdateButtonsBounds();

         _dockLeftGuider.Location      = _leftButtonBounds.Location;
         _dockRightGuider.Location     = _rightButtonBounds.Location;
         _dockTopGuider.Location       = _topButtonBounds.Location;
         _dockBottomGuider.Location    = _bottomButtonBounds.Location;

         if (EnumUtility.Contains(allowedDockMode, zAllowedDock.Left))
         {
            _dockLeftGuider.Visible = true;;
         }
         if (EnumUtility.Contains(allowedDockMode, zAllowedDock.Right))
         {
            _dockRightGuider.Visible = true;
         }
         if (EnumUtility.Contains(allowedDockMode, zAllowedDock.Top))
         {
            _dockTopGuider.Visible = true;
         }
         if (EnumUtility.Contains(allowedDockMode, zAllowedDock.Bottom))
         {
            _dockBottomGuider.Visible = true;;
         }
      }

      /// <summary>
      /// Hide the dock buttons
      /// </summary>
      public void Hide()
      {
         ValidateNotDisposed();

         _dockBottomGuider.Hide();
         _dockLeftGuider.Hide();
         _dockRightGuider.Hide();
         _dockTopGuider.Hide();
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
