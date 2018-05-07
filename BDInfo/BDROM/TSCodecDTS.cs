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

namespace BDInfo
{
    public abstract class TSCodecDTS
    {
        private static readonly int[] DcaSampleRates =
        {
            0, 8000, 16000, 32000, 0, 0, 11025, 22050, 44100, 0, 0,
            12000, 24000, 48000, 96000, 192000
        };

        private static readonly int[] DcaBitRates =
        {
            32000, 56000, 64000, 96000, 112000, 128000,
            192000, 224000, 256000, 320000, 384000,
            448000, 512000, 576000, 640000, 768000,
            896000, 1024000, 1152000, 1280000, 1344000,
            1408000, 1411200, 1472000, 1509000, 1920000,
            2048000, 3072000, 3840000, 1/*open*/, 2/*variable*/, 3/*lossless*/
        };

        private static readonly int[] DcaBitsPerSample =
        {
            16, 16, 20, 20, 0, 24, 24
        };

        public static void Scan(
            TSAudioStream stream,
            TSStreamBuffer buffer,
            long bitrate,
            ref string tag)
        {
            if (stream.IsInitialized) return;

            bool syncFound = false;
            uint sync = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sync = (sync << 8) + buffer.ReadByte();
                if (sync == 0x7FFE8001)
                {
                    syncFound = true;
                    break;
                }
            }
            if (!syncFound) return;

            buffer.BSSkipBits(6);
            uint crcPresent = buffer.ReadBits4(1);
            buffer.BSSkipBits(7);
            uint frameSize = buffer.ReadBits4(14);
            if (frameSize < 95)
            {
                return;
            }
            buffer.BSSkipBits(6);
            uint sampleRate = buffer.ReadBits4(4);
            if (sampleRate >= DcaSampleRates.Length)
            {
                return;
            }
            uint bitRate = buffer.ReadBits4(5);
            if (bitRate >= DcaBitRates.Length)
            {
                return;
            }
            buffer.BSSkipBits(8);
            uint extCoding = buffer.ReadBits4(1);
            buffer.BSSkipBits(1);
            uint lfe = buffer.ReadBits4(2);
            buffer.BSSkipBits(1);
            if (crcPresent == 1)
            {
                buffer.BSSkipBits(16);
            }
            buffer.BSSkipBits(7);
            uint sourcePcmRes = buffer.ReadBits4(3);
            buffer.BSSkipBits(2);
            uint dialogNorm = buffer.ReadBits4(4);
            if (sourcePcmRes >= DcaBitsPerSample.Length)
            {
                return;
            }
            buffer.BSSkipBits(4);
            uint totalChannels = buffer.ReadBits4(3) + 1 + extCoding;

            stream.SampleRate = DcaSampleRates[sampleRate];
            stream.ChannelCount = (int) totalChannels;
            stream.LFE = (lfe > 0 ? 1 : 0);
            stream.BitDepth = DcaBitsPerSample[sourcePcmRes];
            stream.DialNorm = (int) -dialogNorm;
            if ((sourcePcmRes & 0x1) == 0x1)
            {
                stream.AudioMode = TSAudioMode.Extended;
            }

            stream.BitRate = (uint)DcaBitRates[bitRate];
            switch (stream.BitRate)
            {
                case 1:
                    if (bitrate > 0)
                    {
                        stream.BitRate = bitrate;
                        stream.IsVBR = false;
                        stream.IsInitialized = true;
                    }
                    else
                    {
                        stream.BitRate = 0;
                    }
                    break;

                case 2:
                case 3:
                    stream.IsVBR = true;
                    stream.IsInitialized = true;
                    break;
                
                default:
                    stream.IsVBR = false;
                    stream.IsInitialized = true;
                    break;
            }
        }
    }
}
