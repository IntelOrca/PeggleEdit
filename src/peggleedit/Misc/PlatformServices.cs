// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
    /// <summary>
    /// Operating system services used by the editor.
    /// </summary>
    interface IPlatformServices
    {
        /// <summary>
        /// Adds a file to the operating system's list of recently used documents.
        /// </summary>
        void AddRecentDocument(string path);

        /// <summary>
        /// Shortens a path to fit within the given number of characters by replacing directories with an ellipsis.
        /// </summary>
        string CompactPath(string path, int length);

        /// <summary>
        /// Gets whether the given window accepts input, i.e. it is not disabled by a modal dialog.
        /// </summary>
        bool AcceptsInput(Form form);

        /// <summary>
        /// From now on, centres the system's dialogs (message boxes, folder browsers) shown on this thread over the
        /// window they belong to, as the application's own dialogs are, rather than over the screen.
        /// </summary>
        void CentreSystemDialogs();

        /// <summary>
        /// Stops a control and its children being drawn until the returned object is disposed, when all of it is
        /// drawn again at once, so that a change made in steps (e.g. resizing, moving and scrolling) is never shown
        /// part way through.
        /// </summary>
        IDisposable SuspendDrawing(Control control);

        /// <summary>
        /// Gets whether the left mouse button (the main one, if they are swapped) is held down now, including when it
        /// was let go over another program, which <see cref="Control.MouseButtons"/> does not see.
        /// </summary>
        bool IsLeftMouseButtonDown();
    }

    static class PlatformServices
    {
        /// <summary>
        /// Gets the services for the current platform: Win32 on Windows and under Wine, otherwise
        /// a portable implementation (e.g. Mono on Linux or macOS).
        /// </summary>
        public static IPlatformServices Current { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? (IPlatformServices)new Win32PlatformServices()
            : new PortablePlatformServices();
    }

    /// <summary>
    /// Implements <see cref="IPlatformServices"/> using the Windows API.
    /// </summary>
    sealed class Win32PlatformServices : IPlatformServices
    {
        private const uint SHARD_PATHW = 0x00000003;

        [DllImport("shell32.dll")]
        private static extern void SHAddToRecentDocs(uint uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        public void AddRecentDocument(string path)
        {
            SHAddToRecentDocs(SHARD_PATHW, path);
        }

        public string CompactPath(string path, int length)
        {
            var sb = new StringBuilder(capacity: length);
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }

        public bool AcceptsInput(Form form)
        {
            return IsWindowEnabled(form.Handle);
        }

        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public bool IsLeftMouseButtonDown()
        {
            // The physical button, so the right one when they are swapped
            var key = SystemInformation.MouseButtonsSwapped ? VK_RBUTTON : VK_LBUTTON;
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }

        #region Suspending drawing

        private const int WM_SETREDRAW = 0x000B;
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_ERASE = 0x0004;
        private const uint RDW_ALLCHILDREN = 0x0080;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_FRAME = 0x0400;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        public IDisposable SuspendDrawing(Control control)
        {
            // Only a control that is shown, so that one hidden is not shown when drawing is turned back on (and one
            // whose drawing is already suspended, e.g. by its parent's, is left to that)
            if (!control.IsHandleCreated || !IsWindowVisible(control.Handle))
                return new DrawingSuspension(null);
            SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            return new DrawingSuspension(control);
        }

        private sealed class DrawingSuspension : IDisposable
        {
            private Control mControl;

            public DrawingSuspension(Control control)
            {
                mControl = control;
            }

            public void Dispose()
            {
                var control = mControl;
                mControl = null;
                if (control == null || !control.IsHandleCreated)
                    return;
                SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                RedrawWindow(control.Handle, IntPtr.Zero, IntPtr.Zero,
                    RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
            }
        }

        #endregion

        #region Centring system dialogs

        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;
        private const int HCBT_DESTROYWND = 4;
        private const uint GW_OWNER = 4;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        // Kept, so that the delegate the hook calls is not collected
        private HookProc _cbtHook;
        private IntPtr _cbtHookHandle;
        // The dialogs centred, each only when it is first shown, so that one the user has moved stays where it is
        private readonly HashSet<IntPtr> _centredDialogs = new HashSet<IntPtr>();

        public void CentreSystemDialogs()
        {
            if (_cbtHookHandle != IntPtr.Zero)
                return;

            _cbtHook = OnCbt;
            _cbtHookHandle = SetWindowsHookEx(WH_CBT, _cbtHook, IntPtr.Zero, GetCurrentThreadId());
        }

        private IntPtr OnCbt(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (code == HCBT_ACTIVATE)
                {
                    if (!_centredDialogs.Contains(wParam) && IsSystemDialog(wParam))
                    {
                        _centredDialogs.Add(wParam);
                        CentreOnOwner(wParam);
                    }
                }
                else if (code == HCBT_DESTROYWND)
                {
                    _centredDialogs.Remove(wParam);
                }
            }
            catch
            {
                // A dialog left where Windows put it is better than an exception thrown into Windows
            }
            return CallNextHookEx(_cbtHookHandle, code, wParam, lParam);
        }

        private static bool IsSystemDialog(IntPtr hWnd)
        {
            var className = new StringBuilder(16);
            GetClassName(hWnd, className, className.Capacity);
            return className.ToString() == "#32770";
        }

        private static void CentreOnOwner(IntPtr hWnd)
        {
            var owner = GetWindow(hWnd, GW_OWNER);
            if (owner == IntPtr.Zero || !IsWindowVisible(owner) || IsIconic(owner))
                return;
            if (!GetWindowRect(hWnd, out var dialog) || !GetWindowRect(owner, out var ownerRect))
                return;

            var width = dialog.Right - dialog.Left;
            var height = dialog.Bottom - dialog.Top;
            var x = ownerRect.Left + ((ownerRect.Right - ownerRect.Left - width) / 2);
            var y = ownerRect.Top + ((ownerRect.Bottom - ownerRect.Top - height) / 2);

            // Kept on the owner's screen
            var area = Screen.FromHandle(owner).WorkingArea;
            x = Math.Max(area.Left, Math.Min(x, area.Right - width));
            y = Math.Max(area.Top, Math.Min(y, area.Bottom - height));
            SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        }

        #endregion
    }

    /// <summary>
    /// Implements <see cref="IPlatformServices"/> without any native APIs.
    /// </summary>
    sealed class PortablePlatformServices : IPlatformServices
    {
        public void AddRecentDocument(string path)
        {
        }

        public void CentreSystemDialogs()
        {
            // Mono's message boxes are its own forms
        }

        public bool IsLeftMouseButtonDown()
        {
            return (Control.MouseButtons & MouseButtons.Left) != 0;
        }

        public IDisposable SuspendDrawing(Control control)
        {
            // Drawn again afterwards, though not kept from being drawn in between
            return new Redraw(control);
        }

        private sealed class Redraw : IDisposable
        {
            private Control mControl;

            public Redraw(Control control)
            {
                mControl = control;
            }

            public void Dispose()
            {
                mControl?.Invalidate(true);
                mControl = null;
            }
        }

        public string CompactPath(string path, int length)
        {
            // Like PathCompactPathEx, length includes the null terminator
            var maxChars = length - 1;
            if (path.Length <= maxChars)
                return path;

            // Too short for an ellipsis, keep the end of the path
            const string ellipsis = "...";
            if (maxChars <= ellipsis.Length)
                return path.Substring(path.Length - Math.Max(0, maxChars));

            // Keep as much of the directory as fits: <directory>...\<file name>
            var fileName = Path.GetFileName(path);
            var directoryLength = maxChars - ellipsis.Length - 1 - fileName.Length;
            if (directoryLength > 0)
                return path.Substring(0, directoryLength) + ellipsis + Path.DirectorySeparatorChar + fileName;

            // Otherwise keep the end of the file name: ...<file name>
            return ellipsis + fileName.Substring(fileName.Length - Math.Min(fileName.Length, maxChars - ellipsis.Length));
        }

        public bool AcceptsInput(Form form)
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm != form && openForm.Modal)
                    return false;
            }
            return form.Enabled;
        }
    }
}
