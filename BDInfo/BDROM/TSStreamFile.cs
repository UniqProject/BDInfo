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
using System.Numerics;
using BDInfoLib.BDROM.IO;

namespace BDInfoLib.BDROM;

public class TSStreamState
{
    public ulong TransferCount;

    public string StreamTag;

    public ulong TotalPackets;
    public ulong WindowPackets;

    public ulong TotalBytes;
    public ulong WindowBytes;

    public long PeakTransferLength;
    public long PeakTransferRate;

    public double TransferMarker = 0;
    public double TransferInterval = 0;

    public TSStreamBuffer StreamBuffer = new();

    public uint Parse;
    public bool TransferState;
    public int TransferLength;
    public int PacketLength;
    public bool PacketLengthVariable;
    public byte PacketLengthParse;
    public byte PacketParse;

    public byte PTSParse;
    public BigInteger PTS = 0;
    public ulong PTSTemp;
    public BigInteger PTSLast = 0;
    public BigInteger PTSPrev = 0;
    public BigInteger PTSDiff = 0;
    public ulong PTSCount;
    public BigInteger PTSTransfer = 0;

    public byte DTSParse;
    public BigInteger DTSTemp = 0;
    public BigInteger DTSPrev = 0;

    public byte PESHeaderLength;
    public byte PESHeaderFlags;
#if DEBUG && !BETA
    public byte PESHeaderIndex = 0;
    public byte[] PESHeader = new byte[256 + 9];
#endif
}

public class TSPacketParser
{
    public bool SyncState;
    public byte TimeCodeParse = 4;
    public byte PacketLength;
    public byte HeaderParse;

    public uint TimeCode;
    public byte TransportErrorIndicator;
    public byte PayloadUnitStartIndicator;
    public byte TransportPriority;
    public ushort PID;
    public byte TransportScramblingControl;
    public byte AdaptionFieldControl;

    public bool AdaptionFieldState;
    public byte AdaptionFieldParse;
    public byte AdaptionFieldLength;
    public bool VariablePacketEnd;

    public ushort PCRPID = 0xFFFF;
    public byte PCRParse;
    public BigInteger PreviousPCR = 0;
    public BigInteger PCR = 0;
    public BigInteger PCRCount = 0;
    public BigInteger PTSFirst = ulong.MaxValue;
    public BigInteger PTSLast = ulong.MinValue;
    public BigInteger PTSDiff = 0;

    public byte[] PAT = new byte[1024];
    public bool PATSectionStart;
    public byte PATPointerField;
    public uint PATOffset;
    public byte PATSectionLengthParse;
    public ushort PATSectionLength;
    public uint PATSectionParse;
    public bool PATTransferState;
    public byte PATSectionNumber;
    public byte PATLastSectionNumber;

    public ushort TransportStreamId = 0xFFFF;

    public List<TSDescriptor> PMTProgramDescriptors = new();
    public ushort PMTPID = 0xFFFF;
    public Dictionary<ushort, byte[]> PMT = new();
    public bool PMTSectionStart;
    public ushort PMTProgramInfoLength;
    public byte PMTProgramDescriptor;
    public byte PMTProgramDescriptorLengthParse;
    public byte PMTProgramDescriptorLength;
    public ushort PMTStreamInfoLength = 0;
    public uint PMTStreamDescriptorLengthParse = 0;
    public uint PMTStreamDescriptorLength = 0;
    public byte PMTPointerField;
    public uint PMTOffset;
    public uint PMTSectionLengthParse;
    public ushort PMTSectionLength;
    public uint PMTSectionParse;
    public bool PMTTransferState;
    public byte PMTSectionNumber;
    public byte PMTLastSectionNumber;

    public byte PMTTemp;

    public TSStream Stream;
    public TSStreamState StreamState;

    public ulong TotalPackets;
}

public class TSStreamDiagnostics
{
    public ulong Bytes;
    public ulong Packets;
    public double Marker;
    public double Interval;
    public string Tag;
}

public class TSStreamFile
{
    public bool AbortScan = false;
    public IFileInfo FileInfo;
    public string Name;
    public long Size;
    public double Length;

    public TSInterleavedFile InterleavedFile = null;

    private readonly Dictionary<ushort, TSStreamState> _streamStates = new();

    public Dictionary<ushort, TSStream> Streams = new();

    public Dictionary<ushort, List<TSStreamDiagnostics>> StreamDiagnostics = new();

    private List<TSPlaylistFile> _playlists;

    public TSStreamFile(IFileInfo fileInfo)
    {
        FileInfo = fileInfo;
        Name = fileInfo.Name.ToUpper();
    }

    public string DisplayName
    {
        get
        {
            if (BDInfoLibSettings.EnableSSIF &&
                InterleavedFile != null)
            {
                return InterleavedFile.Name;
            }
            return Name;
        }
    }

    public string GetFilePath()
    {
        return !string.IsNullOrEmpty(FileInfo.FullName) ? FileInfo.FullName : string.Empty;
    }

    private bool ScanStream(TSStream stream, TSStreamState streamState, TSStreamBuffer buffer, bool isFullScan)
    {
        streamState.StreamTag = null;

        long bitrate = 0;
        if (stream is { IsAudioStream: true } && streamState.PTSTransfer > 0)
        {
            bitrate = (long)Math.Round(
                buffer.TransferLength * 8.0 /
                ((double)streamState.PTSTransfer / 90000));

            if (bitrate > streamState.PeakTransferRate)
            {
                streamState.PeakTransferRate = bitrate;
            }
        }
        if (buffer.TransferLength > streamState.PeakTransferLength)
        {
            streamState.PeakTransferLength = buffer.TransferLength;
        }

        buffer.BeginRead();
        switch (stream.StreamType)
        {
            case TSStreamType.MPEG2_VIDEO:
                TSCodecMPEG2.Scan(
                    (TSVideoStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.AVC_VIDEO:
                TSCodecAVC.Scan(
                    (TSVideoStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.MVC_VIDEO:
                TSCodecMVC.Scan(
                    (TSVideoStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.HEVC_VIDEO:
                TSCodecHEVC.Scan(
                    (TSVideoStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.VC1_VIDEO:
                TSCodecVC1.Scan(
                    (TSVideoStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.MPEG1_AUDIO:
            case TSStreamType.MPEG2_AUDIO:
                TSCodecMPA.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.MPEG2_AAC_AUDIO:
            case TSStreamType.MPEG4_AAC_AUDIO:
                TSCodecAAC.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.AC3_AUDIO:
                TSCodecAC3.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.AC3_PLUS_AUDIO:
            case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                TSCodecAC3.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.AC3_TRUE_HD_AUDIO:
                TSCodecTrueHD.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.LPCM_AUDIO:
                TSCodecLPCM.Scan(
                    (TSAudioStream)stream, buffer, ref streamState.StreamTag);
                break;

            case TSStreamType.DTS_AUDIO:
                TSCodecDTS.Scan(
                    (TSAudioStream)stream, buffer, bitrate, ref streamState.StreamTag);
                break;

            case TSStreamType.DTS_HD_AUDIO:
            case TSStreamType.DTS_HD_MASTER_AUDIO:
            case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                TSCodecDTSHD.Scan(
                    (TSAudioStream)stream, buffer, bitrate, ref streamState.StreamTag);
                break;

            case TSStreamType.PRESENTATION_GRAPHICS:
                if (isFullScan)
                    TSCodecPGS.Scan(
                        (TSGraphicsStream)stream, buffer, ref streamState.StreamTag);
                else
                    stream.IsInitialized = true;
                break;

            default:
                stream.IsInitialized = true;
                break;
        }
        buffer.EndRead();
        streamState.StreamBuffer.Reset();

        var isAVC = false;
        var isMVC = false;
        foreach (var finishedStream in Streams.Values)
        {
            if (!finishedStream.IsInitialized)
            {
                return false;
            }
            switch (finishedStream.StreamType)
            {
                case TSStreamType.AVC_VIDEO:
                    isAVC = true;
                    break;
                case TSStreamType.MVC_VIDEO:
                    isMVC = true;
                    break;
            }
        }
        if (isMVC && !isAVC)
        {
            return false;
        }
        return true;
    }

    private void UpdateStreamBitrates(ushort ptsPid, BigInteger pts, BigInteger ptsDiff)
    {
        if (_playlists == null) return;

        foreach (var pid in _streamStates.Keys
                     .Where(pid => !Streams.ContainsKey(pid) || !Streams[pid].IsVideoStream || pid == ptsPid)
                     .Where(pid => _streamStates[pid].WindowPackets != 0))
        {
            UpdateStreamBitrate(pid, ptsPid, pts, ptsDiff);
        }

        foreach (var playlist in _playlists)
        {
            var packetSeconds = playlist.StreamClips.Where(clip => clip.AngleIndex == 0).Sum(clip => clip.PacketSeconds);

            if (!(packetSeconds > 0)) continue;

            foreach (var playlistStream in playlist.SortedStreams.Where(playlistStream => playlistStream.IsVBR))
            {
                playlistStream.BitRate = (long)Math.Round(playlistStream.PayloadBytes * 8.0 / packetSeconds);
            }
        }
    }

    private void UpdateStreamBitrate(ushort pid, ushort ptspid, BigInteger pts, BigInteger ptsDiff)
    {
        if (_playlists == null) return;

        var streamState = _streamStates[pid];
        var streamTime = (double)pts / 90000;
        var streamInterval = (double)ptsDiff / 90000;
        var streamOffset = streamTime + streamInterval;

        foreach (var playlist in _playlists)
        {
            foreach (var clip in playlist.StreamClips.Where(clip => clip.Name == Name)
                         .Where(clip =>
                             streamTime == 0 || streamTime >= clip.TimeIn && streamTime <= clip.TimeOut))
            {
                clip.PayloadBytes += streamState.WindowBytes;
                clip.PacketCount += streamState.WindowPackets;

                if (streamOffset > clip.TimeIn &&
                    streamOffset - clip.TimeIn > clip.PacketSeconds)
                {
                    clip.PacketSeconds = streamOffset - clip.TimeIn;
                }

                var playlistStreams = playlist.Streams;
                if (clip.AngleIndex > 0 &&
                    clip.AngleIndex < playlist.AngleStreams.Count + 1)
                {
                    playlistStreams = playlist.AngleStreams[clip.AngleIndex - 1];
                }

                if (!playlistStreams.ContainsKey(pid)) continue;

                var stream = playlistStreams[pid];

                stream.PayloadBytes += streamState.WindowBytes;
                stream.PacketCount += streamState.WindowPackets;

                if (stream.IsVideoStream)
                {
                    stream.PacketSeconds += streamInterval;

                    stream.ActiveBitRate = (long)Math.Round(
                        stream.PayloadBytes * 8.0 /
                        stream.PacketSeconds);
                }

                if (stream.StreamType == TSStreamType.AC3_TRUE_HD_AUDIO && ((TSAudioStream)stream).CoreStream != null)
                {
                    stream.ActiveBitRate -= ((TSAudioStream)stream).CoreStream.BitRate;
                }
            }
        }

        if (Streams.ContainsKey(pid))
        {
            var stream = Streams[pid];
            stream.PayloadBytes += streamState.WindowBytes;
            stream.PacketCount += streamState.WindowPackets;

            if (stream.IsVideoStream)
            {
                var diag = new TSStreamDiagnostics
                {
                    Marker = (double)pts / 90000,
                    Interval = (double)ptsDiff / 90000,
                    Bytes = streamState.WindowBytes,
                    Packets = streamState.WindowPackets,
                    Tag = streamState.StreamTag
                };
                StreamDiagnostics[pid].Add(diag);

                stream.PacketSeconds += streamInterval;
            }
        }
        streamState.WindowPackets = 0;
        streamState.WindowBytes = 0;
    }

    public void Scan(List<TSPlaylistFile> playlists, bool isFullScan)
    {
        if (playlists == null || playlists.Count == 0)
        {
            return;
        }

        _playlists = playlists;
        const int dataSize = 5242880;
        Stream fileStream = null;
        try
        {
            if (BDInfoLibSettings.EnableSSIF && InterleavedFile != null)
            {
                if (InterleavedFile.FileInfo != null)
                    fileStream = InterleavedFile.FileInfo.OpenRead();
            }
            else
            {
                fileStream = FileInfo.OpenRead();
            }

            Size = 0;
            Length = 0;

            Streams.Clear();
            _streamStates.Clear();
            StreamDiagnostics.Clear();

            var parser = new TSPacketParser();

            var buffer = new byte[dataSize];
            int bufferLength;
            while ((bufferLength = fileStream.Read(buffer, 0, buffer.Length)) > 0 && !AbortScan)
            {
                for (var i = 0; i < bufferLength; i++)
                {
                    if (parser.SyncState == false)
                    {
                        if (parser.TimeCodeParse > 0)
                        {
                            parser.TimeCodeParse--;
                            switch (parser.TimeCodeParse)
                            {
                                case 3:
                                    parser.TimeCode = 0;
                                    parser.TimeCode |= ((uint)buffer[i] & 0x3F) << 24;
                                    break;
                                case 2:
                                    parser.TimeCode |= ((uint)buffer[i] & 0xFF) << 16;
                                    break;
                                case 1:
                                    parser.TimeCode |= ((uint)buffer[i] & 0xFF) << 8;
                                    break;
                                case 0:
                                    parser.TimeCode |= (uint)buffer[i] & 0xFF;
                                    break;
                            }
                        }
                        else if (buffer[i] == 0x47)
                        {
                            parser.SyncState = true;
                            parser.PacketLength = 187;
                            parser.TimeCodeParse = 4;
                            parser.HeaderParse = 3;
                        }
                    }
                    else if (parser.HeaderParse > 0)
                    {
                        parser.PacketLength--;
                        parser.HeaderParse--;

                        switch (parser.HeaderParse)
                        {
                            case 2:
                            {
                                parser.TransportErrorIndicator = (byte)(buffer[i] >> 7 & 0x1);
                                parser.PayloadUnitStartIndicator = (byte)(buffer[i] >> 6 & 0x1);
                                parser.TransportPriority = (byte)(buffer[i] >> 5 & 0x1);
                                parser.PID = (ushort)((buffer[i] & 0x1f) << 8);
                            }
                                break;

                            case 1:
                            {
                                parser.PID |= buffer[i];
                                parser.Stream = Streams.ContainsKey(parser.PID) ? Streams[parser.PID] : null;
                                if (!_streamStates.ContainsKey(parser.PID))
                                {
                                    _streamStates[parser.PID] = new TSStreamState();
                                }
                                parser.StreamState = _streamStates[parser.PID];
                                parser.StreamState.TotalPackets++;
                                parser.StreamState.WindowPackets++;
                                parser.TotalPackets++;
                            }
                                break;

                            case 0:
                            {
                                parser.TransportScramblingControl = (byte)(buffer[i] >> 6 & 0x3);
                                parser.AdaptionFieldControl = (byte)(buffer[i] >> 4 & 0x3);

                                if ((parser.AdaptionFieldControl & 0x2) == 0x2)
                                {
                                    parser.AdaptionFieldState = true;
                                }
                                if (parser.PayloadUnitStartIndicator == 1)
                                {
                                    if (parser.PID == 0)
                                    {
                                        parser.PATSectionStart = true;
                                    }
                                    else if (parser.PID == parser.PMTPID)
                                    {
                                        parser.PMTSectionStart = true;
                                    }
                                    else if (parser.StreamState is { TransferState: true })
                                    {
                                        parser.StreamState.TransferState = false;
                                        parser.StreamState.TransferCount++;

                                        var isFinished = ScanStream(parser.Stream,
                                            parser.StreamState,
                                            parser.StreamState.StreamBuffer,
                                            isFullScan);

                                        if (!isFullScan && isFinished)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                                break;
                        }
                    }
                    else if (parser.AdaptionFieldState)
                    {
                        parser.PacketLength--;
                        parser.AdaptionFieldParse = buffer[i];
                        parser.AdaptionFieldLength = buffer[i];
                        parser.AdaptionFieldState = false;
                        parser.VariablePacketEnd = true;
                    }
                    else if (parser.AdaptionFieldParse > 0)
                    {
                        parser.PacketLength--;
                        parser.AdaptionFieldParse--;
                        if (parser.AdaptionFieldLength - parser.AdaptionFieldParse == 1)
                        {
                            if ((buffer[i] & 0x10) == 0x10)
                            {
                                parser.PCRParse = 6;
                                parser.PCR = 0;
                            }
                        }
                        else if (parser.PCRParse > 0)
                        {
                            parser.PCRParse--;
                            parser.PCR = (parser.PCR << 8) + (ulong)buffer[i];
                            if (parser.PCRParse == 0)
                            {
                                parser.PreviousPCR = parser.PCR;
                                parser.PCR = (parser.PCR & 0x1FF) +
                                             (parser.PCR >> 15) * 300;
                            }
                            parser.PCRCount++;
                        }
                        if (parser.PacketLength <= 0)
                        {
                            parser.SyncState = false;
                        }
                    }
                    else
                    {
                        int offset;
                        if (parser.PID == 0)
                        {
                            if (parser.PATTransferState)
                            {
                                if (bufferLength - i > parser.PATSectionLength)
                                {
                                    offset = parser.PATSectionLength;
                                }
                                else
                                {
                                    offset = bufferLength - i;
                                }
                                if (parser.PacketLength <= offset)
                                {
                                    offset = parser.PacketLength;
                                }

                                for (var k = 0; k < offset; k++)
                                {
                                    parser.PAT[parser.PATOffset++] = buffer[i++];
                                    parser.PATSectionLength--;
                                    parser.PacketLength--;
                                }
                                --i;

                                if (parser.PATSectionLength == 0)
                                {
                                    parser.PATTransferState = false;
                                    if (parser.PATSectionNumber == parser.PATLastSectionNumber)
                                    {
                                        for (var k = 0; k < parser.PATOffset - 4; k += 4)
                                        {
                                            var programNumber = (uint)((parser.PAT[k] << 8) +
                                                                       parser.PAT[k + 1]);

                                            var programPID = (ushort)(((parser.PAT[k + 2] & 0x1F) << 8) +
                                                                      parser.PAT[k + 3]);

                                            if (programNumber == 1)
                                            {
                                                parser.PMTPID = programPID;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                --parser.PacketLength;
                                if (parser.PATSectionStart)
                                {
                                    parser.PATPointerField = buffer[i];
                                    if (parser.PATPointerField == 0)
                                    {
                                        parser.PATSectionLengthParse = 3;
                                    }
                                    parser.PATSectionStart = false;
                                }
                                else if (parser.PATPointerField > 0)
                                {
                                    --parser.PATPointerField;
                                    if (parser.PATPointerField == 0)
                                    {
                                        parser.PATSectionLengthParse = 3;
                                    }
                                }
                                else if (parser.PATSectionLengthParse > 0)
                                {
                                    --parser.PATSectionLengthParse;
                                    switch (parser.PATSectionLengthParse)
                                    {
                                        case 2:
                                            break;
                                        case 1:
                                            parser.PATSectionLength = (ushort)
                                                ((buffer[i] & 0xF) << 8);
                                            break;
                                        case 0:
                                            parser.PATSectionLength |= buffer[i];
                                            if (parser.PATSectionLength > 1021)
                                            {
                                                parser.PATSectionLength = 0;
                                            }
                                            else
                                            {
                                                parser.PATSectionParse = 5;
                                            }
                                            break;
                                    }
                                }
                                else if (parser.PATSectionParse > 0)
                                {
                                    --parser.PATSectionLength;
                                    --parser.PATSectionParse;

                                    switch (parser.PATSectionParse)
                                    {
                                        case 4:
                                            parser.TransportStreamId = (ushort)
                                                (buffer[i] << 8);
                                            break;
                                        case 3:
                                            parser.TransportStreamId |= buffer[i];
                                            break;
                                        case 2:
                                            break;
                                        case 1:
                                            parser.PATSectionNumber = buffer[i];
                                            if (parser.PATSectionNumber == 0)
                                            {
                                                parser.PATOffset = 0;
                                            }
                                            break;
                                        case 0:
                                            parser.PATLastSectionNumber = buffer[i];
                                            parser.PATTransferState = true;
                                            break;
                                    }
                                }
                            }
                            if (parser.PacketLength <= 0)
                            {
                                parser.SyncState = false;
                            }
                        }
                        else if (parser.PID == parser.PMTPID)
                        {
                            if (parser.PMTTransferState)
                            {
                                if (bufferLength - i >= parser.PMTSectionLength)
                                {
                                    offset = parser.PMTSectionLength;
                                }
                                else
                                {
                                    offset = bufferLength - i;
                                }
                                if (parser.PacketLength <= offset)
                                {
                                    offset = parser.PacketLength;
                                }
                                if (!parser.PMT.ContainsKey(parser.PID))
                                {
                                    parser.PMT[parser.PID] = new byte[1024];
                                }

                                var pmt = parser.PMT[parser.PID];
                                for (var k = 0; k < offset; k++)
                                {
                                    pmt[parser.PMTOffset++] = buffer[i++];
                                    --parser.PMTSectionLength;
                                    --parser.PacketLength;
                                }
                                --i;

                                if (parser.PMTSectionLength == 0)
                                {
                                    parser.PMTTransferState = false;
                                    if (parser.PMTSectionNumber == parser.PMTLastSectionNumber)
                                    {
                                        //Console.WriteLine("PMT Start: " + parser.PMTTemp);
                                        try
                                        {
                                            for (var k = 0; k < parser.PMTOffset - 4; k += 5)
                                            {
                                                var streamType = pmt[k];

                                                var streamPID = (ushort)
                                                    (((pmt[k + 1] & 0x1F) << 8) +
                                                     pmt[k + 2]);

                                                var streamInfoLength = (ushort)
                                                    (((pmt[k + 3] & 0xF) << 8) +
                                                     pmt[k + 4]);

                                                /*
                                            if (streamInfoLength == 2)
                                            {
                                                // TODO: Cleanup
                                                //streamInfoLength = 0;
                                            }

                                            Console.WriteLine(string.Format(
                                                "Type: {0} PID: {1} Length: {2}",
                                                streamType, streamPID, streamInfoLength));
                                             */

                                                if (!Streams.ContainsKey(streamPID))
                                                {
                                                    var streamDescriptors = new List<TSDescriptor>();

                                                    /*
                                                 * TODO: Getting bad streamInfoLength
                                                if (streamInfoLength > 0)
                                                {
                                                    for (int d = 0; d < streamInfoLength; d++)
                                                    {
                                                        byte name = PMT[k + d + 5];
                                                        byte length = PMT[k + d + 6];
                                                        TSDescriptor descriptor =
                                                            new TSDescriptor(name, length);
                                                        for (int v = 0; v < length; v++)
                                                        {
                                                            descriptor.Value[v] =
                                                                PMT[k + d + v + 7];
                                                        }
                                                        streamDescriptors.Add(descriptor);
                                                        d += (length + 1);
                                                    }
                                                }
                                                */
                                                    CreateStream(streamPID, streamType, streamDescriptors);
                                                    if (Streams[streamPID].IsGraphicsStream)
                                                        Streams[streamPID].IsInitialized = !isFullScan;
                                                }
                                                k += streamInfoLength;
                                            }
                                        }
                                        catch (ThreadInterruptedException)
                                        {
                                            return;
                                        }
                                        catch (Exception ex)
                                        {
                                            // TODO
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                --parser.PacketLength;
                                if (parser.PMTSectionStart)
                                {
                                    parser.PMTPointerField = buffer[i];
                                    if (parser.PMTPointerField == 0)
                                    {
                                        parser.PMTSectionLengthParse = 3;
                                    }
                                    parser.PMTSectionStart = false;
                                }
                                else if (parser.PMTPointerField > 0)
                                {
                                    --parser.PMTPointerField;
                                    if (parser.PMTPointerField == 0)
                                    {
                                        parser.PMTSectionLengthParse = 3;
                                    }
                                }
                                else if (parser.PMTSectionLengthParse > 0)
                                {
                                    --parser.PMTSectionLengthParse;
                                    switch (parser.PMTSectionLengthParse)
                                    {
                                        case 2:
                                            if (buffer[i] != 0x2)
                                            {
                                                parser.PMTSectionLengthParse = 0;
                                            }
                                            break;
                                        case 1:
                                            parser.PMTSectionLength = (ushort)
                                                ((buffer[i] & 0xF) << 8);
                                            break;
                                        case 0:
                                            parser.PMTSectionLength |= buffer[i];
                                            if (parser.PMTSectionLength > 1021)
                                            {
                                                parser.PMTSectionLength = 0;
                                            }
                                            else
                                            {
                                                parser.PMTSectionParse = 9;
                                            }
                                            break;
                                    }
                                }
                                else if (parser.PMTSectionParse > 0)
                                {
                                    --parser.PMTSectionLength;
                                    --parser.PMTSectionParse;

                                    switch (parser.PMTSectionParse)
                                    {
                                        case 8:
                                        case 7:
                                            break;
                                        case 6:
                                            parser.PMTTemp = buffer[i];
                                            break;
                                        case 5:
                                            parser.PMTSectionNumber = buffer[i];
                                            if (parser.PMTSectionNumber == 0)
                                            {
                                                parser.PMTOffset = 0;
                                            }
                                            break;
                                        case 4:
                                            parser.PMTLastSectionNumber = buffer[i];
                                            break;
                                        case 3:
                                            parser.PCRPID = (ushort)
                                                ((buffer[i] & 0x1F) << 8);
                                            break;
                                        case 2:
                                            parser.PCRPID |= buffer[i];
                                            break;
                                        case 1:
                                            parser.PMTProgramInfoLength = (ushort)
                                                ((buffer[i] & 0xF) << 8);
                                            break;
                                        case 0:
                                            parser.PMTProgramInfoLength |= buffer[i];
                                            if (parser.PMTProgramInfoLength == 0)
                                            {
                                                parser.PMTTransferState = true;
                                            }
                                            else
                                            {
                                                parser.PMTProgramDescriptorLengthParse = 2;
                                            }
                                            break;
                                    }
                                }
                                else if (parser.PMTProgramInfoLength > 0)
                                {
                                    --parser.PMTSectionLength;
                                    --parser.PMTProgramInfoLength;

                                    if (parser.PMTProgramDescriptorLengthParse > 0)
                                    {
                                        --parser.PMTProgramDescriptorLengthParse;
                                        switch (parser.PMTProgramDescriptorLengthParse)
                                        {
                                            case 1:
                                                parser.PMTProgramDescriptor = buffer[i];
                                                break;
                                            case 0:
                                                parser.PMTProgramDescriptorLength = buffer[i];
                                                parser.PMTProgramDescriptors.Add(
                                                    new TSDescriptor(
                                                        parser.PMTProgramDescriptor,
                                                        parser.PMTProgramDescriptorLength));
                                                break;
                                        }
                                    }
                                    else if (parser.PMTProgramDescriptorLength > 0)
                                    {
                                        --parser.PMTProgramDescriptorLength;

                                        var descriptor = parser.PMTProgramDescriptors[^1];

                                        var valueIndex =
                                            descriptor.Value.Length -
                                            parser.PMTProgramDescriptorLength - 1;

                                        descriptor.Value[valueIndex] = buffer[i];

                                        if (parser is { PMTProgramDescriptorLength: 0, PMTProgramInfoLength: > 0 })
                                        {
                                            parser.PMTProgramDescriptorLengthParse = 2;
                                        }
                                    }
                                    if (parser.PMTProgramInfoLength == 0)
                                    {
                                        parser.PMTTransferState = true;
                                    }
                                }
                            }
                            if (parser.PacketLength <= 0)
                            {
                                parser.SyncState = false;
                            }
                        }
                        else if (parser is { Stream: { }, StreamState: { }, TransportScramblingControl: 0 })
                        {
                            var stream = parser.Stream;
                            var streamState = parser.StreamState;

                            streamState.Parse =
                                (streamState.Parse << 8) + buffer[i];

                            if (streamState.TransferState)
                            {
                                if (bufferLength - i >= streamState.PacketLength &&
                                    streamState is { PacketLength: > 0, PacketLengthVariable: false })
                                {
                                    offset = streamState.PacketLength;
                                }
                                else if (bufferLength - i >= parser.PacketLength &&
                                         parser.PacketLength > 0 && streamState.PacketLengthVariable)
                                {
                                    offset = parser.PacketLength;
                                }
                                else
                                {
                                    offset = bufferLength - i;
                                }
                                if (parser.PacketLength <= offset)
                                {
                                    offset = parser.PacketLength;
                                }
                                streamState.TransferLength = offset;

                                if (!stream.IsInitialized ||
                                    stream.IsVideoStream ||
                                    stream.IsGraphicsStream)
                                {
                                    streamState.StreamBuffer.Add(
                                        buffer, i, offset);
                                }
                                else
                                {
                                    streamState.StreamBuffer.TransferLength += offset;
                                }

                                i += streamState.TransferLength - 1;
                                streamState.PacketLength -= streamState.TransferLength;
                                parser.PacketLength -= (byte)streamState.TransferLength;

                                streamState.TotalBytes += (ulong)streamState.TransferLength;
                                streamState.WindowBytes += (ulong)streamState.TransferLength;

                                if (parser.VariablePacketEnd && streamState.PacketLengthVariable)
                                {
                                    parser.VariablePacketEnd = false;
                                    streamState.PacketLengthVariable = false;
                                }

                                if (streamState.PacketLength == 0)
                                {
                                    streamState.TransferState = false;
                                    streamState.TransferCount++;
                                    var isFinished = ScanStream(stream,
                                        streamState,
                                        streamState.StreamBuffer,
                                        isFullScan);

                                    if (!isFullScan && isFinished)
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                --parser.PacketLength;

                                var headerFound = stream.IsVideoStream &&
                                                  streamState.Parse == 0x000001FD;

                                if (stream.IsVideoStream &&
                                    streamState.Parse is >= 0x000001E0 and <= 0x000001EF)
                                {
                                    headerFound = true;
                                }

                                if (stream.IsAudioStream &&
                                    streamState.Parse == 0x000001BD)
                                {
                                    headerFound = true;
                                }
                                if (stream.IsAudioStream &&
                                    streamState.Parse is >= 0x000001C0 and <= 0x000001DF)
                                {
                                    headerFound = true;
                                }
                                if (stream.IsAudioStream &&
                                    streamState.Parse is 0x000001FA or 0x000001FD)
                                {
                                    headerFound = true;
                                }

                                if (stream is { IsVideoStream: false, IsAudioStream: false } &&
                                    streamState.Parse is 0x000001FA or 0x000001FD or 0x000001BD
                                        or >= 0x000001E0 and <= 0x000001EF)
                                {
                                    headerFound = true;
                                }

                                if (headerFound)
                                {
                                    streamState.PacketLengthParse = 2;
#if DEBUG && !BETA
                                    streamState.PESHeaderIndex = 0;
                                    streamState.PESHeader[streamState.PESHeaderIndex++] =
                                        (byte)((streamState.Parse >> 24) & 0xFF);
                                    streamState.PESHeader[streamState.PESHeaderIndex++] =
                                        (byte)((streamState.Parse >> 16) & 0xFF);
                                    streamState.PESHeader[streamState.PESHeaderIndex++] =
                                        (byte)((streamState.Parse >> 8) & 0xFF);
                                    streamState.PESHeader[streamState.PESHeaderIndex++] =
                                        (byte)(streamState.Parse & 0xFF);
#endif
                                }
                                else if (streamState.PacketLengthParse > 0)
                                {
                                    --streamState.PacketLengthParse;
                                    switch (streamState.PacketLengthParse)
                                    {
                                        case 1:
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] =
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 0:
                                            streamState.PacketLength =
                                                (int)(streamState.Parse & 0xFFFF);
                                            if (streamState.PacketLength == 0)
                                            {
                                                parser.VariablePacketEnd = false;
                                                streamState.PacketLengthVariable = true;
                                            }


                                            streamState.PacketParse = 3;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] =
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;
                                    }
                                }
                                else if (streamState.PacketParse > 0)
                                {
                                    --streamState.PacketLength;
                                    --streamState.PacketParse;

                                    switch (streamState.PacketParse)
                                    {
                                        case 2:
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] =
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 1:
                                            streamState.PESHeaderFlags =
                                                (byte)(streamState.Parse & 0xFF);
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] =
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 0:
                                            streamState.PESHeaderLength =
                                                (byte)(streamState.Parse & 0xFF);
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] =
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            if ((streamState.PESHeaderFlags & 0xC0) == 0x80)
                                            {
                                                streamState.PTSParse = 5;
                                            }
                                            else if ((streamState.PESHeaderFlags & 0xC0) == 0xC0)
                                            {
                                                streamState.DTSParse = 10;
                                            }
                                            if (streamState.PESHeaderLength == 0)
                                            {
                                                streamState.TransferState = true;
                                            }
                                            break;
                                    }
                                }
                                else if (streamState.PTSParse > 0)
                                {
                                    --streamState.PacketLength;
                                    --streamState.PESHeaderLength;
                                    --streamState.PTSParse;

                                    switch (streamState.PTSParse)
                                    {
                                        case 4:
                                            streamState.PTSTemp =
                                                (streamState.Parse & 0xE) << 29;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            break;

                                        case 3:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFF) << 22;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 2:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFE) << 14;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 1:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFF) << 7;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 0:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFE) >> 1;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            streamState.PTS = streamState.PTSTemp;

                                            if (streamState.PTS != streamState.PTSLast)
                                            {
                                                if (streamState.PTSLast > 0)
                                                {
                                                    streamState.PTSTransfer = streamState.PTS - streamState.PTSLast;
                                                }
                                                streamState.PTSLast = streamState.PTS;
                                            }

                                            // TODO: Frame reorder for streams encoded with b-pyramid > 0
                                            streamState.PTSDiff = streamState.PTS - streamState.DTSPrev;

                                            if (streamState.PTSCount > 0 &&
                                                stream.IsVideoStream)
                                            {
                                                UpdateStreamBitrates(stream.PID, streamState.PTS, streamState.PTSDiff);
                                                if (streamState.DTSTemp < parser.PTSFirst)
                                                {
                                                    parser.PTSFirst = streamState.DTSTemp;
                                                }
                                                if (streamState.DTSTemp > parser.PTSLast)
                                                {
                                                    parser.PTSLast = streamState.DTSTemp;
                                                }
                                                Length = (double)(parser.PTSLast - parser.PTSFirst) / 90000;
                                            }

                                            streamState.DTSPrev = streamState.PTS;
                                            streamState.PTSCount++;
                                            if (streamState.PESHeaderLength == 0)
                                            {
                                                streamState.TransferState = true;
                                            }
                                            break;
                                    }
                                }
                                else if (streamState.DTSParse > 0)
                                {
                                    --streamState.PacketLength;
                                    --streamState.PESHeaderLength;
                                    --streamState.DTSParse;

                                    switch (streamState.DTSParse)
                                    {
                                        case 9:
                                            streamState.PTSTemp =
                                                (streamState.Parse & 0xE) << 29;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 8:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFF) << 22;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 7:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFE) << 14;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            break;

                                        case 6:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFF) << 7;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 5:
                                            streamState.PTSTemp |=
                                                (streamState.Parse & 0xFE) >> 1;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            streamState.PTS = streamState.PTSTemp;
                                            if (streamState.PTS > streamState.PTSLast)
                                            {
                                                streamState.PTSLast = streamState.PTS;
                                            }
                                            break;

                                        case 4:
                                            streamState.DTSTemp =
                                                (streamState.Parse & 0xE) << 29;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            break;

                                        case 3:
                                            streamState.DTSTemp |=
                                                (streamState.Parse & 0xFF) << 22;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            break;

                                        case 2:
                                            streamState.DTSTemp |=
                                                (streamState.Parse & 0xFE) << 14;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif
                                            break;

                                        case 1:
                                            streamState.DTSTemp |=
                                                (streamState.Parse & 0xFF) << 7;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xFF);
#endif
                                            break;

                                        case 0:
                                            streamState.DTSTemp |=
                                                (streamState.Parse & 0xFE) >> 1;
#if DEBUG && !BETA
                                            streamState.PESHeader[streamState.PESHeaderIndex++] = 
                                                (byte)(streamState.Parse & 0xff);
#endif

                                            // TODO: Frame reorder for streams encoded with b-pyramid > 0
                                            streamState.PTSDiff = streamState.DTSTemp - streamState.DTSPrev;

                                            if (streamState.PTSCount > 0 &&
                                                stream.IsVideoStream)
                                            {
                                                UpdateStreamBitrates(stream.PID, streamState.DTSTemp, streamState.PTSDiff);
                                                if (streamState.DTSTemp < parser.PTSFirst)
                                                {
                                                    parser.PTSFirst = streamState.DTSTemp;
                                                }
                                                if (streamState.DTSTemp > parser.PTSLast)
                                                {
                                                    parser.PTSLast = streamState.DTSTemp;
                                                }
                                                Length = (double)(parser.PTSLast - parser.PTSFirst) / 90000;
                                            }
                                            streamState.DTSPrev = streamState.DTSTemp;
                                            streamState.PTSCount++;
                                            if (streamState.PESHeaderLength == 0)
                                            {
                                                streamState.TransferState = true;
                                            }
                                            break;
                                    }
                                }
                                else if (streamState.PESHeaderLength > 0)
                                {
                                    --streamState.PacketLength;
                                    --streamState.PESHeaderLength;
#if DEBUG && !BETA
                                    streamState.PESHeader[streamState.PESHeaderIndex++] =
                                        (byte)(streamState.Parse & 0xFF);
#endif
                                    if (streamState.PESHeaderLength == 0)
                                    {
                                        streamState.TransferState = true;
                                    }
                                }
                            }
                            if (parser.PacketLength <= 0)
                            {
                                parser.SyncState = false;
                            }
                        }
                        else
                        {
                            parser.PacketLength--;
                            if (bufferLength - i >= parser.PacketLength)
                            {
                                i += parser.PacketLength;
                                parser.PacketLength = 0;
                            }
                            else
                            {
                                parser.PacketLength -= (byte)(bufferLength - i + 1);
                                i = bufferLength;
                            }
                            if (parser.PacketLength <= 0)
                            {
                                parser.SyncState = false;
                            }
                        }
                    }
                }
                Size += bufferLength;
            }

            // TODO: Frame reorder for streams encoded with b-pyramid > 0
            BigInteger ptsLast = 0;
            BigInteger ptsDiff = 0;
            foreach (var stream in Streams.Values.Where(stream => stream.IsVideoStream))
            {
                if (_streamStates.ContainsKey(stream.PID) &&
                    _streamStates[stream.PID].PTSLast > ptsLast)
                {
                    ptsLast = _streamStates[stream.PID].PTSLast;
                    ptsDiff = ptsLast - _streamStates[stream.PID].DTSPrev;
                }
                UpdateStreamBitrates(stream.PID, ptsLast, ptsDiff);
            }
        }
        catch (ThreadInterruptedException)
        {
            Debug.WriteLine("Thread Interrupted");

            fileStream?.Close();
        }
        finally
        {
            fileStream?.Close();
        }
    }

    private void CreateStream(ushort streamPID, byte streamType, List<TSDescriptor> streamDescriptors)
    {
        TSStream stream = null;

        switch ((TSStreamType)streamType)
        {
            case TSStreamType.MVC_VIDEO:
            case TSStreamType.AVC_VIDEO:
            case TSStreamType.HEVC_VIDEO:
            case TSStreamType.MPEG1_VIDEO:
            case TSStreamType.MPEG2_VIDEO:
            case TSStreamType.VC1_VIDEO:
            {
                stream = new TSVideoStream();
            }
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
            {
                stream = new TSAudioStream();
            }
                break;

            case TSStreamType.INTERACTIVE_GRAPHICS:
            case TSStreamType.PRESENTATION_GRAPHICS:
            {
                stream = new TSGraphicsStream();
            }
                break;

            case TSStreamType.SUBTITLE:
            {
                stream = new TSTextStream();
            }
                break;
        }

        if (stream != null && !Streams.ContainsKey(streamPID))
        {
            stream.PID = streamPID;
            stream.StreamType = (TSStreamType)streamType;
            stream.Descriptors = streamDescriptors;
            Streams[stream.PID] = stream;
        }
        if (!StreamDiagnostics.ContainsKey(streamPID))
        {
            StreamDiagnostics[streamPID] = new List<TSStreamDiagnostics>();
        }
    }
}