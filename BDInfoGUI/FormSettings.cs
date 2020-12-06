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

using BDInfo;
using System;
using System.Windows.Forms;

namespace BDInfoGUI
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            checkBoxSizeFormatHR.Checked = BDInfoGUISettings.SizeFormatHR;
            checkBoxAutosaveReport.Checked = BDInfoGUISettings.AutosaveReport;
            checkBoxGenerateStreamDiagnostics.Checked = BDInfoGUISettings.GenerateStreamDiagnostics;
            checkBoxGenerateTextSummary.Checked = BDInfoGUISettings.GenerateTextSummary;
            checkBoxUseImagePrefix.Checked = BDInfoGUISettings.UseImagePrefix;
            textBoxUseImagePrefixValue.Text = BDInfoGUISettings.UseImagePrefixValue;
            checkBoxDisplayChapterCount.Checked = BDInfoGUISettings.DisplayChapterCount;

            checkBoxExtendedStreamDiagnostics.Checked = BDInfoSettings.ExtendedStreamDiagnostics;
            checkBoxFilterLoopingPlaylists.Checked = BDInfoSettings.FilterLoopingPlaylists;
            checkBoxFilterShortPlaylists.Checked = BDInfoSettings.FilterShortPlaylists;
            textBoxFilterShortPlaylistsValue.Text = BDInfoSettings.FilterShortPlaylistsValue.ToString();
            checkBoxKeepStreamOrder.Checked = BDInfoSettings.KeepStreamOrder;
            checkBoxEnableSSIF.Checked = BDInfoSettings.EnableSSIF;
        }

        private void buttonOK_Click(object sender,
                                    EventArgs e)
        {
            BDInfoGUISettings.SizeFormatHR = checkBoxSizeFormatHR.Checked;
            BDInfoGUISettings.AutosaveReport = checkBoxAutosaveReport.Checked;
            BDInfoGUISettings.GenerateStreamDiagnostics = checkBoxGenerateStreamDiagnostics.Checked;
            BDInfoGUISettings.GenerateTextSummary = checkBoxGenerateTextSummary.Checked;
            BDInfoGUISettings.UseImagePrefix = checkBoxUseImagePrefix.Checked;
            BDInfoGUISettings.UseImagePrefixValue = textBoxUseImagePrefixValue.Text;
            BDInfoGUISettings.DisplayChapterCount = checkBoxDisplayChapterCount.Checked;

            BDInfoSettings.ExtendedStreamDiagnostics = checkBoxExtendedStreamDiagnostics.Checked;
            BDInfoSettings.FilterLoopingPlaylists = checkBoxFilterLoopingPlaylists.Checked;
            BDInfoSettings.FilterShortPlaylists = checkBoxFilterShortPlaylists.Checked;
            BDInfoSettings.KeepStreamOrder = checkBoxKeepStreamOrder.Checked;
            BDInfoSettings.EnableSSIF = checkBoxEnableSSIF.Checked;

            int filterShortPlaylistsValue;
            if (int.TryParse(textBoxFilterShortPlaylistsValue.Text, out filterShortPlaylistsValue))
            {
                BDInfoSettings.FilterShortPlaylistsValue = filterShortPlaylistsValue;
            }
            BDInfoSettings.SaveSettings();
            BDInfoGUISettings.SaveSettings();
            Close();
        }

        private void buttonCancel_Click(object sender,
                                        EventArgs e)
        {
            Close();
        }
    }
}
