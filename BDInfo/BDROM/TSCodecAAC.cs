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

namespace BDInfoLib.BDROM;

public abstract class TSCodecAAC
{
    private static readonly string[] AacID =
    {
        "MPEG-4",
        "MPEG-2",
    };

    private static string GetAacProfile(int profileType)
    {
        return profileType switch
        {
            0 => "AAC Main",
            1 => "AAC LC",
            2 => "AAC SSR",
            3 => "AAC LTP",
            16 => "ER AAC LC",
            18 => "ER AAC LTP",
            36 => "SLS",
            _ => ""
        };
    }

    public static readonly int[] AacSampleRates =
    {
        96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050,
        16000, 12000, 11025,  8000,  7350,     0,     0, 57600,
        51200, 40000, 38400, 34150, 28800, 25600, 20000, 19200,
        17075, 14400, 12800,  9600,     0,     0,     0
    };

    private const int AacChannelsSize = 8;

    public static readonly int[] AacChannels = { 0, 1, 2, 3, 4, 5, 6, 8 };

    private static readonly byte[] AacChannelModes =
    {
        (byte)TSAudioMode.Unknown,
        (byte)TSAudioMode.Mono,
        (byte)TSAudioMode.Stereo,
        (byte)TSAudioMode.Extended,
        (byte)TSAudioMode.Surround,
        (byte)TSAudioMode.Surround,
        (byte)TSAudioMode.Surround,
        (byte)TSAudioMode.Surround,
    };

    public static void Scan(TSAudioStream stream, TSStreamBuffer buffer, ref string tag)
    {
        if (stream.IsInitialized) return;

        int syncWord = buffer.ReadBits2(12);
        if (syncWord != 0b1111_1111_1111) return;

        // fixed header
        int audioVersionID = buffer.ReadBits2(1);
        int layerIndex = buffer.ReadBits2(2);
        bool protectionAbsent = buffer.ReadBool();
        int profileObjectType = buffer.ReadBits2(2);
        int samplingRateIndex = buffer.ReadBits2(4);
        bool privateBit = buffer.ReadBool();
        int channelMode = buffer.ReadBits2(3);
        bool originalBit = buffer.ReadBool();
        bool home = buffer.ReadBool();


        stream.SampleRate = samplingRateIndex <= 13 ? AacSampleRates[samplingRateIndex] : 0;


        if (channelMode <= AacChannelsSize)
        {
            stream.AudioMode = (TSAudioMode)AacChannelModes[channelMode];
            stream.ChannelCount = AacChannels[channelMode];
        }
        else
        {
            stream.ChannelCount = 0;
            stream.AudioMode = TSAudioMode.Unknown;
        }

        if (channelMode is >= 7 and <= 8)
        {
            stream.ChannelCount--;
            stream.LFE = 1;
        }
        else
            stream.LFE = 0;

        stream.ExtendedData = $"{AacID[audioVersionID]} {GetAacProfile(profileObjectType)}";

        stream.IsVBR = true;
        stream.IsInitialized = true;
    }
}