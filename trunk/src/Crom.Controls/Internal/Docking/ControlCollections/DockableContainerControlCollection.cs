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
   /// Dockable container control collection
   /// </summary>
   internal class DockableContainerControlCollection : Control.ControlCollection
   {
      #region Fields

      private Splitter        _splitter               = null;
      private FormsTabbedView _singleChild            = null;
      private Control         _leftPane               = null;
      private Control         _rightPane              = null;
      private Control         _topPane                = null;
      private Control         _bottomPane             = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="owner">owner control</param>
      public DockableContainerControlCollection(Control owner) : base(owner)
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when single child is changed
      /// </summary>
      public event EventHandler<ControlEventArgs> SingleChildChanged;

      /// <summary>
      /// Add a control to the collection
      /// </summary>
      /// <param name="value">new control added</param>
      public override void Add(Control value)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Add a range of controls
      /// </summary>
      /// <param name="controls">range of controls</param>
      public override void AddRange(Control[] controls)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Clears the controls collection
      /// </summary>
      public override void Clear()
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Remove a control
      /// </summary>
      /// <param name="value">control to be removed</param>
      public override void Remove(Control value)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Remove by key
      /// </summary>
      /// <param name="key">key</param>
      public override void RemoveByKey(string key)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }

      /// <summary>
      /// Set child index
      /// </summary>
      /// <param name="child">child control</param>
      /// <param name="newIndex">zero based new child index</param>
      public override void SetChildIndex(Control child, int newIndex)
      {
         // Disconnect from the base
         throw new NotSupportedException();
      }


      /// <summary>
      /// Accessor of the single child
      /// </summary>
      public FormsTabbedView SingleChild
      {
         get { return _singleChild; }
         private set 
         {
            if (_singleChild != value)
            {
               ControlEventArgs args = new ControlEventArgs(_singleChild);

               _singleChild = value;

               if (SingleChildChanged != null)
               {
                  SingleChildChanged(this, args);
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the left pane
      /// </summary>
      public Control LeftPane
      {
         get { return _leftPane; }
         private set
         {
            if (_leftPane != value)
            {
               if (_leftPane != null)
               {
                  _leftPane.VisibleChanged -= OnLeftPaneVisibleChanged;
               }

               _leftPane = value;

               if (_leftPane != null)
               {
                  _leftPane.VisibleChanged += OnLeftPaneVisibleChanged;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the right pane
      /// </summary>
      public Control RightPane
      {
         get { return _rightPane; }
         private set
         {
            if (_rightPane != value)
            {
               if (_rightPane != null)
               {
                  _rightPane.VisibleChanged -= OnRightPaneVisibleChanged;
               }

               _rightPane = value;

               if (_rightPane != null)
               {
                  _rightPane.VisibleChanged += OnRightPaneVisibleChanged;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the top pane
      /// </summary>
      public Control TopPane
      {
         get { return _topPane; }
         private set
         {
            if (_topPane != value)
            {
               if (_topPane != null)
               {
                  _topPane.VisibleChanged -= OnTopPaneVisibleChanged;
               }

               _topPane = value;

               if (_topPane != null)
               {
                  _topPane.VisibleChanged += OnTopPaneVisibleChanged;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the bottom pane
      /// </summary>
      public Control BottomPane
      {
         get { return _bottomPane; }
         private set
         {
            if (_bottomPane != value)
            {
               if (_bottomPane != null)
               {
                  _bottomPane.VisibleChanged -= OnBottomPaneVisibleChanged;
               }

               _bottomPane = value;

               if (_bottomPane != null)
               {
                  _bottomPane.VisibleChanged += OnBottomPaneVisibleChanged;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the splitter
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
      /// Set mode empty
      /// </summary>
      public void SetModeEmpty()
      {
         for (int index = Count - 1; index >= 0; index--)
         {
            base.Remove(this[index]);
         }

         Reset();
      }

      /// <summary>
      /// Set mode single child control
      /// </summary>
      /// <param name="singleChild">single child</param>
      public void SetModeSingleChild(FormsTabbedView singleChild)
      {
         singleChild.Parent = null;

         singleChild.Dock   = DockStyle.Fill;

         Reset();

         SetModeEmpty();
         base.Add(singleChild);

         SingleChild = singleChild;
      }

      /// <summary>
      /// Set mode horizontal split
      /// </summary>
      /// <param name="leftPane">left pane</param>
      /// <param name="rightPane">right pane</param>
      public void SetModeHSplit(Control leftPane, Control rightPane)
      {
         leftPane.Parent  = null;
         rightPane.Parent = null;

         leftPane.Dock    = DockStyle.Left;
         rightPane.Dock   = DockStyle.Fill;

         SetModeEmpty();

         Splitter.Dock    = DockStyle.Left;
         base.Add(Splitter);

         leftPane.Width   = Owner.Width / 2 - Splitter.Width;
         rightPane.Width  = Owner.Width / 2 - Splitter.Width;

         base.Add(rightPane);
         base.SetChildIndex(rightPane, 0);

         base.Add(leftPane);

         Reset();

         LeftPane  = leftPane;
         RightPane = rightPane;

         UpdateHorizontalLayout();
      }

      /// <summary>
      /// Set mode vertical split
      /// </summary>
      /// <param name="topPane">left pane</param>
      /// <param name="bottomPane">right pane</param>
      public void SetModeVSplit(Control topPane, Control bottomPane)
      {
         topPane.Parent    = null;
         bottomPane.Parent = null;

         topPane.Dock      = DockStyle.Top;
         bottomPane.Dock   = DockStyle.Fill;

         SetModeEmpty();

         Splitter.Dock     = DockStyle.Top;
         base.Add(Splitter);

         topPane.Height    = Owner.Height / 2 - Splitter.Height;
         bottomPane.Height = Owner.Height / 2 - Splitter.Height;

         base.Add(bottomPane);
         base.SetChildIndex(bottomPane, 0);

         base.Add(topPane);

         Reset();

         TopPane    = topPane;
         BottomPane = bottomPane;

         UpdateVerticalLayout();
      }

      #endregion Public section

      #region Private section

      #region Received events

      /// <summary>
      /// Occurs when left pane visibility was changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnLeftPaneVisibleChanged(object sender, EventArgs e)
      {
         UpdateHorizontalLayout();
      }

      /// <summary>
      /// Occurs when right pane visibility was changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnRightPaneVisibleChanged(object sender, EventArgs e)
      {
         UpdateHorizontalLayout();
      }

      /// <summary>
      /// Occurs when top pane visibility was changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnTopPaneVisibleChanged(object sender, EventArgs e)
      {
         UpdateVerticalLayout();
      }

      /// <summary>
      /// Occurs when bottom pane visibility was changed
      /// </summary>
      /// <param name="sender">sender of the event</param>
      /// <param name="e">event argument</param>
      private void OnBottomPaneVisibleChanged(object sender, EventArgs e)
      {
         UpdateVerticalLayout();
      }

      #endregion Received events

      /// <summary>
      /// Reset
      /// </summary>
      private void Reset()
      {
         SingleChild = null;
         LeftPane    = null;
         RightPane   = null;
         TopPane     = null;
         BottomPane  = null;
      }

      /// <summary>
      /// Update horizontal layout
      /// </summary>
      private void UpdateHorizontalLayout()
      {
         if (LeftPane.Visible && RightPane.Visible)
         {
            LeftPane.Dock     = DockStyle.Left;
            Splitter.Visible  = true;
            RightPane.Dock    = DockStyle.Fill;
         }
         else if (LeftPane.Visible)
         {
            LeftPane.Dock     = DockStyle.Fill;
            Splitter.Visible  = false;
         }
         else if (RightPane.Visible)
         {
            RightPane.Dock    = DockStyle.Fill;
            Splitter.Visible  = false;
         }
      }

      /// <summary>
      /// Update vertical layout
      /// </summary>
      private void UpdateVerticalLayout()
      {
         if (TopPane.Visible && BottomPane.Visible)
         {
            TopPane.Dock      = DockStyle.Top;
            Splitter.Visible  = true;
            BottomPane.Dock   = DockStyle.Fill;
         }
         else if (TopPane.Visible)
         {
            TopPane.Dock      = DockStyle.Fill;
            Splitter.Visible  = false;
         }
         else if (BottomPane.Visible)
         {
            BottomPane.Dock   = DockStyle.Fill;
            Splitter.Visible  = false;
         }
      }

      #endregion Private section
   }
}
