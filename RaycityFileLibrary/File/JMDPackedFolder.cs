using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Raycity.File
{
    public class JMDPackedFolderInfo : IPackedObject
    {
        public string FolderName { get; set; }

        public uint Index { get; set; }

        public ObjectType Type => ObjectType.Folder;

        public uint ParentIndex { get; set; }

        public JMDPackedFolderInfo()
        {

        }
    }
}
