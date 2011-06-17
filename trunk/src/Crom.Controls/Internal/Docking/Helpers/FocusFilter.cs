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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
   /// <summary>
   /// Filter for focus
   /// </summary>
   internal class FocusFilter : IMessageFilter
   {
      #region Fields

      private const int WM_SETFOCUS                   = 0x0007;
      private const int WM_LBUTTONDOWN                = 0x0201;
      private const int WM_LBUTTONUP                  = 0x0202;
      private const int WM_LBUTTONDBLCLK              = 0x0203;
      private const int WM_RBUTTONDOWN                = 0x0204;
      private const int WM_RBUTTONUP                  = 0x0205;
      private const int WM_RBUTTONDBLCLK              = 0x0206;
      private const int WM_MBUTTONDOWN                = 0x0207;
      private const int WM_MBUTTONUP                  = 0x0208;
      private const int WM_MBUTTONDBLCLK              = 0x0209;
      private const int WM_KEYDOWN                    = 0x0100;

      private const int WM_NCLBUTTONDOWN              = 0x00A1;
      private const int WM_NCLBUTTONUP                = 0x00A2;
      private const int WM_NCLBUTTONDBLCLK            = 0x00A3;
      private const int WM_NCRBUTTONDOWN              = 0x00A4;
      private const int WM_NCRBUTTONUP                = 0x00A5;
      private const int WM_NCRBUTTONDBLCLK            = 0x00A6;
      private const int WM_NCMBUTTONDOWN              = 0x00A7;
      private const int WM_NCMBUTTONUP                = 0x00A8;
      private const int WM_NCMBUTTONDBLCLK            = 0x00A9;

      private IntPtr    _lastFocusedControl           = IntPtr.Zero;

      #endregion Fields

      #region Instance

      /// <summary>
      /// Default constructor
      /// </summary>
      public FocusFilter()
      {
      }

      #endregion Instance

      #region Public section

      /// <summary>
      /// Occurs when a message was filtered
      /// </summary>
      public event EventHandler<TemplateEventArgs<Message>> MessageFiltered;

      /// <summary>
      /// Occurs when a control got focus
      /// </summary>
      public event EventHandler<TemplateEventArgs<IntPtr>> ControlGotFocus;

      #region IMessageFilter

      /// <summary>
      /// Filter message
      /// </summary>
      /// <param name="m">message to filter</param>
      /// <returns>true if message was filtered</returns>
      public bool PreFilterMessage(ref Message m)
      {
         switch (m.Msg)
         {
            case WM_SETFOCUS:
            case WM_LBUTTONDOWN:
            case WM_LBUTTONUP:
            case WM_LBUTTONDBLCLK:
            case WM_RBUTTONDOWN:
            case WM_RBUTTONUP:
            case WM_RBUTTONDBLCLK:
            case WM_MBUTTONDOWN:
            case WM_MBUTTONUP:
            case WM_MBUTTONDBLCLK:
            case WM_NCLBUTTONDOWN:
            case WM_NCLBUTTONUP:
            case WM_NCLBUTTONDBLCLK:
            case WM_NCRBUTTONDOWN:
            case WM_NCRBUTTONUP:
            case WM_NCRBUTTONDBLCLK:
            case WM_NCMBUTTONDOWN:
            case WM_NCMBUTTONUP:
            case WM_NCMBUTTONDBLCLK:
            case WM_KEYDOWN:

               EventHandler<TemplateEventArgs<Message>> handler = MessageFiltered;
               if (handler != null)
               {
                  TemplateEventArgs<Message> args = new TemplateEventArgs<Message>(m);
                  handler(this, args);
               }

               LastFocusedControl = m.HWnd;
               break;

            default:
               break;
         }

         return false;
      }

      #endregion IMessageFilter

      #endregion Public section

      #region Private section

      [DllImport("User32")]
      private static extern IntPtr GetForegroundWindow();

      /// <summary>
      /// Occurs when a control got focus
      /// </summary>
      /// <param name="hWnd">hwnd</param>
      /// <returns>hwnd of the control that got focus</returns>
      private void OnControlGotFocus(IntPtr hWnd)
      {
         EventHandler<TemplateEventArgs<IntPtr>> handle = ControlGotFocus;
         if (handle != null)
         {
            TemplateEventArgs<IntPtr> arg = new TemplateEventArgs<IntPtr>(hWnd);
            handle(this, arg);
         }
      }


      /// <summary>
      /// Update the focused control
      /// </summary>
      private void UpdateFocusedControl()
      {
         LastFocusedControl = GetForegroundWindow();
      }

      /// <summary>
      /// Accessor of the last focused control
      /// </summary>
      private IntPtr LastFocusedControl
      {
         get { return _lastFocusedControl; }
         set
         {
            if (_lastFocusedControl != value)
            {
               _lastFocusedControl = value;

               OnControlGotFocus(value);
            }
         }
      }

      #endregion Private section
   }
}
