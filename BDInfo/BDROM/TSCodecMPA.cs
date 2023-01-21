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

namespace BDInfo
{
    public abstract class TSCodecMPA
    {
        private static readonly int[][][] MPABitrate =
        {
            /*
             * Audio version ID (see table 3.2 also)
            00 - MPEG Version 2.5 (unofficial extension of MPEG 2)
            01 - reserved
            10 - MPEG Version 2 (ISO/IEC 13818-3)
            11 - MPEG Version 1 (ISO/IEC 11172-3)
            -------------
            Layer index
            00 - reserved
            01 - Layer III
            10 - Layer II
            11 - Layer I
            -----
             * // Bitrate Index    MPEG 1                  MPEG 2, 2.5 (LSF)
                            Layer I Layer II    Layer III   Layer I     Layer II & III
                    0000	free
                    0001	32	    32	        32	        32	        8
                    0010	64	    48	        40	        48	        16
                    0011	96	    56	        48	        56	        24
                    0100	128	    64	        56	        64	        32
                    0101	160	    80	        64	        80	        40
                    0110	192	    96	        80	        96	        48
                    0111	224	    112	        96	        112	        56
                    1000	256	    128	        112	        128	        64
                    1001	288	    160	        128	        144	        80
                    1010	320	    192	        160	        160	        96
                    1011	352	    224	        192	        176	        112
                    1100	384	    256	        224	        192	        128
                    1101	416	    320	        256	        224	        144
                    1110	448	    384	        320	        256	        160
                    1111	reserved
            */
            new [] // MPEG Version 2.5
            {
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // MPEG Version 2.5 Layer 0
                new int[] {0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160,   0}, // MPEG Version 2.5 Layer III
                new int[] {0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160,   0}, // MPEG Version 2.5 Layer II
                new int[] {0,  32,  48,  56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256,   0}, // MPEG Version 2.5 Layer I
            },
            new [] // reserved
            {
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // reserved
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // reserved
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // reserved
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // reserved
            }, 
            new [] // MPEG Version 2
            {
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // MPEG Version 2 Layer 0
                new int[] {0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160,   0}, // MPEG Version 2 Layer III
                new int[] {0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160,   0}, // MPEG Version 2 Layer II
                new int[] {0,  32,  48,  56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256,   0}, // MPEG Version 2 Layer I
            },
            new [] // MPEG Version 1
            {
                new int[] {0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0}, // MPEG Version 1 Layer 0
                new int[] {0,  32,  40,  48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320,   0}, // MPEG Version 1 Layer III
                new int[] {0,  32,  48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384,   0}, // MPEG Version 1 Layer II
                new int[] {0,  32,  64,  96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448,   0}, // MPEG Version 1 Layer I
            }
        };

        private static readonly int[][] MPASampleRate =
        {
            new int[] {11025, 12000,  8000,     0}, // MPEG Version 2.5
            new int[] {    0,     0,     0,     0}, // reserved
            new int[] {22050, 24000, 16000,     0}, // MPEG Version 2
            new int[] {44100, 48000, 32000,     0}  // MPEG Version 1
        };

        private static readonly byte[] MPAChannelModes =
        {
            (byte)TSAudioMode.Stereo,
            (byte)TSAudioMode.JointStereo,
            (byte)TSAudioMode.DualMono,
            (byte)TSAudioMode.Mono
        };

        private static readonly string[] MPAVersion =
        {
            "MPEG 2.5",
            "Unknown MPEG",
            "MPEG 2",
            "MPEG 1"
        };

        private static readonly string[] MPALayer =
        {
            "Unknown Layer",
            "Layer III",
            "Layer II",
            "Layer I"
        };

        private static readonly byte[] MPAChannels = {2, 2, 2, 1};

        public static void Scan(TSAudioStream stream, TSStreamBuffer buffer, ref string tag)
        {
            if (stream.IsInitialized) return;

            int syncWord = buffer.ReadBits2(11) << 5;
            if (syncWord != 0b1111_1111_1110_0000) return;

            int audioVersionID = buffer.ReadBits2(2);
            int layerIndex = buffer.ReadBits2(2);
            bool protectionBit = buffer.ReadBool();
            int bitrateIndex = buffer.ReadBits2(4);
            int samplingRateIndex = buffer.ReadBits2(2);
            bool padding = buffer.ReadBool();
            bool privateBit = buffer.ReadBool();
            int channelMode = buffer.ReadBits2(2);
            int modeExtension = buffer.ReadBits2(2);
            bool copyrightBit = buffer.ReadBool();
            bool originalBit = buffer.ReadBool();
            int emphasis = buffer.ReadBits2(2);

            stream.BitRate = MPABitrate[audioVersionID][layerIndex][bitrateIndex] * 1000;
            stream.SampleRate = MPASampleRate[audioVersionID][samplingRateIndex];
            stream.AudioMode = (TSAudioMode)MPAChannelModes[channelMode];

            stream.ChannelCount = MPAChannels[channelMode];
            stream.LFE = 0;
            
            stream.ExtendedData = $"{MPAVersion[audioVersionID]} {MPALayer[layerIndex]}";

            stream.IsVBR = false;
            stream.IsInitialized = true;
        }
    }
}
