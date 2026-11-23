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

            return WindowingApi.Current.IsWindowVisible(this);
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
            WindowingApi.Current.ShowTopMostWithoutActivating(this);
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
   }
}