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

using Crom.Controls.TabbedDocument;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Implementation of dockable form info
   /// </summary>
   public class DockableFormInfo : Disposable
   {
      #region Fields

      private Guid            _identifier             = Guid.Empty;
      private Form            _dockableForm           = null;
      private zAllowedDock    _allowedDock            = zAllowedDock.All;
      private DockStyle       _hostContainerDock      = DockStyle.None;
      private DockStyle       _autoHideSavedDock      = DockStyle.None;
      private DockStyle       _dock                   = DockStyle.None;
      private zDockMode       _dockMode               = zDockMode.Outer;
      private TabButton       _button                 = null;
      private bool            _isSelected             = false;
      private bool            _isAutoHideMode         = false;
      private bool            _showCloseButton        = true;
      private bool            _showContextMenuButton  = true;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="form">form</param>
      /// <param name="allowedDock">allowed dock mode</param>
      /// <param name="identifier">identifier of the form info</param>
      internal DockableFormInfo(Form form, zAllowedDock allowedDock, Guid identifier)
      {
         if (identifier == Guid.Empty)
         {
            throw new ArgumentException("No identifier found.");
         }

         _identifier    = identifier;
         _dockableForm  = form;
         _allowedDock   = allowedDock;
         _button        = new TabButton(form);
         _button.ExplicitDisposing += OnButtonDisposing;

         form.GotFocus += OnFormGotFocus;
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when the control IsSelected property was changed
      /// </summary>
      public event EventHandler SelectedChanged;

      /// <summary>
      /// Occurs when the control IsAutoHideMode property was changed
      /// </summary>
      public event EventHandler AutoHideModeChanged;

      /// <summary>
      /// Show auto panel
      /// </summary>
      public event EventHandler ShowAutoPanel;


      /// <summary>
      /// Equality operator
      /// </summary>
      /// <param name="a">first form info</param>
      /// <param name="b">second form info</param>
      /// <returns></returns>
      public static bool operator ==(DockableFormInfo a, DockableFormInfo b)
      {
         if (((object)a) == ((object)b))
         {
            return true;
         }

         if (((object)a) == null || ((object)b) == null)
         {
            return false;
         }

         if (a._dockableForm == b._dockableForm)
         {
            if (a._identifier != b._identifier)
            {
               throw new InvalidOperationException("Err");
            }

            return true;
         }

         return false;
      }

      /// <summary>
      /// Inequality operator
      /// </summary>
      /// <param name="a">first form info</param>
      /// <param name="b">second form info</param>
      /// <returns></returns>
      public static bool operator !=(DockableFormInfo a, DockableFormInfo b)
      {
         return (a == b) == false;
      }


      /// <summary>
      /// Accessor of the dockable form identifier
      /// </summary>
      public Guid Id
      {
         get
         {
            ValidateNotDisposed();

            return _identifier;
         }
      }

      /// <summary>
      /// Accessor of the dockable form
      /// </summary>
      public Form DockableForm
      {
         get 
         {
            ValidateNotDisposed();

            return _dockableForm; 
         }
      }

      /// <summary>
      /// Accessor of the allowed dock mode
      /// </summary>
      public zAllowedDock AllowedDock
      {
         get
         {
            ValidateNotDisposed();

            return _allowedDock;
         }
      }

      /// <summary>
      /// Accessor of the form current logical dock
      /// </summary>
      public DockStyle HostContainerDock
      {
         get
         {
            ValidateNotDisposed();

            return _hostContainerDock;
         }
         internal set
         {
            ValidateNotDisposed();

            if (_hostContainerDock != value)
            {
               if (value == Globals.DockAutoHide)
               {
                  _autoHideSavedDock = _hostContainerDock;
               }

               _hostContainerDock = value;
            }
         }
      }

      /// <summary>
      /// Accessor of the form current saved auto-hide dock
      /// </summary>
      public DockStyle AutoHideSavedDock
      {
         get
         {
            ValidateNotDisposed();

            return _autoHideSavedDock;
         }
      }

      /// <summary>
      /// Accessor of the form current dock
      /// </summary>
      public DockStyle Dock
      {
         get
         {
            ValidateNotDisposed();

            return _dock;
         }
         internal set
         {
            ValidateNotDisposed();

            _dock = value;
         }
      }

      /// <summary>
      /// Accessor of the form current dock
      /// </summary>
      public zDockMode DockMode
      {
         get
         {
            ValidateNotDisposed();

            return _dockMode;
         }
         internal set
         {
            ValidateNotDisposed();

            _dockMode = value;
         }
      }

      /// <summary>
      /// Button associated with the form
      /// </summary>
      internal TabButton Button
      {
         get
         {
            ValidateNotDisposed();

            return _button;
         }
      }


      /// <summary>
      /// Show close button
      /// </summary>
      public bool ShowCloseButton
      {
         get { return _showCloseButton; }
         set { _showCloseButton = value; }
      }

      /// <summary>
      /// Show context menu button
      /// </summary>
      public bool ShowContextMenuButton
      {
         get { return _showContextMenuButton; }
         set { _showContextMenuButton = value; }
      }

      /// <summary>
      /// Accessor of IsSelected flag
      /// </summary>
      public bool IsSelected
      {
         get
         {
            ValidateNotDisposed();

            return _isSelected;
         }
         set
         {
            ValidateNotDisposed();

            if (_isSelected != value)
            {
               _isSelected = value;

               EventHandler handler = SelectedChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of IsAutoHideMode flag
      /// </summary>
      public bool IsAutoHideMode
      {
         get
         {
            ValidateNotDisposed();

            return _isAutoHideMode;
         }
         internal set
         {
            ValidateNotDisposed();

            if (_isAutoHideMode != value)
            {
               _isAutoHideMode = value;

               EventHandler handler = AutoHideModeChanged;
               if (handler != null)
               {
                  handler(this, EventArgs.Empty);
               }
            }
         }
      }


      /// <summary>
      /// Show form auto panel
      /// </summary>
      public void ShowFormAutoPanel()
      {
         EventHandler handler = ShowAutoPanel;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }


      /// <summary>
      /// Check if this instance is equal with the obj instance
      /// </summary>
      /// <param name="obj">obj</param>
      /// <returns>true for equalty</returns>
      public override bool Equals(object obj)
      {
         ValidateNotDisposed();

         DockableFormInfo dockable = obj as DockableFormInfo;
         if (dockable != (DockableFormInfo)null)
         {
            return this == dockable;
         }

         Form form = obj as Form;

         return DockableForm == form;
      }

      /// <summary>
      /// Gets the hash code for this instance
      /// </summary>
      /// <returns>hash code</returns>
      public override int GetHashCode()
      {
         ValidateNotDisposed();

         return DockableForm.GetHashCode();
      }

      /// <summary>
      /// Text
      /// </summary>
      /// <returns>text</returns>
      public override string ToString()
      {
         if (DockableForm != null)
         {
            return "DFI: " + DockableForm.ToString();
         }

         return base.ToString();
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
            _button.ExplicitDisposing -= OnButtonDisposing;
            _button.Dispose();

            _dockableForm.GotFocus -= OnFormGotFocus;
            _dockableForm = null;
         }
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// Occurs when the button is disposed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnButtonDisposing(object sender, EventArgs e)
      {
         Dispose();
      }

      /// <summary>
      /// Occurs when the form got focus
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnFormGotFocus(object sender, EventArgs e)
      {
         IsSelected = true;
      }

      #endregion Private section
   }
}
