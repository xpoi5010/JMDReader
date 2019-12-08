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

        //Adler32 Adler:0
        public uint Hash { get; set; }

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
            }
        }
    }
}
