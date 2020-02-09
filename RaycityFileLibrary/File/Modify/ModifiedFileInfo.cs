using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File.Modify
{
    public class ModifiedFileInfo : IModifiedInfo
    {
        public uint FolderIndex { get; set; }

        public uint FileIndex { get; set; }

        public uint NewFileSize { get; set; }

        //version 1.3 content
        public ModifyAction ModifyAction { get; set; } = ModifyAction.Modify;

        public ModifyType ModifyType => ModifyType.File;
    }

    //version 1.3 content

    public enum ModifyAction
    {
        Add,Modify,Delete
    }

    public enum ModifyType
    {
        Folder,File
    }

    public interface IModifiedInfo
    {
        uint FolderIndex { get; set; }

        ModifyType ModifyType { get; }

        ModifyAction ModifyAction { get; set; }
    }

    public class ModifiedFolderInfo : IModifiedInfo
    {
        public uint FolderIndex { get; set; }

        public ModifyType ModifyType => ModifyType.Folder;

        public ModifyAction ModifyAction { get; set; }
    }
}
