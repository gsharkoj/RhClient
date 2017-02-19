using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using CaptureScreen;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace RhClient
{
    public partial class Form1 : Form
    {
        int state_Height = 0;
        int len_x = 0;
        int scale_width, scale_height = 0;
        int scale_width_send, scale_height_send = 0;
        int sleep_param_screen = 70;
        const int sleep_param_screen_max = 300;
        const int sleep_param_screen_min = 70;

        float scale_x, scale_y = 1.0f;
        int byte_per_pixel = 1;                
        Bitmap active_screen = null;
        Bitmap ActiveScreen = null;
        Boolean image_update = true;
        Boolean first_image_update;
        Boolean CloseForm = false;
        Boolean CloseForm_disconnect = false;
        Boolean manage_mouse_data_update = false;
        Boolean connect_to_client = false;
        Boolean send_echo = true;
        ClientType Client_type = ClientType.Undefine;        

        DateTime datetime_send_echo;

        Int32 Draw_mouse = 1;
        Int32 setting_draw_mouse = 0;

        Int32 setting_palette = 8; 
        String setting_resolution = "";
        String setting_url = "";
        String setting_user_data = "";
        Boolean setting_mouse_move = false;

        Int32 ID = 0;
        Int32 Size_W = 0;
        Int32 Size_H = 0;
        Thread thread_base, thread_screen = null;
        bool thread_screen_status = true;

        public TcpClient client = null;
        public Form2 Form_View = null;
        public Form4 Form_FileTransfer = null;

        public MouseKeyboardLibrary.MouseHook mouse_hook = null;
        public MouseEventHandler event_mouse = null;

        int hook_mouse_x = 0, hook_mouse_y = 0;
        int hook_mouse_prev_x = 0, hook_mouse_prev_y = 0;
        public Dictionary<Int32, MouseCursor> dict_index_cursor;
        public int index_cursor_last = 0;

        public string clipboard_string = "";

        byte[] Buffer_line = null;
        Int32 Buffer_line_index = 0;
        const int buf_gz_length = 8192 * 2;
        byte[] buf_gz = new byte[buf_gz_length];

        byte[] image_data_unzip = null;
        byte[] image_data_zip = null;

        public FileTransferJobManagerDownload file_transfer_download = null;
        public FileTransferJobManager file_transfer_upload = null;

        public double read_data_kb = 0;

        private static object critical_sektion = new object();
        private PlatformInvokeUSER32.CURSORINFO pci_cursor;

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]        
        static extern int memcpy(IntPtr b1, IntPtr b2, long count);

        delegate void SetTextCallback(string text);
        delegate void FormView(Boolean state);
        delegate void TimerSendImage(Boolean state);
        delegate void TimerSendPing(Boolean state);
        delegate void TimerSendEcho(Boolean state);
        delegate void SetButtonState(Boolean state);
        delegate DialogResult ShowMessageBoxYesNoDelegate(String text1, String text2);
        delegate void ShowMessageBoxDelegate(String text1, String text2, MessageBoxIcon mbi);

        delegate void MinimizeWindowDelegate();
        delegate void SetMouseHook(Boolean state);

        public Form1(String ip, String port, String close, String palette, String resolution, String url, String draw_mouse, String mouse_move, String user_data)
        {         
            InitializeComponent();

            state_Height = Height;

            setting_url = url;

            if (ip != "")
                textBox1.Text = ip;
            else
                textBox1.Text = "localhost";

            if (port != "")
                textBox2.Text = port;
            else
            {
                port = "45823";
                textBox2.Text = port;
            }

            if (close.ToLower() == "true" || close.ToLower() == "on")
            {
                CloseForm_disconnect = true;
                
                textBox1.ReadOnly = true;
                textBox4.ReadOnly = true;
                linkLabel1.Enabled = false;
                button2.Enabled = false;

                ClientOnly();
            }

            // palette
            if (palette != "")
            {
                if (palette == "8")
                    setting_palette = 8;
                else
                    if (palette == "16")
                        setting_palette = 16;
                    else
                        setting_palette = 8;
            }
            else
                setting_palette = 8;

            // resolution
            if (resolution != "")
                setting_resolution = resolution;
            else
                setting_resolution = "";


            // mouse
            if (draw_mouse.Length == 0)
                Draw_mouse = 1;
            else
            {
                if (draw_mouse == "true")
                    Draw_mouse = 1;
                else
                    Draw_mouse = 0;
            }

            if (mouse_move == "true")
                setting_mouse_move = true;
            else
                if (mouse_move == "false")
                    setting_mouse_move = false;
                else
                    setting_mouse_move = true;

            if (user_data != "")
                setting_user_data = user_data;
            else
                setting_user_data = "";

            SetParamFromSettings(setting_palette, setting_resolution, Draw_mouse == 1? true:false, setting_mouse_move);

            // виды курсоров
            dict_index_cursor = new Dictionary<int, MouseCursor>();
            dict_index_cursor.Add(Cursors.AppStarting.Handle.ToInt32(), MouseCursor.AppStarting);
            dict_index_cursor.Add(Cursors.Arrow.Handle.ToInt32(), MouseCursor.Arrow);
            dict_index_cursor.Add(Cursors.Cross.Handle.ToInt32(), MouseCursor.Cross);
            dict_index_cursor.Add(Cursors.Hand.Handle.ToInt32(), MouseCursor.Hand);
            dict_index_cursor.Add(Cursors.Help.Handle.ToInt32(), MouseCursor.Help);
            dict_index_cursor.Add(Cursors.HSplit.Handle.ToInt32(), MouseCursor.HSplit);
            dict_index_cursor.Add(Cursors.IBeam.Handle.ToInt32(), MouseCursor.IBeam);
            dict_index_cursor.Add(Cursors.No.Handle.ToInt32(), MouseCursor.No);
            dict_index_cursor.Add(Cursors.NoMove2D.Handle.ToInt32(), MouseCursor.NoMove2D);
            dict_index_cursor.Add(Cursors.NoMoveHoriz.Handle.ToInt32(), MouseCursor.NoMoveHoriz);
            dict_index_cursor.Add(Cursors.NoMoveVert.Handle.ToInt32(), MouseCursor.NoMoveVert);
            dict_index_cursor.Add(Cursors.PanEast.Handle.ToInt32(), MouseCursor.PanEast);
            dict_index_cursor.Add(Cursors.PanNE.Handle.ToInt32(), MouseCursor.PanNE);
            dict_index_cursor.Add(Cursors.PanNorth.Handle.ToInt32(), MouseCursor.PanNorth);
            dict_index_cursor.Add(Cursors.PanNW.Handle.ToInt32(), MouseCursor.PanNW);
            dict_index_cursor.Add(Cursors.PanSE.Handle.ToInt32(), MouseCursor.PanSE);
            dict_index_cursor.Add(Cursors.PanSouth.Handle.ToInt32(), MouseCursor.PanSouth);
            dict_index_cursor.Add(Cursors.PanSW.Handle.ToInt32(), MouseCursor.PanSW);
            dict_index_cursor.Add(Cursors.PanWest.Handle.ToInt32(), MouseCursor.PanWest);
            dict_index_cursor.Add(Cursors.SizeAll.Handle.ToInt32(), MouseCursor.SizeAll);
            dict_index_cursor.Add(Cursors.SizeNESW.Handle.ToInt32(), MouseCursor.SizeNESW);
            dict_index_cursor.Add(Cursors.SizeNS.Handle.ToInt32(), MouseCursor.SizeNS);
            dict_index_cursor.Add(Cursors.SizeNWSE.Handle.ToInt32(), MouseCursor.SizeNWSE);
            dict_index_cursor.Add(Cursors.SizeWE.Handle.ToInt32(), MouseCursor.SizeWE);
            dict_index_cursor.Add(Cursors.UpArrow.Handle.ToInt32(), MouseCursor.UpArrow);
            dict_index_cursor.Add(Cursors.VSplit.Handle.ToInt32(), MouseCursor.VSplit);
            dict_index_cursor.Add(Cursors.WaitCursor.Handle.ToInt32(), MouseCursor.WaitCursor);

            if (ip != "" && port != "")
                ConnectToIP(ip, port);

            InitFileTransferData();

            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            ClipboardMonitor.Start();
        }

        public String GetAddressParam()
        {
            WebRequest wrGETURL = null;
            Stream objStream = null;
            String str = "";
            try
            {
                string sURL;
                //sURL = @"http://www.consult-inform.com/rdp";
                sURL = setting_url;

                wrGETURL = WebRequest.Create(sURL);
                objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);
                str = objReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                //...
            }
            finally
            {
                if (objStream != null)
                {
                    objStream.Close();
                    objStream.Dispose();
                }
            }
            return str;
        }

        public void StopView(NetworkStream client_stream)
        {
            byte[] Buf_Head = null;
            int len = ReadData(client_stream, ref Buf_Head);

            if (setting_draw_mouse == 0 || Client_type == ClientType.Control)
                StartMouseHook(false);

            InitFileTransferData();

            StopReceiveImageData(false);
            connect_to_client = false;
        }

        public void StopReceiveImageData(Boolean SendMessage)
        {                               
            UploadImageData(false);
            StartTimerPing(false);
            StartTimerEcho(false);
            send_echo = false;

            while (timer1.Enabled == true);

            if (SendMessage)
            {
                if (client != null)
                {
                    if (client.Connected)
                    {
                        NetworkStream client_stream = client.GetStream();
                        Byte[] byte_to_send = GetSimpleData(Command.set_stop, 0);
                        while (client_stream.CanWrite == false);
                        client_stream.Write(byte_to_send, 0, byte_to_send.Length);
                    }
                }
            }
            else
                OpenViewForm(false);
            
            ButtonState(true);

            if (ActiveScreen != null)
            {
                lock (critical_sektion)
                {
                    ActiveScreen.Dispose();
                    ActiveScreen = null;
                    image_update = true;
                }
            }

            if (active_screen != null)
            {
                lock (critical_sektion)
                {
                    active_screen.Dispose();
                    active_screen = null;
                }
            }            
        }

        public void MouseAndKeyboardDataUpdate(Byte[] Data, Boolean fast_send = false)
        {
            if (client != null)
            {
                if (client.Connected)
                {                    
                    NetworkStream client_stream = client.GetStream();                    
                    Byte[] byte_to_send = GetSimpleData(Command.set_mouse, Data);
                    // отправляем данные мыши
                    while (client_stream.CanWrite == false);
                    if (fast_send)
                    {
                        client.NoDelay = true;
                        client_stream.Write(byte_to_send, 0, byte_to_send.Length);
                        client.NoDelay = false;
                    }
                    else
                        client_stream.Write(byte_to_send, 0, byte_to_send.Length);
                }
            }
        }

        public void FileDataUpload(Byte[] Data)
        {
            if (client != null)
            {
                if (client.Connected)
                {
                    NetworkStream client_stream = client.GetStream();
                    while (client_stream.CanWrite == false) ;
                    Byte[] byte_to_send = GetSimpleData(Command.file_command, Data);
                    // отправляем данные
                    client_stream.Write(byte_to_send, 0, byte_to_send.Length);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!(client != null && client.Client != null && client.Connected && client.Client.Connected != null))
            {
                MessageBox.Show(@"Не установлено соединение с сервером", "Соединение",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Warning);
                return;
            }
            int local_id = 0;
            try
            {
                local_id = Convert.ToInt32(textBox4.Text);
                if (textBox3.Text == textBox4.Text)

                    throw new Exception("Введите другой номер");
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не верный формат номера клиента", "Соединение",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Warning);
                local_id = 0;
            }
            if (local_id != 0)
            {
                InitFileTransferData();

                DefineScale();
                NetworkStream client_stream = client.GetStream();
                ConnectToID(Convert.ToInt32(textBox4.Text), client_stream);
                Client_type = ClientType.Control;
            }
        }

        void InitFileTransferData()
        {
            file_transfer_download = new FileTransferJobManagerDownload(this);
            file_transfer_upload = new FileTransferJobManager(this);
        }

        void DefineBytePerPixel()
        {
            byte_per_pixel = 1;
            switch (setting_palette)
            {
                case 8:
                    byte_per_pixel = 1;
                    break;
                case 16:
                    byte_per_pixel = 2;
                    break;
            }
        }

        void DefineScale()
        {
            scale_width_send = 0;
            scale_height_send = 0;

            if (setting_resolution != "")
            {
                try
                {
                    string val = setting_resolution;
                    string[] mas = val.Split('x');
                    if (mas.Length != 2)
                        mas = val.Split('х');

                    if (mas.Length == 2)
                    {
                        scale_width_send = Convert.ToInt32(mas[0]);
                        scale_height_send = Convert.ToInt32(mas[1]);
                        if (scale_width_send < 1024)
                            scale_width_send = 1024;
                        if (scale_height_send < 768)
                            scale_height_send = 768;
                    }
                }
                catch
                {
                    scale_width_send = 0;
                    scale_height_send = 0;
                }
            }
        }

        void ProcessGetSize(NetworkStream send)
        {
            byte[] Buf_Head = null;
            int len = ReadData(send, ref Buf_Head);

            scale_width = BitConverter.ToInt32(Buf_Head, 0);
            scale_height = BitConverter.ToInt32(Buf_Head, 4);
            setting_draw_mouse = BitConverter.ToInt32(Buf_Head, 8);

            if (scale_width != 0 && scale_height != 0 && scale_width < Screen.PrimaryScreen.Bounds.Width && scale_height < Screen.PrimaryScreen.Bounds.Height)
            {
                scale_x = 1.0f * (Screen.PrimaryScreen.Bounds.Width) / (scale_width);
                scale_y = 1.0f * (Screen.PrimaryScreen.Bounds.Height) / (scale_height);
            }
            else
            {
                scale_x = 1.0f;
                scale_y = 1.0f;
            }

            Bitmap image = null;
            CaptureScreen.CaptureScreen.GetDesktopImage(ref image, byte_per_pixel, 0, scale_width, scale_height);

            // доработать !!!!
            if (image.Width % 10 == 0)
                len_x = image.Width / 10;
            else
                if (image.Width % 8 == 0)
                len_x = image.Width / 8;
            else
                    if (image.Width % 4 == 0)
                len_x = image.Width / 4;
            else
                        if (image.Width % 2 == 0)
                len_x = image.Width / 2;
            else
                len_x = image.Width;

            Byte[] byte_to_send = new Byte[24];
            Buffer.BlockCopy(BitConverter.GetBytes(Command.set_size.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(4 * 4), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(image.Width), 0, byte_to_send, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(image.Height), 0, byte_to_send, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(len_x), 0, byte_to_send, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(byte_per_pixel), 0, byte_to_send, 20, 4);

            while (send.CanWrite == false) ;
            send.Write(byte_to_send, 0, byte_to_send.Length);

            // Инициализируем переменные и начинаем передовать данные 
            Buffer_line_index = 0;
            // точное выделение памяти, в этом же буффере передается и заархивироавнные данные, бывает, что архив весит больше чем исходные данные
            // поэтому выделяем больше памяти
            Buffer_line = new Byte[4 * image.Height * (image.Width / len_x) + image.Width * image.Height * byte_per_pixel];
            //Buffer_line = new Byte[4 * image.Height * image.Width + image.Width * image.Height * byte_per_pixel];

            image.Dispose();

            first_image_update = false;
            if (ActiveScreen != null)
            {
                ActiveScreen.Dispose();
                ActiveScreen = null;
            }

            if (active_screen != null)
            {
                active_screen.Dispose();
                active_screen = null;
            }

            // запускаем поток генерации скриншотов
            index_cursor_last = 0;
            hook_mouse_x = 0;
            hook_mouse_y = 0;
            hook_mouse_prev_x = 0;
            hook_mouse_prev_y = 0;

            thread_screen_status = true;
            first_image_update = false;
            thread_screen = new Thread(new ParameterizedThreadStart(ClientThread_GenerateImageData));
            thread_screen.Start();

            UploadImageData(true);

            // отправляем метку echo для расчетам времени отправки и приемки данных    
            send_echo = true;                
            SendEcho();
            StartTimerEcho(true);
            StartTimerPing(true);

            if (setting_draw_mouse == 0)
            {
                mouse_hook = new MouseKeyboardLibrary.MouseHook();
                StartMouseHook(true);
            }

        }

        void SendEcho()
        {
            if (send_echo)
            {
                NetworkStream send = client.GetStream();
                if (client.Client.Connected != false)
                {
                    send_echo = false;
                    datetime_send_echo = DateTime.Now;
                    byte[] data = GetSimpleData(Command.echo, 0);
                    SendBuf(data, send);
                }
            }
        }

        void SendPing()
        {
            NetworkStream send = client.GetStream();
            if (client.Client.Connected != false)
            {
                byte[] data = GetSimpleData(Command.ping, 0);
                SendBuf(data, send);
            }
        }

        void Send_GetImage(NetworkStream client_stream)
        {
            // отправляем запрос на получение картинки
            /*while (client_stream.CanWrite == false) ;
            byte[] byte_to_send = GetSimpleData(Command.get_image, 0);
            client_stream.Write(byte_to_send, 0, byte_to_send.Length);    
            */
        }

        void ProcessSetSize(NetworkStream client_stream)
        {
            byte[] Buf_Head = null;
            int len = ReadData(client_stream, ref Buf_Head);

            Size_W = BitConverter.ToInt32(Buf_Head, 0); // W
            Size_H = BitConverter.ToInt32(Buf_Head, 4); // H
            len_x = BitConverter.ToInt32(Buf_Head, 8); // len_x
            byte_per_pixel = BitConverter.ToInt32(Buf_Head, 12); // byte_per_pixel
                       
            // выделяем память
            int w_count = Size_W / len_x;
            image_data_unzip = new Byte[4 * w_count * Size_H + Size_W * Size_H * byte_per_pixel];
            image_data_zip = new Byte[4 * w_count * Size_H + Size_W * Size_H * byte_per_pixel];

            // открываем окно
            OpenViewForm(true);

            // отправляем запрос на получение картинки
            Send_GetImage(client_stream);
        }

        void ConnectToID(int id_client, NetworkStream client_stream)
        {
            connect_to_client = false;
            DefineBytePerPixel();
            Byte[] byte_data = new Byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(id_client), 0, byte_data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(byte_per_pixel), 0, byte_data, 4, 4);

            byte[] byte_to_send = GetSimpleData(Command.get_connect, byte_data);
            SendBuf(byte_to_send, client_stream);           
        }

        void ProcessGetConnect(NetworkStream client_stream)
        {
            byte[] Buf_Head = null;
            int len = ReadData(client_stream, ref Buf_Head);
            int byte_size = 0;

            if (len > 0)
            {
                int answer = BitConverter.ToInt32(Buf_Head, 0);                           
                if ((ResultConnection)answer == ResultConnection.not_found)
                {
                    ShowMessageBox(@"Клиент не найден", "Соединение", MessageBoxIcon.Warning);
                    return;
                }
                else
                    if ((ResultConnection)answer == ResultConnection.ok)
                        byte_size = BitConverter.ToInt32(Buf_Head, 8);     
                 
            }

            var result = ShowMessageBoxYesNo(@"Разрешить входящее соединение", "Соединение");
            
            byte[] byte_to_send = null;
            if (result != DialogResult.OK)
            {               
                byte_to_send = GetSimpleData(Command.set_connect, ResultConnection.negative);
                SendBuf(byte_to_send, client_stream);
            }
            else
            {
                connect_to_client = true;
                CloseForm = false;
                MinimizeWindow();
                byte_to_send = GetSimpleData(Command.set_connect, ResultConnection.ok);
                SendBuf(byte_to_send, client_stream);
                // параметр качества - запрос от клиента
                byte_per_pixel = byte_size;
                Client_type = ClientType.View;
                // блокируем клавиши                       
            }

        }

        public DialogResult ShowMessageBoxYesNo(String text1, String text2)
        {
            if (this.InvokeRequired)
            {
                ShowMessageBoxYesNoDelegate d = new ShowMessageBoxYesNoDelegate(ShowMessageBoxYesNo);
                return (DialogResult)this.Invoke(d, new object[] { text1, text2 });
            }
            else
            {
                DialogResult res = MessageBox.Show(text1, text2, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                return res;
            }
        }

        public void ShowMessageBox(String text1, String text2, MessageBoxIcon mbi)
        {
            if (this.InvokeRequired)
            {
                ShowMessageBoxDelegate d = new ShowMessageBoxDelegate(ShowMessageBox);
                this.Invoke(d, new object[] { text1, text2, mbi});
            }
            else
            {
                MessageBox.Show(text1, text2, MessageBoxButtons.OK, mbi);                
                
            }
        }

        public void MinimizeWindow()
        {
            if (this.InvokeRequired)
            {
                MinimizeWindowDelegate d = new MinimizeWindowDelegate(MinimizeWindow);
                this.Invoke(d, new object[] {});
            }
            else
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        void ProcessSetConnect(NetworkStream client_stream)
        {
            byte[] Buf_Head = null;
            int len = ReadData(client_stream, ref Buf_Head);

            int answer = BitConverter.ToInt32(Buf_Head, 0);
            if ((ResultConnection)answer == ResultConnection.ok)
            {            
                Thread.Sleep(100);
                // запрашиваем параметры картинки                
                byte[] data = new byte[12];
                Buffer.BlockCopy(BitConverter.GetBytes(scale_width_send), 0, data, 0, 4); // width scale
                Buffer.BlockCopy(BitConverter.GetBytes(scale_height_send), 0, data, 4, 4); // height scale
                Buffer.BlockCopy(BitConverter.GetBytes(Draw_mouse), 0, data, 8, 4); // draw mouse                
                Byte[] byte_to_send = GetSimpleData(Command.get_size, data);
                SendBuf(byte_to_send, client_stream);
                Client_type = ClientType.Control;
            }
            else
                ShowMessageBox("Подключение отклонено", "Соединение", MessageBoxIcon.Warning);
        }

        void Process_ImageDataUpdate_get(NetworkStream client_stream)
        {
            //byte[] Buf_Read = null;
            int Buf_len = 0;
            lock (image_data_zip)
            {
                Buf_len = ReadData(client_stream, ref image_data_zip);
            }

            if (Form_View != null)
            {
                lock (Form_View.Data_image_byte)
                {
                    while (client_stream.CanWrite == false) ;
                    Send_GetImage(client_stream);

                    int offset = 0;
                    bool error = false;
                    if (Buf_len != 0 && BitConverter.ToInt32(image_data_zip, 0) != 0)
                    {
                        var ms_in = new MemoryStream(image_data_zip);
                        GZipStream stream = new GZipStream(ms_in, CompressionMode.Decompress, true);
                        try
                        {
                            const int size = 4096;
                            byte[] buffer = new byte[size];
                            int count_read = 0;
                            while (true)
                            {
                                count_read = stream.Read(buffer, 0, size);
                                if (count_read == 0)
                                    break;
                                Buffer.BlockCopy(buffer, 0, image_data_unzip, offset, count_read);
                                offset += count_read;
                            }
                        }
                        catch (Exception ex)
                        {
                            // не понятные ошибки с буфером
                            error = true;
                        }
                        finally
                        {
                            stream.Close();
                            ms_in.Close();
                        }

                        int index_j = len_x * byte_per_pixel;
                        if (!error)
                        {
                            int offset_write = 0;
                            while (true)
                            {
                                if (offset_write == offset)
                                    break;
                                int index = BitConverter.ToInt32(image_data_unzip, offset_write);

                                offset_write = offset_write + 4;
                                Buffer.BlockCopy(image_data_unzip, offset_write, Form_View.Data_image_byte, index, index_j);
                                offset_write = offset_write + index_j;
                            }
                        }
                    }
                    if (Form_View != null)
                    {
                        if (Buf_len != 0 && offset != 0)
                            Form_View.update_image = true;
                        //  else
                        //Thread.Sleep(100);
                        // отправляем запрос на получение картинки
                        /*  while (client_stream.CanWrite == false);
                          byte[] byte_to_send = null;
                          byte_to_send = GetSimpleData(Command.get_image, 0);
                          client_stream.Write(byte_to_send, 0, byte_to_send.Length);   */
                    }
                    /*else
                        if (error)
                        {
                            // отправляем запрос на получение картинки
                            while (client_stream.CanWrite == false) ;
                            byte[] byte_to_send = null;
                            byte_to_send = GetSimpleData(Command.get_image, 0);
                            client_stream.Write(byte_to_send, 0, byte_to_send.Length); 
                        }*/
                }
            }
        }

        int Process_Image_Update(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            //image_update = true;

            return 4;
        }

        int Process_Clipboard_Data(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            string text = "";
            try
            {
                text = UTF8Encoding.UTF8.GetString(Buf_Read, 0, Buf_Read.Length);
            }
            catch
            {
                //...
            }

            if (clipboard_string != text)
            {
                clipboard_string = text;

                int count = 0;
                Boolean error = true;
                while (true)
                {
                    count += 1;                    
                    try
                    {
                        Thread thread = new Thread(() => Clipboard.SetText(clipboard_string, TextDataFormat.UnicodeText));
                        thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                        thread.Start();
                        thread.Join();
                        error = false;
                    }
                    catch
                    {
                        error = true;
                    }

                    if (count > 5 || error == false)
                        break;

                    Thread.Sleep(20);
                }
            }

            return len;
        }

        int Process_Mouse(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            Process_Mouse_Action(Buf_Read, ref client_stream);

            return 4 + len;
        }

        int Process_MouseDataUpdate(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);
            return len;
        }


        int Process_File(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            Process_File_Action(Buf_Read, ref client_stream);

            return 4 + len;
        }

        int Process_Echo(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            TimeSpan t = DateTime.Now - datetime_send_echo;

            int val = (int)(t.TotalMilliseconds / 2);
            
            if (val < sleep_param_screen_min)
            {
                sleep_param_screen = sleep_param_screen_min;
            }
            else
            {
                if (val > sleep_param_screen_max)
                    sleep_param_screen = sleep_param_screen_max;
                else
                    sleep_param_screen = val;
            }

            send_echo = true;
            return 4 + len;
        }

        int Process_Ping(NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);

            image_update = true;
            send_echo = true;
            sleep_param_screen = sleep_param_screen_max;
            SendEcho();
            StartTimerEcho(true);

            return 4 + len;
        }

        public bool ReadCommand(NetworkStream client_stream, byte[] Buf_Head)
        {
            int byte_to_read = 4;
            int offset = 0;
            // read data
            while (client_stream.CanRead == false);

            // длина сообщения
            while (true)
            {
                if (!client.Client.Connected)
                    break;
                int count = 0;
                try
                {
                     count = client_stream.Read(Buf_Head, offset, (byte_to_read - offset));
                }
                catch
                {

                }

                if (count == 0)
                {
                    Thread.Sleep(200);
                    break;
                }
                offset = offset + count;

                if (offset == byte_to_read)
                    break;
            }

            if (offset == byte_to_read)
                return true;
            else
                return false;
        }

        void Read_From_Socket()
        {
            byte[] Buf_Head = new byte[4];

            Boolean error = false;
            NetworkStream client_stream = client.GetStream();

            try
            {
                while (true)
                {
                    if (client == null)
                        break;
                    if (client.Client.Connected == false)
                        break;

                    while (client_stream.CanRead == false);

                    if (!ReadCommand(client_stream, Buf_Head))
                        continue;

                    switch (BitConverter.ToInt32(Buf_Head, 0))
                    {
                        case (int)Command.set_id:
                            Process_ID(ref client_stream);
                            break;
                        case (int)Command.get_size:
                            ProcessGetSize(client_stream);
                            break;
                        case (int)Command.set_size:
                            ProcessSetSize(client_stream);
                            break;
                        case (int)Command.get_image:
                            Process_ImageDataUpdate_get(client_stream);
                            //Form_View.fps += 1;
                            break;
                        case (int)Command.get_connect:
                            ProcessGetConnect(client_stream);
                            break;
                        case (int)Command.set_connect:
                            ProcessSetConnect(client_stream);
                            break;
                        case (int)Command.image_update:
                            Process_Image_Update(client_stream);
                            break;
                        case (int)Command.set_mouse:
                            Process_Mouse(client_stream);
                            break;
                        /*case (int)Command.mouse_update:
                            Process_MouseDataUpdate(ref client_stream);
                            break;*/
                        case (int)Command.set_clipboard_data:
                            Process_Clipboard_Data(client_stream);
                            break;
                        case (int)Command.set_stop:
                            StopView(client_stream);
                            break;
                        case (int)Command.file_command:
                            Process_File(client_stream);
                            break;
                        case (int)Command.echo_ok:
                            Process_Echo(client_stream);
                            break;
                        case (int)Command.ping:
                            Process_Ping(client_stream);
                            break;
                        default:
                            break;
                    }
                }
            }

            catch (IOException e)
            {
                // разрыв операции блокирования
                if (!CloseForm)
                    ShowMessageBox("Сервер перестал отвечать", "Соединение", MessageBoxIcon.Warning);
                error = true;
            }
            catch (SocketException e)
            {
                if (!CloseForm)
                    ShowMessageBox("Сервер перестал отвечать", "Соединение", MessageBoxIcon.Warning);
                error = true;
            }
            catch (Exception e)
            {
                if (!CloseForm)
                    ShowMessageBox("Сервер перестал отвечать", "Соединение", MessageBoxIcon.Warning);

                error = true;
            }
            catch
            {
                error = true;
            }

                OpenViewForm(false);
                if (client_stream != null)
                {
                    lock (client_stream)
                    {
                        client_stream.Close();
                    }
                    client_stream = null;
                }
                if (client != null)
                {
                    lock (client)
                    {
                        client.Close();
                        client = null;
                    }
                }

                SetID("");
                ButtonStateConnection(true);
        }

        Boolean SendBuf(Byte[] data, NetworkStream client_stream)
        {
            while (client_stream.CanWrite == false);
            client_stream.Write(data, 0, data.Length);
            return true;
        }

        public Byte[] GetSimpleData(Enum command, int command_result)
        {
            Byte[] byte_to_send = new Byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(command.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(4), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(command_result), 0, byte_to_send, 8, 4);
            return byte_to_send;
        }

        public Byte[] GetSimpleData(Enum command, Enum command_result)
        {
            Byte[] byte_to_send = new Byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(command.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(4), 0, byte_to_send, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(command_result.GetHashCode()), 0, byte_to_send, 8, 4);
            return byte_to_send;
        }

        public Byte[] GetSimpleData(Enum command, Byte[] Data, int offset = 0)
        {
            if (Data == null)
            {
                Byte[] byte_to_send = new Byte[8];
                Buffer.BlockCopy(BitConverter.GetBytes(command.GetHashCode()), 0, byte_to_send, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(0), 0, byte_to_send, 4, 4);
                return byte_to_send;
            }
            else
            {
                Byte[] byte_to_send = null;
                if (offset == 0)
                    byte_to_send = new Byte[Data.Length + 8];
                else
                    byte_to_send = new Byte[offset + 8];

                Buffer.BlockCopy(BitConverter.GetBytes(command.GetHashCode()), 0, byte_to_send, 0, 4);
                if (offset == 0)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(Data.Length), 0, byte_to_send, 4, 4);
                    Buffer.BlockCopy(Data, 0, byte_to_send, 8, Data.Length);
                }
                else
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(offset), 0, byte_to_send, 4, 4);
                    Buffer.BlockCopy(Data, 0, byte_to_send, 8, offset);
                }
                return byte_to_send;
            }
        }
      
        void Process_ID(ref NetworkStream client_stream)
        {
            byte[] Buf_Read = null;
            int len = ReadData(client_stream, ref Buf_Read);
            ID = BitConverter.ToInt32(Buf_Read, 0);
            if (ID == 0)
            {
                SetID("---");
                ShowMessageBox(@"На сервере нет свободного номера", "Соединение", MessageBoxIcon.Warning);
            }
            else
                SetID(ID.ToString());
        }

        private void UploadImageData(Boolean state)
        {
            if (this.InvokeRequired)
            {
                TimerSendImage d = new TimerSendImage(UploadImageData);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                timer1.Enabled = state;
            }
        }

        private void StartTimerPing(Boolean state)
        {
            if (this.InvokeRequired)
            {
                TimerSendPing d = new TimerSendPing(StartTimerPing);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                timer2.Enabled = state;
            }
        }

        private void StartTimerEcho(Boolean state)
        {
            if (this.InvokeRequired)
            {
                TimerSendEcho d = new TimerSendEcho(StartTimerEcho);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                timer3.Enabled = state;
            }
        }
        private void ButtonState(Boolean state)
        {
            if (this.InvokeRequired)
            {
                SetButtonState d = new SetButtonState(ButtonState);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                button2.Enabled = state;
            }
        }

        private void ButtonStateConnection(Boolean state)
        {
            if (this.InvokeRequired)
            {
                SetButtonState d = new SetButtonState(ButtonStateConnection);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                button3.Enabled = state;
            }
        }

        private void OpenViewForm(Boolean state)
        {
            if (this.InvokeRequired)
            {
                FormView d = new FormView(OpenViewForm);
                try
                {
                    this.Invoke(d, new object[] { state });
                }
                catch
                {
                    //...
                }
            }
            else
            {
                if (state == true)
                {
                    connect_to_client = true;
                    button2.Enabled = false;
                    Form_View = new Form2();
                    Form_View.Owner = this;
                    Form_View.SetParam(Size_W, Size_H, len_x, byte_per_pixel, setting_mouse_move);
                    if (Size_W < Screen.PrimaryScreen.Bounds.Width-20 && Size_H < Screen.PrimaryScreen.Bounds.Height-20)
                        Form_View.SetPicrtureSize(new Size(Size_W, Size_H));
                    else
                    {
                        Size s = Form_View.Size;
                        //s.Width = Size_W;
                        //s.Height = Size_H;
                        s.Height = (Screen.PrimaryScreen.Bounds.Height - 100 + 60);
                        s.Width = (Screen.PrimaryScreen.Bounds.Width - 100);

                        Form_View.Size = s;
                    }
                    Form_View.FormClosed += ChildFormClosed;
                    // отображаем окно                    
                    Form_View.UpdatePictureBoxParam();
                    Form_View.Show();
                }
                else
                {
                    connect_to_client = false;
                    lock (critical_sektion)
                    {
                        thread_screen_status = false;
                    }
                    Thread.Sleep(sleep_param_screen + 50);
                    if (thread_screen != null && thread_screen.ThreadState != ThreadState.Aborted)
                    {
                        thread_screen.Abort();                        
                    }
                    thread_screen = null;

                    if (Form_View != null)
                    {
                        lock (Form_View)
                        {
                            Form_View.Close();
                            Form_View = null;
                        }
                    }
                    if (!CloseForm)
                    {
                        CaptureScreen.CaptureScreen.StopScreen();

                        if (!CloseForm_disconnect)
                        {
                            if (this.WindowState == FormWindowState.Minimized)
                                this.WindowState = FormWindowState.Normal;
                            this.Activate();
                            ShowMessageBox("Сеанс окончен", "Соединение", MessageBoxIcon.Information);
                        }
                        CloseForm = true;

                        if (CloseForm_disconnect)
                        {
                            CloseActiveForm();
                            Close();
                        }
                    }
                }
                
            }
        }

        void ChildFormClosed(object sender, FormClosedEventArgs args)
        {
            if (Form_View != null)
            {
                lock (Form_View)
                {
                    Form_View.FormClosed -= ChildFormClosed;
                    Form_View.Dispose();
                    Form_View = null;
                }
            }
            StopReceiveImageData(true);
            scale_width_send = 0;
            scale_height_send = 0;
            scale_x = 1.0f;
            scale_y = 1.0f;
            connect_to_client = false;
            // освобождаем память
            image_data_unzip = null;
        }

        private void SetID(string text)
        {
            if (this.textBox3.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetID);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox3.Text = text;
            }
        }
        
        void Process_Mouse_Action(byte[] data, ref NetworkStream client_stream)
        {
            int x = -1;
            int y = -1;
            int button = -1;
            bool ext = false;
            switch (BitConverter.ToInt32(data, 0))
            {
                case (int)MouseCommand.move:
                    x = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 4) * scale_x, 0));
                    y = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 8) * scale_y, 0));
                    
                    MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                    
                    hook_mouse_x = x;
                    hook_mouse_y = y;
                    manage_mouse_data_update = true;

                    break;

                case (int)MouseCommand.move_client:
                    x = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 4), 0));
                    y = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 8), 0));
                    int index = BitConverter.ToInt32(data, 12);

                    if (data.Length == 16)
                    {
                        if (Form_View != null)
                        {
                            Form_View.cursor_index = index;
                            Form_View.cursor_move_x = x;
                            Form_View.cursor_move_y = y;
                        }
                    }

                    break;

                case (int)MouseCommand.click:
                    x = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 4) * scale_x, 0));
                    y = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 8) * scale_y, 0));
                    
                    hook_mouse_x = x;
                    hook_mouse_y = y;
                    manage_mouse_data_update = true;

                    button = BitConverter.ToInt32(data, 12);

                    switch (button)
                    {
                        case (int)MouseButtons.Left:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.Click(MouseKeyboardLibrary.MouseButton.Left);
                            break;
                        case (int)MouseButtons.Right:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.Click(MouseKeyboardLibrary.MouseButton.Right);
                            break;
                        case (int)MouseButtons.Middle:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.Click(MouseKeyboardLibrary.MouseButton.Middle);
                            break;
                    }
                    break;
                case (int)MouseCommand.mouse_down:
                    
                    x = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 4) * scale_x, 0));
                    y = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 8) * scale_y, 0));
                    
                    hook_mouse_x = x;
                    hook_mouse_y = y;
                    manage_mouse_data_update = true;

                    button = BitConverter.ToInt32(data, 12);

                    switch (button)
                    {
                        case (int)MouseButtons.Left:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseDown(MouseKeyboardLibrary.MouseButton.Left);
                            break;
                        case (int)MouseButtons.Right:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseDown(MouseKeyboardLibrary.MouseButton.Right);
                            break;
                        case (int)MouseButtons.Middle:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseDown(MouseKeyboardLibrary.MouseButton.Middle);
                            break;
                    }
                    break;
                case (int)MouseCommand.mouse_up:
                    
                    x = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 4) * scale_x, 0));
                    y = (int)(Math.Round(1.0f * BitConverter.ToInt32(data, 8) * scale_y, 0));

                    hook_mouse_x = x;
                    hook_mouse_y = y;
                    manage_mouse_data_update = true;

                    button = BitConverter.ToInt32(data, 12);

                    switch (button)
                    {
                        case (int)MouseButtons.Left:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseUp(MouseKeyboardLibrary.MouseButton.Left);
                            break;
                        case (int)MouseButtons.Right:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseUp(MouseKeyboardLibrary.MouseButton.Right);
                            break;
                        case (int)MouseButtons.Middle:
                            MouseKeyboardLibrary.MouseSimulator.Position = new Point(x, y);
                            MouseKeyboardLibrary.MouseSimulator.MouseUp(MouseKeyboardLibrary.MouseButton.Middle);
                            break;
                    }
                    break;
                
                case (int)MouseCommand.mouse_wheel:
                    
                    int delta = BitConverter.ToInt32(data, 4);
                    MouseKeyboardLibrary.MouseSimulator.MouseWheel(delta);
                    break;

                case (int)MouseCommand.key_down:

                    x = BitConverter.ToInt32(data, 4);
                    y = BitConverter.ToInt32(data, 8);

                   if (((Keys)x == Keys.Home || 
                        (Keys)x == Keys.End || 
                        (Keys)x == Keys.Left || 
                        (Keys)x == Keys.Right ||
                        (Keys)x == Keys.RControlKey ||
                        (Keys)x == Keys.Divide ||
                        (Keys)x == Keys.RMenu ||
                        (Keys)x == Keys.Up ||
                        (Keys)x == Keys.Down || 
                        (Keys)x == Keys.PageUp ||
                        (Keys)x == Keys.Delete ||
                        (Keys)x == Keys.Insert ||
                        (Keys)x == Keys.PageDown))
                    {
                        ext = true;
                    }

                    if ((Keys)y != Keys.None)
                        MouseKeyboardLibrary.KeyboardSimulator.KeyDown((Keys)y);

                    if ((Keys)x != Keys.None)
                        MouseKeyboardLibrary.KeyboardSimulator.KeyDown((Keys)x, ext);

                    break;

                case (int)MouseCommand.key_up:

                    x = BitConverter.ToInt32(data, 4);
                    y = BitConverter.ToInt32(data, 8);

                    if ((Keys)x == Keys.Home ||
                        (Keys)x == Keys.End ||
                        (Keys)x == Keys.Left ||
                        (Keys)x == Keys.Right ||
                        (Keys)x == Keys.Up ||
                        (Keys)x == Keys.RControlKey ||
                        (Keys)x == Keys.Divide ||
                        (Keys)x == Keys.RMenu ||
                        (Keys)x == Keys.Down ||
                        (Keys)x == Keys.PageUp ||
                        (Keys)x == Keys.Delete ||
                        (Keys)x == Keys.Insert ||
                        (Keys)x == Keys.PageDown)
                    {
                        ext = true;
                    }

                    if ((Keys)x != Keys.None)
                        MouseKeyboardLibrary.KeyboardSimulator.KeyUp((Keys)x, ext);

                    if ((Keys)y != Keys.None)
                        MouseKeyboardLibrary.KeyboardSimulator.KeyUp((Keys)y);
                    break;
                case (int)MouseCommand.key_press:
                    //...
                    break;
                default:
                    break;
            }

        }

        void Process_File_Action(byte[] data, ref NetworkStream client_stream)
        {
            int len = 0;
            int key = -1;

            switch (BitConverter.ToInt32(data, 0))
            {
                // запрос на список файлов
                case (int)FileCommand.get_dir:

                    len = BitConverter.ToInt32(data, 4);
                    string path = "";
                    if (len > 0)
                        path = UTF8Encoding.UTF8.GetString(data, 8, len);

                    // директории
                    List<ElementFile> elements = FileTransfer.ListDir(path);

                        // серbализация
                        byte[] byte_elements = FileTransfer.SerializeObject(elements);
                        elements.Clear();
                        // отправляем данные
                        Byte[] byte_to_send = new Byte[8 + byte_elements.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.set_dir.GetHashCode()), 0, byte_to_send, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(byte_elements.Length), 0, byte_to_send, 4, 4);
                        Buffer.BlockCopy(byte_elements, 0, byte_to_send, 8, byte_elements.Length);

                        FileDataUpload(byte_to_send);

                    break;

                // установка списка файлов
                case (int)FileCommand.set_dir:

                    if (Form_FileTransfer != null)
                    {
                        len = BitConverter.ToInt32(data, 4);

                        if (len > 0)
                        {
                            lock (data)
                            {
                                // десериализация
                                List<ElementFile> elements_out = FileTransfer.DeserializeList(data, 8, len);
                                Form_FileTransfer.SetViewList(elements_out);
                            }
                        }
                    }

                    break;

                // Начало передачи файла
                case (int)FileCommand.copy_begin:

                    len = BitConverter.ToInt32(data, 4);

                    if (len > 0)
                    {
                        lock (data)
                        {
                            // десериализация и создание файла
                            if (file_transfer_download != null)
                            {
                                CopyHeaderBegin header = FileTransfer.DeserializeHeader(data, 8, len);
                                FileTransferJob job = new FileTransferJob(header);
                                file_transfer_download.AddJob(job);
                            }
                        }
                    }
                    break;

                // Окончание передачи данных
                case (int)FileCommand.copy_end:

                    key = BitConverter.ToInt32(data, 4);
                    if (file_transfer_download != null)
                    {
                        file_transfer_download.EndCopy(key);
                        file_transfer_download.SendStopDownload(key);
                    }
                    break;

                // Окончание передачи данных
                case (int)FileCommand.copy_stop:

                    key = BitConverter.ToInt32(data, 4);

                    if (file_transfer_download != null)
                        file_transfer_download.EndCopy(key);

                    if (file_transfer_upload != null)
                        file_transfer_upload.EndCopy(key);

                    if (Form_FileTransfer != null)
                        Form_FileTransfer.RefreshCurrentDir();
                    break;

                // запрос на продолжение передачи файла
                case (int)FileCommand.get_next_block:

                    key = BitConverter.ToInt32(data, 4);
                    if (file_transfer_upload != null)
                        file_transfer_upload.SendNextData(key);
                    break;

                // ответ на продолжение передачи файла
                case (int)FileCommand.set_next_block:

                    key = BitConverter.ToInt32(data, 4);
                    len = BitConverter.ToInt32(data, 8);
                    if (file_transfer_download != null)
                        file_transfer_download.SetBlockData(key, data, 12, len);
                    break;

                // предложение на запрос отправки файла
                case (int)FileCommand.copy_getfile:

                    len = BitConverter.ToInt32(data, 4);

                    // десериализация и создание файла
                    if (file_transfer_upload != null)
                    {
                        CopyHeaderBegin h = FileTransfer.DeserializeHeader(data, 8, len);
                        file_transfer_upload.BeginCopy(h.folder, h.full_filename);
                    }

                    break;

                default:
                    break;
            }

        }
     
        void ClientThread_GenerateImageData(Object StateInfo)
        {
            while (true)
            {
                if (thread_screen_status == false)
                    break;
                lock (critical_sektion)
                {
                    if (ActiveScreen == null)
                    {
                        CaptureScreen.CaptureScreen.GetDesktopImage(ref ActiveScreen, byte_per_pixel, 0, scale_width, scale_height, setting_draw_mouse);
                        Bitmap tb = (Bitmap)ActiveScreen.Clone();
                        bool result = GetDiv3_8bit(ref tb, ref ActiveScreen, true);
                        tb.Dispose();
                    }
                    else
                    {
                        if (first_image_update)
                        {
                            if (active_screen != null)
                            {
                                active_screen.Dispose();
                                active_screen = null;
                            }

                            CaptureScreen.CaptureScreen.GetDesktopImage(ref active_screen, byte_per_pixel, 0, scale_width, scale_height, setting_draw_mouse);
                            bool result = GetDiv3_8bit(ref ActiveScreen, ref active_screen);
                        }
                    }
                }
                Thread.Sleep(sleep_param_screen);                
            }
        }

        void UploadImage()
        {
            lock (critical_sektion)
            {
                if (Buffer_line_index > 0)
                {
                    SendImage();
                    if (ActiveScreen != null && active_screen != null)
                    {
                        Rectangle r = new Rectangle(0, 0, active_screen.Width, active_screen.Height);
                        BitmapData bmpd1 = ActiveScreen.LockBits(r, ImageLockMode.WriteOnly, ActiveScreen.PixelFormat);
                        BitmapData bmpd2 = active_screen.LockBits(r, ImageLockMode.ReadOnly, active_screen.PixelFormat);

                        if (bmpd1.Stride > 0 && bmpd2.Stride > 0)
                        {
                            IntPtr pointer = new IntPtr(bmpd1.Scan0.ToInt32());
                            IntPtr pointer2 = new IntPtr(bmpd2.Scan0.ToInt32());
                            memcpy(pointer, pointer2, r.Height * r.Width * byte_per_pixel);
                        }
                        else
                            if (bmpd1.Stride < 0 && bmpd2.Stride < 0)
                            {
                                IntPtr pointer = new IntPtr(bmpd1.Scan0.ToInt32() + (bmpd1.Stride) * (r.Height - 1));
                                IntPtr pointer2 = new IntPtr(bmpd2.Scan0.ToInt32() + (bmpd2.Stride) * (r.Height - 1));
                                memcpy(pointer, pointer2, r.Height * r.Width * byte_per_pixel);
                            }
                            else
                                for (int i = 0; i < r.Height; i++)
                                {
                                    IntPtr pointer = new IntPtr(bmpd1.Scan0.ToInt32() + (bmpd1.Stride * (i)));
                                    IntPtr pointer2 = new IntPtr(bmpd2.Scan0.ToInt32() + (bmpd2.Stride * (i)));
                                    memcpy(pointer, pointer2, r.Width * byte_per_pixel);
                                }

                        ActiveScreen.UnlockBits(bmpd1);
                        active_screen.UnlockBits(bmpd2);
                        
                    }
                    if (first_image_update == false)
                        first_image_update = true;
                }
                else
                    image_update = true;
            }
        }
     
        bool GetDiv3_8bit(ref Bitmap Bmp1, ref Bitmap Bmp2, bool all = false)
        {
            Rectangle r = new Rectangle(0, 0, Bmp1.Width, Bmp1.Height);
            BitmapData bmpd1 = Bmp1.LockBits(r, ImageLockMode.ReadOnly, Bmp1.PixelFormat);
            BitmapData bmpd2 = Bmp2.LockBits(r, ImageLockMode.ReadOnly, Bmp2.PixelFormat);
            int Length = 0;

            int offset_buf = 0;

            for (int i = 0; i < r.Height; i++)
            {
                int index_j = len_x * byte_per_pixel; // RGB
                int j = 0;
                for (int ii = 0; ii < r.Width / len_x; ii++)
                {
                    IntPtr pointer = new IntPtr(bmpd1.Scan0.ToInt32() + (bmpd1.Stride * (i) + j));
                    IntPtr pointer2 = new IntPtr(bmpd2.Scan0.ToInt32() + (bmpd2.Stride * (i) + j));

                    if (all || memcmp(pointer, pointer2, index_j) != 0)
                    {
                        Int32 index = i * r.Width * byte_per_pixel + ii * index_j;
                        Buffer.BlockCopy(BitConverter.GetBytes(index), 0, Buffer_line, offset_buf, 4); // y
                        offset_buf = offset_buf + 4;
                        System.Runtime.InteropServices.Marshal.Copy(pointer2, Buffer_line, offset_buf, index_j);                        
                        
                        offset_buf = offset_buf + index_j;
                        Length = Length + index_j + 4;
                    }
                    j = j + index_j;
                }
            }

            Bmp1.UnlockBits(bmpd1);
            Bmp2.UnlockBits(bmpd2);

            Buffer_line_index = 0;

            if (Length > 0)
            {             
                // сжимаем данные                
                var stream_out = new MemoryStream();
                GZipStream gz = new GZipStream(stream_out, CompressionMode.Compress, true);
                gz.Write(Buffer_line, 0, Length);
                gz.Close();

                stream_out.Position = 0;

                const int size = 4096;
                byte[] buffer = new byte[size];
                
                while (true)
                {
                    int count_read = stream_out.Read(buffer, 0, size);
                    if (count_read == 0)
                        break;
                    Buffer.BlockCopy(buffer, 0, Buffer_line, Buffer_line_index, count_read);
                    Buffer_line_index += count_read;
                }

                stream_out.Close();
                return true;
            }
            else
                return false;
        }

        void SendImage()
        {            
            if (Buffer_line_index != 0)
            {

                if (client == null || !client.Connected)
                {
                    client = new TcpClient();
                    client.Connect(textBox1.Text, Convert.ToInt32(textBox2.Text));
                }

                client.SendBufferSize = 50 * 1024;
                client.ReceiveBufferSize = 50 * 1024;

                NetworkStream send = client.GetStream();
                // отправляем размер области в первое соединение
                Byte[] byte_to_send = GetSimpleData(Command.set_image, Buffer_line, Buffer_line_index);            
                while (send.CanWrite == false);
                send.Write(byte_to_send, 0, byte_to_send.Length);
                Buffer_line_index = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ConnectToIP(textBox1.Text, textBox2.Text);
        }

        public Boolean ConnectToIP(String ip, String port, int Sec = 5, Boolean ShowError = true)
        {
            client = new TcpClient();

            IAsyncResult ar = client.BeginConnect(ip, Convert.ToInt32(port), null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(Sec), false))
                {
                    client.Close();
                    throw new TimeoutException();
                }

                client.EndConnect(ar);
            }
            catch (TimeoutException ex)
            {
                if (ShowError)
                    MessageBox.Show("Сервер не найден", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                if (ShowError)
                    MessageBox.Show(ex.Message, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException ex)
            {
                if (ShowError)
                    MessageBox.Show(ex.Message, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                wh.Close();
            }

            if (client.Client != null && client.Connected)
            {
                // изменяем форму
                panel2.Visible = false;
                UpdateFormVisible();

                button3.Enabled = false;

                thread_base = new Thread(Read_From_Socket);
                thread_base.Start();

                NetworkStream send = client.GetStream();            

                Byte[] user_data = Encoding.UTF8.GetBytes(setting_user_data);
                Byte[] byte_to_send = new Byte[4 + 4 + user_data.Length];
                Buffer.BlockCopy(BitConverter.GetBytes(Command.get_id.GetHashCode()), 0, byte_to_send, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(user_data.Length), 0, byte_to_send, 4, 4);

                if (user_data.Length > 0)
                    Buffer.BlockCopy(user_data, 0, byte_to_send, 8, user_data.Length);

                while (send.CanWrite == false) ;
                send.Write(byte_to_send, 0, byte_to_send.Length);
                return true;
            }
            else
            {
                if (ShowError)
                    MessageBox.Show("Ошибка подключения", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
          
        }
        
        public int ReadData(NetworkStream client_stream, ref byte[] Data)
        {
            int byte_to_read = 4;
            byte[] Buf_Head = new byte[byte_to_read];
            int offset = 0;
            // read data
            while (client_stream.CanRead == false) ;

            // длина сообщения
            while (true)
            {
                if (!client.Connected)
                    break;
                int count = client_stream.Read(Buf_Head, offset, (byte_to_read - offset));
                offset = offset + count;

                if (offset == byte_to_read)
                    break;
            }

            if (!client.Connected)
                return 0;

            // читаем остальное
            int len = BitConverter.ToInt32(Buf_Head, 0);
            
            read_data_kb += Math.Round(len / 1000.0, 2);

            if (len != 0)
            {
                if (Data == null)
                    Data = new byte[len];

                while (client_stream.CanRead == false) ;

                byte_to_read = len;
                offset = 0;
                while (true)
                {
                    if (!client.Connected)
                        break;
                    int count = client_stream.Read(Data, offset, (byte_to_read - offset));
                    offset = offset + count;

                    if (offset == byte_to_read)
                        break;
                }
            }
            else
            {
                Data = null;
            }
            return len;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Form_View != null)
                if (MessageBox.Show(@"Есть активные сессии, закрыть?", @"Закрытие программы", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                {
                    ClipboardMonitor.Stop();
                    if (setting_draw_mouse == 0)
                        StartMouseHook(false);
                    CloseActiveForm();
                }
                else
                    e.Cancel = true;
            else
            {
                ClipboardMonitor.Stop();
                if (setting_draw_mouse == 0)
                    StartMouseHook(false);
                CloseActiveForm();
            }
        }

        private void CloseActiveForm()
        {
            if (client != null && client.Client != null && client.Connected)
            {
                CloseForm = true;

                lock (critical_sektion)
                {
                    thread_screen_status = false;
                }
                Thread.Sleep(sleep_param_screen + 50);
                if (thread_screen != null && thread_screen.ThreadState != ThreadState.Aborted)
                {
                    thread_screen.Abort();                    
                }
                thread_screen = null;
                
                StopReceiveImageData(true);
                Thread.Sleep(100);
                if (client != null)
                {
                    if (client.Client.Connected)
                    {
                        client.Client.Disconnect(false);
                        Thread.Sleep(100);
                        client.Client.Close();
                    }
                    client.Close();
                    client = null;
                    Thread.Sleep(100);
                }

                if (thread_base!= null && thread_base.ThreadState != ThreadState.Aborted)
                {
                    thread_base.Abort();
                    Thread.Sleep(50);                    
                }
                thread_base = null;

                if (Form_View != null)
                {
                    lock (Form_View)
                    {
                        Form_View.Close();                        
                    }
                    Form_View = null;
                }
            }
            CaptureScreen.CaptureScreen.StopScreen();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            bool update = false;
            lock (critical_sektion)
            {
                if (image_update || send_echo)
                {
                    image_update = false;
                    update = true;
                }
            }

            if (update)
                UploadImage();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {            
            if (!(client != null && client.Client != null && client.Connected == true))
            {
                String list_server = GetAddressParam();
                if (list_server.Length > 0)
                {
                    String[] list = list_server.Split(';');
                    foreach (String str in list)
                    {
                        if (str.Length > 0)
                        {
                            String[] s = str.Split(' ');
                            String l_port = textBox2.Text;
                            if (s.Length > 1)
                                l_port = s[1].Split('=')[1];

                            if (ConnectToIP(s[0].Split('=')[1], l_port, 2, false))
                            {
                                textBox1.Text = s[0].Split('=')[1];
                                textBox2.Text = l_port;
                                break;
                            }
                        }
                    }
                }
            }
            
            this.ActiveControl = textBox4;
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == '\r')
                ActiveControl = button2;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            panel2.Visible = !panel2.Visible;
            UpdateFormVisible();
        }

        private void UpdateFormVisible()
        {
            panel1.Top = 0;
            if (panel2.Visible)
            {
                panel1.Top = panel2.Height;
                this.Height += panel2.Height;
            }
            else
            {                
                int CaptionHeight = SystemInformation.FixedFrameBorderSize.Height + this.Icon.Size.Height;
                if (button2.Enabled)
                    this.Height = state_Height - panel2.Height;
                else
                    this.Height = state_Height - (panel2.Height + button2.Height + 1);
            }
        }
        private void ClientOnly()
        {
            pictureBox1.Visible = false;
            panel2.Visible = false;
            UpdateFormVisible();

            button2.Visible = false;
            linkLabel1.Visible = false;
            textBox4.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Разработчик: © Жаркой Г.С., 2015 г. \n e-mail:gsharkoj@gmail.com", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form3 form = new Form3(this);
            form.SetParam(setting_palette, setting_resolution, Draw_mouse, setting_mouse_move);
            form.Show();
        }

        // echo control
        private void timer2_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - datetime_send_echo;           
            // интервал замера скорости и 3 сек. простоя
            if (t.TotalMilliseconds > timer3.Interval + 3000)
            {
                StartTimerEcho(false);
                image_update = false;
                send_echo = false;
                sleep_param_screen = sleep_param_screen_max;
                SendPing();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
          SendEcho();
        }

        public void SetParamFromSettings(Int32 Palette, String Resolution, Boolean ShowCursor, Boolean SendMouseCursor)
        {
            setting_palette = Palette;
            setting_resolution = Resolution;
            
            if (ShowCursor)
                Draw_mouse = 1;
            else
                Draw_mouse = 0;
            

            string text_palette = "";
            switch (setting_palette)
            {
                case 8:
                    text_palette = "нормальный";
                    break;
                case 16:
                    text_palette = "отличный";
                    break;
            }

            string text_resolution = "";
            if (setting_resolution == "")
                text_resolution = "исходное";
            else
                text_resolution = setting_resolution;

            setting_mouse_move = SendMouseCursor;

            linkLabel1.Text = "Канал: " + text_palette + ", разрешение: " + text_resolution;
        }

        private void timer_mouse_Tick(object sender, EventArgs e)
        {
            Boolean update_mouse = false;
            Boolean update_cursor = false;

            if (hook_mouse_x != hook_mouse_prev_x || hook_mouse_y != hook_mouse_prev_y)
            {
                update_mouse = true;
                hook_mouse_prev_x = hook_mouse_x;
                hook_mouse_prev_y = hook_mouse_y;               
            }

            pci_cursor.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PlatformInvokeUSER32.CURSORINFO));
            int index_cursor = 0;
            if (PlatformInvokeUSER32.GetCursorInfo(out pci_cursor))
            {
                int val = pci_cursor.hCursor.ToInt32();
                if (dict_index_cursor.ContainsKey(val))
                    index_cursor = (Int32)(dict_index_cursor[val]);                
            }

            if (index_cursor_last != index_cursor)
            {
                index_cursor_last = index_cursor;
                update_cursor = true;
            }

            if (update_mouse || update_cursor)
            {                 
                Byte[] byte_to_send = new Byte[16];
                Buffer.BlockCopy(BitConverter.GetBytes(MouseCommand.move_client.GetHashCode()), 0, byte_to_send, 0, 4);
                if (!manage_mouse_data_update)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(hook_mouse_x), 0, byte_to_send, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(hook_mouse_y), 0, byte_to_send, 8, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(index_cursor), 0, byte_to_send, 12, 4);
                    MouseAndKeyboardDataUpdate(byte_to_send);
                }
                else
                {
                    if (update_cursor)
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, byte_to_send, 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, byte_to_send, 8, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(index_cursor), 0, byte_to_send, 12, 4);
                        MouseAndKeyboardDataUpdate(byte_to_send);
                    }
                }
                manage_mouse_data_update = false;
            }

        }

        void mouseHook_MouseMove(object sender, MouseEventArgs arg)
        {
            hook_mouse_x = arg.X;
            hook_mouse_y = arg.Y;
        }

        private void StartMouseHook(Boolean state)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    SetMouseHook d = new SetMouseHook(StartMouseHook);
                    this.Invoke(d, new object[] { state });
                }
                catch
                {
                    //...
                }
            }
            else
            {
                if (state == true)
                {
                    if (mouse_hook != null)
                    {
                        mouse_hook.Start();
                        event_mouse = new MouseEventHandler(mouseHook_MouseMove);
                        mouse_hook.MouseMove += event_mouse;
                        timer_mouse.Enabled = true;
                        hook_mouse_prev_x = -1;
                        hook_mouse_prev_y = -1;
                        hook_mouse_x = -1;
                        hook_mouse_y = -1;
                    }
                }
                else
                {
                    if (mouse_hook != null)
                    {
                        timer_mouse.Enabled = false;
                        if (mouse_hook != null)
                            mouse_hook.Stop();
                        if (event_mouse != null)
                        {
                            if (mouse_hook != null)
                                mouse_hook.MouseMove -= event_mouse;
                            event_mouse = null;
                        }
                        mouse_hook = null;
                    }
                }

            }
        }

        private void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {

            try
            {
                SendTextToClient((String)data);
            }
            catch
            {
                //....
            }

        }
        private void SendTextToClient(string text)
        {
            if (clipboard_string != text)
            {
                if (client != null)
                {
                    if (client.Connected && connect_to_client)
                    {
                        clipboard_string = text;
                        if (clipboard_string != string.Empty)
                        {
                            Byte[] stext = Encoding.UTF8.GetBytes(text);
                            Byte[] send_data = GetSimpleData(Command.set_clipboard_data, stext);
                            NetworkStream param = client.GetStream();
                            SendBuf(send_data, param);
                        }
                    }
                }
            }
        }
    }
}
