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
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Control changed event args
   /// </summary>
   internal class ControlSwitchedEventArgs : EventArgs
   {
      #region Fields

      private Control            _oldControl       = null;
      private Control            _newControl       = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="oldControl">old control</param>
      /// <param name="newControl">new control</param>
      public ControlSwitchedEventArgs(Control oldControl, Control newControl)
      {
         _oldControl = oldControl;
         _newControl = newControl;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the old control
      /// </summary>
      public Control OldControl
      {
         get { return _oldControl; }
      }

      /// <summary>
      /// Accessor of the new control
      /// </summary>
      public Control NewControl
      {
         get { return _newControl; }
      }

      #endregion Public section
   }
}
