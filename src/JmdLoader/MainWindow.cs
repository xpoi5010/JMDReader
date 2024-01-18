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
using System.Diagnostics;
using System.Resources;
using Raycity.Xml;
using Raycity.IO;
using Raycity.File;

using JmdLoader.PreviewWindow;
using JmdLoader.Setting;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;

using JmdLoader.Controls;
using System.Xml.Linq;

namespace JmdLoader
{
    public partial class MainWindow : Form
    {
        private SettingLoader BaseSettingLoader = new SettingLoader();

        private JmdArchive? _jmdArchive;
        private JmdFolder? _curFolder;
        private JmdFolder? _rootFolder;

        public MainWindow()
        {
            InitializeComponent();
            LanguageManager.LoadLang();
            LanguageManager.LanguageName = "en-us";
            AddLanguages();
            LoadSetting();
            LoadLang();
            listview_main.SmallImageList = imageList_listview;
            listview_main.SmallImageList.Images.Add("file", new Bitmap(global::JmdLoader.Properties.Resources.baseline_insert_drive_file_black_18dp));
            listview_main.SmallImageList.Images.Add("folder", new Bitmap(global::JmdLoader.Properties.Resources.folder_close));
        }
        public MainWindow(StartupOption startupOption) : this()
        {

        }

        #region Globalization
        private void LoadLang()
        {
            this.menu.Text = ((string)this.menu.Tag).GetStringBag();
            this.menu_file.Text = ((string)this.menu_file.Tag).GetStringBag();
            this.menu_file_open.Text = ((string)this.menu_file_open.Tag).GetStringBag();
            this.menu_file_exit.Text = ((string)this.menu_file_exit.Tag).GetStringBag();
            this.menu_extract.Text = ((string)this.menu_extract.Tag).GetStringBag();
            this.menu_extract_all.Text = ((string)this.menu_extract_all.Tag).GetStringBag();
            this.menu_extract_current.Text = ((string)this.menu_extract_current.Tag).GetStringBag();
            this.filemenu_extractfile.Text = ((string)this.filemenu_extractfile.Tag).GetStringBag();
            this.menu_about.Text = ((string)this.menu_about.Tag).GetStringBag();
            this.columnHeader1.Text = ((string)this.columnHeader1.Tag).GetStringBag();
            this.columnHeader2.Text = ((string)this.columnHeader2.Tag).GetStringBag();
            this.columnHeader3.Text = ((string)this.columnHeader3.Tag).GetStringBag();
            this.Text = ((string)this.Tag).GetStringBag();
            this.menu_lang.Text = ((string)this.menu_lang.Tag).GetStringBag();
            this.filemenu_extract_selected.Text = ((string)this.filemenu_extract_selected.Tag).GetStringBag();
            this.filemenu_convertPNG.Text = ((string)this.filemenu_convertPNG.Tag).GetStringBag();
            this.filemenu_convertXML.Text = ((string)this.filemenu_convertXML.Tag).GetStringBag();
            LoadLangFont();
            if (_curFolder is not null)
                UpdateUIFolder();
            /*
            if (!(BaseRhoFile is null))
                UpdateFolders();
            */
        }
        private void AddLanguages()
        {
            List<ToolStripItem> temp = new List<ToolStripItem>();
            Language[] langs = LanguageManager.ListLanguages();
            foreach (Language lang in langs)
            {
                ToolStripMenuItem menu_langname = new ToolStripMenuItem();
                menu_langname.Text = lang.DisplayName;
                menu_langname.AutoSize = true;
                menu_langname.Click += action_changeLanguage;
                menu_langname.Font = lang.GetLangFontWithBase(this.Font);
                temp.Add(menu_langname);
            }
            menu_lang.DropDownItems.AddRange(temp.ToArray());
        }
        private void LoadLangFont()
        {
            this.menu.Font = LanguageManager.GetLangFontWithBase(this.menu.Font);
            this.menu_file.Font = LanguageManager.GetLangFontWithBase(this.menu_file.Font);
            this.menu_file_open.Font = LanguageManager.GetLangFontWithBase(this.menu_file_open.Font);
            this.menu_file_exit.Font = LanguageManager.GetLangFontWithBase(this.menu_file_exit.Font);
            this.menu_extract.Font = LanguageManager.GetLangFontWithBase(this.menu_extract.Font);
            this.menu_extract_all.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_all.Font);
            this.menu_extract_current.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_current.Font);
            this.filemenu_extractfile.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extractfile.Font);
            this.menu_about.Font = LanguageManager.GetLangFontWithBase(this.menu_about.Font);
            this.listview_main.Font = LanguageManager.GetLangFontWithBase(this.listview_main.Font);
            this.Text = ((string)this.Tag).GetStringBag();
            //this.menu_lang.Font = LanguageManager.GetLangFontWithBase(this.menu_lang.Font);
            this.filemenu_convertPNG.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertPNG.Font);
            this.filemenu_convertXML.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertXML.Font);
            this.filemenu_extract_selected.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extract_selected.Font);
        }

        #endregion
        #region Setting Loading
        private void LoadSetting()
        {
            if (!File.Exists("Setting.json"))
                return;
            BaseSettingLoader.LoadSetting("Setting.json");
            JmdLoader.Setting.Setting baseSetting = BaseSettingLoader.Setting;
            LanguageManager.SetLanguage(LanguageName: baseSetting.Language);
        }
        #endregion
        #region Control Action
        private void action_changeLanguage(object sender, EventArgs e)
        {
            ToolStripItem menu_langname = (ToolStripItem)sender;
            LanguageManager.SetLanguage(DisplayName: menu_langname.Text);
            BaseSettingLoader.Setting.Language = LanguageManager.LanguageName;
            BaseSettingLoader.SaveSetting("Setting.json");
            LoadLang();
        }
        private void action_open(object sender, EventArgs e)
        {
            if (dialog_singleFile.ShowDialog() == DialogResult.OK)
            {
                CloseCurrentFile();
                _jmdArchive = new JmdArchive();
                _jmdArchive.Open(dialog_singleFile.FileName);
                Queue<(TreeNode, JmdFolder)> queue = new Queue<(TreeNode, JmdFolder)>();
                TreeNode rootNode = new TreeNode()
                {
                    Text = Path.GetFileName(dialog_singleFile.FileName),
                    Tag = new NodeInfoContainer(NodeType.Folder, _jmdArchive.RootFolder)
                };
                queue.Enqueue((rootNode, _jmdArchive.RootFolder));
                while (queue.Count > 0)
                {
                    (TreeNode, JmdFolder) curEle = queue.Dequeue();
                    foreach (JmdFolder folder in curEle.Item2.Folders)
                    {
                        TreeNode subnode = new TreeNode()
                        {
                            Text = folder.Name,
                            Tag = new NodeInfoContainer(NodeType.Folder, folder)
                        };
                        curEle.Item1.Nodes.Add(subnode);
                        queue.Enqueue((subnode, folder));
                    }
                }
                _curFolder = _rootFolder = _jmdArchive.RootFolder;
                treeview_explorer.Nodes.Add(rootNode);
                //PathStack.Push(rootFolders[0].FullName);
                UpdateUIFolder();
            }
        }
        private void action_aboutWindow(object sender, EventArgs e)
        {
            AboutMe aboutDialog = new AboutMe();
            aboutDialog.ShowDialog();
        }
        private void action_exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void action_listview_click(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            if (listview_main.SelectedItems.Count == 0)
                return;
            contextMenu_list.Show((Control)sender, e.X, e.Y);
            filemenu_extract_selected.Enabled = true;
            if (listview_main.SelectedItems.Count == 1 && listview_main.SelectedItems[0].Tag is JmdFile)
            {
                filemenu_extractfile.Enabled = true;
                filemenu_convertPNG.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".tga") || listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".dds");
                filemenu_convertXML.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".bml");
            }
            else
            {
                filemenu_extractfile.Enabled = false;
                filemenu_convertPNG.Enabled = false;
                filemenu_convertXML.Enabled = false;
            }
        }
        private void action_listview_doubleclick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem selItem = listview_main.SelectedItems[0];
            if (selItem.Tag is JmdFile selFile)
            {
                string FileName = selItem.Text;
                string ext = selFile.FullName[^3..^0];
                if (ext == "dds" || ext == "tga")
                {
                    TgaDDsViewer tdv = new TgaDDsViewer();
                    tdv.Data = selFile.GetBytes();
                    tdv.Type = ext == "dds" ? TgaDDsViewer.FileType.dds : ext == "tga" ? TgaDDsViewer.FileType.tga : throw new Exception();
                    tdv.ShowBox();
                }
                else if (ext == "bml")
                {
                    byte[] bmlData = selFile.GetBytes();
                    bmlViewer bv = new bmlViewer(bmlData, selFile.FullName);
                    bv.Show();
                }
                else
                {
                    FileStream fs = new FileStream(Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}", FileMode.Create);
                    byte[] data = selFile.GetBytes();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                    Process ps = new Process();
                    ps.StartInfo.FileName = "explorer.exe";
                    ps.StartInfo.Arguments = Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}";
                    ps.Start();
                }
            }
            else if (selItem.Tag is JmdFolder selFolder)
            {
                _curFolder = selFolder;
                UpdateUIFolder();
            }
        }
        private void action_back(object sender, EventArgs e)
        {
            if (_curFolder != _rootFolder)
            {
                _curFolder = _curFolder?.Parent == null ? _rootFolder : _curFolder?.Parent;
                UpdateUIFolder();
            }
            else
            {
                icon_back.Enabled = false;
            }
        }
        private void action_icon_enable_changed(object sender, EventArgs e)
        {
            if (icon_back.Enabled)
                this.icon_back.Image = global::JmdLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled;
            else
                this.icon_back.Image = global::JmdLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled_disabled;
        }
        private void action_node_select(object sender, TreeViewEventArgs e)
        {
            var clickNode = e.Node;
            if (clickNode is null)
                return;
            var nodeInfo = clickNode.Tag as NodeInfoContainer;
            if (nodeInfo is null)
                return;
            var folderInfo = nodeInfo.BaseData as JmdFolder;
            if (folderInfo is null)
                return;
            _curFolder = folderInfo;
            UpdateUIFolder();
        }
        private void action_extract_all(object sender, EventArgs e)
        {
            if (_curFolder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_rootFolder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extract_current(object sender, EventArgs e)
        {
            if (_curFolder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_curFolder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extractfile(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is JmdFile selFile)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                sfd.Filter = "AllFiles|*.*";
                sfd.FileName = FileName;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    byte[] data = selFile.GetBytes();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                }
            }
        }
        private void action_extract_selected(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            if (_curFolder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {
                List<JmdFile> outputFiles = new List<JmdFile>();
                List<JmdFolder> outputFolders = new List<JmdFolder>();
                JmdFolder extractVirtualFolder = new JmdFolder();
                foreach (ListViewItem selItem in listview_main.SelectedItems)
                {
                    if (selItem.Tag is JmdFile selFile)
                    {
                        extractVirtualFolder.AddFile(selFile);
                    }
                    else if (selItem.Tag is JmdFolder selFolder)
                    {
                        extractVirtualFolder.AddFolder(selFolder);
                    }
                }
                ExtractFolder _extractFolderDialog = new ExtractFolder(extractVirtualFolder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_convert_png(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is JmdFile selFile)
            {
                string fileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = selFile.GetBytes();
                TgaDDsViewer tga_viewer = new TgaDDsViewer();
                tga_viewer.Data = data;
                tga_viewer.ConvertTGADDSToPng();
                data = null;
            }
        }
        private void action_convert_xml(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is JmdFile selFile)
            {
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = selFile.GetBytes();
                BinaryXmlDocument bxd = new BinaryXmlDocument();
                bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                string output = bxd.RootTag.ToString();
                byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = $"{selFile.FullName[0..^3]}.xml";
                sfd.Filter = "XML File|*.xml";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(output_data, 0, output_data.Length);
                    fs.Close();
                }
            }
        }
        #endregion
        #region Other Function
        private string GetCurrentPath()
        {
            if (_curFolder is null)
                return "";
            return _curFolder.FullName;
        }
        private void UpdateUIFolder()
        {
            if (_curFolder is null)
            {
                return;
            }
            List<ListViewItem> temp_list = new List<ListViewItem>();
            this.listview_main.Items.Clear();
            foreach (JmdFolder subFolder in _curFolder.Folders)
            {
                ListViewItem lvi = new ListViewItem(new string[] { subFolder.Name, ("listview_item2_folder").GetStringBag(), $"" });
                lvi.ImageKey = "folder";
                lvi.Tag = subFolder;
                temp_list.Add(lvi);
            }
            foreach (JmdFile subFile in _curFolder.Files)
            {
                ListViewItem lvi = new ListViewItem(new string[] { subFile.Name, ("listview_item2_file").GetStringBag(), $"{FormatDataLength(subFile.Size)}" });
                lvi.ImageKey = "file";
                lvi.Tag = subFile;
                temp_list.Add(lvi);
            }
            this.listview_main.Items.AddRange(temp_list.ToArray());
            this.textbox_path.Text = GetCurrentPath();
            if (_curFolder != _rootFolder)
                this.icon_back.Enabled = true;
            else
                this.icon_back.Enabled = false;
        }
        private void CloseCurrentFile()
        {
            /*
            FolderStack.Clear();
            PathStack.Clear();
            */
            treeview_explorer.Nodes.Clear();
            listview_main.Items.Clear();
            _jmdArchive?.Dispose();
            _jmdArchive = null;
            _curFolder = _rootFolder = null;
        }
        private string FormatDataLength(int length)
        {
            string[] units = { "Bytes", "KiB", "MiB" };
            double dlen = length;
            foreach (string unit in units)
            {
                if (dlen > 1024)
                    dlen /= 1024;
                else
                    return $"{dlen: 0.00} {unit}";
            }
            return $"{dlen} {units[units.Length - 1]}";
        }
        #endregion

        #region StartupSetting
        public class StartupOption
        {
            public string FileName { get; set; } = "";
            public string DataFolderPath { get; set; } = "";
        }
        #endregion
    }
}
