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
using System.Runtime.InteropServices;

namespace Crom.Controls
{
   /// <summary>
   /// Graphics utility
   /// </summary>
   public sealed class GraphicsUtility
   {
      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      private GraphicsUtility()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Create round rect paht
      /// </summary>
      /// <param name="x">x</param>
      /// <param name="y">y</param>
      /// <param name="width">width</param>
      /// <param name="height">height</param>
      /// <returns>round rect path</returns>
      public static GraphicsPath CreateRoundRectPath(int x, int y, int width, int height, int round)
      {
         return CreateRoundRectPath(x, y, width, height, round, round, round, round);
      }

      /// <summary>
      /// Create round rect
      /// </summary>
      /// <param name="x">x</param>
      /// <param name="y">y</param>
      /// <param name="width">width</param>
      /// <param name="height">height</param>
      /// <param name="roundLeftTop">round left top</param>
      /// <param name="roundRightTop">round right top</param>
      /// <param name="roundLeftBottom">round left bottom</param>
      /// <param name="roundRightBottom">round right bottom</param>
      /// <returns>round rect path</returns>
      public static GraphicsPath CreateRoundRectPath(int x, int y, int width, int height,
         int roundLeftTop, int roundRightTop, int roundLeftBottom, int roundRightBottom)
      {
         GraphicsPath roundRectPath = new GraphicsPath();

         Point[] lines = new Point[]
         {
            new Point(x,                            y + roundLeftTop),
            new Point(x + roundLeftTop,             y),
            new Point(x + width - roundRightTop,    y),
            new Point(x + width,                    y + roundRightTop),
            new Point(x + width,                    y + height - roundRightBottom),
            new Point(x + width - roundRightBottom, y + height),
            new Point(x + roundLeftBottom,          y + height),
            new Point(x,                            y + height - roundLeftBottom)
         };

         roundRectPath.AddLines(lines);
         roundRectPath.CloseFigure();

         return roundRectPath;
      }

      /// <summary>
      /// Draws a round rect
      /// </summary>
      /// <param name="bounds">bound of the round rect</param>
      /// <param name="roundX">round on x</param>
      /// <param name="roundY">round on y</param>
      /// <param name="pen">pen object</param>
      /// <param name="graphics">graphics object</param>
      public static void DrawRoundRect(Rectangle bounds, int roundX, int roundY, Pen pen, Graphics graphics)
      {
         IntPtr hdc = graphics.GetHdc();
         try
         {
            int argb = ColorToRGB(pen.Color);

            IntPtr hPen = CreatePen(GetPenStyle(pen.DashStyle), (int)pen.Width, argb);
            LogBrush brushData = new LogBrush();
            brushData.lbColor  = ColorToRGB(Color.Transparent);
            brushData.lbStyle  = W32BrushStyle.Null;
            IntPtr hBrush  = CreateBrushIndirect(ref brushData);

            IntPtr oldPen     = SelectObject(hdc, hPen);
            IntPtr oldBrush   = SelectObject(hdc, hBrush);

            RoundRect(hdc, bounds.X, bounds.Y, bounds.Right, bounds.Bottom, roundX, roundY);

            DeleteObject(SelectObject(hdc, oldPen));
            DeleteObject(SelectObject(hdc, oldBrush));
         }
         finally
         {
            graphics.ReleaseHdc(hdc);
         }
      }

      /// <summary>
      /// Fills a round rect
      /// </summary>
      /// <param name="bounds">bound of the round rect</param>
      /// <param name="roundX">round on x</param>
      /// <param name="roundY">round on y</param>
      /// <param name="backColor">backColor</param>
      /// <param name="graphics">graphics object</param>
      public static void FillRoundRect(Rectangle bounds, int roundX, int roundY, Color backColor, Graphics graphics)
      {
         IntPtr hdc = graphics.GetHdc();
         try
         {
            int argb = ColorToRGB(backColor);

            IntPtr hPen        = CreatePen(W32PenStyle.Null, 1, 0);
            LogBrush brushData = new LogBrush();
            brushData.lbColor  = argb;
            brushData.lbStyle  = W32BrushStyle.Solid;
            IntPtr hBrush      = CreateBrushIndirect(ref brushData);

            IntPtr oldPen      = SelectObject(hdc, hPen);
            IntPtr oldBrush    = SelectObject(hdc, hBrush);

            RoundRect(hdc, bounds.X, bounds.Y, bounds.Right, bounds.Bottom, roundX, roundY);

            DeleteObject(SelectObject(hdc, oldPen));
            DeleteObject(SelectObject(hdc, oldBrush));
         }
         finally
         {
            graphics.ReleaseHdc(hdc);
         }
      }

      #endregion Public section

      #region Private section

      private static int ColorToRGB(Color color)
      {
         return (color.B << 16) + (color.G << 8) + color.R;
      }

      [DllImport("Gdi32")]
      private static extern bool RoundRect(
           IntPtr hdc,         // handle to DC
           int nLeftRect,   // x-coord of upper-left corner of rectangle
           int nTopRect,    // y-coord of upper-left corner of rectangle
           int nRightRect,  // x-coord of lower-right corner of rectangle
           int nBottomRect, // y-coord of lower-right corner of rectangle
           int nWidth,      // width of ellipse
           int nHeight      // height of ellipse
         );

      [DllImport("Gdi32")]
      private static extern IntPtr SelectObject(
           IntPtr hdc,          // handle to DC
           IntPtr hgdiobj   // handle to object
         );


      [DllImport("Gdi32")]
      private static extern bool DeleteObject(
           IntPtr hgdiobj   // handle to object
         );


      private enum W32PenStyle : int
      {
         Solid = 0,
         Dash = 1,
         Dot = 2,
         DashDot = 3,
         DashDotDot = 4,
         Null = 5,
         InsideFrame = 6,
         UserStyle = 7,
         Alternate = 8,
         StyleMask = 0x0000000F,
         EndCapSquare = 0x00000100,
         EndCapFlat = 0x00000200,
         EndCapMask = 0x00000F00,
         JoinBevel = 0x00001000,
         JoinMiter = 0x00002000,
         JoinMask = 0x0000F000,
         Geometric = 0x00010000,
         TypeMask = 0x000F0000,
      };


      [DllImport("Gdi32")]
      private static extern IntPtr CreatePen(
           W32PenStyle fnPenStyle,    // pen style
           int nWidth,        // pen width
           int crColor   // pen color
         );

      private static W32PenStyle GetPenStyle(DashStyle dashStyle)
      {
         switch (dashStyle)
         {
            case DashStyle.Custom:
               return W32PenStyle.UserStyle;

            case DashStyle.Dash:
               return W32PenStyle.Dash;

            case DashStyle.DashDot:
               return W32PenStyle.DashDot;

            case DashStyle.DashDotDot:
               return W32PenStyle.DashDotDot;

            case DashStyle.Dot:
               return W32PenStyle.Dot;

            case DashStyle.Solid:
               return W32PenStyle.Solid;
         }

         return W32PenStyle.Solid;
      }


      private enum W32BrushStyle : int
      {
         Solid = 0,
         Null = 1,
         Hatched = 2,
         Pattern = 3,
         Indexed = 4,
         DIBPattern = 5,
         DIBPatternPt = 6,
         Pattern8X8 = 7,
         DIBPattern8X8 = 8,
         MonOPattern = 9,
      };

      [StructLayout(LayoutKind.Sequential)]
      struct LogBrush
      {
         public W32BrushStyle lbStyle;
         public int  lbColor;
         public int lbHatch;
      };

      [DllImport("Gdi32")]
      private static extern IntPtr CreateBrushIndirect(
         ref LogBrush lplb   // brush information
         );

      #endregion Private section
   }
}
