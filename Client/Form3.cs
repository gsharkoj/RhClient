using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RhClient
{
    public partial class Form3 : Form
    {
        public Form1 Owner_object;

        public Form3()
        {
            InitializeComponent();
        }

        public Form3(Form1 Form_Object)
        {
            InitializeComponent();
            Owner_object = Form_Object;
        }
        public void SetParam(Int32 palette, String resolution, Int32 Draw_mouse, Boolean Send_mouse_move)
        {
            // palette
            comboBox1.SelectedIndex = 0;            
            switch (palette)
            {
                case 8:
                    comboBox1.SelectedIndex = 0;
                    break;
                case 16:
                    comboBox1.SelectedIndex = 1;
                    break;
            }

            // resolution
            if (resolution == "")
                comboBox2.SelectedIndex = 0;
            else
                comboBox2.Text = resolution; 

            // mouse
            if (Draw_mouse == 1)
                checkBox1.Checked = true;
            else
                checkBox1.Checked = false;

            // mouse
            if (Send_mouse_move)
                checkBox2.Checked = true;
            else
                checkBox2.Checked = false;

        }

        Int32 GetPalleteBits()
        {
            Int32 bits = 8;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    bits = 8;
                    break;
                case 1:
                    bits = 16;
                    break;
            }
            return bits;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Owner_object.SetParamFromSettings(GetPalleteBits(), comboBox2.Text, checkBox1.Checked, checkBox2.Checked);
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
