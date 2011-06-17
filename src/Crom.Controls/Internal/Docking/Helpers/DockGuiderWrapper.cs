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
   /// Dock guider wrapper
   /// </summary>
   internal class DockGuiderWrapper : Disposable
   {
      #region Fields

      private GuidedDockResult         _dockResult                      = new GuidedDockResult();

      private DockPreview              _previewGuider                   = new DockPreview();
      private CenterDockButtons        _centerGuider                    = null;
      private MarginDockButtons        _marginGuiders                   = null;
      private FormWrapper              _host                            = null;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="host">host</param>
      public DockGuiderWrapper(FormWrapper host)
      {
         _host = host;

         _marginGuiders = new MarginDockButtons(host);
         _centerGuider  = new CenterDockButtons(host);
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Initialize dock guider when parent form becomes visible, 
      /// to prevent parent form title bar flicker when showing the guiders
      /// </summary>
      public void Initialize()
      {
         ValidateNotDisposed();

         _previewGuider.Initialize();
      }

      /// <summary>
      /// Show the preview panel at given bounds
      /// </summary>
      /// <param name="screenBounds">bounds of the preview panel</param>
      public void ShowPreviewPanel(Rectangle screenBounds)
      {
         ValidateNotDisposed();

         if (_previewGuider.Size != screenBounds.Size)
         {
            _previewGuider.Initialize();
         }
         _previewGuider.Bounds = screenBounds;
         _previewGuider.ShowWithoutStealingFocus();
      }

      /// <summary>
      /// Hides the preview panel
      /// </summary>
      public void HidePreviewPanel()
      {
         ValidateNotDisposed();

         _previewGuider.Hide();
      }

      /// <summary>
      /// Shows the center guider
      /// </summary>
      /// <param name="allowedDockMode">allowed dock</param>
      /// <param name="screenBounds">screen bounds where to center the guider</param>
      public void ShowCenterGuider(zAllowedDock allowedDockMode, Rectangle screenBounds)
      {
         ValidateNotDisposed();

         _centerGuider.Show(allowedDockMode, screenBounds);
      }

      /// <summary>
      /// Hides the center guider
      /// </summary>
      public void HideCenterGuider()
      {
         ValidateNotDisposed();

         _centerGuider.Hide();
      }

      /// <summary>
      /// Show margin guider
      /// </summary>
      /// <param name="allowedDockMode">allowed dock mode</param>
      /// <param name="screenBounds">screen bounds where to show the margins guider</param>
      public void ShowMarginsGuider(zAllowedDock allowedDockMode, Rectangle screenBounds)
      {
         ValidateNotDisposed();

         _marginGuiders.Show(allowedDockMode);
      }

      /// <summary>
      /// Hide the margins guider
      /// </summary>
      public void HideMarginsGuider()
      {
         ValidateNotDisposed();

         _marginGuiders.Hide();
      }

      /// <summary>
      /// Gets the dock result for given screen location
      /// </summary>
      /// <param name="allowedDockMode">allowed dock mode</param>
      /// <param name="screenLocation">screen location</param>
      /// <returns>dock result</returns>
      public GuidedDockResult GetDockResult(zAllowedDock allowedDockMode, Point screenLocation)
      {
         ValidateNotDisposed();

         _dockResult.Dock     = DockStyle.None;
         _dockResult.DockMode = zDockMode.Outer;

         Point clientLocation = _host.PointToClient(screenLocation);

         if (_marginGuiders.LeftButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Left) )
         {
            _dockResult.Dock = DockStyle.Left;
         }
         else if (_marginGuiders.RightButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Right))
         {
            _dockResult.Dock = DockStyle.Right;
         }
         else if (_marginGuiders.TopButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Top))
         {
            _dockResult.Dock = DockStyle.Top;
         }
         else if (_marginGuiders.BottomButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Bottom))
         {
            _dockResult.Dock = DockStyle.Bottom;
         }
         else if (_centerGuider.IsVisible)
         {
            _dockResult.DockMode = zDockMode.Inner;

            if (_centerGuider.LeftButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Left))
            {
               _dockResult.Dock = DockStyle.Left;
            }
            else if (_centerGuider.TopButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Top))
            {
               _dockResult.Dock = DockStyle.Top;
            }
            else if (_centerGuider.RightButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Right))
            {
               _dockResult.Dock = DockStyle.Right;
            }
            else if (_centerGuider.BottomButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Bottom))
            {
               _dockResult.Dock = DockStyle.Bottom;
            }
            else if (_centerGuider.FillButtonBounds.Contains(clientLocation) && EnumUtility.Contains(allowedDockMode, zAllowedDock.Fill))
            {
               _dockResult.Dock = DockStyle.Fill;
            }
         }

         return _dockResult;
      }

      /// <summary>
      /// Proposed size of the preview guider
      /// </summary>
      public Size ProposedSize
      {
         get { return _previewGuider.Size; }
      }

      #endregion Public section

      #region Protected section

      /// <summary>
      /// Dispose this instance
      /// </summary>
      /// <param name="fromIDisposableDispose">call from IDisposable.Dispose</param>
      protected override void Dispose(bool fromIDisposableDispose)
      {
         if (fromIDisposableDispose)
         {
            if (_previewGuider != null)
            {
               _previewGuider.Dispose();
               _previewGuider = null;
            }

            if (_centerGuider != null)
            {
               _centerGuider.Dispose();
               _centerGuider = null;
            }

            if (_marginGuiders != null)
            {
               _marginGuiders.Dispose();
               _marginGuiders = null;
            }
         }
      }

      #endregion Protected section

      #region Private section

      #endregion Private section
   }
}
