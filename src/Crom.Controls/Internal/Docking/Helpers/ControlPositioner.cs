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
   /// Implementation of control positioner
   /// </summary>
   internal class ControlPositioner : Disposable
   {
      #region Fields

      private Control               _control          = null;
      private bool                  _canSizeLeft      = true;
      private bool                  _canSizeRight     = true;
      private bool                  _canSizeTop       = true;
      private bool                  _canSizeBottom    = true;
      private bool                  _canMove          = true;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="control">control</param>
      public ControlPositioner(Control control)
      {
         _control = control;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Begin move by mouse
      /// </summary>
      public event EventHandler BeginMoveByMouse;

      /// <summary>
      /// Move by mouse
      /// </summary>
      public event EventHandler MoveByMouse;

      /// <summary>
      /// End move by mouse
      /// </summary>
      public event EventHandler EndMoveByMouse;


      /// <summary>
      /// Occurs when CanSizeBottom property was changed
      /// </summary>
      public event EventHandler CanMoveChanged;

      /// <summary>
      /// Occurs when CanSizeLeft property was changed
      /// </summary>
      public event EventHandler CanSizeLeftChanged;

      /// <summary>
      /// Occurs when CanSizeRight property was changed
      /// </summary>
      public event EventHandler CanSizeRightChanged;

      /// <summary>
      /// Occurs when CanSizeTop property was changed
      /// </summary>
      public event EventHandler CanSizeTopChanged;

      /// <summary>
      /// Occurs when CanSizeBottom property was changed
      /// </summary>
      public event EventHandler CanSizeBottomChanged;


      /// <summary>
      /// Can Move flag
      /// </summary>
      public bool CanMove
      {
         get 
         {
            ValidateNotDisposed();

            return _canMove; 
         }
         set 
         {
            ValidateNotDisposed();

            if (_canMove != value)
            {
               _canMove = value;

               EventHandler handler = CanMoveChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Can Size-Left flag
      /// </summary>
      public bool CanSizeLeft
      {
         get
         {
            ValidateNotDisposed();

            return _canSizeLeft;
         }
         set
         {
            ValidateNotDisposed();

            if (_canSizeLeft != value)
            {
               _canSizeLeft = value;

               EventHandler handler = CanSizeLeftChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Can Size-Right flag
      /// </summary>
      public bool CanSizeRight
      {
         get
         {
            ValidateNotDisposed();

            return _canSizeRight;
         }
         set
         {
            ValidateNotDisposed();

            if (_canSizeRight != value)
            {
               _canSizeRight = value;

               EventHandler handler = CanSizeRightChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Can Size-Top flag
      /// </summary>
      public bool CanSizeTop
      {
         get
         {
            ValidateNotDisposed();

            return _canSizeTop;
         }
         set
         {
            ValidateNotDisposed();

            if (_canSizeTop != value)
            {
               _canSizeTop = value;

               EventHandler handler = CanSizeTopChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Can Size-Bottom flag
      /// </summary>
      public bool CanSizeBottom
      {
         get
         {
            ValidateNotDisposed();

            return _canSizeBottom;
         }
         set
         {
            ValidateNotDisposed();

            if (_canSizeBottom != value)
            {
               _canSizeBottom = value;

               EventHandler handler = CanSizeBottomChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Location
      /// </summary>
      public Point Location
      {
         get
         {
            ValidateNotDisposed();

            return _control.Location;
         }
         set
         {
            ValidateNotDisposed();

            _control.Location = value;
         }
      }

      /// <summary>
      /// Size
      /// </summary>
      public Size Size
      {
         get
         {
            ValidateNotDisposed();

            return _control.Size;
         }
         set
         {
            ValidateNotDisposed();

            _control.Size = value;
         }
      }

      /// <summary>
      /// Bounds
      /// </summary>
      public Rectangle Bounds
      {
         get
         {
            ValidateNotDisposed();

            return _control.Bounds;
         }
         set
         {
            ValidateNotDisposed();

            _control.Bounds = value;
         }
      }



      /// <summary>
      /// Start move by mouse
      /// </summary>
      public void StartMoveByMouse()
      {
         ValidateNotDisposed();

         if (BeginMoveByMouse != null)
         {
            BeginMoveByMouse(_control, EventArgs.Empty);
         }
      }

      /// <summary>
      /// Perform move by mouse
      /// </summary>
      /// <param name="x">x</param>
      /// <param name="y">y</param>
      public void PerformMoveByMouse(int x, int y)
      {
         ValidateNotDisposed();

         Location = new Point(x, y);

         if (MoveByMouse != null)
         {
            MoveByMouse(_control, EventArgs.Empty);
         }
      }

      /// <summary>
      /// Stop move by mouse
      /// </summary>
      public void StopMoveByMouse()
      {
         ValidateNotDisposed();

         if (EndMoveByMouse != null)
         {
            EndMoveByMouse(_control, EventArgs.Empty);
         }
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
            _control = null;
         }
      }

      #endregion Protected section
   }
}
