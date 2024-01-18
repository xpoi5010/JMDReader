using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public interface IJmdFolder : IDisposable
    {
        IJmdFolder? Parent { get; }

        string Name { get; }

        IEnumerable<IJmdFile> GetFiles();

        IJmdFile GetFile(string path);

        IEnumerable<IJmdFolder> GetFolders();

        IJmdFolder GetFolder(string path);
    }

    public interface IRhoFolder<T> : IJmdFolder where T : IJmdFile
    {
        new IRhoFolder<T>? Parent { get; }

        new IEnumerable<T> GetFiles();

        new T GetFile(string path);

        new IEnumerable<IRhoFolder<T>> GetFolders();

        new IRhoFolder<T> GetFolder(string path);
    }

}
