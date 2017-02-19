using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CaptureScreen
{    
    public class CaptureScreen
    {    
        protected static IntPtr m_HBitmap = IntPtr.Zero;
        static PlatformInvokeUSER32.CURSORINFO pci;
        static PlatformInvokeUSER32.ICONINFO icInfo;
        static IntPtr hMemDC = IntPtr.Zero;
    
        public static void StopScreen()
        {
            if (hMemDC != IntPtr.Zero)
            {
                PlatformInvokeGDI32.DeleteDC(hMemDC);
                hMemDC = IntPtr.Zero;
            }
            if (m_HBitmap != IntPtr.Zero)
            {
                PlatformInvokeGDI32.DeleteObject(m_HBitmap);
                m_HBitmap = IntPtr.Zero;
            }
            ConvertTo8Bit.Stop();
        }

        public static void GetDesktopImage(ref Bitmap active_screen, int byte_per_pixel, int ScreenNumber = 0, int scale_width = 0, int scale_height = 0, int Draw_mouse = 0)
        {
            GetDesktopImage_GDI(ref active_screen, byte_per_pixel, ScreenNumber, scale_width, scale_height, Draw_mouse);                        
        }

        protected static void GetDesktopImage_GDI(ref Bitmap active_screen, int byte_per_pixel, int ScreenNumber = 0, int scale_width = 0, int scale_height = 0, int Draw_mouse = 0)
        {
            int XX, YY, WW, HH = 0;
            int source_width, source_height = 0;

            if (ScreenNumber == 0)
            {
                XX = Screen.PrimaryScreen.Bounds.X;
                YY = Screen.PrimaryScreen.Bounds.Y;
                WW = Screen.PrimaryScreen.Bounds.Width;
                HH = Screen.PrimaryScreen.Bounds.Height;
                source_width = Screen.PrimaryScreen.Bounds.Width;
                source_height = Screen.PrimaryScreen.Bounds.Height;
            }
            else
            {
                XX = Screen.AllScreens[ScreenNumber].Bounds.X;
                YY = Screen.AllScreens[ScreenNumber].Bounds.Y;
                WW = Screen.AllScreens[ScreenNumber].Bounds.Width;
                HH = Screen.AllScreens[ScreenNumber].Bounds.Height;
                source_width = Screen.PrimaryScreen.Bounds.Width;
                source_height = Screen.PrimaryScreen.Bounds.Height;
            }
         
            IntPtr local_DC = PlatformInvokeUSER32.GetDC(PlatformInvokeUSER32.GetDesktopWindow());

            if (hMemDC == IntPtr.Zero)
                hMemDC = PlatformInvokeGDI32.CreateCompatibleDC(local_DC);            
            
            if (m_HBitmap == IntPtr.Zero)
                m_HBitmap = PlatformInvokeGDI32.CreateCompatibleBitmap(local_DC, WW, HH);

            if (m_HBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)PlatformInvokeGDI32.SelectObject(hMemDC, m_HBitmap);
                PlatformInvokeGDI32.BitBlt(hMemDC, 0, 0, WW, HH, local_DC, XX, YY, 0x00CC0020 | 0x40000000);
                
                // Данные курсора
                if (Draw_mouse == 1)
                {
                    pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PlatformInvokeUSER32.CURSORINFO));

                    if (PlatformInvokeUSER32.GetCursorInfo(out pci))
                    {
                        if (pci.flags == PlatformInvokeUSER32.CURSOR_SHOWING)
                        {
                            if (PlatformInvokeUSER32.GetIconInfo(pci.hCursor, out icInfo))
                            {
                                // корректная позиция курсора
                                int iconX = pci.ptScreenPos.x - ((int)icInfo.xHotspot);
                                int iconY = pci.ptScreenPos.y - ((int)icInfo.yHotspot);

                                // рисуем курсор
                                PlatformInvokeUSER32.DrawIcon(hMemDC, iconX, iconY, pci.hCursor);

                                PlatformInvokeGDI32.DeleteObject(icInfo.hbmColor);
                                PlatformInvokeGDI32.DeleteObject(icInfo.hbmMask);
                                PlatformInvokeGDI32.DeleteObject(pci.hCursor);
                            }
                        }
                    }
                }

                if (scale_width != 0 && scale_height != 0 && scale_width < WW && scale_height < HH)
                {
                    int val = PlatformInvokeGDI32.SetStretchBltMode(hMemDC, PlatformInvokeGDI32.StretchBltMode.STRETCH_HALFTONE);

                    PlatformInvokeGDI32.StretchBlt(hMemDC, 0, 0, scale_width, scale_height, hMemDC, 0, 0, WW, HH, 0x00CC0020 | 0x40000000);

                    PlatformInvokeGDI32.SetStretchBltMode(hMemDC, (PlatformInvokeGDI32.StretchBltMode)val);

                    WW = scale_width;
                    HH = scale_height;
                }

                PlatformInvokeGDI32.SelectObject(hMemDC, hOld);
                PlatformInvokeUSER32.ReleaseDC(PlatformInvokeUSER32.GetDesktopWindow(), local_DC);

                if (active_screen != null)
                    active_screen.Dispose();

                switch (byte_per_pixel)
                {
                    case 1:
                        ConvertTo8Bit.CopyToBpp(m_HBitmap, WW, HH, 8, ref active_screen);
                        break;
                    case 2:
                        active_screen = new Bitmap(WW, HH, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                        Graphics gr = Graphics.FromImage(active_screen);
                        Bitmap img = Image.FromHbitmap(m_HBitmap);
                        gr.DrawImage(img, new Rectangle(0, 0, source_width, source_height));
                        img.Dispose();
                        img = null;
                        gr.Dispose();
                        gr = null;
                        break;
                    case 3:
                        break;
                }
            }
        }
    }
}
