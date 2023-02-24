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
using System.Text;
using BDInfoLib.BDROM.IO;

namespace BDInfoLib.BDROM;

public class TSStreamClipFile
{
    public IFileInfo FileInfo;
    public string FileType;
    public bool IsValid;
    public string Name;

    public Dictionary<ushort, TSStream> Streams = new();

    public TSStreamClipFile(IFileInfo fileInfo)
    {
        FileInfo = fileInfo;
        Name = fileInfo.Name.ToUpper();
    }

    public void Scan()
    {
        Stream fileStream = null;
        BinaryReader fileReader = null;
        ulong streamLength = 0;

        try
        {
#if DEBUG
            Debug.WriteLine($"Scanning {Name}...");
#endif
            Streams.Clear();

            if (FileInfo != null)
            {
                fileStream = FileInfo.OpenRead();
                if (fileStream != null)
                {
                    fileReader = new BinaryReader(fileStream);
                    streamLength = (ulong)fileStream.Length;
                }
            }

            var data = new byte[streamLength];
            fileReader?.Read(data, 0, data.Length);

            var fileType = new byte[8];
            Array.Copy(data, 0, fileType, 0, fileType.Length);

            FileType = Encoding.ASCII.GetString(fileType);
            if (FileType != "HDMV0100" &&
                FileType != "HDMV0200" &&
                FileType != "HDMV0300")
            {
                throw new Exception($"Clip info file {FileInfo?.Name} has an unknown file type {FileType}.");
            }
#if DEBUG                
            Debug.WriteLine($"\tFileType: {FileType}");
#endif
            var clipIndex = (data[12] << 24) +
                            (data[13] << 16) +
                            (data[14] << 8) +
                            data[15];

            var clipLength = (data[clipIndex] << 24) +
                             (data[clipIndex + 1] << 16) +
                             (data[clipIndex + 2] << 8) +
                             data[clipIndex + 3];

            var clipData = new byte[clipLength];
            Array.Copy(data, clipIndex + 4, clipData, 0, clipData.Length);

            int streamCount = clipData[8];
#if DEBUG
            Debug.WriteLine($"\tStreamCount: {streamCount}");
#endif
            var streamOffset = 10;
            for (var streamIndex = 0; streamIndex < streamCount; streamIndex++)
            {
                TSStream stream = null;

                var pid = (ushort)((clipData[streamOffset] << 8) +
                                   clipData[streamOffset + 1]);

                streamOffset += 2;

                var streamType = (TSStreamType)clipData[streamOffset + 1];
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
                    {
                        var videoFormat = (TSVideoFormat)(clipData[streamOffset + 2] >> 4);
                        var frameRate = (TSFrameRate)(clipData[streamOffset + 2] & 0xF);
                        var aspectRatio = (TSAspectRatio)(clipData[streamOffset + 3] >> 4);

                        stream = new TSVideoStream
                        {
                            VideoFormat = videoFormat,
                            AspectRatio = aspectRatio,
                            FrameRate = frameRate,
                        };
#if DEBUG
                        Debug.WriteLine($"\t{pid} {streamType} {videoFormat} {frameRate} {aspectRatio}");
#endif
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
                        var languageBytes = new byte[3];
                        Array.Copy(clipData, streamOffset + 3, languageBytes, 0, languageBytes.Length);

                        var languageCode = Encoding.ASCII.GetString(languageBytes);
                        var channelLayout = (TSChannelLayout)(clipData[streamOffset + 2] >> 4);
                        var sampleRate = (TSSampleRate)(clipData[streamOffset + 2] & 0xF);

                        stream = new TSAudioStream
                        {
                            LanguageCode = languageCode,
                            ChannelLayout = channelLayout,
                            SampleRate = TSAudioStream.ConvertSampleRate(sampleRate)
                        };
#if DEBUG
                        Debug.WriteLine($"\t{pid} {streamType} {languageCode} {channelLayout} {sampleRate}");
#endif
                    }
                        break;

                    case TSStreamType.INTERACTIVE_GRAPHICS:
                    case TSStreamType.PRESENTATION_GRAPHICS:
                    {
                        var languageBytes = new byte[3];
                        Array.Copy(clipData, streamOffset + 2, languageBytes, 0, languageBytes.Length);

                        var languageCode = Encoding.ASCII.GetString(languageBytes);

                        stream = new TSGraphicsStream
                        {
                            LanguageCode = languageCode
                        };
#if DEBUG
                        Debug.WriteLine($"\t{pid} {streamType} {languageCode}");
#endif
                    }
                        break;

                    case TSStreamType.SUBTITLE:
                    {
                        var languageBytes = new byte[3];
                        Array.Copy(clipData, streamOffset + 3, languageBytes, 0, languageBytes.Length);
                        var languageCode = Encoding.ASCII.GetString(languageBytes);
#if DEBUG
                        Debug.WriteLine($"\t{pid} {streamType} {languageCode}");
#endif
                        stream = new TSTextStream
                        {
                            LanguageCode = languageCode
                        };
                    }
                        break;
                }

                if (stream != null)
                {
                    stream.PID = pid;
                    stream.StreamType = streamType;
                    Streams.Add(pid, stream);
                }

                streamOffset += clipData[streamOffset] + 1;
            }
            IsValid = true;
        }
        finally
        {
            fileReader?.Close();
            fileStream?.Close();
        }
    }
}