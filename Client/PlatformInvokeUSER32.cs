using System;
using System.Runtime.InteropServices;

namespace CaptureScreen
{
    /// <summary>
    /// User32 API
    /// </summary>
    public class PlatformInvokeUSER32
    {

        #region Class Variables
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const Int32 CURSOR_SHOWING = 0x00000001;
        #endregion

        #region Class Functions
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgn, uint flags);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hwnd, IntPtr rect, bool erase);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint ideviceIndex, ref DisplayDevice lpdevice, uint dwFlags);

        [Flags]
        public enum DrawingOptions
        {
            PRF_CHECKVISIBLE = 0x01,
            PRF_NONCLIENT = 0x02,
            PRF_CLIENT = 0x04,
            PRF_ERASEBKGND = 0x08,
            PRF_CHILDREN = 0x10,
            PRF_OWNED = 0x20
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTAPI
        {
            public int x;
            public int y;
        }
        
        internal const int WM_PRINT = 0x0317;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg,
            IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public  static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        public static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetIconInfo")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 
            public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 
            public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 
            public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 
            public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceMode
        {

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmSpecVersion;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmDriverVersion;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmSize;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmDriverExtra;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmFields;

            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmOrientation;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmPaperSize;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmPaperLength;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmPaperWidth;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmScale;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmCopies;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmDefaultSource;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmPrintQuality;


            [MarshalAs(UnmanagedType.I2)] // short
            public short dmColor;
            [MarshalAs(UnmanagedType.I2)] // short
            public short dmDuplex;
            [MarshalAs(UnmanagedType.I2)] // short
            public short dmYResolution;
            [MarshalAs(UnmanagedType.I2)] // short
            public short dmTTOption;
            [MarshalAs(UnmanagedType.I2)] // short
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            [MarshalAs(UnmanagedType.U2)] // WORD
            public short dmLogPixels;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmBitsPerPel;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmPelsWidth;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmPelsHeight;

            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmDisplayFlags;

            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmDisplayFrequency;

            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmICMMethod;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmICMIntent;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmMediaType;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmDitherType;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmReserved1;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmReserved2;

            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmPanningWidth;
            [MarshalAs(UnmanagedType.U4)] // DWORD
            public int dmPanningHeight;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DisplayDevice
        {
            public int CallBack;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
        #endregion

        #region External Constants

        public const int Map = 1030;
        public const int UnMap = 1031;
        public const int TestMapped = 1051;

        public const int IGNORE = 0;
        public const int BLIT = 12;
        public const int TEXTOUT = 18;
        public const int MOUSEPTR = 48;

        public const int CDS_UPDATEREGISTRY = 0x00000001;
        public const int CDS_TEST = 0x00000002;
        public const int CDS_FULLSCREEN = 0x00000004;
        public const int CDS_GLOBAL = 0x00000008;
        public const int CDS_SET_PRIMARY = 0x00000010;
        public const int CDS_RESET = 0x40000000;
        public const int CDS_SETRECT = 0x20000000;
        public const int CDS_NORESET = 0x10000000;
        public const int MAXIMUM_ALLOWED = 0x02000000;
        public const int DM_BITSPERPEL = 0x40000;
        public const int DM_PELSWIDTH = 0x80000;
        public const int DM_PELSHEIGHT = 0x100000;
        public const int DM_POSITION = 0x00000020;
        #endregion

        #region Public Constructor
        public PlatformInvokeUSER32()
        {
            // 
            // TODO: Add constructor logic here
            //
        }
        #endregion
    }
   
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}
