using System;
using System.Collections.Generic;

namespace BDInfo.IO
{
    public class DirectoryInfo : IDirectoryInfo
    {
        private System.IO.DirectoryInfo _impl = null;
        public string Name => _impl.Name;

        public string FullName => _impl.FullName;

        public IDirectoryInfo Parent => _impl.Parent != null ? new DirectoryInfo(_impl.Parent) : null;

        public DirectoryInfo(string path)
        {
            _impl = new System.IO.DirectoryInfo(path);
        }

        public DirectoryInfo(System.IO.DirectoryInfo impl)
        {
            _impl = impl;
        }

        public IDirectoryInfo[] GetDirectories()
        {
            return Array.ConvertAll(
                _impl.GetDirectories(),
                x => new DirectoryInfo(x));
        }

        public IFileInfo[] GetFiles()
        {
            return Array.ConvertAll(
                _impl.GetFiles(),
                x => new FileInfo(x));
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return Array.ConvertAll(
                _impl.GetFiles(searchPattern),
                x => new FileInfo(x));
        }

        public IFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption)
        {
            return Array.ConvertAll(
                _impl.GetFiles(searchPattern, searchOption),
                x => new FileInfo(x));
        }

        static public IDirectoryInfo FromDirectoryName(string path)
        {
            return new DirectoryInfo(path);
        }
    }
}
