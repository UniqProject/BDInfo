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


using BDInfoLib.BDROM;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using DynamicData.Binding;
using System.Reactive;
using BDInfo.Views;
using System.Reactive.Linq;

namespace BDInfo.ViewModels;

public class ReportWindowViewModel : ViewModelBase
{
    private string _reportText = string.Empty;
    private readonly BDROM _bdrom;
    private AvaloniaList<TSPlaylistFile> _playlists;
    private readonly ScanBDROMResult _scanResult;
    private TSPlaylistFile _selectedPlaylist;
    private AvaloniaList<TSVideoStream> _videoStreams;
    private AvaloniaList<int> _angleList;
    private int _selectedPlaylistIndex;
    private TSVideoStream _selectedVideoStream;
    private int _selectedVideoStreamIndex;
    private int _selectedAngle;
    private int _selectedAngleIndex;
    private int _chartTypeIndex;

    public Avalonia.Size WindowSize => BDInfoSettings.WindowSize;

    public string ReportText
    {
        get => _reportText;
        set => this.RaiseAndSetIfChanged(ref _reportText, value);
    }

    public AvaloniaList<TSPlaylistFile> Playlists
    {
        get => _playlists;
        set => this.RaiseAndSetIfChanged(ref _playlists, value);
    }

    public TSPlaylistFile SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
    }

    public AvaloniaList<TSVideoStream> VideoStreams
    {
        get => _videoStreams;
        set => this.RaiseAndSetIfChanged(ref _videoStreams, value);
    }

    public AvaloniaList<int> AngleList  
    {
        get => _angleList;
        set => this.RaiseAndSetIfChanged(ref _angleList, value);
    }

    public int SelectedPlaylistIndex
    {
        get => _selectedPlaylistIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistIndex, value);
    }

    public TSVideoStream SelectedVideoStream
    {
        get => _selectedVideoStream;
        set => this.RaiseAndSetIfChanged(ref _selectedVideoStream, value);
    }

    public int SelectedVideoStreamIndex
    {
        get => _selectedVideoStreamIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedVideoStreamIndex, value);
    }

    public int SelectedAngle
    {
        get => _selectedAngle;
        set => this.RaiseAndSetIfChanged(ref _selectedAngle, value);
    }

    public int SelectedAngleIndex
    {
        get => _selectedAngleIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedAngleIndex, value);
    }

    public int ChartTypeIndex
    {
        get => _chartTypeIndex;
        set => this.RaiseAndSetIfChanged(ref _chartTypeIndex, value);
    }

    public ReactiveCommand<Unit, Unit> OpenChartWindow { get; }

    public ReportWindowViewModel()
    {

    }

    public ReportWindowViewModel(BDROM bdrom, IEnumerable<TSPlaylistFile> playlists, ScanBDROMResult scanResult)
    {
        this.WhenPropertyChanged(model => model.SelectedPlaylist, notifyOnInitialValue: false)
            .Subscribe(model => { UpdatePlaylist(); });

        OpenChartWindow = ReactiveCommand.CreateFromObservable(OpenChartWindowImpl);

        _bdrom = bdrom;
        Playlists = new AvaloniaList<TSPlaylistFile>(playlists);
        _scanResult = scanResult;
        SelectedPlaylistIndex = 0;
        ChartTypeIndex = 0;

        GenerateReport();
    }

    private IObservable<Unit> OpenChartWindowImpl()
    {
        var chartWindow = new ChartWindow(MainWindow.Instance.Position)
        {
            DataContext = new ChartWindowViewModel(ChartTypeIndex, SelectedPlaylist,  SelectedVideoStream.PID, SelectedAngle)
        };

        chartWindow.ShowDialog(ReportWindow.Instance);

        return Observable.Return(Unit.Default);
    }

    private void UpdatePlaylist()
    {
        if (SelectedPlaylist == null) return;

        var tempList = new List<int>();
        for (var i = 0; i <= SelectedPlaylist.AngleStreams.Count; i++)
        {
            tempList.Add(i);
        }

        AngleList = new AvaloniaList<int>(tempList);
        VideoStreams = new AvaloniaList<TSVideoStream>(SelectedPlaylist.VideoStreams);
        SelectedAngleIndex = 0;
        SelectedVideoStreamIndex = 0;
    }

    public async void CopyReportToClipboard()
    {
        if (Application.Current is { Clipboard: { } })
            await Application.Current.Clipboard!.SetTextAsync(ReportText);
    }

    private void GenerateReport()
    {
        StreamWriter reportFile = null;
        if (BDInfoSettings.AutosaveReport)
        {
            var reportName = $"BDINFO.{_bdrom.VolumeLabel}.txt";

            reportName = ToolBox.GetSafeFileName(reportName);

            reportFile = File.CreateText(Path.Combine(Environment.CurrentDirectory, reportName));
        }

        ReportText = string.Empty;
        var protection = (_bdrom.IsBDPlus ? "BD+" : _bdrom.IsUHD ? "AACS2" : "AACS");

        if (!string.IsNullOrEmpty(_bdrom.DiscTitle))
            ReportText += $"{"Disc Title:",-16}{_bdrom.DiscTitle}\r\n";

        ReportText += $"{"Disc Label:",-16}{_bdrom.VolumeLabel}\r\n";
        ReportText += $"{"Disc Size:",-16}{_bdrom.Size:N0} bytes\r\n";
        ReportText += $"{"Protection:",-16}{protection}\r\n";

        List<string> extraFeatures = new();
        if (_bdrom.IsUHD)
        {
            extraFeatures.Add("Ultra HD");
        }

        if (_bdrom.IsBDJava)
        {
            extraFeatures.Add("BD-Java");
        }

        if (_bdrom.Is50Hz)
        {
            extraFeatures.Add("50Hz Content");
        }

        if (_bdrom.Is3D)
        {
            extraFeatures.Add("Blu-ray 3D");
        }

        if (_bdrom.IsDBOX)
        {
            extraFeatures.Add("D-BOX Motion Code");
        }

        if (_bdrom.IsPSP)
        {
            extraFeatures.Add("PSP Digital Copy");
        }

        if (extraFeatures.Count > 0)
        {
            ReportText += $"{"Extras:",-16}{string.Join(", ", extraFeatures.ToArray())}\r\n";
        }

        ReportText += $"{"BDInfo:",-16}{ToolBox.GetApplicationVersion()}\r\n";

        ReportText += $"\r\n" +
                      $"{"Notes:",-16}\r\n" +
                      $"\r\n" +
                      $"BDINFO HOME:\r\n" +
                      $"  Cinema Squid (old)\r\n" +
                      $"    http://www.cinemasquid.com/blu-ray/tools/bdinfo\r\n" +
                      $"  UniqProject GitHub (new)\r\n" +
                      $"    https://github.com/UniqProject/BDInfo\r\n\r\n" +
                      $"INCLUDES FORUMS REPORT FOR:\r\n" +
                      $"  AVS Forum Blu-ray Audio and Video Specifications Thread\r\n" +
                      $"    http://www.avsforum.com/avs-vb/showthread.php?t=1155731\r\n" +
                      $"\r\n";

        if (_scanResult.ScanException != null)
        {
            ReportText += $"WARNING: Report is incomplete because: {_scanResult.ScanException.Message}\r\n";

        }

        if (_scanResult.FileExceptions.Count > 0)
        {
            ReportText += "WARNING: File errors were encountered during scan:\r\n";
            foreach (var fileName in _scanResult.FileExceptions.Keys)
            {
                var fileException = _scanResult.FileExceptions[fileName];
                ReportText += $"\r\n" +
                              $"{fileName}\t{fileException.Message}\r\n" +
                              $"{fileException.StackTrace}\r\n";
            }
        }

        foreach (var playlist in _playlists)
        {
            var summary = "";
            
            var title = playlist.Name;
            var discSize = $"{_bdrom.Size:N0}";

            var playlistTotalLength = new TimeSpan((long)(playlist.TotalLength * 10000000));
            var totalLength = $"{playlistTotalLength:hh\\:mm\\:ss\\.fff}";

            var totalLengthShort = $"{playlistTotalLength:hh\\:mm\\:ss}";

            var totalSize = $"{playlist.TotalSize:N0}";

            var totalBitrate = $"{Math.Round((double)playlist.TotalBitRate / 10000) / 100:F2}";

            var playlistAngleLength = new TimeSpan((long)(playlist.TotalAngleLength * 10000000));

            var totalAngleLength = $"{playlistAngleLength:hh\\:mm\\:ss\\.fff}";

            var totalAngleSize = $"{playlist.TotalAngleSize:N0}";

            var totalAngleBitrate = $"{Math.Round((double)playlist.TotalAngleBitRate / 10000) / 100:F2}";

            List<string> angleLengths = new();
            List<string> angleSizes = new();
            List<string> angleBitrates = new();
            List<string> angleTotalLengths = new();
            List<string> angleTotalSizes = new();
            List<string> angleTotalBitrates = new();
            if (playlist.AngleCount > 0)
            {
                for (var angleIndex = 0; angleIndex < playlist.AngleCount; angleIndex++)
                {
                    double angleLength = 0;
                    ulong angleSize = 0;
                    ulong angleTotalSize = 0;
                    if (angleIndex < playlist.AngleClips.Count &&
                        playlist.AngleClips[angleIndex] != null)
                    {
                        foreach (var clip in playlist.AngleClips[angleIndex].Values)
                        {
                            angleTotalSize += clip.PacketSize;
                            if (clip.AngleIndex != angleIndex + 1) continue;

                            angleSize += clip.PacketSize;
                            angleLength += clip.Length;
                        }
                    }

                    angleSizes.Add($"{angleSize:N0}");

                    var angleTimeSpan = new TimeSpan((long)(angleLength * 10000000));

                    angleLengths.Add($"{angleTimeSpan:hh\\:mm\\:ss\\.fff}");

                    angleTotalSizes.Add($"{angleTotalSize:N0}");

                    angleTotalLengths.Add(totalLength);

                    double angleBitrate = 0;
                    if (angleLength > 0)
                    {
                        angleBitrate = Math.Round((angleSize * 8D) / angleLength / 10000) / 100;
                    }
                    angleBitrates.Add($"{angleBitrate:F2} kbps");

                    double angleTotalBitrate = 0;
                    if (playlist.TotalLength > 0)
                    {
                        angleTotalBitrate = Math.Round((angleTotalSize * 8D) / playlist.TotalLength / 10000) / 100;
                    }
                    angleTotalBitrates.Add($"{angleTotalBitrate:F2} kbps");
                }
            }

            var videoCodec = "";
            var videoBitrate = "";
            if (playlist.VideoStreams.Count > 0)
            {
                TSStream videoStream = playlist.VideoStreams[0];
                videoCodec = videoStream.CodecAltName;
                videoBitrate = $"{Math.Round((double)videoStream.BitRate / 10000) / 100:F2}";
            }

            var audio1 = "";
            var languageCode1 = "";
            if (playlist.AudioStreams.Count > 0)
            {
                var audioStream = playlist.AudioStreams[0];

                languageCode1 = audioStream.LanguageCode;

                audio1 = $"{audioStream.CodecAltName} {audioStream.ChannelDescription}";

                if (audioStream.BitRate > 0)
                {
                    audio1 += $" {(int)Math.Round((double)audioStream.BitRate / 1000)} kbps";
                }

                if (audioStream.SampleRate > 0 && audioStream.BitDepth > 0)
                {
                    audio1 += $" ({(int)Math.Round((double)audioStream.SampleRate / 1000)}kHz/{audioStream.BitDepth}-bit)";
                }
            }

            var audio2 = "";
            if (playlist.AudioStreams.Count > 1)
            {
                for (var i = 1; i < playlist.AudioStreams.Count; i++)
                {
                    var audioStream = playlist.AudioStreams[i];

                    if (audioStream.LanguageCode == languageCode1 &&
                        audioStream.StreamType != TSStreamType.AC3_PLUS_SECONDARY_AUDIO &&
                        audioStream.StreamType != TSStreamType.DTS_HD_SECONDARY_AUDIO &&
                        !(audioStream.StreamType == TSStreamType.AC3_AUDIO &&
                          audioStream.ChannelCount == 2))
                    {
                        audio2 = $"{audioStream.CodecAltName} {audioStream.ChannelDescription}";

                        if (audioStream.BitRate > 0)
                        {
                            audio2 += $" {(int)Math.Round((double)audioStream.BitRate / 1000)} kbps";
                        }

                        if (audioStream.SampleRate > 0 && audioStream.BitDepth > 0)
                        {
                            audio2 += $" ({(int)Math.Round((double)audioStream.SampleRate / 1000)}kHz/{audioStream.BitDepth}-bit)";
                        }
                        break;
                    }
                }
            }

            ReportText += $"\r\n" +
                          $"\"********************\\\r\n" +
                          $"PLAYLIST: {playlist.Name}\r\n" +
                          $"\"********************\\\r\n" +
                          $"\r\n" +
                          $"<--- BEGIN FORUMS PASTE --->\r\n" +
                          $"[code]\r\n" +
                          $"{" ",-64}{" ",-8}{" ",-10}{" ",-18}{" ",-18}{"Total",-13}{"Video",-13}{" ",-42}{" ",-25}\r\n" +
                          $"{"Title",-64}{"Codec",-8}{"Length",-10}{"Movie Size",-18}{"Disc Size",-18}{"Bitrate",-13}{"Bitrate",-13}{"Main Audio Track",-42}{"Secondary Audio Track",-25}\r\n" +
                          $"{"-----",-64}{"------",-8}{"-------",-10}{"--------------",-18}{"----------------",-18}{"-----------",-13}{"-----------",-13}{"------------------",-42}{"---------------------",-25}\r\n" +
                          $"{title,-64}{videoCodec,-8}{totalLengthShort,-10}{totalSize,-18}{discSize,-18}{totalBitrate + " Mbps",-13}{videoBitrate + " Mbps",-13}{audio1,-42}{audio2,-25}\r\n" +
                          $"[/code]\r\n\r\n[code]\r\nDISC INFO:\r\n";

            if (!string.IsNullOrEmpty(_bdrom.DiscTitle))
                ReportText += $"{"Disc Title:",-16}{_bdrom.DiscTitle}\r\n";

            ReportText += $"{"Disc Label:",-16}{_bdrom.VolumeLabel}\r\n" +
                          $"{"Disc Size:",-16}{_bdrom.Size:N0} bytes\r\n" +
                          $"{"Protection:",-16}{protection}\r\n";

            if (extraFeatures.Count > 0)
            {
                ReportText += $"{"Extras:",-16}{string.Join(", ", extraFeatures.ToArray())}\r\n";
            }
            ReportText += $"{"BDInfo:",-16}{ToolBox.GetApplicationVersion()}b\r\n";

            ReportText += $"\r\nPLAYLIST REPORT:\r\n" +
                          $"\r\n" +
                          $"{"Name:",-16}{title}\r\n" +
                          $"{"Length:",-16}{totalLength} (h:m:s.ms)\r\n" +
                          $"{"Size:",-16}{totalSize} bytes\r\n" +
                          $"{"Total Bitrate:",-16}{totalBitrate} Mbps\r\n";

            if (playlist.AngleCount > 0)
            {
                for (var angleIndex = 0; angleIndex < playlist.AngleCount; angleIndex++)
                {
                    ReportText += $"{$"Angle {angleIndex + 1} Length:",-24}{angleLengths[angleIndex]} (h:mm:ss.ms) / {angleTotalLengths[angleIndex]} (h:mm:ss.ms)\r\n" +
                                  $"{$"Angle {angleIndex + 1} Size:",-24}{angleSizes[angleIndex]} bytes / {angleTotalSizes[angleIndex]} bytes\r\n" +
                                  $"{$"Angle {angleIndex + 1} Total Bitrate:",-24}{angleBitrates[angleIndex]} Mbps / {angleTotalBitrates[angleIndex]} Mbps";
                }

                ReportText += $"\r\n" +
                              $"{"All Angles Length:",-24}{totalAngleLength} (h:m:s.ms)\r\n" +
                              $"{"All Angles Size:",-24}{totalAngleSize} bytes\r\n" +
                              $"{"All Angles Bitrate:",-24}{totalAngleBitrate} Mbps\r\n";
            }

            //report += $"{"Description:",-24}{""}\r\n";

            if (!string.IsNullOrEmpty(_bdrom.DiscTitle))
                summary += $"{"Disc Title:",-16}{_bdrom.DiscTitle}\r\n";

            summary += $"{"Disc Label:",-16}{_bdrom.VolumeLabel}\r\n" +
                       $"{"Disc Size:",-16}{_bdrom.Size:N0} bytes\r\n" +
                       $"{"Protection:",-16}{protection}\r\n" +
                       $"{"Playlist:",-16}{title}\r\n" +
                       $"{"Size:",-16}{totalSize} bytes\r\n" +
                       $"{"Length:",-16}{totalLength}\r\n" +
                       $"{"Total Bitrate:",-16}{totalBitrate} Mbps\r\n";

            if (playlist.HasHiddenTracks)
            {
                ReportText += "\r\n(*) Indicates included stream hidden by this playlist.\r\n";
            }

            if (playlist.VideoStreams.Count > 0)
            {
                ReportText += $"\r\n" +
                              $"VIDEO:\r\n" +
                              $"\r\n" +
                              $"{"Codec",-24}{"Bitrate",-20}{"Description",-16}\r\n" +
                              $"{"---------------",-24}{"-------------",-20}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsVideoStream) continue;

                    var streamName = stream.CodecName;
                    if (stream.AngleIndex > 0)
                    {
                        streamName += $" ({stream.AngleIndex})";
                    }

                    var streamBitrate = $"{(int)Math.Round((double)stream.BitRate / 1000):N0}";
                    if (stream.AngleIndex > 0)
                    {
                        streamBitrate += $" ({(int)Math.Round((double)stream.ActiveBitRate / 1000):D})";
                    }
                    streamBitrate += " kbps";

                    ReportText += $"{(stream.IsHidden ? "* " : "") + streamName,-24}{streamBitrate,-20}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Video:",-16}{streamName} / {streamBitrate} / {stream.Description}\r\n";
                }
            }

            if (playlist.AudioStreams.Count > 0)
            {
                ReportText += $"\r\n" +
                              $"AUDIO:\r\n" +
                              $"\r\n" +
                              $"{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                              $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsAudioStream) continue;

                    var streamBitrate = $"{(int)Math.Round((double)stream.BitRate / 1000),5:D} kbps";

                    ReportText += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Audio:",-16}{stream.LanguageName} / {stream.CodecName} / {stream.Description}\r\n";
                }
            }

            if (playlist.GraphicsStreams.Count > 0)
            {
                ReportText += $"\r\n" +
                              $"SUBTITLES:\r\n" +
                              $"\r\n" +
                              $"{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                              $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsGraphicsStream) continue;

                    var streamBitrate = $"{(double)stream.BitRate / 1000,5:F2} kbps";

                    ReportText += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Subtitle:",-16}{stream.LanguageName} / {streamBitrate.Trim()}\r\n";
                }
            }

            if (playlist.TextStreams.Count > 0)
            {
                ReportText += $"\r\n" +
                              $"TEXT:\r\n" +
                              $"\r\n{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                              $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsTextStream) continue;

                    var streamBitrate = $"{(double)stream.BitRate / 1000,5:F2} kbps";

                    ReportText += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";
                }
            }

            ReportText += $"\r\n" +
                          $"FILES:\r\n" +
                          $"\r\n{"Name",-16}{"Time In",-16}{"Length",-16}{"Size",-16}{"Total Bitrate",-16}\r\n" +
                          $"{"---------------",-16}{"-------------",-16}{"-------------",-16}{"-------------",-16}{"-------------",-16}\r\n";

            foreach (var clip in playlist.StreamClips)
            {
                var clipName = clip.DisplayName;

                if (clip.AngleIndex > 0)
                {
                    clipName += $" ({clip.AngleIndex})";
                }

                var clipSize = $"{clip.PacketSize:N0}";

                var clipInSpan = new TimeSpan((long)(clip.RelativeTimeIn * 10000000));
                var clipLengthSpan = new TimeSpan((long)(clip.Length * 10000000));

                var clipTimeIn = $"{clipInSpan:h\\:mm\\:ss\\.fff}";
                var clipLength = $"{clipLengthSpan:h\\:mm\\:ss\\.fff}";

                var clipBitrate = $"{Math.Round((double)clip.PacketBitRate / 1000),6:N0} kbps";

                ReportText += $"{clipName,-16}{clipTimeIn,-16}{clipLength,-16}{clipSize,-16}{clipBitrate,-16}\r\n";
            }

            ReportText += "\r\n" +
                          "CHAPTERS:\r\n" +
                          "\r\n";

            ReportText += $"{"Number",-16}{"Time In",-16}{"Length",-16}{"Avg Video Rate",-16}{"Max 1-Sec Rate",-16}{"Max 1-Sec Time",-16}{"Max 5-Sec Rate",-16}" +
                      $"{"Max 5-Sec Time",-16}{"Max 10Sec Rate",-16}{"Max 10Sec Time",-16}{"Avg Frame Size",-16}{"Max Frame Size",-16}{"Max Frame Time",-16}\r\n";

            ReportText += $"{"------",-16}{"-------------",-16}{"-------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}" +
                      $"{"--------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}\r\n";

            Queue<double> window1Bits = new();
            Queue<double> window1Seconds = new();
            var window1BitsSum = 0D;
            var window1SecondsSum = 0D;
            var window1PeakBitrate = 0D;
            var window1PeakLocation = 0D;

            Queue<double> window5Bits = new();
            Queue<double> window5Seconds = new();
            var window5BitsSum = 0D;
            var window5SecondsSum = 0D;
            var window5PeakBitrate = 0D;
            var window5PeakLocation = 0D;

            Queue<double> window10Bits = new();
            Queue<double> window10Seconds = new();
            var window10BitsSum = 0D;
            var window10SecondsSum = 0D;
            var window10PeakBitrate = 0D;
            var window10PeakLocation = 0D;

            var chapterPosition = 0D;
            var chapterBits = 0D;
            var chapterFrameCount = 0L;
            var chapterSeconds = 0D;
            var chapterMaxFrameSize = 0D;
            var chapterMaxFrameLocation = 0D;

            var diagPID = playlist.VideoStreams[0].PID;

            var chapterIndex = 0;
            var clipIndex = 0;
            var diagIndex = 0;

            while (chapterIndex < playlist.Chapters.Count)
            {
                TSStreamClip clip = null;
                TSStreamFile file = null;

                if (clipIndex < playlist.StreamClips.Count)
                {
                    clip = playlist.StreamClips[clipIndex];
                    file = clip.StreamFile;
                }

                var chapterStart = playlist.Chapters[chapterIndex];

                var chapterEnd = chapterIndex < playlist.Chapters.Count - 1
                    ? playlist.Chapters[chapterIndex + 1]
                    : playlist.TotalLength;

                var chapterLength = chapterEnd - chapterStart;

                List<TSStreamDiagnostics> diagList = null;

                if (clip is { AngleIndex: 0 } &&
                    file != null && file.StreamDiagnostics.ContainsKey(diagPID))
                {
                    diagList = file.StreamDiagnostics[diagPID];

                    while (diagIndex < diagList.Count && chapterPosition < chapterEnd)
                    {
                        var diag = diagList[diagIndex++];

                        if (diag.Marker < clip.TimeIn) continue;

                        chapterPosition =
                            diag.Marker -
                            clip.TimeIn +
                            clip.RelativeTimeIn;

                        var seconds = diag.Interval;
                        var bits = diag.Bytes * 8.0;

                        chapterBits += bits;
                        chapterSeconds += seconds;

                        if (diag.Tag != null)
                        {
                            chapterFrameCount++;
                        }

                        window1SecondsSum += seconds;
                        window1Seconds.Enqueue(seconds);
                        window1BitsSum += bits;
                        window1Bits.Enqueue(bits);

                        window5SecondsSum += diag.Interval;
                        window5Seconds.Enqueue(diag.Interval);
                        window5BitsSum += bits;
                        window5Bits.Enqueue(bits);

                        window10SecondsSum += seconds;
                        window10Seconds.Enqueue(seconds);
                        window10BitsSum += bits;
                        window10Bits.Enqueue(bits);

                        if (bits > chapterMaxFrameSize * 8)
                        {
                            chapterMaxFrameSize = bits / 8;
                            chapterMaxFrameLocation = chapterPosition;
                        }
                        if (window1SecondsSum > 1.0)
                        {
                            var bitrate = window1BitsSum / window1SecondsSum;
                            if (bitrate > window1PeakBitrate &&
                                chapterPosition - window1SecondsSum > 0)
                            {
                                window1PeakBitrate = bitrate;
                                window1PeakLocation = chapterPosition - window1SecondsSum;
                            }
                            window1BitsSum -= window1Bits.Dequeue();
                            window1SecondsSum -= window1Seconds.Dequeue();
                        }
                        if (window5SecondsSum > 5.0)
                        {
                            var bitrate = window5BitsSum / window5SecondsSum;
                            if (bitrate > window5PeakBitrate &&
                                chapterPosition - window5SecondsSum > 0)
                            {
                                window5PeakBitrate = bitrate;
                                window5PeakLocation = chapterPosition - window5SecondsSum;
                                if (window5PeakLocation < 0)
                                {
                                    window5PeakLocation = 0;
                                    window5PeakLocation = 0;
                                }
                            }
                            window5BitsSum -= window5Bits.Dequeue();
                            window5SecondsSum -= window5Seconds.Dequeue();
                        }
                        if (window10SecondsSum > 10.0)
                        {
                            var bitrate = window10BitsSum / window10SecondsSum;
                            if (bitrate > window10PeakBitrate &&
                                chapterPosition - window10SecondsSum > 0)
                            {
                                window10PeakBitrate = bitrate;
                                window10PeakLocation = chapterPosition - window10SecondsSum;
                            }
                            window10BitsSum -= window10Bits.Dequeue();
                            window10SecondsSum -= window10Seconds.Dequeue();
                        }
                    }
                }
                if (diagList == null || diagIndex == diagList.Count)
                {
                    if (clipIndex < playlist.StreamClips.Count)
                    {
                        clipIndex++;
                        diagIndex = 0;
                    }
                    else
                    {
                        chapterPosition = chapterEnd;
                    }
                }
                if (chapterPosition >= chapterEnd)
                {
                    ++chapterIndex;

                    var window1PeakSpan = new TimeSpan((long)(window1PeakLocation * 10000000));
                    var window5PeakSpan = new TimeSpan((long)(window5PeakLocation * 10000000));
                    var window10PeakSpan = new TimeSpan((long)(window10PeakLocation * 10000000));
                    var chapterMaxFrameSpan = new TimeSpan((long)(chapterMaxFrameLocation * 10000000));
                    var chapterStartSpan = new TimeSpan((long)(chapterStart * 10000000));
                    var chapterLengthSpan = new TimeSpan((long)(chapterLength * 10000000));

                    double chapterBitrate = 0;
                    if (chapterLength > 0)
                    {
                        chapterBitrate = chapterBits / chapterLength;
                    }
                    double chapterAvgFrameSize = 0;
                    if (chapterFrameCount > 0)
                    {
                        chapterAvgFrameSize = chapterBits / chapterFrameCount / 8;
                    }

                    ReportText += 
                        $"{chapterIndex,-16}{$"{chapterStartSpan:h\\:mm\\:ss\\.fff}",-16}{$"{chapterLengthSpan:h\\:mm\\:ss\\.fff}",-16}{$"{Math.Round(chapterBitrate / 1000),6:N0} kbps",-16}" +
                        $"{$"{Math.Round(window1PeakBitrate / 1000),6:N0} kbps",-16}{$"{window1PeakSpan:hh\\:mm\\:ss\\.fff}",-16}{$"{Math.Round(window5PeakBitrate / 1000),6:N0} kbps",-16}" +
                        $"{$"{window5PeakSpan:hh\\:mm\\:ss\\.fff}",-16}{$"{Math.Round(window10PeakBitrate / 1000),6:N0} kbps",-16}{$"{window10PeakSpan:hh\\:mm\\:ss\\.fff}",-16}" +
                        $"{$"{chapterAvgFrameSize,7:N0} bytes",-16}{$"{chapterMaxFrameSize,7:N0} bytes",-16}{$"{chapterMaxFrameSpan:hh\\:mm\\:ss\\.fff}",-16}\r\n";

                    window1Bits = new Queue<double>();
                    window1Seconds = new Queue<double>();
                    window1BitsSum = 0;
                    window1SecondsSum = 0;
                    window1PeakBitrate = 0;
                    window1PeakLocation = 0;

                    window5Bits = new Queue<double>();
                    window5Seconds = new Queue<double>();
                    window5BitsSum = 0;
                    window5SecondsSum = 0;
                    window5PeakBitrate = 0;
                    window5PeakLocation = 0;

                    window10Bits = new Queue<double>();
                    window10Seconds = new Queue<double>();
                    window10BitsSum = 0;
                    window10SecondsSum = 0;
                    window10PeakBitrate = 0;
                    window10PeakLocation = 0;

                    chapterBits = 0;
                    chapterSeconds = 0;
                    chapterFrameCount = 0;
                    chapterMaxFrameSize = 0;
                    chapterMaxFrameLocation = 0;
                }
            }

            if (BDInfoSettings.GenerateStreamDiagnostics)
            {
                ReportText += $"\r\n" +
                              $"STREAM DIAGNOSTICS:\r\n" +
                              $"\r\n" +
                              $"{"File",-16}{"PID",-16}{"Type",-16}{"Codec",-16}{"Language",-24}{"Seconds",-24}{$"{"Bitrate",11}",-24}{$"{"Bytes",10}",-16}{$"{"Packets",9}",-16}\r\n" +
                              $"{"----------",-16}{"-------------",-16}{"-----",-16}{"----------",-16}{"-------------",-24}{"--------------",-24}{"---------------",-24}{"--------------",-16}{"-----------",-16}\r\n";

                Dictionary<string, TSStreamClip> reportedClips = new();
                foreach (var clip in playlist.StreamClips
                             .Where(clip => clip.StreamFile != null)
                             .Where(clip => !reportedClips.ContainsKey(clip.Name)))
                {
                    reportedClips[clip.Name] = clip;

                    var clipName = clip.DisplayName;
                    if (clip.AngleIndex > 0)
                    {
                        clipName += $" ({clip.AngleIndex})";
                    }

                    foreach (var clipStream in clip.StreamFile.Streams.Values)
                    {
                        if (!playlist.Streams.ContainsKey(clipStream.PID)) continue;

                        var playlistStream = playlist.Streams[clipStream.PID];

                        var clipBitRate = $"{0,7:N0} kbps";
                        var clipSeconds = "0";

                        if (clip.StreamFile.Length > 0)
                        {
                            clipSeconds = clip.StreamFile.Length.ToString("F3");
                            clipBitRate = $"{Math.Round((double)clipStream.PayloadBytes * 8 / clip.StreamFile.Length / 1000),7:N0} kbps";
                        }

                        var language = "";
                        if (!string.IsNullOrEmpty(playlistStream.LanguageCode))
                        {
                            language = $"{playlistStream.LanguageCode} ({playlistStream.LanguageName})";
                        }

                        ReportText += 
                            $"{clipName,-16}{$"{clipStream.PID} (0x{clipStream.PID:X})",-16}{$"0x{(byte)clipStream.StreamType:X2}",-16}{clipStream.CodecShortName,-16}{language,-24}{clipSeconds,-24}" +
                            $"{clipBitRate,-24}{$"{clipStream.PayloadBytes,14:N0}",-16}{$"{clipStream.PacketCount,11:N0}",-16}\r\n";
                    }
                }
            }

            ReportText += "\r\n" +
                          "[/code]\r\n" +
                          "<---- END FORUMS PASTE ---->\r\n" +
                          "\r\n";

            if (BDInfoSettings.GenerateTextSummary)
            {
                ReportText += $"QUICK SUMMARY:\r\n" +
                              $"\r\n" +
                              $"{summary}\r\n";
            }

            GC.Collect();
        }

        if (BDInfoSettings.AutosaveReport && reportFile != null)
        {
            try
            {
                reportFile.Write(ReportText);
            }
            catch
            {
                // ignored
            }
        }

        reportFile?.Close();

    }
}