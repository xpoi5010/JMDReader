using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public interface IDataSource : IDisposable
    {
        bool Locked { get; }

        int Size { get; }

        Stream CreateStream();

        void WriteTo(Stream stream);

        void WriteTo(byte[] array, int offset, int count);

        byte[] GetBytes();
    }
}
