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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Container for dockable tool windows. Place this control on your form to enable window guided docking.
   /// </summary>
   [ToolboxBitmap(typeof(Resources), "Dock16x16.bmp")]
   public class DockContainer : UserControl
   {
      #region Fields

      private FormsDocker           _docker              = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public DockContainer()
      {
         _docker        = new FormsDocker(this);
         BackColor      = Color.FromArgb(118, 118, 118);
         SwitchSelector = new DefaultFormsSelector();

         _docker.ShowContextMenu += OnDockerShowContextMenu;
         _docker.FormClosing     += OnDockerFormClosing;
         _docker.FormClosed      += OnDockerFormClosed;
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
      /// First color of dockable forms title bar gradient
      /// </summary>
      [Category("Appearance")]
      [Description("First color of dockable forms title bar gradient")]
      public Color TitleBarGradientColor1
      {
         get { return _docker.TitleBarGradientColor1; }
         set { _docker.TitleBarGradientColor1 = value; }
      }

      /// <summary>
      /// Second color of dockable forms title bar gradient
      /// </summary>
      [Category("Appearance")]
      [Description("Second color of dockable forms title bar gradient")]
      public Color TitleBarGradientColor2
      {
         get { return _docker.TitleBarGradientColor2; }
         set { _docker.TitleBarGradientColor2 = value; }
      }

      /// <summary>
      /// First color of dockable forms title bar gradient when the form is selected
      /// </summary>
      [Category("Appearance")]
      [Description("First color of dockable forms title bar gradient when the form is selected")]
      public Color TitleBarGradientSelectedColor1
      {
         get { return _docker.TitleBarGradientSelectedColor1; }
         set { _docker.TitleBarGradientSelectedColor1 = value; }
      }

      /// <summary>
      /// Second color of dockable forms title bar gradient when the form is selected
      /// </summary>
      [Category("Appearance")]
      [Description("Second color of dockable forms title bar gradient when the form is selected")]
      public Color TitleBarGradientSelectedColor2
      {
         get { return _docker.TitleBarGradientSelectedColor2; }
         set { _docker.TitleBarGradientSelectedColor2 = value; }
      }

      /// <summary>
      /// Color of dockable forms title bar text
      /// </summary>
      [Category("Appearance")]
      [Description("Color of dockable forms title bar text")]
      public Color TitleBarTextColor
      {
         get { return _docker.TitleBarTextColor; }
         set { _docker.TitleBarTextColor = value; }
      }


      /// <summary>
      /// Flag indicating if the filled forms can be moved by mouse
      /// </summary>
      [Category("Behavior")]
      [Description("Flag indicating if the filled forms can be moved by mouse")]
      public bool CanMoveByMouseFilledForms
      {
         get { return _docker.CanMoveByMouseFilledForms; }
         set { _docker.CanMoveByMouseFilledForms = value; }
      }

      /// <summary>
      /// Accessor of the switch selector
      /// </summary>
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      public FormsSelector SwitchSelector
      {
         get { return _docker.SwitchSelector; }
         set { _docker.SwitchSelector = value; }
      }

      /// <summary>
      /// Accessor of the preview renderer
      /// </summary>
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      public PreviewRenderer PreviewRenderer
      {
         get { return _docker.PreviewRenderer; }
         set { _docker.PreviewRenderer = value; }
      }

      /// <summary>
      /// Accessor of the dockable forms count
      /// </summary>
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      public int Count
      {
         get { return _docker.Count; }
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
         return _docker.Add(form, allowedDock, formIdentifier);
      }

      /// <summary>
      /// Gets the info of the form at given index
      /// </summary>
      /// <param name="index">zero based index of the form</param>
      /// <returns>info of specified form</returns>
      public DockableFormInfo GetFormInfoAt(int index)
      {
         return _docker.GetFormInfoAt(index);
      }

      /// <summary>
      /// Get form info
      /// </summary>
      /// <param name="form">form</param>
      /// <returns>info</returns>
      public DockableFormInfo GetFormInfo(Form form)
      {
         return _docker.GetFormInfo(form);
      }

      /// <summary>
      /// Get form info based on identifier
      /// </summary>
      /// <param name="identifier">identifier</param>
      /// <returns>form info</returns>
      public DockableFormInfo GetFormInfo(Guid identifier)
      {
         return _docker.GetFormInfo(identifier);
      }

      /// <summary>
      /// Dock a form previously added to guider
      /// </summary>
      /// <param name="info">info about the form to dock</param>
      /// <param name="dock">how to dock</param>
      /// <param name="mode">were to dock</param>
      public void DockForm(DockableFormInfo info, DockStyle dock, zDockMode mode)
      {
         _docker.Dock(info, dock, mode);
      }

      /// <summary>
      /// Dock a form previously added to guider over another form previously added to guider
      /// </summary>
      /// <param name="info">info about the form to dock</param>
      /// <param name="infoOver">info about form over which to dock</param>
      /// <param name="dock">how to dock</param>
      /// <param name="mode">were to dock</param>
      public void DockForm(DockableFormInfo info, DockableFormInfo infoOver, DockStyle dock, zDockMode mode)
      {
         _docker.DockOver(info, infoOver, dock, mode);
      }

      /// <summary>
      /// Undock form
      /// </summary>
      /// <param name="info">info about form to undock</param>
      /// <param name="hintBounds">hint bounds</param>
      public void Undock(DockableFormInfo info, Rectangle hintBounds)
      {
         _docker.Undock(info, hintBounds);
      }

      /// <summary>
      /// Remove the form
      /// </summary>
      /// <param name="info">info about form to remove</param>
      public void Remove(DockableFormInfo info)
      {
         _docker.Remove(info);
      }

      /// <summary>
      /// Clear
      /// </summary>
      public void Clear()
      {
         _docker.Clear();
      }

      /// <summary>
      /// Set auto-hide mode
      /// </summary>
      /// <param name="info">info of the form to set</param>
      /// <param name="autoHide">flag indicating if should set auto-hide (true) or unset it</param>
      public void SetAutoHide(DockableFormInfo info, bool autoHide)
      {
         _docker.SetAutoHide(info, autoHide);
      }

      /// <summary>
      /// Set the width of a form docked: left or right or fill
      /// </summary>
      /// <param name="info">info identifing the form</param>
      /// <param name="newWidth">new width value</param>
      public void SetWidth(DockableFormInfo info, int newWidth)
      {
         _docker.SetWidth(info, newWidth);
      }

      /// <summary>
      /// Set the height of a form docked: top or bottom or fill
      /// </summary>
      /// <param name="info">info identifing the form</param>
      /// <param name="newHeight">new height value</param>
      public void SetHeight(DockableFormInfo info, int newHeight)
      {
         _docker.SetHeight(info, newHeight);
      }


      #endregion Public section

      #region Protected section

      /// <summary>
      /// Occurs when key down
      /// </summary>
      /// <param name="keyData">key data</param>
      protected override bool ProcessDialogKey(Keys keyData)
      {
         if (keyData == (Keys.Tab | Keys.Control))
         {
            _docker.ShowSwitchPreview();
            return true;
         }

         return base.ProcessDialogKey(keyData);
      }

      #endregion Protected section

      #region Private section
      #region Received events

      /// <summary>
      /// Event received after a form was closed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnDockerFormClosed(object sender, FormEventArgs e)
      {
         EventHandler<FormEventArgs> handler = FormClosed;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      /// <summary>
      /// Event received when a form is about to be closed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnDockerFormClosing(object sender, DockableFormClosingEventArgs e)
      {
         EventHandler<DockableFormClosingEventArgs> handler = FormClosing;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      /// <summary>
      /// Event received when context menu of the form is open
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event args</param>
      private void OnDockerShowContextMenu(object sender, FormContextMenuEventArgs e)
      {
         EventHandler<FormContextMenuEventArgs> handler = ShowContextMenu;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      #endregion Received events

      #endregion Private section
   }
}
