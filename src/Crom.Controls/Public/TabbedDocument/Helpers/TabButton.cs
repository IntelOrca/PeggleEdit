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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Crom.Controls.TabbedDocument
{
   /// <summary>
   /// Implementation of a tab button
   /// </summary>
   public class TabButton : Disposable
   {
      #region Fields

      private string             _text             = string.Empty;
      private Rectangle          _bounds           = new Rectangle();
      private Control            _tabPage          = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      public TabButton(Control page)
      {
         _tabPage = page;
         Text     = page.Text;
         _tabPage.TextChanged += OnPageTextChanged;
         _tabPage.Disposed    += OnTabPageDisposed;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Event raised when text was changed
      /// </summary>
      public event EventHandler TextChanged;

      /// <summary>
      /// Set the button bounds
      /// </summary>
      /// <param name="bounds">bounds</param>
      public void SetBounds(Rectangle bounds)
      {
         ValidateNotDisposed();

         _bounds = bounds;
      }

      /// <summary>
      /// Checks if the button contains given point
      /// </summary>
      /// <param name="point">point to be checked</param>
      /// <returns>true if the button contains the given point</returns>
      public bool Contains(Point point)
      {
         ValidateNotDisposed();

         return _bounds.Contains(point);
      }

      /// <summary>
      /// Accessor for the tab button text
      /// </summary>
      public string Text
      {
         get
         {
            ValidateNotDisposed();

            return _text;
         }
         set 
         {
            ValidateNotDisposed();

            if (_text != value)
            {
               _text = value;

               if (TextChanged != null)
               {
                  TextChanged(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Accessor for left position
      /// </summary>
      public int Left
      {
         get
         {
            ValidateNotDisposed();

            return _bounds.Left;
         }
      }

      /// <summary>
      /// Accessor for top position
      /// </summary>
      public int Top
      {
         get
         {
            ValidateNotDisposed();

            return _bounds.Top;
         }
      }

      /// <summary>
      /// Accessor for width
      /// </summary>
      public int Width
      {
         get
         {
            ValidateNotDisposed();

            return _bounds.Width;
         }
      }

      /// <summary>
      /// Accessor for height
      /// </summary>
      public int Height
      {
         get
         {
            ValidateNotDisposed();

            return _bounds.Height;
         }
      }

      /// <summary>
      /// Page associated with the button
      /// </summary>
      public Control Page
      {
         get
         {
            ValidateNotDisposed();

            return _tabPage;
         }
      }

      /// <summary>
      /// Icon associated with the button
      /// </summary>
      public Icon PageIcon
      {
         get
         {
            ValidateNotDisposed();

            Icon icon = null;
            Form form = Page as Form;
            if (form != null)
            {
               icon = form.Icon;
            }

            return icon;
         }
      }

      /// <summary>
      /// Draw the button
      /// </summary>
      /// <param name="renderer">renderer used to draw the button</param>
      /// <param name="selected">true if the button is selected</param>
      /// <param name="font">font for drawing text</param>
      /// <param name="graphics">grapics for drawing text</param>
      public void Draw(TabButtonRenderer renderer, bool selected, Font font, Graphics graphics)
      {
         ValidateNotDisposed();

         renderer.Draw(_bounds, Text, selected, font, PageIcon, graphics);
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
            _tabPage.TextChanged -= OnPageTextChanged;
            _tabPage.Disposed    -= OnTabPageDisposed;

            _tabPage = null;

            _bounds = Rectangle.Empty;

            _text = null;
         }
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// On page text changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPageTextChanged(object sender, EventArgs e)
      {
         Text = _tabPage.Text;
      }

      /// <summary>
      /// Occurs when the page was disposed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argumens</param>
      private void OnTabPageDisposed(object sender, EventArgs e)
      {
         Dispose();
      }

      #endregion Private section
   }
}
