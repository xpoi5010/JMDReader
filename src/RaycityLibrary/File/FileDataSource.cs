using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public class FileDataSource: IDataSource
    {
        private string _fileName;
        private FileStream _stream;
        private bool _locked;
        private int _size;

        private bool _disposed;

        public bool Locked => _locked;

        public int Size => _size;

        
        public FileDataSource(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException("file not found", fileName);
            _fileName = fileName;
            _stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _size = (int)_stream.Length;
            _locked = false;
            _disposed = false;
        }

        
        public Stream CreateStream()
        {
            return new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void WriteTo(Stream stream)
        {
            _stream.CopyTo(stream);
        }

        public void WriteTo(byte[] buffer, int offset, int count)
        {
            _stream.Read(buffer, offset, count);
        }

        public byte[] GetBytes()
        {
            byte[] output = new byte[_size];
            _stream.Read(output);
            return output;
        }

        public void Dispose()
        {
            _stream.Dispose();
            _disposed = true;
        }
    }
}
