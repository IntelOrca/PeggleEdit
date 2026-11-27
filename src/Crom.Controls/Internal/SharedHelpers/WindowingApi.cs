using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Crom.Controls
{
   /// <summary>
   /// Window operations that need the native windowing system.
   /// </summary>
   internal interface IWindowingApi
   {
      /// <summary>
      /// Gets the size of the non-client area (borders and caption) on each side of the control.
      /// </summary>
      Margins GetNonClientMargins(Control control);

      /// <summary>
      /// Gets whether the form is shown on screen.
      /// </summary>
      bool IsWindowVisible(Form form);

      /// <summary>
      /// Shows the form above all other windows without activating it. The form must override
      /// <see cref="Form.ShowWithoutActivation"/> to return true, as not all platforms can show a
      /// window without activating it otherwise.
      /// </summary>
      void ShowTopMostWithoutActivating(Form form);

      /// <summary>
      /// Draws now the parts of the control and its children that need drawing, e.g. those a window moved over them
      /// has uncovered, rather than when no input is waiting (which, while the mouse moves, can be a long time).
      /// </summary>
      void DrawNow(Control control);
   }

   /// <summary>
   /// Provides the <see cref="IWindowingApi"/> for the current platform.
   /// </summary>
   internal static class WindowingApi
   {
      /// <summary>
      /// Gets the implementation for the current platform: Win32 on Windows and under Wine,
      /// otherwise one using only Windows Forms (e.g. Mono on Linux or macOS).
      /// </summary>
      public static IWindowingApi Current { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT
         ? (IWindowingApi)new Win32WindowingApi()
         : new ManagedWindowingApi();
   }

   /// <summary>
   /// Implements <see cref="IWindowingApi"/> using the Win32 API.
   /// </summary>
   internal sealed class Win32WindowingApi : IWindowingApi
   {
      private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

      private const int SWP_NOACTIVATE = 0x0010;
      private const int SWP_SHOWWINDOW = 0x0040;

      public Margins GetNonClientMargins(Control control)
      {
         WindowInfo info = new WindowInfo();
         info.cbSize = Marshal.SizeOf(info);

         if (GetWindowInfo(control.Handle, ref info) == false)
         {
            throw new InvalidOperationException();
         }

         return new Margins(
            info.rcClient.left    - info.rcWindow.left,
            info.rcWindow.right   - info.rcClient.right,
            info.rcClient.top     - info.rcWindow.top,
            info.rcWindow.bottom  - info.rcClient.bottom);
      }

      public bool IsWindowVisible(Form form)
      {
         return IsWindowVisible(form.Handle);
      }

      public void ShowTopMostWithoutActivating(Form form)
      {
         SetWindowPos(form.Handle, HWND_TOPMOST, form.Left, form.Top, form.Width, form.Height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
      }

      public void DrawNow(Control control)
      {
         if (control.IsHandleCreated)
         {
            RedrawWindow(control.Handle, IntPtr.Zero, IntPtr.Zero, RDW_UPDATENOW | RDW_ALLCHILDREN);
         }
      }

      private const int RDW_ALLCHILDREN = 0x0080;
      private const int RDW_UPDATENOW   = 0x0100;

      [DllImport("user32")]
      private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

      [StructLayout(LayoutKind.Sequential)]
      private struct Rect
      {
         public int left;
         public int top;
         public int right;
         public int bottom;
      }

      [StructLayout(LayoutKind.Sequential)]
      private struct WindowInfo
      {
         public int cbSize;
         public Rect rcWindow;
         public Rect rcClient;
         public int dwStyle;
         public int dwExStyle;
         public int dwWindowStatus;
         public int cxWindowBorders;
         public int cyWindowBorders;
         public short atomWindowType;
         public short wCreatorVersion;
      }

      [DllImport("user32")]
      private static extern bool GetWindowInfo(IntPtr hWnd, ref WindowInfo info);

      [DllImport("user32")]
      private static extern bool IsWindowVisible(IntPtr hWnd);

      [DllImport("user32")]
      private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, int uFlags);
   }

   /// <summary>
   /// Implements <see cref="IWindowingApi"/> using only Windows Forms, for platforms without the Win32 API.
   /// </summary>
   internal sealed class ManagedWindowingApi : IWindowingApi
   {
      public Margins GetNonClientMargins(Control control)
      {
         Rectangle window = control.Parent == null ? control.Bounds : control.Parent.RectangleToScreen(control.Bounds);
         Rectangle client = control.RectangleToScreen(control.ClientRectangle);

         return new Margins(
            client.Left    - window.Left,
            window.Right   - client.Right,
            client.Top     - window.Top,
            window.Bottom  - client.Bottom);
      }

      public bool IsWindowVisible(Form form)
      {
         return form.Visible;
      }

      public void ShowTopMostWithoutActivating(Form form)
      {
         form.TopMost = true;
         form.Show();
      }

      public void DrawNow(Control control)
      {
         control.Update();
         foreach (Control child in control.Controls)
         {
            DrawNow(child);
         }
      }
   }
}
