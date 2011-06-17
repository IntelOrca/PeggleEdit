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
using System.Drawing;
using System.Windows.Forms;

using Crom.Controls.TabbedDocument;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Tabed host control
   /// </summary>
   internal class FormsTabbedView : ButtonsPanel
   {
      #region Fields

      private FormsTabbedViewControlCollection _controls = null;

      private bool                     _isDocked               = false;
      private DockStyle                _hostContainerDock      = Globals.DockNotSet;
      private DockStyle                _dock                   = Globals.DockNotSet;
      private zDockMode                _dockMode               = zDockMode.Inner;
      private Size                     _floatingSize           = new Size();
      private Form                     _selectedForm           = null;
      private List<DockableFormInfo>   _cachedInfos            = new List<DockableFormInfo>();

      private int                      _movedIndex             = -1;
      private Form                     _movedForm              = null;
      private TabButton                _movedButton            = null;
      private FormsDecorator           _movedDecorator         = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="pagesPanel">pages panel</param>
      public FormsTabbedView()
      {
         PagesPanel.ContextButtonClick   += OnPagesContextMenuClick;
         PagesPanel.AutohideButtonClick  += OnPagesAutohideClick;
         PagesPanel.CloseButtonClick     += OnPagesCloseClick;
         PagesPanel.ColorsChanged        += OnPagesColorsChanged;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Undock a form
      /// </summary>
      public event EventHandler<FormEventArgs> UndockForm;

      /// <summary>
      /// Occurs when autohide button was clicked
      /// </summary>
      public event EventHandler AutohideClick;

      /// <summary>
      /// Occurs when context menu button was clicked
      /// </summary>
      public event EventHandler<FormEventArgs> ContextMenuClick;

      /// <summary>
      /// Occurs when a form is closing
      /// </summary>
      public event EventHandler<DockableFormClosingEventArgs> FormClosing;

      /// <summary>
      /// Occurs when close button was clicked
      /// </summary>
      public event EventHandler<FormEventArgs> FormClosed;

      /// <summary>
      /// Occurs when a form was removed from view
      /// </summary>
      public event EventHandler<FormEventArgs> FormRemoved;

      /// <summary>
      /// Occurs when a form was added to view
      /// </summary>
      public event EventHandler<FormEventArgs> FormAdded;

      /// <summary>
      /// Occurs when a form was selected into view
      /// </summary>
      public event EventHandler<FormEventArgs> FormSelected;


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
      /// <param name="pageInfo">tab page info</param>
      public void Add(DockableFormInfo pageInfo)
      {
         if (pageInfo.AllowedDock != AllowedDock && AllowedDock != zAllowedDock.Unknown)
         {
            throw new InvalidOperationException("Err");
         }

         pageInfo.DockableForm.SetBounds(-15000, 0, PagesPanel.Width, PagesPanel.Height);
         pageInfo.DockableForm.Dock = DockStyle.None;
         PagesPanel.Add(pageInfo.DockableForm);

         _cachedInfos.Add(pageInfo);

         pageInfo.ExplicitDisposing += OnInfoDisposing;
         pageInfo.SelectedChanged   += OnInfoSelectedChanged;

         pageInfo.HostContainerDock = HostContainerDock;

         pageInfo.DockableForm.FormClosing   += OnPageClosing;
         pageInfo.DockableForm.FormClosed    += OnPageClosed;
         pageInfo.DockableForm.ParentChanged += OnPageParentChanged;

         AddButton(pageInfo.Button);

         IsFocused = true;
      }

      /// <summary>
      /// Remove an existing tab page
      /// </summary>
      /// <param name="pageInfo">page info to be removed</param>
      public bool Remove(DockableFormInfo pageInfo)
      {
         return RemoveButton(pageInfo.Button);
      }

      /// <summary>
      /// Move all the pages to given view
      /// </summary>
      /// <param name="view">view where to move the pages</param>
      /// <returns>moved pages</returns>
      public DockableFormInfo[] MovePagesTo(FormsTabbedView view)
      {
         List<DockableFormInfo> movedPages = new List<DockableFormInfo>();

         if (view == this)
         {
            return movedPages.ToArray();
         }


         for (int index = _cachedInfos.Count - 1; index >= 0; index--)
         {
            DockableFormInfo info = _cachedInfos[index];
            Remove(info);
            view.Add(info);

            movedPages.Add(info);
         }

         return movedPages.ToArray();
      }

      /// <summary>
      /// Get the page at given index
      /// </summary>
      /// <param name="pageIndex">zero based page index</param>
      /// <returns>page at given index</returns>
      public Form GetPageAt(int pageIndex)
      {
         TabButton buton = GetButtonAt(pageIndex);
         if (buton == null)
         {
            return null;
         }

         return (Form)buton.Page;
      }

      /// <summary>
      /// Get page info at given index
      /// </summary>
      /// <param name="pageIndex">zero based page index</param>
      /// <returns>info</returns>
      public DockableFormInfo GetPageInfoAt(int pageIndex)
      {
         ValidateNotDisposed();

         Form page = GetPageAt(pageIndex);
         if (page == null)
         {
            return null;
         }

         return GetPageInfo(page);
      }

      /// <summary>
      /// Gets the page info
      /// </summary>
      /// <remarks>Throw an exception if the form is not part of this view</remarks>
      /// <param name="form">form</param>
      /// <returns>info</returns>
      public DockableFormInfo GetPageInfo(Form form)
      {
         foreach (DockableFormInfo cachedInfo in _cachedInfos)
         {
            if (cachedInfo.DockableForm == form)
            {
               return cachedInfo;
            }
         }

         throw new InvalidOperationException("Err");
      }

      /// <summary>
      /// Gets the index of the page
      /// </summary>
      /// <param name="page">page</param>
      /// <returns>index of the page</returns>
      public int GetPageIndex(Form page)
      {
         int pageIndex  = -1;

         for (int index = 0; index < Count; index++)
         {
            if (GetPageAt(index) == page)
            {
               pageIndex = index;
               break;
            }
         }

         return pageIndex;
      }

      /// <summary>
      /// Get pages
      /// </summary>
      /// <returns>pages</returns>
      public Form[] GetPages()
      {
         Form[] pages = new Form[Count];
         for (int index = 0; index < Count; index++)
         {
            pages[index] = GetPageAt(index);
         }

         return pages;
      }

      /// <summary>
      /// Select given page
      /// </summary>
      /// <param name="page">page</param>
      public void SelectPage(Form page)
      {
         for (int index = 0; index < Count; index++)
         {
            if (page == GetPageAt(index))
            {
               SelectedIndex = index;
               return;
            }
         }
      }

      /// <summary>
      /// Change is docked state
      /// </summary>
      public bool IsDocked
      {
         get { return _isDocked; }
         private set
         {
            if (_isDocked != value)
            {
               _isDocked = value;
               PagesPanel.CanResizeByMouse = IsDocked == false;
               SetCutRoundRect(IsDocked == false && HostContainerDock != Globals.DockAutoHide, true);
            }
         }
      }

      /// <summary>
      /// Flag indicating if can move the control
      /// </summary>
      public bool CanMoveByMouse
      {
         get { return PagesPanel.CanMoveByMouse; }
         set { PagesPanel.CanMoveByMouse = value; }
      }

      /// <summary>
      /// Accessor of IsAutoHideMode flag
      /// </summary>
      public bool IsAutoHideMode
      {
         get
         {
            ValidateNotDisposed();

            if (_cachedInfos.Count > 0)
            {
               return _cachedInfos[0].IsAutoHideMode;
            }

            return false;
         }
         set
         {
            ValidateNotDisposed();

            if (IsAutoHideMode != value)
            {
               foreach (DockableFormInfo info in _cachedInfos)
               {
                  info.IsAutoHideMode = value;
               }
            }
         }
      }

      /// <summary>
      /// Change is focused state
      /// </summary>
      public bool IsFocused
      {
         get { return PagesPanel.IsFocused; }
         set 
         {
            if (PagesPanel.IsFocused != value)
            {
               PagesPanel.IsFocused = value;

               if (value && SelectedForm != null)
               {
                  SelectedForm.Activate();
                  SelectedForm.Focus();
               }
            }
         }
      }

      /// <summary>
      /// Accessor for allowed dock
      /// </summary>
      public zAllowedDock AllowedDock
      {
         get
         {
            ValidateNotDisposed();

            if (_cachedInfos.Count > 0)
            {
               return _cachedInfos[0].AllowedDock;
            }

            return zAllowedDock.Unknown;
         }
      }

      /// <summary>
      /// Accessor of the logical dock
      /// </summary>
      public DockStyle HostContainerDock
      {
         get { return _hostContainerDock; }
      }

      /// <summary>
      /// Accessor of the dock
      /// </summary>
      public DockStyle CurrentDock
      {
         get { return _dock; }
      }

      /// <summary>
      /// Accessor of the logical dock mode
      /// </summary>
      public zDockMode CurrentDockMode
      {
         get { return _dockMode; }
      }

      /// <summary>
      /// Accessor of the floating size
      /// </summary>
      public Size FloatingSize
      {
         get { return _floatingSize; }
      }

      /// <summary>
      /// Save floating size
      /// </summary>
      public void SaveFloatingSize()
      {
         _floatingSize = Size;
      }

      /// <summary>
      /// Set logical dock
      /// </summary>
      /// <param name="hostContainerDock">host container dock</param>
      /// <param name="dock">dock</param>
      /// <param name="mode">mode</param>
      public void SetDock(DockStyle hostContainerDock, DockStyle dock, zDockMode mode)
      {
         if (_hostContainerDock != hostContainerDock)
         {
            if (AllowedDock != zAllowedDock.Unknown)
            {
               bool isAllowed = false;
               switch (hostContainerDock)
               {
                  case DockStyle.Left:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.Left);
                     break;

                  case DockStyle.Right:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.Right);
                     break;

                  case DockStyle.Top:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.Top);
                     break;

                  case DockStyle.Bottom:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.Bottom);
                     break;

                  case DockStyle.None:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.None);
                     break;

                  case DockStyle.Fill:
                     isAllowed = EnumUtility.Contains(AllowedDock, zAllowedDock.Fill);
                     break;

                  case Globals.DockAutoHide:
                     isAllowed = _hostContainerDock == DockStyle.Left  || 
                                 _hostContainerDock == DockStyle.Right || 
                                 _hostContainerDock == DockStyle.Top   ||
                                 _hostContainerDock == DockStyle.Bottom;
                     break;
               }

               if (isAllowed == false)
               {
                  throw new NotSupportedException("Err");
               }
            }

            _hostContainerDock = hostContainerDock;

            IsDocked = hostContainerDock != DockStyle.None && hostContainerDock != Globals.DockAutoHide;

            PagesPanel.ShowAutohideButton = (hostContainerDock != DockStyle.None && hostContainerDock != DockStyle.Fill);
         }

         _dock     = dock;
         _dockMode = mode;

         foreach (DockableFormInfo info in _cachedInfos)
         {
            info.HostContainerDock = _hostContainerDock;
            info.Dock     = _dock;
            info.DockMode = _dockMode;
         }
      }

      /// <summary>
      /// Positioner
      /// </summary>
      public ControlPositioner Positioner
      {
         get { return PagesPanel.Positioner; }
         set { PagesPanel.Positioner = value; }
      }

      /// <summary>
      /// Accessor of the text
      /// </summary>
      public override string Text
      {
         get
         {
            if (SelectedForm != null)
            {
               return SelectedForm.Text;
            }

            return string.Empty;
         }
         set
         {
            if (SelectedForm != null)
            {
               SelectedForm.Text = value;
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
            if (SelectedForm != null)
            {
               return SelectedForm.Icon;
            }

            return null;
         }
         set
         {
            if (SelectedForm != null)
            {
               SelectedForm.Icon = value;
            }
         }
      }

      /// <summary>
      /// Accessor of the selected form
      /// </summary>
      public Form SelectedForm
      {
         get { return _selectedForm; }
         private set 
         {
            if (_selectedForm != value)
            {
               DockableFormInfo info = null;
               if (value != null)
               {
                  info = GetPageInfo(value);
               }

               _selectedForm = value;

               PagesPanel.SelectForm(_selectedForm);

               EventHandler<FormEventArgs> handler = FormSelected;
               if (handler != null)
               {
                  FormEventArgs args = null;
                  if (info != null)
                  {
                     args = new FormEventArgs(info.DockableForm, info.Id);
                  }
                  else
                  {
                     args = new FormEventArgs(null, Guid.Empty);
                  }
                  handler(this, args);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the pages panel
      /// </summary>
      public FormsDecorator PagesPanel
      {
         get { return InternalControls.PagesPanel; }
      }

      /// <summary>
      /// Text representation of this view
      /// </summary>
      /// <returns>text representation of this view</returns>
      public override string ToString()
      {
         if (SelectedForm != null)
         {
            return "FTV: " + SelectedForm.Text;
         }

         return "FTV";
      }

      #endregion Public section.

      #region Protected section

      /// <summary>
      /// Create controls
      /// </summary>
      /// <returns>controls</returns>
      protected override ControlCollection CreateControlsInstance()
      {
         return InternalControls;
      }

      /// <summary>
      /// Occurs when the pages panel bounds were changed
      /// </summary>
      /// <param name="bounds">new bounds</param>
      protected override void OnPagesPanelBoundsChanged(Rectangle bounds)
      {
         if (PagesPanel != null)
         {
            PagesPanel.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            OnSelectedIndexSet(EventArgs.Empty);
         }

         base.OnPagesPanelBoundsChanged(bounds);
      }

      /// <summary>
      /// Occurs after selected index was set
      /// </summary>
      /// <param name="e">event argument</param>
      protected override void OnSelectedIndexSet(EventArgs e)
      {
         DockableFormInfo info = GetPageInfoAt(SelectedIndex);
         if (info != null)
         {
            info.IsSelected   = true;
            SelectedForm      = info.DockableForm;
         }

         Invalidate();

         base.OnSelectedIndexSet(e);
      }

      /// <summary>
      /// Gets the margins for pages panel
      /// </summary>
      protected override int PagesPanelMargins
      {
         get { return 4; }
      }

      /// <summary>
      /// Gets the radius for round rect
      /// </summary>
      protected override int RoundRadius
      {
         get { return 2; }
      }

      /// <summary>
      /// Count of caption buttons
      /// </summary>
      protected override int CaptionButtonsCount
      {
         get { return 0; }
      }

      /// <summary>
      /// Set cut round rect state
      /// </summary>
      /// <param name="cutRect">cut rect</param>
      /// <param name="forceNow">force now</param>
      protected void SetCutRoundRect(bool cutRect, bool forceNow)
      {
         CutRoundRect = cutRect;

         if (forceNow)
         {
            UpdateSize();
         }
      }

      /// <summary>
      /// Draw round border
      /// </summary>
      /// <param name="e">e</param>
      protected override void DrawRoundBorder(PaintEventArgs e)
      {
         if (CutRoundRect)
         {
            base.DrawRoundBorder(e);
         }
      }

      /// <summary>
      /// Begin button drag
      /// </summary>
      /// <param name="selected">selected button</param>
      /// <param name="mousePosition">mouse position</param>
      /// <param name="cursor">cursor</param>
      /// <returns>true if button drag is started</returns>
      protected override bool BeginButtonDrag(TabButton selected, Point mousePosition, ref Cursor cursor)
      {
         if (CanMoveByMouse == false)
         {
            return true;
         }

         EventHandler<FormEventArgs> handler = UndockForm;
         if (handler != null)
         {
            _movedButton   = selected;
            _movedForm     = (Form)_movedButton.Page;
            _movedIndex    = SelectedIndex;

            Point mouseScreenPosition = Control.MousePosition;
            FormEventArgs args  = new FormEventArgs(_movedForm, GetPageInfo(_movedForm).Id);
            handler(this, args);

            RemoveButton(_movedButton);

            _movedDecorator = HierarchyUtility.GetFormsDecorator(_movedForm);

            IsFocused = false;
            _movedDecorator.IsFocused = true;

            _movedDecorator.BeginMovementByMouse(mouseScreenPosition);

            cursor = Cursors.SizeAll;
         }

         return true;
      }

      /// <summary>
      /// Continue button drag
      /// </summary>
      /// <param name="mousePosition">mouse position</param>
      /// <param name="cursor">cursor</param>
      protected override void ContinueButtonDrag(Point mousePosition, ref Cursor cursor)
      {
         if (_movedDecorator != null)
         {
            Point mouseScreenPosition = Control.MousePosition;
            _movedDecorator.ContinueMovementByMouse(mouseScreenPosition);

            cursor = Cursors.SizeAll;
         }
      }

      /// <summary>
      /// End button drag
      /// </summary>
      /// <param name="cancel">cancel</param>
      protected override void EndButtonDrag(bool cancel)
      {
         if (cancel)
         {
            InsertButton(_movedButton, _movedIndex);
         }
         else if (_movedDecorator != null)
         {
            _movedDecorator.EndMovementByMouse();
            _movedDecorator = null;
         }
      }

      /// <summary>
      /// Occurs when tab button is removed
      /// </summary>
      /// <param name="button">button removed</param>
      protected override void OnButtonRemoved(TabButton button)
      {
         Form page = (Form)button.Page;
         page.FormClosing    -= OnPageClosing;
         page.FormClosed     -= OnPageClosed;
         page.ParentChanged  -= OnPageParentChanged;

         PagesPanel.Remove(page);

         DockableFormInfo info = null;
         for (int index = _cachedInfos.Count - 1; index >= 0; index--)
         {
            if (_cachedInfos[index].Button == button)
            {
               info = _cachedInfos[index];
               _cachedInfos.RemoveAt(index);

               info.ExplicitDisposing -= OnInfoDisposing;
               info.SelectedChanged   -= OnInfoSelectedChanged;

               break;
            }
         }

         EventHandler<FormEventArgs> handler = FormRemoved;
         if (handler != null)
         {
            FormEventArgs args = new FormEventArgs(page, info.Id);
            handler(this, args);
         }
      }

      /// <summary>
      /// Occurs when tab button is added
      /// </summary>
      /// <param name="button">button added</param>
      protected override void OnButtonAdded(TabButton button)
      {
         EventHandler<FormEventArgs> handler = FormAdded;
         if (handler != null)
         {
            DockableFormInfo info = GetPageInfo((Form)button.Page);
            FormEventArgs args    = new FormEventArgs(info.DockableForm, info.Id);
            handler(this, args);
         }
      }

      /// <summary>
      /// Called when tab button renderer is changed
      /// </summary>
      protected override void OnTabButonRendererChanged()
      {
         ButtonsRenderer.SelectedBackGradient2 = PagesPanel.SelectedColor1;
      }

      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// Occurs when info is disposing
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnInfoDisposing(object sender, EventArgs e)
      {
         Remove((DockableFormInfo)sender);
      }

      /// <summary>
      /// Occurs when info is selected
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnInfoSelectedChanged(object sender, EventArgs e)
      {
         DockableFormInfo info = (DockableFormInfo)sender;
         if (info.IsSelected)
         {
            SelectedIndex = GetPageIndex(info.DockableForm);
         }
      }

      /// <summary>
      /// Occurs when pages close button was clicked
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPagesCloseClick(object sender, EventArgs e)
      {
         Form form = GetPageAt(SelectedIndex);
         form.Close();
      }

      /// <summary>
      /// Occurs when pages autohide button was clicked
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPagesAutohideClick(object sender, EventArgs e)
      {
         EventHandler handler = AutohideClick;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      /// <summary>
      /// Occurs when pages context menu button was clicked
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPagesContextMenuClick(object sender, EventArgs e)
      {
         EventHandler<FormEventArgs> handler = ContextMenuClick;
         if (handler != null)
         {
            DockableFormInfo info = GetPageInfo(PagesPanel.GetFormAt(SelectedIndex));
            FormEventArgs args = new FormEventArgs(info.DockableForm, info.Id);
            handler(this, args);
         }
      }

      /// <summary>
      /// Occurs when pages colors were changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event arguments</param>
      private void OnPagesColorsChanged(object sender, EventArgs e)
      {
         ButtonsRenderer.SelectedBackGradient2 = PagesPanel.SelectedColor1;
      }

      /// <summary>
      /// Occurs when the page parent is changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPageParentChanged(object sender, EventArgs e)
      {
         TabButton button = GetButton((Form)sender);
         RemoveButton(button);
      }

      /// <summary>
      /// Occurs when the page is closed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPageClosed(object sender, FormClosedEventArgs e)
      {
         DockableFormInfo info = GetPageInfo((Form)sender);
         Guid id          = info.Id;
         Form form        = info.DockableForm;
         TabButton button = GetButton(form);
         button.Dispose();

         EventHandler<FormEventArgs> handler = FormClosed;
         if (handler != null)
         {
            FormEventArgs args = new FormEventArgs(form, id);
            handler(this, args);
         }
      }

      /// <summary>
      /// Occurs when the page is closing
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnPageClosing(object sender, FormClosingEventArgs e)
      {
         EventHandler<DockableFormClosingEventArgs> handler = FormClosing;
         if (handler != null)
         {
            DockableFormInfo info = GetPageInfo((Form)sender);
            DockableFormClosingEventArgs args = new DockableFormClosingEventArgs(info.DockableForm, info.Id);
            args.Cancel = false;
            handler(this, args);

            e.Cancel = args.Cancel;
         }
      }

      #endregion Received events

      /// <summary>
      /// Accessor of the controls
      /// </summary>
      private FormsTabbedViewControlCollection InternalControls
      {
         get
         {
            if (_controls == null)
            {
               _controls = new FormsTabbedViewControlCollection(this);
            }

            return _controls;
         }
      }

      /// <summary>
      /// Get the button
      /// </summary>
      /// <param name="form">form that created the button</param>
      /// <returns>button</returns>
      private TabButton GetButton(Form form)
      {
         for (int index = 0; index < Count; index++)
         {
            if (GetButtonAt(index).Page == form)
            {
               return GetButtonAt(index);
            }
         }

         throw new InvalidOperationException();
      }

      #endregion Private section
   }
}
