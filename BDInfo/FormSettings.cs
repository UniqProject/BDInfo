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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BDInfo
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            checkBoxAutosaveReport.Checked = BDInfoSettings.AutosaveReport;
            checkBoxGenerateStreamDiagnostics.Checked = BDInfoSettings.GenerateStreamDiagnostics;
            checkBoxGenerateTextSummary.Checked = BDInfoSettings.GenerateTextSummary;
            checkBoxFilterLoopingPlaylists.Checked = BDInfoSettings.FilterLoopingPlaylists;
            checkBoxFilterShortPlaylists.Checked = BDInfoSettings.FilterShortPlaylists;
            textBoxFilterShortPlaylistsValue.Text = BDInfoSettings.FilterShortPlaylistsValue.ToString();
            checkBoxUseImagePrefix.Checked = BDInfoSettings.UseImagePrefix;
            textBoxUseImagePrefixValue.Text = BDInfoSettings.UseImagePrefixValue;
            checkBoxKeepStreamOrder.Checked = BDInfoSettings.KeepStreamOrder;
            checkBoxEnableSSIF.Checked = BDInfoSettings.EnableSSIF;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            BDInfoSettings.AutosaveReport = checkBoxAutosaveReport.Checked;
            BDInfoSettings.GenerateStreamDiagnostics = checkBoxGenerateStreamDiagnostics.Checked;
            BDInfoSettings.GenerateTextSummary = checkBoxGenerateTextSummary.Checked;
            BDInfoSettings.KeepStreamOrder = checkBoxKeepStreamOrder.Checked;
            BDInfoSettings.UseImagePrefix = checkBoxUseImagePrefix.Checked;
            BDInfoSettings.UseImagePrefixValue = textBoxUseImagePrefixValue.Text;
            BDInfoSettings.FilterLoopingPlaylists = checkBoxFilterLoopingPlaylists.Checked;
            BDInfoSettings.FilterShortPlaylists = checkBoxFilterShortPlaylists.Checked;
            BDInfoSettings.EnableSSIF = checkBoxEnableSSIF.Checked;
            int filterShortPlaylistsValue;
            if (int.TryParse(textBoxFilterShortPlaylistsValue.Text, out filterShortPlaylistsValue))
            {
                BDInfoSettings.FilterShortPlaylistsValue = filterShortPlaylistsValue;
            }
            BDInfoSettings.SaveSettings();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class BDInfoSettings
    {
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

        public static bool EnableSSIF
        {
            get
            {
                try { return Properties.Settings.Default.EnableSSIF; }
                catch { return true; }
            }

            set
            {
                try { Properties.Settings.Default.EnableSSIF = value; }
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

        public static bool GenerateFrameDataFile
        {
            get
            {
                try { return Properties.Settings.Default.GenerateFrameDataFile; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.GenerateFrameDataFile = value; }
                catch { }
            }
        }

        public static bool FilterLoopingPlaylists
        {
            get
            {
                try { return Properties.Settings.Default.FilterLoopingPlaylists; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.FilterLoopingPlaylists = value; }
                catch { }
            }
        }

        public static bool FilterShortPlaylists
        {
            get
            {
                try { return Properties.Settings.Default.FilterShortPlaylists; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.FilterShortPlaylists = value; }
                catch { }
            }
        }

        public static int FilterShortPlaylistsValue
        {
            get
            {
                try { return Properties.Settings.Default.FilterShortPlaylistsValue; }
                catch { return 0; }
            }

            set
            {
                try { Properties.Settings.Default.FilterShortPlaylistsValue = value; }
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

        public static bool KeepStreamOrder
        {
            get
            {
                try { return Properties.Settings.Default.KeepStreamOrder; }
                catch { return false; }
            }

            set
            {
                try { Properties.Settings.Default.KeepStreamOrder = value; }
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
