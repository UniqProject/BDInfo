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

namespace BDInfoGUI
{
    class BDInfoGUISettings
    {
        public static bool SizeFormatHR
        {
            get
            {
                try { return Properties.Settings.Default.SizeFormatHR; }
                catch { return true; }
            }

            set
            {
                try { Properties.Settings.Default.SizeFormatHR = value; }
                catch { }
            }
        }

        public static bool GenerateStreamDiagnostics
        {
            get
            {
                try { return Properties.Settings.Default.GenerateStreamDiagnostics; }
                catch { return true; }
            }

            set
            {
                try { Properties.Settings.Default.GenerateStreamDiagnostics = value; }
                catch { }
            }
        }

        public static bool DisplayChapterCount
        {
            get
            {
                try { return Properties.Settings.Default.DisplayChapterCount; }
                catch { return false; }
            }
            set
            {
                try { Properties.Settings.Default.DisplayChapterCount = value; }
                catch { }
            }
        }

        public static bool AutosaveReport
        {
            get
            {
                try { return Properties.Settings.Default.AutosaveReport; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.AutosaveReport = value; }
                catch { }
            }
        }

        public static bool UseImagePrefix
        {
            get
            {
                try { return Properties.Settings.Default.UseImagePrefix; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.UseImagePrefix = value; }
                catch { }
            }
        }

        public static string UseImagePrefixValue
        {
            get
            {
                try { return Properties.Settings.Default.UseImagePrefixValue; }
                catch { return null; }
            }

            set
            {
                try { Properties.Settings.Default.UseImagePrefixValue = value; }
                catch { }
            }
        }

        public static bool GenerateTextSummary
        {
            get
            {
                try { return Properties.Settings.Default.GenerateTextSummary; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.GenerateTextSummary = value; }
                catch { }
            }
        }

        public static string LastPath
        {
            get
            {
                try { return Properties.Settings.Default.LastPath; }
                catch { return ""; }
            }

            set
            {
                try { Properties.Settings.Default.LastPath = value; }
                catch { }
            }
        }

        public static void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch { }
        }
    }
}
