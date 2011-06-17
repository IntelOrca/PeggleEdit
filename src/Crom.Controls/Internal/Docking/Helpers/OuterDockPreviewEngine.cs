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
   /// Outer dock preview engine
   /// </summary>
   internal sealed class OuterDockPreviewEngine
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
      private OuterDockPreviewEngine()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Get preview bounds
      /// </summary>
      /// <param name="dock">dock for which to get the preview bounds</param>
      /// <param name="host">host</param>
      /// <param name="movedPanel">moved panel</param>
      /// <returns>preview bounds</returns>
      public static Rectangle GetPreviewBounds(DockStyle dock, FormWrapper host, Control movedPanel)
      {
         switch (dock)
         {
            case DockStyle.Left:
               return GetOuterLeftPreviewBounds(host, movedPanel);

            case DockStyle.Right:
               return GetOuterRightPreviewBounds(host, movedPanel);

            case DockStyle.Top:
               return GetOuterTopPreviewBounds(host, movedPanel);

            case DockStyle.Bottom:
               return GetOuterBottomPreviewBounds(host, movedPanel);

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
      /// <returns>preview size</returns>
      private static int GetPreviewSize(Control movedPanel, DockStyle dock)
      {
         if (dock == DockStyle.Left || dock == DockStyle.Right)
         {
            return Math.Max(MinDockPanelSize, Math.Min(MaxDockPanelSize, movedPanel.Width));
         }

         return Math.Max(MinDockPanelSize, Math.Min(MaxDockPanelSize, movedPanel.Height));
      }

      /// <summary>
      /// Get outer top preview bounds
      /// </summary>
      /// <param name="host">host</param>
      /// <param name="movedPanel">moved panel</param>
      /// <returns>outer top preview bounds</returns>
      private static Rectangle GetOuterTopPreviewBounds(FormWrapper host, Control movedPanel)
      {
         Rectangle marginBounds = host.ScreenClientRectangle;
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Top);

            return new Rectangle(marginBounds.Left, marginBounds.Top, marginBounds.Width, size);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer bottom preview bounds
      /// </summary>
      /// <param name="host">host</param>
      /// <param name="movedPanel">moved panel</param>
      /// <returns>outer bottom preview bounds</returns>
      private static Rectangle GetOuterBottomPreviewBounds(FormWrapper host, Control movedPanel)
      {
         Rectangle marginBounds = host.ScreenClientRectangle;
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Bottom);

            return new Rectangle(marginBounds.Left, marginBounds.Bottom - size, marginBounds.Width, size);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer left preview bounds
      /// </summary>
      /// <param name="host">host</param>
      /// <param name="movedPanel">moved panel</param>
      /// <returns>outer left preview bounds</returns>
      private static Rectangle GetOuterLeftPreviewBounds(FormWrapper host, Control movedPanel)
      {
         Rectangle marginBounds = host.ScreenClientRectangle;
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Left);

            return new Rectangle(marginBounds.Left, marginBounds.Top, size, marginBounds.Height);
         }

         return new Rectangle();
      }

      /// <summary>
      /// Get outer right preview bounds
      /// </summary>
      /// <param name="host">host</param>
      /// <param name="movedPanel">moved panel</param>
      /// <returns>outer right preview bounds</returns>
      private static Rectangle GetOuterRightPreviewBounds(FormWrapper host, Control movedPanel)
      {
         Rectangle marginBounds = host.ScreenClientRectangle;
         if (marginBounds.IsEmpty == false)
         {
            int size = GetPreviewSize(movedPanel, DockStyle.Right);

            return new Rectangle(marginBounds.Right - size, marginBounds.Top, size, marginBounds.Height);
         }

         return new Rectangle();
      }

      #endregion Private section
   }
}
