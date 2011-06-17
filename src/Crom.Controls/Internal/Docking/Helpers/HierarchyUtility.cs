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
   internal class HierarchyUtility
   {
      #region Public section

      /// <summary>
      /// Get forms container
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>forms container</returns>
      public static FormsContainer GetFormsContainer(Form form)
      {
         if (form == null)
         {
            return null;
         }

         return (FormsContainer)form.Parent;
      }

      /// <summary>
      /// Get form decorator
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>form decorator</returns>
      public static FormsDecorator GetFormsDecorator(Form form)
      {
         FormsContainer container = GetFormsContainer(form);
         if (container == null)
         {
            return null;
         }
         return (FormsDecorator)container.Parent;
      }

      /// <summary>
      /// Get tabbed host
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>tabbed host</returns>
      public static FormsTabbedView GetTabbedView(Form form)
      {
         FormsDecorator decorator = GetFormsDecorator(form);
         if (decorator == null)
         {
            return null;
         }

         return (FormsTabbedView)decorator.Parent;
      }

      /// <summary>
      /// Get the closest dockable container (first parent of given form which is dockable container)
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>dockable container</returns>
      public static DockableContainer GetClosestDockableContainer(Form form)
      {
         if (form == null)
         {
            return null;
         }

         FormsTabbedView tabbedView = GetTabbedView(form);

         if (tabbedView.IsAutoHideMode)
         {
            AutoHidePanel panel = (AutoHidePanel)tabbedView.Parent;
            return panel.RestoreParent;
         }

         return (DockableContainer)tabbedView.Parent;
      }

      #endregion Public section
   }
}
