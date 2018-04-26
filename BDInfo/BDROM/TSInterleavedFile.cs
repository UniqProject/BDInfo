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

using System.IO;
using DiscUtils;
using DiscUtils.Udf;

// TODO: Do more interesting things here...

namespace BDInfo
{
    public class TSInterleavedFile
    {
        public DiscFileInfo DFileInfo = null;
        public UdfReader CdReader = null;

        public FileInfo FileInfo = null;
        public string Name = null;

        public TSInterleavedFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            DFileInfo = null;
            CdReader = null;
            Name = fileInfo.Name.ToUpper();
        }

        public TSInterleavedFile(DiscFileInfo fileInfo,
            UdfReader reader)
        {
            DFileInfo = fileInfo;
            FileInfo = null;
            CdReader = reader;
            Name = fileInfo.Name.ToUpper();
        }
    }
}
