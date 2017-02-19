using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RhClient
{
    public partial class Form4 : Form
    {
        public int block_size = 25 * 1024 * 8; // 25 Кб
        public Form1 Form_owner = null;
        public string path_current_client = string.Empty;
        public string path_prev_client = string.Empty;
        public string path_current = string.Empty;
        public delegate void SetListViewDelegate(List<ElementFile> list);
        public delegate void UpdateCurrentDirectiryDelegate();

        public delegate Form5 CreateFormProgress(FileTransferJobManagerDownload ftm, int Key, string filename);
        public delegate Form5 CreateFormProgressUpload(FileTransferJobManager ftm, int Key, string filename);
        public delegate void UpdateFormProgress(Form5 fr, int progress);

        public FileTransferJobManager ft_manager = null;

        public Form4()
        {
            InitializeComponent();
            InitView();
            AddFolderAndFile(String.Empty);
            UpdatePathLink();
            this.ActiveControl = splitContainer1;
            splitContainer1.ActiveControl = listView1;
            UpdateBackColor();

            Width = (int)(Screen.PrimaryScreen.Bounds.Width / 4 * 2.5);
            Height = (int)(Screen.PrimaryScreen.Bounds.Height / 4 * 2.5);
            Left = (Screen.PrimaryScreen.Bounds.Width - Width) / 2;
            Top = (Screen.PrimaryScreen.Bounds.Height - Height) / 2;
        }

        public void SetOwner(Form1 form1, FileTransferJobManager file_manager)
        {
            Form_owner = form1;
            ft_manager = file_manager;
        }

        public void InitView()
        {
            ImageList il = new ImageList();
            il.Images.Add(new Bitmap(RhClient.Properties.Resources.Folder));
            il.Images.Add(new Bitmap(RhClient.Properties.Resources.Hard));
            il.Images.Add(new Bitmap(RhClient.Properties.Resources.File));
            il.Images.Add(new Bitmap(RhClient.Properties.Resources.CD));
            il.Images.Add(new Bitmap(RhClient.Properties.Resources.USB));

            listView1.SmallImageList = il;
            listView2.SmallImageList = il;
        }
        public void AddDrive()
        {
            listView1.Items.Clear();
            List<ElementFile> element_list = FileTransfer.GetDriver();

            foreach (ElementFile d in element_list)
            {
                ListViewItem lvi = listView1.Items.Add(d.name);
                lvi.Tag = d.path;

                switch (d.drive_type)
                {
                    case DriveType.Fixed:
                        lvi.ImageIndex = 1;
                        break;
                    case DriveType.Removable:
                        lvi.ImageIndex = 4;
                        break;
                    case DriveType.CDRom:
                        lvi.ImageIndex = 3;
                        break;
                    default:
                        lvi.ImageIndex = 0;
                        break;
                }
            }
        }
        public void AddFolderAndFile(string path)
        {
            if (path != "")
            {
                List<ElementFile> elements = FileTransfer.GetFolderAndFile(path);

                listView1.Items.Clear();
                path_current = path;
                AddFolderAndFileClient(listView1, elements);
            }
            else
            {
                path_current = "";
                AddDrive();
            }
            UpdatePathLink();
            SelectFirstElement(listView1);
        }

        public void AddFolderAndFileClient(ListView lv, List<ElementFile> list)
        {
            lv.Items.Clear();

            if (list.Count == 0 && path_prev_client != string.Empty)
            {
                ElementFile el = new ElementFile("...", path_prev_client, FileType.Folder, DriveType.Unknown);
                list.Add(el);
            }


            foreach (ElementFile s in list)
            {
                ListViewItem lvi = lv.Items.Add(s.name);
                lvi.Tag = s.path;

                switch (s.type)
                {
                    case FileType.Folder:
                        lvi.ImageIndex = 0;
                        break;
                    case FileType.File:
                        lvi.ImageIndex = 2;
                        break;
                    case FileType.Disk:
                        switch (s.drive_type)
                        {
                            case DriveType.Fixed:
                                lvi.ImageIndex = 1;
                                break;
                            case DriveType.Removable:
                                lvi.ImageIndex = 4;
                                break;
                            case DriveType.CDRom:
                                lvi.ImageIndex = 3;
                                break;
                            default:
                                lvi.ImageIndex = 0;
                                break;
                        }
                        break;
                }
            }
        }

        protected void AddSubDir(TreeNode node, string path)
        {
            string[] dir = Directory.GetDirectories(path);
            foreach (string s in dir)
                node.Nodes.Add(s);
        }

        public void RefreshCurrentDir()
        {
            Form_owner.FileDataUpload(GetDirectoryAndFile(path_current_client));
        }

        private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshCurrentDir();
        }

        public Byte[] GetDirectoryAndFile(string path = "")
        {
            Byte[] stext = Encoding.UTF8.GetBytes(path);
            Byte[] byte_to_send = new Byte[8 + stext.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.get_dir.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(stext.Length), 0, byte_to_send, 4, 4);
            if (path.Length > 0)
                Buffer.BlockCopy(stext, 0, byte_to_send, 8, stext.Length);

            return byte_to_send;
        }

        public void SetDirClient(List<ElementFile> list)
        {
            AddFolderAndFileClient(listView2, list);
        }

        public void SetViewList(List<ElementFile> list)
        {
            if (this.InvokeRequired)
            {
                SetListViewDelegate d = new SetListViewDelegate(SetViewList);
                this.Invoke(d, new object[] { list });
            }
            else
            {
                SetDirClient(list);
            }
        }

        public Form5 CreateProgress(FileTransferJobManagerDownload ftm, int key, string filename)
        {
            if (this.InvokeRequired)
            {
                CreateFormProgress d = new CreateFormProgress(CreateProgress);
                return (Form5)this.Invoke(d, new object[] { ftm, key, filename });
            }
            else
            {
                return CreateForm5(ftm, key, filename);
            }
        }

        protected Form5 CreateForm5(FileTransferJobManagerDownload ftm, int Key, string filename)
        {
            Form5 fr = new Form5(ftm, Key);
            fr.filename = filename;
            fr.SetProgress(0);
            fr.Width = Screen.PrimaryScreen.Bounds.Width / 4;
            fr.Show();
            return fr;
        }

        public Form5 CreateProgressUpload(FileTransferJobManager ftm, int key, string filename)
        {
            if (this.InvokeRequired)
            {
                CreateFormProgressUpload d = new CreateFormProgressUpload(CreateProgressUpload);
                return (Form5)this.Invoke(d, new object[] { ftm, key, filename });
            }
            else
               return CreateForm5Upload(ftm, key, filename);
        }

        protected Form5 CreateForm5Upload(FileTransferJobManager ftm, int Key, string filename)
        {
            Form5 fr = new Form5(ftm, Key);
            fr.filename = filename;
            fr.SetProgress(0);
            fr.Width = Screen.PrimaryScreen.Bounds.Width / 4;
            fr.Show();
            return fr;
        }

        public void UpdateProgress(Form5 fr, int progress)
        {
            if (this.InvokeRequired)
            {
                UpdateFormProgress d = new UpdateFormProgress(UpdateProgress);
                this.Invoke(d, new object[] { fr, progress });
            }
            else
            {
                if (fr != null)
                    UpdateForm5(fr, progress);
            }
        }

        public void UpdateForm5(Form5 fr, int progress)
        {
            try
            {
                fr.SetProgressValue(progress);
            }
            catch
            {
                //...
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // инициируем передачу данных
            if (listView1.SelectedItems.Count == 1)
            {
                string path = "";

                try
                {
                    ListViewItem index = listView1.SelectedItems[0];
                    path = (string)index.Tag;
                }
                catch
                {
                    //...   
                }

                if (path.Length > 0 && path_current_client.Length > 0)
                {
                    FileInfo fl = new FileInfo(path);
                    if (fl.Exists)
                    {
                        if (MessageBox.Show("Передать " + fl.Name, "Копирование файла", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            int local_block_size = (int)Math.Ceiling(1.0f * fl.Length / 20);

                            if (local_block_size > FileTransfer.max_block_size)
                                local_block_size = FileTransfer.max_block_size;

                            if (local_block_size < FileTransfer.min_block_size)
                                local_block_size = FileTransfer.min_block_size;

                            int block_count = (int)Math.Ceiling(1.0f * fl.Length / local_block_size);
                            CopyHeaderBegin header = new CopyHeaderBegin(path_current_client, fl.Name, fl.FullName, fl.Length, block_count, local_block_size);

                            FileTransferJob job = new FileTransferJob(header);
                            ft_manager.AddJob(job);
                        }
                    }
                    else
                        MessageBox.Show("Файл " + fl.Name + " не найден", "Ошибка копирования файла", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                try
                {
                    ListViewItem index = listView2.SelectedItems[0];
                    if (index.ImageIndex != 2)
                    {
                        path_prev_client = path_current_client;
                        path_current_client = (string)index.Tag;
                        UpdatePathLink();
                        Form_owner.FileDataUpload(GetDirectoryAndFile(path_current_client));
                    }
                }
                catch
                {
                    //...
                    path_current_client = path_prev_client;
                    path_prev_client = string.Empty;
                }
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            if (Form_owner != null)
                if (Form_owner.client != null)
                    Form_owner.FileDataUpload(GetDirectoryAndFile(path_current_client));
        }

        protected void UpdatePathLink()
        {
            if (path_current.Length == 0)
                label1.Text = "\\";
            else
                label1.Text = path_current;

            if (path_current_client.Length == 0)
                label2.Text = "\\";
            else
                label2.Text = path_current_client;
        }

        private void Form4_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    UpdateBackColor();
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void UpdateBackColor()
        {
            if (splitContainer1.ActiveControl == (Control)listView1)
            {
                listView2.BackColor = SystemColors.InactiveBorder;
                listView1.BackColor = SystemColors.Window;
                SelectFirstElement(listView1);
            }
            if (splitContainer1.ActiveControl == (Control)listView2)
            {
                listView1.BackColor = SystemColors.InactiveBorder;
                listView2.BackColor = SystemColors.Window;
                SelectFirstElement(listView2);
            }
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Enter:
                    EnterElement_local();
                    break;
                case Keys.Back:
                    BackElement_local();
                    break;
                case Keys.F5:
                    CopyToolStripMenuItem_Click(listView1, new EventArgs());
                    break;
            }            
        }
        private void listView2_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    EnterElement_remote();
                    break;
                case Keys.Back:
                    BackElement_remote();
                    break;
                case Keys.F5:
                    copyFromClientToolStripMenuItem_Click(listView2, new EventArgs());
                    break;
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EnterElement_local();
        }

        private void EnterElement_remote()
        {
            if (listView2.SelectedItems.Count == 1)
            {
                try
                {
                    ListViewItem index = listView2.SelectedItems[0];
                    if (index.ImageIndex != 2)
                        listView2_MouseDoubleClick(listView2, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BackElement_remote()
        {
            try
            {
                if (path_current_client != string.Empty)
                {
                    ListViewItem index = listView2.Items[0];
                    path_current_client = (string)index.Tag;
                    UpdatePathLink();
                    Form_owner.FileDataUpload(GetDirectoryAndFile(path_current_client));
                }
            }
            catch
            {
                //...
            }
        }

        private void EnterElement_local()
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    ListViewItem index = listView1.SelectedItems[0];
                    if (index.ImageIndex != 2)
                        AddFolderAndFile((string)index.Tag);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BackElement_local()
        {
            if (path_current.Length > 0)
            {
                try
                {
                    AddFolderAndFile((string)listView1.Items[0].Tag);                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected void SelectFirstElement(ListView lv)
        {
            if (lv.Items.Count > 0)
            {
                if (lv.SelectedItems.Count == 0)
                {
                    lv.Items[0].Selected = true;
                    lv.Items[0].Focused = true;
                }
                else
                {
                    lv.SelectedItems[0].Selected = true;
                    lv.SelectedItems[0].Focused = true;
                }
            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            //..
            bool Cancel = false;
            if (Form_owner.file_transfer_download != null)
                if (Form_owner.file_transfer_download.dic.Count > 0)
                    Cancel = true;

            if (Form_owner.file_transfer_upload != null)
                if (Form_owner.file_transfer_upload.dic.Count > 0)
                    Cancel = true;

            e.Cancel = Cancel;
            if (!Cancel)
                Form_owner.Form_FileTransfer = null;
        }

        private void updateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            UpdateCurrentDir();
        }

        public void UpdateCurrentDir()
        {
            if (this.InvokeRequired)
            {
                UpdateCurrentDirectiryDelegate d = new UpdateCurrentDirectiryDelegate(UpdateCurrentDir);
                this.Invoke(d, new object[] {});
            }
            else
            {
                AddFolderAndFile(path_current);
            }
        }
        private void copyFromClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // инициируем передачу данных
            if (listView2.SelectedItems.Count == 1)
            {
                string path = "";
                string name = "";
                bool is_file = false;
                try
                {
                    ListViewItem index = listView2.SelectedItems[0];
                    path = (string)index.Tag;
                    name = index.Text;
                    if (index.ImageIndex == 2)
                        is_file = true;
                }
                catch
                {
                    //...   
                }

                if (path.Length > 0 && path_current.Length > 0 && is_file)
                {
                    if (MessageBox.Show("Передать " + name, "Копирование файла", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        CopyHeaderBegin head = new CopyHeaderBegin();
                        head.folder = path_current;
                        head.full_filename = path;
                        byte[] name_file = FileTransfer.SerializeObject(head);
                        byte[] data = new byte[8 + name_file.Length];

                        Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.copy_getfile.GetHashCode()), 0, data, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(name_file.Length), 0, data, 4, 4);
                        Buffer.BlockCopy(name_file, 0, data, 8, name_file.Length);

                        Form_owner.FileDataUpload(data);
                    }
                }
            }
        }

        public Form5 CreateFormDownload(FileTransferJobManager ftm, int Key, string filename)
        {
            Form5 fr = new Form5(ftm, Key);
            fr.filename = filename;
            fr.SetProgress(0);
            fr.Width = Screen.PrimaryScreen.Bounds.Width / 4;
            fr.Show();
            return fr;
        }

        private void listView1_Enter(object sender, EventArgs e)
        {
            UpdateBackColor();
        }

        private void listView2_Enter(object sender, EventArgs e)
        {
            UpdateBackColor();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputText it = new InputText();
            
            DialogResult res = it.ShowDialog(this);
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    AddFolderAndFile(it.path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }            
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InputText it = new InputText();

            DialogResult res = it.ShowDialog(this);
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                        path_prev_client = path_current_client;
                        path_current_client = it.path;
                        UpdatePathLink();
                        Form_owner.FileDataUpload(GetDirectoryAndFile(path_current_client));
                }
                catch
                {
                    //...
                    path_current_client = path_prev_client;
                    path_prev_client = string.Empty;
                }

            }
        }
    }

}
