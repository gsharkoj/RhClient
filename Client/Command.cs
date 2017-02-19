using System;
using System.Collections.Generic;
using System.Text;

namespace RhClient
{
    public enum Command
    {
        set_size = 100,
        set_image = 101,
        read_image = 102,
        image_update = 103,
        set_mouse = 104,
        mouse_update = 105,
        echo = 106,
        echo_ok = 107,
        get_image = 108,
        get_id = 109,
        set_id = 110,
        get_connect = 111,
        set_connect = 112,
        get_size = 113,
        get_stop = 114,
        set_stop = 115,
        data_fail = 116,
        key_mouse_update = 117,
        set_clipboard_data = 118,
        set_image_size = 119,
        file_command = 120,
        ping = 121,
    }

    public enum FileCommand
    {
        get_dir = 100,
        set_dir = 101,
        get_next_block = 102,
        set_next_block = 103,
        copy_begin = 104,        
        copy_end = 105,
        copy_stop = 106,
        copy_getfile = 107,
    }
    public enum MouseCommand
        {
            move = 100,
            click = 101,
            dclick = 102,
            mouse_down = 103,
            mouse_up = 104,
            key_down = 105,
            key_up = 106,
            key_press = 107,
            mouse_wheel = 108,
            move_client = 109,
        }

        public enum ResultConnection
        {
            ok = 100,
            negative = 101,
            not_found = 102,
        }
        public enum MouseCursor
        {
            AppStarting = 1,
            Arrow = 2,
            Cross = 3,
            Default = 4,
            Hand = 5,
            Help = 6,
            HSplit = 7,
            IBeam = 8,
            No = 9,
            NoMove2D = 10,
            NoMoveHoriz = 11,
            NoMoveVert = 12,
            PanEast = 13,
            PanNE = 14,
            PanNorth = 15,
            PanNW = 16,
            PanSE = 17,
            PanSouth = 18,
            PanSW = 19,
            PanWest = 20,
            SizeAll = 21,
            SizeNESW = 22,
            SizeNS = 23,
            SizeNWSE = 24,
            SizeWE = 25,
            UpArrow = 26,
            VSplit = 27,
            WaitCursor = 28,
        }

        public enum ClientType
        {
            Control = 1,
            View = 2,
            Undefine = 3
        }
}
