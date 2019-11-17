using System;
using System.Collections.Generic;
using System.Text;

namespace BDInfo.IO
{
    public interface IDirectoryInfo
    {
        string Name { get; }
        string FullName { get; }
        IDirectoryInfo Parent { get; }
        IFileInfo[] GetFiles();
        IFileInfo[] GetFiles(string searchPattern);
        IFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption);

        IDirectoryInfo[] GetDirectories();
    }
}
