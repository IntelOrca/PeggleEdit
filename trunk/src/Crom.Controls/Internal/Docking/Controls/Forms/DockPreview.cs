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
using System.Runtime.InteropServices;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Dock preview form
   /// </summary>
   internal partial class DockPreview : Form
   {
      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public DockPreview ()
      {
         InitializeComponent ();
         TabStop  = false;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Checks if current button is visible
      /// </summary>
      public bool IsVisible
      {
         get
         {
            ValidateNotDisposed();

            return IsWindowVisible(Handle);
         }
      }

      /// <summary>
      /// Shows the window without stealing focus
      /// </summary>
      public void ShowWithoutStealingFocus()
      {
         ValidateNotDisposed();

         if (IsHandleCreated == false)
         {
            CreateControl();
         }

         if (IsVisible == false)
         {
            SetWindowPos(Handle, new IntPtr(-1), Left, Top, Width, Height, Swp.NoActivate | Swp.ShowWindow);
         }
      }

      /// <summary>
      /// Initialize the panels
      /// </summary>
      public void Initialize()
      {
         Point outOfScreen = new Point(-25000, -25000);

         Location = outOfScreen;
         Size = new Size(2, 2);

         ShowWithoutStealingFocus();
         Hide();
      }

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
      /// On paint event
      /// </summary>
      /// <param name="e">e</param>
      protected override void OnPaint(PaintEventArgs e)
      {
         int height = SystemInformation.Border3DSize.Height;
         int width  = SystemInformation.Border3DSize.Width;

         e.Graphics.FillRectangle(Brushes.LightGray, 0, 0, Width, height);
         e.Graphics.FillRectangle(Brushes.LightGray, 0, 0, width, Height);
         e.Graphics.FillRectangle(Brushes.LightGray, Width - width, 0, width, Height);
         e.Graphics.FillRectangle(Brushes.LightGray, 0, Height - height, Width, height);

         base.OnPaint(e);
      }

      /// <summary>
      /// Show the button without activating it
      /// </summary>
      protected override bool ShowWithoutActivation
      {
         get { return true; }
      }

      /// <summary>
      /// Validate that current instance is not disposed
      /// </summary>
      protected void ValidateNotDisposed()
      {
         if (IsDisposed)
         {
            throw new ObjectDisposedException(GetType().Name);
         }
      }

      #endregion Protected section

      #region Private section
      #region API

      enum Sw : int
      {
         Hide             = 0,
         ShowNormal       = 1,
         ShowMinimized    = 2,
         ShowMaximized    = 3,
         ShowNoActivate   = 4,
         Show             = 5,
         Minimize         = 6,
         ShowMinNoActive  = 7,
         ShowNA           = 8,
         Restore          = 9,
         Showdefault      = 10,
         ForceMinimize    = 11,
      };

      enum Swp : int
      {
         NoSize          = 0x0001,
         NoMove          = 0x0002,
         NoZorder        = 0x0004,
         NoRedraw        = 0x0008,
         NoActivate      = 0x0010,
         FrameChanged    = 0x0020,  /* the frame changed: send wm_nccalcsize */
         ShowWindow      = 0x0040,
         HideWindow      = 0x0080,
         NoCopyBits      = 0x0100,
         NoOwnerZorder   = 0x0200,  /* don't do owner z ordering */
         NoSendChanging  = 0x0400,  /* don't send wm_windowposchanging */
      };

      [DllImport("user32")]
      private static extern bool ShowWindow(IntPtr hWnd, Sw style);

      [DllImport("user32")]
      private static extern bool SetWindowPos(IntPtr hWnd,
          IntPtr hWndInsertAfter,
          int x,
          int y,
          int width,
          int height,
          Swp uFlags
      );

      [DllImport("user32")]
      private static extern bool IsWindowVisible(IntPtr hWnd);

      #endregion API

      #endregion Private section
   }
}