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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Implementation of title bar renderer
   /// </summary>
   internal class TitleBarRenderer
   {
      #region Fields

      private bool                  _showCloseButton                 = true;
      private bool                  _showAutohideButton              = false;
      private bool                  _showContextMenuButton           = true;

      private int                   _contentTopOffset                = 0;
      private bool                  _autohide                        = false;
      private int                   _titleBarButtonIndexUnderMouse   = -1;

      private Rectangle             _titleBarBounds                  = new Rectangle();

      private Icon                  _icon                            = null;
      private string                _text                            = string.Empty;

      private Color                 _color1                          = SystemColors.Control;
      private Color                 _color2                          = Color.White;
      private Color                 _selectedColor1                  = Color.DarkGray;
      private Color                 _selectedColor2                  = Color.White;
      private Color                 _textColor                       = Color.Black;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public TitleBarRenderer()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Show close button
      /// </summary>
      public bool ShowCloseButton
      {
         get { return _showCloseButton; }
         set { _showCloseButton = value; }
      }

      /// <summary>
      /// Show autohide button
      /// </summary>
      public bool ShowAutohideButton
      {
         get { return _showAutohideButton; }
         set { _showAutohideButton = value; }
      }

      /// <summary>
      /// Show context menu button
      /// </summary>
      public bool ShowContextMenuButton
      {
         get { return _showContextMenuButton; }
         set { _showContextMenuButton = value; }
      }

      /// <summary>
      /// Autohide
      /// </summary>
      public bool Autohide
      {
         get { return _autohide; }
         set { _autohide = value; }
      }

      /// <summary>
      /// Offset for the content
      /// </summary>
      public int ContentTopOffset
      {
         get { return _contentTopOffset; }
         set { _contentTopOffset = value; }
      }

      /// <summary>
      /// zero based index of title bar button under mouse
      /// </summary>
      public int TitleBarButtonIndexUnderMouse
      {
         get { return _titleBarButtonIndexUnderMouse; }
         private set { _titleBarButtonIndexUnderMouse = value; }
      }

      /// <summary>
      /// Accessor of title bar button bounds
      /// </summary>
      public Rectangle TitleBarBounds
      {
         get { return _titleBarBounds; }
         set { _titleBarBounds = value; }
      }

      /// <summary>
      /// Icon
      /// </summary>
      public Icon Icon
      {
         get { return _icon; }
         set { _icon = value; }
      }

      /// <summary>
      /// Text
      /// </summary>
      public string Text
      {
         get { return _text; }
         set { _text = value; }
      }

      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color Color1
      {
         get { return _color1; }
         set { _color1 = value; }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color Color2
      {
         get { return _color2; }
         set { _color2 = value; }
      }

      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color SelectedColor1
      {
         get { return _selectedColor1; }
         set { _selectedColor1 = value; }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color SelectedColor2
      {
         get { return _selectedColor2; }
         set { _selectedColor2 = value; }
      }

      /// <summary>
      /// Text color
      /// </summary>
      public Color TextColor
      {
         get { return _textColor; }
         set { _textColor = value; }
      }

      /// <summary>
      /// Gets the caption buttons count
      /// </summary>
      /// <returns>bounds of the caption buttons</returns>
      public void UpdateTitleBarButtonIndexUnderMouse(Point mouseLocation)
      {
         Rectangle bounds = GetCaptionButtonsBounds();

         if (bounds.Contains(mouseLocation) == false)
         {
            TitleBarButtonIndexUnderMouse = -1;
         }
         else 
         {
            int index = (mouseLocation.X - bounds.Left) / 16;
            if (index < 0 || index >= ButtonsCount)
            {
               index = -1;
            }
            TitleBarButtonIndexUnderMouse = index;
         }
      }

      /// <summary>
      /// Draw the title bar
      /// </summary>
      /// <param name="font">font used to draw the text</param>
      /// <param name="graphics">graphics</param>
      /// <param name="color1">first gradient color</param>
      /// <param name="color2">second gradient color</param>
      public void Draw(Font font, Graphics graphics, Color color1, Color color2)
      {
         Rectangle titleBounds = TitleBarBounds;
         Rectangle captionButtonsBounds = GetCaptionButtonsBounds();

         using (LinearGradientBrush backBrush = new LinearGradientBrush(titleBounds, color1, color2, LinearGradientMode.Vertical))
         {
            graphics.FillRectangle(backBrush, titleBounds);
         }

         GraphicsState state  = graphics.Save();
         Rectangle clipBounds = new Rectangle(titleBounds.Left, titleBounds.Top, captionButtonsBounds.Left - titleBounds.Left, titleBounds.Height);
         graphics.SetClip(clipBounds);

         int iconSize = Math.Max(0, Math.Min(18, titleBounds.Height - 3));
         int iconTop  = _contentTopOffset + (titleBounds.Height - iconSize) / 2 - 1;
         Rectangle iconBounds = new Rectangle(titleBounds.Left + 4, iconTop, iconSize, iconSize);

         if (Icon != null)
         {
            graphics.DrawIcon (Icon, iconBounds);
         }

         using (Brush textBrush = new SolidBrush(TextColor))
         {
            graphics.DrawString(Text, font, textBrush, iconBounds.Right, iconTop + 2);
         }

         graphics.Restore(state);

         DrawCaptionButtons(captionButtonsBounds, graphics);
      }
      
      /// <summary>
      /// Draw the auto-hide button for dockable tool window
      /// </summary>
      /// <param name="bounds">bounds of the button</param>
      /// <param name="autoHide">true if auto-hide mode is set, false otherwise</param>
      /// <param name="g">draw context</param>
      public static void DrawAutoHideButton (Rectangle bounds, bool autoHide, Graphics g)
      {
         if (autoHide)
         {
            int left = bounds.Left;
            int top  = bounds.Top;

            g.DrawLine (Pens.Navy, left +  4, top + 9, left +  4, top + 3);
            g.DrawLine (Pens.Navy, left +  1, top + 6, left +  4, top + 6);
            g.DrawLine (Pens.Navy, left + 10, top + 8, left + 10, top + 4);
            g.DrawLine (Pens.Navy, left +  4, top + 4, left + 10, top + 4);
            g.DrawLine (Pens.Navy, left +  4, top + 8, left + 10, top + 8);
            g.DrawLine (Pens.Navy, left +  4, top + 7, left + 10, top + 7);
         }
         else
         {
            int left = bounds.Left + 1;
            int top  = bounds.Top;

            g.DrawLine (Pens.Navy, left + 3, top + 8, left + 9, top + 8);
            g.DrawLine (Pens.Navy, left + 6, top + 8, left + 6, top + 11);
            g.DrawLine (Pens.Navy, left + 4, top + 2, left + 8, top + 2);
            g.DrawLine (Pens.Navy, left + 4, top + 2, left + 4, top + 8);
            g.DrawLine (Pens.Navy, left + 8, top + 2, left + 8, top + 8);
            g.DrawLine (Pens.Navy, left + 7, top + 2, left + 7, top + 8);
         }
   }

      #endregion Public section

      #region Private section

      /// <summary>
      /// Accessor of the buttons count
      /// </summary>
      private int ButtonsCount
      {
         get
         {
            int count = 0;
            if (ShowAutohideButton)
            {
               count++;
            }

            if (ShowCloseButton)
            {
               count++;
            }

            if (ShowContextMenuButton)
            {
               count++;
            }

            return count;
         }
      }

      /// <summary>
      /// Draw caption buttons
      /// </summary>
      /// <param name="captionButtonsBounds">caption buttons bounds</param>
      /// <param name="graphics">graphics object</param>
      private void DrawCaptionButtons(Rectangle captionButtonsBounds, Graphics graphics)
      {
         int edge    = 16;

         int width   = 10;
         int height  = 10;
         int x       = captionButtonsBounds.X + (edge - width) / 2;
         int y       = captionButtonsBounds.Y + (edge - height) / 2 - 2;

         int dx      = 0;
         int dy      = 0;

         if (captionButtonsBounds.Width > captionButtonsBounds.Height)
         {
            dx = edge;
         }
         else
         {
            dy = edge;
         }

         int index = 0;

         if (ShowContextMenuButton)
         {
            Rectangle bounds = new Rectangle(x, y, width, height);

            int x1 = bounds.Left + bounds.Width / 2;
            int y1 = bounds.Bottom - 1;

            int y2 = y1 - 4;
            int x2 = x1 - 4;

            int y3 = y2;
            int x3 = x1 + 4;

            if (index == TitleBarButtonIndexUnderMouse)
            {
               Rectangle backBounds = bounds;
               backBounds.Width     = 14;
               backBounds.Height    = 13;
               backBounds.X         = bounds.X - 2;
               backBounds.Y         = bounds.Y - 2;

               DrawSelectedCaptionButtonBackground(true, backBounds, graphics);
            }
            index++;


            using (GraphicsPath arrowHeadPath = new GraphicsPath())
            {
               arrowHeadPath.AddLine(x1, y1, x2, y2);
               arrowHeadPath.AddLine(x2, y2, x3, y3);
               arrowHeadPath.CloseFigure();

               graphics.FillPath(Brushes.Navy, arrowHeadPath);
            }

            x += dx;
            x += dy;
         }

         if (ShowAutohideButton)
         {
            Rectangle bounds = new Rectangle(x - 2, y - 2, width, height);

            if (index == TitleBarButtonIndexUnderMouse)
            {
               Rectangle backBounds = bounds;
               backBounds.Width     = 14;
               backBounds.Height    = 13;
               backBounds.X         = bounds.X;
               backBounds.Y         = bounds.Y;

               DrawSelectedCaptionButtonBackground(true, backBounds, graphics);
            }
            index++;

            DrawAutoHideButton(bounds, Autohide, graphics);

            x += dx;
            x += dy;
         }

         if (ShowCloseButton)
         {
            Rectangle bounds = new Rectangle(x, y + 1, 8, 7);

            if (index == TitleBarButtonIndexUnderMouse)
            {
               Rectangle backBounds = bounds;
               backBounds.Width     = 14;
               backBounds.Height    = 13;
               backBounds.X         = bounds.X - 3;
               backBounds.Y         = bounds.Y - 3;

               DrawSelectedCaptionButtonBackground(true, backBounds, graphics);
            }
            index++;

            graphics.DrawLine(Pens.Navy, bounds.Left,       bounds.Top, bounds.Right - 1, bounds.Bottom);
            graphics.DrawLine(Pens.Navy, bounds.Left + 1,   bounds.Top, bounds.Right,     bounds.Bottom);

            graphics.DrawLine(Pens.Navy, bounds.Right - 1,  bounds.Top, bounds.Left,      bounds.Bottom);
            graphics.DrawLine(Pens.Navy, bounds.Right,      bounds.Top, bounds.Left + 1,  bounds.Bottom);
         }
      }

      /// <summary>
      /// Gets the caption buttons count
      /// </summary>
      /// <returns>bounds of the caption buttons</returns>
      private Rectangle GetCaptionButtonsBounds()
      {
         Rectangle titleBounds = TitleBarBounds;

         int buttonsWidth = 16 * ButtonsCount;
         return new Rectangle(titleBounds.Right - buttonsWidth - 4, 2, buttonsWidth, titleBounds.Height - 8);
      }

      /// <summary>
      /// Draw selected caption button background
      /// </summary>
      /// <param name="active">flag indicating if the parent form is active</param>
      /// <param name="bounds">bounds</param>
      /// <param name="graphics">graphics</param>
      private static void DrawSelectedCaptionButtonBackground(bool active, Rectangle bounds, Graphics graphics)
      {
         graphics.FillRectangle(Brushes.LemonChiffon, bounds);
         graphics.DrawRectangle(Pens.DarkGray,        bounds);
      }

      #endregion Private section
   }
}
