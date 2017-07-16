#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define SET_TEST(title, flag, bits) { \
	int result = sceKernelSetEventFlag(flag, bits); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_FLAG(flag); \
}

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLog + schedulingLogPos, format, args);
	va_end(args);
}

static int scheduleTestFunc1(SceSize argSize, void* argPointer) {
	u32 bits = 0xDEADBEEF;
	SceUInt timeout = 5000;

	schedf("T1B");
	int result = sceKernelWaitEventFlagCB(*(int*) argPointer, 0x00FF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR, &bits, &timeout);
	schedf("T1D (result=%08x, bits=%08x) ", result, (uint) bits);

	return 0;
}

static int scheduleTestFunc2(SceSize argSize, void* argPointer) {
	int result = 0x800201A8;
	u32 bits = 0xDEADBEEF;
	SceUInt timeout;

	schedf("T2B");
	while (result == 0x800201A8) {
		timeout = 1;
		result = sceKernelWaitEventFlagCB(*(int*) argPointer, 0xFF00, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR, &bits, &timeout);
	}
	schedf("T2D (result=%08x, bits=%08x) ", result, (uint) bits);

	return 0;
}

static int scheduleTestFunc3(SceSize argSize, void* argPointer) {
	int result = 0x800201A8;
	u32 bits = 0xDEADBEEF;
	SceUInt timeout;

	schedf("T3B");
	while (result == 0x800201A8) {
		timeout = 1;
		result = sceKernelWaitEventFlagCB(*(int*) argPointer, 0xFFFF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR, &bits, &timeout);
	}
	schedf("T3D (result=%08x, bits=%08x) ", result, (uint) bits);

	return 0;
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("set", 0, 0, NULL);
	PRINT_FLAG(flag);

	SET_TEST("Basic 0x2", flag, 0x2);
	SET_TEST("Basic 0x1", flag, 0x1);
	SET_TEST("Basic 0x1", flag, 0x1);
	SET_TEST("Basic 0xF000", flag, 0xF000);
	SET_TEST("Zero", flag, 0);

	sceKernelDeleteEventFlag(flag);

	flag = sceKernelCreateEventFlag("signal", 0, 0xFFFFFFFE, NULL);
	PRINT_FLAG(flag);
	SET_TEST("All but 0x1 + 0x1", flag, 0x1);
	sceKernelDeleteEventFlag(flag);

	SET_TEST("NULL", 0, 1);
	SET_TEST("Invalid", 0xDEADBEEF, 1);
	SET_TEST("Deleted", flag, 1);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelSetEventFlag(0, 0);
	);
	BASIC_SCHED_TEST("Other + 2",
		result = sceKernelSetEventFlag(flag2, 2);
	);
	BASIC_SCHED_TEST("Other + 1",
		result = sceKernelSetEventFlag(flag2, 1);
	);
	BASIC_SCHED_TEST("Same + 2",
		result = sceKernelSetEventFlag(flag1, 2);
	);
	BASIC_SCHED_TEST("Same + 1",
		result = sceKernelSetEventFlag(flag1, 1);
	);

	BASIC_SCHED_TEST("Same + 1",
		result = sceKernelSetEventFlag(flag1, 1);
	);

	flag = sceKernelCreateEventFlag("set", 0x200, 0, NULL);
	schedulingLogPos = 0;
	schedf("A");
	SceUID thread1 = CREATE_SIMPLE_THREAD(scheduleTestFunc1);
	SceUID thread2 = CREATE_SIMPLE_THREAD(scheduleTestFunc2);
	SceUID thread3 = CREATE_SIMPLE_THREAD(scheduleTestFunc3);
	sceKernelStartThread(thread1, sizeof(flag), &flag);
	sceKernelStartThread(thread2, sizeof(flag), &flag);
	sceKernelStartThread(thread3, sizeof(flag), &flag);
	schedf("C");
	int result = sceKernelSetEventFlag(flag, 0xFFFF);
	schedf("E");
	sceKernelDeleteEventFlag(flag);
	schedf("F");

	printf("Set with multi and clear: %s(main=%08X)\n", schedulingLog, result);

	return 0;
}