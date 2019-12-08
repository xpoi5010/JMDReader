using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.Crypt
{
    public static class JMDCrypt
    {
        public static byte[] Decrypt(byte[] data,uint Key)
        {
            byte[] longkey = GetLongKey(Key);
            byte[] output = new byte[data.Length];
            int b = 0;
            for(int i = 0; i < data.Length; i++)
            {
                output[i] = (byte)(data[i]^longkey[b]);
                b++;
                if (b >= 64)
                    b = 0;
            }
            return output;
        }

        private static byte[] GetLongKey(uint OriginalKey)
        {
            List<byte> output = new List<byte>();
            uint a = OriginalKey ^ 0x8473fbc1;
            output.AddRange(BitConverter.GetBytes(a));
            for (int i = 1; i < 16; i++)
            {
               a -= 0x7b8c043f;
               output.AddRange(BitConverter.GetBytes(a));
            }
            return output.ToArray();
        }
    }
}
