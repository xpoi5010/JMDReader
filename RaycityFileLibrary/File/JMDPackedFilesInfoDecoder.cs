using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Raycity.File
{
    public static class JMDPackedFilesInfoDecoder
    {
        //CurrentPath : /
        public static IPackedObject[] GetJMDPackedFileInfos(byte[] CryptedData,uint HeaderKey,uint CurrentPathindex)
        {
            uint hash11 = Ionic.Zlib.Adler.Adler32(0, CryptedData, 0, CryptedData.Length);
            System.Diagnostics.Debug.Print($"B:{hash11:000000000000}");
            byte[] DecryptedData = Crypt.JMDCrypt.Decrypt(CryptedData, HeaderKey - 0x41014EBF);
            uint hash12 = Ionic.Zlib.Adler.Adler32(0, DecryptedData, 0, DecryptedData.Length);
            System.Diagnostics.Debug.Print($"C:{hash12:000000000000}");
            List<IPackedObject> files = new List<IPackedObject>();
            using (MemoryStream ms = new MemoryStream(DecryptedData))
            {
                BinaryReader br = new BinaryReader(ms);
                int type1Count = br.ReadInt32();
                for (int i = 1; i <= type1Count; i++)
                {
                    List<char> t_FileName = new List<char>();
                    short a = br.ReadInt16();
                    while (a != 0x00)
                    {
                        t_FileName.Add((char)a);
                        a = br.ReadInt16();
                    }
                    uint index = br.ReadUInt32();
                    string filename = new string(t_FileName.ToArray());
                    files.Add(new JMDPackedFolderInfo()
                    {
                        Index = index,
                        FolderName = filename,
                        ParentIndex = CurrentPathindex
                    }) ;
                }
                int type2Count = br.ReadInt32();
                for (int i = 1; i <= type2Count; i++)
                {
                    List<char> t_FileName = new List<char>();
                    short a = br.ReadInt16();
                    while (a != 0x00)
                    {
                        t_FileName.Add((char)a);
                        a = br.ReadInt16();
                    }
                    uint ext = br.ReadUInt32();
                    int cm = br.ReadInt32();
                    uint index = br.ReadUInt32();
                    int fileSize = br.ReadInt32();//FileSize
                    string filename = new string(t_FileName.ToArray());
                    files.Add(new JMDPackedFileInfo()
                    {
                        CryptMode = (CryptMode)cm,
                        Ext = ext,
                        FileName = filename,
                        Index = index,
                        Path = CurrentPathindex,
                        FileSize = fileSize
                    });
                }
            }
            return files.ToArray();
        }
    
        public static byte[] ToByteArray(IPackedObject[] objects, uint HeaderKey,out uint Hash)
        {
            List<JMDPackedFolderInfo> Folders = new List<JMDPackedFolderInfo>();
            List<JMDPackedFileInfo> Files = new List<JMDPackedFileInfo>();
            foreach(IPackedObject obj in objects)
            {
                if (obj.Type == ObjectType.Folder)
                    Folders.Add((JMDPackedFolderInfo)obj);
                else if (obj.Type == ObjectType.File)
                    Files.Add((JMDPackedFileInfo)obj);
            }
            byte[] output;
            using(MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                //Folders
                bw.Write(Folders.Count);
                foreach (JMDPackedFolderInfo folder in Folders)
                {
                    byte[] fnb = Encoding.GetEncoding("UTF-16").GetBytes(folder.FolderName);
                    bw.Write(fnb);
                    bw.Write((short)0x00);
                    bw.Write(folder.Index);
                }
                //Files
                bw.Write(Files.Count);
                foreach(JMDPackedFileInfo file in Files)
                {
                    byte[] fnb = Encoding.GetEncoding("UTF-16").GetBytes(file.FileName);
                    bw.Write(fnb);
                    bw.Write((short)0x00);
                    /*
                    uint ext = br.ReadUInt32();
                    int cm = br.ReadInt32();
                    uint index = br.ReadUInt32();
                    int fileSize = br.ReadInt32();//FileSize
                     */
                    bw.Write(file.Ext);
                    bw.Write((int)file.CryptMode);
                    bw.Write(file.Index);
                    bw.Write(file.FileSize);
                }
                output = ms.ToArray();
            }
            Hash = Ionic.Zlib.Adler.Adler32(0, output, 0,output.Length);
            output = Crypt.JMDCrypt.Decrypt(output, HeaderKey - 0x41014EBF);
            return output;
        }
    }
}
