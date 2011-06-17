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
   internal class DockControlEventArgs : EventArgs
   {
      #region Fields

      private Control            _control             = null;
      private DockStyle          _dock                = DockStyle.None;
      private zDockMode          _dockMode            = zDockMode.Outer;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="control">control</param>
      /// <param name="dock">dock</param>
      /// <param name="mode">dock mode</param>
      public DockControlEventArgs(Control control, DockStyle dock, zDockMode mode)
      {
         _control  = control;
         _dock     = dock;
         _dockMode = mode;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the control
      /// </summary>
      public Control Control
      {
         get { return _control; }
      }

      /// <summary>
      /// Accessor for dock result
      /// </summary>
      public DockStyle Dock
      {
         get { return _dock; }
      }

      /// <summary>
      /// Accessor for dock mode
      /// </summary>
      public zDockMode DockMode
      {
         get { return _dockMode; }
      }

      #endregion Public section
   }
}
