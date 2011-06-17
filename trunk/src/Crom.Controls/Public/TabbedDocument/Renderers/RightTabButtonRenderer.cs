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
   /// Renderer for left tab button
   /// </summary>
   public class RightTabButtonRenderer : TabButtonRenderer
   {
      #region Fields.

      private const int                ButtonRightMargin                   = 2;
      private const int                PanelMargin                         = 0;
      private const int                ButtonVerticalMargins               = 16;
      private const int                ButtonWidth                         = 16;

      #endregion Fields.

      #region Instance.

      /// <summary>
      /// Default constructor
      /// </summary>
      public RightTabButtonRenderer()
      {
         BackGradientMode = LinearGradientMode.Horizontal;
      }

      #endregion Instance

      #region Public section.

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
            int x1 = bounds.X;
            int y1 = bounds.Bottom + (int)(bounds.Width * 0.75);

            int x2 = bounds.Right - 2;
            int y2 = bounds.Bottom - (int)(bounds.Width * 0.25) + 2;

            int x3 = bounds.Right;
            int y3 = bounds.Bottom - (int)(bounds.Width * 0.25) - 1;

            int x4 = bounds.Right;
            int y4 = bounds.Y + 2;

            int x5 = bounds.Right - 2;
            int y5 = bounds.Y;

            int x6 = bounds.X;
            int y6 = bounds.Y;

            button.AddLine(x1 - 1, y1, x2, y2);
            button.AddLine(x2, y2, x3, y3);
            button.AddLine(x3, y3, x4, y4);
            button.AddLine(x4, y4, x5, y5);
            button.AddLine(x5, y5, x6 - 1, y6);
            button.CloseFigure();

            bounds.X -= 2;
            bounds.Width += 2;
            using (LinearGradientBrush backBrush = new LinearGradientBrush(bounds, GetBackGradient2Color(selected), GetBackGradient1Color(selected), BackGradientMode))
            {
               graphics.FillPath(backBrush, button);

               using (Pen pen = new Pen(GetBorder2Color(selected)))
               {
                  graphics.DrawLine(pen, x1, y1, x2, y2);
                  graphics.DrawLine(pen, x2, y2, x3, y3);
                  graphics.DrawLine(pen, x3, y3, x4, y4);
                  graphics.DrawLine(pen, x4, y4, x5, y5);
                  graphics.DrawLine(pen, x5, y5, x6 - 1, y6);
               }

               using (Pen pen = new Pen(GetBorder1Color(selected)))
               {
                  graphics.DrawLine(pen, x1, y1 - 1, x2, y2 - 1);
               }

               if (selected == false)
               {
                  using (Pen pen = new Pen(GetBorder2Color(selected)))
                  {
                     graphics.DrawLine(pen, x1, y6 + 1, x6, y1);
                  }
               }
            }
         }

         Rectangle textBounds = bounds;
         textBounds.Y += 10;

         if (icon != null)
         {
            graphics.DrawIcon(icon, new Rectangle(bounds.X + 4, textBounds.Y + 2, ButtonWidth - 4, ButtonWidth - 4));
            textBounds.Y += ButtonWidth;
         }

         using (Brush textBrush = new SolidBrush(TextColor))
         {
            graphics.DrawString(text, font, textBrush, textBounds, new StringFormat(StringFormatFlags.DirectionVertical));
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
            graphics.DrawLine(pen, buttonsPanelBounds.Left + 1, panelBounds.Top, buttonsPanelBounds.Left + 1, panelBounds.Bottom);
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
         int x1 = bounds.Left + bounds.Width / 2 + 1;
         int y1 = bounds.Top + 5;

         int y2 = y1 + 5;
         int x2 = x1 - 4;

         int y3 = y2;
         int x3 = x1 + 4;

         using (GraphicsPath arrowHeadPath = new GraphicsPath())
         {
            arrowHeadPath.AddLine(x1, y1, x2, y2);
            arrowHeadPath.AddLine(x2, y2, x3, y3);
            arrowHeadPath.CloseFigure();

            switch (state)
            {
               case zButtonState.Normal:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkBlue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Focus:
                  graphics.FillPath(Brushes.LightGreen, arrowHeadPath);

                  //graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Pressed:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkBlue, x3, y3 + 1, x1, y1 + 1);
                  //graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.WhiteSmoke, x1, y1 - 1, x2, y2 - 1);

                  break;

               case zButtonState.UnderMouseCursor:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Disabled:
                  graphics.FillPath(Brushes.Gray, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkGray, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.DarkGray, x1, y1 + 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White,    x2, y2 - 1, x3, y3 + 1);

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
         int x1 = bounds.Left + bounds.Width / 2 + 1;
         int y1 = bounds.Bottom - 5;

         int y2 = y1 - 4;
         int x2 = x1 - 4;

         int y3 = y2;
         int x3 = x1 + 4;

         using (GraphicsPath arrowHeadPath = new GraphicsPath())
         {
            arrowHeadPath.AddLine(x1, y1, x2, y2);
            arrowHeadPath.AddLine(x2, y2, x3, y3);
            arrowHeadPath.CloseFigure();

            switch (state)
            {
               case zButtonState.Normal:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkBlue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Focus:
                  graphics.FillPath(Brushes.LightGreen, arrowHeadPath);

                  //graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Pressed:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkBlue, x3, y3 + 1, x1, y1 + 1);
                  //graphics.DrawLine(Pens.DarkBlue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.WhiteSmoke, x1, y1 - 1, x2, y2 - 1);

                  break;

               case zButtonState.UnderMouseCursor:
                  graphics.FillPath(Brushes.LightSteelBlue, arrowHeadPath);

                  //graphics.DrawLine(Pens.Blue, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.Blue, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

                  break;

               case zButtonState.Disabled:
                  graphics.FillPath(Brushes.Gray, arrowHeadPath);

                  //graphics.DrawLine(Pens.DarkGray, x1, y1 - 1, x2, y2 - 1);
                  //graphics.DrawLine(Pens.DarkGray, x2, y2 - 1, x3, y3 + 1);
                  //graphics.DrawLine(Pens.White, x3, y3 + 1, x1, y1 + 1);

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
         return lastButtonBounds.Bottom >= panelBounds.Bottom;
      }

      /// <summary>
      /// Get the bounds of the scroll back button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll back button</returns>
      public override Rectangle GetScrollBackButtonBounds(Rectangle panelBounds)
      {
         return new Rectangle(panelBounds.Left + 3, panelBounds.Top - ButtonWidth, ButtonWidth - ButtonRightMargin, ButtonWidth - ButtonRightMargin);
      }

      /// <summary>
      /// Get the bounds of the scroll next button
      /// </summary>
      /// <param name="panelBounds">bounds of the panel</param>
      /// <returns>bounds of the scroll next button</returns>
      public override Rectangle GetScrollNextButtonBounds(Rectangle panelBounds)
      {
         return new Rectangle(panelBounds.Left + 3, panelBounds.Bottom + 4, ButtonWidth - ButtonRightMargin, ButtonWidth - ButtonRightMargin);
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
         int dY = 4 + PanelMargin;

         if (hasScroll)
         {
            dY += ButtonWidth - ButtonRightMargin;
         }

         return GetButtonBounds(panelBounds.Right - ButtonRightMargin - ButtonWidth - 1, -scrolPos + dY, text, font, icon, graphics);
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
         return GetButtonBounds(previousBounds.Left, previousBounds.Bottom + 2, text, font, icon, graphics);
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
         clientRectangle.Y       = panelBounds.Y;
         clientRectangle.Width   = panelBounds.Width - clientRectangle.X - ButtonRightMargin - ButtonWidth;
         clientRectangle.Height  = panelBounds.Height - 1;

         return clientRectangle;
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
         int dY = 4 + PanelMargin;

         if (hasScroll)
         {
            dY += ButtonWidth - ButtonRightMargin;
         }

         int dh = ButtonWidth * captionButtonsCount;

         return new Rectangle(panelBounds.Right - ButtonRightMargin - ButtonWidth - 1, dY, ButtonWidth + 2, panelBounds.Bottom - 2 * dY - PanelMargin - dh);
      }

      /// <summary>
      /// Get the rectangle which will contain caption buttons drawn on panel buttons
      /// </summary>
      /// <param name="panelBounds">panel bounds</param>
      /// <param name="captionButtonsCount">number of caption buttons</param>
      /// <returns>rectangle containing caption buttons</returns>
      public override Rectangle GetCaptionButtonsRectangle(Rectangle panelBounds, int captionButtonsCount)
      {
         int width  = ButtonWidth - ButtonRightMargin;
         int height = ButtonWidth * captionButtonsCount;

         return new Rectangle(panelBounds.Right - ButtonWidth - 2, panelBounds.Bottom - height - PanelMargin, width, height);
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

         return (point.Y - captionButtonsBounds.Top) / ButtonWidth;
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
         return mouseLocation.Y < displacedButton.Top + selectedButton.Height;
      }

      /// <summary>
      /// Checks if can scroll next
      /// </summary>
      /// <param name="lastButton">last button</param>
      /// <param name="scrollNextBounds">scroll-next bounds</param>
      /// <returns>true if can scroll next</returns>
      internal override bool CanScrollNext(TabButton lastButton, Rectangle scrollNextBounds)
      {
         return lastButton.Top + lastButton.Height > scrollNextBounds.Top;
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
            scrollPos += buttons[buttonIndex].Height;
         }

         return scrollPos;
      }

      #endregion Public section.

      #region Private section.

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
         int height = ButtonVerticalMargins + (int)graphics.MeasureString(text, font).Width;

         Rectangle bounds = new Rectangle();

         bounds.X       = xPos;
         bounds.Y       = yPos;
         bounds.Width   = ButtonWidth;
         bounds.Height  = height;

         if (icon != null)
         {
            bounds.Height += ButtonWidth;
         }

         return bounds;
      }

      #endregion Private section.
   }
}
