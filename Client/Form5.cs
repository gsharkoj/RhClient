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
    public partial class Form5 : Form
    {
        public string filename = "";
        public FileTransferJobManager file_job_manager = null;
        public FileTransferJobManagerDownload file_job_manager_download = null;
        public int key;

        public delegate void SetProgressDelegate(int Value);
        public Form5(FileTransferJobManager JM, int Key)
        {
            InitializeComponent();
            file_job_manager = JM;
            key = Key;
        }

        public Form5(FileTransferJobManagerDownload JMD, int Key)
        {
            InitializeComponent();
            file_job_manager_download = JMD;
            key = Key;
        }

        public void SetProgress(int Value)
        {
            if (this.InvokeRequired)
            {
                SetProgressDelegate d = new SetProgressDelegate(SetProgress);
                this.Invoke(d, new object[] { Value });
            }
            else
                SetProgressValue(Value);
        }

        public void SetProgressValue(int Value)
        {
            Text = Value.ToString() + "%";
            if (filename.Length > 0)
                Text += " " + filename;
            progressBar1.Value = Value;
            if(progressBar1.Value >= 100)
            {
                Close();
            }
        }
        public void Step()
        {            
            progressBar1.PerformStep();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(file_job_manager!=null)
                file_job_manager.SendStopDownload(key);

            if (file_job_manager_download != null)
                file_job_manager_download.SendStopDownload(key);

            Close();
        }
    }
}
