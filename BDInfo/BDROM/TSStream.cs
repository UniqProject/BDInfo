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

public enum TSStreamType : byte
{
    Unknown = 0,
    MPEG1_VIDEO = 0x01,
    MPEG2_VIDEO = 0x02,
    AVC_VIDEO = 0x1b,
    MVC_VIDEO = 0x20,
    HEVC_VIDEO = 0x24,
    VC1_VIDEO = 0xea,
    MPEG1_AUDIO = 0x03,
    MPEG2_AUDIO = 0x04,
    MPEG2_AAC_AUDIO = 0x0F,
    MPEG4_AAC_AUDIO = 0x11,
    LPCM_AUDIO = 0x80,
    AC3_AUDIO = 0x81,
    AC3_PLUS_AUDIO = 0x84,
    AC3_PLUS_SECONDARY_AUDIO = 0xA1,
    AC3_TRUE_HD_AUDIO = 0x83,
    DTS_AUDIO = 0x82,
    DTS_HD_AUDIO = 0x85,
    DTS_HD_SECONDARY_AUDIO = 0xA2,
    DTS_HD_MASTER_AUDIO = 0x86,
    PRESENTATION_GRAPHICS = 0x90,
    INTERACTIVE_GRAPHICS = 0x91,
    SUBTITLE = 0x92
}

public enum TSVideoFormat : byte
{
    Unknown = 0,
    VIDEOFORMAT_480i = 1,
    VIDEOFORMAT_576i = 2,
    VIDEOFORMAT_480p = 3,
    VIDEOFORMAT_1080i = 4,
    VIDEOFORMAT_720p = 5,
    VIDEOFORMAT_1080p = 6,
    VIDEOFORMAT_576p = 7,
    VIDEOFORMAT_2160p = 8,
}

public enum TSFrameRate : byte
{
    Unknown = 0,
    FRAMERATE_23_976 = 1,
    FRAMERATE_24 = 2,
    FRAMERATE_25 = 3,
    FRAMERATE_29_97 = 4,
    FRAMERATE_50 = 6,
    FRAMERATE_59_94 = 7
}

public enum TSChannelLayout : byte
{
    Unknown = 0,
    CHANNELLAYOUT_MONO = 1,
    CHANNELLAYOUT_STEREO = 3,
    CHANNELLAYOUT_MULTI = 6,
    CHANNELLAYOUT_COMBO = 12
}

public enum TSSampleRate : byte
{
    Unknown = 0,
    SAMPLERATE_48 = 1,
    SAMPLERATE_96 = 4,
    SAMPLERATE_192 = 5,
    SAMPLERATE_48_192 = 12,
    SAMPLERATE_48_96 = 14
}

public enum TSAspectRatio
{
    Unknown = 0,
    ASPECT_4_3 = 2,
    ASPECT_16_9 = 3,
    ASPECT_2_21 = 4
}

public class TSDescriptor
{
    public byte Name;
    public byte[] Value;

    public TSDescriptor(byte name, byte length)
    {
        Name = name;
        Value = new byte[length];
    }

    public TSDescriptor Clone()
    {
        var descriptor = new TSDescriptor(Name, (byte)Value.Length);
        Value.CopyTo(descriptor.Value, 0);
        return descriptor;
    }
}

public abstract class TSStream
{
    public TSStream()
    {
    }

    public override string ToString()
    {
        return FormattableString.Invariant($"{CodecShortName} ({PID})");
    }

    public ushort PID;
    public TSStreamType StreamType;
    public List<TSDescriptor> Descriptors;
    public long BitRate;
    public long ActiveBitRate = 0;
    public bool IsVBR;
    public bool IsInitialized;
    public string LanguageName;
    public bool IsHidden = false;

    public ulong PayloadBytes = 0;
    public ulong PacketCount = 0;
    public double PacketSeconds = 0;
    public int AngleIndex = 0;

    public bool? BaseView;

    public ulong PacketSize => PacketCount * 192;

    private string _languageCode;

    public string LanguageCode
    {
        get => _languageCode;
        set
        {
            _languageCode = value;
            LanguageName = LanguageCodes.GetName(value);
        }
    }

    public bool IsVideoStream
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.MPEG1_VIDEO:
                case TSStreamType.MPEG2_VIDEO:
                case TSStreamType.AVC_VIDEO:
                case TSStreamType.MVC_VIDEO:
                case TSStreamType.VC1_VIDEO:
                case TSStreamType.HEVC_VIDEO:
                    return true;

                default:
                    return false;
            }
        }
    }

    public bool IsAudioStream
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.MPEG1_AUDIO:
                case TSStreamType.MPEG2_AUDIO:
                case TSStreamType.MPEG2_AAC_AUDIO:
                case TSStreamType.MPEG4_AAC_AUDIO:
                case TSStreamType.LPCM_AUDIO:
                case TSStreamType.AC3_AUDIO:
                case TSStreamType.AC3_PLUS_AUDIO:
                case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                case TSStreamType.AC3_TRUE_HD_AUDIO:
                case TSStreamType.DTS_AUDIO:
                case TSStreamType.DTS_HD_AUDIO:
                case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                case TSStreamType.DTS_HD_MASTER_AUDIO:
                    return true;

                default:
                    return false;
            }
        }
    }

    public bool IsGraphicsStream
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.PRESENTATION_GRAPHICS:
                case TSStreamType.INTERACTIVE_GRAPHICS:
                    return true;

                default:
                    return false;
            }
        }
    }

    public bool IsTextStream
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.SUBTITLE:
                    return true;

                default:
                    return false;
            }
        }
    }

    public string CodecName
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.MPEG1_VIDEO:
                    return "MPEG-1 Video";
                case TSStreamType.MPEG2_VIDEO:
                    return "MPEG-2 Video";
                case TSStreamType.AVC_VIDEO:
                    return "MPEG-4 AVC Video";
                case TSStreamType.MVC_VIDEO:
                    return "MPEG-4 MVC Video";
                case TSStreamType.HEVC_VIDEO:
                    return "MPEG-H HEVC Video";
                case TSStreamType.VC1_VIDEO:
                    return "VC-1 Video";
                case TSStreamType.MPEG1_AUDIO:
                case TSStreamType.MPEG2_AUDIO:
                case TSStreamType.MPEG2_AAC_AUDIO:
                case TSStreamType.MPEG4_AAC_AUDIO:
                    return (string)((TSAudioStream)this).ExtendedData;
                case TSStreamType.LPCM_AUDIO:
                    return "LPCM Audio";
                case TSStreamType.AC3_AUDIO:
                    return ((TSAudioStream)this).AudioMode == TSAudioMode.Extended
                        ? "Dolby Digital EX Audio"
                        : "Dolby Digital Audio";
                case TSStreamType.AC3_PLUS_AUDIO:
                    return ((TSAudioStream)this).HasExtensions
                        ? "Dolby Digital Plus/Atmos Audio"
                        : "Dolby Digital Plus Audio";
                case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                    return "Dolby Digital Plus Audio";
                case TSStreamType.AC3_TRUE_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "Dolby TrueHD/Atmos Audio" 
                        : "Dolby TrueHD Audio";
                case TSStreamType.DTS_AUDIO:
                    return ((TSAudioStream)this).AudioMode == TSAudioMode.Extended 
                        ? "DTS-ES Audio" 
                        : "DTS Audio";
                case TSStreamType.DTS_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X High-Res Audio" 
                        : "DTS-HD High-Res Audio";
                case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                    return "DTS Express";
                case TSStreamType.DTS_HD_MASTER_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X Master Audio" 
                        : "DTS-HD Master Audio";
                case TSStreamType.PRESENTATION_GRAPHICS:
                    return "Presentation Graphics";
                case TSStreamType.INTERACTIVE_GRAPHICS:
                    return "Interactive Graphics";
                case TSStreamType.SUBTITLE:
                    return "Subtitle";
                default:
                    return "UNKNOWN";
            }
        }
    }

    public string CodecAltName
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.MPEG1_VIDEO:
                    return "MPEG-1";
                case TSStreamType.MPEG2_VIDEO:
                    return "MPEG-2";
                case TSStreamType.AVC_VIDEO:
                    return "AVC";
                case TSStreamType.MVC_VIDEO:
                    return "MVC";
                case TSStreamType.HEVC_VIDEO:
                    return "HEVC";
                case TSStreamType.VC1_VIDEO:
                    return "VC-1";
                case TSStreamType.MPEG1_AUDIO:
                    return "MP1";
                case TSStreamType.MPEG2_AUDIO:
                    return "MP2";
                case TSStreamType.MPEG2_AAC_AUDIO:
                    return "MPEG-2 AAC";
                case TSStreamType.MPEG4_AAC_AUDIO:
                    return "MPEG-4 AAC";
                case TSStreamType.LPCM_AUDIO:
                    return "LPCM";
                case TSStreamType.AC3_AUDIO:
                    return "DD AC3";
                case TSStreamType.AC3_PLUS_AUDIO:
                case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                    return "DD AC3+";
                case TSStreamType.AC3_TRUE_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "Dolby Atmos" 
                        : "Dolby TrueHD";
                case TSStreamType.DTS_AUDIO:
                    return "DTS";
                case TSStreamType.DTS_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X Hi-Res" 
                        : "DTS-HD Hi-Res";
                case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                    return "DTS Express";
                case TSStreamType.DTS_HD_MASTER_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X Master" 
                        : "DTS-HD Master";
                case TSStreamType.PRESENTATION_GRAPHICS:
                    return "PGS";
                case TSStreamType.INTERACTIVE_GRAPHICS:
                    return "IGS";
                case TSStreamType.SUBTITLE:
                    return "SUB";
                default:
                    return "UNKNOWN";
            }
        }
    }

    public string CodecShortName
    {
        get
        {
            switch (StreamType)
            {
                case TSStreamType.MPEG1_VIDEO:
                    return "MPEG-1";
                case TSStreamType.MPEG2_VIDEO:
                    return "MPEG-2";
                case TSStreamType.AVC_VIDEO:
                    return "AVC";
                case TSStreamType.MVC_VIDEO:
                    return "MVC";
                case TSStreamType.HEVC_VIDEO:
                    return "HEVC";
                case TSStreamType.VC1_VIDEO:
                    return "VC-1";
                case TSStreamType.MPEG1_AUDIO:
                    return "MP1";
                case TSStreamType.MPEG2_AUDIO:
                    return "MP2";
                case TSStreamType.MPEG2_AAC_AUDIO:
                    return "MPEG-2 AAC";
                case TSStreamType.MPEG4_AAC_AUDIO:
                    return "MPEG-4 AAC";
                case TSStreamType.LPCM_AUDIO:
                    return "LPCM";
                case TSStreamType.AC3_AUDIO:
                    return ((TSAudioStream)this).AudioMode == TSAudioMode.Extended 
                        ? "AC3-EX" 
                        : "AC3";
                case TSStreamType.AC3_PLUS_AUDIO:
                case TSStreamType.AC3_PLUS_SECONDARY_AUDIO:
                    return "AC3+";
                case TSStreamType.AC3_TRUE_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "Atmos" 
                        : "TrueHD";
                case TSStreamType.DTS_AUDIO:
                    return ((TSAudioStream)this).AudioMode == TSAudioMode.Extended 
                        ? "DTS-ES" 
                        : "DTS";
                case TSStreamType.DTS_HD_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X HR" 
                        : "DTS-HD HR";
                case TSStreamType.DTS_HD_SECONDARY_AUDIO:
                    return "DTS Express";
                case TSStreamType.DTS_HD_MASTER_AUDIO:
                    return ((TSAudioStream)this).HasExtensions 
                        ? "DTS:X MA" 
                        : "DTS-HD MA";
                case TSStreamType.PRESENTATION_GRAPHICS:
                    return "PGS";
                case TSStreamType.INTERACTIVE_GRAPHICS:
                    return "IGS";
                case TSStreamType.SUBTITLE:
                    return "SUB";
                default:
                    return "UNKNOWN";
            }
        }
    }

    public virtual string Description => "";

    public abstract TSStream Clone();

    protected void CopyTo(TSStream stream)
    {
        if (stream == null) return;

        stream.PID = PID;
        stream.StreamType = StreamType;
        stream.IsVBR = IsVBR;
        stream.BitRate = BitRate;
        stream.IsInitialized = IsInitialized;
        stream.LanguageCode = _languageCode;

        if (Descriptors == null) return;

        stream.Descriptors = new List<TSDescriptor>();
        foreach (var descriptor in Descriptors)
        {
            stream.Descriptors.Add(descriptor.Clone());
        }
    }
}

public class TSVideoStream : TSStream
{
    public TSVideoStream()
    {
    }

    public int Width;
    public int Height;
    public bool IsInterlaced;
    public int FrameRateEnumerator;
    public int FrameRateDenominator;
    public TSAspectRatio AspectRatio;
    public string EncodingProfile;

    public object ExtendedData;

    private TSVideoFormat _videoFormat;
    public TSVideoFormat VideoFormat
    {
        get => _videoFormat;
        set
        {
            _videoFormat = value;
            switch (value)
            {
                case TSVideoFormat.VIDEOFORMAT_480i:
                    Height = 480;
                    IsInterlaced = true;
                    break;
                case TSVideoFormat.VIDEOFORMAT_480p:
                    Height = 480;
                    IsInterlaced = false;
                    break;
                case TSVideoFormat.VIDEOFORMAT_576i:
                    Height = 576;
                    IsInterlaced = true;
                    break;
                case TSVideoFormat.VIDEOFORMAT_576p:
                    Height = 576;
                    IsInterlaced = false;
                    break;
                case TSVideoFormat.VIDEOFORMAT_720p:
                    Height = 720;
                    IsInterlaced = false;
                    break;
                case TSVideoFormat.VIDEOFORMAT_1080i:
                    Height = 1080;
                    IsInterlaced = true;
                    break;
                case TSVideoFormat.VIDEOFORMAT_1080p:
                    Height = 1080;
                    IsInterlaced = false;
                    break;
                case TSVideoFormat.VIDEOFORMAT_2160p:
                    Height = 2160;
                    IsInterlaced = false;
                    break;
            }
        }
    }

    private TSFrameRate _frameRate;
    public TSFrameRate FrameRate
    {
        get => _frameRate;
        set
        {
            _frameRate = value;
            switch (value)
            {
                case TSFrameRate.FRAMERATE_23_976:
                    FrameRateEnumerator = 24000;
                    FrameRateDenominator = 1001;
                    break;
                case TSFrameRate.FRAMERATE_24:
                    FrameRateEnumerator = 24000;
                    FrameRateDenominator = 1000;
                    break;
                case TSFrameRate.FRAMERATE_25:
                    FrameRateEnumerator = 25000;
                    FrameRateDenominator = 1000;
                    break;
                case TSFrameRate.FRAMERATE_29_97:
                    FrameRateEnumerator = 30000;
                    FrameRateDenominator = 1001;
                    break;
                case TSFrameRate.FRAMERATE_50:
                    FrameRateEnumerator = 50000;
                    FrameRateDenominator = 1000;
                    break;
                case TSFrameRate.FRAMERATE_59_94:
                    FrameRateEnumerator = 60000;
                    FrameRateDenominator = 1001;
                    break;
            }
        }
    }

    public override string Description
    {
        get
        {
            var description = "";

            if (BaseView != null)
            {
                if (BaseView == true)
                    description += "Right Eye";
                else
                    description += "Left Eye";
                description += " / ";
            }

            if (Height > 0)
            {
                description += FormattableString.Invariant($"{Height:D}{(IsInterlaced ? "i" : "p")} / ");
            }
            if (FrameRateEnumerator > 0 &&
                FrameRateDenominator > 0)
            {
                if (FrameRateEnumerator % FrameRateDenominator == 0)
                {
                    description +=
                        FormattableString.Invariant($"{FrameRateEnumerator / FrameRateDenominator:D} fps / ");
                }
                else
                {
                    description +=
                        FormattableString.Invariant($"{(double)FrameRateEnumerator / FrameRateDenominator:F3} fps / ");
                }

            }
            switch (AspectRatio)
            {
                case TSAspectRatio.ASPECT_4_3:
                    description += "4:3 / ";
                    break;
                case TSAspectRatio.ASPECT_16_9:
                    description += "16:9 / ";
                    break;
            }
            if (EncodingProfile != null)
            {
                description += EncodingProfile + " / ";
            }
            if (StreamType == TSStreamType.HEVC_VIDEO && ExtendedData != null)
            {
                var extendedData = (TSCodecHEVC.ExtendedDataSet)ExtendedData;
                var extendedInfo = string.Join(" / ", extendedData.ExtendedFormatInfo);
                description += extendedInfo;
            }
            if (description.EndsWith(" / "))
            {
                description = description[..^3];
            }
            return description;
        }
    }

    public override TSStream Clone()
    {
        var stream = new TSVideoStream();
        CopyTo(stream);

        stream.VideoFormat = _videoFormat;
        stream.FrameRate = _frameRate;
        stream.Width = Width;
        stream.Height = Height;
        stream.IsInterlaced = IsInterlaced;
        stream.FrameRateEnumerator = FrameRateEnumerator;
        stream.FrameRateDenominator = FrameRateDenominator;
        stream.AspectRatio = AspectRatio;
        stream.EncodingProfile = EncodingProfile;
        stream.ExtendedData = ExtendedData;

        return stream;
    }
}

public enum TSAudioMode
{
    Unknown,
    DualMono,
    Stereo,
    Surround,
    Extended,
    JointStereo,
    Mono
}

public class TSAudioStream : TSStream
{
    public TSAudioStream()
    {
    }

    public int SampleRate;
    public int ChannelCount;
    public int BitDepth;
    public int LFE;
    public int DialNorm;

    public bool HasExtensions = false;

    public object ExtendedData;

    public TSAudioMode AudioMode;
    public TSAudioStream CoreStream;
    public TSChannelLayout ChannelLayout;

    public static int ConvertSampleRate(TSSampleRate sampleRate)
    {
        switch (sampleRate)
        {
            case TSSampleRate.SAMPLERATE_48:
                return 48000;

            case TSSampleRate.SAMPLERATE_96:
            case TSSampleRate.SAMPLERATE_48_96:
                return 96000;

            case TSSampleRate.SAMPLERATE_192:
            case TSSampleRate.SAMPLERATE_48_192:
                return 192000;
        }
        return 0;
    }

    public string ChannelDescription
    {
        get
        {
            if (ChannelLayout == TSChannelLayout.CHANNELLAYOUT_MONO &&
                ChannelCount == 2)
            {
            }

            var description = "";
            if (ChannelCount > 0)
            {
                description += FormattableString.Invariant($"{ChannelCount:D}.{LFE:D}");
            }
            else
            {
                switch (ChannelLayout)
                {
                    case TSChannelLayout.CHANNELLAYOUT_MONO:
                        description += "1.0";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_STEREO:
                        description += "2.0";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_MULTI:
                        description += "5.1";
                        break;
                }
            }

            if (AudioMode != TSAudioMode.Extended) return description;

            switch (StreamType)
            {
                case TSStreamType.AC3_AUDIO:
                    description += "-EX";
                    break;
                case TSStreamType.DTS_AUDIO or TSStreamType.DTS_HD_AUDIO or TSStreamType.DTS_HD_MASTER_AUDIO:
                    description += "-ES";
                    break;
            }

            return description;
        }
    }

    public override string Description
    {
        get
        {
            var description = ChannelDescription;

            if (SampleRate > 0)
            {
                description += FormattableString.Invariant($" / {SampleRate / 1000:D} kHz");
            }
            if (BitRate > 0)
            {
                long coreBitRate = 0;
                if (StreamType == TSStreamType.AC3_TRUE_HD_AUDIO && CoreStream != null)
                    coreBitRate = CoreStream.BitRate;
                description +=
                    FormattableString.Invariant(
                        $" / {(uint)Math.Round((double)(BitRate - coreBitRate) / 1000),5:D} kbps");
            }
            if (BitDepth > 0)
            {
                description += FormattableString.Invariant($" / {BitDepth:D}-bit");
            }
            if (DialNorm != 0)
            {
                description += FormattableString.Invariant($" / DN {DialNorm}dB");
            }
            if (ChannelCount == 2)
            {
                switch (AudioMode)
                {
                    case TSAudioMode.DualMono:
                        description += " / Dual Mono";
                        break;

                    case TSAudioMode.Surround:
                        description += " / Dolby Surround";
                        break;

                    case TSAudioMode.JointStereo:
                        description += " / Joint Stereo";
                        break;
                }
            }
            if (description.EndsWith(" / "))
            {
                description = description[..^3];
            }

            if (CoreStream == null) return description;
            var codec = CoreStream.StreamType switch
            {
                TSStreamType.AC3_AUDIO => "AC3 Embedded",
                TSStreamType.DTS_AUDIO => "DTS Core",
                TSStreamType.AC3_PLUS_AUDIO => "DD+ Embedded",
                _ => ""
            };

            description += FormattableString.Invariant($" ({codec}: {CoreStream.Description})");
            return description;
        }
    }

    public override TSStream Clone()
    {
        var stream = new TSAudioStream();
        CopyTo(stream);

        stream.SampleRate = SampleRate;
        stream.ChannelLayout = ChannelLayout;
        stream.ChannelCount = ChannelCount;
        stream.BitDepth = BitDepth;
        stream.LFE = LFE;
        stream.DialNorm = DialNorm;
        stream.AudioMode = AudioMode;
        stream.ExtendedData = ExtendedData;

        if (CoreStream != null)
        {
            stream.CoreStream = (TSAudioStream)CoreStream.Clone();
        }

        return stream;
    }
}

public class TSGraphicsStream : TSStream
{
    public int Width;
    public int Height;
    public int Captions;
    public int ForcedCaptions;
    public Dictionary<int, TSCodecPGS.Frame> CaptionIDs;
    public TSCodecPGS.Frame LastFrame;

    public TSGraphicsStream()
    {
        IsVBR = true;
        IsInitialized = false;
        Width = 0;
        Height = 0;
        Captions = 0;
        ForcedCaptions = 0;
        CaptionIDs = new Dictionary<int, TSCodecPGS.Frame>();
        LastFrame = new TSCodecPGS.Frame();
    }

    public override TSStream Clone()
    {
        var stream = new TSGraphicsStream();
        CopyTo(stream);
        stream.Width = Width;
        stream.Height = Height;
        stream.Captions = Captions;
        stream.ForcedCaptions = ForcedCaptions;
        stream.CaptionIDs = CaptionIDs;
        stream.LastFrame = LastFrame;
        return stream;
    }

    public override string Description
    {
        get
        {
            var description = string.Empty;
            if (Width > 0 || Height > 0)
            {
                description = FormattableString.Invariant($"{Width:D}x{Height:D}");
            }

            if (Captions <= 0 && ForcedCaptions <= 0) return description;

            if (Captions > 0)
            {
                description += FormattableString.Invariant($" / {Captions:D} Caption{(Captions > 1 ? "s" : "")}");
            }

            if (ForcedCaptions > 0)
            {
                var descriptionStr =
                    FormattableString.Invariant($"{ForcedCaptions:D} Forced Caption{(ForcedCaptions > 1 ? "s" : "")}");

                description += Captions > 0
                    ? FormattableString.Invariant($" ({descriptionStr})")
                    : FormattableString.Invariant($" / {descriptionStr}");
            }
            return description;
        }
    }
}

public class TSTextStream : TSStream
{
    public TSTextStream()
    {
        IsVBR = true;
        IsInitialized = true;
    }

    public override TSStream Clone()
    {
        var stream = new TSTextStream();
        CopyTo(stream);
        return stream;
    }
}