using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycity.File
{
    public interface IJmdArchive<TFile, TFolder> : IDisposable where TFile : IJmdFile where TFolder : IRhoFolder<TFile>
    {
        TFolder RootFolder { get; }

        void SaveTo(string path);
    }
}
