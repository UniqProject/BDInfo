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

using System.IO.IsolatedStorage;
using System.Text;

namespace BDInfoLib;

public class ToolBox
{
    public static string ReadString(byte[] data, int count, ref int pos)
    {
        var val = Encoding.ASCII.GetString(data, pos, count);

        pos += count;

        return val;
    }

    public static IsolatedStorageFileStream GetIsolatedStorageFileStream(string fileName, bool readFile)
    {
        var isolatedStorageFile =
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly, null,
                null);
        var isoFile = isolatedStorageFile.OpenFile(fileName, readFile ? FileMode.OpenOrCreate : FileMode.Create,
            FileAccess.ReadWrite, FileShare.ReadWrite);
        return isoFile;
    }

    public static object GetTextReaderWriter(string fileName, bool readFile)
    {
        if (readFile)
        {
            return new StreamReader(fileName, Encoding.UTF8, true,
                new FileStreamOptions
                    { Access = FileAccess.ReadWrite, Mode = FileMode.OpenOrCreate, Share = FileShare.ReadWrite });
        }
        else
        {
            return new StreamWriter(fileName, Encoding.UTF8,
                new FileStreamOptions
                    { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Share = FileShare.ReadWrite });
        }
    }
}