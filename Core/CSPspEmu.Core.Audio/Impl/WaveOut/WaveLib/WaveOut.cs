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
    internal class WaveOutHelper
    {
        public static void Try(int err)
        {
            if (err != WaveNative.MmsyserrNoerror)
                throw new Exception(err.ToString());
        }
    }

    public delegate void BufferFillEventHandler(IntPtr data, int size);

    internal class WaveOutBuffer : IDisposable
    {
        public WaveOutBuffer NextBuffer;

        private AutoResetEvent m_PlayEvent = new AutoResetEvent(false);
        private IntPtr m_WaveOut;

        private WaveNative.WaveHdr _mHeader;
        private readonly byte[] _mHeaderData;
        private GCHandle _mHeaderHandle;
        private GCHandle _mHeaderDataHandle;

        private bool _mPlaying;

        internal static void WaveOutProc(IntPtr hdrvr, int uMsg, int dwUser, ref WaveNative.WaveHdr wavhdr,
            int dwParam2)
        {
            if (uMsg != WaveNative.MmWomDone) return;
            
            try
            {
                var h = (GCHandle) wavhdr.dwUser;
                var buf = (WaveOutBuffer) h.Target;
                buf.OnCompleted();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public WaveOutBuffer(IntPtr waveOutHandle, int size)
        {
            m_WaveOut = waveOutHandle;

            _mHeaderHandle = GCHandle.Alloc(_mHeader, GCHandleType.Pinned);
            _mHeader.dwUser = (IntPtr) GCHandle.Alloc(this);
            _mHeaderData = new byte[size];
            _mHeaderDataHandle = GCHandle.Alloc(_mHeaderData, GCHandleType.Pinned);
            _mHeader.lpData = _mHeaderDataHandle.AddrOfPinnedObject();
            _mHeader.dwBufferLength = size;
            WaveOutHelper.Try(WaveNative.waveOutPrepareHeader(m_WaveOut, ref _mHeader, Marshal.SizeOf(_mHeader)));
        }

        ~WaveOutBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_mHeader.lpData != IntPtr.Zero)
            {
                WaveNative.waveOutUnprepareHeader(m_WaveOut, ref _mHeader, Marshal.SizeOf(_mHeader));
                _mHeaderHandle.Free();
                _mHeader.lpData = IntPtr.Zero;
            }
            m_PlayEvent.Close();
            if (_mHeaderDataHandle.IsAllocated)
                _mHeaderDataHandle.Free();
            GC.SuppressFinalize(this);
        }

        public int Size => _mHeader.dwBufferLength;
        public IntPtr Data => _mHeader.lpData;

        public bool Play()
        {
            lock (this)
            {
                m_PlayEvent.Reset();
                _mPlaying = WaveNative.waveOutWrite(m_WaveOut, ref _mHeader, Marshal.SizeOf(_mHeader)) ==
                            WaveNative.MmsyserrNoerror;
                return _mPlaying;
            }
        }

        public void WaitFor()
        {
            if (_mPlaying)
            {
                _mPlaying = m_PlayEvent.WaitOne();
            }
            else
            {
                Thread.Sleep(0);
            }
        }

        public void OnCompleted()
        {
            m_PlayEvent.Set();
            _mPlaying = false;
        }
    }

    public class WaveOutPlayer : IDisposable
    {
        private IntPtr _mWaveOut;
        private WaveOutBuffer _mBuffers; // linked list
        private WaveOutBuffer _mCurrentBuffer;
        private Thread _mThread;
        private BufferFillEventHandler _mFillProc;
        private bool _mFinished;
        private byte _mZero;

        private WaveNative.WaveDelegate m_BufferProc = new WaveNative.WaveDelegate(WaveOutBuffer.WaveOutProc);
        public bool Disposing;

        public static int DeviceCount => WaveNative.waveOutGetNumDevs();

        public WaveOutPlayer(int device, WaveFormat format, int bufferSize, int bufferCount,
            BufferFillEventHandler fillProc)
        {
            _mZero = format.wBitsPerSample == 8 ? (byte) 128 : (byte) 0;
            _mFillProc = fillProc;
            WaveOutHelper.Try(WaveNative.waveOutOpen(out _mWaveOut, device, format, m_BufferProc, 0,
                WaveNative.CallbackFunction));
            AllocateBuffers(bufferSize, bufferCount);
            _mThread = new Thread(ThreadProc) {IsBackground = true};
            _mThread.Start();
        }

        ~WaveOutPlayer()
        {
            Dispose();
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            Disposing = true;
            if (_mThread != null)
                try
                {
                    _mFinished = true;
                    //if (m_WaveOut != IntPtr.Zero) WaveNative.waveOutReset(m_WaveOut);
                    //m_Thread.Join();
                    _mFillProc = null;
                    //FreeBuffers();
                    //if (m_WaveOut != IntPtr.Zero) WaveNative.waveOutClose(m_WaveOut);
                }
                finally
                {
                    _mThread = null;
                    _mWaveOut = IntPtr.Zero;
                }
            GC.SuppressFinalize(this);
        }

        private void ThreadProc()
        {
            Console.WriteLine("WaveOut.Start()");
            try
            {
                while (!_mFinished && !Disposing)
                {
                    //Console.WriteLine("[1] {0}", Thread.CurrentThread.ManagedThreadId);
                    //Console.Write("a{0}", m_Finished);
                    Advance();
                    //Console.Write("b");
                    if (_mFillProc != null && !_mFinished)
                    {
                        _mFillProc(_mCurrentBuffer.Data, _mCurrentBuffer.Size);
                    }
                    else
                    {
                        // zero out buffer
                        byte v = _mZero;
                        byte[] b = new byte[_mCurrentBuffer.Size];
                        for (int i = 0; i < b.Length; i++) b[i] = v;
                        Marshal.Copy(b, 0, _mCurrentBuffer.Data, b.Length);
                    }
                    _mCurrentBuffer.Play();
                }
                //Console.WriteLine("Test!");
                //Console.Write("X");
                //WaitForAllBuffers();
                //Console.WriteLine("Test2!");
            }
            finally
            {
                Console.WriteLine("WaveOut.End()");
            }
        }

        private void AllocateBuffers(int bufferSize, int bufferCount)
        {
            FreeBuffers();
            if (bufferCount > 0)
            {
                _mBuffers = new WaveOutBuffer(_mWaveOut, bufferSize);
                var prev = _mBuffers;
                try
                {
                    for (var i = 1; i < bufferCount; i++)
                    {
                        var buf = new WaveOutBuffer(_mWaveOut, bufferSize);
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
            _mCurrentBuffer = _mCurrentBuffer == null ? _mBuffers : _mCurrentBuffer.NextBuffer;
            _mCurrentBuffer.WaitFor();
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