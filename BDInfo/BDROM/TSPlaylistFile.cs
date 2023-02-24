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

using System.Diagnostics;
using BDInfoLib.BDROM.IO;
using Stream = System.IO.Stream;
using BinaryReader = System.IO.BinaryReader;
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Global

namespace BDInfoLib.BDROM;

public class TSPlaylistFile
{
    private readonly IFileInfo _fileInfo;
    public string FileType;
    public bool IsInitialized;
    public string Name;
    public BDROM BDROM;
    public bool HasHiddenTracks;
    public bool HasLoops;
    public bool IsCustom;

    public bool MVCBaseViewR;

    public List<double> Chapters = new();

    public Dictionary<ushort, TSStream> Streams = new();
    public Dictionary<ushort, TSStream> PlaylistStreams = new();
    public List<TSStreamClip> StreamClips = new();
    public List<Dictionary<ushort, TSStream>> AngleStreams = new();
    public List<Dictionary<double, TSStreamClip>> AngleClips = new();
    public int AngleCount;

    public List<TSStream> SortedStreams = new();
    public List<TSVideoStream> VideoStreams = new();
    public List<TSAudioStream> AudioStreams = new();
    public List<TSTextStream> TextStreams = new();
    public List<TSGraphicsStream> GraphicsStreams = new();

    public TSPlaylistFile(BDROM bdrom, IFileInfo fileInfo)
    {
        BDROM = bdrom;
        _fileInfo = fileInfo;
        Name = fileInfo.Name.ToUpper();
    }

    public TSPlaylistFile(BDROM bdrom, string name, List<TSStreamClip> clips)
    {
        BDROM = bdrom;
        Name = name;
        IsCustom = true;
        foreach (var clip in clips)
        {
            var newClip = new TSStreamClip(clip.StreamFile, clip.StreamClipFile)
            {
                Name = clip.Name,
                TimeIn = clip.TimeIn,
                TimeOut = clip.TimeOut
            };

            newClip.Length = newClip.TimeOut - newClip.TimeIn;
            newClip.RelativeTimeIn = TotalLength;
            newClip.RelativeTimeOut = newClip.RelativeTimeIn + newClip.Length;
            newClip.AngleIndex = clip.AngleIndex;
            newClip.Chapters.Add(clip.TimeIn);
            StreamClips.Add(newClip);

            if (newClip.AngleIndex > AngleCount)
            {
                AngleCount = newClip.AngleIndex;
            }
            if (newClip.AngleIndex == 0)
            {
                Chapters.Add(newClip.RelativeTimeIn);
            }
        }
        LoadStreamClips();
        IsInitialized = true;
    }

    public override string ToString()
    {
        return Name;
    }

    public ulong InterleavedFileSize
    {
        get
        {
            return StreamClips.Aggregate<TSStreamClip, ulong>(0, (current, clip) => current + clip.InterleavedFileSize);
        }
    }
    public ulong FileSize
    {
        get
        {
            return StreamClips.Aggregate<TSStreamClip, ulong>(0, (current, clip) => current + clip.FileSize);
        }
    }
    public double TotalLength
    {
        get
        {
            return StreamClips.Where(clip => clip.AngleIndex == 0).Sum(clip => clip.Length);
        }
    }

    public double TotalAngleLength
    {
        get
        {
            return StreamClips.Sum(clip => clip.Length);
        }
    }

    public ulong TotalSize
    {
        get
        {
            return StreamClips.Where(clip => clip.AngleIndex == 0).Aggregate<TSStreamClip, ulong>(0, (current, clip) => current + clip.PacketSize);
        }
    }

    public ulong TotalAngleSize
    {
        get
        {
            return StreamClips.Aggregate<TSStreamClip, ulong>(0, (current, clip) => current + clip.PacketSize);
        }
    }

    public ulong TotalBitRate
    {
        get
        {
            if (TotalLength > 0)
            {
                return (ulong)Math.Round(((TotalSize * 8.0) / TotalLength));
            }
            return 0;
        }
    }

    public ulong TotalAngleBitRate
    {
        get
        {
            if (TotalAngleLength > 0)
            {
                return (ulong)Math.Round(((TotalAngleSize * 8.0) / TotalAngleLength));
            }
            return 0;
        }
    }

    public string GetFilePath()
    {
        return !string.IsNullOrEmpty(_fileInfo.FullName) ? _fileInfo.FullName : string.Empty;
    }

    public void Scan(Dictionary<string, TSStreamFile> streamFiles, Dictionary<string, TSStreamClipFile> streamClipFiles)
    {
        Stream fileStream = null;
        BinaryReader fileReader = null;

        try
        {
            Streams.Clear();
            StreamClips.Clear();

            fileStream = _fileInfo.OpenRead();
            fileReader = new BinaryReader(fileStream!);
            var streamLength = (ulong)fileStream.Length;

            var data = new byte[streamLength];
            var dataLength = fileReader.Read(data, 0, data.Length);

            var pos = 0;

            FileType = ToolBox.ReadString(data, 8, ref pos);
            if (FileType != "MPLS0100" && FileType != "MPLS0200" && FileType != "MPLS0300")
            {
                throw new Exception($"Playlist {_fileInfo.Name} has an unknown file type {FileType}.");
            }

            var playlistOffset = ReadInt32(data, ref pos);
            var chaptersOffset = ReadInt32(data, ref pos);
            var extensionsOffset = ReadInt32(data, ref pos);

            // misc flags
            pos = 0x38;
            var miscFlags = ReadByte(data, ref pos);
                
            // MVC_Base_view_R_flag is stored in 4th bit
            MVCBaseViewR = (miscFlags & 0x10) != 0;

            pos = playlistOffset;

            var playlistLength = ReadInt32(data, ref pos);
            var playlistReserved = ReadInt16(data, ref pos);
            var itemCount = ReadInt16(data, ref pos);
            var subitemCount = ReadInt16(data, ref pos);

            var chapterClips = new List<TSStreamClip>();
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var itemStart = pos;
                var itemLength = ReadInt16(data, ref pos);
                var itemName = ToolBox.ReadString(data, 5, ref pos);
                var itemType = ToolBox.ReadString(data, 4, ref pos);

                TSStreamFile streamFile = null;
                var streamFileName = $"{itemName}.M2TS";
                if (streamFiles.ContainsKey(streamFileName))
                {
                    streamFile = streamFiles[streamFileName];
                }
                if (streamFile == null)
                {
                    Debug.WriteLine($"Playlist {_fileInfo.Name} referenced missing file {streamFileName}.");
                }

                TSStreamClipFile streamClipFile = null;
                var streamClipFileName = $"{itemName}.CLPI";
                if (streamClipFiles.ContainsKey(streamClipFileName))
                {
                    streamClipFile = streamClipFiles[streamClipFileName];
                }
                if (streamClipFile == null)
                {
                    throw new Exception($"Playlist {_fileInfo.Name} referenced missing file {streamFileName}.");
                }

                pos += 1;
                var multiangle = (data[pos] >> 4) & 0x01;
                var condition = data[pos] & 0x0F;
                pos += 2;

                var inTime = ReadInt32(data, ref pos);
                if (inTime < 0) inTime &= 0x7FFFFFFF;
                var timeIn = (double)inTime / 45000;

                var outTime = ReadInt32(data, ref pos);
                if (outTime < 0) outTime &= 0x7FFFFFFF;
                var timeOut = (double)outTime / 45000;

                var streamClip = new TSStreamClip(streamFile, streamClipFile)
                {
                    Name = streamFileName, //TODO
                    TimeIn = timeIn,
                    TimeOut = timeOut
                };

                streamClip.Length = streamClip.TimeOut - streamClip.TimeIn;
                streamClip.RelativeTimeIn = TotalLength;
                streamClip.RelativeTimeOut = streamClip.RelativeTimeIn + streamClip.Length;
                streamClip.RelativeLength = streamClip.Length / TotalLength;
                StreamClips.Add(streamClip);
                chapterClips.Add(streamClip);

                pos += 12;
                if (multiangle > 0)
                {
                    int angles = data[pos];
                    pos += 2;
                    for (var angle = 0; angle < angles - 1; angle++)
                    {
                        var angleName = ToolBox.ReadString(data, 5, ref pos);
                        var angleType = ToolBox.ReadString(data, 4, ref pos);
                        pos += 1;

                        TSStreamFile angleFile = null;
                        var angleFileName = $"{angleName}.M2TS";
                        if (streamFiles.ContainsKey(angleFileName))
                        {
                            angleFile = streamFiles[angleFileName];
                        }
                        if (angleFile == null)
                        {
                            throw new Exception($"Playlist {_fileInfo.Name} referenced missing angle file {angleFileName}.");
                        }

                        TSStreamClipFile angleClipFile = null;
                        var angleClipFileName = $"{angleName}.CLPI";
                        if (streamClipFiles.ContainsKey(angleClipFileName))
                        {
                            angleClipFile = streamClipFiles[angleClipFileName];
                        }
                        if (angleClipFile == null)
                        {
                            throw new Exception($"Playlist {_fileInfo.Name} referenced missing angle file {angleClipFileName}.");
                        }

                        var angleClip = new TSStreamClip(angleFile, angleClipFile)
                        {
                            AngleIndex = angle + 1,
                            TimeIn = streamClip.TimeIn,
                            TimeOut = streamClip.TimeOut,
                            RelativeTimeIn = streamClip.RelativeTimeIn,
                            RelativeTimeOut = streamClip.RelativeTimeOut,
                            Length = streamClip.Length
                        };
                        StreamClips.Add(angleClip);
                    }
                    if (angles - 1 > AngleCount) AngleCount = angles - 1;
                }

                var streamInfoLength = ReadInt16(data, ref pos);
                pos += 2;
                int streamCountVideo = data[pos++];
                int streamCountAudio = data[pos++];
                int streamCountPg = data[pos++];
                int streamCountIg = data[pos++];
                int streamCountSecondaryAudio = data[pos++];
                int streamCountSecondaryVideo = data[pos++];
                int streamCountPip = data[pos++];
                pos += 5;

#if DEBUG
                Debug.WriteLine(
                    $"{Name} : {streamFileName} -> V:{streamCountVideo} A:{streamCountAudio} PG:{streamCountPg} IG:{streamCountIg} 2A:{streamCountSecondaryAudio} 2V:{streamCountSecondaryVideo} PIP:{streamCountPip}");
#endif

                for (var i = 0; i < streamCountVideo; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream == null) continue;
                    if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                        PlaylistStreams[stream.PID] = stream;
                }
                for (var i = 0; i < streamCountAudio; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream == null) continue;

                    if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                        PlaylistStreams[stream.PID] = stream;
                }
                for (var i = 0; i < streamCountPg; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream == null) continue;

                    if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                        PlaylistStreams[stream.PID] = stream;
                }
                for (var i = 0; i < streamCountIg; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream == null) continue;

                    if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                        PlaylistStreams[stream.PID] = stream;
                }
                for (var i = 0; i < streamCountSecondaryAudio; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream != null)
                    {
                        if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                            PlaylistStreams[stream.PID] = stream;
                    }
                    pos += 2;
                }
                for (var i = 0; i < streamCountSecondaryVideo; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream != null)
                    {
                        if (!PlaylistStreams.ContainsKey(stream.PID) || streamClip.RelativeLength > 0.01)
                            PlaylistStreams[stream.PID] = stream;
                    }

                    pos += 6;
                }
                /*
                 * TODO
                 * 
                for (var i = 0; i < streamCountPIP; i++)
                {
                    var stream = CreatePlaylistStream(data, ref pos);
                    if (stream != null) PlaylistStreams[stream.PID] = stream;
                }
                */

                pos += itemLength - (pos - itemStart) + 2;
            }

            pos = chaptersOffset + 4;

            var chapterCount = ReadInt16(data, ref pos);

            for (var chapterIndex = 0; chapterIndex < chapterCount; chapterIndex++)
            {
                int chapterType = data[pos+1];

                if (chapterType == 1)
                {
                    var streamFileIndex = (data[pos + 2] << 8) + data[pos + 3];

                    var chapterTime = ((long)data[pos + 4] << 24) +
                                      ((long)data[pos + 5] << 16) +
                                      ((long)data[pos + 6] << 8) +
                                      data[pos + 7];

                    var streamClip = chapterClips[streamFileIndex];

                    var chapterSeconds = (double)chapterTime / 45000;

                    var relativeSeconds = chapterSeconds - streamClip.TimeIn + streamClip.RelativeTimeIn;

                    // TODO: Ignore short last chapter?
                    if (TotalLength - relativeSeconds > 1.0)
                    {
                        streamClip.Chapters.Add(chapterSeconds);
                        this.Chapters.Add(relativeSeconds);
                    }
                }
                else
                {
                    // TODO: Handle other chapter types?
                }
                pos += 14;
            }
        }
        finally
        {
            fileReader?.Close();
            fileStream?.Close();
        }
    }

    public void Initialize()
    {
        LoadStreamClips();

        var clipTimes = new Dictionary<string, List<double>>();
        foreach (var clip in StreamClips.Where(clip => clip!.AngleIndex == 0))
        {
            if (clip.Name != null && clipTimes.ContainsKey(clip.Name))
            {
                if (clipTimes[clip.Name].Contains(clip.TimeIn))
                {
                    HasLoops = true;
                    break;
                }

                clipTimes[clip.Name].Add(clip.TimeIn);
            }
            else
            {
                if (clip.Name != null) 
                    clipTimes[clip.Name] = new List<double> { clip.TimeIn };
            }
        }
        ClearBitrates();
        IsInitialized = true;
    }

    protected static TSStream CreatePlaylistStream(byte[] data, ref int pos)
    {
        TSStream stream = null;

        var start = pos;

        int headerLength = data[pos++];
        var headerPos = pos;
        int headerType = data[pos++];

        var pid = 0;
        var subpathid = 0;
        var subclipid = 0;

        switch (headerType)
        {
            case 1:
                pid = ReadInt16(data, ref pos);
                break;
            case 2:
                subpathid = data[pos++];
                subclipid = data[pos++];
                pid = ReadInt16(data, ref pos);
                break;
            case 3:
                subpathid = data[pos++];
                pid = ReadInt16(data, ref pos);
                break;
            case 4:
                subpathid = data[pos++];
                subclipid = data[pos++];
                pid = ReadInt16(data, ref pos);
                break;
        }

        pos = headerPos + headerLength;

        int streamLength = data[pos++];
        var streamPos = pos;

        var streamType = (TSStreamType)data[pos++];
        switch (streamType)
        {
            case TSStreamType.MVC_VIDEO:
                // TODO
                break;

            case TSStreamType.HEVC_VIDEO:
            case TSStreamType.AVC_VIDEO:
            case TSStreamType.MPEG1_VIDEO:
            case TSStreamType.MPEG2_VIDEO:
            case TSStreamType.VC1_VIDEO:

                var videoFormat = (TSVideoFormat)(data[pos] >> 4);
                var frameRate = (TSFrameRate)(data[pos] & 0xF);
                var aspectRatio = (TSAspectRatio)(data[pos + 1] >> 4);

                stream = new TSVideoStream
                {
                    VideoFormat = videoFormat,
                    AspectRatio = aspectRatio,
                    FrameRate = frameRate
                };

#if DEBUG
                Debug.WriteLine($"\t{pid} {streamType} {videoFormat} {frameRate} {aspectRatio}");
#endif
                break;

            case TSStreamType.AC3_AUDIO:
            case TSStreamType.AC3_PLUS_AUDIO:
            case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
            case TSStreamType.AC3_TRUE_HD_AUDIO:
            case TSStreamType.DTS_AUDIO:
            case TSStreamType.DTS_HD_AUDIO:
            case TSStreamType.DTS_HD_MASTER_AUDIO:
            case TSStreamType.DTS_HD_SECONDARY_AUDIO:
            case TSStreamType.LPCM_AUDIO:
            case TSStreamType.MPEG1_AUDIO:
            case TSStreamType.MPEG2_AUDIO:
            case TSStreamType.MPEG2_AAC_AUDIO:
            case TSStreamType.MPEG4_AAC_AUDIO:

                int audioFormat = ReadByte(data, ref pos);

                var channelLayout = (TSChannelLayout)(audioFormat >> 4);
                var sampleRate = (TSSampleRate)(audioFormat & 0xF);

                var audioLanguage = ToolBox.ReadString(data, 3, ref pos);

                stream = new TSAudioStream
                {
                    ChannelLayout = channelLayout,
                    SampleRate = TSAudioStream.ConvertSampleRate(sampleRate),
                    LanguageCode = audioLanguage
                };

#if DEBUG
                Debug.WriteLine($"\t{pid} {streamType} {audioLanguage} {channelLayout} {sampleRate}");
#endif
                break;

            case TSStreamType.INTERACTIVE_GRAPHICS:
            case TSStreamType.PRESENTATION_GRAPHICS:

                var graphicsLanguage = ToolBox.ReadString(data, 3, ref pos);

                stream = new TSGraphicsStream
                {
                    LanguageCode = graphicsLanguage
                };

                if (data[pos] != 0)
                {
                    // TODO
                }

#if DEBUG
                Debug.WriteLine($"\t{pid} {streamType} {graphicsLanguage}");
#endif
                break;

            case TSStreamType.SUBTITLE:

                int code = ReadByte(data, ref pos); // TODO
                var textLanguage = ToolBox.ReadString(data, 3, ref pos);

                stream = new TSTextStream
                {
                    LanguageCode = textLanguage
                };

#if DEBUG
                Debug.WriteLine($"\t{pid} {streamType} {textLanguage}");
#endif

                break;
        }

        pos = streamPos + streamLength;

        if (stream == null) return null;

        stream.PID = (ushort)pid;
        stream.StreamType = streamType;

        return stream;
    }

    private void LoadStreamClips()
    {
        AngleClips.Clear();
        if (AngleCount > 0)
        {
            for (var angleIndex = 0; angleIndex < AngleCount; angleIndex++)
            {
                AngleClips.Add(new Dictionary<double, TSStreamClip>());
            }
        }

        TSStreamClip referenceClip = null;
        if (StreamClips.Count > 0)
        {
            referenceClip = StreamClips[0];
        }
        foreach (var clip in StreamClips)
        {
            if (referenceClip?.StreamFile == null && clip.StreamFile != null)
                referenceClip = clip;

            if (clip.StreamClipFile.Streams.Count > referenceClip?.StreamClipFile.Streams.Count && clip.RelativeLength > 0.01)
            {
                referenceClip = clip;
            }
            else if (clip.Length > referenceClip?.Length && clip.StreamFile != null)
            {
                referenceClip = clip;
            }

            if (AngleCount <= 0) continue;

            if (clip.AngleIndex == 0)
            {
                for (var angleIndex = 0; angleIndex < AngleCount; angleIndex++)
                {
                    AngleClips[angleIndex][clip.RelativeTimeIn] = clip;
                }
            }
            else
            {
                AngleClips[clip.AngleIndex - 1][clip.RelativeTimeIn] = clip;
            }
        }

        if (referenceClip == null) return;

        foreach (var clipStream in referenceClip.StreamClipFile.Streams.Values!)
        {
            if (clipStream != null && Streams.ContainsKey(clipStream.PID)) continue;

            var stream = clipStream?.Clone();
            if (clipStream != null) Streams[clipStream.PID] = stream;

            if (!IsCustom && !PlaylistStreams.ContainsKey(stream.PID))
            {
                stream.IsHidden = true;
                HasHiddenTracks = true;
            }

            if (stream.IsVideoStream)
            {
                VideoStreams.Add((TSVideoStream)stream);
            }
            else if (stream.IsAudioStream)
            {
                AudioStreams.Add((TSAudioStream)stream);
            }
            else if (stream.IsGraphicsStream)
            {
                GraphicsStreams.Add((TSGraphicsStream)stream);
            }
            else if (stream.IsTextStream)
            {
                TextStreams.Add((TSTextStream)stream);
            }
        }

        if (referenceClip.StreamFile != null)
        {
            // TODO: Better way to add this in?
            if (BDInfoSettings.EnableSSIF &&
                referenceClip.StreamFile.InterleavedFile != null &&
                referenceClip.StreamFile.Streams.ContainsKey(4114) && 
                !Streams.ContainsKey(4114))
            {
                var stream = referenceClip.StreamFile.Streams[4114].Clone();
                Streams[4114] = stream;
                if (stream.IsVideoStream)
                {
                    VideoStreams.Add((TSVideoStream)stream);
                }
            }

            foreach (var clipStream in referenceClip.StreamFile.Streams.Values)
            {
                if (!Streams.ContainsKey(clipStream.PID)) continue;

                var stream = Streams[clipStream.PID];

                if (stream.StreamType != clipStream.StreamType) continue;

                if (clipStream.BitRate > stream.BitRate)
                {
                    stream.BitRate = clipStream.BitRate;
                }
                stream.IsVBR = clipStream.IsVBR;

                if (stream.IsVideoStream &&
                    clipStream.IsVideoStream)
                {
                    ((TSVideoStream)stream).EncodingProfile =
                        ((TSVideoStream)clipStream).EncodingProfile;
                    ((TSVideoStream) stream).ExtendedData = 
                        ((TSVideoStream) clipStream).ExtendedData;
                }
                else if (stream.IsAudioStream &&
                         clipStream.IsAudioStream)
                {
                    var audioStream = (TSAudioStream)stream;
                    var clipAudioStream = (TSAudioStream)clipStream;

                    if (clipAudioStream.ChannelCount > audioStream.ChannelCount)
                    {
                        audioStream.ChannelCount = clipAudioStream.ChannelCount;
                    }
                    if (clipAudioStream.LFE > audioStream.LFE)
                    {
                        audioStream.LFE = clipAudioStream.LFE;
                    }
                    if (clipAudioStream.SampleRate > audioStream.SampleRate)
                    {
                        audioStream.SampleRate = clipAudioStream.SampleRate;
                    }
                    if (clipAudioStream.BitDepth > audioStream.BitDepth)
                    {
                        audioStream.BitDepth = clipAudioStream.BitDepth;
                    }
                    if (clipAudioStream.DialNorm < audioStream.DialNorm)
                    {
                        audioStream.DialNorm = clipAudioStream.DialNorm;
                    }
                    if (clipAudioStream.AudioMode != TSAudioMode.Unknown)
                    {
                        audioStream.AudioMode = clipAudioStream.AudioMode;
                    }
                    if (!clipAudioStream.HasExtensions.Equals(audioStream.HasExtensions))
                    {
                        audioStream.HasExtensions = clipAudioStream.HasExtensions;
                    }
                    if (!Equals(clipAudioStream.ExtendedData, audioStream.ExtendedData))
                    {
                        audioStream.ExtendedData = clipAudioStream.ExtendedData;
                    }
                    if (clipAudioStream.CoreStream != null)
                    {
                        audioStream.CoreStream = (TSAudioStream)clipAudioStream.CoreStream.Clone();
                    }
                }
                else if (stream.IsGraphicsStream &&
                         clipStream.IsGraphicsStream)
                {
                    var graphicsStream = (TSGraphicsStream)stream;
                    var clipGraphicsStream = (TSGraphicsStream)clipStream;
                            
                    graphicsStream.Captions = clipGraphicsStream.Captions;
                    graphicsStream.ForcedCaptions = clipGraphicsStream.ForcedCaptions;
                    graphicsStream.Width = clipGraphicsStream.Width;
                    graphicsStream.Height = clipGraphicsStream.Height;
                    graphicsStream.CaptionIDs = clipGraphicsStream.CaptionIDs;
                }
            }
        }

        for (var i = 0; i < AngleCount; i++)
        {
            AngleStreams.Add(new Dictionary<ushort, TSStream>());
        }

        if (!BDInfoSettings.KeepStreamOrder)
        {
            VideoStreams.Sort(CompareVideoStreams);
        }
        foreach (var stream in VideoStreams)
        {
            SortedStreams.Add(stream);
            for (var i = 0; i < AngleCount; i++)
            {
                var angleStream = stream.Clone();
                angleStream.AngleIndex = i + 1;
                AngleStreams[i][angleStream.PID] = angleStream;
                SortedStreams.Add(angleStream);
            }
        }

        if (!BDInfoSettings.KeepStreamOrder)
        {
            AudioStreams.Sort(CompareAudioStreams);
        }
        foreach (var stream in AudioStreams)
        {
            SortedStreams.Add(stream);
        }

        if (!BDInfoSettings.KeepStreamOrder)
        {
            GraphicsStreams.Sort(CompareGraphicsStreams);
        }
        foreach (var stream in GraphicsStreams)
        {
            SortedStreams.Add(stream);
        }

        if (!BDInfoSettings.KeepStreamOrder)
        {
            TextStreams.Sort(CompareTextStreams);
        }
        foreach (var stream in TextStreams)
        {
            SortedStreams.Add(stream);
        }
    }

    public void ClearBitrates()
    {
        foreach (var clip in StreamClips)
        {
            clip.PayloadBytes = 0;
            clip.PacketCount = 0;
            clip.PacketSeconds = 0;

            if (clip.StreamFile == null) continue;

            foreach (var stream in clip.StreamFile.Streams.Values)
            {
                stream.PayloadBytes = 0;
                stream.PacketCount = 0;
                stream.PacketSeconds = 0;
            }

            clip.StreamFile.StreamDiagnostics.Clear();
        }

        foreach (var stream in SortedStreams)
        {
            stream.PayloadBytes = 0;
            stream.PacketCount = 0;
            stream.PacketSeconds = 0;
        }
    }

    public bool IsValid
    {
        get
        {
            if (!IsInitialized) return false;

            if (BDInfoSettings.FilterShortPlaylists &&
                TotalLength < BDInfoSettings.FilterShortPlaylistsValue)
            {
                return false;
            }

            return !HasLoops || !BDInfoSettings.FilterLoopingPlaylists;
        }
    }

    public static int CompareVideoStreams(TSVideoStream x, TSVideoStream y)
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

        if (x.Height > y.Height)
        {
            return -1;
        }

        if (y.Height > x.Height)
        {
            return 1;
        }

        if (x.PID > y.PID)
        {
            return 1;
        }

        if (y.PID > x.PID)
        {
            return -1;
        }

        return 0;
    }

    public static int CompareAudioStreams(TSAudioStream x, TSAudioStream y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null && y != null)
        {
            return -1;
        }

        if (x != null && y == null)
        {
            return 1;
        }

        if (x.ChannelCount > y.ChannelCount)
        {
            return -1;
        }

        if (y.ChannelCount > x.ChannelCount)
        {
            return 1;
        }

        var sortX = GetStreamTypeSortIndex(x.StreamType);
        var sortY = GetStreamTypeSortIndex(y.StreamType);

        if (sortX > sortY)
        {
            return -1;
        }

        if (sortY > sortX)
        {
            return 1;
        }

        if (x.LanguageCode == "eng")
        {
            return -1;
        }

        if (y.LanguageCode == "eng")
        {
            return 1;
        }

        if (x.LanguageCode != y.LanguageCode)
            return string.CompareOrdinal(x.LanguageName, y.LanguageName);
            
        if (x.PID < y.PID)
        {
            return -1;
        }

        return y.PID < x.PID ? 1 : 0;
    }

    public static int CompareTextStreams(TSTextStream x, TSTextStream y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null && y != null)
        {
            return -1;
        }

        if (x != null && y == null)
        {
            return 1;
        }

        if (x.LanguageCode == "eng")
        {
            return -1;
        }

        if (y.LanguageCode == "eng")
        {
            return 1;
        }

        if (x.LanguageCode != y.LanguageCode) 
            return string.CompareOrdinal(x.LanguageName, y.LanguageName);

        if (x.PID > y.PID)
        {
            return 1;
        }

        if (y.PID > x.PID)
        {
            return -1;
        }

        return 0;

    }

    private static int CompareGraphicsStreams(TSGraphicsStream x, TSGraphicsStream y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null && y != null)
        {
            return -1;
        }

        if (x != null && y == null)
        {
            return 1;
        }

        var sortX = GetStreamTypeSortIndex(x.StreamType);
        var sortY = GetStreamTypeSortIndex(y.StreamType);

        if (sortX > sortY)
        {
            return -1;
        }

        if (sortY > sortX)
        {
            return 1;
        }

        if (x.LanguageCode == "eng")
        {
            return -1;
        }

        if (y.LanguageCode == "eng")
        {
            return 1;
        }

        if (x.LanguageCode != y.LanguageCode) 
            return string.CompareOrdinal(x.LanguageName, y.LanguageName);

        if (x.PID > y.PID)
        {
            return 1;
        }

        if (y.PID > x.PID)
        {
            return -1;
        }

        return 0;

    }

    private static int GetStreamTypeSortIndex(TSStreamType? streamType)
    {
        return streamType switch
        {
            TSStreamType.Unknown => 0,
            TSStreamType.MPEG1_VIDEO => 1,
            TSStreamType.MPEG2_VIDEO => 2,
            TSStreamType.AVC_VIDEO => 3,
            TSStreamType.VC1_VIDEO => 4,
            TSStreamType.MVC_VIDEO => 5,
            TSStreamType.HEVC_VIDEO => 6,
            TSStreamType.MPEG1_AUDIO => 1,
            TSStreamType.MPEG2_AUDIO => 2,
            TSStreamType.AC3_PLUS_SECONDARY_AUDIO => 3,
            TSStreamType.DTS_HD_SECONDARY_AUDIO => 4,
            TSStreamType.AC3_AUDIO => 5,
            TSStreamType.DTS_AUDIO => 6,
            TSStreamType.AC3_PLUS_AUDIO => 7,
            TSStreamType.MPEG2_AAC_AUDIO => 8,
            TSStreamType.MPEG4_AAC_AUDIO => 9,
            TSStreamType.DTS_HD_AUDIO => 10,
            TSStreamType.AC3_TRUE_HD_AUDIO => 11,
            TSStreamType.DTS_HD_MASTER_AUDIO => 12,
            TSStreamType.LPCM_AUDIO => 13,
            TSStreamType.SUBTITLE => 1,
            TSStreamType.INTERACTIVE_GRAPHICS => 2,
            TSStreamType.PRESENTATION_GRAPHICS => 3,
            _ => 0
        };
    }

    protected static int ReadInt32(byte[] data, ref int pos)
    {
        var val = (data[pos] << 24) +
                  (data[pos + 1] << 16) +
                  (data[pos + 2] << 8) +
                  data[pos + 3];

        pos += 4;

        return val;
    }

    protected static int ReadInt16(byte[] data, ref int pos)
    {
        var val = (data[pos] << 8) + data[pos + 1];

        pos += 2;

        return val;
    }

    protected static byte ReadByte(byte[] data, ref int pos)
    {
        return data[pos++];
    }
}