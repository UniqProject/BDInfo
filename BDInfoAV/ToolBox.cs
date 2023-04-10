﻿//============================================================================
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

using System;
using System.Reflection;
using BDInfoLib.BDROM;

namespace BDInfo;

internal class ToolBox
{
    public static string FixVolumeLabel(string label)
    {
        // TODO: Other Volume Label Tweaks?
        return label.Replace(" ", "_");
    }

    public static string FormatFileSize(double fSize, bool formatHR = false)
    {
        if (fSize <= 0) return "0";
        var units = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        var digitGroups = 0;
        if (formatHR)
            digitGroups = (int)(Math.Log10(fSize) / Math.Log10(1024));

        return $"{fSize / Math.Pow(1024, digitGroups):N2} {units[digitGroups]}";
    }

    public static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        if (version != null)
#if DEBUG || BETA
            return $"{version}b";
#else
            return $"{version}";
#endif

        return string.Empty;
    }

    public static string GetSafeFileName(string fileName)
    {
        var outFileName = fileName;

        foreach (var lDisallowed in System.IO.Path.GetInvalidFileNameChars())
        {
            outFileName = outFileName.Replace(lDisallowed.ToString(), "");
        }
        foreach (var lDisallowed in System.IO.Path.GetInvalidPathChars())
        {
            outFileName = outFileName.Replace(lDisallowed.ToString(), "");
        }

        return outFileName;
    }

    public static int ComparePlaylistFiles(TSPlaylistFile x, TSPlaylistFile y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null && y != null)
        {
            return 1;
        }

        if (x != null && y == null)
        {
            return -1;
        }

        if (x!.TotalLength > y!.TotalLength)
        {
            return -1;
        }

        if (y.TotalLength > x.TotalLength)
        {
            return 1;
        }

        return string.CompareOrdinal(x.Name, y.Name);
    }
}