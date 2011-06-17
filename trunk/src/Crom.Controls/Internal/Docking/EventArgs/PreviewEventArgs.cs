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
   /// Preview event args
   /// </summary>
   internal class PreviewEventArgs : FormEventArgs
   {
      #region Fields

      private Point _buttonLocation = new Point();

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="buttonLocation">button location</param>
      /// <param name="form">form</param>
      public PreviewEventArgs(Point buttonLocation, Form form) : base(form, Guid.Empty)
      {
         _buttonLocation = buttonLocation;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of button location
      /// </summary>
      public Point ButtonLocation
      {
         get { return _buttonLocation; }
      }

      #endregion Public section
   }
}
