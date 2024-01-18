using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Pfim;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;
using JmdLoader.Setting;

using Raycity.File;

namespace JmdLoader
{
    public partial class ExtractFolder : Form
    {
        private struct ExtractInfo
        {
            public string RelativePath { get; set; }
            public string Out_filename { get; set; }
            public JmdFile FileInfo { get; set; }
            public FileConvertProcessor ConvertProcessor { get; set; }
        }

        private delegate byte[] FileConvertProcessor(byte[] input);

        private static class ExtractConverter
        {
            public static byte[] DDSConverter(byte[] inputData)
            {
                var stream = new MemoryStream(inputData);
                IImage image = Pfim.Pfim.FromStream(stream);
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                var d = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                PixelFormat pf;
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                        pf = PixelFormat.Format32bppArgb;
                        break;
                    case Pfim.ImageFormat.Rgba16:
                        pf = PixelFormat.Format16bppArgb1555;
                        break;
                    case Pfim.ImageFormat.Rgb8:
                        pf = PixelFormat.Format8bppIndexed;
                        break;
                    case Pfim.ImageFormat.Rgb24:
                        pf = PixelFormat.Format24bppRgb;
                        break;
                    default:
                        throw new Exception("");
                }
                stream.Dispose();
                stream = new MemoryStream();
                Bitmap bmp = new Bitmap(image.Width, image.Height, image.Stride, pf, d);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] output = ms.ToArray();

                handle.Free();
                image.Dispose();
                ms.Dispose();
                bmp.Dispose();

                return output;
            }

            public static byte[] BMLConverter(byte[] inputData)
            {
                Raycity.Xml.BinaryXmlDocument bxd = new Raycity.Xml.BinaryXmlDocument();
                bxd.Read(Encoding.GetEncoding("UTF-16"), inputData);
                Raycity.Xml.BinaryXmlTag bxt = bxd.RootTag;
                string xmlData = bxt.ToString();
                byte[] output = Encoding.UTF8.GetBytes(xmlData);
                return output;
            }

            public static byte[] KSVConverter(byte[] inputData)
            {
                return inputData;
            }

            public static byte[] NoneConvert(byte[] inputData)
            {
                return inputData;
            }
        }

        private JmdFolder _extractFolder;
        private ExtractOptionToken _extractOption;
        private Task _bgWorker;
        private string _extractPath;
        private int _totalFiles = 0;

        private bool _terminated = false;
        private bool _bgWorkerFinished = false;
        

        public ExtractFolder(JmdFolder extract_Folder, string extractToPath, ExtractOptionToken extractOption)
        {
            InitializeComponent();
            _extractFolder = extract_Folder;
            _extractOption = extractOption;
            _extractPath = extractToPath;
        }

        private void BeginExtract()
        {
            //relative_path means the relative path of folder.
            Queue<(string relative_path, JmdFolder folder)> extend_queue = new Queue<(string relative_path, JmdFolder folder)>();
            Queue<ExtractInfo> file_queue = new Queue<ExtractInfo>();
            extend_queue.Enqueue(( "",_extractFolder));
            ReportProgress("( Preprocessing extract files )", 0);
            while (extend_queue.Count > 0) 
            {
                if (_terminated)
                {
                    _bgWorkerFinished = true;
                    TerminateExtract();
                    return;
                }
                (string relative_path, JmdFolder folder) cur_proc_obj = extend_queue.Dequeue();
                string out_path = $"{_extractPath}{cur_proc_obj.relative_path}";
                foreach (JmdFolder subFolder in cur_proc_obj.folder.Folders)
                {
                    extend_queue.Enqueue(($"{cur_proc_obj.relative_path}\\{subFolder.Name}", subFolder));
                }
                foreach(JmdFile sub_file in cur_proc_obj.folder.Files)
                {
                    ExtractInfo extractInfo = new ExtractInfo
                    {
                        RelativePath = cur_proc_obj.relative_path,
                        FileInfo = sub_file
                    };
                    if((_extractOption & ExtractOptionToken.ConvertDDS) != ExtractOptionToken.None && sub_file.Name.EndsWith(".dds"))
                    {
                        extractInfo.ConvertProcessor = ExtractConverter.DDSConverter;
                        extractInfo.Out_filename = $"{sub_file.Name[0..^4]}.png";
                    }
                    else if ((_extractOption & ExtractOptionToken.ConvertBML) != ExtractOptionToken.None && sub_file.Name.EndsWith(".bml"))
                    {
                        extractInfo.ConvertProcessor = ExtractConverter.BMLConverter;
                        extractInfo.Out_filename = $"{sub_file.Name[0..^4]}.xml";
                    }
                    else
                    {
                        extractInfo.Out_filename = $"{sub_file.Name}";
                    }
                    file_queue.Enqueue(extractInfo);
                }
            }
            _totalFiles = file_queue.Count;
            if (this.InvokeRequired)
                this.Invoke(() =>
                {
                    this.progress_main.MaxValue = _totalFiles;
                });
            while(file_queue.Count > 0)
            {
                if (_terminated)
                {
                    _bgWorkerFinished = true;
                    TerminateExtract();
                    return;
                }
                ExtractInfo extract_info = file_queue.Dequeue();
                ReportProgress(extract_info.FileInfo.FullName, _totalFiles - file_queue.Count);
                string filePath = extract_info.RelativePath;
                filePath = filePath.Replace("/", "\\");
                string[] pathSp = filePath.Split('\\');
                string tmpStr = "";
                for(int i = 0; i < pathSp.Length; i++)
                {
                    tmpStr += pathSp[i] + "\\";
                    if (!Directory.Exists($"{_extractPath}\\{tmpStr}"))
                        Directory.CreateDirectory($"{_extractPath}\\{tmpStr}");
                }
                FileStream out_fs = new FileStream($"{_extractPath}\\{extract_info.RelativePath}\\{extract_info.Out_filename}", FileMode.Create);
                byte[] file_data = extract_info.FileInfo.GetBytes();
                try
                {
                    byte[] proc_file_data = extract_info.ConvertProcessor?.Invoke(file_data) ?? file_data;
                    if (proc_file_data is null || proc_file_data.Length == 0)
                        throw new Exception("zero!");

                    out_fs.Write(proc_file_data, 0, proc_file_data.Length);
                    out_fs.Close();
                    file_data = null;
                    proc_file_data = null;
                }
                catch (Exception ex) 
                {
                    Debug.Print($"Error: {ex.Message}");
                    this._bgWorkerFinished = true;
                    TerminateExtract();
                }
            }
            ReportProgress("Finished", _totalFiles);
            _bgWorkerFinished = true;
            FinishExtract();
        }

        private void FinishExtract()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(FinishExtract);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void TerminateExtract()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(TerminateExtract);
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void ReportProgress(string current_output_file, int file_no)
        {
            if (this.InvokeRequired)
            {
                Action<string, int> action = ReportProgress;
                this.Invoke(action, current_output_file, file_no);
            }
            else
            {
                text_extract_file.Text = current_output_file;
                text_progress.Text = $"{file_no}/{_totalFiles}";
                progress_main.Value = file_no - 1;

            }
        }

        // actions
        private void action_show(object sender, EventArgs e)
        {
            _bgWorker = new Task(BeginExtract);
            _bgWorker.Start();
        }

        private void action_cancel(object sender, EventArgs e)
        {
            if(MessageBox.Show("msg_cancelExtract".GetStringBag(), "msg_level_question".GetStringBag(), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _terminated = true;
            }
        }
    }
}
