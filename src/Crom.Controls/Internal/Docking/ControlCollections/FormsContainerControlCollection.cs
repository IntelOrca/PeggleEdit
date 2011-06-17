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
   /// Control collection for forms container
   /// </summary>
   internal class FormsContainerControlCollection : Control.ControlCollection
   {
      #region Fields

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="owner">owner control</param>
      public FormsContainerControlCollection(Control owner) : base(owner)
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when top control was changed
      /// </summary>
      public event EventHandler<ControlSwitchedEventArgs> TopControlChanged;

      /// <summary>
      /// Add a control to the collection
      /// </summary>
      /// <param name="value">new control added</param>
      public override void Add(Control value)
      {
         Control top = TopControl;

         base.Add(value);

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Add a range of controls
      /// </summary>
      /// <param name="controls">range of controls</param>
      public override void AddRange(Control[] controls)
      {
         Control top = TopControl;

         base.AddRange(controls);

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Clears the controls collection
      /// </summary>
      public override void Clear()
      {
         Control top = TopControl;

         base.Clear();

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Remove a control
      /// </summary>
      /// <param name="value">control to be removed</param>
      public override void Remove(Control value)
      {
         Control top = TopControl;

         base.Remove(value);

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Remove by key
      /// </summary>
      /// <param name="key">key</param>
      public override void RemoveByKey(string key)
      {
         Control top = TopControl;

         base.RemoveByKey(key);

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Set child index
      /// </summary>
      /// <param name="child">child control</param>
      /// <param name="newIndex">zero based new child index</param>
      public override void SetChildIndex(Control child, int newIndex)
      {
         Control top = TopControl;

         base.SetChildIndex(child, newIndex);

         if (TopControl != top)
         {
            OnTopControlChnaged(top, TopControl);
         }
      }

      /// <summary>
      /// Accessor of the top control
      /// </summary>
      public Control TopControl
      {
         get
         {
            if (Count == 0)
            {
               return null;
            }

            return this[0];
         }
      }

      #endregion Public section

      #region Private section

      /// <summary>
      /// Raises top control changed event
      /// </summary>
      /// <param name="oldControl">old control</param>
      /// <param name="newControl">new control</param>
      private void OnTopControlChnaged(Control oldControl, Control newControl)
      {
         EventHandler<ControlSwitchedEventArgs> handler = TopControlChanged;
         if (handler != null)
         {
            ControlSwitchedEventArgs args = new ControlSwitchedEventArgs(oldControl, newControl);
            handler(this, args);
         }
      }

      #endregion Private section
   }
}
