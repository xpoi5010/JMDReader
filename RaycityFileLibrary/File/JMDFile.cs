using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.IO.File;
using Raycity.Crypt;
using Ionic.Zlib;
using Raycity.File.Modify;

namespace Raycity.File
{
    public class JMDFile
    {
        public string Path { get; set; }

        public JMDHeader Header { get; set; }

        public List<JMDStreamInfo> StreamInfos { get; set; }

        public uint HeaderKey { get; set; }

        public bool FixedMode { get; private set; }

        public JMDPackedFolderInfo ParentFolder { get; set; }

        private Stack<JMDPackedFolderInfo> FolderStack = new Stack<JMDPackedFolderInfo>();

        public JMDPackedFolderInfo NowFolder => FolderStack.Peek();

        public IPackedObject[] NowFolderContent { get; set; }

        private Stack<string> PathStack = new Stack<string>();

        public string NowPath => PathStack.Count == 1? "/": string.Join("/", PathStack.ToArray().Reverse());

        public List<ModifiedFileInfo> ModifiedFileInfos = new List<ModifiedFileInfo>();

        public const string HeaderString = "J2m Data Format 1.0";

        public const string DescriptString = "j2m & raycity fighting!!";

        public JMDFile(string path)
        {
            Load(path);
        }

        private void Load(string path)
        {
            if (!Exists(path))
                throw new FileNotFoundException($"File:{path} can not be found.");
            //initialization
            FolderStack = new Stack<JMDPackedFolderInfo>();
            PathStack = new Stack<string>();
            ModifiedFileInfos = new List<ModifiedFileInfo>();
            //
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
            for (int i = 0; i < Block3Count; i++)
            {
                byte[] bk3 = br.ReadBytes(0x20);
                StreamInfos.Add(new JMDStreamInfo(bk3, Header.StreamInfosKey));
            }
            JMDStreamInfo bk4Info = GetStreamInfo(0xFFFFFFFF);
            fs.Seek(bk4Info.Offset, SeekOrigin.Begin);
            byte[] bk4 = new byte[bk4Info.Size];
            fs.Read(bk4, 0, bk4.Length);
            NowFolderContent = JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(bk4, HeaderKey, 0xFFFFFFFF);
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

        public byte[] GetStreamData(uint index, FileStream fs)
        {
            JMDStreamInfo info = GetStreamInfo(index);
            fs.Seek(info.Offset, SeekOrigin.Begin);
            byte[] data = new byte[(int)info.Size];
            fs.Read(data, 0, (int)info.Size);
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

        //Modify Functions
        //Version 1.1
        
        public void ModifyDataStream(string FileName,string LocalFile)
        {
            JMDPackedFileInfo fileInfo = (JMDPackedFileInfo)Array.Find(NowFolderContent, x => x.Type == ObjectType.File && ((JMDPackedFileInfo)x).FileName == FileName);
            if (fileInfo == null)
                throw new FileNotFoundException($"File: {FileName} is not found in \"{NowPath}\" folder.");
            int StreamIndex = StreamInfos.FindIndex(x => x.Index == fileInfo.Index);
            FileStream fs = new FileStream(LocalFile, FileMode.Open);
            StreamInfos[StreamIndex].StreamMode = StreamMode.Local;
            switch (fileInfo.CryptMode)
            {
                case CryptMode.None:
                    StreamInfos[StreamIndex].StreamAddition = new LocalStreamAddition
                    {
                        FileName = LocalFile,
                        Key = 0x00,
                        Length = (int)fs.Length,
                        Offset = 0x00
                    };
                    StreamInfos[StreamIndex].Size = (uint)fs.Length;
                    StreamInfos[StreamIndex].Size2 = (uint)fs.Length;
                    break;
                case CryptMode.PartCryption:
                    if (fs.Length <= 0x100)
                        throw new NotSupportedException("This mode is not supported now,Sorry.");
                    StreamInfos[StreamIndex].StreamAddition = new LocalStreamAddition
                    {
                        FileName = LocalFile,
                        Key = KeyGenerator.GetDataKey(HeaderKey,fileInfo),
                        Length = 0x100,
                        Offset = 0x00
                    };
                    StreamInfos[StreamIndex+1].StreamAddition = new LocalStreamAddition
                    {
                        FileName = LocalFile,
                        Key = 0x00,
                        Length = (int)fs.Length-0x100,
                        Offset = 0x100
                    };
                    StreamInfos[StreamIndex].Size = 0x100;
                    StreamInfos[StreamIndex].Size2 = 0x100;
                    StreamInfos[StreamIndex + 1].Size = (uint)fs.Length - 0x100;
                    StreamInfos[StreamIndex + 1].Size2 = (uint)fs.Length - 0x100;
                    StreamInfos[StreamIndex+1].StreamMode = StreamMode.Local;
                    break;
                case CryptMode.FullCryption:
                    StreamInfos[StreamIndex].StreamAddition = new LocalStreamAddition
                    {
                        FileName = LocalFile,
                        Key = KeyGenerator.GetDataKey(HeaderKey, fileInfo),
                        Length = (int)fs.Length,
                        Offset = 0x00
                    };
                    StreamInfos[StreamIndex].Size = (uint)fs.Length;
                    StreamInfos[StreamIndex].Size2 = (uint)fs.Length;
                    break;
            }
            ModifiedFileInfos.Add(new ModifiedFileInfo()
            {
                FileIndex = fileInfo.Index,
                FolderIndex = NowFolder.Index,
                NewFileSize = (uint)fs.Length

            });
            fs.Close();

            
        }

        //AutoClose!!!
        public void ApplyModification(string File)
        {
            string tempS = "";
            bool tempMode = false;
            if (Path == File)
            {
                tempS = File;
                string newPath = $"{Path}.temp{DateTime.Now.ToString("yyyyMMddhhmmssffff")}";
                File = newPath;
                tempMode = true;
            }
            FileStream fs = new FileStream(File, FileMode.Create);
            FileStream jmdFileOrg = new FileStream(Path, FileMode.Open);
            byte[] headerString = Encoding.GetEncoding("UTF-16").GetBytes(HeaderString);
            byte[] DescriptString = Encoding.GetEncoding("UTF-16").GetBytes(JMDFile.DescriptString);
          //offset:0x00 size:0x80
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(headerString,0,headerString.Length);
            fs.Seek(0x40, SeekOrigin.Begin);
            fs.Write(DescriptString, 0, DescriptString.Length);
            fs.Seek(0x80, SeekOrigin.Begin);
            //FileDataWriter
            byte[] data2;
            int offset = GetNextOffset(0,0x100 + (0x20 * StreamInfos.Count));
            int firstOffset = offset;
            using (MemoryStream ms = new MemoryStream())
            {
                for(int i =0;i<StreamInfos.Count;i++)
                {
                    ms.Seek(offset - firstOffset, SeekOrigin.Begin);
                    JMDStreamInfo streamInfo = StreamInfos[i];
                    int tempOffset = offset;
                    switch (streamInfo.StreamMode)
                    {
                        case StreamMode.JMDFile:
                            if (IsChangedFolder(streamInfo.Index))
                            {
                                IPackedObject[] objects = JMDPackedFilesInfoDecoder.GetJMDPackedFileInfos(GetStreamData(streamInfo.Index, jmdFileOrg), this.HeaderKey, this.NowFolder.Index);
                                List<ModifiedFileInfo> needModify = ModifiedFileInfos.FindAll(x => x.FolderIndex == streamInfo.Index);
                                for(int a = 0; a < objects.Length; a++)
                                {
                                    int indexAAA = needModify.FindIndex(x => x.FileIndex == ((JMDPackedFileInfo)objects[a]).Index);
                                    if (indexAAA != -1)
                                    {
                                        JMDPackedFileInfo objT = objects[a] as JMDPackedFileInfo;
                                        objT.FileSize = (int)needModify[indexAAA].NewFileSize;
                                     }
                                }
                                uint hash = 0;
                                byte[] newD = JMDPackedFilesInfoDecoder.ToByteArray(objects, HeaderKey,out hash);
                                ms.Write(newD, 0, newD.Length);
                                StreamInfos[i].Size = (uint)newD.Length;
                                StreamInfos[i].Size2 = (uint)newD.Length;
                                StreamInfos[i].Hash = hash;
                                offset = GetNextOffset(offset, (int)newD.Length);
                            }
                            else
                            {
                                jmdFileOrg.Seek(streamInfo.Offset,SeekOrigin.Begin);
                                byte[] dataT = new byte[streamInfo.Size];
                                jmdFileOrg.Read(dataT, 0, dataT.Length);
                                ms.Write(dataT, 0, dataT.Length);
                                offset = GetNextOffset(offset, (int)dataT.Length);
                            }
                            break;
                        case StreamMode.Local:
                            LocalStreamAddition lsa = streamInfo.StreamAddition as LocalStreamAddition;
                            FileStream localFile = new FileStream(lsa.FileName, FileMode.Open);
                            localFile.Seek(lsa.Offset, SeekOrigin.Begin);
                            byte[] dataT2 = new byte[lsa.Length];
                            localFile.Read(dataT2, 0, dataT2.Length);
                            localFile.Close();
                            if (streamInfo.NeedCrypt)
                                dataT2 = JMDCrypt.Decrypt(dataT2, lsa.Key);
                            ms.Write(dataT2, 0, dataT2.Length);
                            offset = GetNextOffset(offset, (int)dataT2.Length);
                            break;
                    }
                    StreamInfos[i].Offset = (uint)tempOffset;
                }
                data2 = ms.ToArray();
            }
            //offset:0x80 size:0x80
            byte[] b2 = Header.ToByteArray(HeaderKey);
            fs.Write(b2, 0, b2.Length);
            //offset:0x100 size:?
            foreach (JMDStreamInfo jsi in StreamInfos)
            {
                byte[] atrq = jsi.ToByteArray(Header.StreamInfosKey);
                fs.Write(atrq, 0, atrq.Length);
            }
            //
            fs.Seek(firstOffset, SeekOrigin.Begin);
            fs.Write(data2, 0, data2.Length);
            byte[] zzzzz = new byte[offset - fs.Position];
            fs.Write(zzzzz,0,zzzzz.Length);
            fs.Close();
            jmdFileOrg.Close();
            //Reload
            if (tempMode)
            {
                Delete(Path);
                FileInfo fi = new FileInfo(File);
                fi.MoveTo(tempS);
                File = tempS;
            }
            Load(File);
            
        }

        private int GetNextOffset(int nowOffset,int fileSize)
        {
            int nextOffset = nowOffset + fileSize;
            if ((nextOffset & 0xFF) != 0)
                nextOffset += 0x100;
            nextOffset >>= 8;
            nextOffset <<= 8;
            return nextOffset;
        }

        private bool IsChangedFolder(uint index)
        {
            return ModifiedFileInfos.Exists(x => x.FolderIndex == index);
        }
    }
}
