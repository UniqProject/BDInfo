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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BDInfoGUI;

public partial class FormReport : Form
{
    private List<TSPlaylistFile> _playlists;

    public FormReport()
    {
        InitializeComponent();
    }

    private void FormReport_FormClosed(object sender,
        FormClosedEventArgs e)
    {
        GC.Collect();
    }

    public void Generate(BDROM bdrom, List<TSPlaylistFile> playlists, ScanBDROMResult scanResult)
    {
        _playlists = playlists;

        StreamWriter reportFile = null;
        if (BDInfoGuiSettings.AutosaveReport)
        {
            var reportName = $"BDINFO.{bdrom.VolumeLabel}.txt";

            reportName = ToolBox.GetSafeFileName(reportName);

            reportFile = File.CreateText(Path.Combine(Environment.CurrentDirectory, reportName));
        }
        textBoxReport.Text = "";

        var report = "";
        var protection = (bdrom.IsBDPlus ? "BD+" : bdrom.IsUHD ? "AACS2" : "AACS");

        if (!string.IsNullOrEmpty(bdrom.DiscTitle))
            report += $"{"Disc Title:",-16}{bdrom.DiscTitle}\r\n";

        report += $"{"Disc Label:",-16}{bdrom.VolumeLabel}\r\n";
        report += $"{"Disc Size:",-16}{bdrom.Size:N0} bytes\r\n";
        report += $"{"Protection:",-16}{protection}\r\n";

        List<string> extraFeatures = new();
        if (bdrom.IsUHD)
        {
            extraFeatures.Add("Ultra HD");
        }
        if (bdrom.IsBDJava)
        {
            extraFeatures.Add("BD-Java");
        }
        if (bdrom.Is50Hz)
        {
            extraFeatures.Add("50Hz Content");
        }
        if (bdrom.Is3D)
        {
            extraFeatures.Add("Blu-ray 3D");
        }
        if (bdrom.IsDBOX)
        {
            extraFeatures.Add("D-BOX Motion Code");
        }
        if (bdrom.IsPSP)
        {
            extraFeatures.Add("PSP Digital Copy");
        }
        if (extraFeatures.Count > 0)
        {
            report += $"{"Extras:",-16}{string.Join(", ", extraFeatures.ToArray())}\r\n";
        }
#if DEBUG || BETA
        report += $"{"BDInfo:",-16}{Application.ProductVersion}b\r\n";
#else
        report += $"{"BDInfo:",-16}{Application.ProductVersion}\r\n";
#endif

        report += $"\r\n" +
                  $"{"Notes:",-16}\r\n" +
                  $"\r\n" +
                  $"BDINFO HOME:\r\n" +
                  $"  Cinema Squid (old)\r\n" +
                  $"    http://www.cinemasquid.com/blu-ray/tools/bdinfo\r\n" +
                  $"  UniqProject GitHub (new)\r\n" +
                  $"    https://github.com/UniqProject/BDInfo\r\n" +
                  $"\r\n" +
                  $"INCLUDES FORUMS REPORT FOR:\r\n" +
                  $"  AVS Forum Blu-ray Audio and Video Specifications Thread\r\n" +
                  $"    http://www.avsforum.com/avs-vb/showthread.php?t=1155731\r\n" +
                  $"\r\n";

        if (scanResult.ScanException != null)
        {
            report += $"WARNING: Report is incomplete because: {scanResult.ScanException.Message}\r\n";
        }
        if (scanResult.FileExceptions.Count > 0)
        {
            report += "WARNING: File errors were encountered during scan:\r\n";
            foreach (var fileName in scanResult.FileExceptions.Keys)
            {
                var fileException = scanResult.FileExceptions[fileName];
                report += $"\r\n" +
                          $"{fileName}\t{fileException.Message}\r\n" +
                          $"{fileException.StackTrace}\r\n";
            }
        }            

        foreach (var playlist in playlists)
        {
            var summary = "";

            comboBoxPlaylist.Items.Add(playlist);

            var title = playlist.Name;
            var discSize = $"{bdrom.Size:N0}";

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

            report += $"\r\n" +
                      $"\"********************\\\r\n" +
                      $"PLAYLIST: {playlist.Name}\r\n" +
                      $"\"********************\\\r\n" +
                      $"\r\n" +
                      $"<--- BEGIN FORUMS PASTE --->\r\n" +
                      $"[code]\r\n" +
                      $"{" ",-64}{" ",-8}{" ",-8}{" ",-16}{" ",-18}{"Total",-13}{"Video",-13}{" ",-42}{" ",-25}\r\n" +
                      $"{"Title",-64}{"Codec",-8}{"Length",-8}{"Movie Size",-16}{"Disc Size",-18}{"Bitrate",-13}{"Bitrate",-13}{"Main Audio Track",-42}{"Secondary Audio Track",-25}\r\n" +
                      $"{"-----",-64}{"------",-8}{"-------",-8}{"--------------",-16}{"----------------",-18}{"-----------",-13}{"-----------",-13}{"------------------",-42}{"---------------------",-25}\r\n" +
                      $"{title,-64}{videoCodec,-8}{totalLengthShort,-8}{totalSize,-16}{discSize,-18}{totalBitrate + " Mbps",-13}{videoBitrate + " Mbps",-13}{audio1,-42}{audio2,-25}\r\n" +
                      $"[/code]\r\n" +
                      $"\r\n" +
                      $"[code]\r\n" +
                      $"DISC INFO:\r\n";

            if (!string.IsNullOrEmpty(bdrom.DiscTitle))
                report += $"{"Disc Title:",-16}{bdrom.DiscTitle}\r\n";

            report += $"{"Disc Label:",-16}{bdrom.VolumeLabel}\r\n" +
                      $"{"Disc Size:",-16}{bdrom.Size:N0} bytes\r\n" +
                      $"{"Protection:",-16}{protection}\r\n";

            if (extraFeatures.Count > 0)
            {
                report += $"{"Extras:",-16}{string.Join(", ", extraFeatures.ToArray())}\r\n";
            }
#if DEBUG || BETA
            report += $"{"BDInfo:",-16}{Application.ProductVersion}b\r\n";
#else
            report += $"{"BDInfo:",-16}{Application.ProductVersion}\r\n";
#endif

            report += $"\r\n" +
                      $"PLAYLIST REPORT:\r\n" +
                      $"\r\n" +
                      $"{"Name:",-16}{title}\r\n" +
                      $"{"Length:",-16}{totalLength} (h:m:s.ms)\r\n" +
                      $"{"Size:",-16}{totalSize} bytes\r\n" +
                      $"{"Total Bitrate:",-16}{totalBitrate} Mbps\r\n";

            if (playlist.AngleCount > 0)
            {
                for (var angleIndex = 0; angleIndex < playlist.AngleCount; angleIndex++)
                {
                    report += $"{$"Angle {angleIndex + 1} Length:",-24}{angleLengths[angleIndex]} (h:mm:ss.ms) / {angleTotalLengths[angleIndex]} (h:mm:ss.ms)\r\n" +
                              $"{$"Angle {angleIndex + 1} Size:",-24}{angleSizes[angleIndex]} bytes / {angleTotalSizes[angleIndex]} bytes\r\n" +
                              $"{$"Angle {angleIndex + 1} Total Bitrate:",-24}{angleBitrates[angleIndex]} Mbps / {angleTotalBitrates[angleIndex]} Mbps";
                }

                report += $"\r\n" +
                          $"{"All Angles Length:",-24}{totalAngleLength} (h:m:s.ms)\r\n" +
                          $"{"All Angles Size:",-24}{totalAngleSize} bytes\r\n" +
                          $"{"All Angles Bitrate:",-24}{totalAngleBitrate} Mbps\r\n";
            }
            
            //report += $"{"Description:",-24}{""}\r\n";
             
            if (!string.IsNullOrEmpty(bdrom.DiscTitle))
                summary += $"{"Disc Title:",-16}{bdrom.DiscTitle}\r\n";

            summary += $"{"Disc Label:",-16}{bdrom.VolumeLabel}\r\n" +
                       $"{"Disc Size:",-16}{bdrom.Size:N0} bytes\r\n" +
                       $"{"Protection:",-16}{protection}\r\n" +
                       $"{"Playlist:",-16}{title}\r\n" +
                       $"{"Size:",-16}{totalSize} bytes\r\n" +
                       $"{"Length:",-16}{totalLength}\r\n" +
                       $"{"Total Bitrate:",-16}{totalBitrate} Mbps\r\n";

            if (playlist.HasHiddenTracks)
            {
                report += "\r\n(*) Indicates included stream hidden by this playlist.\r\n";
            }

            if (playlist.VideoStreams.Count > 0)
            {
                report += $"\r\n" +
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

                    report += $"{(stream.IsHidden ? "* " : "") + streamName,-24}{streamBitrate,-20}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Video:",-16}{streamName} / {streamBitrate} / {stream.Description}\r\n";
                }
            }

            if (playlist.AudioStreams.Count > 0)
            {
                report += $"\r\n" +
                          $"AUDIO:\r\n" +
                          $"\r\n" +
                          $"{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                          $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsAudioStream) continue;

                    var streamBitrate = $"{(int)Math.Round((double)stream.BitRate / 1000),5:D} kbps";

                    report += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Audio:",-16}{stream.LanguageName} / {stream.CodecName} / {stream.Description}\r\n";
                }
            }

            if (playlist.GraphicsStreams.Count > 0)
            {
                report += $"\r\n" +
                          $"SUBTITLES:\r\n" +
                          $"\r\n" +
                          $"{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                          $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsGraphicsStream) continue;

                    var streamBitrate = $"{(double)stream.BitRate / 1000,5:F2} kbps";

                    report += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";

                    summary += $"{(stream.IsHidden ? "* " : "") + "Subtitle:",-16}{stream.LanguageName} / {streamBitrate.Trim()}\r\n";
                }
            }

            if (playlist.TextStreams.Count > 0)
            {
                report += $"\r\n" +
                          $"TEXT:\r\n" +
                          $"\r\n" +
                          $"{"Codec",-32}{"Language",-16}{"Bitrate",-16}{"Description",-16}\r\n" +
                          $"{"---------------",-32}{"-------------",-16}{"-------------",-16}{"-----------",-16}\r\n";

                foreach (var stream in playlist.SortedStreams)
                {
                    if (!stream.IsTextStream) continue;

                    var streamBitrate = $"{(double)stream.BitRate / 1000,5:F2} kbps";

                    report += $"{(stream.IsHidden ? "* " : "") + stream.CodecName,-32}{stream.LanguageName,-16}{streamBitrate,-16}{stream.Description,-16}\r\n";
                }
            }

            report += $"\r\n" +
                      $"FILES:\r\n" +
                      $"\r\n" +
                      $"{"Name",-16}{"Time In",-16}{"Length",-16}{"Size",-16}{"Total Bitrate",-16}\r\n" +
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

                report += $"{clipName,-16}{clipTimeIn,-16}{clipLength,-16}{clipSize,-16}{clipBitrate,-16}\r\n";
            }

            report += "\r\n" +
                      "CHAPTERS:\r\n" +
                      "\r\n";

            report += $"{"Number",-16}{"Time In",-16}{"Length",-16}{"Avg Video Rate",-16}{"Max 1-Sec Rate",-16}{"Max 1-Sec Time",-16}{"Max 5-Sec Rate",-16}" +
                      $"{"Max 5-Sec Time",-16}{"Max 10Sec Rate",-16}{"Max 10Sec Time",-16}{"Avg Frame Size",-16}{"Max Frame Size",-16}{"Max Frame Time",-16}\r\n";

            report += $"{"------",-16}{"-------------",-16}{"-------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}{"--------------",-16}" +
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

            var diagPID  = playlist.VideoStreams[0].PID;

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

                    report += $"{chapterIndex,-16}{$"{chapterStartSpan:h\\:mm\\:ss\\.fff}",-16}{$"{chapterLengthSpan:h\\:mm\\:ss\\.fff}",-16}{$"{Math.Round(chapterBitrate / 1000),6:N0} kbps",-16}" +
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

            if (BDInfoGuiSettings.GenerateStreamDiagnostics)
            {           
                report += $"\r\n" +
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

                        report += $"{clipName,-16}{$"{clipStream.PID} (0x{clipStream.PID:X})",-16}{$"0x{(byte)clipStream.StreamType:X2}",-16}{clipStream.CodecShortName,-16}{language,-24}{clipSeconds,-24}" +
                                  $"{clipBitRate,-24}{$"{clipStream.PayloadBytes,14:N0}",-16}{$"{clipStream.PacketCount,11:N0}",-16}\r\n";
                    }
                }
            }

            report += "\r\n" +
                      "[/code]\r\n" +
                      "<---- END FORUMS PASTE ---->\r\n" +
                      "\r\n";

            if (BDInfoGuiSettings.GenerateTextSummary)
            {
                report += $"QUICK SUMMARY:\r\n" +
                          $"\r\n" +
                          $"{summary}\r\n";
            }

            if (BDInfoGuiSettings.AutosaveReport && reportFile != null)
            {
                try
                {
                    reportFile.Write(report);
                }
                catch
                {
                    // ignored
                }
            }
            textBoxReport.Text += report;
            report = "";
            GC.Collect();
        }

        if (BDInfoGuiSettings.AutosaveReport && reportFile != null)
        {
            try
            {
                reportFile.Write(report);
            }
            catch
            {
                // ignored
            }
        }
        textBoxReport.Text += report;

        reportFile?.Close();

        textBoxReport.Select(0, 0);
        comboBoxPlaylist.SelectedIndex = 0;
        comboBoxChartType.SelectedIndex = 0;
    }

    private void textBoxReport_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && (e.KeyCode == Keys.A))
        {
            textBoxReport.SelectAll();
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
        else
        {
            e.Handled = false;
        }
    }

    private void comboBoxPlaylist_SelectedIndexChanged(object sender, EventArgs e)
    {
        var playlist = (TSPlaylistFile)comboBoxPlaylist.SelectedItem;

        comboBoxAngle.Items.Clear();
        for (var i = 0; i <= playlist.AngleStreams.Count; i++)
        {
            comboBoxAngle.Items.Add(i);
        }
        if (comboBoxAngle.Items.Count > 0)
        {
            comboBoxAngle.SelectedIndex = 0;
        }

        comboBoxStream.Items.Clear();
        foreach (var videoStream in playlist.VideoStreams)
        {
            comboBoxStream.Items.Add(videoStream);
        }
        if (comboBoxStream.Items.Count > 0)
        {
            comboBoxStream.SelectedIndex = 0;
        }
    }

    private void buttonChart_Click(object sender, EventArgs e)
    {
        if (_playlists == null ||
            comboBoxPlaylist.SelectedItem == null ||
            comboBoxStream.SelectedItem == null ||
            comboBoxAngle.SelectedItem == null ||
            comboBoxChartType.SelectedItem == null)
        {
            return;
        }

        var playlist = (TSPlaylistFile)comboBoxPlaylist.SelectedItem;
        var videoStream = (TSVideoStream)comboBoxStream.SelectedItem;
        var angleIndex = (int)comboBoxAngle.SelectedItem;
        var chartType = comboBoxChartType.SelectedItem.ToString();

        var chart = new FormChart();
        chart.Generate(chartType, playlist, videoStream.PID, angleIndex);
        chart.Show();
    }

    private void buttonCopy_Click(object sender, EventArgs e)
    {
        Clipboard.SetText(textBoxReport.Text);
    }
}