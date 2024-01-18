using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.IO
{
    public abstract class RaycityObject
    {
        protected RaycityObject() 
        {
            
        }
        public virtual string ClassName => this.GetType().Name;

        public uint ClassStamp
        {
            get
            {
                byte[] classNameEnc = Encoding.UTF8.GetBytes(ClassName);
                return Adler.Adler32(0, classNameEnc, 0, classNameEnc.Length);
            }
        }

        public virtual void DecodeObject(BinaryReader reader, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap) 
        {
            
        }
        public virtual void EncodeObject(BinaryWriter writer, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object>? decodedFieldMap)
        {

        }
    }
}
