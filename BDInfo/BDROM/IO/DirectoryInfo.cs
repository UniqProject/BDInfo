//============================================================================
// BDInfo - Blu-ray Video and Audio Analysis Tool
// Copyright © 2010 Cinema Squid
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

namespace BDInfoLib.BDROM.IO;

public class DirectoryInfo : IDirectoryInfo
{
    private readonly System.IO.DirectoryInfo _impl;
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
        return Array.ConvertAll(_impl.GetDirectories(), x => new DirectoryInfo(x));
    }

    public IFileInfo[] GetFiles()
    {
        return Array.ConvertAll(_impl.GetFiles(), x => new FileInfo(x));
    }

    public IFileInfo[] GetFiles(string searchPattern)
    {
        return Array.ConvertAll(_impl.GetFiles(searchPattern), x => new FileInfo(x));
    }

    public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
    {
        return Array.ConvertAll(_impl.GetFiles(searchPattern, searchOption), x => new FileInfo(x));
    }

    public string GetVolumeLabel()
    {
        var driveInfo = new DriveInfo(_impl.FullName);
        return _impl.Root.FullName == _impl.FullName ? driveInfo.VolumeLabel : Name;
    }

    public static IDirectoryInfo FromDirectoryName(string path)
    {
        return new DirectoryInfo(path);
    }
}