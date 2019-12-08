﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.IO.File;
using Raycity.Crypt;
using Ionic.Zlib;

namespace Raycity.File
{
    public class JMDFile
    {
        public string Path { get; set; }

        public JMDHeader Header { get; set; }

        public List<JMDStreamInfo> StreamInfos { get; set; }

        public IPackedObject[] JMDPackedFiles { get; set; }

        public uint HeaderKey { get; set; }

        public bool FixedMode { get; private set; }

        public JMDPackedFolderInfo ParentFolder { get; set; }

        private Stack<JMDPackedFolderInfo> FolderStack = new Stack<JMDPackedFolderInfo>();

        public JMDPackedFolderInfo NowFolder => FolderStack.Peek();

        public IPackedObject[] NowFolderContent { get; set; }

        private Stack<string> PathStack = new Stack<string>();

        public string NowPath => PathStack.Count == 1? "/": string.Join("/", PathStack.ToArray().Reverse());

        public JMDFile(string path)
        {
            if (!Exists(path))
                throw new FileNotFoundException($"File:{path} can not be found.");
            Path = path;
            FileInfo fi = new FileInfo(path);
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            string FileName = fi.Extension != "" ? fi.Name.Replace(fi.Extension, "") : fi.Name;
            HeaderKey = KeyGenerator.GetHeaderKey(FileName);
            byte[] block1 = br.ReadBytes(0x80);
            if (!CheckFile(block1))
                throw new Exception($"File:{path} is not a correct JMDFile,check if this file is changed.");
            byte[] block2 = br.ReadBytes(0x80);
            Header = new JMDHeader(block2, HeaderKey);
            if (!Header.IsCorrectJMDFile)
            {
                HeaderKey = (BitConverter.ToUInt32(block2, 4) ^ 0x100) + 0x7b8c043f ^ 0x8473fbc1;
                Header = new JMDHeader(block2, HeaderKey);
                FixedMode = true;
            }
            uint Block3Count = Header.StreamInfoCount;
            StreamInfos = new List<JMDStreamInfo>();
            for(int i = 0; i < Block3Count; i++)
            {
                byte[] bk3 = br.ReadBytes(0x20);
                StreamInfos.Add(new JMDStreamInfo(bk3, Header.StreamInfosKey));
            }
            JMDStreamInfo bk4Info = GetStreamInfo(0xFFFFFFFF);
            fs.Seek(bk4Info.Offset, SeekOrigin.Begin);
            byte[] bk4 = new byte[bk4Info.Size];
            fs.Read(bk4, 0, bk4.Length);
            NowFolderContent = JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(bk4, HeaderKey,0xFFFFFFFF);
            this.FolderStack.Push(new JMDPackedFolderInfo()
            {
                FolderName = "",
                Index = 0xFFFFFFFF,
                ParentIndex = 0
            });
            PathStack.Push("");
            fs.Close();
        }

        public JMDStreamInfo GetStreamInfo(uint index)
        {
            return StreamInfos.Find(x => x.Index == index);
        }

        
        private bool CheckFile(byte[] data)
        {
            using(MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader br = new BinaryReader(ms);
                short a = br.ReadInt16();
                List<char> temp_str = new List<char>();
                while(a != 0x00)
                {
                    temp_str.Add((char)a);
                    a = br.ReadInt16();
                }
                string str = new string(temp_str.ToArray());
                return str == "J2m Data Format 1.0";
            }
        }

        public byte[] GetPackedFile(JMDPackedFileInfo info)
        {
            FileStream fs = new FileStream(Path, FileMode.Open);
            if(info.CryptMode == CryptMode.None)
            {
                JMDStreamInfo streaminfo = this.GetStreamInfo(info.Index);
                fs.Seek(streaminfo.Offset, SeekOrigin.Begin);
                byte[] data = new byte[streaminfo.Size];
                fs.Read(data, 0, data.Length);
                fs.Close();
                return data;
            }
            else if(info.CryptMode == CryptMode.FullCryption)
            {
                JMDStreamInfo streaminfo = this.GetStreamInfo(info.Index);
                fs.Seek(streaminfo.Offset, SeekOrigin.Begin);
                byte[] data = new byte[streaminfo.Size];
                fs.Read(data, 0, data.Length);
                fs.Close();
                uint key = Crypt.KeyGenerator.GetDataKey(HeaderKey,info);
                data = JMDCrypt.Decrypt(data, key);
                return data;
            }
            else if(info.CryptMode == CryptMode.PartCryption)
            {
                List<byte> output = new List<byte>();
                JMDStreamInfo streaminfo = this.GetStreamInfo(info.Index);
                fs.Seek(streaminfo.Offset, SeekOrigin.Begin);
                byte[] data = new byte[streaminfo.Size];
                fs.Read(data, 0, data.Length);
                uint key = Crypt.KeyGenerator.GetDataKey(HeaderKey, info);
                data = JMDCrypt.Decrypt(data, key);
                output.AddRange(data);
                streaminfo = this.GetStreamInfo(info.Index + 1);
                fs.Seek(streaminfo.Offset, SeekOrigin.Begin);
                data = new byte[streaminfo.Size];
                fs.Read(data, 0, data.Length);
                output.AddRange(data);
                fs.Close();
                return output.ToArray();
            }
            else
            {
                fs.Close();
                throw new Exception("Unknown CryptMode.");
            }
        }

        public byte[] GetStreamData(uint index)
        {
            JMDStreamInfo info = GetStreamInfo(index);
            FileStream fs = new FileStream(Path, FileMode.Open);
            fs.Seek(info.Offset, SeekOrigin.Begin);
            byte[] data = new byte[(int)info.Size];
            fs.Read(data, 0, (int)info.Size);
            fs.Close();
            return data;
        }

        public IPackedObject[] GetInFolderObject(JMDPackedFolderInfo info)
        {
            return JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(GetStreamData(info.Index), HeaderKey,info.Index);
        }

        public bool EnterToFolder(string FolderName)
        {
            JMDPackedFolderInfo FolderInfo = (JMDPackedFolderInfo)Array.Find(NowFolderContent, x => x.Type == ObjectType.Folder && ((JMDPackedFolderInfo)x).FolderName == FolderName);
            if (FolderName is null)
                return false;
            this.ParentFolder = this.NowFolder;
            this.NowFolderContent = JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(GetStreamData(FolderInfo.Index), this.HeaderKey, this.NowFolder.Index);
            this.FolderStack.Push(FolderInfo);
            this.PathStack.Push(FolderInfo.FolderName);
            return true;
        }

        public bool BackToParentFolder()
        {
            if (NowFolder.Index == 0xFFFFFFFF)
                return false;
            JMDPackedFolderInfo nowFolder = this.FolderStack.Pop();
            this.NowFolderContent = JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(GetStreamData(nowFolder.ParentIndex), this.HeaderKey, nowFolder.ParentIndex);
            this.PathStack.Pop();
            return true;
        }

        
    }
}
