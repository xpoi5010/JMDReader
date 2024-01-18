using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.IO
{
    public class NamedObject: RaycityObject
    {
        public string Name { get; set; } = "";
        protected NamedObject() 
        {
            
        }
        public override void DecodeObject(BinaryReader reader, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object> decodedFieldMap)
        {
            Name = reader.ReadText();
        }
        public override void EncodeObject(BinaryWriter writer, Dictionary<short, RaycityObject>? decodedObjectMap, Dictionary<short, object> decodedFieldMap)
        {
            writer.WriteKRString(Name);
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}: {Name}";
        }
    }
}
