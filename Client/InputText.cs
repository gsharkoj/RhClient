using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace RhClient
{
    public partial class InputText : Form
    {
        public string path = "";

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }

        public InputText()
        {
            InitializeComponent();
        }

        private void InputText_Load(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (text.Contains(":\\") || text.Contains("\\\\"))
                {
                    SetText(text);
                    ActiveControl = button1;
                }
                else
                    ActiveControl = textBox1;
            }
        }

        void ClientThread(Object StateInfo)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (text.Contains(":\\") || text.Contains("\\\\"))
                    SetText(text);                    
                else
                    ActiveControl = textBox1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            path = textBox1.Text;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

    }
}
