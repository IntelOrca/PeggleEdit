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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Center button for guiding dock fill
   /// </summary>
   internal partial class CenterButtonToDockFill : Control
   {
      #region Fields

      private bool         _showFillPreview           = false;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public CenterButtonToDockFill ()
      {
         InitializeComponent ();

         using (GraphicsPath path = new GraphicsPath())
         {
            path.AddLine( 0,  6,  6,  0);
            path.AddLine(35,  0, 41,  6);
            path.AddLine(41, 35, 35, 41);
            path.AddLine( 6, 41,  0, 35);

            Region = new Region(path);
         }

         _fillImage.Left = (Width  - _fillImage.Width) / 2;
         _fillImage.Top  = (Height - _fillImage.Height) / 2;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Show fill preview button inside the form
      /// </summary>
      public bool ShowFillPreview
      {
         get { return _showFillPreview; }
         set 
         { 
            _showFillPreview = value;
            _fillImage.Visible = value;
         }
      }

      /// <summary>
      /// Fill size
      /// </summary>
      public Size FillSize
      {
         get
         {
            return _fillImage.Size;
         }
      }

      /// <summary>
      /// Fill bounds
      /// </summary>
      public Rectangle FillBounds
      {
         get
         {
            return _fillImage.Bounds;
         }
      }

      #endregion Public section
   }
}