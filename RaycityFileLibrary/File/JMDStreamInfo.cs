using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Raycity.File
{
    public class JMDStreamInfo
    {
        public uint Index { get; set; }

        public uint Offset { get; set; }

        public uint Size { get; set; }

        public uint Size2 { get; set; }

        public uint CryptInfomation { get; set; }

        public bool NeedHash => (CryptInfomation & 1) == 1;

        public bool NeedCrypt => (CryptInfomation & 4) == 4;

        //Adler32 Adler:0
        public uint Hash { get; set; }

        public StreamMode StreamMode { get; set; } = StreamMode.JMDFile;

        public object StreamAddition { get; set; }

        public JMDStreamInfo(byte[] data,byte[] key)
        {

            byte[] DecryptData = new byte[data.Length];
            for(int i = 0; i < key.Length; i++)
            {
                DecryptData[i] = (byte)(data[i] ^ key[i]);
            }

            using (MemoryStream ms = new MemoryStream(DecryptData))
            {
                BinaryReader br = new BinaryReader(ms);
                Index = br.ReadUInt32();
                Offset = br.ReadUInt32() * 0x100;
                Size = br.ReadUInt32();
                Size2 = br.ReadUInt32();
                if (Size != Size2)
                    System.Diagnostics.Debugger.Launch();
                CryptInfomation = br.ReadUInt32();
                Hash = br.ReadUInt32();
                if (Hash != 0)
                    System.Diagnostics.Debug.Print($"A:{Hash:000000000000}");
            }
        }

        public byte[] ToByteArray(byte[] key)
        {
            byte[] output;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(Index);
                bw.Write(Offset>>8);
                bw.Write(Size);
                bw.Write(Size2);
                bw.Write(CryptInfomation);
                bw.Write(Hash);
                bw.Write(new byte[8]);
                output = ms.ToArray();
            }
            for (int i = 0; i < key.Length; i++)
            {
                output[i] = (byte)(output[i] ^ key[i]);
            }
            return output;
        }

    }

    public enum StreamMode
    {
        JMDFile,Local
    }

    public class LocalStreamAddition
    {
        public uint Key { get; set; }

        public string FileName { get; set; }

        public int Offset { get; set; }

        public int Length { get; set; }
    }
}
