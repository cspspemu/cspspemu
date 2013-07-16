using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle.Formats.audio.At3.SUB
{
	public class MaiQueue0
	{
		byte[] @base;
		int rear,front;
		int max_size;
		int status;
		//Heap_Alloc0 heap0;
		//MaiCriticalSection mcs0;

		public MaiQueue0(int quene_max_size)
		{
			//is_ining = 0;
			//is_outing = 0;

			quene_max_size++;
			@base = null;
			rear = 0;
			front = 0;
			max_size = 0;
			status = 0;
			@base = new byte[quene_max_size];
			if (@base != null)
			{
				max_size = quene_max_size;
			}
			else
			{
				status = -1;
			}
		}

		~MaiQueue0()
		{
			Dis();
		}

		public int In(ManagedPointer<byte> head, int length)
		{
			if (status != 0) return 0;

			//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
			//is_ining = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{

				byte[] @base = this.@base;
				int rear = this.rear;
				int front = this.front;
				int max_size = this.max_size;

				int yoyuu = (front - rear - 1 + max_size) % max_size;
				int copy_length = (length > yoyuu) ? yoyuu : length;

				int ato = max_size - rear;
				int copy1 = (copy_length > ato) ? ato : copy_length;
				int copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

				if (copy1 != 0)
				{
					@base.GetPointer(rear).Memcpy(head, copy1);
					rear = (rear + copy1) % max_size;
					head += copy1;
				}

				if (copy2 != 0)
				{
					@base.GetPointer(rear).Memcpy(head, copy2);
					rear = (rear + copy2) % max_size;
					head += copy2;
				}

				this.rear = rear;

				//is_ining = 0;
				return copy_length;
			}
		}

		public int Out(ManagedPointer<byte> head, int length)
		{
			if (status != 0) return 0;

			//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
			//is_outing = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{

				byte[] @base = this.@base;
				int rear = this.rear;
				int front = this.front;
				int max_size = this.max_size;

				int space = (rear - front + max_size) % max_size;
				int copy_length = (length > space) ? space : length;

				int ato = max_size - front;
				int copy1 = (copy_length > ato) ? ato : copy_length;
				int copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

				if (copy1 != 0)
				{
					head.Memcpy(@base.GetPointer(front), copy1);
					front = (front + copy1) % max_size;
					head += copy1;
				}

				if (copy2 != 0)
				{
					head.Memcpy(@base.GetPointer(front), copy2);
					front = (front + copy2) % max_size;
					head += copy2;
				}

				this.front = front;

				//is_outing = 0;
				return copy_length;
			}
		}

		public int OutPre(ManagedPointer<byte> head, int length)
		{
			if (status != 0) return 0;

			//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
			//is_outing = 1;
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{

				byte[] @base = this.@base;
				int rear = this.rear;
				int front = this.front;
				int max_size = this.max_size;

				int space = (rear - front + max_size) % max_size;
				int copy_length = (length > space) ? space : length;

				int ato = max_size - front;
				int copy1 = (copy_length > ato) ? ato : copy_length;
				int copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

				if (copy1 != 0)
				{
					head.Memcpy(@base.GetPointer(front), copy1);
					front = (front + copy1) % max_size;
					head += copy1;
				}

				if (copy2 != 0)
				{
					head.Memcpy(@base.GetPointer(front), copy2);
					front = (front + copy2) % max_size;
					head += copy2;
				}

				//this.front = front;

				//is_outing = 0;
				return copy_length;
			}
		}

		public int GetLength()
		{
			if (status != 0) return 0;
			int space = (rear - front + max_size) % max_size;
			return space;
		}

		public int GetMaxLength()
		{
			if (status != 0) return 0;
			return max_size - 1;
		}

		public int Flush()
		{
			if (status != 0) return -1;
			front = rear;
			return 0;
		}

		public int Dis()
		{
			if (status != 0) return -1;
			@base = null;
			//if (heap0.free(@base)) return -1;
			return 0;
		}

		public bool isOK()
		{
			if (status != 0) return false;
			else return true;
		}
	}
}
