#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct {
	int value1;
	int value2;
	int index;
} Message;

void testMsgPipeSimple1() {
	Message send_message1 = {1, 2, -1};
	Message recv_message1 = {0};
	int msgpipe_capacity = 2;
	int msgpipe_trycount = 3;
	int msgpipe;
	int result;
	int n;

	msgpipe = sceKernelCreateMsgPipe("MSGPIPE", 2, 0, (void *)(sizeof(Message) * msgpipe_capacity), /*NULL*/0);
	//msgpipe = sceKernelCreateMsgPipe("MSGPIPE", 2, 0, 1, NULL);
	schedf("CREATE:%08X\n", msgpipe > 0 ? 1 : msgpipe);
	for (n = 0; n < msgpipe_trycount; n++)
	{
		int resultSize;
		send_message1.index = n;
		result = sceKernelTrySendMsgPipe(msgpipe, &send_message1, sizeof(Message), 0, &resultSize);
		schedf("SEND[%d]:%08X, %08X, %d, %d, %d\n", n, result, resultSize, send_message1.value1, send_message1.value2, send_message1.index);
	}
	for (n = 0; n < msgpipe_trycount; n++)
	{
		memset(&recv_message1, 0, sizeof(Message));
		int resultSize;
		result = sceKernelTryReceiveMsgPipe(msgpipe, &recv_message1, sizeof(Message), 0, &resultSize);
		schedf("RECV[%d]:%08X, %08X, %d, %d, %d\n", n, result, resultSize, recv_message1.value1, recv_message1.value2, recv_message1.index);
	}
	schedf("DELETE:%08X\n", sceKernelDeleteMsgPipe(msgpipe));
	schedf("DELETE2:%08X\n", sceKernelDeleteMsgPipe(msgpipe));
}

int msgpipe;
#define msgpipe_readcount (3)
#define msgpipe_capacity (2)
#define msgpipe_threadcount (2)
#define msgpipe_writecount (msgpipe_readcount * msgpipe_threadcount)
/*
int msgpipe_readcount = 3;
int msgpipe_capacity = 1;
int msgpipe_threadcount = 2;
int msgpipe_writecount = msgpipe_readcount * msgpipe_threadcount;
*/
int current_thread_id;

void testMsgPipeWithThreads_thread(int argc, void* argv) {
	Message message;
	int message_size;
	int thread_id = current_thread_id;
	int n;
	int result;
	for (n = 0; n < msgpipe_readcount; n++) {
		result = sceKernelReceiveMsgPipe(msgpipe, &message, sizeof(Message), 0, &message_size, NULL);
		schedf("RECV_THREAD[%d][%d] : %08X, %d, %d, %d\n", thread_id, n, result, message.value1, message.value2, message.index);
	}
}

void testMsgPipeWithThreads() {
	int n;
	msgpipe = sceKernelCreateMsgPipe("MSGPIPE", 2, 0, (void *)(sizeof(Message) * msgpipe_capacity), NULL);
	{
		// Create acceptor threads, that will receive messages.
		for (n = 0; n < msgpipe_threadcount; n++) {
			current_thread_id = n;
			sceKernelStartThread(sceKernelCreateThread("MSGPIPE_thread", (void *)&testMsgPipeWithThreads_thread, 0x12, 0x10000, 0, NULL), 0, NULL);
		}
		
		// Write all the messages.
		Message message = {1, 2, -1};
		int messageSize;
		int result;
		for (n = 0; n < msgpipe_writecount; n++) {
			message.index = n;
			result = sceKernelSendMsgPipe(msgpipe, &message, sizeof(Message), 0, &messageSize, NULL);
			schedf("SEND[%d] : %08X, %d, %d, %d\n", n, result, message.value1, message.value2, message.index);
		}
	}
	sceKernelDeleteMsgPipe(msgpipe);
}

int main(int argc, char **argv) {
	testMsgPipeSimple1();
	testMsgPipeWithThreads();
	return 0;
}