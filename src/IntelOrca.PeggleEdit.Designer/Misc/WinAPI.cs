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
using System.Runtime.InteropServices;

namespace IntelOrca.PeggleEdit.Designer
{
	/// <summary>
	/// Defines wrapper methods for using the windows api.
	/// </summary>
	static class WinAPI
	{
		[DllImport("shell32.dll")]
		static extern void SHAddToRecentDocs(UInt32 uFlags, IntPtr pv);

		[DllImport("shell32.dll")]
		static extern void SHAddToRecentDocs(UInt32 uFlags, [MarshalAs(UnmanagedType.LPWStr)] String pv);

		enum ShellAddRecentDocs
		{
			SHARD_PIDL  = 0x00000001,
			SHARD_PATHA = 0x00000002,
			SHARD_PATHW = 0x00000003,
		}

		public static void AddRecentDocument(String path)
		{
			SHAddToRecentDocs((uint)ShellAddRecentDocs.SHARD_PATHW, path);
		}
 
		public static void ClearRecentDocuments()
		{
			SHAddToRecentDocs((uint)ShellAddRecentDocs.SHARD_PIDL, IntPtr.Zero);
		}
	}
}
