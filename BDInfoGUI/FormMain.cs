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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BDInfoLib;
using BDInfoLib.BDROM;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace BDInfoGUI
{
    public partial class FormMain : Form
    {
        private BDROM _bdrom;
        private int _customPlaylistCount = 0;
        private ScanBDROMResult _scanResult = new();
        private bool _isImage;

        #region UI Handlers

        private readonly ListViewColumnSorter _playlistColumnSorter;

        public static Control FindFocusedControl(Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != (Keys.Control | Keys.C)) return base.ProcessCmdKey(ref msg, keyData);

            var focusedControl = FindFocusedControl(this);

            Clipboard.Clear();

            if (focusedControl == listViewPlaylistFiles && listViewPlaylistFiles.SelectedItems.Count > 0)
            {
                var playlistItem = listViewPlaylistFiles.SelectedItems[0];
                {
                    TSPlaylistFile playlist = null;
                    var playlistFileName = playlistItem.Text;
                    if (_bdrom.PlaylistFiles.ContainsKey(playlistFileName))
                    {
                        playlist = _bdrom.PlaylistFiles[playlistFileName];
                    }
                    if (playlist != null)
                        Clipboard.SetText(playlist.GetFilePath());
                }
            }

            if (focusedControl != listViewStreamFiles || listViewStreamFiles.SelectedItems.Count <= 0) return true;

            var streamFileItem = listViewStreamFiles.SelectedItems[0];
            {
                TSStreamFile tsStreamFile = null;
                var streamFileName = streamFileItem.Text;
                if (_bdrom.StreamFiles.ContainsKey(streamFileName))
                {
                    tsStreamFile = _bdrom.StreamFiles[streamFileName];
                }
                if (tsStreamFile != null)
                    Clipboard.SetText(tsStreamFile.GetFilePath());
            }
            return true;
        }

        private void ResetColumnWidths()
        {
            listViewPlaylistFiles.Columns[0].Width = (int)(listViewPlaylistFiles.ClientSize.Width * 0.30);
            listViewPlaylistFiles.Columns[1].Width = (int)(listViewPlaylistFiles.ClientSize.Width * 0.07);
            listViewPlaylistFiles.Columns[2].Width = (int)(listViewPlaylistFiles.ClientSize.Width * 0.19);
            listViewPlaylistFiles.Columns[3].Width = (int)(listViewPlaylistFiles.ClientSize.Width * 0.21);
            listViewPlaylistFiles.Columns[4].Width = (int)(listViewPlaylistFiles.ClientSize.Width * 0.21);

            listViewStreamFiles.Columns[0].Width = (int)(listViewStreamFiles.ClientSize.Width * 0.23);
            listViewStreamFiles.Columns[1].Width = (int)(listViewStreamFiles.ClientSize.Width * 0.08);
            listViewStreamFiles.Columns[2].Width = (int)(listViewStreamFiles.ClientSize.Width * 0.21);
            listViewStreamFiles.Columns[3].Width = (int)(listViewStreamFiles.ClientSize.Width * 0.23);
            listViewStreamFiles.Columns[4].Width = (int)(listViewStreamFiles.ClientSize.Width * 0.23);

            listViewStreams.Columns[0].Width = (int)(listViewStreams.ClientSize.Width * 0.22);
            listViewStreams.Columns[1].Width = (int)(listViewStreams.ClientSize.Width * 0.10);
            listViewStreams.Columns[2].Width = (int)(listViewStreams.ClientSize.Width * 0.10);
            listViewStreams.Columns[3].Width = (int)(listViewStreams.ClientSize.Width * 0.56);
        }

        public void OnCustomPlaylistAdded()
        {
            LoadPlaylists();
        }

        public FormMain(string[] args)
        {
            InitializeComponent();

            _playlistColumnSorter = new ListViewColumnSorter();
            listViewPlaylistFiles.ListViewItemSorter = _playlistColumnSorter;
            if (args.Length > 0)
            {
                var path = args[0];
                textBoxSource.Text = path;
                InitBDROM(path);
            }
            else
            {
                textBoxSource.Text = BDInfoGuiSettings.LastPath;
            }
            Icon = Properties.Resources.Bluray_Disc;

            Text += $@" v{Application.ProductVersion}";
#if DEBUG && BETA
            Text += "b";
#endif
            Size = Properties.Settings.Default.WindowSize;
            Location = Properties.Settings.Default.WindowLocation;
            WindowState = Properties.Settings.Default.WindowState;

            labelScanTime.Text = $@" {labelScanTime.Tag} 00:00:00 / 00:00:00";
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            ResetColumnWidths();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            BDInfoGuiSettings.LastPath = textBoxSource.Text;
            BDInfoGuiSettings.WindowState = WindowState;

            if (WindowState == FormWindowState.Normal)
            {
                BDInfoGuiSettings.WindowSize = Size;
                BDInfoGuiSettings.WindowLocation = Location;
            }
            else
            {
                BDInfoGuiSettings.WindowSize = RestoreBounds.Size;
                BDInfoGuiSettings.WindowLocation = RestoreBounds.Location;
            }


            BDInfoSettings.Save();
            BDInfoGuiSettings.SaveSettings();

            if (_initBDROMWorker is { IsBusy: true })
            {
                _initBDROMWorker.CancelAsync();
            }
            if (_scanBDROMWorker is { IsBusy: true })
            {
                _abortScan = true;
                if (_streamFile != null)
                    _streamFile.AbortScan = true;
            }
            if (_reportWorker is { IsBusy: true })
            {
                _reportWorker.CancelAsync();
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            ResetColumnWidths();
            UpdateNotification();
        }

        private void FormMain_LocationChanged(object sender, EventArgs e)
        {
            UpdateNotification();
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            var sources = (string[])e.Data?.GetData(DataFormats.FileDrop, false);
            if (sources is not { Length: > 0 }) return;

            var path = sources[0];
            textBoxSource.Text = path;
            InitBDROM(path);
        }

        private void textBoxSource_TextChanged(object sender, EventArgs e)
        {
            buttonRescan.Enabled = textBoxSource.Text.Length > 0;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            string path = null;
            try
            {
                if (((Button)sender).Name == "buttonBrowse")
                {
                    using var dialog = new FolderBrowserDialog();
                    dialog.Description = @"Select a BluRay BDMV Folder:";
#if NETCOREAPP3_1_OR_GREATER
                    dialog.UseDescriptionForTitle = true;
#endif
                    if (!string.IsNullOrEmpty(textBoxSource.Text))
                    {
                        dialog.SelectedPath = textBoxSource.Text;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.SelectedPath;
                        _isImage = false;
                    }
                }
                else
                {
                    using var dialog = new OpenFileDialog();
                    dialog.Title = @"Select a BluRay .ISO file:";
                    dialog.Filter = @"ISO-Image|*.iso";
                    dialog.RestoreDirectory = true;
                    if (!string.IsNullOrEmpty(textBoxSource.Text))
                    {
                        dialog.InitialDirectory = textBoxSource.Text;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.FileName;
                        _isImage = true;
                    }
                }

                if (string.IsNullOrEmpty(path)) return;

                textBoxSource.Text = path;
                InitBDROM(path);
            }
            catch (Exception ex)
            {
                var msg = $"Error opening path {path}: {ex.Message}{Environment.NewLine}";

                MessageBox.Show(msg, @"BDInfo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            var path = textBoxSource.Text;
            try
            {
                InitBDROM(path);
            }
            catch (Exception ex)
            {
                var msg = $"Error opening path {path}: {ex.Message}{Environment.NewLine}";

                MessageBox.Show(msg, @"BDInfo Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            FormSettings settings = new();
            if (settings.ShowDialog() == DialogResult.OK)
                LoadPlaylists(); 
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewPlaylistFiles.Items)
            {
                item.Checked = true;
            }
        }

        private void buttonUnselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewPlaylistFiles.Items)
            {
                item.Checked = false;
            }
        }

        private void buttonCustomPlaylist_Click(object sender, EventArgs e)
        {
            var name = $"USER.{++_customPlaylistCount:D3}";

            FormPlaylist form = new(name, _bdrom, OnCustomPlaylistAdded);
            form.LoadPlaylists();
            form.Show();
        }

        private void buttonScan_Click(object sender, EventArgs e)
        {
            ScanBDROM();
        }

        private void buttonViewReport_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void listViewPlaylistFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPlaylist();
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
       
        #endregion

        #region BDROM Initialization Worker

        private BackgroundWorker _initBDROMWorker;

        private void InitBDROM(string path)
        {
            ShowNotification("Please wait while we scan the disc...");

            _customPlaylistCount = 0;
            buttonBrowse.Enabled = false;
            buttonIsoBrowse.Enabled = false;
            buttonRescan.Enabled = false;
            buttonSelectAll.Enabled = false;
            buttonUnselectAll.Enabled = false;
            buttonCustomPlaylist.Enabled = false;
            buttonScan.Enabled = false;
            buttonViewReport.Enabled = false;
            textBoxDetails.Enabled = false;
            listViewPlaylistFiles.Enabled = false;
            listViewStreamFiles.Enabled = false;
            listViewStreams.Enabled = false;
            textBoxDetails.Clear();
            listViewPlaylistFiles.Items.Clear();
            listViewStreamFiles.Items.Clear();
            listViewStreams.Items.Clear();


            _initBDROMWorker = new BackgroundWorker();
            _initBDROMWorker.WorkerReportsProgress = true;
            _initBDROMWorker.WorkerSupportsCancellation = true;
            _initBDROMWorker.DoWork += InitBDROMWork;
            _initBDROMWorker.ProgressChanged += InitBDROMProgress;
            _initBDROMWorker.RunWorkerCompleted += InitBDROMCompleted;
            _initBDROMWorker.RunWorkerAsync(path);
        }

        private void InitBDROMWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _bdrom = new BDROM((string)e.Argument);

                _bdrom.StreamClipFileScanError += BDROM_StreamClipFileScanError;
                _bdrom.StreamFileScanError += BDROM_StreamFileScanError;
                _bdrom.PlaylistFileScanError += BDROM_PlaylistFileScanError;
                _bdrom.Scan();
                e.Result = null;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        protected bool BDROM_PlaylistFileScanError(TSPlaylistFile playlistFile, Exception ex)
        {
            var result = MessageBox.Show(
                text: $"""
                An error occurred while scanning the playlist file {playlistFile.Name}.
                The disc may be copy-protected or damaged.
                Do you want to continue scanning the playlist files?
                """, 
                @"BDInfo Scan Error", MessageBoxButtons.YesNo);

            return result == DialogResult.Yes;
        }

        protected bool BDROM_StreamFileScanError(TSStreamFile tsStreamFile, Exception ex)
        {
            var result = MessageBox.Show(
                $"""
                An error occurred while scanning the stream file {tsStreamFile.Name}.
                The disc may be copy-protected or damaged.
                Do you want to continue scanning the stream files?
                """, @"BDInfo Scan Error", MessageBoxButtons.YesNo);

            return result == DialogResult.Yes;
        }

        protected bool BDROM_StreamClipFileScanError(TSStreamClipFile streamClipFile, Exception ex)
        {
            var result = MessageBox.Show(
                $"""
                An error occurred while scanning the stream clip file {streamClipFile.Name}.
                The disc may be copy-protected or damaged.
                Do you want to continue scanning the stream clip files?
                """,
                @"BDInfo Scan Error", MessageBoxButtons.YesNo);

            return result == DialogResult.Yes;
        }

        private void InitBDROMProgress(object sender, ProgressChangedEventArgs e)
        {
        }

        private void InitBDROMCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            HideNotification();

            if (e.Result != null)
            {
                var msg = $"{((Exception)e.Result).Message}";

                MessageBox.Show(msg, @"BDInfo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonBrowse.Enabled = true;
                buttonIsoBrowse.Enabled = true;
                buttonRescan.Enabled = true;
                return;
            }

            buttonBrowse.Enabled = true;
            buttonIsoBrowse.Enabled = true;
            buttonRescan.Enabled = true;
            buttonScan.Enabled = true;
            buttonSelectAll.Enabled = true;
            buttonUnselectAll.Enabled = true;
            buttonCustomPlaylist.Enabled = true;
            buttonViewReport.Enabled = true;
            textBoxDetails.Enabled = true;
            listViewPlaylistFiles.Enabled = true;
            listViewStreamFiles.Enabled = true;
            listViewStreams.Enabled = true;
            progressBarScan.Value = 0;
            labelProgress.Text = "";
            labelScanTime.Text = $@" {labelScanTime.Tag} 00:00:00 / 00:00:00";

            if (!string.IsNullOrEmpty(_bdrom.DiscTitle))
            {
                textBoxDetails.Text += $@"Disc Title: {_bdrom.DiscTitle}{Environment.NewLine}";
            }

            if (!_isImage)
                textBoxSource.Text = _bdrom.DirectoryRoot.FullName;

            textBoxDetails.Text +=
                $@"Detected BDMV Folder: {_bdrom.DirectoryBDMV.FullName} (Disc Label: {_bdrom.VolumeLabel}){Environment.NewLine}";



            var features = new List<string>();
            if (_bdrom.IsUHD)
            {
                features.Add("Ultra HD");
            }
            if (_bdrom.Is50Hz)
            {
                features.Add("50Hz Content");
            }
            if (_bdrom.IsBDPlus)
            {
                features.Add("BD+ Copy Protection");
            }
            if (_bdrom.IsBDJava)
            {
                features.Add("BD-Java");
            }
            if (_bdrom.Is3D)
            {
                features.Add("Blu-ray 3D");
            }
            if (_bdrom.IsDBOX)
            {
                features.Add("D-BOX Motion Code");
            }
            if (_bdrom.IsPSP)
            {
                features.Add("PSP Digital Copy");
            }
            if (features.Count > 0)
            {
                textBoxDetails.Text += @"Detected Features: " + string.Join(", ", features.ToArray()) + Environment.NewLine;
            }

            textBoxDetails.Text +=
                $@"Disc Size: {_bdrom.Size:N0} bytes ({ToolBox.FormatFileSize(_bdrom.Size, true)}){Environment.NewLine}";

            LoadPlaylists();
        }

        #endregion

        #region File/Stream Lists

        private void LoadPlaylists()
        {
            listViewPlaylistFiles.Items.Clear();
            listViewStreamFiles.Items.Clear();
            listViewStreams.Items.Clear();

            if (_bdrom == null) return;

            var hasHiddenTracks = false;

            //Dictionary<string, int> playlistGroup = new Dictionary<string, int>();
            var groups = new List<List<TSPlaylistFile>>();

            var sortedPlaylistFiles = new TSPlaylistFile[_bdrom.PlaylistFiles.Count];
            _bdrom.PlaylistFiles.Values.CopyTo(sortedPlaylistFiles, 0);
            Array.Sort(sortedPlaylistFiles, ComparePlaylistFiles);

            foreach (var playlist1 in sortedPlaylistFiles.Where(playlist => playlist.IsValid))
            {
                var matchingGroupIndex = 0;
                for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                {
                    var group = groups[groupIndex];
                    foreach (var playlist2 in group.Where(playlist2 => playlist2.IsValid))
                    {
                        foreach (var clip1 in playlist1.StreamClips)
                        {
                            if (playlist2.StreamClips.Any(clip2 => clip1.Name == clip2.Name))
                            {
                                matchingGroupIndex = groupIndex + 1;
                            }

                            if (matchingGroupIndex > 0) break;
                        }
                        if (matchingGroupIndex > 0) break;
                    }
                    if (matchingGroupIndex > 0) break;
                }
                if (matchingGroupIndex > 0)
                {
                    groups[matchingGroupIndex - 1].Add(playlist1);
                }
                else
                {
                    groups.Add(new List<TSPlaylistFile> { playlist1 });
                    //matchingGroupIndex = groups.Count;
                }
                //playlistGroup[playlist1.Name] = matchingGroupIndex;
            }

            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                group.Sort(ComparePlaylistFiles);

                foreach (var playlist in group.Where(playlist => playlist.IsValid))
                {
                    if (playlist.HasHiddenTracks)
                    {
                        hasHiddenTracks = true;
                    }

                    ListViewItem.ListViewSubItem playlistIndex = new()
                    {
                        Text = (groupIndex + 1).ToString(),
                        Tag = groupIndex
                    };

                    ListViewItem.ListViewSubItem playlistName = new()
                    {
                        Text = playlist.Name,
                        Tag = playlist.Name
                    };

                    if (playlist.Chapters is { Count: > 1 } && BDInfoGuiSettings.DisplayChapterCount)
                        playlistName.Text += $@" [{playlist.Chapters.Count:D2} Chapters]";

                    var playlistLengthSpan = new TimeSpan((long)(playlist.TotalLength * 10000000));
                    ListViewItem.ListViewSubItem playlistLength = new()
                    {
                        Text = $@"{playlistLengthSpan:hh\:mm\:ss}",
                        Tag = playlist.TotalLength
                    };

                    ListViewItem.ListViewSubItem playlistSize = new();
                    if (BDInfoSettings.EnableSSIF &&
                        playlist.InterleavedFileSize > 0)
                    {
                        playlistSize.Text = ToolBox.FormatFileSize(playlist.InterleavedFileSize, BDInfoGuiSettings.SizeFormatHR);
                        playlistSize.Tag = playlist.InterleavedFileSize;
                    }
                    else if (playlist.FileSize > 0)
                    {
                        playlistSize.Text = ToolBox.FormatFileSize(playlist.FileSize, BDInfoGuiSettings.SizeFormatHR);
                        playlistSize.Tag = playlist.FileSize;
                    }
                    else
                    {
                        playlistSize.Text = @"-";
                        playlistSize.Tag = playlist.FileSize;
                    }                    

                    ListViewItem.ListViewSubItem playlistSize2 = new()
                    {
                        Text = playlist.TotalAngleSize > 0
                            ? ToolBox.FormatFileSize(playlist.TotalAngleSize, BDInfoGuiSettings.SizeFormatHR)
                            : @"-",
                        Tag = playlist.TotalAngleSize
                    };

                    ListViewItem.ListViewSubItem[] playlistSubItems =
                    {
                        playlistName,
                        playlistIndex,
                        playlistLength,
                        playlistSize,
                        playlistSize2
                    };

                    ListViewItem playlistItem = new(playlistSubItems, 0);
                    listViewPlaylistFiles.Items.Add(playlistItem);
                }
            }

            if (hasHiddenTracks)
            {
                textBoxDetails.Text += @"(*) Some playlists on this disc have hidden tracks. These tracks are marked with an asterisk.";
            }

            if (listViewPlaylistFiles.Items.Count > 0)
            {
                listViewPlaylistFiles.Items[0].Selected = true;
            }
            ResetColumnWidths();
        }

        private void LoadPlaylist()
        {
            listViewStreamFiles.Items.Clear();
            listViewStreams.Items.Clear();

            if (_bdrom == null) return;
            if (listViewPlaylistFiles.SelectedItems.Count == 0) return;

            var playlistItem = listViewPlaylistFiles.SelectedItems[0];

            TSPlaylistFile playlist = null;
            var playlistFileName = (string)playlistItem.SubItems[0].Tag;
            if (playlistFileName != null && _bdrom.PlaylistFiles.ContainsKey(playlistFileName))
            {
                playlist = _bdrom.PlaylistFiles[playlistFileName];
            }
            if (playlist == null) return;

            var clipCount = 0;
            foreach (var clip in playlist.StreamClips)
            {
                if (clip.AngleIndex == 0)
                {
                    ++clipCount;
                }

                ListViewItem.ListViewSubItem clipIndex = new()
                {
                    Text = clipCount.ToString(CultureInfo.InvariantCulture), 
                    Tag = clipCount
                };

                ListViewItem.ListViewSubItem clipName = new()
                {
                    Text = clip.DisplayName,
                    Tag = clip.Name
                };

                if (clip.AngleIndex > 0)
                {
                    clipName.Text += $@" ({clip.AngleIndex})";
                }

                var clipLengthSpan = new TimeSpan((long)(clip.Length * 10000000));
                ListViewItem.ListViewSubItem clipLength = new()
                {
                    Text = $@"{clipLengthSpan:hh\:mm\:ss}",
                    Tag = clip.Length
                };

                ListViewItem.ListViewSubItem clipSize = new();
                if (BDInfoSettings.EnableSSIF && clip.InterleavedFileSize > 0)
                {
                    clipSize.Text = ToolBox.FormatFileSize(clip.InterleavedFileSize, BDInfoGuiSettings.SizeFormatHR);
                    clipSize.Tag = clip.InterleavedFileSize;
                }
                else if (clip.FileSize > 0)
                {
                    clipSize.Text = ToolBox.FormatFileSize(clip.FileSize, BDInfoGuiSettings.SizeFormatHR);
                    clipSize.Tag = clip.FileSize;
                }
                else
                {
                    clipSize.Text = @"-";
                    clipSize.Tag = clip.FileSize;
                }

                ListViewItem.ListViewSubItem clipSize2 = new()
                {
                    Text = clip.PacketSize > 0
                        ? ToolBox.FormatFileSize(clip.PacketSize, BDInfoGuiSettings.SizeFormatHR)
                        : @"-",
                    Tag = clip.PacketSize
                };

                ListViewItem.ListViewSubItem[] streamFileSubItems =
                {
                    clipName, 
                    clipIndex, 
                    clipLength, 
                    clipSize, 
                    clipSize2
                };

                ListViewItem streamFileItem = new(streamFileSubItems, 0);
                listViewStreamFiles.Items.Add(streamFileItem);
            }

            foreach (var stream in playlist.SortedStreams)
            {
                ListViewItem.ListViewSubItem codec = new()
                {
                    Text = stream.CodecName
                };

                if (stream.AngleIndex > 0)
                {
                    codec.Text += $@" ({stream.AngleIndex})";
                }
                codec.Tag = stream.CodecName;

                if (stream.IsHidden)
                {
                    codec.Text = $@"* {codec.Text}";
                }

                ListViewItem.ListViewSubItem language = new()
                {
                    Text = stream.LanguageName, 
                    Tag = stream.LanguageName
                };

                ListViewItem.ListViewSubItem bitrate = new();

                if (stream.AngleIndex > 0)
                {
                    bitrate.Text = stream.ActiveBitRate > 0
                        ? $@"{Math.Round((double)stream.ActiveBitRate / 1000)} kbps"
                        : @"-";
                    bitrate.Tag = stream.ActiveBitRate;
                }
                else
                {
                    bitrate.Text = stream.BitRate > 0 
                        ? $@"{Math.Round((double)stream.BitRate / 1000)} kbps" 
                        : @"-";
                    bitrate.Tag = stream.BitRate;
                }

                ListViewItem.ListViewSubItem description = new()
                {
                    Text = stream.Description,
                    Tag = stream.Description
                };

                ListViewItem.ListViewSubItem[] streamSubItems =
                {
                    codec, 
                    language, 
                    bitrate, 
                    description
                };

                ListViewItem streamItem = new(streamSubItems, 0)
                {
                    Tag = stream.PID
                };
                listViewStreams.Items.Add(streamItem);
            }

            ResetColumnWidths();
        }

        private void UpdateSubtitleChapterCount()
        {
            foreach (ListViewItem item in listViewPlaylistFiles.Items)
            {
                var playlistName = (string)item.SubItems[0].Tag;
                if (playlistName == null || !_bdrom.PlaylistFiles.ContainsKey(playlistName)) continue;

                var playlist = _bdrom.PlaylistFiles[playlistName];

                foreach (var stream in playlist.Streams.Values.Where(stream => stream.IsGraphicsStream))
                {
                    ((TSGraphicsStream)stream).ForcedCaptions = 0;
                    ((TSGraphicsStream)stream).Captions = 0;
                }

                foreach (var clip in playlist.StreamClips.Where(clip => clip.StreamFile != null))
                {
                    foreach (var stream in clip.StreamFile.Streams.Values!)
                    {
                        if (!stream.IsGraphicsStream) continue;
                        if (!playlist.Streams.ContainsKey(stream.PID)) continue;

                        var plStream = (TSGraphicsStream)playlist.Streams[stream.PID];
                        var clipStream = (TSGraphicsStream)stream;

                        plStream.ForcedCaptions += clipStream.ForcedCaptions;
                        plStream.Captions += clipStream.Captions;

                        if (plStream.Width == 0 && clipStream.Width > 0)
                            plStream.Width = clipStream.Width;
                        if (plStream.Height == 0 && clipStream.Height > 0)
                            plStream.Height = clipStream.Height;
                    }
                }
            }
        }

        private void UpdatePlaylistBitrates()
        {
            foreach (ListViewItem item in listViewPlaylistFiles.Items)
            {
                var playlistName = (string)item.SubItems[0].Tag;
                if (playlistName == null || !_bdrom.PlaylistFiles.ContainsKey(playlistName)) continue;

                var playlist = _bdrom.PlaylistFiles[playlistName];
                item.SubItems[4].Text = ToolBox.FormatFileSize(playlist.TotalAngleSize, BDInfoGuiSettings.SizeFormatHR);
                item.SubItems[4].Tag = playlist.TotalAngleSize;
            }

            if (listViewPlaylistFiles.SelectedItems.Count == 0)
            {
                return;
            }

            var selectedPlaylistItem = listViewPlaylistFiles.SelectedItems[0];
            var selectedPlaylistName = (string)selectedPlaylistItem.SubItems[0].Tag;

            TSPlaylistFile selectedPlaylist = null;
            if (selectedPlaylistName != null && _bdrom.PlaylistFiles.ContainsKey(selectedPlaylistName))
            {
                selectedPlaylist = _bdrom.PlaylistFiles[selectedPlaylistName];
            }
            if (selectedPlaylist == null)
            {
                return;
            }

            for (var i = 0; i < listViewStreamFiles.Items.Count; i++)
            {
                var item = listViewStreamFiles.Items[i];
                if (selectedPlaylist.StreamClips.Count <= i ||
                    selectedPlaylist.StreamClips[i].Name != (string)item.SubItems[0].Tag) continue;

                item.SubItems[4].Text = ToolBox.FormatFileSize(selectedPlaylist.StreamClips[i].PacketSize, BDInfoGuiSettings.SizeFormatHR);
                item.Tag = selectedPlaylist.StreamClips[i].PacketSize;
            }

            for (var i = 0; i < listViewStreams.Items.Count; i++)
            {
                var item = listViewStreams.Items[i];
                if (i >= selectedPlaylist.SortedStreams.Count ||
                    selectedPlaylist.SortedStreams[i].PID != (ushort)item.Tag) continue;

                var stream = selectedPlaylist.SortedStreams[i];
                int kbps;
                if (stream.AngleIndex > 0)
                {
                    kbps = (int)Math.Round((double)stream.ActiveBitRate / 1000);
                }
                else
                {
                    kbps = (int)Math.Round((double)stream.BitRate / 1000);
                }
                item.SubItems[2].Text = $@"{kbps} kbps";
                item.SubItems[3].Text =
                    stream.Description;
            }
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

            if (x.TotalLength > y.TotalLength)
            {
                return -1;
            }

            if (y.TotalLength > x.TotalLength)
            {
                return 1;
            }

            return x.Name.CompareTo(y.Name);
        }

        #endregion

        #region Scan BDROM

        private BackgroundWorker _scanBDROMWorker;

        private class ScanBDROMState
        {
            public long TotalBytes = 0;
            public long FinishedBytes = 0;
            public readonly DateTime TimeStarted = DateTime.Now;
            public TSStreamFile StreamFile;
            public readonly Dictionary<string, List<TSPlaylistFile>> PlaylistMap = new();
            public Exception Exception;
        }

        private bool _abortScan;
        private TSStreamFile _streamFile;

        private TaskbarManager _tbManager;

        private void ScanBDROM()
        {
            if (_scanBDROMWorker is { IsBusy: true })
            {
                _abortScan = true;
                if (_streamFile != null)
                    _streamFile.AbortScan = true;
                return;
            }

            buttonScan.Text = @"Cancel Scan";
            progressBarScan.Value = 0;
            progressBarScan.Minimum = 0;
            progressBarScan.Maximum = 100;
            labelProgress.Text = @"Scanning disc...";
            labelScanTime.Text = $@" {labelScanTime.Tag} 00:00:00 / 00:00:00";
            buttonBrowse.Enabled = false;
            buttonIsoBrowse.Enabled = false;
            buttonRescan.Enabled = false;

            if (TaskbarManager.IsPlatformSupported)
            {
                _tbManager = TaskbarManager.Instance;
                _tbManager.SetProgressValue(0, 100);
                _tbManager.SetProgressState(TaskbarProgressBarState.Normal);
            }

            List<TSStreamFile> streamFiles = new();
            if (listViewPlaylistFiles.CheckedItems.Count == 0)
            {
                streamFiles.AddRange(_bdrom.StreamFiles.Values.Where(streamFile => streamFile != null));
            }
            else
            {
                foreach (ListViewItem item in listViewPlaylistFiles.CheckedItems)
                {
                    var playlistName = (string)item.SubItems[0].Tag;
                    if (playlistName == null || !_bdrom.PlaylistFiles.ContainsKey(playlistName)) continue;

                    var playlist = _bdrom.PlaylistFiles[playlistName];

                    foreach (var clip in playlist.StreamClips
                                 .Where(clip => clip.StreamFile != null && !streamFiles.Contains(clip.StreamFile)))
                    {
                        streamFiles.Add(clip.StreamFile);
                    }
                }
            }

            _abortScan = false;
            _scanBDROMWorker = new BackgroundWorker();
            _scanBDROMWorker.WorkerReportsProgress = true;
            _scanBDROMWorker.WorkerSupportsCancellation = true;
            _scanBDROMWorker.DoWork += ScanBDROMWork;
            _scanBDROMWorker.ProgressChanged += ScanBDROMProgress;
            _scanBDROMWorker.RunWorkerCompleted += ScanBDROMCompleted;
            _scanBDROMWorker.RunWorkerAsync(streamFiles);
        }

        private void ScanBDROMWork(object sender, DoWorkEventArgs e)
        {
            _scanResult = new ScanBDROMResult {ScanException = new Exception("Scan is still running.")};

            System.Threading.Timer timer = null;
            try
            {
                var streamFiles = (List<TSStreamFile>)e.Argument;
                if (streamFiles == null) return;

                var scanState = new ScanBDROMState();
                foreach (var streamFile in streamFiles)
                {
                    if (BDInfoSettings.EnableSSIF && streamFile.InterleavedFile != null)
                    {
                        if (streamFile.InterleavedFile.FileInfo != null)
                            scanState.TotalBytes += streamFile.InterleavedFile.FileInfo.Length;
                    }
                    else
                    {
                        if (streamFile.FileInfo != null)
                            scanState.TotalBytes += streamFile.FileInfo.Length;
                    }
                    
                    if (!scanState.PlaylistMap.ContainsKey(streamFile.Name))
                    {
                        scanState.PlaylistMap[streamFile.Name] = new List<TSPlaylistFile>();
                    }

                    foreach (var playlist in _bdrom.PlaylistFiles.Values)
                    {
                        playlist.ClearBitrates();

                        foreach (var clip in playlist.StreamClips.Where(clip => clip.Name == streamFile.Name))
                        {
                            if (!scanState.PlaylistMap[streamFile.Name].Contains(playlist))
                            {
                                scanState.PlaylistMap[streamFile.Name].Add(playlist);
                            }
                        }
                    }
                }

                timer = new System.Threading.Timer(ScanBDROMEvent, scanState, 1000, 1000);

                foreach (var streamFile in streamFiles)
                {
                    scanState.StreamFile = streamFile;
                    
                    var thread = new Thread(ScanBDROMThread);
                    thread.Start(scanState);
                    while (thread.IsAlive)
                    {
                        Thread.Sleep(1000);
                    }
                    if (streamFile.FileInfo != null)
                        scanState.FinishedBytes += streamFile.FileInfo.Length;
                    if (scanState.Exception != null)
                    {
                        _scanResult.FileExceptions[streamFile.Name] = scanState.Exception;
                    }

                    if (!_abortScan) continue;

                    _scanResult.ScanException = new Exception("Scan was cancelled.");
                    return;
                }
                _scanResult.ScanException = null;
            }
            catch (Exception ex)
            {
                _scanResult.ScanException = ex;
            }
            finally
            {
                timer?.Dispose();
            }
        }

        private void ScanBDROMThread(object parameter)
        {
            var scanState = (ScanBDROMState)parameter;
            try
            {
                _streamFile = scanState.StreamFile;
                _streamFile.AbortScan = false;
                var playlists = scanState.PlaylistMap[_streamFile.Name];
                _streamFile.Scan(playlists, true);
            }
            catch (Exception ex)
            {
                scanState.Exception = ex;
            }
            finally
            {
                _streamFile = null;
            }
        }

        private void ScanBDROMEvent(object state)
        {
            try
            {
                if (_scanBDROMWorker.IsBusy && !_scanBDROMWorker.CancellationPending)
                {
                    _scanBDROMWorker.ReportProgress(0, state);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScanBDROMProgress(object sender, ProgressChangedEventArgs e)
        {
            var scanState = (ScanBDROMState)e.UserState;

            try
            {
                if (scanState is { StreamFile: { } })
                {
                    labelProgress.Text = $@"Scanning {scanState.StreamFile.DisplayName}...";

                    var finishedBytes = scanState.FinishedBytes;
                    if (scanState.StreamFile != null)
                    {
                        finishedBytes += scanState.StreamFile.Size;
                    }

                    var progress = ((double)finishedBytes / scanState.TotalBytes);
                    var progressValue = (int)Math.Round(progress * 100);
                    if (progressValue < 0) progressValue = 0;
                    if (progressValue > 100) progressValue = 100;
                    progressBarScan.Value = progressValue;

                    if (TaskbarManager.IsPlatformSupported && _tbManager != null)
                    {
                        _tbManager.SetProgressValue(progressValue, 100);
                    }

                    var elapsedTime = DateTime.Now.Subtract(scanState.TimeStarted);
                    var remainingTime = progress is > 0 and < 1
                        ? new TimeSpan((long)(elapsedTime.Ticks / progress) - elapsedTime.Ticks)
                        : new TimeSpan(0);

                    labelScanTime.Text = $@" {labelScanTime.Tag} {elapsedTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)} / {remainingTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}";
                }
                
                UpdateSubtitleChapterCount();
                UpdatePlaylistBitrates();
            }
            catch
            {
                // ignored
            }
        }

        private void ScanBDROMCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonScan.Enabled = false;

            UpdateSubtitleChapterCount();
            UpdatePlaylistBitrates();

            labelProgress.Text = @"Scan complete.";
            progressBarScan.Value = 100;

            labelScanTime.Text = $@" {labelScanTime.Tag} 00:00:00 / 00:00:00";

            if (_scanResult.ScanException != null)
            {
                var msg = $"{_scanResult.ScanException.Message}";

                MessageBox.Show(msg, @"BDInfo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (BDInfoGuiSettings.AutosaveReport)
                {
                    GenerateReport();
                }
                else if (_scanResult.FileExceptions.Count > 0)
                {
                    MessageBox.Show(@"Scan completed with errors (see report).", @"BDInfo Scan", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(@"Scan completed successfully.", @"BDInfo Scan", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            buttonBrowse.Enabled = true;
            buttonIsoBrowse.Enabled = true;
            buttonRescan.Enabled = true;
            buttonScan.Enabled = true;
            buttonScan.Text = @"Scan Bitrates";

            if (TaskbarManager.IsPlatformSupported && _tbManager != null)
            {
                _tbManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
        }

        #endregion

        #region Report Generation

        private BackgroundWorker _reportWorker;

        private void GenerateReport()
        {
            ShowNotification("Please wait while we generate the report...");
            buttonViewReport.Enabled = false;

            List<TSPlaylistFile> playlists = new();
            if (listViewPlaylistFiles.CheckedItems.Count == 0)
            {
                foreach (ListViewItem item in listViewPlaylistFiles.Items)
                {
                    var tag = item.SubItems[0].Tag?.ToString();
                    if (tag != null && _bdrom.PlaylistFiles.ContainsKey(tag))
                    {
                        playlists.Add(_bdrom.PlaylistFiles[tag]);
                    }
                }
            }
            else
            {
                foreach (ListViewItem item in listViewPlaylistFiles.CheckedItems)
                {
                    var tag = item.SubItems[0].Tag?.ToString();
                    if (tag != null && _bdrom.PlaylistFiles.ContainsKey(tag))
                    {
                        playlists.Add(_bdrom.PlaylistFiles[tag]);
                    }
                }
            }

            _reportWorker = new BackgroundWorker();
            _reportWorker.WorkerReportsProgress = true;
            _reportWorker.WorkerSupportsCancellation = true;
            _reportWorker.DoWork += GenerateReportWork;
            _reportWorker.ProgressChanged += GenerateReportProgress;
            _reportWorker.RunWorkerCompleted += GenerateReportCompleted;
            _reportWorker.RunWorkerAsync(playlists);
        }

        private void GenerateReportWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var playlists = (List<TSPlaylistFile>)e.Argument;
                var report = new FormReport();
                report.Generate(_bdrom, playlists, _scanResult);
                e.Result = report;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void GenerateReportProgress(object sender, ProgressChangedEventArgs e)
        {
        }

        private void GenerateReportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            HideNotification();
            if (e.Result != null)
            {
                switch (e.Result.GetType().Name)
                {
                    case "FormReport":
                        ((Form)e.Result).Show();
                        break;
                    case "Exception":
                    {
                        var msg = $"{((Exception)e.Result).Message}";

                        MessageBox.Show(msg, @"BDInfo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                }
            }
            buttonViewReport.Enabled = true;
        }

        #endregion

        #region Notification Display

        private Form _formNotification;

        private void ShowNotification(string text)
        {
            HideNotification();

            var label = new Label
            {
                AutoSize = true,
                Font = new Font(Font.SystemFontName, 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = text
            };

            _formNotification = new Form
            {
                ControlBox = false,
                ShowInTaskbar = false,
                ShowIcon = false,
                FormBorderStyle = FormBorderStyle.Fixed3D
            };
            _formNotification.Controls.Add(label);
            _formNotification.Size = new Size(label.Width + 10, label.Height + 10);
            _formNotification.Show(this);
            _formNotification.Location = new Point(
                Location.X + Width / 2 - _formNotification.Width / 2,
                Location.Y + Height / 2 - _formNotification.Height / 2);
        }

        private void HideNotification()
        {
            if (_formNotification is not { IsDisposed: false }) return;

            _formNotification.Close();
            _formNotification = null;
        }

        private void UpdateNotification()
        {
            if (_formNotification is { IsDisposed: false, Visible: true })
            {
                _formNotification.Location = new Point(
                    Location.X + Width / 2 - _formNotification.Width / 2,
                    Location.Y + Height / 2 - _formNotification.Height / 2);
            }
        }

        #endregion

    }

    public class ListViewColumnSorter : IComparer
    {
        private int _columnToSort;
        private SortOrder _orderOfSort;
        private readonly CaseInsensitiveComparer _objectCompare;

        public ListViewColumnSorter()
        {
            _columnToSort = 0;
            _orderOfSort = SortOrder.None;
            _objectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            var listviewX = (ListViewItem)x;
            var listviewY = (ListViewItem)y;
            
            var compareResult = _objectCompare.Compare(
                listviewX?.SubItems[_columnToSort].Tag, 
                listviewY?.SubItems[_columnToSort].Tag);

            return _orderOfSort switch
            {
                SortOrder.Ascending => compareResult,
                SortOrder.Descending => (-compareResult),
                _ => 0
            };
        }

        public int SortColumn
        {
            set => _columnToSort = value;
            get => _columnToSort;
        }

        public SortOrder Order
        {
            set => _orderOfSort = value;
            get => _orderOfSort;
        }
    }

    public class ScanBDROMResult
    {
        public Exception ScanException = new("Scan has not been run.");
        public Dictionary<string, Exception> FileExceptions = new();
    }
}
