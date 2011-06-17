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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Autohide
   /// </summary>
   internal class Autohide
   {
      #region Fields

      private FormWrapper              _host                   = null;
      private AutoHideButtonsPanel     _leftAutoHideSpacer     = null;
      private AutoHideButtonsPanel     _topAutoHideSpacer      = null;
      private AutoHideButtonsPanel     _rightAutoHideSpacer    = null;
      private AutoHideButtonsPanel     _bottomAutoHideSpacer   = null;
      private AutoHideButtonsPanel     _leftAutoHideButtons    = null;
      private AutoHideButtonsPanel     _topAutoHideButtons     = null;
      private AutoHideButtonsPanel     _rightAutoHideButtons   = null;
      private AutoHideButtonsPanel     _bottomAutoHideButtons  = null;
      private AutoHidePanel            _autoShowPanel          = null;
      private AutoHidePanel            _autoHidePanel          = null;
      private Command                  _animationCommand       = new Command();
      private int                      _animationSpeed         = 25;
      private int                      _firstControlOffset     = 0;
      private PreviewPane              _previewPane            = new PreviewPane();
      private readonly Size            _leftRightPreviewSize   = new Size(150, 250);
      private readonly Size            _topBottomPreviewSize   = new Size(250, 150);

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="host">host</param>
      /// <param name="firstControlOffset">offset for first control</param>
      /// <param name="animationCommand">animation command</param>
      public Autohide(FormWrapper host, int firstControlOffset, Command animationCommand)
      {
         _host = host;
         _animationCommand    = animationCommand;
         _firstControlOffset  = firstControlOffset;

         _previewPane.Visible = false;
         _host.AddFirst(_previewPane);
         _host.KeepFirst[4 + _firstControlOffset] = _previewPane;

         _previewPane.BackColor = Color.Black;

         _host.SizeChanged    += OnHostSizeChanged;
         _host.VisibleChanged += OnHostVisibleChanged;

         LeftAutohideButtons.Visible   = false;
         RightAutohideButtons.Visible  = false;
         TopAutohideButtons.Visible    = false;
         BottomAutohideButtons.Visible = false;
         ArrangeAutoButtonsPanels();
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Set host container dock for the control
      /// </summary>
      public event EventHandler<DockControlEventArgs> SetHostContainerDock;

      /// <summary>
      /// Accessor of the animation speed
      /// </summary>
      public int AnimationSpeed
      {
         get { return _animationSpeed; }
         set { _animationSpeed = value; }
      }

      /// <summary>
      /// Hide auto pane when mouse exits
      /// </summary>
      public void HideAutoPaneWhenMouseExits()
      {
         Point mousePosition = Control.MousePosition;


         bool checkIfShouldHideAutoPane = false;
         bool checkIfShouldHidePreview  = _previewPane.Visible;

         if (checkIfShouldHidePreview)
         {
            checkIfShouldHidePreview = (_host.RectangleToScreen(_previewPane.Bounds).Contains(mousePosition) == false);
         }

         AutoHidePanel panel = _autoShowPanel;
         if (panel != null)
         {
            if (panel.Visible)
            {
               checkIfShouldHideAutoPane = (panel.RectangleToScreen(panel.ClientRectangle).Contains(mousePosition) == false);
            }
            else
            {
               _autoShowPanel = null;
            }
         }

         if (checkIfShouldHideAutoPane || checkIfShouldHidePreview)
         {
            Point clientPosition = _host.PointToClient(mousePosition);

            if (IsPointInSpacers(clientPosition))
            {
               return;
            }

            if (checkIfShouldHideAutoPane)
            {
               _autoHidePanel = panel;
               _animationCommand.Handler = panel.AutoHideHandler;
            }

            if (checkIfShouldHidePreview)
            {
               _previewPane.Visible = false;
            }
         }
      }

      /// <summary>
      /// Accessor of the renderer
      /// </summary>
      public PreviewRenderer PreviewRenderer
      {
         get { return _previewPane.Renderer; }
         set 
         {
            if (value == null)
            {
               _previewPane.Visible = false;
            }

            _previewPane.Renderer = value; 
         }
      }


      /// <summary>
      /// Show auto form
      /// </summary>
      /// <param name="form">form to be shown</param>
      public void ShowAutoForm(Form form)
      {
         FormsTabbedView view = HierarchyUtility.GetTabbedView(form);
         AutoHidePanel panel  = view.Parent as AutoHidePanel;
         if (panel == null)
         {
            return;
         }

         if (_leftAutoHideButtons != null)
         {
            if (_leftAutoHideButtons.ContainsPanel(panel))
            {
               OnLeftPaneSelectButton(this, new ControlEventArgs(form));
               return;
            }
         }

         if (_rightAutoHideButtons != null)
         {
            if (_rightAutoHideButtons.ContainsPanel(panel))
            {
               OnRightPaneSelectButton(this, new ControlEventArgs(form));
               return;
            }
         }

         if (_topAutoHideButtons != null)
         {
            if (_topAutoHideButtons.ContainsPanel(panel))
            {
               OnTopPaneSelectButton(this, new ControlEventArgs(form));
               return;
            }
         }

         if (_bottomAutoHideButtons != null)
         {
            if (_bottomAutoHideButtons.ContainsPanel(panel))
            {
               OnBottomPaneSelectButton(this, new ControlEventArgs(form));
               return;
            }
         }
      }



      /// <summary>
      /// Auto-hide the given view
      /// </summary>
      /// <param name="view">view to hide</param>
      public void SetAutoHideMode(FormsTabbedView view)
      {
         AutoHidePanel panel  = new AutoHidePanel();
         panel.RestoreParent  = (DockableContainer)view.Parent;
         panel.Size    = view.Size;
         panel.Visible = false;

         HideRestoreContainers(panel);

         panel.RestoreParent.SetModeLinked(view);
         view.Parent = panel;

         DockStyle logicalDock = view.HostContainerDock;

         OnSetHostContainerDock(view, Globals.DockAutoHide);

         view.Positioner = new ControlPositioner(panel);
         view.Positioner.CanMove = false;

         if (logicalDock == DockStyle.Left)
         {
            panel.Bounds = new Rectangle(LeftAutohideButtons.Right, LeftAutohideButtons.Top, view.Width, LeftAutohideButtons.Height);

            view.Positioner.CanSizeLeft   = false;
            view.Positioner.CanSizeTop    = false;
            view.Positioner.CanSizeBottom = false;

            panel.AutoHideHandler = AutoHideLeftPane;
            panel.AutoShowHandler = AutoShowLeftPane;

            LeftAutohideButtons.Add(panel);
         }
         else if (logicalDock == DockStyle.Right)
         {
            panel.Bounds = new Rectangle(RightAutohideButtons.Left - view.Width, RightAutohideButtons.Top, view.Width, RightAutohideButtons.Height);

            view.Positioner.CanSizeRight  = false;
            view.Positioner.CanSizeTop    = false;
            view.Positioner.CanSizeBottom = false;

            panel.AutoHideHandler = AutoHideRightPane;
            panel.AutoShowHandler = AutoShowRightPane;

            RightAutohideButtons.Add(panel);
         }
         else if (logicalDock == DockStyle.Top)
         {
            panel.Bounds = new Rectangle(TopAutohideButtons.Left, TopAutohideButtons.Bottom, TopAutohideButtons.Width, view.Height);

            view.Positioner.CanSizeLeft   = false;
            view.Positioner.CanSizeRight  = false;
            view.Positioner.CanSizeTop    = false;

            panel.AutoHideHandler = AutoHideTopPane;
            panel.AutoShowHandler = AutoShowTopPane;

            TopAutohideButtons.Add(panel);
         }
         else if (logicalDock == DockStyle.Bottom)
         {
            panel.Bounds = new Rectangle(BottomAutohideButtons.Left, BottomAutohideButtons.Top - view.Height, BottomAutohideButtons.Width, view.Height);

            view.Positioner.CanSizeLeft   = false;
            view.Positioner.CanSizeRight  = false;
            view.Positioner.CanSizeBottom = false;

            panel.AutoHideHandler = AutoHideBottomPane;
            panel.AutoShowHandler = AutoShowBottomPane;

            BottomAutohideButtons.Add(panel);
         }
         else
         {
            Debug.Fail("Autohide should be available only for docked left, right, top or bottom");
            return;
         }

         _host.AddFirst(panel);
         view.IsAutoHideMode = true;
         view.PagesPanel.AutoHidden = true;

         ArrangeAutoButtonsPanels();
      }

      /// <summary>
      /// Auto-show the given view
      /// </summary>
      /// <param name="view">view to show</param>
      public void UnsetAutoHideMode(FormsTabbedView view)
      {
         AutoHidePanel panel         = (AutoHidePanel)view.Parent;
         DockableContainer container = panel.RestoreParent;

         DockStyle logicalDock = DockStyle.None;
         if (LeftAutohideButtons.Remove(panel))
         {
            logicalDock = DockStyle.Left;
         }
         else if (RightAutohideButtons.Remove(panel))
         {
            logicalDock = DockStyle.Right;
         }
         else if (TopAutohideButtons.Remove(panel))
         {
            logicalDock = DockStyle.Top;
         }
         else if (BottomAutohideButtons.Remove(panel))
         {
            logicalDock = DockStyle.Bottom;
         }
         else
         {
            Debug.Fail("Panel not found");
         }

         _host.Remove(panel);

         container.SetModeSingleChild(view);

         OnSetHostContainerDock(view, logicalDock);
         view.IsAutoHideMode = false;
         view.PagesPanel.AutoHidden = false;

         ShowRestoreContainers(panel);

         ArrangeAutoButtonsPanels();

         _autoShowPanel = null;
      }


      /// <summary>
      /// Arrange the panels containing auto buttons
      /// </summary>
      public void ArrangeAutoButtonsPanels()
      {
         int sizeLimit = AutoHideButtonsPanel.SizeLimit;

         if (_leftAutoHideButtons != null)
         {
            _leftAutoHideButtons.Left = 0;

            if (HasTopButtons)
            {
               _leftAutoHideButtons.Top = sizeLimit;
            }
            else
            {
               _leftAutoHideButtons.Top = 0;
            }

            if (HasBottomButtons)
            {
               _leftAutoHideButtons.Height = _host.ClientSize.Height - _leftAutoHideButtons.Top - sizeLimit;
            }
            else
            {
               _leftAutoHideButtons.Height = _host.ClientSize.Height - _leftAutoHideButtons.Top;
            }

            bool visible = _leftAutoHideButtons.Count > 0;
            _leftAutoHideButtons.Visible = visible;
            _leftAutoHideSpacer.Visible  = visible;
         }

         if (_rightAutoHideButtons != null)
         {
            _rightAutoHideButtons.Left = _host.ClientSize.Width - sizeLimit;

            if (HasTopButtons)
            {
               _rightAutoHideButtons.Top = sizeLimit;
            }
            else
            {
               _rightAutoHideButtons.Top = 0;
            }

            if (HasBottomButtons)
            {
               _rightAutoHideButtons.Height = _host.ClientSize.Height - _rightAutoHideButtons.Top - sizeLimit;
            }
            else
            {
               _rightAutoHideButtons.Height = _host.ClientSize.Height - _rightAutoHideButtons.Top;
            }

            bool visible = _rightAutoHideButtons.Count > 0;
            _rightAutoHideButtons.Visible = visible;
            _rightAutoHideSpacer.Visible  = visible;
         }

         if (_topAutoHideButtons != null)
         {
            _topAutoHideButtons.Top = 0;

            if (HasLeftButtons)
            {
               _topAutoHideButtons.Left = sizeLimit;
            }
            else
            {
               _topAutoHideButtons.Left = 0;
            }

            if (HasRightButtons)
            {
               _topAutoHideButtons.Width = _host.ClientSize.Width - _topAutoHideButtons.Left - sizeLimit;
            }
            else
            {
               _topAutoHideButtons.Width = _host.ClientSize.Width - _topAutoHideButtons.Left;
            }

            bool visible = _topAutoHideButtons.Count > 0;
            _topAutoHideButtons.Visible = visible;
            _topAutoHideSpacer.Visible  = visible;
         }


         if (_bottomAutoHideButtons != null)
         {
            _bottomAutoHideButtons.Top = _host.ClientSize.Height - sizeLimit;

            if (HasLeftButtons)
            {
               _bottomAutoHideButtons.Left = sizeLimit;
            }
            else
            {
               _bottomAutoHideButtons.Left = 0;
            }

            if (HasRightButtons)
            {
               _bottomAutoHideButtons.Width = _host.ClientSize.Width - _bottomAutoHideButtons.Left - sizeLimit;
            }
            else
            {
               _bottomAutoHideButtons.Width = _host.ClientSize.Width - _bottomAutoHideButtons.Left;
            }

            bool visible = _bottomAutoHideButtons.Count > 0;
            _bottomAutoHideButtons.Visible = visible;
            _bottomAutoHideSpacer.Visible  = visible;
         }
      }


      /// <summary>
      /// Hide restore containers
      /// </summary>
      /// <param name="panel">panel</param>
      public static void HideRestoreContainers(AutoHidePanel panel)
      {
         DockableContainer parentContainer = panel.RestoreParent;
         List<DockableContainer> containersToHide = new List<DockableContainer>();
         while (parentContainer != null)
         {
            int visibleChildrenCount = GetVisibleChildrenCount(parentContainer);
            if (visibleChildrenCount > 1)
            {
               break;
            }

            containersToHide.Add(parentContainer);
            parentContainer = parentContainer.Parent as DockableContainer;
         }

         for (int index = containersToHide.Count - 1; index >= 0; index--)
         {
            parentContainer = containersToHide[index];
            int splitterIndex  = parentContainer.Parent.Controls.IndexOf(parentContainer.Splitter);
            int containerIndex = parentContainer.Parent.Controls.IndexOf(parentContainer);
            parentContainer.Splitter.Visible = false;
            parentContainer.Visible = false;
            parentContainer.SplitterBefore = splitterIndex < containerIndex;
         }

      }

      #endregion Public section

      #region Private section
      #region Received events

      /// <summary>
      /// On host size changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnHostSizeChanged(object sender, EventArgs e)
      {
         ArrangeAutoButtonsPanels();
      }

      /// <summary>
      /// On host visible changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnHostVisibleChanged(object sender, EventArgs e)
      {
         ArrangeAutoButtonsPanels();
      }


      /// <summary>
      /// On autohide pane select button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnLeftPaneSelectButton(object sender, ControlEventArgs e)
      {
         StartShowForm((Form)e.Control, e.Control.Width, LeftAutohideButtons.Height);
      }

      /// <summary>
      /// On autohide pane preview button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnLeftPaneShowPreview(object sender, PreviewEventArgs e)
      {
         if (_autoShowPanel != null)
         {
            return;
         }

         _previewPane.Size = _leftRightPreviewSize;
         _previewPane.Left = LeftAutohideButtons.Right;

         int y = e.ButtonLocation.Y;

         if (y < LeftAutohideButtons.Top)
         {
            y = LeftAutohideButtons.Top;
         }
         else if (y + _previewPane.Height > LeftAutohideButtons.Bottom)
         {
            y = LeftAutohideButtons.Bottom - _previewPane.Height;
         }

         _previewPane.Top = y;

         if (_previewPane.Renderer != null)
         {
            _previewPane.Renderer.SelectedForm = e.Form;
         }

         _previewPane.Visible = PreviewRenderer != null;
      }


      /// <summary>
      /// On autohide pane select button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnRightPaneSelectButton(object sender, ControlEventArgs e)
      {
         StartShowForm((Form)e.Control, e.Control.Width, RightAutohideButtons.Height);
      }

      /// <summary>
      /// On autohide pane preview button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnRightPaneShowPreview(object sender, PreviewEventArgs e)
      {
         if (_autoShowPanel != null)
         {
            return;
         }

         _previewPane.Size = _leftRightPreviewSize;
         _previewPane.Left = RightAutohideButtons.Left - _previewPane.Width;

         int y = e.ButtonLocation.Y;

         if (y < RightAutohideButtons.Top)
         {
            y = RightAutohideButtons.Top;
         }
         else if (y + _previewPane.Height > RightAutohideButtons.Bottom)
         {
            y = RightAutohideButtons.Bottom - _previewPane.Height;
         }

         _previewPane.Top = y;

         if (_previewPane.Renderer != null)
         {
            _previewPane.Renderer.SelectedForm = e.Form;
         }

         _previewPane.Visible = PreviewRenderer != null;
      }


      /// <summary>
      /// On autohide pane select button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnTopPaneSelectButton(object sender, ControlEventArgs e)
      {
         StartShowForm((Form)e.Control, TopAutohideButtons.Width, e.Control.Height);
      }

      /// <summary>
      /// On autohide pane preview button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnTopPaneShowPreview(object sender, PreviewEventArgs e)
      {
         if (_autoShowPanel != null)
         {
            return;
         }

         _previewPane.Size = _topBottomPreviewSize;

         int x = e.ButtonLocation.X;

         if (x < TopAutohideButtons.Left)
         {
            x = TopAutohideButtons.Left;
         }
         else if (x + _previewPane.Width > TopAutohideButtons.Right)
         {
            x = TopAutohideButtons.Right - _previewPane.Width;
         }

         _previewPane.Left = x;

         _previewPane.Top = TopAutohideButtons.Bottom;

         if (_previewPane.Renderer != null)
         {
            _previewPane.Renderer.SelectedForm = e.Form;
         }

         _previewPane.Visible = PreviewRenderer != null;
      }


      /// <summary>
      /// On autohide pane select button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnBottomPaneSelectButton(object sender, ControlEventArgs e)
      {
         StartShowForm((Form)e.Control, BottomAutohideButtons.Width, e.Control.Height);
      }

      /// <summary>
      /// On autohide pane preview button
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnBottomPaneShowPreview(object sender, PreviewEventArgs e)
      {
         if (_autoShowPanel != null)
         {
            return;
         }

         _previewPane.Size = _topBottomPreviewSize;

         int x = e.ButtonLocation.X;

         if (x < BottomAutohideButtons.Left)
         {
            x = BottomAutohideButtons.Left;
         }
         else if (x + _previewPane.Width > BottomAutohideButtons.Right)
         {
            x = BottomAutohideButtons.Right - _previewPane.Width;
         }

         _previewPane.Left = x;

         _previewPane.Top = BottomAutohideButtons.Top - _previewPane.Height;

         if (_previewPane.Renderer != null)
         {
            _previewPane.Renderer.SelectedForm = e.Form;
         }

         _previewPane.Visible = PreviewRenderer != null;
      }

      #endregion Received events

      /// <summary>
      /// Show restore container
      /// </summary>
      /// <param name="container">container</param>
      private static void ShowRestoreContainer(DockableContainer container)
      {
         container.Visible          = true;
         container.Splitter.Visible = true;

         if (container.Parent as DockableContainer != null)
         {
            return;
         }

         int splitterIndex  = container.Parent.Controls.IndexOf(container.Splitter);
         int containerIndex = container.Parent.Controls.IndexOf(container);
         if (container.SplitterBefore)
         {
            if (splitterIndex - containerIndex != -1)
            {
               container.Parent.Controls.SetChildIndex(container, splitterIndex + 1);

               splitterIndex  = container.Parent.Controls.IndexOf(container.Splitter);
               containerIndex = container.Parent.Controls.IndexOf(container);

               container.Parent.Controls.SetChildIndex(container, splitterIndex + 1);
            }
         }
         else
         {
            if (splitterIndex - containerIndex != 1)
            {
               container.Parent.Controls.SetChildIndex(container, splitterIndex - 1);

               splitterIndex  = container.Parent.Controls.IndexOf(container.Splitter);
               containerIndex = container.Parent.Controls.IndexOf(container);

               container.Parent.Controls.SetChildIndex(container, splitterIndex + 1);
            }
         }
      }

      /// <summary>
      /// Show restore containers
      /// </summary>
      /// <param name="panel">panel</param>
      private static void ShowRestoreContainers(AutoHidePanel panel)
      {
         DockableContainer parentContainer = panel.RestoreParent;
         ShowRestoreContainer(parentContainer);

         parentContainer = parentContainer.Parent as DockableContainer;

         while (parentContainer != null)
         {
            if (parentContainer.Visible == false)
            {
               ShowRestoreContainer(parentContainer);
               parentContainer = parentContainer.Parent as DockableContainer;
            }
            else
            {
               break;
            }
         }
      }


      /// <summary>
      /// On set logical dock
      /// </summary>
      /// <param name="view">view</param>
      /// <param name="dock">dock</param>
      private void OnSetHostContainerDock(FormsTabbedView view, DockStyle dock)
      {
         EventHandler<DockControlEventArgs> handler = SetHostContainerDock;
         if (handler != null)
         {
            DockControlEventArgs args = new DockControlEventArgs(view, dock, zDockMode.Inner);
            handler(this, args);
         }
      }



      /// <summary>
      /// Accessor of the left autohide buttons
      /// </summary>
      private AutoHideButtonsPanel LeftAutohideButtons
      {
         get
         {
            if (_leftAutoHideButtons == null)
            {
               _leftAutoHideButtons = new AutoHideButtonsPanel();
               _leftAutoHideButtons.ShowPreview  += OnLeftPaneShowPreview;
               _leftAutoHideButtons.SelectButton += OnLeftPaneSelectButton;

               _leftAutoHideSpacer = new AutoHideButtonsPanel();

               _host.AddLast(_leftAutoHideSpacer);
               _host.AddFirst(_leftAutoHideButtons);
               
               _host.KeepLast[0]   = _leftAutoHideSpacer;
               _host.KeepFirst[0 + _firstControlOffset] = _leftAutoHideButtons;

               _leftAutoHideSpacer.Dock = DockStyle.Left;
               _leftAutoHideSpacer.LogicalDock  = DockStyle.None;
               _leftAutoHideButtons.LogicalDock = DockStyle.Left;

               ArrangeAutoButtonsPanels();
            }

            return _leftAutoHideButtons;
         }
      }

      /// <summary>
      /// Accessor of the right autohide buttons
      /// </summary>
      private AutoHideButtonsPanel RightAutohideButtons
      {
         get
         {
            if (_rightAutoHideButtons == null)
            {
               _rightAutoHideButtons = new AutoHideButtonsPanel();
               _rightAutoHideButtons.ShowPreview  += OnRightPaneShowPreview;
               _rightAutoHideButtons.SelectButton += OnRightPaneSelectButton;

               _rightAutoHideSpacer = new AutoHideButtonsPanel();

               _host.AddLast(_rightAutoHideSpacer);
               _host.AddFirst(_rightAutoHideButtons);

               _host.KeepLast[1]  = _rightAutoHideSpacer;
               _host.KeepFirst[1 + _firstControlOffset] = _rightAutoHideButtons;

               _rightAutoHideSpacer.Dock  = DockStyle.Right;
               _rightAutoHideSpacer.LogicalDock  = DockStyle.None;
               _rightAutoHideButtons.LogicalDock = DockStyle.Right;

               ArrangeAutoButtonsPanels();
            }

            return _rightAutoHideButtons;
         }
      }

      /// <summary>
      /// Accessor of the top autohide buttons
      /// </summary>
      private AutoHideButtonsPanel TopAutohideButtons
      {
         get
         {
            if (_topAutoHideButtons == null)
            {
               _topAutoHideButtons = new AutoHideButtonsPanel();
               _topAutoHideButtons.ShowPreview  += OnTopPaneShowPreview;
               _topAutoHideButtons.SelectButton += OnTopPaneSelectButton;

               _topAutoHideSpacer = new AutoHideButtonsPanel();

               _host.AddLast(_topAutoHideSpacer);
               _host.AddFirst(_topAutoHideButtons);

               _host.KeepLast[2]  = _topAutoHideSpacer;
               _host.KeepFirst[2 + _firstControlOffset] = _topAutoHideButtons;

               _topAutoHideSpacer.Dock = DockStyle.Top;
               _topAutoHideSpacer.LogicalDock  = DockStyle.None;
               _topAutoHideButtons.LogicalDock = DockStyle.Top;

               ArrangeAutoButtonsPanels();
            }

            return _topAutoHideButtons;
         }
      }

      /// <summary>
      /// Accessor of the bottom autohide buttons
      /// </summary>
      private AutoHideButtonsPanel BottomAutohideButtons
      {
         get
         {
            if (_bottomAutoHideButtons == null)
            {
               _bottomAutoHideButtons = new AutoHideButtonsPanel();
               _bottomAutoHideButtons.ShowPreview  += OnBottomPaneShowPreview;
               _bottomAutoHideButtons.SelectButton += OnBottomPaneSelectButton;

               _bottomAutoHideSpacer = new AutoHideButtonsPanel();

               _host.AddLast(_bottomAutoHideSpacer);
               _host.AddFirst(_bottomAutoHideButtons);

               _host.KeepLast[3]  = _bottomAutoHideSpacer;
               _host.KeepFirst[3 + _firstControlOffset] = _bottomAutoHideButtons;

               _bottomAutoHideSpacer.Dock = DockStyle.Bottom;
               _bottomAutoHideSpacer.LogicalDock  = DockStyle.None;
               _bottomAutoHideButtons.LogicalDock = DockStyle.Bottom;

               ArrangeAutoButtonsPanels();
            }

            return _bottomAutoHideButtons;
         }
      }

      /// <summary>
      /// Start auto-show pane process
      /// </summary>
      /// <param name="panelToShow">panel to show</param>
      private void StartAutoShowPane(AutoHidePanel panelToShow)
      {
         if (_autoShowPanel != panelToShow)
         {
            _animationCommand.Handler = null;

            _autoHidePanel = _autoShowPanel;
            _autoShowPanel = panelToShow;

            if (_autoShowPanel != null)
            {
               _host.MoveFirst(_autoShowPanel);
            }

            if (_autoHidePanel != null)
            {
               _animationCommand.Handler = _autoHidePanel.AutoHideHandler;
            }
            else if (_autoShowPanel != null)
            {
               _animationCommand.Handler = _autoShowPanel.AutoShowHandler;
            }
            else
            {
               _animationCommand.Handler = HideAutoPaneWhenMouseExits;
            }
         }
      }



      /// <summary>
      /// Flag indicating if has left buttons
      /// </summary>
      private bool HasLeftButtons
      {
         get
         {
            if (_leftAutoHideButtons == null)
            {
               return false;
            }

            return _leftAutoHideButtons.Count > 0;
         }
      }

      /// <summary>
      /// Flag indicating if has right buttons
      /// </summary>
      private bool HasRightButtons
      {
         get
         {
            if (_rightAutoHideButtons == null)
            {
               return false;
            }

            return _rightAutoHideButtons.Count > 0;
         }
      }

      /// <summary>
      /// Flag indicating if has top buttons
      /// </summary>
      private bool HasTopButtons
      {
         get
         {
            if (_topAutoHideButtons == null)
            {
               return false;
            }

            return _topAutoHideButtons.Count > 0;
         }
      }

      /// <summary>
      /// Flag indicating if has bottom buttons
      /// </summary>
      private bool HasBottomButtons
      {
         get
         {
            if (_bottomAutoHideButtons == null)
            {
               return false;
            }

            return _bottomAutoHideButtons.Count > 0;
         }
      }


      /// <summary>
      /// Autohide left pane
      /// </summary>
      private void AutoHideLeftPane()
      {
         if (_autoHidePanel != null)
         {
            if (_autoHidePanel.Visible)
            {
               if (_autoHidePanel.Right <= LeftAutohideButtons.Right)
               {
                  _autoHidePanel.Visible = false;

                  if (_autoShowPanel != null && _autoShowPanel != _autoHidePanel)
                  {
                     _animationCommand.Handler = _autoShowPanel.AutoShowHandler;
                     return;
                  }
                  else
                  {
                     _animationCommand.Handler = HideAutoPaneWhenMouseExits;
                  }
               }
               else
               {
                  _autoHidePanel.Left -= _animationSpeed;

                  return;
               }
            }
         }

         _animationCommand.Handler = HideAutoPaneWhenMouseExits;
      }

      /// <summary>
      /// Autoshow left pane
      /// </summary>
      private void AutoShowLeftPane()
      {
         if (_autoShowPanel != null)
         {
            if (_autoShowPanel.Visible == false)
            {
               _autoShowPanel.Left    = LeftAutohideButtons.Right - _autoShowPanel.Width;
               _autoShowPanel.Top     = LeftAutohideButtons.Top;
               _autoShowPanel.Height  = LeftAutohideButtons.Height;
               _autoShowPanel.Visible = true;
            }

            if (_autoShowPanel.Left + _animationSpeed  >= LeftAutohideButtons.Right)
            {
               _autoShowPanel.Left       = LeftAutohideButtons.Right;
               _animationCommand.Handler = HideAutoPaneWhenMouseExits;
            }
            else
            {
               _autoShowPanel.Left += _animationSpeed;
            }
         }
      }

      /// <summary>
      /// Autohide right pane
      /// </summary>
      private void AutoHideRightPane()
      {
         if (_autoHidePanel != null)
         {
            if (_autoHidePanel.Visible)
            {
               if (_autoHidePanel.Left >= RightAutohideButtons.Left)
               {
                  _autoHidePanel.Visible = false;

                  if (_autoShowPanel != null && _autoShowPanel != _autoHidePanel)
                  {
                     _animationCommand.Handler = _autoShowPanel.AutoShowHandler;
                     return;
                  }
                  else
                  {
                     _animationCommand.Handler = HideAutoPaneWhenMouseExits;
                  }
               }
               else
               {
                  _autoHidePanel.Left += _animationSpeed;

                  return;
               }
            }
         }

         _animationCommand.Handler = HideAutoPaneWhenMouseExits;
      }

      /// <summary>
      /// Autoshow right pane
      /// </summary>
      private void AutoShowRightPane()
      {
         if (_autoShowPanel != null)
         {
            if (_autoShowPanel.Visible == false)
            {
               _autoShowPanel.Left    = RightAutohideButtons.Left;
               _autoShowPanel.Top     = RightAutohideButtons.Top;
               _autoShowPanel.Height  = RightAutohideButtons.Height;
               _autoShowPanel.Visible = true;
            }

            if (_autoShowPanel.Right - _animationSpeed <= RightAutohideButtons.Left)
            {
               _autoShowPanel.Left    = RightAutohideButtons.Left - _autoShowPanel.Width;
               _animationCommand.Handler = HideAutoPaneWhenMouseExits;
            }
            else
            {
               _autoShowPanel.Left -= _animationSpeed;
            }
         }
      }

      /// <summary>
      /// Autohide top pane
      /// </summary>
      private void AutoHideTopPane()
      {
         if (_autoHidePanel != null)
         {
            if (_autoHidePanel.Visible)
            {
               if (_autoHidePanel.Bottom <= TopAutohideButtons.Bottom)
               {
                  _autoHidePanel.Visible = false;

                  if (_autoShowPanel != null && _autoShowPanel != _autoHidePanel)
                  {
                     _animationCommand.Handler = _autoShowPanel.AutoShowHandler;
                     return;
                  }
                  else
                  {
                     _animationCommand.Handler = HideAutoPaneWhenMouseExits;
                  }
               }
               else
               {
                  _autoHidePanel.Top -= _animationSpeed;

                  return;
               }
            }
         }

         _animationCommand.Handler = HideAutoPaneWhenMouseExits;
      }

      /// <summary>
      /// Autoshow top pane
      /// </summary>
      private void AutoShowTopPane()
      {
         if (_autoShowPanel != null)
         {
            if (_autoShowPanel.Visible == false)
            {
               _autoShowPanel.Top     = TopAutohideButtons.Bottom - _autoShowPanel.Height;
               _autoShowPanel.Left    = TopAutohideButtons.Left;
               _autoShowPanel.Width   = TopAutohideButtons.Width;
               _autoShowPanel.Visible = true;
            }

            if (_autoShowPanel.Top + _animationSpeed >= TopAutohideButtons.Bottom)
            {
               _autoShowPanel.Top     = TopAutohideButtons.Bottom;
               _animationCommand.Handler = HideAutoPaneWhenMouseExits;
            }
            else
            {
               _autoShowPanel.Top += _animationSpeed;
            }
         }
      }

      /// <summary>
      /// Autohide bottom pane
      /// </summary>
      private void AutoHideBottomPane()
      {
         if (_autoHidePanel != null)
         {
            if (_autoHidePanel.Visible)
            {
               if (_autoHidePanel.Top >= BottomAutohideButtons.Top)
               {
                  _autoHidePanel.Visible = false;

                  if (_autoShowPanel != null && _autoShowPanel != _autoHidePanel)
                  {
                     _animationCommand.Handler = _autoShowPanel.AutoShowHandler;
                     return;
                  }
                  else
                  {
                     _animationCommand.Handler = HideAutoPaneWhenMouseExits;
                  }
               }
               else
               {
                  _autoHidePanel.Top += _animationSpeed;

                  return;
               }
            }
         }

         _animationCommand.Handler = HideAutoPaneWhenMouseExits;
      }

      /// <summary>
      /// Autoshow bottom pane
      /// </summary>
      private void AutoShowBottomPane()
      {
         if (_autoShowPanel != null)
         {
            if (_autoShowPanel.Visible == false)
            {
               _autoShowPanel.Top     = BottomAutohideButtons.Top;
               _autoShowPanel.Left    = BottomAutohideButtons.Left;
               _autoShowPanel.Width   = BottomAutohideButtons.Width;
               _autoShowPanel.Visible = true;
            }

            if (_autoShowPanel.Bottom - _animationSpeed <= BottomAutohideButtons.Top)
            {
               _autoShowPanel.Top     = BottomAutohideButtons.Top - _autoShowPanel.Height;
               _animationCommand.Handler = HideAutoPaneWhenMouseExits;
            }
            else
            {
               _autoShowPanel.Top -= _animationSpeed;
            }
         }
      }

      /// <summary>
      /// Get the count of visible children
      /// </summary>
      /// <param name="container">container</param>
      /// <returns>count of visible children</returns>
      private static int GetVisibleChildrenCount(DockableContainer container)
      {
         int count = 0;

         foreach (Control control in container.Controls)
         {
            if (control.Visible)
            {
               count++;
            }
         }

         return count;
      }

      /// <summary>
      /// Start showing form
      /// </summary>
      /// <param name="form">form to show</param>
      /// <param name="width">width of the form</param>
      /// <param name="height">height of the form</param>
      private void StartShowForm(Form form, int width, int height)
      {
         _previewPane.Visible = false;

         FormsTabbedView view = HierarchyUtility.GetTabbedView(form);
         view.Size = new Size(width, height);
         view.SelectPage(form);
         StartAutoShowPane(view.Parent as AutoHidePanel);
      }

      /// <summary>
      /// Check if the point is in spacers
      /// </summary>
      /// <param name="clientLocation">client location</param>
      /// <returns>true if the point is in spacers</returns>
      private bool IsPointInSpacers(Point clientLocation)
      {
         if (_leftAutoHideSpacer != null)
         {
            if (_leftAutoHideSpacer.Bounds.Contains(clientLocation))
            {
               return true;
            }
         }

         if (_rightAutoHideSpacer != null)
         {
            if (_rightAutoHideSpacer.Bounds.Contains(clientLocation))
            {
               return true;
            }
         }

         if (_topAutoHideSpacer != null)
         {
            if (_topAutoHideSpacer.Bounds.Contains(clientLocation))
            {
               return true;
            }
         }

         if (_bottomAutoHideSpacer != null)
         {
            if (_bottomAutoHideSpacer.Bounds.Contains(clientLocation))
            {
               return true;
            }
         }

         return false;
      }

      #endregion Private section
   }
}
