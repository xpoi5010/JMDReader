using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public class JmdDataSource : IDataSource
    {
        #region Members
        private bool _disposed;
        private JmdFileHandler _fileHandler;
        #endregion
        
        #region Properties
        public bool Locked => false; // BufferedDataSource don't require lock.

        public int Size => _fileHandler._size;
        #endregion

        #region Constructors
        internal JmdDataSource(JmdFileHandler fileHandler)
        {
            _disposed = false;
            _fileHandler = fileHandler;
        }
        #endregion

        #region Methods
        public Stream CreateStream()
        {
            byte[] data = _fileHandler.getData();
            return new MemoryStream(data, false);
        }

        public void WriteTo(Stream stream)
        {
            if (!stream.CanWrite)
                throw new Exception("This stream is not writeable");
            byte[] data = _fileHandler.getData();
            stream.Write(data, 0, data.Length);
        }

        public void WriteTo(byte[] buffer, int offset, int count)
        {
            if ((buffer.Length - offset) < count)
                throw new IndexOutOfRangeException("given buffer is not enough to store the required data.");
            if(count > _fileHandler._size)
                throw new IndexOutOfRangeException("size is greater than file.");
            byte[] data = _fileHandler.getData();
            Array.Copy(data, 0, buffer, offset, count);
        }

        public byte[] GetBytes()
        {
            byte[] output = new byte[_fileHandler._size];
            byte[] data = _fileHandler.getData();
            Array.Copy(data, output, data.Length);
            return output;
        }

        public void Dispose()
        {
            _disposed = true;
        }
        #endregion
    }
}
