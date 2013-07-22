#include "SUB/MaiQueue0.h"

MaiQueue0::MaiQueue0(Mai_I32 quene_max_size)
{
	//is_ining = 0;
	//is_outing = 0;

	quene_max_size++;
	base = NULL;
	rear = 0;
	front = 0;
	max_size = 0;
	status = 0;
	base = (Mai_I8*)heap0.alloc(quene_max_size);
	if (base)
	{
		max_size = quene_max_size;
	}
	else
	{
		status = -1;
	}
}

MaiQueue0::~MaiQueue0()
{
	Dis();
}

Mai_I32 MaiQueue0::In(Mai_I8 *head, Mai_I32 length)
{
	if (status) return 0;

	//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
	//is_ining = 1;
	mcs0.enter();

	Mai_I8 *base = this->base;
	Mai_I32 rear = this->rear;
	Mai_I32 front = this->front;
	Mai_I32 max_size = this->max_size;

	Mai_I32 yoyuu = (front - rear - 1 + max_size) % max_size;
	Mai_I32 copy_length = (length > yoyuu) ? yoyuu : length;

	Mai_I32 ato = max_size - rear;
	Mai_I32 copy1 = (copy_length > ato) ? ato : copy_length;
	Mai_I32 copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

	if (copy1)
	{
		Mai_memcpy(&base[rear], head, copy1);
		rear = (rear + copy1) % max_size;
		head += copy1;
	}

	if (copy2)
	{
		Mai_memcpy(&base[rear], head, copy2);
		rear = (rear + copy2) % max_size;
		head += copy2;
	}

	this->rear = rear;

	//is_ining = 0;
	mcs0.leave();
	return copy_length;
}

Mai_I32 MaiQueue0::Out(Mai_I8 *head, Mai_I32 length)
{
	if (status) return 0;

	//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
	//is_outing = 1;
	mcs0.enter();

	Mai_I8 *base = this->base;
	Mai_I32 rear = this->rear;
	Mai_I32 front = this->front;
	Mai_I32 max_size = this->max_size;

	Mai_I32 space = (rear - front + max_size) % max_size;
	Mai_I32 copy_length = (length > space) ? space : length;

	Mai_I32 ato = max_size - front;
	Mai_I32 copy1 = (copy_length > ato) ? ato : copy_length;
	Mai_I32 copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

	if (copy1)
	{
		Mai_memcpy(head, &base[front], copy1);
		front = (front + copy1) % max_size;
		head += copy1;
	}

	if (copy2)
	{
		Mai_memcpy(head, &base[front], copy2);
		front = (front + copy2) % max_size;
		head += copy2;
	}

	this->front = front;

	//is_outing = 0;
	mcs0.leave();
	return copy_length;
}

Mai_I32 MaiQueue0::OutPre(Mai_I8 *head, Mai_I32 length)
{
	if (status) return 0;

	//while ( (is_ining) || (is_outing) ) Mai_Sleep(1);
	//is_outing = 1;
	mcs0.enter();

	Mai_I8 *base = this->base;
	Mai_I32 rear = this->rear;
	Mai_I32 front = this->front;
	Mai_I32 max_size = this->max_size;

	Mai_I32 space = (rear - front + max_size) % max_size;
	Mai_I32 copy_length = (length > space) ? space : length;

	Mai_I32 ato = max_size - front;
	Mai_I32 copy1 = (copy_length > ato) ? ato : copy_length;
	Mai_I32 copy2 = (copy_length > ato) ? (copy_length - ato) : 0;

	if (copy1)
	{
		Mai_memcpy(head, &base[front], copy1);
		front = (front + copy1) % max_size;
		head += copy1;
	}

	if (copy2)
	{
		Mai_memcpy(head, &base[front], copy2);
		front = (front + copy2) % max_size;
		head += copy2;
	}

	//this->front = front;

	//is_outing = 0;
	mcs0.leave();
	return copy_length;
}

Mai_I32 MaiQueue0::GetLength()
{
	if (status) return 0;
	Mai_I32 space = (rear - front + max_size) % max_size;
	return space;
}

Mai_I32 MaiQueue0::GetMaxLength()
{
	if (status) return 0;
	return max_size - 1;
}

Mai_Status MaiQueue0::Flush()
{
	if (status) return -1;
	front = rear;
	return 0;
}

Mai_Status MaiQueue0::Dis()
{
	if (status) return -1;
	if (heap0.free(base)) return -1;
	return 0;
}

Mai_Bool MaiQueue0::isOK()
{
	if (status) return 0;
	else return 1;
}