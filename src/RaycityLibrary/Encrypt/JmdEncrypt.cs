using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Raycity.Encrypt
{
    public static class JmdEncrypt
    {

        /// <summary>
        /// Used to decrypt rho file data, or DataProcessed data.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static byte[] DecryptData(uint Key,byte[] Data)
        {
            byte[] extendedKey = JmdKey.ExtendKey(Key);
            byte[] output = new byte[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                output[i] = (byte)(Data[i] ^ extendedKey[i & 63]);
            }
            return output;
        }

        public unsafe static void DecryptData(uint Key,byte[] Data,int Offset,int Length)
        {
            if ((Offset + Length) > Data.Length)
                throw new Exception("Over range.");
            byte[] extendedKey = JmdKey.ExtendKey(Key);
            for (int i = 0; i < Length; i++)
            {
                int index = i + Offset;
                Data[index] = (byte)(Data[index] ^ extendedKey[index & 63]);
            }
            
        }

        /// <summary>
        /// Used to decrypt rho header, and block info.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        
        public static byte[] DecryptDataInfo(byte[] Data,byte[] key)
        {
            if (Data.Length != 0x20)
                throw new NotSupportedException("Exception: the length of Data is not 32 bytes.");
            byte[] output = new byte[32];
            for (int i = 0; i < 32; i++)
                output[i] =(byte)( key[i] ^ Data[i]);
            return output;
        }

        public unsafe static void DecryptDataInfo(byte[] key, byte[] data, int offset, int length)
        {
            if (data.Length != 0x20)
                throw new NotSupportedException("Exception: the length of Data is not 32 bytes.");
            fixed (byte* ptr = &data[offset])
                for (int i = 0; i < 32; i++)
                    ptr[i] = (byte)(key[i] ^ ptr[i]);
        }

        /// <summary>
        /// Used to encrypt rho file data, or DataProcessed data.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static byte[] EncryptData(uint Key, byte[] Data)
        {
            byte[] extendedKey = JmdKey.ExtendKey(Key);
            byte[] output = new byte[Data.Length];
            for(int i =0;i<Data.Length;i++)
            {
                output[i] = (byte)(Data[i] ^ extendedKey[i & 63]);
            }
            return output;
        }

        public static void EncryptData(uint Key, byte[] Data, int Offset, int Length)
        {
            if ((Offset + Length) > Data.Length)
                throw new Exception("Over range.");
            byte[] extendedKey = JmdKey.ExtendKey(Key);
            for (int i = 0; i < Length; i++)
            {
                int index = i + Offset;
                Data[index] = (byte)(Data[index] ^ extendedKey[index & 63]);
            }
        }

        /// <summary>
        /// Used to encrypt rho header, and block info.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static byte[] EncryptDataInfo(byte[] Data, byte[] key)
        {
            if (Data.Length != 0x20)
                throw new NotSupportedException("Exception: the length of Data is not 32 bytes.");
            byte[] output = new byte[32];
            for (int i = 0; i < 32; i++)
                output[i] = (byte)(key[i] ^ Data[i]);
            return output;
        }

        public unsafe static void EncryptDataInfo(byte[] key, byte[] data, int offset, int length)
        {
            if (data.Length != 0x20)
                throw new NotSupportedException("Exception: the length of Data is not 32 bytes.");
            fixed(byte* ptr = &data[offset])
                for (int i = 0; i < 32; i++)
                    ptr[i] = (byte)(key[i] ^ ptr[i]);
        }
    }
}
