using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Raycity.Xml;

namespace Raycity.IO
{
    public static class BinaryWriterExt
    {
        public static void WriteString(this BinaryWriter br, Encoding encoding, string Text)
        {
            byte[] data = encoding.GetBytes(Text);
            br.Write(Text.Length);
            br.Write(data);
            data = null;
        }

        public static void Write(this BinaryWriter br, Encoding encoding, string Key, string Value)
        {
            br.WriteString(encoding, Key);
            br.WriteString(encoding, Value);
        }

        public static void WriteKRString(this BinaryWriter bw, string str)
        {
            int len = str.Length;
            byte[] strData = Encoding.GetEncoding("UTF-16").GetBytes(str);
            bw.Write(len);
            bw.Write(strData);
        }

        public static void WriteNullTerminatedText(this BinaryWriter br, string text, bool wideString)
        {
            if (!wideString)
            {
                byte[] encData = Encoding.ASCII.GetBytes(text);
                br.Write(encData);
                br.Write((byte)0x00);
            }
            else
            {
                byte[] encData = Encoding.Unicode.GetBytes(text);
                br.Write(encData);
                br.Write((short)0x00);
            }
        }
    }
}
