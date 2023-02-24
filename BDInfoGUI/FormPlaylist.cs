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
using BDInfoLib.BDROM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BDInfoGUI
{
    public delegate void OnCustomPlaylistFinished();

    public partial class FormPlaylist : Form
    {
        private readonly BDROM _bdrom;
        private readonly ListViewColumnSorter _playlistColumnSorter;
        public List<TSStreamClip> StreamClips = new();
        private readonly OnCustomPlaylistFinished _onFinished;

        private TSPlaylistFile SelectedPlaylist
        {
            get
            {
                if (_bdrom == null || listViewPlaylistFiles.SelectedItems.Count == 0)
                {
                    return null;
                }

                var playlistItem = listViewPlaylistFiles.SelectedItems[0];

                TSPlaylistFile playlist = null;
                var playlistFileName = playlistItem.Text;
                if (_bdrom.PlaylistFiles.ContainsKey(playlistFileName))
                {
                    playlist = _bdrom.PlaylistFiles[playlistFileName];
                }
                return playlist;
            }
        }

        public FormPlaylist(string name, BDROM bdrom, OnCustomPlaylistFinished func)
        {
            InitializeComponent();

            textBoxName.Text = name;
            _bdrom = bdrom;
            _onFinished = func;

            _playlistColumnSorter = new ListViewColumnSorter();
            listViewPlaylistFiles.ListViewItemSorter = _playlistColumnSorter;
        }

        private void FormPlaylist_Load(object sender,
                                       EventArgs e)
        {
            ResetColumnWidths();
        }

        private void FormPlaylist_Resize(object sender,
                                         EventArgs e)
        {
            ResetColumnWidths();
        }

        private void CheckOK()
        {
            buttonOK.Enabled = StreamClips.Count > 0;
        }

        private void ResetColumnWidths()
        {
            var listViewPlaylistFilesColumnWidth =
                listViewPlaylistFiles.ClientSize.Width /
                listViewPlaylistFiles.Columns.Count;

            foreach (ColumnHeader column in listViewPlaylistFiles.Columns)
            {
                column.Width = listViewPlaylistFilesColumnWidth;
            }

            var listViewStreamFilesColumnWidth =
                listViewStreamFiles.ClientSize.Width /
                listViewStreamFiles.Columns.Count;

            foreach (ColumnHeader column in listViewStreamFiles.Columns)
            {
                column.Width = listViewStreamFilesColumnWidth;
            }

            var listViewTargetFilesColumnWidth =
                listViewTargetFiles.ClientSize.Width /
                listViewTargetFiles.Columns.Count;

            foreach (ColumnHeader column in listViewTargetFiles.Columns)
            {
                column.Width = listViewTargetFilesColumnWidth;
            }
        }

        public void LoadPlaylists()
        {
            string selectedPlaylistName = null;
            var selectedStreamFileIndex = 0;
            if (listViewPlaylistFiles.SelectedItems.Count > 0)
            {
                selectedPlaylistName = listViewPlaylistFiles.SelectedItems[0].Text;
                if (listViewStreamFiles.SelectedItems.Count > 0)
                {
                    selectedStreamFileIndex = listViewStreamFiles.SelectedIndices[0];
                }
            }

            listViewPlaylistFiles.Items.Clear();

            if (_bdrom == null) return;

            foreach (var playlist in _bdrom.PlaylistFiles.Values.Where(playlist => playlist.IsValid))
            {
                if (checkBoxFilterIncompatible.Checked)
                {
                    var isCompatible = true;
                    foreach (var clip1 in playlist.StreamClips)
                    {
                        if (StreamClips.Any(clip2 => !clip1.IsCompatible(clip2)))
                        {
                            isCompatible = false;
                        }
                    }
                    if (!isCompatible) continue;
                }

                ListViewItem.ListViewSubItem playlistName = new()
                {
                    Text = playlist.Name, 
                    Tag = playlist.Name
                };

                TimeSpan playlistLengthSpan = new((long)(playlist.TotalLength * 10000000));
                ListViewItem.ListViewSubItem playlistLength = new()
                {
                    Text = $@"{playlistLengthSpan:hh\:mm\:ss}",
                    Tag = playlist.TotalLength
                };

                ListViewItem.ListViewSubItem playlistSize = new();
                if (BDInfoSettings.EnableSSIF && playlist.InterleavedFileSize > 0)
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
                    playlistSize.Text = @"-";
                    playlistSize.Tag = playlist.FileSize;
                }

                ListViewItem.ListViewSubItem playlistSize2 = new()
                {
                    Text = playlist.TotalAngleSize > 0 ? ToolBox.FormatFileSize(playlist.TotalAngleSize) : "-",
                    Tag = playlist.TotalAngleSize
                };

                ListViewItem.ListViewSubItem[] playlistSubItems =
                {
                    playlistName, 
                    playlistLength, 
                    playlistSize, 
                    playlistSize2
                };

                ListViewItem playlistItem = new(playlistSubItems, 0);

                listViewPlaylistFiles.Items.Add(playlistItem);
            }

            if (selectedPlaylistName != null)
            {
                foreach (ListViewItem item in listViewPlaylistFiles.Items)
                {
                    if (item.Text != selectedPlaylistName) continue;

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
            else if (listViewPlaylistFiles.Items.Count > 0)
            {
                _playlistColumnSorter.SortColumn = 1;
                _playlistColumnSorter.Order = SortOrder.Descending;
                listViewPlaylistFiles.Sort();
                listViewPlaylistFiles.Items[0].Selected = true;
                ResetColumnWidths();
            }
        }

        private void LoadPlaylist(ListView listView, List<TSStreamClip> clips)
        {
            listView.Items.Clear();

            foreach (var clip in clips)
            {
                ListViewItem.ListViewSubItem clipName = new()
                {
                    Text = clip.Name,
                    Tag = clip.Name
                };
                if (clip.AngleIndex > 0)
                {
                    clipName.Text += $@" ({clip.AngleIndex})";
                }

                TimeSpan clipLengthSpan = new((long)(clip.Length * 10000000));

                ListViewItem.ListViewSubItem clipLength = new()
                {
                    Text = $@"{clipLengthSpan:hh\:mm\:ss}",
                    Tag = clip.Length
                };

                ListViewItem.ListViewSubItem clipSize = new();
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
                    clipSize.Text = @"-";
                    clipSize.Tag = clip.FileSize;
                }

                ListViewItem.ListViewSubItem clipSize2 = new()
                {
                    Text = clip.PacketSize > 0 ? ToolBox.FormatFileSize(clip.PacketSize) : "-",
                    Tag = clip.PacketSize
                };

                ListViewItem.ListViewSubItem[] streamFileSubItems =
                {
                    clipName, 
                    clipLength,
                    clipSize, 
                    clipSize2
                };

                ListViewItem streamFileItem = new(streamFileSubItems, 0);
                listView.Items.Add(streamFileItem);
            }

            if (listView.Items.Count > 0)
            {
                listView.Items[0].Selected = true;
            }
        }

        private void listViewPlaylistFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var playlist = SelectedPlaylist;
            if (playlist != null)
            {
                LoadPlaylist(listViewStreamFiles, playlist.StreamClips);
            }
        }

        private void listViewStreamFiles_SelectedIndexChanged(object sender, EventArgs e)
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

        private void listViewTargetFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemove.Enabled = listViewTargetFiles.SelectedItems.Count > 0;

            var itemCount = listViewTargetFiles.Items.Count;
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

        private void listViewPlaylistFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _playlistColumnSorter.SortColumn)
            {
                _playlistColumnSorter.Order = _playlistColumnSorter.Order == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _playlistColumnSorter.SortColumn = e.Column;
                _playlistColumnSorter.Order = SortOrder.Ascending;
            }
            listViewPlaylistFiles.Sort();
        }
        private void checkBoxFilterIncompatible_CheckedChanged(object sender, EventArgs e)
        {
            LoadPlaylists();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (listViewStreamFiles.SelectedItems.Count == 0) return;

            var playlist = SelectedPlaylist;
            if (playlist != null)
            {
                var clipIndex = listViewStreamFiles.SelectedIndices[0];
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
            var playlist = SelectedPlaylist;
            if (playlist != null)
            {
                foreach (var clip in playlist.StreamClips)
                {
                    StreamClips.Add(clip);
                }
                LoadPlaylist(listViewTargetFiles, StreamClips);
                LoadPlaylists();
            }
            CheckOK();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            var clipIndex = listViewTargetFiles.SelectedIndices[0];
            if (clipIndex < StreamClips.Count)
            {
                StreamClips.RemoveAt(clipIndex);
            }
            LoadPlaylist(listViewTargetFiles, StreamClips);
            LoadPlaylists();
            CheckOK();
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            var selectedIndex = listViewTargetFiles.SelectedIndices[0];
            if (selectedIndex <= 0 || selectedIndex >= StreamClips.Count) return;

            (StreamClips[selectedIndex - 1], StreamClips[selectedIndex]) = (StreamClips[selectedIndex], StreamClips[selectedIndex - 1]);

            LoadPlaylist(listViewTargetFiles, StreamClips);
            listViewTargetFiles.Items[selectedIndex - 1].Selected = true;
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (listViewTargetFiles.SelectedItems.Count == 0) return;

            var selectedIndex = listViewTargetFiles.SelectedIndices[0];
            if (selectedIndex >= listViewTargetFiles.Items.Count - 1
                || selectedIndex >= StreamClips.Count - 1) return;

            (StreamClips[selectedIndex + 1], StreamClips[selectedIndex]) = (StreamClips[selectedIndex], StreamClips[selectedIndex + 1]);

            LoadPlaylist(listViewTargetFiles, StreamClips);
            listViewTargetFiles.Items[selectedIndex + 1].Selected = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;

            TSPlaylistFile playlist = new(_bdrom, textBoxName.Text, StreamClips);

            _bdrom.PlaylistFiles[playlist.Name] = playlist;

            _onFinished();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
