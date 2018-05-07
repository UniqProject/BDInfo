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

#undef DEBUG
using System.IO;

namespace BDInfo
{
    public abstract class TSCodecAC3
    {
        private static readonly int[] AC3Bitrate = 
        {
             32,
             40,
             48,
             56,
             64,
             80,
             96,
            112,
            128,
            160,
            192,
            224,
            256,
            320,
            384,
            448,
            512,
            576,
            640,
        };

        private static readonly byte[] AC3Channels = {2, 1, 2, 3, 3, 4, 4, 5};

        public static byte AC3ChanMap(int chanMap)
        {
            byte channels = 0;

            for (byte i = 0; i < 16; i++)
            {
                if ((chanMap & (1<<(15-i))) != 0)
                    switch (i)
                    {
                        case 5:
                        case 6:
                        case 9:
                        case 10:
                        case 11:
                            channels += 2; break;
                    }
            }
            return channels;
        }

        public static void Scan(TSAudioStream stream, TSStreamBuffer buffer, ref string tag)
        {
            if (stream.IsInitialized) return;

            byte[] sync = buffer.ReadBytes(2);
            if (sync == null || sync[0] != 0x0B || sync[1] != 0x77)
            {
                return;
            }

            var secondFrame = stream.ChannelCount > 0;

            uint srCode;
            uint frameSize = 0;
            uint frameSizeCode = 0;
            uint channelMode;
            uint lfeOn;
            uint dialNorm;
            uint numBlocks = 0;

            byte[] hdr = buffer.ReadBytes(4);
            uint bsid = (uint)((hdr[3] & 0xF8) >> 3);
            buffer.Seek(-4, SeekOrigin.Current);
            if (bsid <= 10)
            {
                buffer.BSSkipBytes(2);
                srCode = buffer.ReadBits2(2);
                frameSizeCode = buffer.ReadBits2(6);
                bsid = buffer.ReadBits2(5);
                buffer.BSSkipBits(3);

                channelMode = buffer.ReadBits2(3);
                if (((channelMode & 0x1) > 0) && (channelMode != 0x1))
                {
                    buffer.BSSkipBits(2);
                }
                if ((channelMode & 0x4) > 0)
                {
                    buffer.BSSkipBits(2);
                }
                if (channelMode == 0x2)
                {
                    var dsurmod = buffer.ReadBits2(2);
                    if (dsurmod == 0x2)
                    {
                        stream.AudioMode = TSAudioMode.Surround;
                    }
                }
                lfeOn = buffer.ReadBits2(1);
                dialNorm = buffer.ReadBits2(5);
                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(8);
                }
                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(8);
                }
                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(7);
                }
                if (channelMode == 0)
                {
                    buffer.BSSkipBits(5);
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(8);
                    }
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(8);
                    }
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(7);
                    }
                }
                buffer.BSSkipBits(2);
                if (bsid == 6)
                {
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(14);
                    }
                    if (buffer.ReadBool())
                    {
                        uint dsurexmod = buffer.ReadBits2(2);
                        uint dheadphonmod = buffer.ReadBits2(2);
                        if (dheadphonmod == 0x2)
                        {
                            // TODO
                        }
                        buffer.BSSkipBits(10);
                        if (dsurexmod == 2)
                        {
                            stream.AudioMode = TSAudioMode.Extended;
                        }
                    }
                }
            }
            else
            {
                uint frameType = buffer.ReadBits2(2);
                buffer.BSSkipBits(3);

                frameSize = (buffer.ReadBits4(11) + 1) << 1;

                srCode = buffer.ReadBits2(2);
                if (srCode == 3)
                {
                    srCode = buffer.ReadBits2(2);
                    numBlocks = 3;
                }
                else
                {
                    numBlocks = buffer.ReadBits2(2);
                }
                channelMode = buffer.ReadBits2(3);
                lfeOn = buffer.ReadBits2(1);
                bsid = buffer.ReadBits2(5);
                dialNorm = buffer.ReadBits2(5);

                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(8);
                }
                if (channelMode == 0) // 1+1
                {
                    buffer.BSSkipBits(5);
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(8);
                    }
                }
                if (frameType == 1) //dependent stream
                {
                    stream.CoreStream = (TSAudioStream)stream.Clone();

                    if (buffer.ReadBool()) //channel remapping
                    {
                        uint chanmap = buffer.ReadBits4(16);
                        
                        stream.ChannelCount = stream.CoreStream.ChannelCount;
                        stream.ChannelCount += AC3ChanMap((int) chanmap);
                        lfeOn = (uint) stream.CoreStream.LFE;
                    }
                }

                bool emdfFound = false;

                do
                {
                    uint emdfSync = (buffer.ReadBits4(16));
                    if ((emdfSync) == 0x5838)
                    {
                        emdfFound = true;
                        break;
                    }
                    buffer.Seek(-2, SeekOrigin.Current);
                    buffer.BSSkipBits(1); // skip 1 bit
                } while (buffer.Position < buffer.Length);

                if (emdfFound)
                {
                    uint emdfContainerSize = buffer.ReadBits4(16);
                    var remainAfterEmdf = buffer.DataBitStreamRemain() - emdfContainerSize*8;

                    uint emdfVersion = buffer.ReadBits2(2); //emdf_version
                    if (emdfVersion == 3)
                        emdfVersion += buffer.ReadBits2(2);

                    if (emdfVersion > 0)
                    {
                        buffer.BSSkipBits((int) (buffer.DataBitStreamRemain() - remainAfterEmdf));
                    }
                    else
                    {
                        var temp = buffer.ReadBits2(3);
                        if (temp == 0x7)
                            buffer.BSSkipBits(2); //skip 3 bits

                        var emdfPayloadID = buffer.ReadBits2(5);
                        
                        if (emdfPayloadID > 0 && emdfPayloadID < 16)
                        {
                            if (emdfPayloadID == 0x1F)
                                buffer.BSSkipBits(5); //skip 5 bits

                            EmdfPayloadConfig(buffer);

                            int emdfPayloadSize = buffer.ReadBits2(8)*8;
                            buffer.BSSkipBits(emdfPayloadSize + 1);
                        }

                        while ((emdfPayloadID = buffer.ReadBits2(5)) != 14 && buffer.Position < buffer.Length)
                        {
                            if (emdfPayloadID == 0x1F)
                                buffer.BSSkipBits(5); //skip 5 bits

                            EmdfPayloadConfig(buffer);

                            int emdfPayloadSize = buffer.ReadBits2(8) * 8;
                            buffer.ReadBits4(emdfPayloadSize + 1);
                        }

                        if (buffer.Position < buffer.Length && emdfPayloadID == 14)
                        {
                            EmdfPayloadConfig(buffer);

                            buffer.BSSkipBits(12);

                            uint jocNumObjectsBits = buffer.ReadBits2(6);

                            if (jocNumObjectsBits > 0)
                                stream.HasExtensions = true;
                        }
                    }
                }
            }

            if ((channelMode < 8) && stream.ChannelCount == 0)
                stream.ChannelCount = AC3Channels[channelMode];

            if (stream.AudioMode == TSAudioMode.Unknown)
            {
                switch (channelMode)
                {
                    case 0: // 1+1
                        stream.AudioMode = TSAudioMode.DualMono;
                        break;
                    case 2: // 2/0
                        stream.AudioMode = TSAudioMode.Stereo;
                        break;
                    default:
                        stream.AudioMode = TSAudioMode.Unknown;
                        break;
                }
            }

            switch (srCode)
            {
                case 0:
                    stream.SampleRate = 48000;
                    break;
                case 1:
                    stream.SampleRate = 44100;
                    break;
                case 2:
                    stream.SampleRate = 32000;
                    break;
                default:
                    stream.SampleRate = 0;
                    break;
            }

            if (bsid <= 10)
            {

                uint fSize = frameSizeCode >> 1;
                if (fSize < 19)
                    stream.BitRate = AC3Bitrate[fSize] * 1000;
            }
            else
            {
                stream.BitRate = (long)
                    (4.0 * frameSize * stream.SampleRate / (numBlocks * 256));
                if (stream.CoreStream != null)
                    stream.BitRate += stream.CoreStream.BitRate;
            }

            stream.LFE = (int) lfeOn;
            if (stream.StreamType != TSStreamType.AC3_PLUS_AUDIO &&
                stream.StreamType != TSStreamType.AC3_PLUS_SECONDARY_AUDIO)
            {
                stream.DialNorm = (int) (dialNorm - 31);
            }
            stream.IsVBR = false;
            if (stream.StreamType == TSStreamType.AC3_PLUS_AUDIO && bsid == 6 && !secondFrame)
                stream.IsInitialized = false;
            else
                stream.IsInitialized = true;
        }

        private static void EmdfPayloadConfig(TSStreamBuffer buffer)
        {
            bool sampleOffsetE = buffer.ReadBool();
            if (sampleOffsetE)
                buffer.BSSkipBits(12);

            if (buffer.ReadBool()) //duratione
                buffer.BSSkipBits(11); //duration

            if (buffer.ReadBool()) //groupide
                buffer.BSSkipBits(2); //groupid

            if (buffer.ReadBool())
                buffer.BSSkipBits(8); // reserved

            if (!buffer.ReadBool()) //discard_unknown_payload
            {
                buffer.BSSkipBits(1);

                if (!sampleOffsetE)
                {
                    if (buffer.ReadBool()) //payload_frame_aligned
                        buffer.BSSkipBits(9);
                }
            }
        }
    }
}
