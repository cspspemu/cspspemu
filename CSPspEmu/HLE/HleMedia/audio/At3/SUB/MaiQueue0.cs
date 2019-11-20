using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Media.audio.At3.SUB
{
    public sealed unsafe class MaiQueue0
    {
        byte[] _base;
        int _rear, _front;
        int _maxSize;

        int _status;
        //Heap_Alloc0 heap0;
        //MaiCriticalSection mcs0;

        public MaiQueue0(int queneMaxSize)
        {
            //is_ining = 0;
            //is_outing = 0;

            queneMaxSize++;
            _base = null;
            _rear = 0;
            _front = 0;
            _maxSize = 0;
            _status = 0;
            _base = new byte[queneMaxSize];
            if (_base != null)
            {
                _maxSize = queneMaxSize;
            }
            else
            {
                _status = -1;
            }
        }

        ~MaiQueue0()
        {
            Dis();
        }

        public int In(byte* head, int length)
        {
            if (_status != 0) return 0;

            //while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
            //is_ining = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                var @base = _base;
                var rear = _rear;
                var front = _front;
                var maxSize = _maxSize;

                var yoyuu = (front - rear - 1 + maxSize) % maxSize;
                var copyLength = length > yoyuu ? yoyuu : length;

                var ato = maxSize - rear;
                var copy1 = copyLength > ato ? ato : copyLength;
                var copy2 = copyLength > ato ? copyLength - ato : 0;

                if (copy1 != 0)
                {
                    //for (int n = 0; n < copy1; n++) @base[rear + n] = head[n];
                    Marshal.Copy(new IntPtr(head), @base, rear, copy1);
                    rear = (rear + copy1) % maxSize;
                    head += copy1;
                }

                if (copy2 != 0)
                {
                    //for (int n = 0; n < copy2; n++) @base[rear + n] = head[n];
                    Marshal.Copy(new IntPtr(head), @base, rear, copy2);
                    rear = (rear + copy2) % maxSize;
                    // ReSharper disable once RedundantAssignment
                    head += copy2;
                }

                _rear = rear;

                //is_ining = 0;
                return copyLength;
            }
        }

        public int Out(ManagedPointer<byte> head, int length)
        {
            if (_status != 0) return 0;

            //while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
            //is_outing = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                var @base = _base;
                var rear = _rear;
                var front = _front;
                var maxSize = _maxSize;

                var space = (rear - front + maxSize) % maxSize;
                var copyLength = length > space ? space : length;

                var ato = maxSize - front;
                var copy1 = copyLength > ato ? ato : copyLength;
                var copy2 = copyLength > ato ? copyLength - ato : 0;

                if (copy1 != 0)
                {
                    head.Memcpy(@base.GetPointer(front), copy1);
                    front = (front + copy1) % maxSize;
                    head += copy1;
                }

                if (copy2 != 0)
                {
                    head.Memcpy(@base.GetPointer(front), copy2);
                    front = (front + copy2) % maxSize;
                    // ReSharper disable once RedundantAssignment
                    head += copy2;
                }

                _front = front;

                //is_outing = 0;
                return copyLength;
            }
        }

        public int OutPre(ManagedPointer<byte> head, int length)
        {
            if (_status != 0) return 0;

            //while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
            //is_outing = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                var @base = _base;
                var rear = _rear;
                var front = _front;
                var maxSize = _maxSize;

                var space = (rear - front + maxSize) % maxSize;
                var copyLength = length > space ? space : length;

                var ato = maxSize - front;
                var copy1 = copyLength > ato ? ato : copyLength;
                var copy2 = copyLength > ato ? copyLength - ato : 0;

                if (copy1 != 0)
                {
                    head.Memcpy(@base.GetPointer(front), copy1);
                    front = (front + copy1) % maxSize;
                    head += copy1;
                }

                // ReSharper disable RedundantAssignment
                if (copy2 != 0)
                {
                    head.Memcpy(@base.GetPointer(front), copy2);
                    front = (front + copy2) % maxSize;
                    head += copy2;
                }

                //this.front = front;

                //is_outing = 0;
                return copyLength;
            }
        }

        public int GetLength()
        {
            if (_status != 0) return 0;
            var space = (_rear - _front + _maxSize) % _maxSize;
            return space;
        }

        public int GetMaxLength()
        {
            if (_status != 0) return 0;
            return _maxSize - 1;
        }

        public int Flush()
        {
            if (_status != 0) return -1;
            _front = _rear;
            return 0;
        }

        public int Dis()
        {
            if (_status != 0) return -1;
            _base = null;
            //if (heap0.free(@base)) return -1;
            return 0;
        }

        public bool IsOk()
        {
            if (_status != 0) return false;
            else return true;
        }
    }
}