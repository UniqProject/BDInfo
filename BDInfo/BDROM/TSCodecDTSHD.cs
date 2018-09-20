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

using System.IO;

namespace BDInfo
{
    public abstract class TSCodecDTSHD
    {
        private static readonly int[] SampleRates = { 0x1F40, 0x3E80, 0x7D00, 0x0FA00, 0x1F400, 0x5622, 0x0AC44, 0x15888, 0x2B110, 0x56220, 0x2EE0, 0x5DC0, 0x0BB80, 0x17700, 0x2EE00, 0x5DC00 };
        
        public static void Scan(TSAudioStream stream, TSStreamBuffer buffer, long bitrate, ref string tag)
        {
            if (stream.IsInitialized &&
                (stream.StreamType == TSStreamType.DTS_HD_SECONDARY_AUDIO ||
                (stream.CoreStream != null &&
                 stream.CoreStream.IsInitialized))) return;

            var syncFound = false;
            uint sync = 0;
            for (var i = 0; i < buffer.Length; i++)
            {
                sync = (sync << 8) + buffer.ReadByte();
                if (sync == 0x64582025)
                {
                    syncFound = true;
                    break;
                }
            }

            if (!syncFound)
            {
                tag = "CORE";
                if (stream.CoreStream == null)
                {
                    stream.CoreStream = new TSAudioStream {StreamType = TSStreamType.DTS_AUDIO};
                }
                if (!stream.CoreStream.IsInitialized)
                {
                    buffer.BeginRead();
                    TSCodecDTS.Scan(stream.CoreStream, buffer, bitrate, ref tag);
                }
                return;
            }

            tag = "HD";
            buffer.BSSkipBits(8);
            var nuSubStreamIndex = buffer.ReadBits4(2);
            var bBlownUpHeader = buffer.ReadBool();

            buffer.BSSkipBits(bBlownUpHeader ? 32 : 24);

            var nuNumAssets = 1;
            var bStaticFieldsPresent = buffer.ReadBool();
            if (bStaticFieldsPresent)
            {
                buffer.BSSkipBits(5);

                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(36);
                }
                var nuNumAudioPresent = buffer.ReadBits2(3) + 1;
                nuNumAssets = buffer.ReadBits2(3) + 1;
                var nuActiveExSsMask = new uint[nuNumAudioPresent];
                for (var i = 0; i < nuNumAudioPresent; i++)
                {
                    nuActiveExSsMask[i] = buffer.ReadBits4((int) (nuSubStreamIndex + 1)); //?
                }
                for (var i = 0; i < nuNumAudioPresent; i++)
                {
                    for (var j = 0; j < nuSubStreamIndex + 1; j++)
                    {
                        if (((j + 1) % 2) == 1)
                        {
                            buffer.BSSkipBits(8);
                        }
                    }
                }
                if (buffer.ReadBool())
                {
                    buffer.BSSkipBits(2);
                    var nuBits4MixOutMask = buffer.ReadBits2(2) * 4 + 4;
                    var nuNumMixOutConfigs = buffer.ReadBits2(2) + 1;
                    var nuMixOutChMask = new uint[nuNumMixOutConfigs];
                    for (var i = 0; i < nuNumMixOutConfigs; i++)
                    {
                        nuMixOutChMask[i] = buffer.ReadBits4(nuBits4MixOutMask);
                    }
                }
            }
            var assetSizes = new uint[nuNumAssets];
            for (var i = 0; i < nuNumAssets; i++)
            {
                if (bBlownUpHeader)
                {
                    assetSizes[i] = buffer.ReadBits4(20) + 1;
                }
                else
                {
                    assetSizes[i] = buffer.ReadBits4(16) + 1;
                }                
            }
            for (var i = 0; i < nuNumAssets; i++)
            {
                buffer.BSSkipBits(12);
                if (bStaticFieldsPresent)
                {
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(4);
                    }
                    if (buffer.ReadBool())
                    {
                        buffer.BSSkipBits(24);
                    }
                    if (buffer.ReadBool())
                    {
                        var nuInfoTextByteSize = buffer.ReadBits2(10) + 1;
                        var infoText = new ushort[nuInfoTextByteSize];
                        for (var j = 0; j < nuInfoTextByteSize; j++)
                        {
                            infoText[j] = buffer.ReadBits2(8);
                        }
                    }
                    var nuBitResolution = buffer.ReadBits2(5) + 1;
                    int nuMaxSampleRate = buffer.ReadBits2(4);
                    var nuTotalNumChs = buffer.ReadBits2(8) + 1;
                    uint nuSpkrActivityMask = 0;
                    if (buffer.ReadBool())
                    {
                        if (nuTotalNumChs > 2)
                        {
                            buffer.BSSkipBits(1);
                        }
                        if (nuTotalNumChs > 6)
                        {
                            buffer.BSSkipBits(1);
                        }
                        if (buffer.ReadBool())
                        {
                            int nuNumBits4SAMask = buffer.ReadBits2(2);
                            nuNumBits4SAMask = nuNumBits4SAMask * 4 + 4;
                            nuSpkrActivityMask = buffer.ReadBits4(nuNumBits4SAMask);
                        }
                        // TODO...
                    }
                    stream.SampleRate = SampleRates[nuMaxSampleRate];
                    stream.BitDepth = nuBitResolution;
                    
                    stream.LFE = 0;
                    if ((nuSpkrActivityMask & 0x8) == 0x8)
                    {
                        ++stream.LFE;
                    }
                    if ((nuSpkrActivityMask & 0x1000) == 0x1000)
                    {
                        ++stream.LFE;
                    }
                    stream.ChannelCount = nuTotalNumChs - stream.LFE;
                }
                if (nuNumAssets > 1)
                {
                    // TODO...
                    break;
                }
            }

            uint temp2 = 0;

            while (buffer.Position < buffer.Length)
            {
                temp2 = (temp2 << 8) + buffer.ReadByte();
                switch (temp2)
                {
                    case 0x41A29547: // XLL Extended data
                    case 0x655E315E: // XBR Extended data
                    case 0x0A801921: // XSA Extended data
                    case 0x1D95F262: // X96k
                    case 0x47004A03: // XXch
                    case 0x5A5A5A5A: // Xch
                        int temp3 = 0;
                        for (var i = (int)buffer.Position; i < buffer.Length; i++)
                        {
                            temp3 = (temp3 << 8) + buffer.ReadByte();

                            if (temp3 == 0x02000850) //DTS:X Pattern
                            {
                                stream.HasExtensions = true;
                                break;
                            }
                        }
                        break;
                }

                if (stream.HasExtensions) break;
            }

            // TODO
            if (stream.CoreStream != null)
            {
                var coreStream = (TSAudioStream)stream.CoreStream;
                if (coreStream.AudioMode == TSAudioMode.Extended &&
                    stream.ChannelCount == 5)
                {
                    stream.AudioMode = TSAudioMode.Extended;
                }
                /*
                if (coreStream.DialNorm != 0)
                {
                    stream.DialNorm = coreStream.DialNorm;
                }
                */
            }

            if (stream.StreamType == TSStreamType.DTS_HD_MASTER_AUDIO)
            {
                stream.IsVBR = true;
                stream.IsInitialized = true;
            }
            else if (bitrate > 0)
            {
                stream.IsVBR = false;
                stream.BitRate = bitrate;
                if (stream.CoreStream != null)
                {
                    stream.BitRate += stream.CoreStream.BitRate;
                    stream.IsInitialized = true;
                }
                stream.IsInitialized = (stream.BitRate > 0);
            }            
        }
    }
}
