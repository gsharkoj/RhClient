using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RhClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            String address = "";
            String port = "";
            String close = "";
            String resolution = "";
            String palette = "";
            String url = "";
            String draw_mouse = "";
            String mouse_move = "";
            String user_data = "";
            if (args.Length > 0)
            {
                foreach (String str in args)
                {
                    
                    String[] s = str.Split('=');
                    if (s.Length == 2)
                    {
                        if (s[0] == "ip")
                            address = s[1];
                        if (s[0] == "port")
                            port = s[1];
                        if (s[0] == "close" || s[0] == "client")
                            close = s[1];
                        if (s[0] == "resolution")
                            resolution = s[1];
                        if (s[0] == "url")
                            url = s[1];
                        if (s[0] == "mouse")
                            draw_mouse = s[1];
                        if (s[0] == "mouse_move")
                            mouse_move = s[1];
                        if (s[0] == "user_data")
                            user_data = s[1];
                    }
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(address, port, close, palette, resolution, url, draw_mouse, mouse_move, user_data));
        }
    }
}
