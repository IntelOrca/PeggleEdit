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

using Crom.Controls.TabbedDocument;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Preview pane
   /// </summary>
   internal sealed class PreviewPane : OwnerDrawPanel
   {
      #region Fields

      private PreviewRenderer       _renderer      = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public PreviewPane()
      {
         Renderer = new PreviewRenderer();
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the renderer
      /// </summary>
      public PreviewRenderer Renderer
      {
         get { return _renderer; }
         set 
         {
            if (_renderer != null)
            {
               _renderer.Invalidated -= OnRendererInvalidated;
            }

            _renderer = value;

            if (_renderer != null)
            {
               _renderer.Invalidated += OnRendererInvalidated;
            }
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Occurs when visibility of this control changes
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnSizeChanged(EventArgs e)
      {
         using (GraphicsPath path = GraphicsUtility.CreateRoundRectPath(0, 0, Width, Height, 5))
         {
            Region = new Region(ClientRectangle);
            Region = new Region(path);
         }

         base.OnSizeChanged(e);

         Invalidate();
      }

      /// <summary>
      /// Paint
      /// </summary>
      /// <param name="e"></param>
      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);

         if (Renderer != null)
         {
            PreviewRenderer.ShowFormPreview(ClientRectangle, Renderer, e.Graphics);
         }
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// Occurs when renderer is invalidated
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnRendererInvalidated(object sender, InvalidateEventArgs e)
      {
         if (e.Bounds.IsEmpty)
         {
            Invalidate();
         }
         else
         {
            Invalidate(e.Bounds);
         }
      }

      #endregion Private section
   }
}
