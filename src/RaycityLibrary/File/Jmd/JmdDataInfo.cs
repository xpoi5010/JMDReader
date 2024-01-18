using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Raycity.Encrypt;
using Raycity.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace Raycity.File
{
    public class JmdDataInfo: IComparable<JmdDataInfo>
    {
        public uint Index { get; set; }
        public long Offset { get; set; }
        public int DataSize { get; set; }
        public int UncompressedSize { get; set; }
        public JmdDataInfoProperty BlockProperty { get; set; }
        public uint Checksum { get; set; }

        public int CompareTo(JmdDataInfo? other)
        {
            return this.Index.CompareTo(other?.Index);
        }

        public override int GetHashCode()
        {
            return (int)Index;
        }
    }
    //Extension
    public static class JmdBlockReader
    {
        public static JmdDataInfo ReadBlockInfo(this BinaryReader reader, byte[] Key)
        {
            JmdDataInfo output = new JmdDataInfo();
            byte[] blockInfoData = reader.ReadBytes(0x20);
            JmdEncrypt.DecryptDataInfo(Key, blockInfoData, 0, blockInfoData.Length);
            using (MemoryStream ms = new MemoryStream(blockInfoData))
            {
                BinaryReader msReader = new BinaryReader(ms);
                output.Index = msReader.ReadUInt32();
                output.Offset = msReader.ReadUInt32() << 8;
                output.DataSize = msReader.ReadInt32();
                output.UncompressedSize = msReader.ReadInt32();
                output.BlockProperty = (JmdDataInfoProperty)msReader.ReadInt32();
                output.Checksum = msReader.ReadUInt32();
            }
            
            return output;
        }
    }

    public enum JmdDataInfoProperty
    {
        None,
        Compressed = 2,
        PartialEncrypted = 4,
        FullEncrypted = 5,
        CompressedEncrypted = FullEncrypted | Compressed
    }
}
