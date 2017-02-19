using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace RhClient
{

    [Serializable]
    public enum FileType
    {
        Disk = 0,
        Folder = 1,
        File = 2,
    }

    [Serializable]
    public enum DiskType
    {
        Fixed = 0,
        Folder = 1,
        File = 2,
    }


    [Serializable]
    public struct ElementFile
    {

       public string path;
       public string name;
       public FileType type;
       public DriveType drive_type;

        public ElementFile(string Name, string Path, FileType Type, DriveType Driver_Type) : this()
        {
            path = Path;
            name = Name;
            type = Type;
            drive_type = Driver_Type;
        }

    }

    [Serializable]
    public struct CopyHeaderBegin
    {
        public string folder;
        public string filename;
        public string full_filename;
        public long size;
        public int block_count;
        public int block_size;
        public int key;

        public CopyHeaderBegin(string Folder, string Filename, string Full_Filename, long Size, int Block_Count, int Block_Size) : this()
        {
            folder = Folder;
            filename = Filename;
            full_filename = Full_Filename;
            size = Size;
            block_count = Block_Count;
            block_size = Block_Size;
            key = -1;
        }
    }

    public static class FileTransfer
    {
         public const int max_block_size = 100 * 1024 * 8; // 100 Кб
         public const int min_block_size = 5 * 1024 * 8; // 5 Кб

        public static List<ElementFile> ListDir(string path)
        {
            try
            {
                if (path.Length == 0)
                    return GetDriver();
                else
                    return GetFolderAndFile(path);
            }
            catch
            {
                return new List<ElementFile>();
            }

        }
        public static List<ElementFile> GetDriver()
        {
            
            DriveInfo[] dr = DriveInfo.GetDrives();
            List<ElementFile> lvi = new List<ElementFile>();
            
            foreach (DriveInfo d in dr)
            {
                ElementFile ft = new ElementFile();
                ft.path = d.Name;
                ft.name = d.Name;
                ft.drive_type = d.DriveType;
                ft.type = FileType.Disk;
                lvi.Add(ft);
            }
            return lvi;
        }


        public static List<ElementFile> GetFolderAndFile(string path)
        {
            List<ElementFile> lvi = new List<ElementFile>();

            var val_path = path;
            string[] str_path = val_path.Split('\\');

            // папки
            string[] dir = Directory.GetDirectories(path);

            string prev_path = "";
            for (int i = 0; i < str_path.Length - 1; i++)
            {
                if (i == str_path.Length - 2)
                    prev_path = prev_path + str_path[i];
                else
                    prev_path = prev_path + str_path[i] + "\\";
            }

            if ((prev_path.Split('\\')).Length == 1)
                prev_path = prev_path + "\\";

            if (str_path[1] == "")
                prev_path = "";

            ElementFile ft = new ElementFile("...", prev_path, FileType.Folder, DriveType.Unknown);
            lvi.Add(ft);

            foreach (string s in dir)
            {
                var val = s;
                string[] str = val.Split('\\');
                ft = new ElementFile(str[str.Length - 1], s, FileType.Folder, DriveType.Unknown);
                lvi.Add(ft);
            }

            // Файлы
            dir = Directory.GetFiles(path);
            foreach (string s in dir)
            {
                var val = s;
                string[] str = val.Split('\\');
                ft = new ElementFile(str[str.Length - 1], s, FileType.File, DriveType.Unknown);
                lvi.Add(ft);
            }

            return lvi;
        }

        public static byte[] SerializeObject(Object Obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, Obj);
            ms.Close();
            return ms.ToArray();
        }

        public static List<ElementFile> DeserializeList(byte[] Data, int StartIndex, int Count)
        {
            // десериализация
            MemoryStream mem_stream_in = new MemoryStream(Data, StartIndex, Count, false);
            BinaryFormatter formatter_out = new BinaryFormatter();
            List<ElementFile> elements_out = (List<ElementFile>)formatter_out.Deserialize(mem_stream_in);
            mem_stream_in.Close();
            return elements_out;
        }

        public static string DeserializeString(byte[] Data, int StartIndex, int Count)
        {
            // десериализация
            MemoryStream mem_stream_in = new MemoryStream(Data, StartIndex, Count, false);
            BinaryFormatter formatter_out = new BinaryFormatter();
            string elements_out = (string)formatter_out.Deserialize(mem_stream_in);
            mem_stream_in.Close();
            return elements_out;
        }
        public static CopyHeaderBegin DeserializeHeader(byte[] Data, int StartIndex, int Count)
        {
            // десериализация
            MemoryStream mem_stream_in = new MemoryStream(Data, StartIndex, Count, false);
            BinaryFormatter formatter_out = new BinaryFormatter();
            CopyHeaderBegin elements_out = (CopyHeaderBegin)formatter_out.Deserialize(mem_stream_in);
            mem_stream_in.Close();
            return elements_out;
        }

    }

    public class FileTransferJob
    {
        public CopyHeaderBegin header;
        protected bool[] block_transfer;
        public int progress = 0;
        public string error = string.Empty;
        public int block_download = 0;
        public FileTransferJob(CopyHeaderBegin Header)
        {
            header = Header;
            block_transfer = new bool[header.block_count];

            for (int i = 0; i < block_transfer.Length; i++)
                block_transfer[i] = false;
        }
        
        public void SetBlockUploadet(int block_number)
        {
            block_transfer[block_number] = true;
        }

        public byte[] GetStopData()
        {
            lock(block_transfer)
            {
                for (int i = 0; i < block_transfer.Length; i++)
                    block_transfer[i] = true;
            }
            byte[] byte_to_send = new byte[8];

            Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.copy_stop.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(this.header.key), 0, byte_to_send, 4, 4);
            return byte_to_send;
        }

        protected byte[] GetDataCommand(FileCommand command)
        {
            byte[] byte_to_send = new byte[8];

            Buffer.BlockCopy(BitConverter.GetBytes(command.GetHashCode()), 0, byte_to_send, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(this.header.key), 0, byte_to_send, 4, 4);
            return byte_to_send;
        }
        public byte[] GetNextData(int block_number)
        {
            int index = -1;
            for (int i = 0; i < block_transfer.Length; i++)
                if (block_transfer[i] == false)
                {
                    index = i;
                    break;
                }
            if (index == -1)
            {
                progress = 100;
                return GetDataCommand(FileCommand.copy_end);
            }
            else
            {
                long start = header.block_size * index;
                long end = header.block_size * (index + 1);
                if (end > header.size)
                    end = header.size;

                FileStream fs = null;
                try
                {
                    fs = new FileStream(header.full_filename, FileMode.Open, FileAccess.Read);
                    fs.Seek(start, SeekOrigin.Begin);
                }
                catch(Exception ex)
                {
                    error = ex.Message;
                    progress = 100;
                    return GetDataCommand(FileCommand.copy_stop);
                }

                int byte_to_read = (int)(end - start);
                byte[] byte_to_send = new byte[12 + byte_to_read];

                Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.set_next_block.GetHashCode()), 0, byte_to_send, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(this.header.key), 0, byte_to_send, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(byte_to_read), 0, byte_to_send, 8, 4);

                try
                {
                    int offset = 0;
                    while (offset != byte_to_read)
                        offset = offset + fs.Read(byte_to_send, 12 + offset, byte_to_read - offset);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    progress = 100;
                    return GetDataCommand(FileCommand.copy_stop);
                }

                fs.Close();
                                
                int count = 0;
                foreach (bool val in block_transfer)
                {
                    if (val == true)
                        count += 1;
                }

                block_transfer[index] = true;
                progress = (int)Math.Floor(1.0f * count / block_transfer.Length * 100);

                return byte_to_send;
            }
        }

        public bool CreateFile()
        {
            try
            {
                FileStream fs = new FileStream(header.folder + "\\" + header.filename, FileMode.Create, FileAccess.Write);
                fs.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool SetBlockData(byte[] data, int offset, int count)
        {
            try
            {
                FileStream fs = new FileStream(header.folder + "\\" + header.filename, FileMode.Append, FileAccess.Write);
                fs.Write(data, offset, count);
                fs.Close();

                block_download += 1;                
                if (block_download >= header.block_count)
                    progress = 100;
                else
                    progress = (int)Math.Floor(1.0f * block_download / header.block_count * 100);

            }
            catch
            {
                return false;
            }

            return true;
        }
    }
    public class FileTransferJobManager
    {
        int key;
        public Dictionary<int, FileTransferJob> dic = null;
        Dictionary<int, Form5> dic_form = null;
        Form1 net_send;
        public FileTransferJobManager(Form1 Sender)
        {
            dic = new Dictionary<int, FileTransferJob>();
            dic_form = new Dictionary<int, Form5>();
            key = 1;
            net_send = Sender;
        }

        public void BeginCopy(string path, string full_filename)
        {
            FileInfo fl = new FileInfo(full_filename);
            if (fl.Exists)
            {
                    int local_block_size = (int)Math.Ceiling(1.0f * fl.Length / 20);

                    if (local_block_size > FileTransfer.max_block_size)
                        local_block_size = FileTransfer.max_block_size;

                    if (local_block_size < FileTransfer.min_block_size)
                        local_block_size = FileTransfer.min_block_size;

                    int block_count = (int)Math.Ceiling(1.0f * fl.Length / local_block_size);
                    CopyHeaderBegin header = new CopyHeaderBegin(path, fl.Name, fl.FullName, fl.Length, block_count, local_block_size);

                    FileTransferJob job = new FileTransferJob(header);
                    AddJob(job, false);
            }
        }

        public void AddJob(FileTransferJob Job, bool local = true)
        {
            lock (dic)
            {
                Job.header.key = key;
                dic.Add(key, Job);
                SendBeginUpload(key);
                key = key + 1;
            }
            
            if (net_send.Form_FileTransfer != null)
            {
                Form5 fr = net_send.Form_FileTransfer.CreateProgressUpload(this, Job.header.key, Job.header.filename);
                dic_form.Add(Job.header.key, fr);
            }
        }

        public int SendStopDownload(int Key)
        {
            int count = 0;

            if (dic.ContainsKey(Key))
            {
                lock (dic[Key])
                {
                    byte[] data = new byte[8];
                    Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.copy_stop.GetHashCode()), 0, data, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, data, 4, 4);
                    net_send.FileDataUpload(data);
                    count = data.Length;
                }
            }            
            EndCopy(Key);
            return count;
        }

        public void SetBlockUploadet(int Key, int block_number)
        {
            lock(dic[Key])
                dic[Key].SetBlockUploadet(block_number);
        }

        public int SendNextData(int Key)
        {
            byte[] data = null;
            lock(dic)
            {
                if (dic.ContainsKey(Key))
                {
                    data = dic[Key].GetNextData(0);                        
                }
            }

            net_send.FileDataUpload(data);
            Form5 fr = null;
            bool have_form = false;

            if (dic_form.TryGetValue(Key, out fr))
            {
                if (fr != null)
                {
                    have_form = true;
                    net_send.Form_FileTransfer.UpdateProgress(fr, dic[Key].progress);    
                }
            }
            
            lock (dic)
            {

                if (dic.ContainsKey(Key))
                {
                    if (dic[Key].error != string.Empty && have_form)
                        net_send.ShowMessageBox(dic[Key].error, "Ошибка чтения файла", MessageBoxIcon.Error);
                }
            }

            if (data == null)
                return 0;
            else
                return data.Length;            
        }

        protected int SendBeginUpload(int Key)
        {
            int count = 0;
            lock(dic)
            {
                if (dic.ContainsKey(Key))
                {
                    byte[] byte_header = FileTransfer.SerializeObject(dic[Key].header);
                    byte[] data = new byte[8 + byte_header.Length];

                    Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.copy_begin.GetHashCode()), 0, data, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(byte_header.Length), 0, data, 4, 4);
                    Buffer.BlockCopy(byte_header, 0, data, 8, byte_header.Length);

                    net_send.FileDataUpload(data);
                    count = data.Length;
                }
            }
            return count;
        }
        public void EndCopy(int Key)
        {
            lock(dic)
            {
                if (dic.ContainsKey(Key))
                    dic.Remove(Key);
            }

            lock (dic_form)
            {
                if (dic_form.ContainsKey(Key))
                {
                    if (dic_form[Key] != null)
                        dic_form[Key].Close();
                    dic_form.Remove(Key);
                }
            }
        }

        ~FileTransferJobManager()
        {
            if (dic != null)
            {
                lock (dic)
                {
                    foreach (KeyValuePair<int, Form5> val in dic_form)
                        if (val.Value != null)
                            val.Value.Close();
                    dic.Clear();
                    dic = null;
                }
            }
        }
    }

    public class FileTransferJobManagerDownload
    {
        public Dictionary<int, FileTransferJob> dic = null;
        Dictionary<int, Form5> dic_form = null;
        Form1 net_send;
        public FileTransferJobManagerDownload(Form1 Sender)
        {
            dic = new Dictionary<int, FileTransferJob>();
            dic_form = new Dictionary<int, Form5>();
            net_send = Sender;
        }

        public void AddJob(FileTransferJob Job)
        {
            lock (dic)
            {
                dic.Add(Job.header.key, Job);
                if (net_send.Form_FileTransfer != null)
                {
                    Form5 fr = net_send.Form_FileTransfer.CreateProgress(this, Job.header.key, Job.header.filename);
                    dic_form.Add(Job.header.key, fr);
                }
            }

            // создаем файл, в который записываем данные
            bool res = Job.CreateFile();
            if (res)
                SendNextDownload(Job.header.key);
            else
                SendStopDownload(Job.header.key);
        }

        public void SetBlockData(int Key, byte[] data, int offset, int count)
        {
            bool res = false;
            lock (dic)
            {
                if (dic.ContainsKey(Key))
                {
                   res = dic[Key].SetBlockData(data, offset, count);
                }
            }

            Form5 fr = null;
            bool have_form = false;

            if (dic_form.TryGetValue(Key, out fr))
            {
                if (fr != null)
                {
                    have_form = true;
                    net_send.Form_FileTransfer.UpdateProgress(fr, dic[Key].progress);
                }
                if (dic[Key].error != string.Empty && have_form)
                    net_send.ShowMessageBox(dic[Key].error, "Ошибка чтения файла", MessageBoxIcon.Error);
            }

            if (res)
                SendNextDownload(Key);
            else
                SendStopDownload(Key);
        }

        public void EndCopy(int Key)
        {
            lock(dic)
            {
                if (dic.ContainsKey(Key))
                    dic.Remove(Key);
            }
            lock(dic_form)
            {
                if (dic_form.ContainsKey(Key))
                {
                    if (dic_form[Key] != null)
                        dic_form[Key].Close();
                    dic_form.Remove(Key);
                }
            }
        }

        protected int SendNextDownload(int Key)
        {
            int count = 0;
            lock(dic)
            {
                if (dic.ContainsKey(Key))
                {
                    byte[] byte_header = FileTransfer.SerializeObject(dic[Key].header);
                    byte[] data = new byte[8];
                    Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.get_next_block.GetHashCode()), 0, data, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, data, 4, 4);
                    net_send.FileDataUpload(data);
                    count = data.Length;
                }
            }
            return count;
        }

        public int SendStopDownload(int Key)
        {
            if (net_send.Form_FileTransfer != null)
                net_send.Form_FileTransfer.UpdateCurrentDir();

            int count = 0;

            byte[] data = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(FileCommand.copy_stop.GetHashCode()), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Key), 0, data, 4, 4);
            net_send.FileDataUpload(data);
            count = data.Length;

            EndCopy(Key);

            return count;
        }

        ~FileTransferJobManagerDownload()
        {
            if (dic != null)
                lock(dic)
                    dic.Clear();
        }
    }

}
