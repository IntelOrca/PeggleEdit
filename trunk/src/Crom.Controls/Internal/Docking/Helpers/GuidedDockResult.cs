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

using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Result of guided dock
   /// </summary>
   internal class GuidedDockResult
   {
      #region Fields

      private DockStyle       _dock          = DockStyle.None;
      private zDockMode       _dockMode      = zDockMode.Outer;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public GuidedDockResult()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor for dock result
      /// </summary>
      public DockStyle Dock
      {
         get { return _dock; }
         set { _dock = value; }
      }

      /// <summary>
      /// Accessor for dock mode
      /// </summary>
      public zDockMode DockMode
      {
         get { return _dockMode; }
         set { _dockMode = value; }
      }

      #endregion Public section
   }
}
