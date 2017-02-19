using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;

// Author
// https://www.codeproject.com/articles/28064/global-mouse-and-keyboard-library

namespace MouseKeyboardLibrary
{

    /// <summary>
    /// Standard Keyboard Shortcuts used by most applications
    /// </summary>
    public enum StandardShortcut
    {
        Copy,
        Cut,
        Paste,
        SelectAll,
        Save,
        Open,
        New,
        Close,
        Print
    }

    /// <summary>
    /// Simulate keyboard key presses
    /// </summary>
    public static class KeyboardSimulator
    {

        #region Windows API Code

        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte key, byte scan, int flags, int extraInfo); 

        #endregion

        #region Methods

        public static void KeyDown(Keys key, Boolean ext = false)
        {
            
            byte sk = (byte)CaptureScreen.PlatformInvokeUSER32.MapVirtualKey((uint) (key.GetHashCode()), 0);

            if (ext)
            {
                keybd_event(ParseKey(key), sk, KEYEVENTF_EXTENDEDKEY | 0, 0);
            }
            else
                keybd_event(ParseKey(key), sk, 0, 0);
        }

        public static void KeyUp(Keys key, Boolean ext = false)
        {
            byte sk = (byte)CaptureScreen.PlatformInvokeUSER32.MapVirtualKey((uint)(key.GetHashCode()), 0);
            if (ext)
            {
                keybd_event(ParseKey(key), (byte) (sk | 0x80), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
            else
                keybd_event(ParseKey(key), (byte) (sk | 0x80), KEYEVENTF_KEYUP, 0);

        }

        public static void KeyPress(Keys key)
        {
            KeyDown(key);
            KeyUp(key);
        }

        public static void SimulateStandardShortcut(StandardShortcut shortcut)
        {
            switch (shortcut)
            {
                case StandardShortcut.Copy:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.C);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.Cut:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.X);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.Paste:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.V);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.SelectAll:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.A);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.Save:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.S);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.Open:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.O);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.New:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.N);
                    KeyUp(Keys.Control);
                    break;
                case StandardShortcut.Close:
                    KeyDown(Keys.Alt);
                    KeyPress(Keys.F4);
                    KeyUp(Keys.Alt);
                    break;
                case StandardShortcut.Print:
                    KeyDown(Keys.Control);
                    KeyPress(Keys.P);
                    KeyUp(Keys.Control);
                    break;
            }
        }

        static byte ParseKey(Keys key)
        {
            return (byte)key;
            // Alt, Shift, and Control need to be changed for API function to work with them
            switch (key)
            {                
                case Keys.Alt:
                    return (byte)18;
                case Keys.Control:
                    return (byte)17;
                case Keys.Shift:
                    return (byte)16;
                default:
                    return (byte)key;
            }
        }

        #endregion

    }

}
