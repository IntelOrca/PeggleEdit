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
   /// Form closing event args
   /// </summary>
   public class DockableFormClosingEventArgs : FormEventArgs
   {
      #region Fields

      private bool            _cancel           = false;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="form">form</param>
      /// <param name="formId">form identifier</param>
      public DockableFormClosingEventArgs(Form form, Guid formId) : base(form, formId)
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Flag indicating if should cancel the form closing
      /// </summary>
      public bool Cancel
      {
         get { return _cancel; }
         set { _cancel = value; }
      }

      #endregion Public section
   }
}
