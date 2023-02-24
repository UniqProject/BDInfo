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

using BDInfoLib;
using System;
using System.Windows.Forms;

namespace BDInfoGUI;

public partial class FormSettings : Form
{
    public FormSettings()
    {
        InitializeComponent();

        BDInfoSettings.Load();

        checkBoxSizeFormatHR.Checked = BDInfoGuiSettings.SizeFormatHR;
        checkBoxAutosaveReport.Checked = BDInfoGuiSettings.AutosaveReport;
        checkBoxGenerateStreamDiagnostics.Checked = BDInfoGuiSettings.GenerateStreamDiagnostics;
        checkBoxGenerateTextSummary.Checked = BDInfoGuiSettings.GenerateTextSummary;
        checkBoxUseImagePrefix.Checked = BDInfoGuiSettings.UseImagePrefix;
        textBoxUseImagePrefixValue.Text = BDInfoGuiSettings.UseImagePrefixValue;
        checkBoxDisplayChapterCount.Checked = BDInfoGuiSettings.DisplayChapterCount;

        checkBoxExtendedStreamDiagnostics.Checked = BDInfoSettings.ExtendedStreamDiagnostics;
        checkBoxFilterLoopingPlaylists.Checked = BDInfoSettings.FilterLoopingPlaylists;
        checkBoxFilterShortPlaylists.Checked = BDInfoSettings.FilterShortPlaylists;
        textBoxFilterShortPlaylistsValue.Text = BDInfoSettings.FilterShortPlaylistsValue.ToString();
        checkBoxKeepStreamOrder.Checked = BDInfoSettings.KeepStreamOrder;
        checkBoxEnableSSIF.Checked = BDInfoSettings.EnableSSIF;
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
        BDInfoGuiSettings.SizeFormatHR = checkBoxSizeFormatHR.Checked;
        BDInfoGuiSettings.AutosaveReport = checkBoxAutosaveReport.Checked;
        BDInfoGuiSettings.GenerateStreamDiagnostics = checkBoxGenerateStreamDiagnostics.Checked;
        BDInfoGuiSettings.GenerateTextSummary = checkBoxGenerateTextSummary.Checked;
        BDInfoGuiSettings.UseImagePrefix = checkBoxUseImagePrefix.Checked;
        BDInfoGuiSettings.UseImagePrefixValue = textBoxUseImagePrefixValue.Text;
        BDInfoGuiSettings.DisplayChapterCount = checkBoxDisplayChapterCount.Checked;

        BDInfoSettings.ExtendedStreamDiagnostics = checkBoxExtendedStreamDiagnostics.Checked;
        BDInfoSettings.FilterLoopingPlaylists = checkBoxFilterLoopingPlaylists.Checked;
        BDInfoSettings.FilterShortPlaylists = checkBoxFilterShortPlaylists.Checked;
        BDInfoSettings.KeepStreamOrder = checkBoxKeepStreamOrder.Checked;
        BDInfoSettings.EnableSSIF = checkBoxEnableSSIF.Checked;

        if (int.TryParse(textBoxFilterShortPlaylistsValue.Text, out var filterShortPlaylistsValue))
        {
            BDInfoSettings.FilterShortPlaylistsValue = filterShortPlaylistsValue;
        }
        BDInfoSettings.Save();
        BDInfoGuiSettings.SaveSettings();
        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        Close();
    }
}