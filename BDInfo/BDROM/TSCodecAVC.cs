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

// ReSharper disable NotAccessedVariable
// ReSharper disable RedundantAssignment

namespace BDInfoLib.BDROM;

public abstract class TSCodecAVC
{
    public static void Scan(TSVideoStream stream, TSStreamBuffer buffer, ref string tag)
    {
        uint parse = 0;
        byte accessUnitDelimiterParse = 0;
        byte sequenceParameterSetParse = 0;
        string profile = null;
        byte constraintSet0Flag = 0;
        byte constraintSet1Flag = 0;
        byte constraintSet2Flag = 0;
        byte constraintSet3Flag = 0;

        for (var i = 0; i < buffer.Length; i++)
        {
            parse = (parse << 8) + buffer.ReadByte(true);

            if (parse == 0x00000109)
            {
                accessUnitDelimiterParse = 1;
            }
            else if (accessUnitDelimiterParse > 0)
            {
                --accessUnitDelimiterParse;

                if (accessUnitDelimiterParse != 0) continue;

                switch ((parse & 0xFF) >> 5)
                {
                    case 0: // I
                    case 3: // SI
                    case 5: // I, SI
                        tag = "I";
                        break;

                    case 1: // I, P
                    case 4: // SI, SP
                    case 6: // I, SI, P, SP
                        tag = "P";
                        break;

                    case 2: // I, P, B
                    case 7: // I, SI, P, SP, B
                        tag = "B";
                        break;
                }
                if (stream.IsInitialized) return;
            }
            else if (parse is 0x00000127 or 0x00000167)
            {
                sequenceParameterSetParse = 3;
            }
            else if (sequenceParameterSetParse > 0)
            {
                --sequenceParameterSetParse;
                if (!stream.IsInitialized)
                    switch (sequenceParameterSetParse)
                    {
                        case 2:
                            profile = (parse & 0xFF) switch
                            {
                                66 => "Baseline Profile",
                                77 => "Main Profile",
                                88 => "Extended Profile",
                                100 => "High Profile",
                                110 => "High 10 Profile",
                                122 => "High 4:2:2 Profile",
                                144 => "High 4:4:4 Profile",
                                _ => "Unknown Profile"
                            };
                            break;

                        case 1:
                            constraintSet0Flag = (byte)((parse & 0x80) >> 7);
                            constraintSet1Flag = (byte)((parse & 0x40) >> 6);
                            constraintSet2Flag = (byte)((parse & 0x20) >> 5);
                            constraintSet3Flag = (byte)((parse & 0x10) >> 4);
                            break;

                        case 0:
                            var b = (byte)(parse & 0xFF);
                            string level;
                            if (b == 11 && constraintSet3Flag == 1)
                            {
                                level = "1b";
                            }
                            else
                            {
                                level = $"{b / 10:D}.{b - b / 10 * 10:D}";
                            }
                            stream.EncodingProfile = $"{profile} {level}";
                            stream.IsVBR = true;
                            stream.IsInitialized = true;
                            break;
                    }
            }
        }
    }
}