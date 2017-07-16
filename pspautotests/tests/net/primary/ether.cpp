#define sceNetEtherNtostr sceNetEtherNtostr_WRONG
#define sceNetEtherStrton sceNetEtherStrton_WRONG

#include <common.h>
#include <pspnet.h>
#include <psputility_modules.h>

#undef sceNetEtherNtostr
#undef sceNetEtherStrton

extern "C" int sceNetEtherNtostr(const char *mac, char *name);
extern "C" int sceNetEtherStrton(const char *name, char *mac);

void testNtostr(const char *title, const char *mac, char *name) {
	if (name != NULL) {
		memset(name, 0, 64);
	}
	int result = sceNetEtherNtostr(mac, name);
	checkpoint("  %s: %08x, %s", title, result, name);
}

void testStrton(const char *title, const char *name, char *mac) {
	if (mac != NULL) {
		memset(mac, 0, 6);
	}
	int result = sceNetEtherStrton(name, mac);
	checkpoint("  %s: %08x, %02x:%02x:%02x:%02x:%02x:%02x", title, result, (u8)mac[0], (u8)mac[1], (u8)mac[2], (u8)mac[3], (u8)mac[4], (u8)mac[5]);
}

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Init:");
	checkpoint("sceUtilityLoadModule: %08x", sceUtilityLoadModule(PSP_MODULE_NET_COMMON));
	checkpoint("sceNetInit: %08x", sceNetInit(65536, 30, 0x1000, 30, 0x1000));

	char str[64] = {0};
	char mac[7] = {0xAB, 0xCD, 0x12, 0x34, 0x65, 0x43, 0x99};

	checkpointNext("sceNetEtherNtostr MAC values:");
	testNtostr("7 bytes", mac, str);
	mac[6] = 0;
	testNtostr("6 bytes", mac, str);
	memset(mac, 0, sizeof(mac));
	testNtostr("All zeros", mac, str);

	checkpointNext("sceNetEtherNtostr errors:");
	testNtostr("NULL dest", NULL, str);
	testNtostr("NULL source", mac, NULL);
	testNtostr("Both NULL", NULL, NULL);
	sceNetTerm();
	testNtostr("After sceNetTerm", mac, str);
	sceNetInit(65536, 30, 0x1000, 30, 0x1000);

	checkpointNext("sceNetEtherStrton strings:");
	testStrton("Normal", "AB:CD:12:34:65:43", mac);
	testStrton("Lowercase", "ab:cd:12:34:65:43", mac);
	testStrton("Too long", "AB:CD:12:34:65:43:99", mac);
	testStrton("No colons", "ABCD12346543", mac);
	testStrton("Spaces (sets of 1)", "A B C D 1 2 3 4 6 5 4 3", mac);
	testStrton("Spaces (sets of 2)", "AB CD 12 34 65 43", mac);
	testStrton("Spaces (with a zero)", "AB 0 12 34 65 43", mac);
	testStrton("Letter separators", "ABZCDZ12Z34Z65Z43", mac);
	testStrton("Letter separators (2x)", "ABZZCDZZ12ZZ34ZZ65ZZ43", mac);
	testStrton("NULL separators", "AB\0CD\012\034\065\043", mac);
	testStrton("Too short", "AB:CD:12:34", mac);
	testStrton("Empty", "", mac);

	checkpointNext("sceNetEtherStrton errors:");
	testStrton("NULL dest", NULL, mac);
	// Crashes.
	// testStrton("NULL source", str, NULL);
	// testStrton("Both NULL", NULL, NULL);
	sceNetTerm();
	testStrton("After sceNetTerm", "AB:CD:12:34:65:43", mac);
	sceNetInit(65536, 30, 0x1000, 30, 0x1000);

	checkpointNext("Cleanup:");
	checkpoint("sceNetTerm: %08x", sceNetTerm());
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadModule(PSP_MODULE_NET_COMMON));
	return 0;
}