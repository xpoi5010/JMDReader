using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Raycity.Xml;
using System.Numerics;

namespace Raycity.IO
{
    public static class BinaryReaderExt
    {
        public static string ReadText(this BinaryReader reader)
        {
            int count = reader.ReadInt32() << 1;
            byte[] data = reader.ReadBytes(count);
            return Encoding.Unicode.GetString(data);
        }

        public static string ReadText(this BinaryReader br, Encoding encoding, int Count)
        {
            byte[] data = br.ReadBytes(Count);
            return encoding.GetString(data);
        }

        public static string ReadText(this BinaryReader br, Encoding encoding)
        {
            int count = br.ReadInt32() << 1;
            byte[] data = br.ReadBytes(count);
            return encoding.GetString(data);
        }

        public static BinaryXmlTag ReadBinaryXmlTag(this BinaryReader br, Encoding encoding)
        {
            BinaryXmlTag tag = new BinaryXmlTag();
            tag.Name = br.ReadText(encoding);
            //Text
            tag.Text = br.ReadText(encoding);
            //Attributes
            int attCount = br.ReadInt32();
            for (int i = 0; i < attCount; i++)
                tag.SetAttribute(br.ReadText(encoding), br.ReadText(encoding));
            //SubTags
            int SubCount = br.ReadInt32();
            for (int i = 0; i < SubCount; i++)
                tag.Children.Add(br.ReadBinaryXmlTag(encoding));
            return tag;
        }

        public static string ReadNullTerminatedText(this BinaryReader br, bool wideString)
        {
            StringBuilder stringBuilder = new StringBuilder(16);
            if (wideString)
            {
                char ch;
                while ((ch = (char)br.ReadInt16()) != '\0')
                    stringBuilder.Append(ch);
            }
            else
            {
                char ch;
                while ((ch = (char)br.ReadByte()) != '\0')
                    stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            return new Vector2(x, y);
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            return new Vector3(x, y, z);
        }

        public static Vector4 ReadVector4(this BinaryReader br)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            float w = br.ReadSingle();
            return new Vector4(x, y, z, w);
        }
    }

    public static class RaycityObjectExt
    {
        public static RaycityObject? ReadRaycityObject(this BinaryReader br, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap)
        {
            uint classStamp;
            RaycityObject output;
            if (decodedObjectMap is not null)
            {
                int isnull = br.ReadUInt16();
                if(isnull == 0x47BB)
                {
                    short objIndex = br.ReadInt16();
                    if (!decodedObjectMap.ContainsKey(objIndex))
                        throw new IndexOutOfRangeException();
                    output = decodedObjectMap[objIndex];
                }
                else
                {
                    classStamp = br.ReadUInt32();
                    short objIndex = br.ReadInt16();
                    output = RaycityObjectManager.CreateObject(classStamp);
                    output?.DecodeObject(br, decodedObjectMap, decodedFieldMap);
                    decodedObjectMap.Add(objIndex, output);
                }
            }
            else
            {
                classStamp = br.ReadUInt32();
                output = RaycityObjectManager.CreateObject(classStamp);
                output?.DecodeObject(br, decodedObjectMap, decodedFieldMap);
            }
            return output;
        }

        public static TBase ReadRaycityObject<TBase>(this BinaryReader br, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap) where TBase : RaycityObject, new()
        {
            uint classStamp;
            TBase output;
            if (decodedObjectMap is not null)
            {
                int isnull = br.ReadUInt16();
                if (isnull == 0x47BB)
                {
                    short objIndex = br.ReadInt16();
                    if (!decodedObjectMap.ContainsKey(objIndex))
                        throw new IndexOutOfRangeException();
                    if(decodedObjectMap[objIndex] is TBase decTBase)
                        output = decTBase;
                    else
                        throw new InvalidCastException();
                }
                else
                {
                    classStamp = br.ReadUInt32();
                    short objIndex = br.ReadInt16();
                    output = RaycityObjectManager.CreateObject<TBase>(classStamp);
                    output?.DecodeObject(br, decodedObjectMap, decodedFieldMap);
                    decodedObjectMap.Add(objIndex, output);
                }
            }
            else
            {
                classStamp = br.ReadUInt32();
                output = RaycityObjectManager.CreateObject<TBase>(classStamp);
                output?.DecodeObject(br, decodedObjectMap, decodedFieldMap);
            }
            return output;
        }

        // AA27 BB27
        public static T ReadField<T>(this BinaryReader br, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap, DecodeFieldFunc<T> decodeFieldFunc)
        {
            if(decodedFieldMap is not null)
            {
                ushort token = br.ReadUInt16();
                if (token == 0x27AA)
                {
                    short fieldObjIndex = br.ReadInt16();
                    T decodedField = decodeFieldFunc(br, decodedObjectMap, decodedFieldMap);
                    decodedFieldMap.Add(fieldObjIndex, decodedField);
                    return decodedField;
                }
                else if (token == 0x27BB)
                {
                    short fieldObjIndex = br.ReadInt16();
                    if (decodedFieldMap.ContainsKey(fieldObjIndex))
                        if (decodedFieldMap[fieldObjIndex] is T outField)
                            return outField;
                        else
                            throw new InvalidCastException();
                    else
                        throw new IndexOutOfRangeException();
                }
                else
                    throw new Exception();
            }
            else
            {
                return decodeFieldFunc(br, decodedObjectMap, decodedFieldMap);
            }
        }
    }

    public delegate T DecodeFieldFunc<T>(BinaryReader reader, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap);
}
