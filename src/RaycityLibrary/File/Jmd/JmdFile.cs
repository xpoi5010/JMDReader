using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raycity.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Raycity.File
{
    public class JmdFile : IJmdFile
    {
        #region Members
        internal JmdFolder? _parentFolder;

        private string _name;
        private string _nameWithoutExt;
        private string _fullname;
        private uint? _extNum;
        private uint? _dataIndexBase;
        private JmdFileProperty _fileProperty;
        private IDataSource? _dataSource;

        private bool _disposed;
        #endregion

        #region Properties
        public JmdFolder? Parent => _parentFolder;

        IJmdFolder? IJmdFile.Parent => Parent;

        public string Name 
        {
            get => _name;
            set
            {
                _name = value;
                _extNum = null;
                _dataIndexBase = null;
                Regex fileNamePattern = new Regex(@"^(.*)\..*");
                Match match = fileNamePattern.Match(_name);
                if (match.Success)
                {
                    _nameWithoutExt = match.Groups[1].Value;
                }
                else
                {
                    _nameWithoutExt = _name;
                }
            }
        }

        public string FullName
        {
            get => Parent is not null ? $"{Parent.FullName}/{_name}" : _name;
        }

        public string NameWithoutExt => _nameWithoutExt;

        public int Size => _dataSource?.Size ?? 0;

        public IDataSource? DataSource
        {
            set => _dataSource = value;
        }

        public bool HasDataSource => _dataSource != null;

        public JmdFileProperty FileEncryptionProperty
        {
            get => _fileProperty; 
            set => _fileProperty = value;
        }
        #endregion

        #region Constructors
        public JmdFile()
        {
            _parentFolder = null;
            _name = "";
            _fullname = "";
            _dataSource = null;
            _nameWithoutExt = "";
        }
        #endregion

        #region Methods
        public Stream CreateStream()
        {
            if (_dataSource is null)
                throw new InvalidOperationException("DataSource is null.");
            return _dataSource.CreateStream();
        }

        public void WriteTo(Stream stream)
        {
            if (_dataSource is null)
                throw new InvalidOperationException("DataSource is null.");
            _dataSource.WriteTo(stream);
        }

        public void WriteTo(byte[] array, int offset, int count)
        {
            if (_dataSource is null)
                throw new InvalidOperationException("DataSource is null.");
            _dataSource.WriteTo(array, offset, count);
        }

        public byte[] GetBytes()
        {
            if (_dataSource is null)
                throw new InvalidOperationException("DataSource is null.");
            return _dataSource.GetBytes();
        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }
        }

        internal uint getExtNum()
        {
            if(_extNum is null)
            {
                string[] spiltStrs = _name.Split('.');
                if(spiltStrs.Length > 0)
                {
                    string ext = spiltStrs[^1];
                    byte[] extEncData = Encoding.ASCII.GetBytes(ext);
                    byte[] extNumEncData = new byte[4];
                    Array.Copy(extEncData, extNumEncData, Math.Min(4, extEncData.Length));
                    _extNum = BitConverter.ToUInt32(extNumEncData);
                }
                else
                {
                    _extNum = 0;
                }
            }
            return _extNum.Value;
        }

        internal uint getDataIndex(uint folderDataIndex)
        {
            if (_dataIndexBase is null)
            {
                byte[] fileNameEncData = Encoding.Unicode.GetBytes(_nameWithoutExt);
                uint fileNameChksum = Adler.Adler32(0, fileNameEncData, 0, fileNameEncData.Length);
                uint extNum = getExtNum();
                _dataIndexBase = fileNameChksum + extNum;
            }
            if (folderDataIndex == 0xFFFFFFFFu)
                folderDataIndex = 0;
            return _dataIndexBase.Value + folderDataIndex;
        }
        #endregion
    }
}
