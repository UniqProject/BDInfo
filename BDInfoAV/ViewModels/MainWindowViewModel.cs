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
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using BDInfo.DataTypes;
using BDInfo.Views;
using BDInfoLib;
using BDInfoLib.BDROM;
using DynamicData.Binding;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace BDInfo.ViewModels;

public class ScanBDROMState
{
    public long TotalBytes;
    public long FinishedBytes;
    public DateTime TimeStarted = DateTime.Now;
    public TSStreamFile StreamFile;
    public Dictionary<string, List<TSPlaylistFile>> PlaylistMap = new();
    public Exception Exception;
}

public class ScanBDROMResult
{
    public Exception ScanException = new("Scan has not been run.");
    public Dictionary<string, Exception> FileExceptions = new();
}

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        this.WhenPropertyChanged(model => model.SelectedPlaylist, notifyOnInitialValue: false)
            .Subscribe(model => { UpdatePlaylist(); });

        OpenReportWindow = ReactiveCommand.CreateFromObservable(OpenReportWindowImpl);
    }

    public ReactiveCommand<Unit, Unit> OpenReportWindow { get; }

    private BDROM _bdRom;
    public bool IsImage { get; set; }

    private bool _popupVisible;
    private string _summary = string.Empty;
        
    private PlayListFileItem _selectedPlaylist = new();
    private int? _selectedPlaylistIndex = -1;
    private double? _scanProgress = 0;
    private string _processedFile = string.Empty;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private TimeSpan _remainingTime = TimeSpan.Zero;
    private bool _isScanRunning;
    private bool _showConfig;
        

    private BackgroundWorker _scanBDROMWorker;
    private BackgroundWorker _initBDROMWorker;
    private bool _abortScan;
    private TSStreamFile _streamFile;
    private int _customPlaylistCount = 0;
    private ScanBDROMResult _scanResult = new();


    private AvaloniaList<PlayListFileItem> _playlistFiles = new();
    private AvaloniaList<StreamClipItem> _streamFiles = new();
    private AvaloniaList<StreamFileItem> _streams = new();

    public Avalonia.Controls.WindowState WindowState
    {
        get => BDInfoSettings.WindowState;
        set
        {
            BDInfoSettings.WindowState = value;
            this.RaisePropertyChanged();
        }
    }

    public Avalonia.Size WindowSize => BDInfoSettings.WindowSize;

    public AvaloniaList<PlayListFileItem> PlaylistFiles
    {
        get => _playlistFiles;
        set
        {
            this.RaiseAndSetIfChanged(ref _playlistFiles, value);
            this.RaisePropertyChanged();
        }
    }

    public bool DisplayChapterCount
    {
        get => BDInfoSettings.DisplayChapterCount;
        set
        {
            BDInfoSettings.DisplayChapterCount = value;
            this.RaisePropertyChanged();
        }
    }
    public bool StreamSizesHR
    {
        get => BDInfoSettings.SizeFormatHR;
        set
        {
            BDInfoSettings.SizeFormatHR = value;
            this.RaisePropertyChanged();
        }
    }

    public bool GenerateStreamDiagnostics
    {
        get => BDInfoSettings.GenerateStreamDiagnostics;
        set
        {
            BDInfoSettings.GenerateStreamDiagnostics = value;
            this.RaisePropertyChanged();
        }
    }

    public bool AutosaveReport
    {
        get => BDInfoSettings.AutosaveReport;
        set
        {
            BDInfoSettings.AutosaveReport = value;
            this.RaisePropertyChanged();
        }
    }

    public bool UseImagePrefix
    {
        get => BDInfoSettings.UseImagePrefix;
        set
        {
            BDInfoSettings.UseImagePrefix = value;
            this.RaisePropertyChanged();
        }
    }

    public string UseImagePrefixValue
    {
        get => BDInfoSettings.UseImagePrefixValue;
        set
        {
            BDInfoSettings.UseImagePrefixValue = value;
            this.RaisePropertyChanged();
        }
    }

    public bool GenerateTextSummary
    {
        get => BDInfoSettings.GenerateTextSummary;
        set
        {
            BDInfoSettings.GenerateTextSummary = value;
            this.RaisePropertyChanged();
        }
    }

    public bool ExtendedStreamDiagnostics
    {
        get => BDInfoLibSettings.ExtendedStreamDiagnostics;
        set
        {
            BDInfoLibSettings.ExtendedStreamDiagnostics = value;
            this.RaisePropertyChanged();
        }
    }

    public bool EnableSSIF
    {
        get => BDInfoLibSettings.EnableSSIF;
        set
        {
            BDInfoLibSettings.EnableSSIF = value;
            this.RaisePropertyChanged();
        }
    }

    public bool FilterLoopingPlaylists
    {
        get => BDInfoLibSettings.FilterLoopingPlaylists;
        set
        {
            BDInfoLibSettings.FilterLoopingPlaylists = value;
            this.RaisePropertyChanged();
        }
    }

    public bool FilterShortPlaylists
    {
        get => BDInfoLibSettings.FilterShortPlaylists;
        set
        {
            BDInfoLibSettings.FilterShortPlaylists = value;
            this.RaisePropertyChanged();
        }
    }

    public int FilterShortPlaylistsValue
    {
        get => BDInfoLibSettings.FilterShortPlaylistsValue;
        set
        {
            BDInfoLibSettings.FilterShortPlaylistsValue = value;
            this.RaisePropertyChanged();
        }
    }

    public bool KeepStreamOrder
    {
        get => BDInfoLibSettings.KeepStreamOrder;
        set
        {
            BDInfoLibSettings.KeepStreamOrder = value;
            this.RaisePropertyChanged();
        }
    }

    public AvaloniaList<StreamClipItem> StreamFiles
    {
        get => _streamFiles;
        set => this.RaiseAndSetIfChanged(ref _streamFiles, value);
    }

    public AvaloniaList<StreamFileItem> Streams
    {
        get => _streams;
        set => this.RaiseAndSetIfChanged(ref _streams, value);
    }

    public string Folder
    {
        get => BDInfoSettings.LastPath;
        set
        {
            BDInfoSettings.LastPath = value;
            this.RaisePropertyChanged();
        }
    }

    public string DiscSummary
    {
        get => _summary;
        set => this.RaiseAndSetIfChanged(ref _summary, value);
    }

    public PlayListFileItem SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
    }

    public int? SelectedPlaylistIndex
    {
        get => _selectedPlaylistIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistIndex, value);
    }

    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
    }

    public TimeSpan RemainingTime
    {
        get => _remainingTime;
        set => this.RaiseAndSetIfChanged(ref _remainingTime, value);
    }

    public bool IsScanRunning
    {
        get => _isScanRunning;
        set => this.RaiseAndSetIfChanged(ref _isScanRunning, value);
    }

    public double? ScanProgress
    {
        get => _scanProgress;
        set => this.RaiseAndSetIfChanged(ref _scanProgress, value);
    }

    public string ProcessedFile
    {
        get => _processedFile;
        set => this.RaiseAndSetIfChanged(ref _processedFile, value);
    }

    public bool IsPopupVisible
    {
        get => _popupVisible;
        set => this.RaiseAndSetIfChanged(ref _popupVisible, value);
    }

    public bool ShowConfig
    {
        get => _showConfig;
        set => this.RaiseAndSetIfChanged(ref _showConfig, value);
    }

    public async void SelectFolder()
    {
        var path = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
                Title = "Select Folder"
            });

        if (!(path.Count > 0)) return;

        IsImage = false;

        // TODO > Avalonia 11 preview 5 
        //
        // from Avalonia 11 preview 5 on this will be the correct way of getting file path
        //
        //var localPath = path.First().Path.LocalPath;
        //SetPath(localPath);

        // working up until Avalonia 11 preview 4
        if (path.First().TryGetUri(out Uri localPath))
            SetPath(localPath.LocalPath);
    }

    public async void SelectIso()
    {
        var path = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new("ISO Files")
                {
                    Patterns = new List<string> { "*.iso" }
                }
            }
        });

        if (!(path.Count > 0)) return;

        IsImage = true;

        // TODO > Avalonia 11 preview 5
        //
        // from Avalonia 11 preview 5 on this will be the correct way of getting file path
        //
        //var localPath = path.First().Path.LocalPath;
        //SetPath(localPath);

        // working up until Avalonia 11 preview 4
        if (path.First().TryGetUri(out Uri localPath))
            SetPath(localPath.LocalPath);
    }

    public void Rescan()
    {
        if (string.IsNullOrEmpty(Folder)) return;

        var attr = File.GetAttributes(Folder);
        IsImage = attr.HasFlag(FileAttributes.Normal) || attr.HasFlag(FileAttributes.Archive);

        SetPath(Folder);
    }

    private void SetPath(string path)
    {
        if (IsImage)
            Folder = path;

        InitBDRom(path);
    }

    public void InitBDRom(string path)
    {
        IsPopupVisible = true;

        PlaylistFiles.Clear();
        StreamFiles.Clear();
        Streams.Clear();

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
            _bdRom = new BDROM((string)e.Argument);
            _bdRom.PlaylistFileScanError += BdRomPlaylistFileScanError;
            _bdRom.StreamFileScanError += BdRomStreamFileScanError;
            _bdRom.StreamClipFileScanError += BdRomStreamClipFileScanError;
            _bdRom.Scan();
            e.Result = null;
        }
        catch (Exception ex)
        {
            e.Result = ex;
        }
    }

    private bool BdRomPlaylistFileScanError(TSPlaylistFile playlistFile, Exception ex)
    {
        var result = MessageBoxManager.GetMessageBoxStandardWindow(
            title: "BDInfo Scan Error",
            text: $"An error occurred while scanning the playlist file {playlistFile.Name}.\r\n" +
                  $"The disc may be copy-protected or damaged.\r\n" +
                  $"Do you want to continue scanning the playlist files?",
            ButtonEnum.YesNo, Icon.Error, Avalonia.Controls.WindowStartupLocation.CenterOwner).Show();
        return result.Result == ButtonResult.Yes;
    }

    private bool BdRomStreamFileScanError(TSStreamFile streamFile, Exception ex)
    {
        var result = MessageBoxManager.GetMessageBoxStandardWindow(
            title: "BDInfo Scan Error",
            text: $"An error occurred while scanning the stream file {streamFile.Name}.\r\n" +
                  $"The disc may be copy-protected or damaged.\r\n" +
                  $"Do you want to continue scanning the stream files?", 
            ButtonEnum.YesNo, Icon.Error, Avalonia.Controls.WindowStartupLocation.CenterOwner).Show();
        return result.Result == ButtonResult.Yes;
    }

    private bool BdRomStreamClipFileScanError(TSStreamClipFile streamClipFile, Exception ex)
    {
        var result = MessageBoxManager.GetMessageBoxStandardWindow(
            title: "BDInfo Scan Error", 
            text: $"An error occurred while scanning the stream clip file {streamClipFile.Name}.\r\n" +
                  $"The disc may be copy-protected or damaged.\r\n" +
                  $"Do you want to continue scanning the stream clip files?", 
            ButtonEnum.YesNo, Icon.Error, Avalonia.Controls.WindowStartupLocation.CenterOwner).Show();
        return result.Result == ButtonResult.Yes;
    }

    private void InitBDROMProgress(object sender, ProgressChangedEventArgs e)
    {
        // ignored
    }

    private void InitBDROMCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        IsPopupVisible = false;

        if (e.Result != null)
        {

            var msg = $"{((Exception)e.Result).Message}";
            MessageBoxManager.GetMessageBoxStandardWindow(
                    title: "BDInfo Error",
                    text: msg,
                    ButtonEnum.Ok, Icon.Error, Avalonia.Controls.WindowStartupLocation.CenterOwner)
                .Show();
            return;
        }

        DiscSummary = string.Empty;

        if (!string.IsNullOrEmpty(_bdRom.DiscTitle))
        {
            DiscSummary += $"Disc Title: {_bdRom.DiscTitle}\r\n";
        }

        if (!IsImage)
            Folder = _bdRom.DirectoryRoot?.FullName;

        if (_bdRom.DirectoryBDMV != null)
        {
            DiscSummary += $"Detected BDMV Folder: {_bdRom.DirectoryBDMV.FullName} (Disc Label: {_bdRom.VolumeLabel})\r\n";
            if (IsImage)
                DiscSummary += $"ISO Image: {Folder}\r\n";
        }

        var features = new List<string>();
        if (_bdRom.IsUHD)
        {
            features.Add("Ultra HD");
        }
        if (_bdRom.Is50Hz)
        {
            features.Add("50Hz Content");
        }
        if (_bdRom.IsBDPlus)
        {
            features.Add("BD+ Copy Protection");
        }
        if (_bdRom.IsBDJava)
        {
            features.Add("BD-Java");
        }
        if (_bdRom.Is3D)
        {
            features.Add("Blu-ray 3D");
        }
        if (_bdRom.IsDBOX)
        {
            features.Add("D-BOX Motion Code");
        }
        if (_bdRom.IsPSP)
        {
            features.Add("PSP Digital Copy");
        }
        if (features.Count > 0)
        {
            DiscSummary += $"Detected Features: {string.Join(", ", features.ToArray())}\r\n";
        }

        DiscSummary += $"Disc Size: {_bdRom.Size:N0} bytes ({ToolBox.FormatFileSize(_bdRom.Size, true)})\r\n";

        LoadPlaylists();
    }

    public void LoadPlaylists()
    {
        if (_bdRom == null) return;

        var hasHiddenTracks = false;

        //Dictionary<string, int> playlistGroup = new Dictionary<string, int>();
        var groups = new List<List<TSPlaylistFile>>();

        var sortedPlaylistFiles = new TSPlaylistFile[_bdRom.PlaylistFiles.Count];
        _bdRom.PlaylistFiles.Values.CopyTo(sortedPlaylistFiles, 0);
        Array.Sort(sortedPlaylistFiles, ToolBox.ComparePlaylistFiles);

        foreach (var playlist in sortedPlaylistFiles)
        {
            if (!playlist.IsValid) continue;

            var matchingGroupIndex = 0;
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                foreach (var playlist2 in group.Where(playlist2 => playlist2.IsValid))
                {
                    foreach (var clip1 in playlist.StreamClips)
                    {
                        if (playlist2.StreamClips.Any(clip2 => clip1?.Name == clip2?.Name))
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
                groups[matchingGroupIndex - 1].Add(playlist);
            }
            else
            {
                groups.Add(new List<TSPlaylistFile> { playlist });
                //matchingGroupIndex = groups.Count;
            }
            //playlistGroup[playlist1.Name] = matchingGroupIndex;
        }

        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            group.Sort(ToolBox.ComparePlaylistFiles);

            foreach (var playlist in group.Where(playlist => playlist.IsValid))
            {
                if (playlist.HasHiddenTracks)
                {
                    hasHiddenTracks = true;
                }
                var playListFile = new PlayListFileItem
                {
                    Group = groupIndex + 1,
                    PlayListName = playlist.Name,
                    Length = new TimeSpan((long)(playlist.TotalLength * 10000000)),
                    Chapters = playlist.Chapters is { Count: > 1 } ? playlist.Chapters.Count : 0,
                    EstimatedSize = BDInfoLibSettings.EnableSSIF && playlist.InterleavedFileSize > 0
                        ? playlist.InterleavedFileSize
                        : playlist.FileSize,
                    TotalSize = playlist.TotalAngleSize
                };

                PlaylistFiles.Add(playListFile);
            }
        }

        if (hasHiddenTracks)
        {
            DiscSummary += "(*) Some playlists on this disc have hidden tracks. These tracks are marked with an asterisk.";
        }

        if (PlaylistFiles.Count > 0)
        {
            SelectedPlaylistIndex = 0;
        }
    }

    private void UpdatePlaylist()
    {
        StreamFiles.Clear();
        Streams.Clear();

        if (_bdRom == null || _selectedPlaylist == null) return;

        TSPlaylistFile playlistFile = null;
        var playlistFileName = _selectedPlaylist.PlayListName;
        if (_bdRom.PlaylistFiles.ContainsKey(playlistFileName))
        {
            playlistFile = _bdRom.PlaylistFiles[playlistFileName];
        }
        if (playlistFile == null) { return; }

        var clipCount = 0;
        foreach (var clip in playlistFile.StreamClips)
        {
            if (clip.AngleIndex == 0)
            {
                ++clipCount;
            }

            var streamClip = new StreamClipItem
            {
                Index = clipCount,
                ClipName = clip.Name,
                Angle = clip.AngleIndex,
                Length = new TimeSpan((long)(clip.Length * 10000000)),
                TotalSize = clip.PacketSize,
                EstimatedSize = BDInfoLibSettings.EnableSSIF && clip.InterleavedFileSize > 0
                    ? clip.InterleavedFileSize
                    : clip.FileSize
            };
            StreamFiles.Add(streamClip);
        }

        foreach (var stream in playlistFile.SortedStreams)
        {
            var streamItem = new StreamFileItem
            {
                Codec = $"{(stream.IsHidden ? "* " : "")}{stream.CodecName}",
                Language = stream.LanguageName,
                BitRate = stream.AngleIndex > 0 ? stream.ActiveBitRate / 1000 : stream.BitRate / 1000,
                Description = stream.Description,
                PID = stream.PID
            };
            Streams.Add(streamItem);
        }
    }

    public void SelectAllPlaylists()
    {
        foreach (var item in PlaylistFiles)
        {
            item.IsChecked = true;
        }
    }

    public void UnselectPlaylists()
    {
        foreach (var item in PlaylistFiles)
        {
            item.IsChecked = false;
        }
    }

    public void StartScan()
    {
        if (_bdRom == null) return;

        if (_scanBDROMWorker is { IsBusy: true })
        {
            _abortScan = true;
            if (_streamFile != null)
                _streamFile.AbortScan = true;
            return;
        }

        ScanProgress = 0;

        var streamFiles = new List<TSStreamFile>();
        if (!PlaylistFiles.Any(item => item.IsChecked))
        {
            streamFiles.AddRange(_bdRom.StreamFiles.Values.Where(streamFile => streamFile != null));
        }
        else
        {
            foreach (var playListFile in PlaylistFiles.Where(item => item.IsChecked))
            {
                var playListName = playListFile.PlayListName;
                if (playListName == null || !_bdRom.PlaylistFiles.ContainsKey(playListName)) continue;

                var playList = _bdRom.PlaylistFiles[playListName];
                foreach (var clip in playList.StreamClips.Where(clip => clip is { StreamFile: { } }
                                                                        && !streamFiles.Contains(clip.StreamFile)))
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
        _scanResult = new ScanBDROMResult { ScanException = new Exception("Scan is still running.") };
        Timer timer = null;

        try
        {
            var streamFiles = (List<TSStreamFile>)e.Argument;
            var scanState = new ScanBDROMState();
            foreach (var streamFile in streamFiles!)
            {
                var streamFileName = streamFile.Name;
                if (streamFileName == null) continue;

                if (BDInfoLibSettings.EnableSSIF && streamFile.InterleavedFile is { FileInfo: { } })
                {
                    scanState.TotalBytes += streamFile.InterleavedFile.FileInfo.Length;
                }
                else
                {
                    if (streamFile.FileInfo != null) 
                        scanState.TotalBytes += streamFile.FileInfo.Length;
                }

                if (!scanState.PlaylistMap.ContainsKey(streamFileName))
                {
                    scanState.PlaylistMap[streamFileName] = new List<TSPlaylistFile>();
                }

                if (_bdRom is not { PlaylistFiles.Values: { } }) continue;

                foreach (var playlist in _bdRom.PlaylistFiles.Values)
                {
                    playlist.ClearBitrates();

                    foreach (var clip in playlist.StreamClips.Where(clip => clip.Name == streamFileName)
                                 .Where(_ => !scanState.PlaylistMap[streamFileName].Contains(playlist)))
                    {
                        scanState.PlaylistMap[streamFileName].Add(playlist);
                    }
                }
            }

            timer = new Timer(ScanBDROMEvent, scanState, 1000, 1000);

            foreach (var streamFile in streamFiles)
            {
                scanState.StreamFile = streamFile;

                var thread = new Thread(ScanBDROMThread);
                thread.Start(scanState);

                while (thread.IsAlive)
                {
                    Thread.Sleep(500);
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
            timer.Dispose();
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
            if (_scanBDROMWorker is { IsBusy: true, CancellationPending: false })
            {
                _scanBDROMWorker.ReportProgress(0, state);
            }
        }
        catch (Exception)
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
                ProcessedFile = $"Scanning {scanState.StreamFile.DisplayName}...";
            }

            if (scanState != null)
            {
                var finishedBytes = scanState.FinishedBytes;
                if (scanState.StreamFile != null)
                {
                    finishedBytes += scanState.StreamFile.Size;
                }

                var progress = ((double)finishedBytes / scanState.TotalBytes);
                if (progress < 0) progress = 0;
                if (progress > 1) progress = 1;
                ScanProgress = progress * 100;

                ElapsedTime = DateTime.Now.Subtract(scanState.TimeStarted);
                RemainingTime = progress is > 0 and < 100 
                    ? new TimeSpan((long)(ElapsedTime.Ticks / progress) - ElapsedTime.Ticks) 
                    : new TimeSpan(0);
            }

            UpdateSubtitleChapterCount();
            UpdatePlaylistBitrates();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void ScanBDROMCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        IsScanRunning = false;

        UpdateSubtitleChapterCount();
        UpdatePlaylistBitrates();

        ScanProgress = 100;
        ProcessedFile = "Scan complete";

        ElapsedTime = TimeSpan.Zero;
        RemainingTime = TimeSpan.Zero;

        if (_scanResult.ScanException != null)
        {
            var msg = $"{_scanResult.ScanException.Message}";
            MessageBoxManager
                .GetMessageBoxStandardWindow("BDInfo Error", msg, 
                    ButtonEnum.Ok, Icon.Error)
                .Show();
        }
        else
        {
            if (BDInfoSettings.AutosaveReport)
            {
                OpenReportWindow.Execute();
            }
            else if (_scanResult.FileExceptions.Count > 0)
            {
                MessageBoxManager
                    .GetMessageBoxStandardWindow("BDInfo Scan", "Scan completed with errors (see report).",
                        ButtonEnum.Ok, Icon.Warning)
                    .Show();
            }
            else
            {
                MessageBoxManager
                    .GetMessageBoxStandardWindow("BDInfo Scan", "Scan completed successfully.",
                        ButtonEnum.Ok, Icon.Info)
                    .Show();
            }
        }

    }

    private void UpdateSubtitleChapterCount()
    {
        if (_bdRom == null) return;

        foreach (var item in PlaylistFiles)
        {
            var playlistName = item.PlayListName;
            if (playlistName == null || !_bdRom.PlaylistFiles.ContainsKey(playlistName)) continue;

            var playlist = _bdRom.PlaylistFiles[playlistName];

            foreach (var stream in playlist.Streams.Values.Where(stream => stream is { IsGraphicsStream: true }))
            {
                ((TSGraphicsStream)stream).ForcedCaptions = 0;
                ((TSGraphicsStream)stream).Captions = 0;
            }

            foreach (var clip in playlist.StreamClips)
            {
                if (clip?.StreamFile?.Streams.Values == null) continue;

                foreach (var stream in clip.StreamFile.Streams.Values.Where(stream => stream is { IsGraphicsStream: true }))
                {
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
        if (_bdRom == null) return;

        foreach (var item in PlaylistFiles)
        {
            var playlistName = item.PlayListName;
            if (playlistName == null || !_bdRom.PlaylistFiles.ContainsKey(playlistName)) continue;

            var playlist = _bdRom.PlaylistFiles[playlistName];
            item.TotalSize = playlist.TotalAngleSize;
        }

        if (SelectedPlaylist == null)
        {
            return;
        }

        var selectedPlaylistName = SelectedPlaylist.PlayListName;
        TSPlaylistFile selectedPlaylistFile = null;
        if (selectedPlaylistName != null && _bdRom.PlaylistFiles.ContainsKey(selectedPlaylistName))
        {
            selectedPlaylistFile = _bdRom.PlaylistFiles[selectedPlaylistName];
        }
        if (selectedPlaylistFile == null)
        {
            return;
        }

        for (var i = 0; i < StreamFiles.Count; i++)
        {
            var file = StreamFiles[i];

            if (selectedPlaylistFile.StreamClips.Count <= i ||
                selectedPlaylistFile.StreamClips[i]?.Name != file.ClipName) continue;
            file.TotalSize = selectedPlaylistFile.StreamClips[i].PacketSize;
        }

        for (var i = 0; i < Streams.Count; i++)
        {
            var streamItem = Streams[i];
            if (i >= selectedPlaylistFile.SortedStreams.Count ||
                selectedPlaylistFile.SortedStreams[i].PID != streamItem.PID) continue;

            var stream = selectedPlaylistFile.SortedStreams[i];

            streamItem.BitRate = stream switch
            {
                { AngleIndex: > 0 } => (int)Math.Round((double)stream.ActiveBitRate / 1000),
                { AngleIndex: <= 0 } => (int)Math.Round((double)stream.BitRate / 1000),
                _ => 0
            };
            streamItem.Description = stream?.Description;
        }
    }

    private IObservable<Unit> OpenReportWindowImpl()
    {
        List<TSPlaylistFile> playlists = new();

        var source = PlaylistFiles.Any(item => item.IsChecked)
            ? PlaylistFiles.Where(item => item.IsChecked)
            : PlaylistFiles;

        foreach (var item in source)
        {
            if (_bdRom.PlaylistFiles.TryGetValue(item.PlayListName, out var value))
                playlists.Add(value);
        }

        var reportWindow = new ReportWindow(MainWindow.Instance.Position)
        {
            DataContext = new ReportWindowViewModel(_bdRom, playlists, _scanResult)
        };

        reportWindow.ShowDialog(MainWindow.Instance);

        return Observable.Return(Unit.Default);
    }
}