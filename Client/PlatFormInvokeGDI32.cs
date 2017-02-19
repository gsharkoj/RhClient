using System;
using System.Runtime.InteropServices;

namespace CaptureScreen
{
    /// <summary>
    /// GDI32 API
    /// </summary>
    public class PlatformInvokeGDI32
    {

        #region Class Variables
        public const int SRCCOPY = 13369376;
        #endregion

        #region Class Functions

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteObject(IntPtr hDc);
        
        [DllImport("gdi32.dll")]
        public static extern int InvalidateRect(IntPtr hwnd, IntPtr rect, int bErase);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hdcDst, int xDst, int yDst, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            int dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg,
           IntPtr lppt);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, StretchBltMode iStretchMode);

        public enum StretchBltMode : int
        {
            STRETCH_ANDSCANS = 1,
            STRETCH_ORSCANS = 2,
            STRETCH_DELETESCANS = 3,
            STRETCH_HALFTONE = 4,
        }

        public static uint BI_RGB = 0;
        public static uint DIB_RGB_COLORS = 0;

        #endregion


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public uint biSize;
            public int biWidth, biHeight;
            public short biPlanes, biBitCount;
            public uint biCompression, biSizeImage;
            public int biXPelsPerMeter, biYPelsPerMeter;
            public uint biClrUsed, biClrImportant;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] cols;
        }
    }
}
