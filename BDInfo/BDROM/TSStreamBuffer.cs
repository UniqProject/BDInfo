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

using System.Collections;
using System.Collections.Specialized;

namespace BDInfoLib.BDROM;

public class TSStreamBuffer
{
    private readonly MemoryStream _stream;
    private int _skipBits;
    private int _skippedBytes;
    private readonly byte[] _buffer;
    private int _bufferLength;
    public int TransferLength;

    public TSStreamBuffer()
    {
        _buffer = new byte[5242880];
        _stream = new MemoryStream(_buffer);
    }

    public long Length => _bufferLength;

    public long Position => _stream.Position;

    public void Add(byte[] buffer, int offset, int length)
    {
        TransferLength += length;

        if (_bufferLength + length >= _buffer.Length)
        {
            length = _buffer.Length - _bufferLength;
        }

        if (length <= 0) return;

        Array.Copy(buffer, offset, _buffer, _bufferLength, length);
        _bufferLength += length;
    }

    public void Seek(long offset, SeekOrigin loc)
    {
        _stream.Seek(offset, loc);
    }

    public void Reset()
    {
        _bufferLength = 0;
        TransferLength = 0;
    }

    public void BeginRead()
    {
        _skipBits = 0;
        _skippedBytes = 0;
        _stream.Seek(0, SeekOrigin.Begin);
    }

    public void EndRead()
    {
    }

    public byte[] ReadBytes(int bytes)
    {
        if (_stream.Position + bytes >= _bufferLength)
        {
            return null;
        }

        var value = new byte[bytes];
        _stream.Read(value, 0, bytes);
        return value;
    }

    public byte ReadByte(bool skipH26XEmulationByte)
    {
        var tempByte = (byte)_stream.ReadByte();
        var tempPosition = _stream.Position;

        if (!skipH26XEmulationByte || tempByte != 0x03) return tempByte;

        _stream.Seek(-3, SeekOrigin.Current);
        if (_stream.ReadByte() == 0x00 && _stream.ReadByte() == 0x00)
        {
            _stream.Seek(1, SeekOrigin.Current);
            tempByte = (byte)_stream.ReadByte();
            _skippedBytes++;
        }
        else
        {
            _stream.Seek(tempPosition, SeekOrigin.Begin);
        }
        return tempByte;
    }

    public byte ReadByte()
    {
        return ReadByte(false);
    }


    public bool ReadBool(bool skipH26XEmulationByte)
    {
        var pos = _stream.Position;
        _skippedBytes = 0;
        if (pos == _bufferLength) return false;

        var data = ReadByte(skipH26XEmulationByte);
        var vector = new BitVector32(data);

        var value = vector[1 << 8 - _skipBits - 1];

        _skipBits += 1;
        _stream.Seek(pos + (_skipBits >> 3) + _skippedBytes, SeekOrigin.Begin);
        _skipBits %= 8;

        return value;
    }

    public bool ReadBool()
    {
        return ReadBool(false);
    }

    public ushort ReadBits2(int bits, bool skipH26XEmulationByte)
    {
        var pos = _stream.Position;
        _skippedBytes = 0;

        var shift = 8;
        var data = 0;
        for (var i = 0; i < 2; i++)
        {
            if (pos + i >= _bufferLength) break;
            data += ReadByte(skipH26XEmulationByte) << shift;
            shift -= 8;
        }
        var vector = new BitVector32(data);

        ushort value = 0;
        for (var i = _skipBits; i < _skipBits + bits; i++)
        {
            value <<= 1;
            value += (ushort)(vector[1 << 16 - i - 1] ? 1 : 0);
        }
        _skipBits += bits;
        _stream.Seek(pos + (_skipBits >> 3) + _skippedBytes, SeekOrigin.Begin);
        _skipBits %= 8;

        return value;
    }

    public ushort ReadBits2(int bits)
    {
        return ReadBits2(bits, false);
    }

    public uint ReadBits4(int bits, bool skipH26XEmulationByte)
    {
        var pos = _stream.Position;
        _skippedBytes = 0;

        var shift = 24;
        var data = 0;
        for (var i = 0; i < 4; i++)
        {
            if (pos + i >= _bufferLength) break;
            data += ReadByte(skipH26XEmulationByte) << shift;
            shift -= 8;
        }
        var vector = new BitVector32(data);

        uint value = 0;
        for (var i = _skipBits; i < _skipBits + bits; i++)
        {
            value <<= 1;
            value += (uint)(vector[1 << 32 - i - 1] ? 1 : 0);
        }
        _skipBits += bits;
        _stream.Seek(pos + (_skipBits >> 3) + _skippedBytes, SeekOrigin.Begin);
        _skipBits %= 8;

        return value;
    }

    public uint ReadBits4(int bits)
    {
        return ReadBits4(bits, false);
    }

    public ulong ReadBits8(int bits, bool skipH26XEmulationByte)
    {
        var pos = _stream.Position;
        _skippedBytes = 0;

        var shift = 24;
        var data = 0;
        for (var i = 0; i < 4; i++)
        {
            if (pos + i >= _bufferLength) break;
            data += ReadByte(skipH26XEmulationByte) << shift;
            shift -= 8;
        }

        shift = 24;
        var data2 = 0;
        for (var i = 0; i < 4; i++)
        {
            if (pos + i >= _bufferLength) break;
            data2 += ReadByte(skipH26XEmulationByte) << shift;
            shift -= 8;
        }
        var vector = new BitArray(new[] { data2, data });


        ulong value = 0;
        for (var i = _skipBits; i < _skipBits + bits; i++)
        {
            value <<= 1;
            value += (ulong)(vector[64 - i - 1] ? 1 : 0);
        }

        _skipBits += bits;
        _stream.Seek(pos + (_skipBits >> 3) + _skippedBytes, SeekOrigin.Begin);
        _skipBits %= 8;

        return value;
    }

    public ulong ReadBits8(int bits)
    {
        return ReadBits8(bits, false);
    }

    public void BSSkipBits(int bits, bool skipH26XEmulationByte)
    {
        var count = bits / 16 + (bits % 16 > 0 ? 1 : 0);
        var bitsRead = 0;
        for (var i = 0; i < count; i++)
        {
            var bitsToRead = bits - bitsRead;
            bitsToRead = bitsToRead > 16 ? 16 : bitsToRead;
            ReadBits2(bitsToRead, skipH26XEmulationByte);
            bitsRead += bitsToRead;
        }
    }

    public void BSSkipBits(int bits)
    {
        BSSkipBits(bits, false);
    }

    public void BSSkipNextByte()
    {
        if (_skipBits > 0)
            BSSkipBits(8 - _skipBits);
    }

    public void BSResetBits()
    {
        _skipBits = 0;
    }

    public void BSSkipBytes(int bytes, bool skipH26XEmulationByte)
    {
        if (bytes > 0)
        {
            for (var i = 0; i < bytes; i++)
                ReadByte(skipH26XEmulationByte);
        }
        else
        {
            var pos = _stream.Position;
            _stream.Seek(pos + (_skipBits >> 3) + bytes, SeekOrigin.Begin);
        }
    }

    public void BSSkipBytes(int bytes)
    {
        BSSkipBytes(bytes, false);
    }

    public uint ReadExp(bool skipH26XEmulationByte)
    {
        byte leadingZeroes = 0;
        while (DataBitStreamRemain() > 0 && !ReadBool(skipH26XEmulationByte))
            leadingZeroes++;

        var infoD = Math.Pow(2, leadingZeroes);
        var result = (uint)infoD - 1 + ReadBits4(leadingZeroes, skipH26XEmulationByte);

        return result;
    }

    public uint ReadExp()
    {
        return ReadExp(false);
    }

    public void SkipExp(bool skipH26XEmulationByte)
    {
        byte leadingZeroes = 0;
        while (DataBitStreamRemain() > 0 && !ReadBool(skipH26XEmulationByte))
            leadingZeroes++;

        BSSkipBits(leadingZeroes, skipH26XEmulationByte);
    }

    public void SkipExp()
    {
        SkipExp(false);
    }

    public void SkipExpMulti(int num, bool skipH26XEmulationByte)
    {
        for (var i = 0; i < num; i++)
        {
            SkipExp(skipH26XEmulationByte);
        }
    }

    public void SkipExpMulti(int num)
    {
        SkipExpMulti(num, false);
    }

    public long DataBitStreamRemain()
    {
        var remain = (_bufferLength - _stream.Position) * 8 - _skipBits;
        return remain;
    }

    public long DataBitStreamRemainBytes()
    {
        return _bufferLength - _stream.Position;
    }
}