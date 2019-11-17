using System;
using System.Collections.Generic;
using System.Text;

namespace BDInfo.IO
{
    public class FileInfo : IFileInfo
    {
        private readonly System.IO.FileInfo _impl;
        public string Name => _impl.Name;

        public string FullName => _impl.FullName;

        public string Extension => _impl.Extension;

        public long Length => _impl.Length;

        public bool IsDir => _impl.Attributes.HasFlag(System.IO.FileAttributes.Directory);

        public FileInfo(System.IO.FileInfo impl)
        {
            _impl = impl;
        }

        public System.IO.Stream OpenRead()
        {
            return _impl.OpenRead();
        }

        public System.IO.StreamReader OpenText()
        {
            return _impl.OpenText();
        }

        static public IFileInfo FromFullName(string path)
        {
            return new FileInfo(new System.IO.FileInfo(path));
        }
    }
}
