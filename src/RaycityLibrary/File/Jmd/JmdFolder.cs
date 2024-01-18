using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Raycity.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace Raycity.File
{
    public class JmdFolder : IRhoFolder<JmdFile>
    {
        #region Members
        private string _name;
        private Dictionary<string, JmdFile> _files;
        private Dictionary<string, JmdFolder> _folders;
        private JmdFolder? _parent;
        private uint _prevParentUpdatsCounter = 0xBAD_BEEFu;
        private uint _nameUpdatesCounter = 0x14325768u;
        private uint? _folderDataIndex;

        private string _parentFullname = "";
        #endregion

        #region Properties
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _nameUpdatesCounter += 0x4B9AD755u;
                _folderDataIndex = null;
            }
        }

        public string FullName 
        {
            get
            {
                if (_parent is null)
                    return _name;
                else
                {
                    if(_prevParentUpdatsCounter != _parent._nameUpdatesCounter)
                    {
                        _parentFullname = _parent.FullName;
                        _prevParentUpdatsCounter = _parent._nameUpdatesCounter;
                    }
                    return $"{_parentFullname}/{_name}";
                }
            }
        }

        public JmdFolder? Parent => _parent;

        IRhoFolder<JmdFile>? IRhoFolder<JmdFile>.Parent => _parent;
        
        IJmdFolder? IJmdFolder.Parent => _parent;

        public IReadOnlyCollection<JmdFile> Files => _files.Values;

        public IReadOnlyCollection<JmdFolder> Folders => _folders.Values;

        #endregion

        #region Constructors
        public JmdFolder()
        {
            _name = "";
            _files = new Dictionary<string, JmdFile>();
            _folders = new Dictionary<string, JmdFolder>();
            _parent = null;
        }
        #endregion

        #region Methods
        public JmdFile GetFile(string path)
        {
            string[] splittedPath = path.Split('/');
            JmdFolder findFolders = this;
            List<string> folderNameList= new List<string>();
            for(int i = 0; i < splittedPath.Length - 1; i++)
            {
                string folderName = splittedPath[i];
                folderNameList.Add(folderName);
                if (!findFolders._folders.ContainsKey(folderName))
                    throw new Exception($"Folder: {string.Join('/', folderNameList.ToArray())} can not be found.");
                else
                {
                    findFolders = findFolders._folders[folderName];
                }
            }
            if (splittedPath.Length >= 1 && findFolders._files.ContainsKey(splittedPath[^1]))
            {
                return findFolders._files[splittedPath[^1]];
            }
            else
            {
                throw new Exception($"File: {path} can not be found.");
            }
        }

        public IEnumerable<JmdFile> GetFiles()
        {
            return _files.Values;
        }

        public JmdFolder GetFolder(string path)
        {
            string[] splittedPath = path.Split('/');
            JmdFolder findFolder = this;
            List<string> folderNameList = new List<string>();
            for (int i = 0; i < splittedPath.Length; i++)
            {
                string folderName = splittedPath[i];
                folderNameList.Add(folderName);
                if (!findFolder._folders.ContainsKey(folderName))
                    throw new Exception($"Folder: {string.Join('/', folderNameList.ToArray())} can not be found.");
                else
                {
                    findFolder = findFolder._folders[folderName];
                }
            }
            return findFolder;
        }
        
        public IEnumerable<JmdFolder> GetFolders()
        {
            return _folders.Values;
        }

        public void AddFile(JmdFile file)
        {
            if(_files.ContainsKey(file.Name))
                throw new Exception($"File: {file.Name} is exist.");
            else
            {
                if(file._parentFolder is not null)
                    throw new Exception("The parent of a file you want to add is not null.");
                else
                {
                    _files.Add(file.Name, file);
                    file._parentFolder = this;
                }
            }
        }

        public void AddFile(string path, JmdFile file)
        {
            string[] splittedPath = path.Split('/');
            JmdFolder findFolders = this;
            List<string> folderNameList = new List<string>();
            for (int i = 0; i < splittedPath.Length; i++)
            {
                string folderName = splittedPath[i];
                folderNameList.Add(folderName);
                if (!findFolders._folders.ContainsKey(folderName))
                    throw new Exception($"Folder: {string.Join('/', folderNameList.ToArray())} can not be found.");
                else
                {
                    findFolders = findFolders._folders[folderName];
                }
            }
            if (splittedPath.Length > 1)
            {
                string fileName = file.Name;
                if (!findFolders._files.ContainsKey(fileName))
                {
                    if(file.Parent is not null)
                    {
                        throw new Exception("The parent of adding file is in other folder.");
                    }
                    else
                    {
                        findFolders.AddFile(file);
                    }
                }
                else
                    throw new Exception($"File: {path}/{fileName} is exist.");
            }
            else
            {
                throw new Exception($"Path: {path} is invalid.");
            }
        }

        public void AddFolder(JmdFolder folder)
        {
            if(_folders.ContainsKey(folder.Name))
                throw new Exception($"Folder: {folder.Name} is exist.");
            else
            {
                if(folder._parent is not null)
                    throw new Exception("The parent of a folder you want to add is not null.");
                else
                {
                    _folders.Add(folder.Name, folder);
                    folder._parent = this;
                }
            }

        }

        public void AddFolder(string path, JmdFolder folder)
        {
            string[] splittedPath = path.Split('/');
            JmdFolder findFolders = this;
            List<string> folderNameList = new List<string>();
            for (int i = 0; i < splittedPath.Length; i++)
            {
                string folderName = splittedPath[i];
                folderNameList.Add(folderName);
                if (!findFolders._folders.ContainsKey(folderName))
                    throw new Exception($"Folder: {string.Join('/', folderNameList.ToArray())} can not be found.");
                else
                {
                    findFolders = findFolders._folders[folderName];
                }
            }
            if (splittedPath.Length > 1)
            {
                string folderName = folder.Name;
                if (!findFolders._folders.ContainsKey(folderName))
                    findFolders.AddFolder(folder);
                else
                    throw new Exception($"Folder: {path}/{folderName} is exist.");
            }
            else
            {
                throw new Exception($"Path: {path} is invalid.");
            }
        }

        public bool RemoveFile(string fileFullName)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {

        }

        IEnumerable<IJmdFile> IJmdFolder.GetFiles()
        {
            return GetFiles();
        }

        IJmdFile IJmdFolder.GetFile(string path)
        {
            return GetFile(path);
        }

        IJmdFolder IJmdFolder.GetFolder(string path)
        {
            return GetFolder(path);
        }

        IRhoFolder<JmdFile> IRhoFolder<JmdFile>.GetFolder(string path) 
        {
            return GetFolder(path);
        }

        IEnumerable<IJmdFolder> IJmdFolder.GetFolders()
        {
            return GetFolders();
        }

        IEnumerable<IRhoFolder<JmdFile>> IRhoFolder<JmdFile>.GetFolders()
        {
            return GetFolders();
        }
        
        //void IRhoFolder<RhoFile>.AddFolder(IRhoFolder<RhoFile> folder)
        //{
        //    if(folder is RhoFolder rhoFolder)
        //    {
        //        AddFolder(rhoFolder);
        //    }
        //    else
        //    {
        //        throw new Exception($"folder should be the type of RhoFolder.");
        //    }
        //}

        //void IRhoFolder<RhoFile>.AddFolder(string path, IRhoFolder<RhoFile> folder)
        //{
        //    if(folder is RhoFolder rhoFolder)
        //    {
        //        AddFolder(path, rhoFolder);
        //    }
        //    else
        //    {
        //        throw new Exception($"folder should be the type of RhoFolder.");
        //    }
        //}

        internal uint getFolderDataIndex()
        {
            if (_parent is not null && _prevParentUpdatsCounter != _parent._nameUpdatesCounter)
                _folderDataIndex = null;
            if(_folderDataIndex is null)
            {
                if (_name.Length == 0 && _parent is null)
                    return 0xFFFFFFFFu;
                string fullName = FullName;
                byte[] fullNameEncData = Encoding.Unicode.GetBytes(fullName);
                _folderDataIndex = Adler.Adler32(0, fullNameEncData, 0, fullNameEncData.Length);
            }
            return _folderDataIndex.Value;
        }

        #endregion
    }
}
