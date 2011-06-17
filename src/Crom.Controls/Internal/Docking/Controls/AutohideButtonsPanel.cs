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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Crom.Controls.TabbedDocument;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Autohide buttons panel
   /// </summary>
   internal class AutoHideButtonsPanel : ButtonsPanel
   {
      #region Fields

      public  const int       SizeLimit         = 18;
      private DockStyle       _logicalDock      = DockStyle.None;
      private Color           _borderColor      = Color.FromArgb(150, 150, 150);

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      public AutoHideButtonsPanel()
      {
         BackColor    = SystemColors.Control;
         CutRoundRect = false;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when show preview is requested
      /// </summary>
      public event EventHandler<PreviewEventArgs> ShowPreview;

      /// <summary>
      /// Occurs when select button is requested
      /// </summary>
      public event EventHandler<ControlEventArgs> SelectButton;

      /// <summary>
      /// Accessor of the dock style
      /// </summary>
      public DockStyle LogicalDock
      {
         get { return _logicalDock; }
         set
         {
            if (_logicalDock != value)
            {
               _logicalDock = value;

               ApplyDockBasedStyle(value);
            }
         }
      }

      /// <summary>
      /// Accessor of the dock style
      /// </summary>
      public override DockStyle Dock
      {
         get { return base.Dock; }
         set
         {
            if (base.Dock != value)
            {
               base.Dock = value;

               ApplyDockBasedStyle(value);
            }
         }
      }

      /// <summary>
      /// Accessor of the border color
      /// </summary>
      public Color BorderColor
      {
         get { return _borderColor; }
         set
         {
            if (_borderColor != value)
            {
               _borderColor = value;
               Invalidate();
            }
         }
      }


      /// <summary>
      /// Accessor of the buttons count
      /// </summary>
      public int Count
      {
         get { return ButtonsCount; }
      }


      /// <summary>
      /// Add autohide panel
      /// </summary>
      /// <param name="panel">panel to add</param>
      public void Add(AutoHidePanel panel)
      {
         for (int index = 0; index < panel.View.Count; index++)
         {
            AddButton(new TabButton(panel.View.GetPageAt(index)));
         }

         panel.View.FormAdded    += OnFormAddedToPanel;
         panel.View.FormRemoved  += OnFormRemovedFromPanel;
         panel.View.FormSelected += OnFormSelectedIntoView;
      }

      /// <summary>
      /// Removes the panel if is contained here
      /// </summary>
      /// <param name="panel">panel to remove</param>
      /// <returns>true if is removed the panel</returns>
      public bool Remove(AutoHidePanel panel)
      {
         bool removed = false;
         for (int index = ButtonsCount - 1; index >= 0; index--)
         {
            TabButton button = GetButtonAt(index);

            FormsTabbedView view = HierarchyUtility.GetTabbedView((Form)button.Page);
            if (view.Parent == panel)
            {
               RemoveButton(button);
               removed = true;
            }
         }

         panel.View.FormAdded    -= OnFormAddedToPanel;
         panel.View.FormRemoved  -= OnFormRemovedFromPanel;
         panel.View.FormSelected -= OnFormSelectedIntoView;

         return removed;
      }

      /// <summary>
      /// Checks if this panel contains give panel
      /// </summary>
      /// <param name="panel">panel to check</param>
      /// <returns>true if this panel contains given pane</returns>
      public bool ContainsPanel(AutoHidePanel panel)
      {
         for (int index = ButtonsCount - 1; index >= 0; index--)
         {
            TabButton button = GetButtonAt(index);

            FormsTabbedView view = HierarchyUtility.GetTabbedView((Form)button.Page);
            if (view.Parent == panel)
            {
               return true;
            }
         }

         return false;
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Round radius
      /// </summary>
      protected override int RoundRadius
      {
         get { return 0; }
      }

      /// <summary>
      /// On paint panel background
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnPaintPanelBackground(PaintEventArgs e)
      {
         DockStyle dock = LogicalDock;
         if (dock == DockStyle.None)
         {
            dock = Dock;
         }

         if (dock == DockStyle.Top || dock == DockStyle.Bottom)
         {
            using (LinearGradientBrush backBrush = new LinearGradientBrush(ClientRectangle, BackGradient1, BackGradient2, BackGradientMode))
            {
               e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }
         }
         else if (dock == DockStyle.Left)
         {
            using (SolidBrush backBrush = new SolidBrush(BackGradient1))
            {
               e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }
         }
         else if (dock == DockStyle.Right)
         {
            using (SolidBrush backBrush = new SolidBrush(BackGradient2))
            {
               e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }
         }
      }

      /// <summary>
      /// Draw round border
      /// </summary>
      /// <param name="e">event args</param>
      protected override void DrawRoundBorder(PaintEventArgs e)
      {
         using (Pen pen = new Pen(BorderColor))
         {
            switch (LogicalDock)
            {
               case DockStyle.Left:
                  e.Graphics.DrawLine(pen, ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height);
                  break;

               case DockStyle.Right:
                  e.Graphics.DrawLine(pen, 0, 0, 0, ClientSize.Height);
                  break;

               case DockStyle.Top:
                  e.Graphics.DrawLine(pen, 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1);
                  break;

               case DockStyle.Bottom:
                  e.Graphics.DrawLine(pen, 0, 0, ClientSize.Width - 1, 0);
                  break;
            }
         }
      }

      /// <summary>
      /// Draw buttons line
      /// </summary>
      /// <param name="e">event arg</param>
      protected override void DrawButtonsLine(PaintEventArgs e)
      {
      }

      /// <summary>
      /// Occurs when mouse button was moved over a tab button
      /// </summary>
      /// <param name="buttonUnderMouse">button under mouse</param>
      protected override void OnMouseMoveOverTabButton(TabButton buttonUnderMouse)
      {
         EventHandler<PreviewEventArgs> handler = ShowPreview;
         if (handler != null)
         {
            PreviewEventArgs args = new PreviewEventArgs(new Point(buttonUnderMouse.Left, buttonUnderMouse.Top), (Form)buttonUnderMouse.Page);
            handler(this, args);
         }
      }

      /// <summary>
      /// Occurs when selected index was set
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnSelectedIndexSet(EventArgs e)
      {
      }

      /// <summary>
      /// Occurs when mouse button was pressed while the cursor was over a tab button
      /// </summary>
      /// <param name="buttonUnderMouse">button under mouse</param>
      protected override void OnMouseDownInTabButton(TabButton buttonUnderMouse)
      {
         EventHandler<ControlEventArgs> handler = SelectButton;
         if (handler != null)
         {
            ControlEventArgs args = new ControlEventArgs(buttonUnderMouse.Page);
            handler(this, args);
         }
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// On form selected into view
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnFormSelectedIntoView(object sender, FormEventArgs e)
      {
         TabButton button = GetButton(e.Form);
         SelectedButton   = button;
      }

      /// <summary>
      /// On form removed from view
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnFormRemovedFromPanel(object sender, FormEventArgs e)
      {
         TabButton button = GetButton(e.Form);
         RemoveButton(button);
      }

      /// <summary>
      /// On form added to view
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnFormAddedToPanel(object sender, FormEventArgs e)
      {
         AddButton(new TabButton(e.Form));
      }

      /// <summary>
      /// Apply style based on panel dock
      /// </summary>
      /// <param name="dock">dock</param>
      private void ApplyDockBasedStyle(DockStyle dock)
      {
         switch (dock)
         {
            case DockStyle.Left:
               Width = SizeLimit;
               ButtonsRenderer = new RightTabButtonRenderer();
               break;

            case DockStyle.Top:
               Height = SizeLimit;
               ButtonsRenderer = new BottomTabButtonRenderer();
               break;

            case DockStyle.Right:
               Width = SizeLimit;
               ButtonsRenderer = new LeftTabButtonRenderer();
               break;

            case DockStyle.Bottom:
               Height = SizeLimit;
               ButtonsRenderer = new TopTabButtonRenderer();
               break;
         }
      }

      /// <summary>
      /// Get the button associated with given form
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>button</returns>
      private TabButton GetButton(Form form)
      {
         for (int index = ButtonsCount - 1; index >= 0; index--)
         {
            TabButton button = GetButtonAt(index);

            if (button.Page == form)
            {
               return button;
            }
         }

         return null;
      }

      #endregion Private section
   }
}
