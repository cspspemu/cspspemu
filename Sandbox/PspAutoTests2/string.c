#include "@common.h"
#include <math.h>

// SET PSPSDK=c:\pspsdk
// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" string.c -lpspdebug -lpspdisplay -lpspge -lpspctrl -lpspsdk -lpsplibc -lpspnet -lpspnet_inet -lpspnet_apctl -lpspnet_resolver -lpsputility -lpspuser -lpspkernel -o string.elf && psp-fixup-imports string.elf

int main(int argc, char **argv)
{
	char buffer[100];
	strcpy(buffer + 1, "/system");
	pspDebugScreenInit();
	pspDebugScreenPrintf("%s", buffer + 1);

	return 0;
}