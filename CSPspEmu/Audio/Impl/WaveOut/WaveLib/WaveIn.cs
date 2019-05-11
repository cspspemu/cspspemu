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
using System.Runtime.InteropServices;
using System.Threading;

namespace CSPspEmu.Core.Audio.Impl.WaveOut.WaveLib
{
    internal class WaveInHelper
    {
        public static void Try(int err)
        {
            if (err != WaveNative.MmsyserrNoerror)
                throw new Exception(err.ToString());
        }
    }

    public delegate void BufferDoneEventHandler(IntPtr data, int size);

    internal class WaveInBuffer : IDisposable
    {
        public WaveInBuffer NextBuffer;

        private AutoResetEvent m_RecordEvent = new AutoResetEvent(false);
        private IntPtr m_WaveIn;

        private WaveNative.WaveHdr _mHeader;
        private readonly byte[] _mHeaderData;
        private GCHandle _mHeaderHandle;
        private GCHandle _mHeaderDataHandle;

        private bool _mRecording;

        internal static void WaveInProc(IntPtr hdrvr, int uMsg, int dwUser, ref WaveNative.WaveHdr wavhdr, int dwParam2)
        {
            if (uMsg == WaveNative.MmWimData)
            {
                try
                {
                    GCHandle h = (GCHandle) wavhdr.dwUser;
                    WaveInBuffer buf = (WaveInBuffer) h.Target;
                    buf.OnCompleted();
                }
                catch
                {
                    // ignored
                }
            }
        }

        public WaveInBuffer(IntPtr waveInHandle, int size)
        {
            m_WaveIn = waveInHandle;

            _mHeaderHandle = GCHandle.Alloc(_mHeader, GCHandleType.Pinned);
            _mHeader.dwUser = (IntPtr) GCHandle.Alloc(this);
            _mHeaderData = new byte[size];
            _mHeaderDataHandle = GCHandle.Alloc(_mHeaderData, GCHandleType.Pinned);
            _mHeader.lpData = _mHeaderDataHandle.AddrOfPinnedObject();
            _mHeader.dwBufferLength = size;
            WaveInHelper.Try(WaveNative.waveInPrepareHeader(m_WaveIn, ref _mHeader, Marshal.SizeOf(_mHeader)));
        }

        ~WaveInBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_mHeader.lpData != IntPtr.Zero)
            {
                WaveNative.waveInUnprepareHeader(m_WaveIn, ref _mHeader, Marshal.SizeOf(_mHeader));
                _mHeaderHandle.Free();
                _mHeader.lpData = IntPtr.Zero;
            }
            m_RecordEvent.Close();
            if (_mHeaderDataHandle.IsAllocated)
                _mHeaderDataHandle.Free();
            GC.SuppressFinalize(this);
        }

        public int Size => _mHeader.dwBufferLength;
        public IntPtr Data => _mHeader.lpData;

        public bool Record()
        {
            lock (this)
            {
                m_RecordEvent.Reset();
                _mRecording = WaveNative.waveInAddBuffer(m_WaveIn, ref _mHeader, Marshal.SizeOf(_mHeader)) ==
                              WaveNative.MmsyserrNoerror;
                return _mRecording;
            }
        }

        public void WaitFor()
        {
            if (_mRecording)
                _mRecording = m_RecordEvent.WaitOne();
            else
                Thread.Sleep(0);
        }

        private void OnCompleted()
        {
            m_RecordEvent.Set();
            _mRecording = false;
        }
    }

    public class WaveInRecorder : IDisposable
    {
        private IntPtr _mWaveIn;
        private WaveInBuffer _mBuffers; // linked list
        private WaveInBuffer _mCurrentBuffer;
        private Thread _mThread;
        private BufferDoneEventHandler _mDoneProc;
        private bool _mFinished;

        private readonly WaveNative.WaveDelegate _mBufferProc = WaveInBuffer.WaveInProc;

        public static int DeviceCount => WaveNative.waveInGetNumDevs();

        public WaveInRecorder(int device, WaveFormat format, int bufferSize, int bufferCount,
            BufferDoneEventHandler doneProc)
        {
            _mDoneProc = doneProc;
            WaveInHelper.Try(WaveNative.waveInOpen(out _mWaveIn, device, format, _mBufferProc, 0,
                WaveNative.CallbackFunction));
            AllocateBuffers(bufferSize, bufferCount);
            for (var i = 0; i < bufferCount; i++)
            {
                SelectNextBuffer();
                _mCurrentBuffer.Record();
            }
            WaveInHelper.Try(WaveNative.waveInStart(_mWaveIn));
            _mThread = new Thread(ThreadProc) {IsBackground = true};
            _mThread.Start();
        }

        ~WaveInRecorder()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_mThread != null)
                try
                {
                    _mFinished = true;
                    if (_mWaveIn != IntPtr.Zero)
                        WaveNative.waveInReset(_mWaveIn);
                    WaitForAllBuffers();
                    _mThread.Join();
                    _mDoneProc = null;
                    FreeBuffers();
                    if (_mWaveIn != IntPtr.Zero)
                        WaveNative.waveInClose(_mWaveIn);
                }
                finally
                {
                    _mThread = null;
                    _mWaveIn = IntPtr.Zero;
                }
            GC.SuppressFinalize(this);
        }

        private void ThreadProc()
        {
            Console.WriteLine("WaveIn.Start()");
            try
            {
                while (!_mFinished)
                {
                    Advance();
                    if (_mDoneProc != null && !_mFinished)
                        _mDoneProc(_mCurrentBuffer.Data, _mCurrentBuffer.Size);
                    _mCurrentBuffer.Record();
                }
            }
            finally
            {
                Console.WriteLine("WaveIn.End()");
            }
        }

        private void AllocateBuffers(int bufferSize, int bufferCount)
        {
            FreeBuffers();
            if (bufferCount > 0)
            {
                _mBuffers = new WaveInBuffer(_mWaveIn, bufferSize);
                var prev = _mBuffers;
                try
                {
                    for (var i = 1; i < bufferCount; i++)
                    {
                        var buf = new WaveInBuffer(_mWaveIn, bufferSize);
                        prev.NextBuffer = buf;
                        prev = buf;
                    }
                }
                finally
                {
                    prev.NextBuffer = _mBuffers;
                }
            }
        }

        private void FreeBuffers()
        {
            _mCurrentBuffer = null;
            if (_mBuffers == null) return;
            var first = _mBuffers;
            _mBuffers = null;

            var current = first;
            do
            {
                var next = current.NextBuffer;
                current.Dispose();
                current = next;
            } while (current != first);
        }

        private void Advance()
        {
            SelectNextBuffer();
            _mCurrentBuffer.WaitFor();
        }

        private void SelectNextBuffer()
        {
            _mCurrentBuffer = _mCurrentBuffer == null ? _mBuffers : _mCurrentBuffer.NextBuffer;
        }

        private void WaitForAllBuffers()
        {
            var buf = _mBuffers;
            while (buf.NextBuffer != _mBuffers)
            {
                buf.WaitFor();
                buf = buf.NextBuffer;
            }
        }
    }
}