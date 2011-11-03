#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.Hooks;
using MediaPortal.Util;

namespace MyAnimePlugin3
{
	public class KeyEventArgsEx : KeyEventArgs
	{
		protected char _c;

		#region Properties
		public char keyChar { get { return _c; } }
		#endregion

		#region Methods
		[DllImportAttribute("User32.dll")]
		public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpChar, int uFlags);

		[DllImportAttribute("User32.dll")]
		public static extern int GetKeyboardState(byte[] pbKeyState);

		public static char GetAsciiCharacter(int uVirtKey, int uScanCode)
		{
			byte[] lpKeyState = new byte[256];
			GetKeyboardState(lpKeyState);
			byte[] lpChar = new byte[2];
			if (ToAscii(uVirtKey, uScanCode, lpKeyState, lpChar, 0) == 1)
				return (char)lpChar[0];
			else
				return new char();
		}
		#endregion

		public KeyEventArgsEx(int virtualKey, Keys modifierKeys, int scanCode)
			: base((Keys)virtualKey | modifierKeys)
		{
			byte[] lpKeyState = new byte[256];
			GetKeyboardState(lpKeyState);
			byte[] lpChar = new byte[2];
			if (ToAscii(virtualKey, scanCode, lpKeyState, lpChar, 0) == 1)
				_c = (char)lpChar[0];
			else
				_c = new char();
		}
	}

	public delegate void KeyEventHandlerEx(object sender, KeyEventArgsEx e);

    public class KeyboardHook : Hook
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        #region Constructors

        public KeyboardHook()
            : base(Win32API.HookType.WH_KEYBOARD_LL)
        {
            BaseConfig.MyAnimeLog.Write("Adding hook to hook event handler");
            base.HookInvoked += new HookEventHandler(OnHookInvoked);
        }

        #endregion Constructors

        public void UnHook()
        {
            //BaseConfig.MyAnimeLog.Write("Removing hook from hook event handler");
            base.HookInvoked -= new HookEventHandler(OnHookInvoked);
        }

        ~KeyboardHook()
        {
            //BaseConfig.MyAnimeLog.Write("Closing hook");
            base.HookInvoked -= new HookEventHandler(OnHookInvoked);
        }

        #region Events

        public event KeyEventHandlerEx KeyDown;
        public event KeyEventHandlerEx KeyUp;

        #endregion Events

        #region Methods

        void OnHookInvoked(object sender, HookEventArgs e)
        {
            if ((e.WParam == WM_KEYDOWN || e.WParam == WM_SYSKEYDOWN) && KeyDown != null)
            {
                KeyboardHookStruct khs = new KeyboardHookStruct(e);
				KeyEventArgsEx kea = new KeyEventArgsEx(khs.virtualKey, Control.ModifierKeys, khs.scanCode);
                KeyDown(sender, kea);
                e.Handled = kea.Handled;
            }
            else if ((e.WParam == WM_KEYUP || e.WParam == WM_SYSKEYUP) && KeyUp != null)
            {
                KeyboardHookStruct khs = new KeyboardHookStruct(e);
				KeyEventArgsEx kea = new KeyEventArgsEx(khs.virtualKey, Control.ModifierKeys, khs.scanCode);
                KeyUp(sender, kea);
                e.Handled = kea.Handled;
            }
        }

        #endregion Methods

        #region Structures

        struct KeyboardHookStruct
        {
            public KeyboardHookStruct(HookEventArgs e)
            {
                KeyboardHookStruct khs = (KeyboardHookStruct)Marshal.PtrToStructure(e.LParam, typeof(KeyboardHookStruct));

                virtualKey = khs.virtualKey;
                scanCode = khs.scanCode;
                flags = khs.flags;
                time = khs.time;
                dwExtraInfo = khs.dwExtraInfo;
            }

            public int virtualKey;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;

            public override string ToString()
            {
                return "KeyboardHookStruct[" +
                    "virtualKey=" + virtualKey +
                    ",scanCode=" + scanCode +
                    ",flags=" + flags +
                    ",time=" + time +
                    ",dwExtraInfo=" + dwExtraInfo + "]";
            }
        }

        #endregion Structures
    }
}
