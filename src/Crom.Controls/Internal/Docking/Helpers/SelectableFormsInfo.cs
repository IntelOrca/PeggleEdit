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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Selectable forms info
   /// </summary>
   internal sealed class SelectableFormsInfo
   {
      #region Fields

      private List<DockableFormInfo>   _forms               = new List<DockableFormInfo>();
      private int                      _firstIndex          = 0;
      private Rectangle                _initialBounds       = new Rectangle();
      private int                      _dx                  = 0;
      private int                      _dy                  = 0;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="initialBounds">initial bounds</param>
      /// <param name="offsetX">offset x</param>
      /// <param name="offsetY">offset y</param>
      public SelectableFormsInfo(Rectangle initialBounds, int offsetX, int offsetY)
      {
         _initialBounds = initialBounds;
         _dx = offsetX;
         _dy = offsetY;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor for preview initial bounds
      /// </summary>
      public Rectangle InitialBounds
      {
         get { return _initialBounds; }
      }

      /// <summary>
      /// Accessor for offset on x axis
      /// </summary>
      public int OffsetX
      {
         get { return _dx; }
      }

      /// <summary>
      /// Accessor for offset on y axis
      /// </summary>
      public int OffsetY
      {
         get { return _dy; }
      }

      /// <summary>
      /// Accessor for initial x
      /// </summary>
      public int InitialX
      {
         get { return _initialBounds.X; }
         set { _initialBounds.X = value; }
      }

      /// <summary>
      /// Accessor for initial Y
      /// </summary>
      public int InitialY
      {
         get { return _initialBounds.Y; }
         set { _initialBounds.Y = value; }
      }

      /// <summary>
      /// Accessor for dockable forms
      /// </summary>
      public List<DockableFormInfo> Forms
      {
         get { return _forms; }
      }

      /// <summary>
      /// Accessor for first index
      /// </summary>
      public int FirstIndex
      {
         get { return _firstIndex; }
         set { _firstIndex = value; }
      }

      #endregion Public section
   }
}
