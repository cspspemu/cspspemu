#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct {
	SceKernelMsgPacket header;
	char text[32];
} MyMessage;

SceUID mbxid;
MyMessage messageToSend;

int testSimpleMbx_Emiter(SceSize arglen, void *argp) {
	strcpy(messageToSend.text, "Hello World");
	
	//sceKernelDelayThread(1000);
	
	printf("send:'%s'\n", messageToSend.text);

	printf("sceKernelSendMbx: %08X\n", sceKernelSendMbx(mbxid, &messageToSend));

	return 0;
}

void testSimpleMbx() {
	SceUID thid;
	MyMessage* message;
	
	mbxid = sceKernelCreateMbx("MBX-1", 0, NULL);
	
	thid = sceKernelCreateThread("MBX-Thread", testSimpleMbx_Emiter, 0x18, 0x10000, 0, NULL);
	
	printf("sceKernelPollMbx: %08X\n", sceKernelPollMbx(mbxid, (void **)&message));
	
	sceKernelStartThread(thid, 0, NULL);
	//printf("sceKernelPollMbx: %08X\n", sceKernelPollMbx(mbxid, (void **)&message));
	printf("sceKernelReceiveMbx: %08X\n", sceKernelReceiveMbx(mbxid, (void **)&message, NULL));

	printf("recv:'%s'\n", message->text);
	
	sceKernelDeleteMbx(mbxid);
}

int main(int argc, char **argv) {
	testSimpleMbx();

	return 0;
}