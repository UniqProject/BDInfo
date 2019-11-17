using System;
using System.Collections.Generic;
using System.Text;

using BDInfo.IO;

namespace BDInfo
{
    class DiscFileInfo : IFileInfo
    {
        private readonly DiscUtils.DiscFileInfo _impl;
        public string Name => _impl.Name;

        public string FullName => _impl.FullName;

        public string Extension => _impl.Extension;

        public long Length => _impl.Length;

        public bool IsDir => _impl.Attributes.HasFlag(System.IO.FileAttributes.Directory);

        public DiscFileInfo(DiscUtils.DiscFileInfo impl)
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
    }
}
