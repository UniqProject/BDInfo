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

namespace BDInfo
{
    public abstract class TSCodecTrueHD
    {
        public static void Scan(TSAudioStream stream, TSStreamBuffer buffer, ref string tag)
        {
            if (stream.IsInitialized &&
                stream.CoreStream != null &&
                stream.CoreStream.IsInitialized) return;

            var syncFound = false;
            uint sync = 0;
            for (var i = 0; i < buffer.Length; i++)
            {
                sync = (sync << 8) + buffer.ReadByte();
                if (sync == 0xF8726FBA) 
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
                    stream.CoreStream = new TSAudioStream {StreamType = TSStreamType.AC3_AUDIO};
                }
                if (!stream.CoreStream.IsInitialized)
                {
                    buffer.BeginRead();
                    TSCodecAC3.Scan(stream.CoreStream, buffer, ref tag);
                }
                return;
            }

            tag = "HD";
            int ratebits = buffer.ReadBits2(4);
            if (ratebits != 0xF)
            {
                stream.SampleRate = 
                    (((ratebits & 8) > 0 ? 44100 : 48000) << (ratebits & 7));
            }
            buffer.BSSkipBits(15);

            stream.ChannelCount = 0;
            stream.LFE = 0;
            if (buffer.ReadBool())
            {
                stream.LFE += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }
            if (buffer.ReadBool())
            {
                stream.LFE += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 1;
            }
            if (buffer.ReadBool())
            {
                stream.ChannelCount += 2;
            }

            buffer.BSSkipBits(49);

            var peakBitrate = buffer.ReadBits4(15);
            peakBitrate = (uint) ((peakBitrate * stream.SampleRate) >> 4);

            var peakBitdepth =  (double)peakBitrate / (stream.ChannelCount + stream.LFE) / stream.SampleRate;

            stream.BitDepth = peakBitdepth > 14 ? 24 : 16;

            buffer.BSSkipBits(79);

            var hasExtensions = buffer.ReadBool();
            int numExtensions = (buffer.ReadBits2(4)*2) + 1;
            var hasContent = Convert.ToBoolean(buffer.ReadBits4(4));

            if (hasExtensions)
            {
                for (var idx = 0; idx < numExtensions; ++idx)
                {
                    if (Convert.ToBoolean(buffer.ReadBits2(8)))
                        hasContent = true;
                }

                if (hasContent)
                    stream.HasExtensions = true;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{stream.PID}\t{peakBitrate}\t{peakBitdepth:F2}");
#endif
            /*
            // TODO: Get THD dialnorm from metadata
            if (stream.CoreStream != null)
            {
                TSAudioStream coreStream = (TSAudioStream)stream.CoreStream;
                if (coreStream.DialNorm != 0)
                {
                    stream.DialNorm = coreStream.DialNorm;
                }
            }
            */

            stream.IsVBR = true;
            if (stream.CoreStream != null && stream.CoreStream.IsInitialized)
                stream.IsInitialized = true;
        }
    }
}
