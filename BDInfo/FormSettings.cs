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
}
