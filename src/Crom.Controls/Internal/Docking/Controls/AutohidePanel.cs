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
   /// Implementation of autohide panel
   /// </summary>
   internal class AutoHidePanel : Control
   {
      #region Fields

      private DockableContainer        _restoreParent          = null;
      private CommandHandler           _autoHideHandler        = null;
      private CommandHandler           _autoShowHandler        = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public AutoHidePanel()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the text
      /// </summary>
      public override string Text
      {
         get
         {
            if (View != null)
            {
               return View.Text;
            }

            return string.Empty;
         }
         set
         {
            if (View != null)
            {
               View.Text = value;
            }
         }
      }

      /// <summary>
      /// Accessor of the icon
      /// </summary>
      public Icon Icon
      {
         get
         {
            if (View != null)
            {
               return View.Icon;
            }

            return null;
         }
         set
         {
            if (View != null)
            {
               View.Icon = value;
            }
         }
      }


      /// <summary>
      /// Restore panel
      /// </summary>
      public DockableContainer RestoreParent
      {
         get { return _restoreParent; }
         set { _restoreParent = value; }
      }


      /// <summary>
      /// command
      /// </summary>
      public CommandHandler AutoHideHandler
      {
         get { return _autoHideHandler; }
         set { _autoHideHandler = value; }
      }

      /// <summary>
      /// command
      /// </summary>
      public CommandHandler AutoShowHandler
      {
         get { return _autoShowHandler; }
         set { _autoShowHandler = value; }
      }


      /// <summary>
      /// Accessor of the selected form
      /// </summary>
      public FormsTabbedView View
      {
         get
         {
            if (Controls.Count > 0)
            {
               return Controls[0] as FormsTabbedView;
            }

            return null;
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Occurs when a control was added to collection
      /// </summary>
      /// <param name="e"></param>
      protected override void OnControlAdded(ControlEventArgs e)
      {
         e.Control.TextChanged += OnViewTextChanged;
         OnTextChanged(e);

         base.OnControlAdded(e);
      }

      /// <summary>
      /// Occurs when control was removed
      /// </summary>
      /// <param name="e"></param>
      protected override void OnControlRemoved(ControlEventArgs e)
      {
         e.Control.TextChanged -= OnViewTextChanged;

         base.OnControlRemoved(e);
      }

      /// <summary>
      /// Occurs when view text is changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnViewTextChanged(object sender, EventArgs e)
      {
         OnTextChanged(e);
      }

      #endregion Protected section
   }
}
