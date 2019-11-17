using System;
using BDInfo.IO;

namespace BDInfo
{
    internal class DiscDirectoryInfo : IDirectoryInfo
    {
        private DiscUtils.DiscDirectoryInfo _impl = null;
        public string Name => _impl.Name;

        public string FullName => _impl.FullName;

        public IDirectoryInfo Parent => _impl.Parent != null ? new DiscDirectoryInfo(_impl.Parent) : null;

        public DiscDirectoryInfo(DiscUtils.DiscDirectoryInfo impl)
        {
            _impl = impl;
        }

        public IDirectoryInfo[] GetDirectories()
        {
            return Array.ConvertAll(
                _impl.GetDirectories(),
                x => new DiscDirectoryInfo(x));
        }

        public IFileInfo[] GetFiles()
        {
            return Array.ConvertAll(
                _impl.GetFiles(),
                x => new DiscFileInfo(x));
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return Array.ConvertAll(
                _impl.GetFiles(searchPattern),
                x => new DiscFileInfo(x));
        }

        public IFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption)
        {
            return Array.ConvertAll(
                _impl.GetFiles(searchPattern, searchOption),
                x => new DiscFileInfo(x));
        }

        static public IDirectoryInfo FromImage(DiscUtils.Udf.UdfReader reader, string path)
        {
            return new DiscDirectoryInfo(reader.GetDirectoryInfo(path));
        }
    }
}
