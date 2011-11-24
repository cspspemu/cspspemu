#include "@common.h"
#include <math.h>

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" fpu.c -lpspumd -lpsppower -lpspdebug -lpspgu -lpspgum -lpspge -lpspdisplay -lpspsdk -lm -lc -lpspnet -lpspnet_inet -lpspuser -lpsprtc -lpspctrl -o fpu.elf && psp-fixup-imports fpu.elf
// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" fpu.c -lc -lpspuser -o fpu.elf && psp-fixup-imports fpu.elf

float floatValues[] = { -67123.0f, +67123.0f, -100.0f, +100.0f, 0.0f, 1.0f, -1.0f };

/*
int _checkSetLessThanSigned(int a, int b) {
	return (a < b);
}

int _checkSetLessThanUnsigned(unsigned int a, unsigned int b) {
	return (a < b);
}

void checkSetLessThan() {
	int i, j;
	for (i = 0; i < lengthof(floatValues); i++) {
		for (j = 0; j < lengthof(floatValues); j++) {
			emitFloat(_checkSetLessThanSigned(i, j));
		}
	}
}
*/

float performanceSum = 100000.0f;
int performanceCount = 100000;
float performanceCountf = 100000.0f;

char *testNames[] = { "", "Empty loop", "Simple loop", "read32", "read16", "read8", "write32", "write16", "write8",
                      "Function call no params", "Function call with params",
					  "FPU add.s", "FPU mul.s",
					  "VFPU vadd.s", "VFPU vadd.p", "VFPU vadd.t", "VFPU vadd.q", "VFPU vadd.q sequence",
					  "LWC1", "SWC1",
					  "memcpy (native)", "memset (native)", "strcpy (native)",
					  "memcpy (non-native)", "memset (non-native)", "strcpy (non-native)",
                    };
float pspDurationMillis[] = { 0, 910, 1138, 1215, 1024, 989, 1229, 962, 1007, 1066, 1365, 682, 682, 819, 819, 819, 819, 682, 1214, 1229, 866, 1072, 1361, 792, 770, 846 };

void test2() {
	char s[1024];
	int testNumber = 1;
	int startSystemTime = 0;
	int endSystemTime = 1000000; // 1 million microseconds = 1 second
	int durationMicros = endSystemTime - startSystemTime;
	int durationMillis = (durationMicros + 500) / 1000;
	float pspReference = pspDurationMillis[testNumber] / durationMillis;

	sprintf(s, "(%4.0f%%)\n", pspReference * 100);
	sprintf(s, "%-25s: %4d ms (%4.0f%%) @ %d MHz\n", testNames[testNumber], durationMillis, pspReference * 100, scePowerGetCpuClockFrequencyInt());
	emitString(s);
}

int main(int argc, char **argv) {
	int n;
	char temp[1024];
	emitFloat(floatValues[0]);
	emitFloat(floatValues[1]);
	emitFloat(floatValues[2]);
	emitFloat(floatValues[3]);
	emitFloat(floatValues[4]);
	emitFloat(floatValues[5]);
	emitFloat(floatValues[0] * 300 / 1000);
	emitFloat(11111.22222);
	sprintf(temp, "%.6f", (float)(floatValues[0] * 300 / 1000));
	emitFloat(98765.43210);
	emitString(temp);
	emitFloat(123.4567890);
	
	//pspDebugScreenPrintf("Overall performance index: %3.0f%%\n", performanceSum * 100 / performanceCount);
	sprintf(temp, "%3.0f%%", performanceSum * 100 / performanceCount);
	emitString(temp);
	emitFloat(performanceSum * 100 / performanceCount);
	emitFloat(performanceSum * 100 / performanceCountf);

	test2();

	for (n = 0; n < 100; n++) {
		//emitFloat(sinf(1.141592f));
		emitFloat(sinf(n * 0.3f));
	}
	
	return 0;
}