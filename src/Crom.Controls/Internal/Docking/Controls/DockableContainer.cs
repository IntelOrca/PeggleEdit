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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Implementation of dockable container
   /// </summary>
   internal class DockableContainer : Control
   {
      #region Fields

      private readonly Size   _minSize                = new Size(80, 80);
      private DockStyle       _savedDock              = DockStyle.None;
      private FormsTabbedView _linkedView             = null;

      private DockableContainerControlCollection _controls = null;
      private Splitter        _splitter               = null;
      private bool            _splitterBefore         = false;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public DockableContainer()
      {
         if (Controls.GetType() != typeof(DockableContainerControlCollection))
         {
            throw new InvalidOperationException();
         }
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Accessor of the single child
      /// </summary>
      public FormsTabbedView SingleChild
      {
         get { return InternalControls.SingleChild; }
      }

      /// <summary>
      /// Accessor of the left pane
      /// </summary>
      public DockableContainer LeftPane
      {
         get { return (DockableContainer)InternalControls.LeftPane; }
      }

      /// <summary>
      /// Accessor of the right pane
      /// </summary>
      public DockableContainer RightPane
      {
         get { return (DockableContainer)InternalControls.RightPane; }
      }

      /// <summary>
      /// Accessor of the top pane
      /// </summary>
      public DockableContainer TopPane
      {
         get { return (DockableContainer)InternalControls.TopPane; }
      }

      /// <summary>
      /// Accessor of the bottom pane
      /// </summary>
      public DockableContainer BottomPane
      {
         get { return (DockableContainer)InternalControls.BottomPane; }
      }

      /// <summary>
      /// Get horizontal splitter
      /// </summary>
      public Splitter HSplitter
      {
         get { return InternalControls.Splitter; }
      }

      /// <summary>
      /// Get vertical splitter
      /// </summary>
      public Splitter VSplitter
      {
         get { return InternalControls.Splitter; }
      }

      /// <summary>
      /// Get associated splitter
      /// </summary>
      public Splitter Splitter
      {
         get 
         {
            if (_splitter == null)
            {
               _splitter = new Splitter();
               _splitter.BackColor = Color.LightGray;
            }

            return _splitter;
         }
      }

      /// <summary>
      /// Flag indicating if the splitter is before or after container
      /// </summary>
      /// <remarks>Used when restoring auto-hidden containers</remarks>
      public bool SplitterBefore
      {
         get { return _splitterBefore; }
         set { _splitterBefore = value; }
      }

      /// <summary>
      /// Accessor of saved dock
      /// </summary>
      public DockStyle SavedDock
      {
         get
         {
            return _savedDock;
         }
         set
         {
            _savedDock = value;
         }
      }

      /// <summary>
      /// Minimum size of the container
      /// </summary>
      public override Size MinimumSize
      {
         get
         {
            return _minSize;
         }
         set
         {
            base.MinimumSize = value;
         }
      }

      /// <summary>
      /// Accessor of linked view
      /// </summary>
      public FormsTabbedView LinkedView
      {
         get { return _linkedView; }
      }

      /// <summary>
      /// Set mode empty
      /// </summary>
      public void SetModeEmpty()
      {
         _linkedView = null;
         InternalControls.SetModeEmpty();
      }

      /// <summary>
      /// Set mode linked
      /// </summary>
      /// <param name="link">link</param>
      public void SetModeLinked(FormsTabbedView link)
      {
         InternalControls.SetModeEmpty();
         _linkedView = link;
      }

      /// <summary>
      /// Set mode single child control
      /// </summary>
      /// <param name="singleChild">single child</param>
      public void SetModeSingleChild(FormsTabbedView singleChild)
      {
         _linkedView = null;
         InternalControls.SetModeSingleChild(singleChild);
      }

      /// <summary>
      /// Set mode horizontal split
      /// </summary>
      /// <param name="leftPane">left pane</param>
      /// <param name="rightPane">right pane</param>
      public void SetModeHSplit(DockableContainer leftPane, DockableContainer rightPane)
      {
         _linkedView = null;
         InternalControls.SetModeHSplit(leftPane, rightPane);
      }

      /// <summary>
      /// Set mode vertical split
      /// </summary>
      /// <param name="topPane">left pane</param>
      /// <param name="bottomPane">right pane</param>
      public void SetModeVSplit(DockableContainer topPane, DockableContainer bottomPane)
      {
         _linkedView = null;
         InternalControls.SetModeVSplit(topPane, bottomPane);
      }

      /// <summary>
      /// Accessor of the other pane
      /// </summary>
      /// <param name="view">view</param>
      /// <returns>other pane or null</returns>
      public DockableContainer OtherPane(DockableContainer view)
      {
         if (view == LeftPane)
         {
            return RightPane;
         }

         if (view == RightPane)
         {
            return LeftPane;
         }

         if (view == TopPane)
         {
            return BottomPane;
         }

         if (view == BottomPane)
         {
            return TopPane;
         }

         return null;
      }

      /// <summary>
      /// Text
      /// </summary>
      /// <returns>text</returns>
      public override string ToString()
      {
         if (SingleChild != null)
         {
            return "DC: " + SingleChild.Text;
         }

         if (LeftPane != null)
         {
            return "DC[" + LeftPane.ToString() + " | " + RightPane.ToString() + "]";
         }

         if (TopPane != null)
         {
            return "DC[" + TopPane.ToString() + " | " + BottomPane.ToString() + "]";
         }

         return "DC<ModeEmpty>";
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Creates the controls instance
      /// </summary>
      /// <returns>controls instance</returns>
      protected override ControlCollection CreateControlsInstance()
      {
         return InternalControls;
      }

      #endregion Protected section

      #region Private section

      /// <summary>
      /// On single child changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnSingleChildChanged(object sender, ControlEventArgs e)
      {
         if (e.Control != null)
         {
            e.Control.RegionChanged -= OnSingleChildRegionChanged;
         }

         if (_controls.SingleChild != null)
         {
            _controls.SingleChild.RegionChanged += OnSingleChildRegionChanged;
         }

         OnSingleChildRegionChanged(this, EventArgs.Empty);
      }

      /// <summary>
      /// On single child region changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnSingleChildRegionChanged(object sender, EventArgs e)
      {
         Region = null;
         if (SingleChild != null)
         {
            if (SingleChild.IsDocked == false)
            {
               if (SingleChild.Region != null)
               {
                  try
                  {
                     Region = SingleChild.Region;
                  }
                  catch { }
               }
            }
         }
      }

      /// <summary>
      /// Internal collection
      /// </summary>
      private DockableContainerControlCollection InternalControls
      {
         get
         {
            if (_controls == null)
            {
               _controls = new DockableContainerControlCollection(this);
               _controls.SingleChildChanged += OnSingleChildChanged;
            }

            return _controls;
         }
      }

      #endregion Private section
   }
}
