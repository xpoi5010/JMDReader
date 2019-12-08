using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zlib;

namespace Raycity.File
{
    public class JMDHeader
    {
        public byte[] StreamInfosKey;

        public bool IsCorrectJMDFile => Check == 0x100;

        public uint StreamInfoCount;

        public uint Check;

        public uint Hash;

        public uint b;
        public JMDHeader(byte[] data,uint HeaderKey)
        {
            byte[] newData = Crypt.JMDCrypt.Decrypt(data, HeaderKey);
            byte[] _hash_data = new byte[0x7C];
            Array.Copy(newData, 0x04, _hash_data, 0, 0x7C);
            uint Hash = Adler.Adler32(0, _hash_data, 0, _hash_data.Length);
            using (MemoryStream ms = new MemoryStream(newData))
            {
                BinaryReader br = new BinaryReader(ms);
                this.Hash = br.ReadUInt32();
                if (Hash != this.Hash)
                    throw new Exception("*** JMDDecoderError!!! Reason: The hash is not match.");
                Check = br.ReadUInt32();
                this.StreamInfoCount = br.ReadUInt32();
                b = br.ReadUInt32();
                this.StreamInfosKey = br.ReadBytes(32);
            }
        }
    }
}
