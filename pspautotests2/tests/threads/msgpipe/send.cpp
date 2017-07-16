#include "shared.h"

inline void testSend(const char *title, SceUID msgpipe, void *buf, SceSize bufsize, int mode, bool doBytes = true, SceUInt timeout = 1000) {
	int bytes = 1337;
	int result = sceKernelSendMsgPipe(msgpipe, buf, bufsize, mode, doBytes ? &bytes : NULL, timeout == NO_TIMEOUT ? NULL : &timeout);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (bytes=%d, timeout=%dms) ", title, doBytes ? bytes : -1, timeout == NO_TIMEOUT ? 0 : timeout / 1000);
		schedfMsgPipe(msgpipe);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08x, bytes=%d, timeout=%dms) ", title, result, doBytes ? bytes : -1, timeout == NO_TIMEOUT ? 0 : timeout / 1000);
		schedfMsgPipe(msgpipe);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	char *temp = new char[0x1000];

	checkpointNext("Objects:");
	testSend("  Normal", msgpipe, temp, 0x100, 0);
	testSend("  NULL", 0, temp, 0x100, 0);
	testSend("  Invalid", 0xDEADBEEF, temp, 0x100, 0);
	sceKernelDeleteMsgPipe(msgpipe);
	testSend("  Deleted", msgpipe, temp, 0x100, 0);

	msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	checkpointNext("Buffers:");
	testSend("  0 length", msgpipe, temp, 0, 0);
	testSend("  NULL and 0 length", msgpipe, NULL, 0, 0);
	// Crashes.
	//testSend("  NULL with length", msgpipe, NULL, 0x100, 0);
	testSend("  Unaligned", msgpipe, temp, 1, 0);
	testSend("  Bigger than buffer", msgpipe, temp, 0x2000, 0);
	testSend("  Larger (0x10000)", msgpipe, temp, 0x10000, 0);
	testSend("  Larger (0x40000000)", msgpipe, temp, 0x40000000, 0);
	testSend("  Negative", msgpipe, temp, -1, 0);
	testSend("  Fill", msgpipe, temp, 0xFFF, 0);
	testSend("  Timeout", msgpipe, temp, 0x100, 0);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testSend("  Without bytes", msgpipe, temp, 0x100, 0, false);
	testSend("  Without timeout", msgpipe, temp, 0x100, 0, true, NO_TIMEOUT);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	checkpointNext("ASAP mode:");
	testSend("  ASAP", msgpipe, temp, 0x100, 1);
	testSend("  ASAP partial", msgpipe, temp, 0x1000, 1);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testSend("  ASAP too big", msgpipe, temp, 0x2000, 1);
	testSend("  ASAP no bytes", msgpipe, temp, 0x100, 1, false);
	testSend("  ASAP partial no bytes", msgpipe, temp, 0x1000, 1, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	checkpointNext("Other modes:");
	const static int modes[] = {-2, -1, 2, 3, 4, 5, 6, 7, 8, 9, 0x101, 0x1001, 0x10001};
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, sizeof(temp), "  Mode %d", modes[i]);
		testSend(temp, msgpipe, temp, 0x10, modes[i]);
	}

	sceKernelDeleteMsgPipe(msgpipe);
	msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
	checkpointNext("Buffers (pipe size = 0):");
	testSend("  0 length", msgpipe, temp, 0, 0);
	testSend("  NULL and 0 length", msgpipe, NULL, 0, 0);
	// Crashes.
	//testSend("  NULL with length", msgpipe, NULL, 0x100, 0);
	testSend("  Unaligned", msgpipe, temp, 1, 0);
	testSend("  Bigger than buffer", msgpipe, temp, 0x2000, 0);
	testSend("  Negative", msgpipe, temp, -1, 0);
	testSend("  Fill", msgpipe, temp, 0xFFF, 0);
	testSend("  Timeout", msgpipe, temp, 0x100, 0);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testSend("  Without bytes", msgpipe, temp, 0x100, 0, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	checkpointNext("ASAP mode (pipe size = 0):");
	testSend("  ASAP", msgpipe, temp, 0x100, 1);
	testSend("  ASAP partial", msgpipe, temp, 0x1000, 1);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testSend("  ASAP too big", msgpipe, temp, 0x2000, 1);
	testSend("  ASAP no bytes", msgpipe, temp, 0x100, 1, false);
	testSend("  ASAP partial no bytes", msgpipe, temp, 0x1000, 1, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	checkpointNext("Timeouts:");
	static const SceUInt timeouts[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 30, 50, 100, 200, 205, 210, 215, 220, 300};
	for (size_t i = 0; i < ARRAY_SIZE(timeouts); ++i) {
		char temp[32];
		snprintf(temp, sizeof(temp), "%dus", timeouts[i]);
		testSend(temp, msgpipe, temp, 0x100, 0, true, timeouts[i]);
	}
	sceKernelDeleteMsgPipe(msgpipe);

	checkpointNext("With one waiting:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeReceiveWaitThread wait_r("receiving thread", msgpipe, 10000);
		schedfMsgPipe(msgpipe);
		testSend("  With receiving thread", msgpipe, temp, 0x100, 0);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("With partial receive timeout:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeReceiveWaitThread wait_r1("receiving thread 1", msgpipe, 10000, 0x080);
		MsgPipeReceiveWaitThread wait_r2("receiving thread 2", msgpipe, 10000, 0x100);
		schedfMsgPipe(msgpipe);
		testSend("  With receiving threads", msgpipe, temp, 0x100, 0);
		sceKernelDelayThread(10000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("With interleaved send waits:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, 10000, 0x080);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000, 0x100);
		MsgPipeSendWaitThread wait_s3("sending thread 3", msgpipe, 10000, 0x080);
		SceUInt timeout = 5000;
		schedfMsgPipe(msgpipe);
		checkpoint("  Receive with three sending: %08x", sceKernelReceiveMsgPipe(msgpipe, temp, 0x100, 0, NULL, &timeout));
		sceKernelDelayThread(10000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Without a buffer:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
		MsgPipeReceiveWaitThread wait_r1("receiving thread 1", msgpipe, 10000);
		MsgPipeReceiveWaitThread wait_r2("receiving thread 2", msgpipe, 10000);
		schedfMsgPipe(msgpipe);
		testSend("  Partial packet", msgpipe, temp, 0x080, 0);
		testSend("  Complete packet", msgpipe, temp, 0x100, 0);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	delete [] temp;
	return 0;
}