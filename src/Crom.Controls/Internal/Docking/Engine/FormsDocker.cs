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

using Crom.Controls.TabbedDocument;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Forms docker
   /// </summary>
   internal class FormsDocker : Disposable
   {
      #region Fields

      private FormWrapper              _host                            = null;

      private DockGuider               _guider                          = null;
      private DockLayout               _layout                          = null;
      private Autohide                 _autohide                        = null;

      private Timer                    _animationTimer                  = new Timer();
      private Command                  _animationCommand                = new Command();

      private List<DockableFormInfo>   _dockableForms                   = new List<DockableFormInfo>();
      private DockableFormInfo         _selectedForm                    = null;

      private FormsSelector            _switchSelector                  = null;

      private FocusFilter              _focusDetector                   = new FocusFilter();

      private Color                    _titleBarGradientColor1          = SystemColors.Control;
      private Color                    _titleBarGradientColor2          = Color.White;
      private Color                    _titleBarGradientSelectedColor1  = Color.DarkGray;
      private Color                    _titleBarGradientSelectedColor2  = Color.White;
      private Color                    _titleBarTextColor               = Color.Black;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="host">host window</param>
      public FormsDocker(IWin32Window host)
      {
         _host      = new FormWrapper(host, 14, 4);
         _guider    = new DockGuider (_host);
         _layout    = new DockLayout (_host);
         _autohide  = new Autohide   (_host, 9, _animationCommand);

         _layout.BeginMoveByMouse        += OnPositionerBeginMoveByMouse;
         _layout.MoveByMouse             += OnPositionerMoveByMouse;
         _layout.EndMoveByMouse          += OnPositionerEndMoveByMouse;
         _layout.DestroyFormsTabbedView  += OnDestroyFormsTabbedView;
         _layout.ShowFloatingWindows     += OnShowFloatingWindows;

         _autohide.SetHostContainerDock  += OnSetHostContainerDock;

         _guider.ApplyDock     += OnApplyDock;
         _animationTimer.Tick  += OnTimedAnimation;

         _animationTimer.Interval = 50;
         _animationTimer.Enabled  = true;

         _focusDetector.ControlGotFocus += OnControlGotFocus;
         _focusDetector.MessageFiltered += OnMessageFiltered;
         Application.AddMessageFilter(_focusDetector);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when show context menu is requested
      /// </summary>
      public event EventHandler<FormContextMenuEventArgs> ShowContextMenu;

      /// <summary>
      /// Occurs when a form is about to be closed
      /// </summary>
      public event EventHandler<DockableFormClosingEventArgs> FormClosing;

      /// <summary>
      /// Occurs after a form was closed
      /// </summary>
      public event EventHandler<FormEventArgs> FormClosed;


      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color TitleBarGradientColor1
      {
         get { return _titleBarGradientColor1; }
         set
         {
            if (_titleBarGradientColor1 != value)
            {
               _titleBarGradientColor1 = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view   = HierarchyUtility.GetTabbedView(info.DockableForm);
                  view.PagesPanel.Color1 = value;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color TitleBarGradientColor2
      {
         get { return _titleBarGradientColor2; }
         set
         {
            if (_titleBarGradientColor2 != value)
            {
               _titleBarGradientColor2 = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view   = HierarchyUtility.GetTabbedView(info.DockableForm);
                  view.PagesPanel.Color2 = value;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 1
      /// </summary>
      public Color TitleBarGradientSelectedColor1
      {
         get { return _titleBarGradientSelectedColor1; }
         set
         {
            if (_titleBarGradientSelectedColor1 != value)
            {
               _titleBarGradientSelectedColor1 = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
                  view.PagesPanel.SelectedColor1 = value;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the color 2
      /// </summary>
      public Color TitleBarGradientSelectedColor2
      {
         get { return _titleBarGradientSelectedColor2; }
         set
         {
            if (_titleBarGradientSelectedColor2 != value)
            {
               _titleBarGradientSelectedColor2 = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
                  view.PagesPanel.SelectedColor2 = value;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the text color
      /// </summary>
      public Color TitleBarTextColor
      {
         get { return _titleBarTextColor; }
         set
         {
            if (_titleBarTextColor != value)
            {
               _titleBarTextColor = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
                  view.PagesPanel.TextColor = value;
               }
            }
         }
      }



      /// <summary>
      /// Flag indicating if the filled forms can be moved by mouse
      /// </summary>
      public bool CanMoveByMouseFilledForms
      {
         get { return _layout.CanMoveByMouseFilledForms; }
         set 
         {
            if (_layout.CanMoveByMouseFilledForms != value)
            {
               _layout.CanMoveByMouseFilledForms = value;

               foreach (DockableFormInfo info in _dockableForms)
               {
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
                  if (view.HostContainerDock == DockStyle.Fill)
                  {
                     view.CanMoveByMouse = value;
                  }
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the switch selector
      /// </summary>
      public FormsSelector SwitchSelector
      {
         get { return _switchSelector; }
         set { _switchSelector = value; }
      }

      /// <summary>
      /// Accessor of the preview renderer
      /// </summary>
      public PreviewRenderer PreviewRenderer
      {
         get { return _autohide.PreviewRenderer; }
         set { _autohide.PreviewRenderer = value; }
      }

      /// <summary>
      /// Accessor of the dockable forms count
      /// </summary>
      public int Count
      {
         get
         {
            ValidateNotDisposed();

            return _dockableForms.Count;
         }
      }

      /// <summary>
      /// Add form to guider
      /// </summary>
      /// <param name="form">form to guide</param>
      /// <param name="allowedDock">allowed dock</param>
      /// <param name="formIdentifier">identifier of the form added</param>
      /// <returns>object that encapsulates relevant information for the guided form</returns>
      public DockableFormInfo Add(Form form, zAllowedDock allowedDock, Guid formIdentifier)
      {
         if (GetFormInfo(form) != null)
         {
            throw new ArgumentException("Err");
         }

         // Should set the border as None to prevent Microsoft bug in TextBox:
         // TextBox on Form with TopLevel = False and FormBorderStyle != None doesn't process Click event
         form.FormBorderStyle = FormBorderStyle.None;

         Rectangle bounds = form.Bounds;

         DockableFormInfo info = new DockableFormInfo(form, allowedDock, formIdentifier);
         info.ExplicitDisposing  += OnInfoDisposing;
         info.SelectedChanged    += OnFormSelectedChanged;
         info.ShowAutoPanel      += OnShowFormAutoPanel;

         _dockableForms.Add(info);

         FormsTabbedView view  = CreateFormsTabbedView(bounds.Size, null);
         view.Add(info);

         _layout.CreateFloatingContainer(view, bounds);

         return info;
      }

      /// <summary>
      /// Gets the info of the form at given index
      /// </summary>
      /// <param name="index">zero based index of the form</param>
      /// <returns>info of specified form</returns>
      public DockableFormInfo GetFormInfoAt(int index)
      {
         ValidateNotDisposed();

         return _dockableForms[index];
      }

      /// <summary>
      /// Get form info
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>info</returns>
      public DockableFormInfo GetFormInfo(Form form)
      {
         ValidateNotDisposed();

         for (int index = 0; index < _dockableForms.Count; index++)
         {
            if (_dockableForms[index].DockableForm == form)
            {
               return _dockableForms[index];
            }
         }

         return null;
      }

      /// <summary>
      /// Get form info based on identifier
      /// </summary>
      /// <param name="identifier">identifier</param>
      /// <returns>form info</returns>
      public DockableFormInfo GetFormInfo(Guid identifier)
      {
         ValidateNotDisposed();

         for (int index = 0; index < _dockableForms.Count; index++)
         {
            if (_dockableForms[index].Id == identifier)
            {
               return _dockableForms[index];
            }
         }

         return null;
      }

      /// <summary>
      /// Dock a form previously added to guider
      /// </summary>
      /// <param name="info">info about the form to dock</param>
      /// <param name="dock">how to dock</param>
      /// <param name="mode">were to dock</param>
      public void Dock(DockableFormInfo info, DockStyle dock, zDockMode mode)
      {
         DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);
         _layout.DockControl(container, null, dock, mode);
      }

      /// <summary>
      /// Dock a form previously added to guider over another form previously added to guider
      /// </summary>
      /// <param name="info">info about the form to dock</param>
      /// <param name="infoOver">info about form over which to dock</param>
      /// <param name="dock">how to dock</param>
      /// <param name="mode">were to dock</param>
      public void DockOver(DockableFormInfo info, DockableFormInfo infoOver, DockStyle dock, zDockMode mode)
      {
         DockableContainer containerToDock      = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);
         DockableContainer containerWhereToDock = null;

         if (infoOver != null)
         {
            containerWhereToDock = HierarchyUtility.GetClosestDockableContainer(infoOver.DockableForm);
         }

         _layout.DockControl(containerToDock, containerWhereToDock, dock, mode);
      }

      /// <summary>
      /// Undock form
      /// </summary>
      /// <param name="info">info about form to undock</param>
      /// <param name="hintBounds">hint bounds</param>
      public void Undock(DockableFormInfo info, Rectangle hintBounds)
      {
         Undock(info.DockableForm, hintBounds);
      }

      /// <summary>
      /// Remove the form
      /// </summary>
      /// <param name="info">info about form to remove</param>
      public void Remove(DockableFormInfo info)
      {
         if (info == null)
         {
            return;
         }

         FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);

         if (view != null)
         {
            if (view.Count == 1 && view.IsDocked)
            {
               _layout.Undock(view);
            }

            if (view.Count == 1)
            {
               _host.Remove(view.Parent);

               DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);
               container.SetModeEmpty();

               view.Remove(info);
            }
            else
            {
               if (view.Remove(info) == false)
               {
                  if (SelectedFormInfo == info)
                  {
                     SelectedFormInfo = null;
                  }

                  return;
               }
            }
         }

         if (SelectedFormInfo == info)
         {
            SelectedFormInfo = null;
         }

         _dockableForms.Remove(info);
         info.SelectedChanged    -= OnFormSelectedChanged;
         info.ShowAutoPanel      -= OnShowFormAutoPanel;
         info.ExplicitDisposing  -= OnInfoDisposing;

         if (info.IsAutoHideMode)
         {
            _autohide.ArrangeAutoButtonsPanels();
         }

         info.Dispose();
      }

      /// <summary>
      /// Clear
      /// </summary>
      public void Clear()
      {
         for (int index = Count - 1; index >= 0; index--)
         {
            Remove(_dockableForms[index]);
         }
      }

      /// <summary>
      /// Show switch preview
      /// </summary>
      public void ShowSwitchPreview()
      {
         if (SwitchSelector != null)
         {
            SwitchSelector.Show(PreviewRenderer, _host.ScreenClientRectangle, _dockableForms.ToArray());
         }
      }

      /// <summary>
      /// Accessor of the selected form
      /// </summary>
      public Form SelectedForm
      {
         get
         {
            ValidateNotDisposed();

            if (SelectedFormInfo != null)
            {
               return SelectedFormInfo.DockableForm;
            }

            return null;
         }
         set
         {
            ValidateNotDisposed();

            DockableFormInfo info = GetFormInfo(value);
            info.IsSelected = true;
         }
      }

      /// <summary>
      /// Set auto-hide mode
      /// </summary>
      /// <param name="info">info of the form to set</param>
      /// <param name="autoHide">flag indicating if should set auto-hide (true) or unset it</param>
      public void SetAutoHide(DockableFormInfo info, bool autoHide)
      {
         if (autoHide && info.IsAutoHideMode == false)
         {
            _autohide.SetAutoHideMode(HierarchyUtility.GetTabbedView(info.DockableForm));
         }
         else if (autoHide == false && info.IsAutoHideMode)
         {
            _autohide.UnsetAutoHideMode(HierarchyUtility.GetTabbedView(info.DockableForm));
         }
      }

      /// <summary>
      /// Set the width of a form docked: left or right or fill
      /// </summary>
      /// <param name="info">info identifing the form</param>
      /// <param name="newWidth">new width value</param>
      public void SetWidth(DockableFormInfo info, int newWidth)
      {
         int delta = newWidth - info.DockableForm.Width;
         if (delta == 0)
         {
            return;
         }

         DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);

         List<DockableContainer> hierarchy = new List<DockableContainer>();
         List<int> modifiers = new List<int>();
         hierarchy.Add(container);
         modifiers.Add(delta);

         DockableContainer containerParent = container.Parent as DockableContainer;
         while (containerParent != null)
         {
            hierarchy.Add(containerParent);
            modifiers.Add(containerParent.Width - container.Width + delta);

            container       = containerParent;
            containerParent = containerParent.Parent as DockableContainer;
         }

         for (int index = hierarchy.Count - 1; index >= 0; index--)
         {
            hierarchy[index].Width += modifiers[index];
         }
      }

      /// <summary>
      /// Set the height of a form docked: top or bottom or fill
      /// </summary>
      /// <param name="info">info identifing the form</param>
      /// <param name="newHeight">new height value</param>
      public void SetHeight(DockableFormInfo info, int newHeight)
      {
         int delta = newHeight - info.DockableForm.Height;
         if (delta == 0)
         {
            return;
         }

         DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);

         List<DockableContainer> hierarchy = new List<DockableContainer>();
         List<int> modifiers = new List<int>();
         hierarchy.Add(container);
         modifiers.Add(delta);

         DockableContainer containerParent = container.Parent as DockableContainer;
         while (containerParent != null)
         {
            hierarchy.Add(containerParent);
            modifiers.Add(containerParent.Height - container.Height + delta);

            container       = containerParent;
            containerParent = containerParent.Parent as DockableContainer;
         }

         for (int index = hierarchy.Count - 1; index >= 0; index--)
         {
            hierarchy[index].Height += modifiers[index];
         }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose the current instance
      /// </summary>
      /// <param name="fromIDisposableDispose">dispose was called from IDisposable.Dispose</param>
      protected override void Dispose(bool fromIDisposableDispose)
      {
         if (fromIDisposableDispose)
         {
            if (_animationTimer != null)
            {
               _animationTimer.Tick -= OnTimedAnimation;
               _animationTimer.Dispose();
               _animationTimer = null;
            }
         }
      }

      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// Occurs when focus message was filtered
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnMessageFiltered(object sender, TemplateEventArgs<Message> e)
      {
         CheckSwitcherFocus();
      }

      /// <summary>
      /// Occurs when control got focus
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnControlGotFocus(object sender, TemplateEventArgs<IntPtr> e)
      {
         Control control = Control.FromHandle(e.Data);
         while (control != null)
         {
            if (control as Form != null)
            {
               break;
            }

            if (control as FormsTabbedView != null)
            {
               control = ((FormsTabbedView)control).SelectedForm;
               break;
            }

            control = control.Parent;
         }

         Form focusedForm = control as Form;

         if (focusedForm != null)
         {
            foreach (DockableFormInfo info in _dockableForms)
            {
               if (info.DockableForm == focusedForm)
               {
                  SelectedFormInfo = info;
                  break;
               }
            }
         }

         return;
      }


      /// <summary>
      /// On show floating windows
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnShowFloatingWindows(object sender, EventArgs e)
      {
         ShowFloatingWindows();
      }

      /// <summary>
      /// On destroy forms tabbed view
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnDestroyFormsTabbedView(object sender, ControlEventArgs e)
      {
         DestroyFormsTabbedView((FormsTabbedView)e.Control);
      }

      /// <summary>
      /// On begin move a positioner using the mouse
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPositionerBeginMoveByMouse(object sender, ControlEventArgs e)
      {
         DockableContainer container = (DockableContainer)e.Control;
         if (container.SingleChild == null)
         {
            return;
         }

         _guider.BeginWindowMovement(container, container.SingleChild.AllowedDock);
      }

      /// <summary>
      /// On moving a positioner using the mouse
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPositionerMoveByMouse(object sender, EventArgs e)
      {
         _guider.MoveWindowByMouse();
      }

      /// <summary>
      /// On end moving a positioner using the mouse
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPositionerEndMoveByMouse(object sender, EventArgs e)
      {
         _guider.EndWindowMovement();
      }


      /// <summary>
      /// On set logical dock
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnSetHostContainerDock(object sender, DockControlEventArgs e)
      {
         FormsTabbedView view = (FormsTabbedView)e.Control;

         _layout.SetViewDock(view, e.Dock, view.CurrentDock, view.CurrentDockMode);
      }


      /// <summary>
      /// On apply dock
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnApplyDock(object sender, DockControlEventArgs e)
      {
         Point screenPoint = Control.MousePosition;

         DockableContainer containerToDock      = (DockableContainer)e.Control;
         DockableContainer containerWhereToDock = null;

         if (e.DockMode == zDockMode.Inner)
         {
            containerWhereToDock = _guider.GetLeafDockedContainerFromPoint(screenPoint, e.Control);
         }

         _layout.DockControl(containerToDock, containerWhereToDock, e.Dock, e.DockMode);
      }



      /// <summary>
      /// Occurs when a dockable form was closed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnFormClosed(object sender, FormEventArgs e)
      {
         Remove(GetFormInfo(e.Form));

         EventHandler<FormEventArgs> handler = FormClosed;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      /// <summary>
      /// Occurs when a dockable form is closing
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnFormClosing(object sender, DockableFormClosingEventArgs e)
      {
         EventHandler<DockableFormClosingEventArgs> closingHandler = FormClosing;
         if (closingHandler != null)
         {
            closingHandler(this, e);
         }
      }

      /// <summary>
      /// Occurs when pages autohide button was clicked
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnViewAutoHideClick(object sender, EventArgs e)
      {
         FormsTabbedView view = (FormsTabbedView)sender;
         if (view.PagesPanel.AutoHidden)
         {
            _autohide.UnsetAutoHideMode(view);
         }
         else
         {
            _autohide.SetAutoHideMode(view);
         }
      }

      /// <summary>
      /// Occurs when pages context menu button was clicked
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnViewContextMenuClick(object sender, FormEventArgs e)
      {
         EventHandler<FormContextMenuEventArgs> handler = ShowContextMenu;
         if (handler != null)
         {
            Point menuLocation = Control.MousePosition;
            menuLocation       = e.Form.PointToClient(menuLocation);
            menuLocation.Y     = 0;
            FormContextMenuEventArgs args = new FormContextMenuEventArgs(menuLocation, e.Form, e.FormId);
            handler(this, args);
         }
      }

      /// <summary>
      /// Occurs when undock form from view is raised
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnUndockFormFromView(object sender, FormEventArgs e)
      {
         Point position = _host.PointToClient(Control.MousePosition);
         Undock(e.Form, new Rectangle(position.X - 150, position.Y - 4, 300, 300));
      }



      /// <summary>
      /// On autohide check
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnTimedAnimation(object sender, EventArgs e)
      {
         CheckSwitcherFocus();

         if (_animationCommand.Handler != null)
         {
            _animationCommand.Handler();
         }
         else
         {
            _animationCommand.Handler = _autohide.HideAutoPaneWhenMouseExits;
         }
      }


      /// <summary>
      /// Occurs when IsSelected property of a form is changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnFormSelectedChanged(object sender, EventArgs e)
      {
         DockableFormInfo info = (DockableFormInfo)sender;
         
         if (info.IsSelected)
         {
            SelectedFormInfo = info;
         }

         FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
         if (view != null)
         {
            if (view.SelectedForm != null)
            {
               DockableFormInfo topFormInfo = GetFormInfo(view.SelectedForm);

               view.PagesPanel.ShowCloseButton        = topFormInfo.ShowCloseButton;
               view.PagesPanel.ShowContextMenuButton  = topFormInfo.ShowContextMenuButton;
            }
         }         
      }

      /// <summary>
      /// Occurs when ShowAuto property of a form is changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnShowFormAutoPanel(object sender, EventArgs e)
      {
         if (SelectedFormInfo != null)
         {
            if (SelectedFormInfo.IsAutoHideMode)
            {
               _autohide.ShowAutoForm(SelectedFormInfo.DockableForm);
            }
         }
      }

      /// <summary>
      /// Occurs when form info is disposing
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnInfoDisposing(object sender, EventArgs e)
      {
         Remove((DockableFormInfo)sender);
      }

      #endregion Received events

      /// <summary>
      /// Create forms tabbed view
      /// </summary>
      /// <param name="size">initial size of decorator</param>
      /// <param name="positioner">positioner for control</param>
      /// <returns>forms tabbed view</returns>
      private FormsTabbedView CreateFormsTabbedView(Size size, ControlPositioner positioner)
      {
         FormsTabbedView view = new FormsTabbedView();

         view.ShowOneTabButton      = false;
         view.Size                  = size;
         view.PagesPanel.Positioner = positioner;
         view.PagesPanel.Size       = size;
         view.PagesPanel.BackColor  = SystemColors.Control;
         view.PagesPanel.TextColor  = TitleBarTextColor;
         view.PagesPanel.Color1     = TitleBarGradientColor1;
         view.PagesPanel.Color2     = TitleBarGradientColor2;
         view.PagesPanel.SelectedColor1 = TitleBarGradientSelectedColor1;
         view.PagesPanel.SelectedColor2 = TitleBarGradientSelectedColor2;

         view.UndockForm         += OnUndockFormFromView;
         view.ContextMenuClick   += OnViewContextMenuClick;
         view.AutohideClick      += OnViewAutoHideClick;
         view.FormClosing        += OnFormClosing;
         view.FormClosed         += OnFormClosed;

         return view;
      }

      /// <summary>
      /// Destroy forms tabbed view
      /// </summary>
      /// <param name="formTabbedView">form tabbed view</param>
      private void DestroyFormsTabbedView(FormsTabbedView formTabbedView)
      {
         formTabbedView.UndockForm        -= OnUndockFormFromView;
         formTabbedView.ContextMenuClick  -= OnViewContextMenuClick;
         formTabbedView.AutohideClick     -= OnViewAutoHideClick;
         formTabbedView.FormClosing       -= OnFormClosing;
         formTabbedView.FormClosed        -= OnFormClosed;

         formTabbedView.Dispose();
      }
      
      /// <summary>
      /// Show the floating windows
      /// </summary>
      private void ShowFloatingWindows()
      {
         List<DockableContainer> containersToBringInFront = new List<DockableContainer>();

         AddSortedFloatingContainers(containersToBringInFront);
         AddSortedFillContainers(containersToBringInFront);

         for (int index = containersToBringInFront.Count - 1; index >= 0; index--)
         {
            _host.MoveFirst(containersToBringInFront[index]);
         }
      }

      /// <summary>
      /// Add sorted fill containers to the list
      /// </summary>
      /// <param name="containers">containers</param>
      private void AddSortedFillContainers(List<DockableContainer> containers)
      {
         List<DockableContainer> containersToBringInFront = new List<DockableContainer>();

         foreach (DockableFormInfo info in _dockableForms)
         {
            FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
            if (view.HostContainerDock != DockStyle.Fill)
            {
               continue;
            }

            DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);
            while (true)
            {
               if (container.Parent as DockableContainer != null)
               {
                  container = (DockableContainer)container.Parent;
               }
               else
               {
                  break;
               }
            }

            Debug.Assert(_host.Contains(container), "Floating form views must have their parent in form");

            if (containersToBringInFront.Contains(container) == false)
            {
               containersToBringInFront.Add(container);
            }
         }

         SortContainersBasedOnTheirZOrder(containersToBringInFront);

         containers.AddRange(containersToBringInFront);
      }

      /// <summary>
      /// Add sorted floating containers to the list
      /// </summary>
      /// <param name="containers">containers</param>
      private void AddSortedFloatingContainers(List<DockableContainer> containers)
      {
         List<DockableContainer> containersToBringInFront = new List<DockableContainer>();

         foreach (DockableFormInfo info in _dockableForms)
         {
            FormsTabbedView view = HierarchyUtility.GetTabbedView(info.DockableForm);
            if (view.IsDocked)
            {
               continue;
            }

            DockableContainer container = HierarchyUtility.GetClosestDockableContainer(info.DockableForm);
            if (_host.Contains(container) == false)
            {
               continue;
            }

            if (containersToBringInFront.Contains(container) == false)
            {
               containersToBringInFront.Add(container);
            }
         }

         SortContainersBasedOnTheirZOrder(containersToBringInFront);

         containers.AddRange(containersToBringInFront);
      }

      /// <summary>
      /// Sort the containers from the list, based on their z-order
      /// </summary>
      /// <param name="containers">containers to sort</param>
      private void SortContainersBasedOnTheirZOrder(List<DockableContainer> containers)
      {
         Comparison<DockableContainer> containersSorterHandler = delegate(DockableContainer container1, DockableContainer container2)
         {
            if (container1 == container2)
            {
               return 0;
            }

            int indexView1 = _host.GetZOrderIndex(container1);
            int indexView2 = _host.GetZOrderIndex(container2);

            return Math.Sign(indexView2 - indexView1);
         };

         containers.Sort(containersSorterHandler);
      }

      /// <summary>
      /// Accessor of selected form info
      /// </summary>
      private DockableFormInfo SelectedFormInfo
      {
         get { return _selectedForm; }
         set 
         {
            if (_selectedForm != value)
            {
               if (_selectedForm != null)
               {
                  _selectedForm.IsSelected = false;
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(_selectedForm.DockableForm);
                  if (view != null)
                  {
                     view.IsFocused = false;
                  }
               }

               _selectedForm = value;

               if (_selectedForm != null)
               {
                  _selectedForm.IsSelected = true;
                  FormsTabbedView view = HierarchyUtility.GetTabbedView(_selectedForm.DockableForm);
                  if (view != null)
                  {
                     view.IsFocused     = false;
                     view.SelectedIndex = view.GetPageIndex(_selectedForm.DockableForm);
                     view.IsFocused     = true;
                  }
               }
            }
         }
      }

      /// <summary>
      /// Check switcher focus
      /// </summary>
      private void CheckSwitcherFocus()
      {
         if (SwitchSelector != null)
         {
            if (SwitchSelector.HasFocus == false)
            {
               SwitchSelector.Cancel();
            }
         }
      }

      /// <summary>
      /// Undock view
      /// </summary>
      /// <param name="view">view to undock</param>
      /// <param name="hintBounds">hint bounds</param>
      private void Undock(Form formToUndock, Rectangle hintBounds)
      {
         FormsTabbedView view = HierarchyUtility.GetTabbedView(formToUndock);
         if (view.Count == 1)
         {
            _layout.Undock(view, hintBounds);
         }
         else
         {
            DockableFormInfo info = GetFormInfo(formToUndock);

            FormsTabbedView newView = CreateFormsTabbedView(info.DockableForm.Size, null);
            newView.Add(info);

            _layout.CreateFloatingContainer(newView, hintBounds);
         }
      }

      #endregion Private section
   }
}
