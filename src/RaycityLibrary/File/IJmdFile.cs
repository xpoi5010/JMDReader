using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public interface IJmdFile : IDisposable
    {
        IJmdFolder? Parent { get; }

        string Name { get; set; }

        string FullName { get; }

        int Size { get; }

        IDataSource? DataSource { set; }

        Stream CreateStream();

        void WriteTo(Stream stream);

        void WriteTo(byte[] array, int offset, int count);

        byte[] GetBytes();
    }
}
