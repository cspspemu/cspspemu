//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  This material may not be duplicated in whole or in part, except for 
//  personal use, without the express written consent of the author. 
//
//  Email:  ianier@hotmail.com
//
//  Copyright (C) 1999-2003 Ianier Munoz. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CSPspEmu.Core.Audio.Impl.WaveOut.WaveLib
{
    public class FifoStream : Stream
    {
        private const int BlockSize = 65536;
        private const int MaxBlocksInCache = 3 * 1024 * 1024 / BlockSize;

        private int _mSize;
        private int _mRPos;
        private int _mWPos;
        private readonly Stack _mUsedBlocks = new Stack();
        private readonly List<byte[]> _mBlocks = new List<byte[]>();

        private byte[] AllocBlock() => _mUsedBlocks.Count > 0 ? (byte[]) _mUsedBlocks.Pop() : new byte[BlockSize];

        private void FreeBlock(byte[] block)
        {
            if (_mUsedBlocks.Count < MaxBlocksInCache)
                _mUsedBlocks.Push(block);
        }

        private byte[] GetWBlock()
        {
            if (_mWPos < BlockSize && _mBlocks.Count > 0)
                return (byte[]) _mBlocks[^1];
            else
            {
                var result = AllocBlock();
                _mBlocks.Add(result);
                _mWPos = 0;
                return result;
            }
        }

        // Stream members
        public override bool CanRead => true;

        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                lock (this) return _mSize;
            }
        }

        public override long Position
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override void Close() => Flush();

        public override void Flush()
        {
            lock (this)
            {
                foreach (byte[] block in _mBlocks)
                    FreeBlock(block);
                _mBlocks.Clear();
                _mRPos = 0;
                _mWPos = 0;
                _mSize = 0;
            }
        }

        public override void SetLength(long len) => throw new InvalidOperationException();

        public override long Seek(long pos, SeekOrigin o) => throw new InvalidOperationException();

        public override int Read(byte[] buf, int ofs, int count)
        {
            lock (this)
            {
                var result = Peek(buf, ofs, count);
                Advance(result);
                return result;
            }
        }

        public override void Write(byte[] buf, int ofs, int count)
        {
            lock (this)
            {
                var left = count;
                while (left > 0)
                {
                    var toWrite = Math.Min(BlockSize - _mWPos, left);
                    Array.Copy(buf, ofs + count - left, GetWBlock(), _mWPos, toWrite);
                    _mWPos += toWrite;
                    left -= toWrite;
                }
                _mSize += count;
            }
        }

        // extra stuff
        public int Advance(int count)
        {
            lock (this)
            {
                var sizeLeft = count;
                while (sizeLeft > 0 && _mSize > 0)
                {
                    if (_mRPos == BlockSize)
                    {
                        _mRPos = 0;
                        FreeBlock((byte[]) _mBlocks[0]);
                        _mBlocks.RemoveAt(0);
                    }
                    var toFeed = _mBlocks.Count == 1
                        ? Math.Min(_mWPos - _mRPos, sizeLeft)
                        : Math.Min(BlockSize - _mRPos, sizeLeft);
                    _mRPos += toFeed;
                    sizeLeft -= toFeed;
                    _mSize -= toFeed;
                }
                return count - sizeLeft;
            }
        }

        public int Peek(byte[] buf, int ofs, int count)
        {
            lock (this)
            {
                var sizeLeft = count;
                var tempBlockPos = _mRPos;
                var tempSize = _mSize;

                var currentBlock = 0;
                while (sizeLeft > 0 && tempSize > 0)
                {
                    if (tempBlockPos == BlockSize)
                    {
                        tempBlockPos = 0;
                        currentBlock++;
                    }
                    var upper = currentBlock < _mBlocks.Count - 1 ? BlockSize : _mWPos;
                    var toFeed = Math.Min(upper - tempBlockPos, sizeLeft);
                    Array.Copy((byte[]) _mBlocks[currentBlock], tempBlockPos, buf, ofs + count - sizeLeft, toFeed);
                    sizeLeft -= toFeed;
                    tempBlockPos += toFeed;
                    tempSize -= toFeed;
                }
                return count - sizeLeft;
            }
        }
    }
}