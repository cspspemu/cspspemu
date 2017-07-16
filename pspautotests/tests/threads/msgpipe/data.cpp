#include "shared.h"

struct ReceiveDataThread : public KernelObjectWaitThread {
	ReceiveDataThread(const char *name, SceUID uid, u32 size, int mode = 0, int prio = 0x60)
		: KernelObjectWaitThread(name, uid, 10000, prio), size_(size), mode_(mode) {
		start();
	}

	virtual int wait() {
		char msg[256];
		int received = 0x1337;
		int result = sceKernelReceiveMsgPipe(object_, msg, size_, mode_, &received, &timeout_);
		checkpoint("  ** %s got result: %08x, received = %08x = %.*s", name_, result, received, received, msg);
		return 0;
	}

	u32 size_;
	int mode_;
};

struct SendDataThread : public KernelObjectWaitThread {
	SendDataThread(const char *name, SceUID uid, const char *data, u32 size, int mode = 0, int prio = 0x60)
		: KernelObjectWaitThread(name, uid, 10000, prio), data_(data), size_(size), mode_(mode) {
		start();
	}

	virtual int wait() {
		int sent = 0x1337;
		int result = sceKernelSendMsgPipe(object_, (void *)data_, size_, mode_, &sent, &timeout_);
		checkpoint("  ** %s got result: %08x, sent = %08x", name_, result, sent);
		return 0;
	}

	const char *data_;
	u32 size_;
	int mode_;
};

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Using a buffer:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg1msg2m", 9);
		SendDataThread wait_s2("sending thread 2", msgpipe, "sg3", 3);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Without a buffer:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg1msg2m", 9);
		SendDataThread wait_s2("sending thread 2", msgpipe, "sg3", 3);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Using receive priorities:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0x1000, 4, NULL);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4, 0, 0x33);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4, 0, 0x32);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4, 0, 0x31);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg", 3);
		SendDataThread wait_s2("sending thread 2", msgpipe, "1ms", 3);
		SendDataThread wait_s3("sending thread 3", msgpipe, "g2m", 3);
		SendDataThread wait_s4("sending thread 4", msgpipe, "sg3", 3);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Using send priorities:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0x100, 4, NULL);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg", 3, 0, 0x39);
		SendDataThread wait_s2("sending thread 2", msgpipe, "1ms", 3, 0, 0x38);
		SendDataThread wait_s3("sending thread 3", msgpipe, "g2m", 3, 0, 0x37);
		SendDataThread wait_s4("sending thread 4", msgpipe, "sg3", 3, 0, 0x36);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4, 0, 0x33);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4, 0, 0x32);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4, 0, 0x31);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Using ASAP:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 4, NULL);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg", 3, 1);
		SendDataThread wait_s2("sending thread 2", msgpipe, "1ms", 3, 1);
		SendDataThread wait_s3("sending thread 3", msgpipe, "g2m", 3, 1);
		SendDataThread wait_s4("sending thread 4", msgpipe, "sg3", 3, 1);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4, 1);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4, 1);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4, 1);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Using ASAP and no buffer:");
	{
		SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
		SendDataThread wait_s1("sending thread 1", msgpipe, "msg", 3, 1);
		SendDataThread wait_s2("sending thread 2", msgpipe, "1ms", 3, 1);
		SendDataThread wait_s3("sending thread 3", msgpipe, "g2m", 3, 1);
		SendDataThread wait_s4("sending thread 4", msgpipe, "sg3", 3, 1);
		ReceiveDataThread wait_r1("receiving thread 1", msgpipe, 4, 1);
		ReceiveDataThread wait_r2("receiving thread 2", msgpipe, 4, 1);
		ReceiveDataThread wait_r3("receiving thread 3", msgpipe, 4, 1);
		schedfMsgPipe(msgpipe);
		sceKernelDelayThread(1000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	return 0;
}