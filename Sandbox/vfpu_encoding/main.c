#include <pspdebug.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspctrl.h>
#include <pspdisplay.h>
#include <pspiofilemgr.h>

PSP_MODULE_INFO("TESTMODULE", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_USER | PSP_THREAD_ATTR_VFPU);

void _encodeInstructions() {
	__asm__ volatile (
		"vmov.q R000, C000\n"
		"vmov.q R300, C300\n"
		"vmov.q R700, C700\n"
		"vmov.q R101, C610\n"
		"vmov.q R202, C420\n"
		"vmov.q R503, C030\n"
		"vmov.s S123, S623\n"
		"vnop\n"
		"vmov.p R000, C000\n"
		"vmov.p R301, C310\n"
		"vmov.p R303, C330\n"
		"vmov.p R721, C712\n"
		"vmov.p R723, C732\n"
		"vnop\n"
		"vmov.t R100, C100\n"
		"vmov.t R213, C231\n"
		"vmov.t R302, C320\n"
	);
}

int main(int argc, char *argv[]) {
	_encodeInstructions();
	return 0;
}