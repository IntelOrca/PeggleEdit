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
   /// Preview renderer
   /// </summary>
   public class PreviewRenderer
   {
      #region Fields

      private Form _form = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public PreviewRenderer()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when renderer is invalidated
      /// </summary>
      internal event EventHandler<InvalidateEventArgs> Invalidated;

      /// <summary>
      /// Accessor of the selected form
      /// </summary>
      public Form SelectedForm
      {
         get { return _form; }
         internal set 
         {
            if (_form != value)
            {
               _form = value;

               OnSelectedFormChanged();
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Draw background
      /// </summary>
      /// <param name="bounds">bounds</param>
      /// <param name="borderPath">border path</param>
      /// <param name="graphics">graphics</param>
      public virtual void DrawBackground(Rectangle bounds, GraphicsPath borderPath, Graphics graphics)
      {
         graphics.FillPath(Brushes.Black, borderPath);
      }

      /// <summary>
      /// Draw border
      /// </summary>
      /// <param name="size">size</param>
      /// <param name="borderPath">border path</param>
      /// <param name="graphics">graphics</param>
      public virtual void DrawBorder(Size size, GraphicsPath borderPath, Graphics graphics)
      {
         using (Pen pen = new Pen(Color.WhiteSmoke, 3))
         {
            graphics.DrawPath(pen, borderPath);
         }
      }

      /// <summary>
      /// Draw title
      /// </summary>
      /// <param name="bounds">bounds</param>
      /// <param name="graphics">graphics</param>
      public virtual void DrawTitle(Rectangle bounds, Graphics graphics)
      {
         Form form = SelectedForm;
         if (form != null)
         {
            using (Font font = new Font(form.Name, 18, FontStyle.Bold))
            {
               graphics.DrawString(form.Text, font, Brushes.WhiteSmoke, bounds.Location);
            }
         }
      }

      /// <summary>
      /// Draw preview
      /// </summary>
      /// <param name="bounds">bounds</param>
      /// <param name="graphics">graphics</param>
      public virtual void DrawPreview(Rectangle bounds, Graphics graphics)
      {
         graphics.DrawRectangle(Pens.DarkGray, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
      }

      /// <summary>
      /// Invalidate the renderer
      /// </summary>
      public virtual void Invalidate()
      {
         Invalidate(Rectangle.Empty);
      }

      /// <summary>
      /// Invalidate the renderer
      /// </summary>
      /// <param name="bounds">bounds to invalidate</param>
      public virtual void Invalidate(Rectangle bounds)
      {
         EventHandler<InvalidateEventArgs> handler = Invalidated;
         if (handler != null)
         {
            InvalidateEventArgs args = new InvalidateEventArgs(bounds);
            handler(this, args);
         }
      }

      /// <summary>
      /// Show form preview
      /// </summary>
      /// <param name="bounds">preview bounds</param>
      /// <param name="renderer">renderer</param>
      /// <param name="graphics">graphics</param>
      public static void ShowFormPreview(Rectangle bounds, PreviewRenderer renderer, Graphics graphics)
      {
         using (GraphicsPath path = GraphicsUtility.CreateRoundRectPath(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 5))
         {
            renderer.DrawBackground(bounds, path, graphics);
            renderer.DrawBorder(bounds.Size, path, graphics);
         }

         Rectangle titleBounds = new Rectangle(bounds.Left + 10, bounds.Top + 10, bounds.Width - 21, 24);
         graphics.SetClip(titleBounds, CombineMode.Replace);
         renderer.DrawTitle(titleBounds, graphics);

         Rectangle previewBounds = new Rectangle(bounds.Left + 10, bounds.Top + 40, bounds.Width - 21, bounds.Height - 50);
         graphics.SetClip(previewBounds, CombineMode.Replace);
         renderer.DrawPreview(previewBounds, graphics);
      }

      /// <summary>
      /// Clone current renderer
      /// </summary>
      /// <returns>cloned renderer</returns>
      public virtual PreviewRenderer Clone()
      {
         return new PreviewRenderer();
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Occurs when the selected form was changed
      /// </summary>
      protected virtual void OnSelectedFormChanged()
      {
      }

      #endregion Protected section
   }
}
