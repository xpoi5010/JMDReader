using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public class JmdFileHandler
    {
        internal uint _fileDataIndex;
        internal uint _key;
        internal int _size;
        internal JmdFileProperty _fileProperty;

        private JmdArchive _archive;
        private bool _released;
        private bool _disposed;

        internal JmdFileHandler(JmdArchive archive, JmdFileProperty fileProperty, uint fileDataIndex, int size, uint key)
        {
            _fileDataIndex = fileDataIndex;
            _fileProperty = fileProperty;
            _archive = archive;
            _size = size;
            _key = key;

            _released = false;
            _disposed = false;
        }

        internal byte[] getData()
        {
            if (_released)
                throw new Exception("this handle was released.");
            return _archive.getData(this);
        }

        internal void releaseHandler()
        {
            _released = true;
        }
    }
}
