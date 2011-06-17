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
   /// Inner dock preview engine
   /// </summary>
   internal sealed class InnerDockPreviewEngine
   {
      #region Fields
      #region Constants

      private const int                MaxDockPanelSize                 = 300;
      private const int                MinDockPanelSize                 = 100;

      #endregion Constants

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      private InnerDockPreviewEngine()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Get preview bounds
      /// </summary>
      /// <param name="dock">dock for which to get the preview bounds</param>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="panelUnderMouse">panel under mouse</param>
      /// <param name="freeAreaBounds">free area bounds</param>
      /// <returns>preview bounds</returns>
      public static Rectangle GetPreviewBounds(DockStyle dock, Control movedPanel, Control panelUnderMouse, Rectangle freeAreaBounds)
      {
         Rectangle bounds = freeAreaBounds;
         if (panelUnderMouse != null)
         {
            bounds = panelUnderMouse.RectangleToScreen(panelUnderMouse.ClientRectangle);
         }

         switch (dock)
         {
            case DockStyle.Left:
               return GetInnerLeftPreviewBounds(movedPanel, bounds);

            case DockStyle.Right:
               return GetInnerRightPreviewBounds(movedPanel, bounds);

            case DockStyle.Top:
               return GetInnerTopPreviewBounds(movedPanel, bounds);

            case DockStyle.Bottom:
               return GetInnerBottomPreviewBounds(movedPanel, bounds);

            case DockStyle.Fill:
               return GetInnerFillPreviewBounds(movedPanel, bounds);

            default:
               throw new InvalidOperationException();
         }
      }

      #endregion Public section

      #region Private section

      /// <summary>
      /// Get preview size
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>preview size</returns>
      private static int GetPreviewSize(Control movedPanel, DockStyle dock, Rectangle marginBounds)
      {
         if (dock == DockStyle.Left || dock == DockStyle.Right)
         {
            return Math.Min(marginBounds.Width / 2, Math.Max(MinDockPanelSize, movedPanel.Width));
         }

         return Math.Min(marginBounds.Height / 2, Math.Max(MinDockPanelSize, movedPanel.Height));
      }

      /// <summary>
      /// Get outer top preview bounds
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>outer top preview bounds</returns>
      private static Rectangle GetInnerTopPreviewBounds(Control movedPanel, Rectangle marginBounds)
      {
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Top, marginBounds);

            return new Rectangle(marginBounds.Left, marginBounds.Top, marginBounds.Width, size);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer bottom preview bounds
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>outer bottom preview bounds</returns>
      private static Rectangle GetInnerBottomPreviewBounds(Control movedPanel, Rectangle marginBounds)
      {
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Bottom, marginBounds);

            return new Rectangle(marginBounds.Left, marginBounds.Bottom - size, marginBounds.Width, size);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer left preview bounds
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>outer left preview bounds</returns>
      private static Rectangle GetInnerLeftPreviewBounds(Control movedPanel, Rectangle marginBounds)
      {
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Left, marginBounds);

            return new Rectangle(marginBounds.Left, marginBounds.Top, size, marginBounds.Height);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer right preview bounds
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>outer right preview bounds</returns>
      private static Rectangle GetInnerRightPreviewBounds(Control movedPanel, Rectangle marginBounds)
      {
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Right, marginBounds);

            return new Rectangle(marginBounds.Right - size, marginBounds.Top, size, marginBounds.Height);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get inner fill preview bounds
      /// </summary>
      /// <param name="movedPanel">moved panel</param>
      /// <param name="marginBounds">free area bounds</param>
      /// <returns>bounds</returns>
      private static Rectangle GetInnerFillPreviewBounds(Control movedPanel, Rectangle marginBounds)
      {
         return marginBounds;
      }

      #endregion Private section
   }
}
