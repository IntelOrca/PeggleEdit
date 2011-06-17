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

namespace Crom.Controls.TabbedDocument
{
   /// <summary>
   /// Tabed view control
   /// </summary>
   public partial class TabbedView : ButtonsPanel
   {
      #region Instance.

      /// <summary>
      /// Default constructor
      /// </summary>
      public TabbedView()
      {
         InitializeComponent();
      }

      #endregion Instance.

      #region Public section.

      /// <summary>
      /// Gets the count of available pages
      /// </summary>
      public int Count
      {
         get { return base.ButtonsCount; }
      }

      /// <summary>
      /// Add a new tab page
      /// </summary>
      /// <param name="page">tab page</param>
      public void Add(TabPageView page)
      {
         page.SetBounds(-15000, 0, _pagesPanel.Width, _pagesPanel.Height);
         _pagesPanel.Controls.Add(page);
         AddButton(page.Button);
      }

      /// <summary>
      /// Remove an existing tab page
      /// </summary>
      /// <param name="page">page to be removed</param>
      public void Remove(TabPageView page)
      {
         _pagesPanel.Controls.Remove(page);
         RemoveButton(page.Button);
      }

      /// <summary>
      /// Get the page at given index
      /// </summary>
      /// <param name="pageIndex">zero based page index</param>
      /// <returns>page at given index</returns>
      public TabPageView GetPageAt(int pageIndex)
      {
         TabButton buton = GetButtonAt(pageIndex);
         return (TabPageView)buton.Page;
      }

      #endregion Public section.

      #region Protected section.

      /// <summary>
      /// Occurs when the pages panel bounds were changed
      /// </summary>
      /// <param name="bounds">new bounds</param>
      protected override void OnPagesPanelBoundsChanged(Rectangle bounds)
      {
         if (_pagesPanel != null)
         {
            _pagesPanel.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
         }

         base.OnPagesPanelBoundsChanged(bounds);
      }

      /// <summary>
      /// Occurs after selected index was set
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnSelectedIndexSet(EventArgs e)
      {
         if (Count > 0)
         {
            for (int index = 0; index < Count; index++)
            {
               TabPageView page = GetPageAt(index);

               if (index == SelectedIndex)
               {
                  page.SetBounds(0, 0, _pagesPanel.Width, _pagesPanel.Height);
               }
               else
               {
                  page.SetBounds(-15000, 0, _pagesPanel.Width, _pagesPanel.Height);
               }
            }
         }

         base.OnSelectedIndexSet(e);
      }

      #endregion Protected section.
   }
}
