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

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Switch preview renderer
   /// </summary>
   public abstract class FormsSelector
   {
      #region Fields

      private PreviewRenderer    _renderer            = null;
      private DockableFormInfo[] _forms               = null;
      private int                _selectedIndex       = -1;
      private Rectangle          _screenBounds        = new Rectangle();

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      protected FormsSelector()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Show forms selector
      /// </summary>
      /// <param name="containerScreenBounds">screen bounds of the container</param>
      /// <param name="forms">forms to select</param>
      public void Show(PreviewRenderer renderer, Rectangle containerScreenBounds, DockableFormInfo[] forms)
      {
         Renderer   = renderer;
         _forms     = forms;
         if (_forms == null)
         {
            _forms = new DockableFormInfo[0];
         }

         for (int index = 0; index < _forms.Length; index++)
         {
            if (_forms[index].IsSelected)
            {
               SelectedIndex = index;
               break;
            }
         }

         _screenBounds = containerScreenBounds;

         ShowSelector();
      }

      /// <summary>
      /// Accessor of the selected index
      /// </summary>
      public int SelectedIndex
      {
         get { return _selectedIndex; }
         set
         {
            if (_selectedIndex != value)
            {
               if (_forms.Length == 0)
               {
                  _selectedIndex = -1;
               }
               else
               {
                  _selectedIndex = value;

                  if (_selectedIndex < 0)
                  {
                     _selectedIndex = _forms.Length - 1;
                  }

                  if (_selectedIndex > _forms.Length - 1)
                  {
                     _selectedIndex = 0;
                  }
               }

               if (_selectedIndex >= 0 && _selectedIndex < _forms.Length)
               {
                  _renderer.SelectedForm = _forms[_selectedIndex].DockableForm;
               }
            }
         }
      }

      /// <summary>
      /// Accessor of the forms
      /// </summary>
      public DockableFormInfo[] Forms
      {
         get { return _forms; }
      }

      /// <summary>
      /// Accessor of the container screen bounds
      /// </summary>
      public Rectangle ContainerScreenBounds
      {
         get { return _screenBounds; }
      }

      /// <summary>
      /// Checks if the selector has focus
      /// </summary>
      /// <returns>true if the selector has focus</returns>
      public abstract bool HasFocus
      {
         get;
      }

      /// <summary>
      /// Cancel select
      /// </summary>
      public abstract void Cancel();

      /// <summary>
      /// Apply select
      /// </summary>
      public abstract void Apply();

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Show the selector
      /// </summary>
      protected abstract void ShowSelector();

      /// <summary>
      /// Accessor of current form preview renderer
      /// </summary>
      protected virtual PreviewRenderer Renderer
      {
         get { return _renderer; }
         set { _renderer = value; }
      }

      #endregion Protected section
   }
}
