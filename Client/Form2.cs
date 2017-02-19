using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using CaptureScreen;
using System.Threading;

namespace RhClient
{
    public partial class Form2 : Form
    {
        Boolean use_image_source = false;
        Bitmap source_image, dest_image;
        int scroll_height, scroll_width;
        int width_current, height_current;
        Boolean upload_mouse_command = true;
        Boolean mouse_in_rect = false;
        Boolean Key_Handeled = true;
        Boolean active_window = false;
        Boolean icon_strech = false;
        TcpClient client = null;
        public Form1 Owner = null;
        public Boolean stop_receive_data = false;
        public Byte[] byte_to_send_clipboard = new Byte[12];
        public byte[] Data_image_byte = null;

        public int cursor_index = 0, cursor_index_last = -1;
        public int cursor_move_x = -1;
        public int cursor_move_y = -1;

        protected Form4 form_ft = null;

        int len_x = 0;
        int byte_per_color = 0;
        int rect_x = 0, rect_y = 0;
        int mouse_x = 0, mouse_y = 0;
        int mouse_x_last = -1, mouse_y_last = -1;
        int mouse_state_x = 0, mouse_state_y = 0;
        public bool update_image = false;
        bool mouse_move_upload = true;
        bool scroll_update = true;
        bool setting_mouse_move;
        public int fps = 0;
        Bitmap Bitmap_to_image = null;

        FormWindowState OldFormState;
        
        private static object critical_sektion = new object();

        public Form2()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, false);

            scroll_width = SystemInformation.VerticalScrollBarWidth;
            scroll_height = SystemInformation.HorizontalScrollBarHeight;
        }

        public int Scroll_W()
        {
            if (panel1.VerticalScroll.Value != 0)
                return scroll_width;
            else
                return 0;
        }

        public int Scroll_H()
        {
            if (panel1.HorizontalScroll.Value != 0)
                return scroll_height;
            else
                return 0;
        }

        public void SetParam(Int32 W, Int32 H, Int32 len, Int32 byte_pixel, Boolean mouse_move)
        {
            rect_x = W;
            rect_y = H;
            len_x = len;
            byte_per_color = byte_pixel;

            // создаем изображение 
            switch (byte_per_color)
            {
                case 1:
                    Make_Image(8);
                    break;
                case 2:
                    Make_Image_2bit();
                    break;
                case 3:
                    break;
            }

             setting_mouse_move = mouse_move;
             UpdateMouseCursorSend(false);
        }

        public void SetPicrtureSize(Size s)
        {          
            int deltaX = Width - panel1.Width;
            int deltaY = Height - panel1.Height;

            Width = s.Width + deltaX;
            Height = s.Height + deltaY;
        }

        public Size GetPicrtureBoxParam()
        {
            return pictureBox1.Size;
        }

        void Make_Image_2bit()
        {
            if (!use_image_source)
                pictureBox1.Image = new Bitmap(rect_x, rect_y, PixelFormat.Format16bppRgb565);
            else
                source_image = new Bitmap(rect_x, rect_y, PixelFormat.Format16bppRgb565);

            Data_image_byte = new byte[rect_x * rect_y * byte_per_color];
            Array.Clear(Data_image_byte, 0, rect_x * rect_y * byte_per_color);
        }

        void Make_Image(int bpp = 8)
        {
            // Заголовок 8bit изображения
            PlatformInvokeGDI32.BITMAPINFO bmi = new PlatformInvokeGDI32.BITMAPINFO();
            bmi.biSize = 40;
            bmi.biWidth = rect_x;
            bmi.biHeight = -rect_y;
            bmi.biPlanes = 1;
            bmi.biBitCount = (short)bpp;
            bmi.biCompression = PlatformInvokeGDI32.BI_RGB;
            bmi.biSizeImage = (uint)(((rect_x + 7) & 0xFFFFFFF8) * rect_y / 8);
            bmi.biXPelsPerMeter = 1000000;
            bmi.biYPelsPerMeter = 1000000;
            // Now for the colour table.
            uint ncols = (uint)1 << bpp;
            bmi.biClrUsed = ncols;
            bmi.biClrImportant = ncols;
            bmi.cols = new uint[256];
            if (bpp == 1) { bmi.cols[0] = MAKERGB(0, 0, 0); bmi.cols[1] = MAKERGB(255, 255, 255); }
            else
            {
                // Устанавливаем свою палитру
                bmi.biClrUsed = 256; bmi.biClrImportant = 256;                
                ConvertTo8Bit.SetPallete(bmi.cols);
            }
            IntPtr bits0;
            IntPtr hbm0 = PlatformInvokeGDI32.CreateDIBSection(IntPtr.Zero, ref bmi, PlatformInvokeGDI32.DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
            //
            if (!use_image_source)
                pictureBox1.Image = System.Drawing.Bitmap.FromHbitmap(hbm0);
            else
                source_image = System.Drawing.Bitmap.FromHbitmap(hbm0);

            PlatformInvokeGDI32.DeleteObject(hbm0);
            PlatformInvokeGDI32.DeleteObject(bits0);

            Data_image_byte = new byte[rect_x * rect_y * byte_per_color];
            Array.Clear(Data_image_byte, 0, rect_x * rect_y * byte_per_color);
        }

        uint MAKERGB(int r, int g, int b)
        {
            return ((uint)(b & 255)) | ((uint)((g & 255) << 16)) | ((uint)((r & 255) << 8));
        }
        
        void ImageToScreen()
        {
            if (update_image)
            {
                update_image = false;
                if (rect_x == 0 || rect_y == 0)
                    return;

                lock (Data_image_byte)
                {
                    Rectangle r = new Rectangle(0, 0, rect_x, rect_y);
                    BitmapData bmpd1 = null;
                    if (!use_image_source)                        
                        bmpd1 = ((Bitmap)pictureBox1.Image).LockBits(r, ImageLockMode.WriteOnly, ((Bitmap)pictureBox1.Image).PixelFormat);
                    else
                        bmpd1 = ((Bitmap)source_image).LockBits(r, ImageLockMode.WriteOnly, ((Bitmap)source_image).PixelFormat);
                    
                    int offset = 0;
                    
                    for (int i = 0; i < rect_y; i++)
                    {
                        IntPtr pointer = new IntPtr(bmpd1.Scan0.ToInt32() + (bmpd1.Stride * (i)));
                        System.Runtime.InteropServices.Marshal.Copy(Data_image_byte, offset, pointer, rect_x * byte_per_color);
                        offset = offset + rect_x * byte_per_color;
                    }
    
                    if (!use_image_source)
                        ((Bitmap)pictureBox1.Image).UnlockBits(bmpd1);                    
                    else
                        ((Bitmap)source_image).UnlockBits(bmpd1);

                    if (use_image_source)
                    {
                        // растягиваем картинку
                        if (dest_image != null)
                            dest_image.Dispose();
                        if (pictureBox1.Width > 0 && pictureBox1.Height > 0)
                        {
                            dest_image = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format24bppRgb);
                            dest_image.SetResolution(source_image.HorizontalResolution, source_image.VerticalResolution);

                            using (Graphics g = Graphics.FromImage(dest_image))
                            {
                                var destRect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);

                                g.CompositingMode = CompositingMode.SourceCopy;
                                g.CompositingQuality = CompositingQuality.HighSpeed;
                                g.InterpolationMode = InterpolationMode.Bicubic;
                                g.SmoothingMode = SmoothingMode.HighSpeed;
                                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                                g.DrawImage(source_image, 0, 0, pictureBox1.Width, pictureBox1.Height);
                            }
                            if (pictureBox1.Image != null)
                                pictureBox1.Image.Dispose();

                            pictureBox1.Image = dest_image;
                        }

                    }
                    pictureBox1.Invalidate();
                }
                fps += 1;
            }

            if (cursor_index_last != cursor_index)
            {
                // отображаем известные курсоры в режиме передачи не изображения курсора, а его типа
                switch (cursor_index)
                {
                    case (Int32)MouseCursor.AppStarting:
                        pictureBox1.Cursor = Cursors.AppStarting;
                        break;
                    case (Int32)MouseCursor.Arrow:
                        pictureBox1.Cursor = Cursors.Arrow;
                        break;
                    case (Int32)MouseCursor.Cross:
                        pictureBox1.Cursor = Cursors.Cross;
                        break;
                    case (Int32)MouseCursor.Default:
                        pictureBox1.Cursor = Cursors.Default;
                        break;
                    case (Int32)MouseCursor.Hand:
                        pictureBox1.Cursor = Cursors.Hand;
                        break;
                    case (Int32)MouseCursor.Help:
                        pictureBox1.Cursor = Cursors.Help;
                        break;
                    case (Int32)MouseCursor.HSplit:
                        pictureBox1.Cursor = Cursors.HSplit;
                        break;
                    case (Int32)MouseCursor.IBeam:
                        pictureBox1.Cursor = Cursors.IBeam;
                        break;
                    case (Int32)MouseCursor.No:
                        pictureBox1.Cursor = Cursors.No;
                        break;
                    case (Int32)MouseCursor.NoMove2D:
                        pictureBox1.Cursor = Cursors.NoMove2D;
                        break;
                    case (Int32)MouseCursor.NoMoveHoriz:
                        pictureBox1.Cursor = Cursors.NoMoveHoriz;
                        break;
                    case (Int32)MouseCursor.NoMoveVert:
                        pictureBox1.Cursor = Cursors.NoMoveVert;
                        break;
                    case (Int32)MouseCursor.PanEast:
                        pictureBox1.Cursor = Cursors.PanEast;
                        break;
                    case (Int32)MouseCursor.PanNE:
                        pictureBox1.Cursor = Cursors.PanNE;
                        break;
                    case (Int32)MouseCursor.PanNorth:
                        pictureBox1.Cursor = Cursors.PanNorth;
                        break;
                    case (Int32)MouseCursor.PanNW:
                        pictureBox1.Cursor = Cursors.PanNW;
                        break;
                    case (Int32)MouseCursor.PanSE:
                        pictureBox1.Cursor = Cursors.PanSE;
                        break;
                    case (Int32)MouseCursor.PanSouth:
                        pictureBox1.Cursor = Cursors.PanSouth;
                        break;
                    case (Int32)MouseCursor.PanSW:
                        pictureBox1.Cursor = Cursors.PanSW;
                        break;
                    case (Int32)MouseCursor.PanWest:
                        pictureBox1.Cursor = Cursors.PanWest;
                        break;
                    case (Int32)MouseCursor.SizeAll:
                        pictureBox1.Cursor = Cursors.SizeAll;
                        break;
                    case (Int32)MouseCursor.SizeNESW:
                        pictureBox1.Cursor = Cursors.SizeNESW;
                        break;
                    case (Int32)MouseCursor.SizeNS:
                        pictureBox1.Cursor = Cursors.SizeNS;
                        break;
                    case (Int32)MouseCursor.SizeNWSE:
                        pictureBox1.Cursor = Cursors.SizeNWSE;
                        break;
                    case (Int32)MouseCursor.SizeWE:
                        pictureBox1.Cursor = Cursors.SizeWE;
                        break;
                    case (Int32)MouseCursor.UpArrow:
                        pictureBox1.Cursor = Cursors.UpArrow;
                        break;
                    case (Int32)MouseCursor.VSplit:
                        pictureBox1.Cursor = Cursors.VSplit;
                        break;
                    case (Int32)MouseCursor.WaitCursor:
                        pictureBox1.Cursor = Cursors.WaitCursor;
                        break;
                    default:
                        pictureBox1.Cursor = Cursors.Default;
                        break;
                }

                cursor_index_last = cursor_index;
            }

            // позиция курсора
            if (cursor_move_x >= 0 && cursor_move_y >= 0)
            {
                
                Point start_point = pictureBox1.PointToScreen(new Point(0, 0));
                Point end_point = pictureBox1.PointToScreen(new Point(panel1.Width - Scroll_W(), panel1.Height - Scroll_H()));

                lock (critical_sektion)
                {
                    Point cur_pos = Cursor.Position;
                    if ((cur_pos.X >= start_point.X && cur_pos.X <= end_point.X) && (cur_pos.Y >= start_point.Y && cur_pos.Y <= end_point.Y) && active_window)
                    {
                        InverseCalcMouseXY(ref cursor_move_x, ref cursor_move_y);

                        mouse_x = cursor_move_x;
                        mouse_y = cursor_move_y;
                        mouse_x_last = mouse_x;
                        mouse_y_last = mouse_y;

                        int x = start_point.X + cursor_move_x;
                        int y = start_point.Y + cursor_move_y;
                        
                        Cursor.Position = new Point(x, y);
                    }

                    cursor_move_x = -1;
                    cursor_move_y = -1;
                }
                
            }
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            Upload_Key_Down(e.KeyCode.GetHashCode(), e.Modifiers.GetHashCode());    
            e.Handled = Key_Handeled;
        }

        private void Form2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Key_Handeled;
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            Upload_Key_Up(e.KeyCode.GetHashCode(), e.Modifiers.GetHashCode());
            e.Handled = Key_Handeled;
        }

        void Upload_Mouse_Data(Byte[] Buf, Boolean fast_send = false)
        {
            if(Owner != null)
                Owner.MouseAndKeyboardDataUpdate(Buf, fast_send);
        }

        void Upload_Mouse_Move()
        {
            // конвертируем данные мыши
            int x = mouse_x;
            int y = mouse_y;

            CalcMouseXY(ref x, ref y);
            Byte[] byte_to_send = new Byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.move.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byte_to_send, 8, 4);
            Upload_Mouse_Data(byte_to_send);
            mouse_move_upload = true;
            mouse_x_last = mouse_x;
            mouse_y_last = mouse_y;
        }

        void Upload_Mouse_Click(int x, int y, int hash)
        {
            // конвертируем данные мыши
            CalcMouseXY(ref x, ref y);
            Byte[] byte_to_send = new Byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.click.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byte_to_send, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(hash), 0, byte_to_send, 12, 4);
            Upload_Mouse_Data(byte_to_send, true);
            mouse_move_upload = true;
        }

        void Upload_Mouse_Down(int x, int y, int hash)
        {
            CalcMouseXY(ref x, ref y);
            Byte[] byte_to_send = new Byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.mouse_down.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byte_to_send, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(hash), 0, byte_to_send, 12, 4);
            Upload_Mouse_Data(byte_to_send, true);
        }

        void Upload_Mouse_Up(int x, int y, int hash)
        {
            CalcMouseXY(ref x, ref y);
            Byte[] byte_to_send = new Byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.mouse_up.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byte_to_send, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(hash), 0, byte_to_send, 12, 4);
            Upload_Mouse_Data(byte_to_send, true);
        }

        void Upload_Mouse_Wheel(int delta)
        {
            Byte[] byte_to_send = new Byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.mouse_wheel.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(delta), 0, byte_to_send, 4, 4);
            Upload_Mouse_Data(byte_to_send, true);
        }

        void Upload_Key_Down(int Key, int Key2, string text = "")
        {
            Byte[] stext = null;
            Byte[] byte_to_send = null;

            if (text.Length != 0)
                stext = Encoding.UTF8.GetBytes(text);
            if (stext == null)
            {
                byte_to_send = new Byte[12];
                Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.key_down.GetHashCode()), 0, byte_to_send, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, byte_to_send, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Key2), 0, byte_to_send, 8, 4);
            }
            else
            {
                byte_to_send = new Byte[12 + stext.Length];
                Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.key_down.GetHashCode()), 0, byte_to_send, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, byte_to_send, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Key2), 0, byte_to_send, 8, 4);
                Buffer.BlockCopy(stext, 0, byte_to_send, 12, stext.Length);
            }
            Upload_Mouse_Data(byte_to_send, true);
        }
        void Upload_Key_Up(int Key, int Key2)
        {
            Byte[] byte_to_send = new Byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.key_up.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Key2), 0, byte_to_send, 8, 4);
            Upload_Mouse_Data(byte_to_send, true);
        }
        void Upload_Key_Press(char r)
        {
            Byte[] byte_to_send = new Byte[6];
            Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.key_press.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(r), 0, byte_to_send, 4, 2);
            Upload_Mouse_Data(byte_to_send, true);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (upload_mouse_command && active_window)
                Upload_Mouse_Down(e.X, e.Y, e.Button.GetHashCode());
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_x = (e.X);
            mouse_y = (e.Y);

            mouse_state_x = e.X + pictureBox1.Left;
            mouse_state_y = e.Y + pictureBox1.Top;
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (upload_mouse_command)
                Upload_Mouse_Wheel(e.Delta);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (upload_mouse_command && active_window)
                Upload_Mouse_Up(e.X, e.Y, e.Button.GetHashCode());
        }

        private void timer_mouse_move_Tick(object sender, EventArgs e)
        {
            if (mouse_move_upload && setting_mouse_move)
            {
                lock (critical_sektion)
                {
                    mouse_move_upload = false;
                    if (mouse_x >= 0 && mouse_y >= 0 && (mouse_x_last != mouse_x || mouse_y_last != mouse_y))
                        Upload_Mouse_Move();
                    else
                        mouse_move_upload = true;
                }
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            timer1.Enabled = false;            
            ImageToScreen();
            timer1.Enabled = true;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (form_ft != null)
                form_ft.Close();
            stop_receive_data = true;
        }

        protected void UpdateSizeFrame()
        {
            // подгоняем изображение по центру окна, актуально в режим исходного изображения
            lock (this)
            {       
                if (pictureBox1.Width < panel1.Width)
                    pictureBox1.Left = -panel1.HorizontalScroll.Value + (panel1.Width - rect_x) / 2;
                else
                    pictureBox1.Left = -panel1.HorizontalScroll.Value;

                if (pictureBox1.Height < panel1.Height)
                    pictureBox1.Top = -panel1.VerticalScroll.Value + (panel1.Height - rect_y) / 2;
                else
                    pictureBox1.Top = -panel1.VerticalScroll.Value;
            }
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized || (WindowState == FormWindowState.Normal && OldFormState == FormWindowState.Maximized))
            {
                UpdateSizeFrame();
                update_image = true;
                scroll_update = true;
                timer2.Enabled = true;
            }
            OldFormState = WindowState;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            OldFormState = WindowState;
            label1.Text = "";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (scroll_update)
            {
                lock (this)
                {
                    if (mouse_in_rect)
                    {
                        int delta = 70;
                        int d = 10;
                        int s = 0;
                        float k = 1.2f;

                        if (panel1.HorizontalScroll.Visible)
                        {
                            if (panel1.HorizontalScroll.Value != panel1.HorizontalScroll.Maximum)
                            {
                                int ostatok = (panel1.Width - mouse_state_x);
                                if (ostatok <= 0)
                                    ostatok = 1;

                                s = 30;

                                if (ostatok <= delta)
                                    if (panel1.HorizontalScroll.Value + s > panel1.HorizontalScroll.Maximum)
                                        panel1.HorizontalScroll.Value = panel1.HorizontalScroll.Maximum;
                                    else
                                        panel1.HorizontalScroll.Value += s;
                            }

                            if (panel1.HorizontalScroll.Value != panel1.HorizontalScroll.Minimum)
                            {
                                int ostatok = mouse_state_x;
                                if (ostatok <= 0)
                                    ostatok = 0;

                                s = 30;
                                if (ostatok < delta)
                                {
                                    if (panel1.HorizontalScroll.Value - s < panel1.HorizontalScroll.Minimum)
                                        panel1.HorizontalScroll.Value = panel1.HorizontalScroll.Minimum;
                                    else
                                        panel1.HorizontalScroll.Value -= s;
                                }
                            }
                        }

                        if (panel1.VerticalScroll.Visible)
                        {
                            if (panel1.VerticalScroll.Value != panel1.VerticalScroll.Maximum)
                            {
                                int ostatok = (panel1.Height - mouse_state_y);
                                if (ostatok <= 0)
                                    ostatok = 1;
                                s = 30;
                                if (ostatok <= delta)
                                    if (panel1.VerticalScroll.Value + s > panel1.VerticalScroll.Maximum)
                                        panel1.VerticalScroll.Value = panel1.VerticalScroll.Maximum;
                                    else
                                        panel1.VerticalScroll.Value += s;
                            }

                            if (panel1.VerticalScroll.Value != panel1.VerticalScroll.Minimum)
                            {
                                int ostatok = mouse_state_y;
                                if (ostatok <= 0)
                                    ostatok = 0;
                                s = 30;
                                if (ostatok < delta)
                                    if (panel1.VerticalScroll.Value - s < panel1.VerticalScroll.Minimum)
                                        panel1.VerticalScroll.Value = panel1.VerticalScroll.Minimum;
                                    else
                                        panel1.VerticalScroll.Value -= s;
                            }
                        }
                    }
                }
            
            }             
        }

        protected void CalcMouseXY(ref int x, ref int y)
        {
            if (pictureBox1.Width != rect_x)
            {
                float scale_x = 1.0f * (rect_x) / (pictureBox1.Width);
                x = (int)(Math.Round(1.0f * x * scale_x, 0));
            }

            if (pictureBox1.Height != rect_y)
            {
                float scale_y = 1.0f * (rect_y) / (pictureBox1.Height);
                y = (int)(Math.Round(1.0f * y * scale_y, 0));
            }
        }

        protected void InverseCalcMouseXY(ref int x, ref int y)
        {
            if (pictureBox1.Width != rect_x)
            {
                x = (int)(Math.Round(1.0f * x * pictureBox1.Width / rect_x, 0));
                if (x > pictureBox1.Width)
                    x = pictureBox1.Width;
            }

            if (pictureBox1.Height != rect_y)
            {
                y = (int)(Math.Round(1.0f * y * pictureBox1.Height / rect_y, 0));
                if (y > pictureBox1.Height)
                    y = pictureBox1.Height;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_state_x = e.X;
            mouse_state_y = e.Y;
        }

        private void Form2_ResizeBegin(object sender, EventArgs e)
        {
            if (scroll_update)
                scroll_update = false;
            if (timer2.Enabled)
                timer2.Enabled = false;

            width_current = Width;
            height_current = Height;
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)
        {
            if (timer2.Enabled)
                timer2.Enabled = false;
            UpdateSizeFrame();
            update_image = true;

            if (width_current != 0 && height_current != 0 && icon_strech)
            {
                
                int deltaX = Width - panel1.Width;
                int deltaY = Height - panel1.Height;

                Size s = this.Size;
                s.Width -= deltaX;
                s.Height -= deltaY;

                int dw = s.Width - (width_current - deltaX);

                int dh = s.Height - (height_current - deltaY);

                if (dh == 0 && dw == 0)
                    return;

                if (Math.Abs(dw) > Math.Abs(dh))
                {
                    dh = s.Width - (rect_x);
                    double k = (double)(rect_x) / (double)(rect_y == 0 ? 1 : (rect_y));
                    s.Height = (rect_y) + (int)(dh / k);
                }
                else
                    if (Math.Abs(dw) < Math.Abs(dh))
                    {
                        dh = s.Height - (rect_y);
                        double k = (double)(rect_x) / (double)(rect_y == 0 ? 1 : (rect_y));
                        s.Width = (rect_x) + (int)(dh * k);
                    }

                s.Width += deltaX;
                s.Height += deltaY;
                Size = s;

            }

            scroll_update = true;
            timer2.Enabled = true;

            width_current = 0;
            height_current = 0;
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            if (width_current == 0 && height_current == 0)
                return;

            if (scroll_update)
                scroll_update = false;
            if (timer2.Enabled)
                timer2.Enabled = false;
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            mouse_in_rect = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            mouse_in_rect = false;
        }

        private void panel1_MouseHover(object sender, EventArgs e)
        {
            mouse_in_rect = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            UpdateMouseCursorSend();
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            mouse_in_rect = false;
        }

        private void panel1_Resize(object sender, EventArgs e)
        {

        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {
            UpdateMouseCursorSend();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (Owner.Form_FileTransfer == null)
            {
                form_ft = new Form4();
                Owner.Form_FileTransfer = form_ft;
                form_ft.SetOwner(Owner, Owner.file_transfer_upload);
                form_ft.Show();
            }
            else
            {
                if (Owner.Form_FileTransfer.WindowState == FormWindowState.Minimized)
                    Owner.Form_FileTransfer.WindowState = FormWindowState.Normal;
                Owner.Form_FileTransfer.Activate();
            }
        }

        private void label3_Click_1(object sender, EventArgs e)
        {
            UpdatePictureBoxParam();
            update_image = true;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            label1.Text = "Количество кадров в секунду:" + fps.ToString() + ". Поток Кб/с: " + (Math.Round(1.0f * Owner.read_data_kb/8, 1)).ToString();
            fps = 0;
            Owner.read_data_kb = 0;
        }

        public void UpdateMouseCursorSend(Boolean update = true)
        {
            if (update)
                setting_mouse_move = !setting_mouse_move;

            if (setting_mouse_move)
            {
                label5.BorderStyle = BorderStyle.Fixed3D;
                toolTip1.SetToolTip(label5, "Отключить передачу координат курсора");
            }
            else
            {
                label5.BorderStyle = BorderStyle.None;
                toolTip1.SetToolTip(label5, "Включить передачу координат курсора");
            }                        
        }

        public void UpdatePictureBoxParam()
        {
            if (label3.BorderStyle == BorderStyle.Fixed3D)
                label3.BorderStyle = BorderStyle.None;
            else
                label3.BorderStyle = BorderStyle.Fixed3D;

            if (label3.BorderStyle == BorderStyle.Fixed3D)
            {
                pictureBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

                if (use_image_source)
                    pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                else
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

                pictureBox1.Left = 0;
                pictureBox1.Top = 0;

                Size s = this.ClientSize;
                Size cs = this.ClientSize;

                int delta = (ClientSize.Height - panel1.Height);

                s.Height -= delta;
                int dh = s.Height - (rect_y);
                double k = (double)(rect_x) / (double)(rect_y == 0 ? 1 : (rect_y));
                s.Width = (rect_x) + (int)(dh * k);
                cs.Width = s.Width;

                this.ClientSize = cs;

                pictureBox1.Width = s.Width;
                pictureBox1.Height = s.Height;

                icon_strech = true;
                toolTip1.SetToolTip(label3, "Включить исходный масштаб изображения");
            }
            else
            {
                icon_strech = false;
                scroll_update = true;
                pictureBox1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                Size s = pictureBox1.Size;
                s.Width = rect_x;
                s.Height = rect_y;
                pictureBox1.Size = s;
                toolTip1.SetToolTip(label3, "Включить автомасштаб (подстройка под размер окна)");
                timer2.Enabled = true;
            }
            UpdateSizeFrame();
            pictureBox1.Invalidate();
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            active_window = true;
        }

        private void Form2_Deactivate(object sender, EventArgs e)
        {
            active_window = false;
        }   
    }
}
