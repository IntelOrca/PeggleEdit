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

namespace Crom.Controls.TabbedDocument
{
   /// <summary>
   /// Renderer for bottom tab button
   /// </summary>
   public class BottomTabButtonRenderer : TabButtonRenderer
   {
      #region Fields

      private const int                ButtonBottomMargin                  = 2;
      private const int                PanelMargin                         = 0;
      private const int                ButtonHorizontalMargins             = 16;
      private const int                ButtonHeight                        = 16;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public BottomTabButtonRenderer()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Draw the button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="text">button text</param>
      /// <param name="selected">flag indicating that should use selected colors</param>
      /// <param name="font">font used to draw the text</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics</param>
      public override void Draw(Rectangle bounds, string text, bool selected, Font font, Icon icon, Graphics graphics)
      {
         using (GraphicsPath button = new GraphicsPath())
         {
            bounds.Y--;

            int x1 = bounds.X - (int)(bounds.Height * 0.75);
            int y1 = bounds.Top;

            int x2 = bounds.X + (int)(bounds.Height * 0.25) - 2;
            int y2 = bounds.Bottom - 2;

            int x3 = bounds.X + (int)(bounds.Height * 0.25) + 1;
            int y3 = bounds.Bottom;

            int x4 = bounds.Right - 2;
            int y4 = bounds.Bottom;

            int x5 = bounds.Right;
            int y5 = bounds.Bottom - 2;

            int x6 = bounds.Right;
            int y6 = bounds.Top;

            button.AddLine(x1, y1 - 1, x2, y2);
            button.AddLine(x2, y2, x3, y3);
            button.AddLine(x3, y3, x4, y4);
            button.AddLine(x4, y4, x5, y5);
            button.AddLine(x5, y5, x6, y6 - 1);
            button.CloseFigure();

            bounds.Y -= 2;
            bounds.Height += 2;
            using (LinearGradientBrush backBrush = new LinearGradientBrush(bounds, GetBackGradient2Color(selected), GetBackGradient1Color(selected), BackGradientMode))
            {
               graphics.FillPath(backBrush, button);

               using (Pen pen = new Pen(GetBorder2Color(selected)))
               {
                  graphics.DrawLine(pen, x1, y1, x2, y2);
                  graphics.DrawLine(pen, x2, y2, x3, y3);
                  graphics.DrawLine(pen, x3, y3, x4, y4);
                  graphics.DrawLine(pen, x4, y4, x5, y5);
                  graphics.DrawLine(pen, x5, y5, x6, y6);
               }
            }

            if (selected == false)
            {
               using (Pen pen = new Pen(GetBorder2Color(true)))
               {
                  graphics.DrawLine(pen, x1, y1 + 1, x6, y6 + 1);
               }
            }
         }

         Rectangle textBounds = bounds;
         textBounds.X += 10;
         textBounds.Y += 4;

         if (icon != null)
         {
            graphics.DrawIcon(icon, new Rectangle(textBounds.X + 2, bounds.Y + 4, ButtonHeight - 4, ButtonHeight - 4));
            textBounds.X += ButtonHeight;
         }

         using (Brush textBrush = new SolidBrush(TextColor))
         {
            graphics.DrawString(text, font, textBrush, textBounds);
         }
      }

      /// <summary>
      /// Draw buttons line
      /// </summary>
      /// <param name="graphics">graphics object</param>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="buttonsPanelBounds">buttons panel bounds</param>
      public override void DrawButtonsLine(Graphics graphics, Rectangle panelBounds, Rectangle buttonsPanelBounds)
      {
         using (Pen pen = new Pen(GetBorder2Color(true)))
         {
            graphics.DrawLine(pen, panelBounds.X, buttonsPanelBounds.Y + 1, panelBounds.Width, buttonsPanelBounds.Y + 1);
         }
      }

      /// <summary>
      /// Draw the scroll back button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="state">flag indicating the state of the button</param>
      /// <param name="graphics">graphics</param>
      public override void DrawScrollBackButton(Rectangle bounds, zButtonState state, Graphics graphics)
      {
         int x1 = bounds.Left + 5;
         int y1 = bounds.Top + bounds.Height / 2 + 1;

         int x2 = x1 + 4;
         int y2 = y1 - 4;

         int x3 = x2;
         int y3 = y1 + 4;

         using (GraphicsPath arrowHeadPath = new GraphicsPath())
         {
            arrowHeadPath.AddLine(x1, y1, x2, y2);
            arrowHeadPath.AddLine(x2, y2, x3, y3);
            arrowHeadPath.CloseFigure();

            switch (state)
            {
               case zButtonState.Normal:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkBlue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Focus:
                  graphics.FillPath(Brushes.LightGreen, arrowHeadPath);

                  graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Pressed:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkBlue, x3, y3 + 1, x1, y1 + 1);
                  graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.WhiteSmoke, x1, y1 - 1, x2, y2 - 1);

                  break;

               case zButtonState.UnderMouseCursor:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Disabled:
                  graphics.FillPath(Brushes.Gray, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkGray, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.DarkGray, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;
            }
         }
      }

      /// <summary>
      /// Draw the scroll next button
      /// </summary>
      /// <param name="bounds">button bounds</param>
      /// <param name="state">flag indicating the state of the button</param>
      /// <param name="graphics">graphics</param>
      public override void DrawScrollNextButton(Rectangle bounds, zButtonState state, Graphics graphics)
      {
         int x1 = bounds.Left + 5;
         int y1 = bounds.Top + bounds.Height / 2 + 2;

         int x2 = x1 - 5;
         int y2 = y1 - 5;

         int x3 = x2;
         int y3 = y1 + 4;

         using (GraphicsPath arrowHeadPath = new GraphicsPath())
         {
            arrowHeadPath.AddLine(x1, y1 - 1, x2, y2 - 1);
            arrowHeadPath.AddLine(x2, y2 - 1, x3, y3 + 1);
            arrowHeadPath.CloseFigure();

            switch (state)
            {
               case zButtonState.Normal:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkBlue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Focus:
                  graphics.FillPath(Brushes.LightGreen, arrowHeadPath);

                  graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Pressed:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkBlue, x3, y3 + 1, x1, y1 + 1);
                  graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.WhiteSmoke, x1, y1 - 1, x2, y2 - 1);

                  break;

               case zButtonState.UnderMouseCursor:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Disabled:
                  graphics.FillPath(Brushes.Gray, arrowHeadPath);

                  graphics.DrawLine(Pens.DarkGray, x1, y1 - 1, x2, y2 - 1);
                  graphics.DrawLine(Pens.DarkGray, x2, y2 - 1, x3, y3 + 1);
                  graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;
            }
         }
      }

      /// <summary>
      /// Checks if the panel has scrolls
      /// </summary>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="lastButtonBounds">last button bounds</param>
      /// <returns>true if the panel has scrolls</returns>
      public override bool HasScroll(Rectangle panelBounds, Rectangle lastButtonBounds)
      {
         return lastButtonBounds.Right >= panelBounds.Right;
      }

      /// <summary>
      /// Get the bounds of the scroll back button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll back button</returns>
      public override Rectangle GetScrollBackButtonBounds(Rectangle panelBounds)
      {
         return new Rectangle(panelBounds.Left - ButtonHeight, panelBounds.Top + 1, ButtonHeight - ButtonBottomMargin, ButtonHeight - ButtonBottomMargin);
      }

      /// <summary>
      /// Get the bounds of the scroll next button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll next button</returns>
      public override Rectangle GetScrollNextButtonBounds(Rectangle panelBounds)
      {
         return new Rectangle(panelBounds.Right + 2 * 4, panelBounds.Top + 1, ButtonHeight - ButtonBottomMargin, ButtonHeight - ButtonBottomMargin);
      }

      /// <summary>
      /// Get first button bounds
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <param name="hasScroll">flag indicating that panel has scrolls</param>
      /// <param name="scrolPos">scroll position</param>
      /// <param name="text">text of the button</param>
      /// <param name="font">font used to draw the button</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics object</param>
      /// <returns>bounds of the first button</returns>
      public override Rectangle GetFirstButtonBounds(Rectangle panelBounds, bool hasScroll, int scrolPos, string text, Font font, Icon icon, Graphics graphics)
      {
         int dX = 5 + PanelMargin + (int)(ButtonHeight * 0.75);

         if (hasScroll)
         {
            dX += ButtonHeight - ButtonBottomMargin;
         }

         return GetButtonBounds(-scrolPos + dX, panelBounds.Bottom - ButtonHeight - ButtonBottomMargin - 1, text, font, icon, graphics);
      }

      /// <summary>
      /// Get next button bounds
      /// </summary>
      /// <param name="previousBounds">bounds of the previous button</param>
      /// <param name="text">text of the button</param>
      /// <param name="font">font used to draw the button</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics object</param>
      /// <returns>bounds of the next button</returns>
      public override Rectangle GetNextButtonBounds(Rectangle previousBounds, string text, Font font, Icon icon, Graphics graphics)
      {
         return GetButtonBounds(previousBounds.Right + 2, previousBounds.Top, text, font, icon, graphics);
      }

      /// <summary>
      /// Get buttons clip rectangle
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <param name="hasScroll">true if the panel has scroll</param>
      /// <param name="captionButtonsCount">number of caption buttons that may be drawn on buttons panel</param>
      /// <returns>clip rectangle for the buttons</returns>
      public override Rectangle GetButtonsClipRectangle(Rectangle panelBounds, bool hasScroll, int captionButtonsCount)
      {
         int dX = 5 + PanelMargin;

         if (hasScroll)
         {
            dX += ButtonHeight - ButtonBottomMargin;
         }

         int dw = ButtonHeight * captionButtonsCount;

         return new Rectangle(dX, panelBounds.Bottom - ButtonHeight - ButtonBottomMargin - 1, panelBounds.Right - 2 * dX - PanelMargin - dw, ButtonHeight + 2);
      }

      /// <summary>
      /// Get the rectangle which will contain caption buttons drawn on panel buttons
      /// </summary>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="captionButtonsCount">number of caption buttons</param>
      /// <returns>rectangle containing caption buttons</returns>
      public override Rectangle GetCaptionButtonsRectangle(Rectangle panelBounds, int captionButtonsCount)
      {
         int width   = ButtonHeight * captionButtonsCount;
         int height  = ButtonHeight - ButtonBottomMargin;

         return new Rectangle(panelBounds.Right - width - PanelMargin, panelBounds.Bottom - ButtonHeight - 1, width, height);
      }

      /// <summary>
      /// Get client rectangle for the panel bounds
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>client rectangle for the panel</returns>
      public override Rectangle GetClientRectangle(Rectangle panelBounds)
      {
         Rectangle clientRectangle = new Rectangle();

         clientRectangle.X       = panelBounds.X;
         clientRectangle.Y       = 0;
         clientRectangle.Width   = panelBounds.Width;
         clientRectangle.Height  = panelBounds.Height - clientRectangle.Y - ButtonHeight - ButtonBottomMargin - 1;

         //clientRectangle.X       = panelBounds.X + PanelMargin;
         //clientRectangle.Y       = PanelMargin;
         //clientRectangle.Width   = panelBounds.Width - 2 * PanelMargin - 1;
         //clientRectangle.Height  = panelBounds.Height - clientRectangle.Y - ButtonHeight - ButtonBottomMargin;

         return clientRectangle;
      }

      /// <summary>
      /// Get the caption button index
      /// </summary>
      /// <param name="captionButtonsBounds">bounds of the caption buttons block</param>
      /// <param name="point">point where to search the button</param>
      /// <returns>zero based caption button index</returns>
      public override int GetCaptionButtonIndex(Rectangle captionButtonsBounds, Point point)
      {
         if (captionButtonsBounds.Contains(point) == false)
         {
            return -1;
         }

         return (point.X - captionButtonsBounds.Left) / ButtonHeight;
      }

      /// <summary>
      /// Can undo the displacement of the next button (the next button was displaced before)
      /// </summary>
      /// <param name="displacedButton">displaced button</param>
      /// <param name="selectedButton">selected button</param>
      /// <param name="mouseLocation">current mouse location</param>
      /// <returns>true if can undo</returns>
      internal override bool CanUndoDisplaceNext(TabButton displacedButton, TabButton selectedButton, Point mouseLocation)
      {
         return true;
      }

      /// <summary>
      /// Can undo the displacement of the previous button (the previous button was displaced before)
      /// </summary>
      /// <param name="displacedButton">displaced button</param>
      /// <param name="selectedButton">selected button</param>
      /// <param name="mouseLocation">current mouse location</param>
      /// <returns>true if can undo</returns>
      internal override bool CanUndoDisplaceBack(TabButton displacedButton, TabButton selectedButton, Point mouseLocation)
      {
         return mouseLocation.X < displacedButton.Left + selectedButton.Width;
      }

      /// <summary>
      /// Checks if can scroll next
      /// </summary>
      /// <param name="lastButton">last button</param>
      /// <param name="scrollNextBounds">scroll-next bounds</param>
      /// <returns>true if can scroll next</returns>
      internal override bool CanScrollNext(TabButton lastButton, Rectangle scrollNextBounds)
      {
         return lastButton.Left + lastButton.Width > scrollNextBounds.Left;
      }

      /// <summary>
      /// Get scroll position
      /// </summary>
      /// <param name="buttons">buttons collection</param>
      /// <param name="firstShownButtonIndex">zero based first shown button index</param>
      /// <returns>scroll position</returns>
      internal override int GetScrollPos(IList<TabButton> buttons, int firstShownButtonIndex)
      {
         int scrollPos = 0;
         for (int buttonIndex = 0; buttonIndex < firstShownButtonIndex; buttonIndex++)
         {
            scrollPos += buttons[buttonIndex].Width;
         }

         return scrollPos;
      }

      #endregion Public section

      #region Private section

      /// <summary>
      /// Get button bounds
      /// </summary>
      /// <param name="xPos">x position of the button</param>
      /// <param name="yPos">y position of the button</param>
      /// <param name="text">text of the button</param>
      /// <param name="font">font used to draw the button</param>
      /// <param name="icon">icon associated with the button</param>
      /// <param name="graphics">graphics object</param>
      /// <returns>bounds of the button</returns>
      private Rectangle GetButtonBounds(int xPos, int yPos, string text, Font font, Icon icon, Graphics graphics)
      {
         int width = ButtonHorizontalMargins + (int)graphics.MeasureString(text, font).Width;

         Rectangle bounds = new Rectangle();

         bounds.X       = xPos;
         bounds.Y       = yPos;
         bounds.Width   = width;
         bounds.Height  = ButtonHeight;

         if (icon != null)
         {
            bounds.Width += ButtonHeight;
         }

         return bounds;
      }

      #endregion Private section
   }
}
