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
   /// Control collection for forms tabbed view
   /// </summary>
   internal class FormsTabbedViewControlCollection : Control.ControlCollection
   {
      #region Fields

      private FormsDecorator     _pagesPanel       = new FormsDecorator();

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="owner">owner control</param>
      public FormsTabbedViewControlCollection(Control owner) : base(owner)
      {
         base.Add(_pagesPanel);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Add a control to the collection
      /// </summary>
      /// <param name="value">new control added</param>
      public override void Add(Control value)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Add a range of controls
      /// </summary>
      /// <param name="controls">range of controls</param>
      public override void AddRange(Control[] controls)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Clears the controls collection
      /// </summary>
      public override void Clear()
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Remove a control
      /// </summary>
      /// <param name="value">control to be removed</param>
      public override void Remove(Control value)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Remove by key
      /// </summary>
      /// <param name="key">key</param>
      public override void RemoveByKey(string key)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Set child index
      /// </summary>
      /// <param name="child">child control</param>
      /// <param name="newIndex">zero based new child index</param>
      public override void SetChildIndex(Control child, int newIndex)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Accessor of the forms panel
      /// </summary>
      public FormsDecorator PagesPanel
      {
         get { return _pagesPanel; }
      }

      #endregion Public section
   }
}
