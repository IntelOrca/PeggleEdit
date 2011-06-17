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
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Helper class which provide information about forms
   /// </summary>
   internal class FormWrapper : Disposable
   {
      #region Fields

      private IntPtr                   _hostHandle             = IntPtr.Zero;
      private Control                  _host                   = null;
      private Control[]                _keepLast               = null;
      private Control[]                _keepFirst              = null;
      private List<Control>            _ignoreOnGetFromPoint   = new List<Control>();

      #endregion Fields

      #region Instance

      /// <summary>
      /// Light-constructor
      /// </summary>
      /// <param name="control">constructor</param>
      private FormWrapper(Control control)
      {
         _hostHandle = control.Handle;
         _host       = control;
         _keepFirst  = new Control[0];
         _keepLast   = new Control[0];
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="window">window object</param>
      /// <param name="keepFirstCount">number of controls to keep first</param>
      /// <param name="keepLastCount">number of controls to keep last</param>
      public FormWrapper(IWin32Window window, int keepFirstCount, int keepLastCount)
      {
         _hostHandle = window.Handle;
         _keepFirst  = new Control[keepFirstCount];
         _keepLast   = new Control[keepLastCount];

         // Just for now only allow .Net control
         // TODO: implement bounds change detection for any window
         _host = window as Control;
         if (_host == null)
         {
            throw new ArgumentException("In this version are allowed only .NET controls as hosts for docking");
         }
         _host.SizeChanged     += OnHostSizeChanged;
         _host.Move            += OnHostMoved;
         _host.VisibleChanged  += OnHostVisibleChanged;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Event raised when form size was changed
      /// </summary>
      public event EventHandler SizeChanged;

      /// <summary>
      /// Event raised when form was moved
      /// </summary>
      public event EventHandler Move;

      /// <summary>
      /// Event raised when form visibility was changed
      /// </summary>
      public event EventHandler VisibleChanged;

      /// <summary>
      /// Checks if the form is visible
      /// </summary>
      public bool Visible
      {
         get
         {
            ValidateNotDisposed();

            return _host.Visible;// IsWindowVisible(_hostHandle);
         }
      }

      /// <summary>
      /// Accessor for the form handle
      /// </summary>
      public IntPtr Handle
      {
         get
         {
            ValidateNotDisposed();

            return _hostHandle;
         }
      }

      /// <summary>
      /// Accessor for the form text
      /// </summary>
      public string Text
      {
         get
         {
            ValidateNotDisposed();

            return _host.Text;
         }
         set
         {
            ValidateNotDisposed();

            _host.Text = value;
         }
      }

      /// <summary>
      /// Accessor for the form width
      /// </summary>
      public int Width
      {
         get
         {
            ValidateNotDisposed();

            return _host.Width;
         }
      }

      /// <summary>
      /// Accessor for the form height
      /// </summary>
      public int Height
      {
         get
         {
            ValidateNotDisposed();

            return _host.Height;
         }
      }

      /// <summary>
      /// Accessor for host client size
      /// </summary>
      public Size ClientSize
      {
         get { return _host.ClientSize; }
      }

      /// <summary>
      /// Rectangle to screen coordinates
      /// </summary>
      /// <param name="clientRectangle">client rectangle</param>
      /// <returns>rectangle in screen coordinates</returns>
      public Rectangle RectangleToScreen(Rectangle clientRectangle)
      {
         return _host.RectangleToScreen(clientRectangle);
      }

      /// <summary>
      /// Rectangle to client coordinates
      /// </summary>
      /// <param name="screenRectangle">screen rectangle</param>
      /// <returns>rectangle in client coordinates</returns>
      public Rectangle RectangleToClient(Rectangle screenRectangle)
      {
         return _host.RectangleToClient(screenRectangle);
      }

      /// <summary>
      /// Converts a screen location into client location
      /// </summary>
      /// <param name="screenLocation">screen location</param>
      /// <returns>client location</returns>
      public Point PointToClient(Point screenLocation)
      {
         return _host.PointToClient(screenLocation);
      }

      /// <summary>
      /// Client rectangle in screen coordinates
      /// </summary>
      public Rectangle ScreenClientRectangle
      {
         get
         {
            return _host.RectangleToScreen(_host.ClientRectangle);
         }
      }

      /// <summary>
      /// Gets the count of child controls
      /// </summary>
      public int ControlsCount
      {
         get { return _host.Controls.Count; }
      }

      /// <summary>
      /// Gets the child control at given index
      /// </summary>
      /// <param name="index">zero based index</param>
      /// <returns>child control at given index</returns>
      public Control GetControlAt(int index)
      {
         return _host.Controls[index];
      }

      /// <summary>
      /// Add control to host
      /// </summary>
      /// <param name="control">control to add</param>
      public void AddLast(Control control)
      {
         _host.Controls.Add(control);

         int index = GetFirstIndex(KeepLast, control);

         SetChildIndex(control, index);
      }

      /// <summary>
      /// Add control to host
      /// </summary>
      /// <param name="control">control to add</param>
      public void AddFirst(Control control)
      {
         int index = GetLastIndex(KeepFirst, control);

         _host.Controls.Add(control);
         SetChildIndex(control, index);
      }

      /// <summary>
      /// Move the control first
      /// </summary>
      /// <param name="controlToMove">control to be moved first</param>
      public void MoveFirst(Control controlToMove)
      {
         int index = GetLastIndex(KeepFirst, controlToMove);

         SetChildIndex(controlToMove, index);
      }

      /// <summary>
      /// Move the control last in zorder 
      /// </summary>
      /// <param name="controlToMove">form to be moved last</param>
      public void MoveLast(Control controlToMove)
      {
         int index = GetFirstIndex(KeepLast, controlToMove);

         SetChildIndex(controlToMove, index);
      }

      /// <summary>
      /// Remove form from host
      /// </summary>
      /// <param name="form">form</param>
      public void Remove(Control form)
      {
         _host.Controls.Remove(form);
      }

      /// <summary>
      /// Get ZOrder index
      /// </summary>
      /// <param name="form">form for which ZOrder is requested</param>
      /// <returns>ZOrder index</returns>
      public int GetZOrderIndex(Control form)
      {
         return _host.Controls.GetChildIndex(form);
      }

      /// <summary>
      /// Check if the host contains this control
      /// </summary>
      /// <param name="control">control</param>
      /// <returns>true if the host contains control</returns>
      public bool Contains(Control control)
      {
         return _host.Controls.Contains(control);
      }

      /// <summary>
      /// Gets the child leaf window found at given screen location
      /// </summary>
      /// <param name="screenPoint">point in screen coordinates</param>
      /// <param name="ignored">control to be ignored from search</param>
      /// <returns>found dockable container</returns>
      public DockableContainer GetLeafDockedContainerFromPoint(Point screenPoint, Control ignored)
      {
         for (int childIndex = 0; childIndex < ControlsCount; childIndex++)
         {
            Control child = GetControlAt(childIndex);

            if (child == ignored)
            {
               continue;
            }

            if (child.Visible == false)
            {
               continue;
            }

            DockableContainer container = child as DockableContainer;
            if (container == null)
            {
               continue;
            }

            if (container.RectangleToScreen(container.ClientRectangle).Contains(screenPoint) == false)
            {
               continue;
            }

            if (container.SingleChild != null)
            {
               if (container.SingleChild.IsDocked == false)
               {
                  continue;
               }

               return container;
            }

            return GetLeafDockedContainerFromPoint(container, screenPoint);
         }

         return null;
      }



      /// <summary>
      /// Gets the fill rectangle based on a point
      /// </summary>
      /// <param name="screenPoint">point in screen coordinates</param>
      /// <returns>rectangle from point</returns>
      public static Rectangle GetFillRectangleFromPoint(Point screenPoint, Control underPoint, FormWrapper host)
      {
         if (underPoint == null)
         {
            if (host != null)
            {
               return GetFillScreenRectangle(host);
            }

            return new Rectangle();
         }

         Rectangle result  = GetFillScreenRectangle(underPoint);
         if (result.Contains(screenPoint) == false)
         {
            return new Rectangle();
         }

         return result;
      }


      /// <summary>
      /// GEt fill screen rectangle
      /// </summary>
      /// <param name="wrapper">wrapper</param>
      /// <returns></returns>
      public static Rectangle GetFillScreenRectangle(FormWrapper wrapper)
      {
         Rectangle clientArea  = wrapper.RectangleToClient(GetScreenClientRectangle(wrapper._host));

         int x1 = clientArea.Left;
         int x2 = clientArea.Right;
         int y1 = clientArea.Top;
         int y2 = clientArea.Bottom;

         for (int index = 0; index < wrapper.ControlsCount; index++)
         {
            Control control  = wrapper.GetControlAt(index);
            if (control.Dock == DockStyle.None || control.Visible == false)
            {
               continue;
            }

            if (control.Dock == DockStyle.Left)
            {
               x1 = Math.Max(x1, control.Right);
            }
            else if (control.Dock == DockStyle.Right)
            {
               x2 = Math.Min(x2, control.Left);
            }
            else if (control.Dock == DockStyle.Top)
            {
               y1 = Math.Max(y1, control.Bottom);
            }
            else if (control.Dock == DockStyle.Bottom)
            {
               y2 = Math.Min(y2, control.Top);
            }
         }

         if (x2 <= x1 || y2 <= y1)
         {
            return new Rectangle();
         }

         Rectangle result = new Rectangle(x1, y1, x2 - x1, y2 - y1);

         return wrapper.RectangleToScreen(result);
      }

      /// <summary>
      /// Get the fill rectangle of the control in screen coordinates
      /// </summary>
      /// <param name="window">window</param>
      /// <returns>fill screen rectangle</returns>
      public static Rectangle GetFillScreenRectangle(Control window)
      {
         FormWrapper wrapper = new FormWrapper(window);
         return GetFillScreenRectangle(wrapper);
      }


      /// <summary>
      /// Gets the client rectangle of given form
      /// </summary>
      /// <param name="form">form object</param>
      /// <returns>client rectangle for given form</returns>
      public static Rectangle GetScreenClientRectangle(Control form)
      {
         return form.RectangleToScreen(form.ClientRectangle);
      }

      /// <summary>
      /// Form from point
      /// </summary>
      /// <param name="screenPoint">screen point</param>
      /// <returns>form</returns>
      public static Control FromPoint(Point screenPoint)
      {
         IntPtr handle = WindowFromPoint(screenPoint);
         if (handle == null)
         {
            return null;
         }

         try
         {
            Control control = Control.FromHandle(handle);
            return control;
         }
         catch { }

         return null;
      }

      /// <summary>
      /// Gets the margins of the control
      /// </summary>
      /// <param name="control">control</param>
      /// <returns>margins</returns>
      public static Margins GetMargins(Control control)
      {
         Margins margins  = new Margins();

         if (control != null)
         {
            WindowInfo info = new WindowInfo();
            info.cbSize = Marshal.SizeOf(info);

            if (GetWindowInfo(control.Handle, ref info) == false)
            {
               throw new InvalidOperationException();
            }

            margins.Left   = info.rcClient.left    - info.rcWindow.left;
            margins.Right  = info.rcWindow.right   - info.rcClient.right;
            margins.Top    = info.rcClient.top     - info.rcWindow.top;
            margins.Bottom = info.rcWindow.bottom  - info.rcClient.bottom;
         }

         return margins;
      }

      /// <summary>
      /// Get the container bounds inside host
      /// </summary>
      /// <param name="container">container</param>
      /// <returns>bounds relative to host</returns>
      public Rectangle GetBoundsInHost(Control control)
      {
         Rectangle bounds = control.Bounds;
         Control parent   = control.Parent;

         while (parent != null)
         {
            if (parent.Handle == Handle)
            {
               return bounds;
            }

            bounds.X += parent.Left;
            bounds.Y += parent.Top;

            parent = parent.Parent;
         }

         throw new InvalidOperationException("The control should be parented inside host");
      }

      /// <summary>
      /// Accessor for the controls to keep last
      /// </summary>
      public Control[] KeepLast
      {
         get { return _keepLast; }
      }

      /// <summary>
      /// Accessor for the controls to keep first
      /// </summary>
      public Control[] KeepFirst
      {
         get { return _keepFirst; }
      }


      /// <summary>
      /// Add control to the list of controls ignored when getting child from point
      /// </summary>
      /// <param name="control">control to ignore</param>
      public void AddToIgnoreOnGetChildFromPoint(Control control)
      {
         if (_ignoreOnGetFromPoint.Contains(control) == false)
         {
            _ignoreOnGetFromPoint.Add(control);
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">call from IDisposable.Dispose</param>
      protected override void Dispose(bool fromIDisposableDispose)
      {
         if (fromIDisposableDispose)
         {
            _host.SizeChanged     -= OnHostSizeChanged;
            _host.Move            -= OnHostMoved;
            _host.VisibleChanged  -= OnHostVisibleChanged;
         }

         _host = null;
      }

      #endregion Protected section

      #region Private section
      #region API

      /// <summary>
      /// Rect
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      public struct Rect
      {
          public int left;
          public int top;
          public int right;
          public int bottom;
      };

      /// <summary>
      /// Titlebar info
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      private struct TitlebarInfo
      {
         public int  cbSize;
         public Rect rcTitleBar;
         public int childBar0;
         public int childBar1;
         public int childBar2;
         public int childBar3;
         public int childBar4;
         public int childBar5;
      };

      /// <summary>
      /// Window info
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      private struct WindowInfo
      {
         public int cbSize;
         public Rect rcWindow;
         public Rect rcClient;
         public int dwStyle;
         public int dwExStyle;
         public int dwWindowStatus;
         public int cxWindowBorders;
         public int cyWindowBorders;
         public short atomWindowType;
         public short wCreatorVersion;
      }

      [DllImport("user32")]
      private static extern bool GetTitleBarInfo(IntPtr hWnd, ref TitlebarInfo info);

      [DllImport("user32")]
      private static extern bool GetWindowInfo(IntPtr hWnd, ref WindowInfo info);

      [DllImport("user32")]
      private static extern bool IsWindowVisible(IntPtr hWnd);

      [DllImport("user32")]
      private static extern IntPtr WindowFromPoint(Point screenPoint);

      #endregion API

      /// <summary>
      /// On host moved
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnHostMoved(object sender, EventArgs e)
      {
         if (Move != null)
         {
            Move(this, EventArgs.Empty);
         }
      }

      /// <summary>
      /// On host size changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnHostSizeChanged(object sender, EventArgs e)
      {
         if (SizeChanged != null)
         {
            SizeChanged(this, EventArgs.Empty);
         }
      }

      /// <summary>
      /// On host visibility changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnHostVisibleChanged(object sender, EventArgs e)
      {
         if (VisibleChanged != null)
         {
            VisibleChanged(this, EventArgs.Empty);
         }
      }

      ///// <summary>
      ///// Gets the child leaf window found at given screen location
      ///// </summary>
      ///// <param name="screenPoint">point in screen coordinates</param>
      ///// <param name="parent">parent inside which the child is searched</param>
      ///// <param name="validate">handler to validate if child is included in search</param>
      ///// <param name="containedExcluded">flag indicating that parent contained excluded</param>
      ///// <returns>rectangle from point</returns>
      //private static Control ChildLeafFromPoint(Point screenPoint, FormWrapper parent, ExtractControlHandler validate, out bool containedExcluded)
      //{
      //   containedExcluded = false;

      //   if (parent == null)
      //   {
      //      return null;
      //   }

      //   for (int childIndex = 0; childIndex < parent.ControlsCount; childIndex++)
      //   {
      //      Control child = parent.GetControlAt(childIndex);
      //      if (IgnoreOnGetFromPoint(parent, child))
      //      {
      //         continue;
      //      }

      //      if (child.RectangleToScreen(child.ClientRectangle).Contains(screenPoint) == false)
      //      {
      //         continue;
      //      }

      //      if (validate(child) == false)
      //      {
      //         containedExcluded = true;
      //         continue;
      //      }

      //      FormWrapper childWrapper    = new FormWrapper(child);
      //      bool childContainedExcluded = false;
      //      Control found = ChildLeafFromPoint(screenPoint, childWrapper, validate, out childContainedExcluded);
      //      if (childContainedExcluded)
      //      {
      //         continue;
      //      }

      //      if (found != null)
      //      {
      //         return found;
      //      }

      //      return child;
      //   }

      //   return null;
      //}

      /// <summary>
      /// Check if should ignore the child on get from point
      /// </summary>
      /// <param name="parent">parent</param>
      /// <param name="child">child</param>
      /// <returns></returns>
      private static bool IgnoreOnGetFromPoint(FormWrapper parent, Control child)
      {
         if (child == null)
         {
            return true;
         }

         if (child.Visible == false)
         {
            return true;
         }

         Control control = child;
         while (control != null)
         {
            if (parent._ignoreOnGetFromPoint.Contains(control))
            {
               return true;
            }

            control = control.Parent;
         }

         return false;
      }

      /// <summary>
      /// Get first index of the controls
      /// </summary>
      /// <param name="controls">controls array</param>
      /// <param name="match">control that should be checked for a match in controls list</param>
      /// <returns>first index</returns>
      private int GetFirstIndex(Control[] controls, Control match)
      {
         int firstIndex = _host.Controls.Count - 1;
         if (controls != null)
         {
            for (int index = controls.Length - 1; index >= 0; index--)
            {
               Control control = controls[index];
               if (control == null)
               {
                  continue;
               }

               if (control == match)
               {
                  break;
               }

               firstIndex--;
            }
         }

         return firstIndex;
      }

      /// <summary>
      /// Get last index of the controls
      /// </summary>
      /// <param name="controls">controls array</param>
      /// <param name="match">control that should be checked for a match in controls list</param>
      /// <returns>last index</returns>
      private int GetLastIndex(Control[] controls, Control match)
      {
         int lastIndex = 0;
         if (controls != null)
         {
            for (int index = 0; index < controls.Length; index++)
            {
               Control control = controls[index];
               if (control == null)
               {
                  continue;
               }

               if (control == match)
               {
                  break;
               }

               lastIndex++;
            }
         }

         return lastIndex;
      }


      /// <summary>
      /// Set child index
      /// </summary>
      /// <param name="controlToMove">control to move</param>
      /// <param name="index">index</param>
      private void SetChildIndex(Control controlToMove, int newIndex)
      {
         _host.Controls.SetChildIndex(controlToMove, newIndex);

         int orderIndex = 0;
         for (int index = 0; index < KeepFirst.Length; index++)
         {
            if (KeepFirst[index] == null)
            {
               continue;
            }

            _host.Controls.SetChildIndex(KeepFirst[index], orderIndex);

            orderIndex++;
         }

         orderIndex = _host.Controls.Count - 1;
         for (int index = KeepLast.Length - 1; index >= 0; index--)
         {
            if (KeepLast[index] == null)
            {
               continue;
            }

            _host.Controls.SetChildIndex(KeepLast[index], orderIndex);

            orderIndex--;
         }
      }

      /// <summary>
      /// Get dock container from point
      /// </summary>
      /// <param name="parentContainer">parent container</param>
      /// <param name="screenPoint">screen point</param>
      /// <returns>leaf dock container under given screen point</returns>
      private DockableContainer GetLeafDockedContainerFromPoint(DockableContainer parentContainer, Point screenPoint)
      {
         if (parentContainer.SingleChild != null)
         {
            return parentContainer;
         }

         if (parentContainer.LeftPane != null)
         {
            if (parentContainer.LeftPane.RectangleToScreen(parentContainer.LeftPane.ClientRectangle).Contains(screenPoint))
            {
               return GetLeafDockedContainerFromPoint(parentContainer.LeftPane, screenPoint);
            }
            else if (parentContainer.RightPane.RectangleToScreen(parentContainer.RightPane.ClientRectangle).Contains(screenPoint))
            {
               return GetLeafDockedContainerFromPoint(parentContainer.RightPane, screenPoint);
            }

            // Mouse over splitter
            return null;
         }

         if (parentContainer.TopPane != null)
         {
            if (parentContainer.TopPane.RectangleToScreen(parentContainer.TopPane.ClientRectangle).Contains(screenPoint))
            {
               return GetLeafDockedContainerFromPoint(parentContainer.TopPane, screenPoint);
            }
            else if (parentContainer.BottomPane.RectangleToScreen(parentContainer.BottomPane.ClientRectangle).Contains(screenPoint))
            {
               return GetLeafDockedContainerFromPoint(parentContainer.BottomPane, screenPoint);
            }

            // Mouse over splitter
            return null;
         }

         // Mouse over splitter
         return null;
      }

      #endregion Private section
   }
}
