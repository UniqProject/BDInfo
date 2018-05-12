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
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace BDInfo
{
    public class TSStreamBuffer
    {
        private readonly MemoryStream _stream;
        private int _skipBits;
        private readonly byte[] _buffer;
        private int _bufferLength;
        public int TransferLength;

        public TSStreamBuffer()
        {
            _buffer = new byte[20480];
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

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public bool ReadBool()
        {
            var pos = _stream.Position;
            if (pos == _bufferLength) return false;

            var shift = 24;
            var data = 0;
            for (var i = 0; i < 4; i++)
            {
                if (pos + i >= _bufferLength) break;
                data += (_stream.ReadByte() << shift);
                shift -= 8;
            }
            var vector = new BitVector32(data);

            var value = vector[1 << (32 - _skipBits - 1)];

            _skipBits += 1;
            _stream.Seek(pos + (_skipBits >> 3), SeekOrigin.Begin);
            _skipBits = _skipBits % 8;

            return value;
        }

        public ushort ReadBits2(int bits)
        {
            var pos = _stream.Position;

            var shift = 8;
            var data = 0;
            for (var i = 0; i < 2; i++)
            {
                if (pos + i >= _bufferLength) break;
                data += (_stream.ReadByte() << shift);
                shift -= 8;
            }
            var vector = new BitVector32(data);

            ushort value = 0;
            for (var i = _skipBits; i < _skipBits + bits; i++)
            {
                value <<= 1;
                value += (ushort)(vector[1 << (16 - i - 1)] ? 1 : 0);
            }
            _skipBits += bits;
            _stream.Seek(pos + (_skipBits >> 3), SeekOrigin.Begin);
            _skipBits = _skipBits % 8;

            return value;
        }

        public uint ReadBits4(int bits)
        {
            var pos = _stream.Position;

            var shift = 24;
            var data = 0;
            for (var i = 0; i < 4; i++)
            {
                if (pos + i >= _bufferLength) break;
                data += (_stream.ReadByte() << shift);
                shift -= 8;
            }
            var vector = new BitVector32(data);

            uint value = 0;
            for (var i = _skipBits; i < _skipBits + bits; i++)
            {
                value <<= 1;
                value += (uint)(vector[1<<(32 - i - 1)] ? 1 : 0);
            }
            _skipBits += bits;
            _stream.Seek(pos + (_skipBits >> 3), SeekOrigin.Begin);
            _skipBits = _skipBits % 8;

            return value;
        }

        public ulong ReadBits8(int bits)
        {
            var pos = _stream.Position;

            var shift = 24;
            var data = 0;
            for (var i = 0; i < 4; i++)
            {
                if (pos + i >= _bufferLength) break;
                data += (_stream.ReadByte() << shift);
                shift -= 8;
            }

            shift = 24;
            var data2 = 0;
            for (var i = 0; i < 4; i++)
            {
                if (pos + i >= _bufferLength) break;
                data2 += (_stream.ReadByte() << shift);
                shift -= 8;
            }
            var vector = new BitArray(new []{data2, data});


            ulong value = 0;
            for (var i = _skipBits; i < _skipBits + bits; i++)
            {
                value <<= 1;
                value += (ulong)(vector[(64 - i - 1)] ? 1 : 0);
            }

            _skipBits += bits;
            _stream.Seek(pos + (_skipBits >> 3), SeekOrigin.Begin);
            _skipBits = _skipBits % 8;

            return value;
        }

        public void BSSkipBits(int bits)
        {
            var pos = _stream.Position;
            if (pos == _bufferLength) return;

            _skipBits += bits;
            _stream.Seek(pos + (_skipBits >> 3), SeekOrigin.Begin);
            _skipBits = _skipBits % 8;
        }

        public void BSSkipNextByte()
        {
            if (_skipBits > 0)
                BSSkipBits(8 - _skipBits);
        }

        public void BSSkipBytes(int bytes)
        {
            var pos = _stream.Position;
            if (pos + bytes >= _bufferLength) return;

            _stream.Seek(pos + bytes, SeekOrigin.Begin);
        }

        public uint ReadExp()
        {
            byte leadingZeroes = 0;
            while (DataBitStreamRemain() > 0 && !ReadBool())
                leadingZeroes++;

            var infoD = Math.Pow(2, leadingZeroes);
            var result = (uint)infoD - 1 + ReadBits4(leadingZeroes);

            return result;
        }

        public void SkipExp()
        {
            byte leadingZeroes = 0;
            while (DataBitStreamRemain() > 0 && !ReadBool())
                leadingZeroes++;

            BSSkipBits(leadingZeroes);
        }

        public void SkipExpMulti(int num)
        {
            for (int i = 0; i < num; i++)
            {
                SkipExp();
            }
        }

        public long DataBitStreamRemain()
        {
            return (_stream.Length - _stream.Position)*8 - _skipBits;
        }

        public long DataBitStreamRemainBytes()
        {
            return (_stream.Length - _stream.Position);
        }
    }
}
