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
    public delegate void OnCustomPlaylistFinished();

    public partial class FormPlaylist : Form
    {
        private BDROM BDROM = null;
        private ListViewColumnSorter PlaylistColumnSorter;
        public List<TSStreamClip> StreamClips = new List<TSStreamClip>();
        private OnCustomPlaylistFinished OnFinished;

        public FormPlaylist(string name, BDROM bdrom, OnCustomPlaylistFinished func)
        {
            InitializeComponent();

            textBoxName.Text = name;
            BDROM = bdrom;
            OnFinished = func;

            PlaylistColumnSorter = new ListViewColumnSorter();
            listViewPlaylistFiles.ListViewItemSorter = PlaylistColumnSorter;
        }

        private TSPlaylistFile SelectedPlaylist
        {
            get
            {
                if (BDROM == null ||
                    listViewPlaylistFiles.SelectedItems.Count == 0 ||
                    listViewPlaylistFiles.SelectedItems[0] == null)
                {
                    return null;
                }

                ListViewItem playlistItem = listViewPlaylistFiles.SelectedItems[0];

                TSPlaylistFile playlist = null;
                string playlistFileName = playlistItem.Text;
                if (BDROM.PlaylistFiles.ContainsKey(playlistFileName))
                {
                    playlist = BDROM.PlaylistFiles[playlistFileName];
                }
                return playlist;
            }
        }

        public void LoadPlaylists()
        {
            string selectedPlaylistName = null;
            int selectedStreamFileIndex = 0;
            if (listViewPlaylistFiles.SelectedItems.Count > 0)
            {
                selectedPlaylistName = listViewPlaylistFiles.SelectedItems[0].Text;
                if (listViewStreamFiles.SelectedItems.Count > 0)
                {
                    selectedStreamFileIndex = listViewStreamFiles.SelectedIndices[0];
                }
            }

            listViewPlaylistFiles.Items.Clear();

            if (BDROM == null) return;

            foreach (TSPlaylistFile playlist
                in BDROM.PlaylistFiles.Values)
            {
                if (!playlist.IsValid) continue;

                if (checkBoxFilterIncompatible.Checked)
                {
                    bool isCompatible = true;
                    foreach (TSStreamClip clip1 in playlist.StreamClips)
                    {
                        foreach (TSStreamClip clip2 in StreamClips)
                        {
                            if (!clip1.IsCompatible(clip2))
                            {
                                isCompatible = false;
                                break;
                            }
                        }
                    }
                    if (!isCompatible) continue;
                }

                ListViewItem.ListViewSubItem playlistName =
                    new ListViewItem.ListViewSubItem();
                playlistName.Text = playlist.Name;
                playlistName.Tag = playlist.Name;

                TimeSpan playlistLengthSpan =
                    new TimeSpan((long)(playlist.TotalLength * 10000000));
                ListViewItem.ListViewSubItem playlistLength =
                    new ListViewItem.ListViewSubItem();
                playlistLength.Text = string.Format(
                    "{0:D2}:{1:D2}:{2:D2}",
                    playlistLengthSpan.Hours,
                    playlistLengthSpan.Minutes,
                    playlistLengthSpan.Seconds);
                playlistLength.Tag = playlist.TotalLength;

                ListViewItem.ListViewSubItem playlistSize =
                    new ListViewItem.ListViewSubItem();
                if (BDInfoSettings.EnableSSIF &&
                    playlist.InterleavedFileSize > 0)
                {
                    playlistSize.Text = ToolBox.FormatFileSize(playlist.InterleavedFileSize);
                    playlistSize.Tag = playlist.InterleavedFileSize;
                }
                else if (playlist.FileSize > 0)
                {
                    playlistSize.Text = ToolBox.FormatFileSize(playlist.FileSize);
                    playlistSize.Tag = playlist.FileSize;
                }
                else
                {
                    playlistSize.Text = "-";
                    playlistSize.Tag = playlist.FileSize;
                }

                ListViewItem.ListViewSubItem playlistSize2 =
                    new ListViewItem.ListViewSubItem();
                if (playlist.TotalAngleSize > 0)
                {
                    playlistSize2.Text = ToolBox.FormatFileSize(playlist.TotalAngleSize);
                }
                else
                {
                    playlistSize2.Text = "-";
                }
                playlistSize2.Tag = playlist.TotalAngleSize;

                ListViewItem.ListViewSubItem[] playlistSubItems =
                    new ListViewItem.ListViewSubItem[]
                    {
                        playlistName,
                        playlistLength,
                        playlistSize,
                        playlistSize2
                    };

                ListViewItem playlistItem =
                    new ListViewItem(playlistSubItems, 0);
                listViewPlaylistFiles.Items.Add(playlistItem);
            }

            if (selectedPlaylistName != null)
            {
                foreach (ListViewItem item in listViewPlaylistFiles.Items)
                {
                    if (item.Text == selectedPlaylistName)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                        if (selectedStreamFileIndex < listViewStreamFiles.Items.Count)
                        {
                            listViewStreamFiles.Items[selectedStreamFileIndex].Selected = true;
                            listViewStreamFiles.Items[selectedStreamFileIndex].EnsureVisible();
                        }
                        break;
                    }
                }
            }
            else if (listViewPlaylistFiles.Items.Count > 0)
            {
                PlaylistColumnSorter.SortColumn = 1;
                PlaylistColumnSorter.Order = SortOrder.Descending;
                listViewPlaylistFiles.Sort();
                listViewPlaylistFiles.Items[0].Selected = true;
                ResetColumnWidths();
            }
        }

        private void listViewPlaylistFiles_SelectedIndexChanged(
            object sender, 
            EventArgs e)        
        {
            TSPlaylistFile playlist = SelectedPlaylist;
            if (playlist != null)
            {
                LoadPlaylist(listViewStreamFiles, playlist.StreamClips);
            }
        }

        private void LoadPlaylist(
            ListView listView, 
            List<TSStreamClip> clips)
        {
            listView.Items.Clear();

            foreach (TSStreamClip clip in clips)
            {
                ListViewItem.ListViewSubItem clipName =
                    new ListViewItem.ListViewSubItem();
                clipName.Text = clip.Name;
                clipName.Tag = clip.Name;
                if (clip.AngleIndex > 0)
                {
                    clipName.Text += string.Format(
                        " ({0})", clip.AngleIndex);
                }

                TimeSpan clipLengthSpan =
                    new TimeSpan((long)(clip.Length * 10000000));

                ListViewItem.ListViewSubItem clipLength =
                    new ListViewItem.ListViewSubItem();
                clipLength.Text = string.Format(
                    "{0:D2}:{1:D2}:{2:D2}",
                    clipLengthSpan.Hours,
                    clipLengthSpan.Minutes,
                    clipLengthSpan.Seconds);
                clipLength.Tag = clip.Length;

                ListViewItem.ListViewSubItem clipSize =
                    new ListViewItem.ListViewSubItem();
                if (BDInfoSettings.EnableSSIF &&
                    clip.InterleavedFileSize > 0)
                {
                    clipSize.Text = ToolBox.FormatFileSize(clip.InterleavedFileSize);
                    clipSize.Tag = clip.InterleavedFileSize;
                }
                else if (clip.FileSize > 0)
                {
                    clipSize.Text = ToolBox.FormatFileSize(clip.FileSize);
                    clipSize.Tag = clip.FileSize;
                }
                else
                {
                    clipSize.Text = "-";
                    clipSize.Tag = clip.FileSize;
                }

                ListViewItem.ListViewSubItem clipSize2 =
                    new ListViewItem.ListViewSubItem();
                if (clip.PacketSize > 0)
                {
                    clipSize2.Text = ToolBox.FormatFileSize(clip.PacketSize);
                }
                else
                {
                    clipSize2.Text = "-";
                }
                clipSize2.Tag = clip.PacketSize;

                ListViewItem.ListViewSubItem[] streamFileSubItems =
                    new ListViewItem.ListViewSubItem[]
                    {
                        clipName,
                        clipLength,
                        clipSize,
                        clipSize2
                    };

                ListViewItem streamFileItem =
                    new ListViewItem(streamFileSubItems, 0);
                listView.Items.Add(streamFileItem);
            }

            if (listView.Items.Count > 0)
            {
                listView.Items[0].Selected = true;
            }
        }

        private void ResetColumnWidths()
        {
            int listViewPlaylistFilesColumnWidth =
                listViewPlaylistFiles.ClientSize.Width /
                listViewPlaylistFiles.Columns.Count;

            foreach (ColumnHeader column in listViewPlaylistFiles.Columns)
            {
                column.Width = listViewPlaylistFilesColumnWidth;
            }

            int listViewStreamFilesColumnWidth =
                listViewStreamFiles.ClientSize.Width /
                listViewStreamFiles.Columns.Count;

            foreach (ColumnHeader column in listViewStreamFiles.Columns)
            {
                column.Width = listViewStreamFilesColumnWidth;
            }

            int listViewTargetFilesColumnWidth =
                listViewTargetFiles.ClientSize.Width /
                listViewTargetFiles.Columns.Count;

            foreach (ColumnHeader column in listViewTargetFiles.Columns)
            {
                column.Width = listViewTargetFilesColumnWidth;
            }
        }

        private void FormPlaylist_Load(
            object sender, 
            EventArgs e)
        {
            ResetColumnWidths();
        }

        private void FormPlaylist_Resize(
            object sender, 
            EventArgs e)
        {
            ResetColumnWidths();
        }

        private void buttonAdd_Click(
            object sender, 
            EventArgs e)
        {
            if (listViewStreamFiles.SelectedItems.Count == 0) return;

            TSPlaylistFile playlist = SelectedPlaylist;
            if (playlist != null)
            {
                int clipIndex = listViewStreamFiles.SelectedIndices[0];
                if (clipIndex < playlist.StreamClips.Count)
                {
                    StreamClips.Add(playlist.StreamClips[clipIndex]);
                }
                LoadPlaylist(listViewTargetFiles, StreamClips);                
                LoadPlaylists();
            }
            CheckOK();
            listViewStreamFiles.Focus();
        }

        private void buttonAddAll_Click(object sender, EventArgs e)
        {
            TSPlaylistFile playlist = SelectedPlaylist;
            if (playlist != null)
            {
                foreach (TSStreamClip clip in playlist.StreamClips)
                {
                    StreamClips.Add(clip);
                }
                LoadPlaylist(listViewTargetFiles, StreamClips);
                LoadPlaylists();
            }
            CheckOK();
        }

        private void buttonRemove_Click(
            object sender, 
            EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            int clipIndex = listViewTargetFiles.SelectedIndices[0];
            if (clipIndex < StreamClips.Count)
            {
                StreamClips.RemoveAt(clipIndex);
            }
            LoadPlaylist(listViewTargetFiles, StreamClips);
            LoadPlaylists();
            CheckOK();
        }

        private void CheckOK()
        {
            if (StreamClips.Count > 0)
            {
                buttonOK.Enabled = true;
            }
            else
            {
                buttonOK.Enabled = false;
            }
        }

        private void buttonUp_Click(
            object sender, 
            EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            int selectedIndex = listViewTargetFiles.SelectedIndices[0];
            if (selectedIndex > 0 && selectedIndex < StreamClips.Count)
            {
                TSStreamClip temp = StreamClips[selectedIndex - 1];
                StreamClips[selectedIndex - 1] = StreamClips[selectedIndex];
                StreamClips[selectedIndex] = temp;
                LoadPlaylist(listViewTargetFiles, StreamClips);
                listViewTargetFiles.Items[selectedIndex - 1].Selected = true;
            }
        }

        private void buttonDown_Click(
            object sender, 
            EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            int selectedIndex = listViewTargetFiles.SelectedIndices[0];
            if (selectedIndex < listViewTargetFiles.Items.Count - 1 
                && selectedIndex < StreamClips.Count - 1)
            {
                TSStreamClip temp = StreamClips[selectedIndex + 1];
                StreamClips[selectedIndex + 1] = StreamClips[selectedIndex];
                StreamClips[selectedIndex] = temp;
                LoadPlaylist(listViewTargetFiles, StreamClips);
                listViewTargetFiles.Items[selectedIndex + 1].Selected = true;
            }
        }

        private void buttonOK_Click(
            object sender, 
            EventArgs e)
        {            
            DialogResult = DialogResult.OK;

            TSPlaylistFile playlist = 
                new TSPlaylistFile(BDROM, textBoxName.Text, StreamClips);

            BDROM.PlaylistFiles[playlist.Name] = playlist;

            OnFinished();
            Close();
        }

        private void buttonCancel_Click(
            object sender, 
            EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void listViewStreamFiles_SelectedIndexChanged(
            object sender, 
            EventArgs e)
        {
            if (listViewStreamFiles.SelectedItems.Count > 0)
            {
                buttonAdd.Enabled = true;
                buttonAddAll.Enabled = true;
            }
            else
            {
                buttonAdd.Enabled = false;
                buttonAddAll.Enabled = false;
            }
        }

        private void listViewTargetFiles_SelectedIndexChanged(
            object sender, 
            EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count > 0)
            {
                buttonRemove.Enabled = true;
            }
            else
            {
                buttonRemove.Enabled = false;
            }

            int itemCount = listViewTargetFiles.Items.Count;
            if (itemCount > 1 &&
                listViewTargetFiles.SelectedItems.Count > 0 &&
                listViewTargetFiles.SelectedIndices[0] > 0)
            {
                buttonUp.Enabled = true;
            }
            else
            {
                buttonUp.Enabled = false;
            }

            if (itemCount > 1 &&
                listViewTargetFiles.SelectedItems.Count > 0 &&
                listViewTargetFiles.SelectedIndices[0] < itemCount - 1)
            {
                buttonDown.Enabled = true;
            }
            else
            {
                buttonDown.Enabled = false;
            }
        }

        private void listViewPlaylistFiles_ColumnClick(
            object sender, 
            ColumnClickEventArgs e)
        {
            if (e.Column == PlaylistColumnSorter.SortColumn)
            {
                if (PlaylistColumnSorter.Order == SortOrder.Ascending)
                {
                    PlaylistColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    PlaylistColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                PlaylistColumnSorter.SortColumn = e.Column;
                PlaylistColumnSorter.Order = SortOrder.Ascending;
            }
            listViewPlaylistFiles.Sort();
        }

        private void checkBoxFilterIncompatible_CheckedChanged(object sender, EventArgs e)
        {
            LoadPlaylists();
        }
    }
}
