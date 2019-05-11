using System;

namespace cscodec.util
{
    public class Buffer
    {
        private int cap = 0;
        private int _limit = 0;
        private int pos = 0;
        private int _mark = -1;

        public Buffer(int capacity, int limit_, int position, int mark_)
        {
            if (capacity < 0) throw new ArgumentException();
            cap = capacity;
            _limit = limit_;
            pos = position;
            if (mark_ > 0)
            {
                if (mark_ > pos)
                    throw new ArgumentException();
            } // if
            _mark = mark_;
        }

        public int capacity()
        {
            return cap;
        }

        public Buffer clear()
        {
            _limit = cap;
            pos = 0;
            _mark = -1;
            return this;
        }

        public Buffer flip()
        {
            _limit = pos;
            pos = 0;
            _mark = -1;
            return this;
        }

        public bool hasRemaining()
        {
            return _limit > pos;
        }

        public int limit()
        {
            return _limit;
        }

        public Buffer limit(int newLimit)
        {
            if ((newLimit < 0) || (newLimit > cap))
                throw new ArgumentException();
            if (newLimit <= _mark) _mark = -1;
            if (pos > newLimit) pos = newLimit - 1;
            _limit = newLimit;
            return this;
        }

        public Buffer mark()
        {
            _mark = pos;
            return this;
        }

        public int position()
        {
            return pos;
        }

        public Buffer position(int newPosition)
        {
            if ((newPosition < 0) || (newPosition > _limit))
                throw new ArgumentException();
            if (newPosition <= _mark) _mark = -1;
            pos = newPosition;
            return this;
        }

        public int remaining()
        {
            return _limit - pos;
        }

        public Buffer reset()
        {
            if (_mark == -1)
                throw new ArgumentException();
            pos = _mark;
            return this;
        }

        public Buffer rewind()
        {
            pos = 0;
            _mark = -1;
            return this;
        }
    }
}