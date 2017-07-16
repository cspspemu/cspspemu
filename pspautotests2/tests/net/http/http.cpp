// WARNING: This test doesn't actually work yet. SendRequest comes back with some DNS error on the real PSP.


#include <common.h>
#include <pspnet.h>
#include <psphttp.h>
#include <psputility_netmodules.h>

extern "C" int main(int argc, char *argv[]) {
  int tmpl, connid, reqid;

	checkpointNext("Init:");
	checkpoint("sceUtilityLoadNetModule common: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_COMMON));
	checkpoint("sceUtilityLoadNetModule adhoc: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_ADHOC));
	checkpoint("sceUtilityLoadNetModule inet: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_INET));
	checkpoint("sceUtilityLoadNetModule parsehttp: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_PARSEHTTP));
	checkpoint("sceUtilityLoadNetModule parseuri: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_PARSEURI));
	checkpoint("sceUtilityLoadNetModule http: %08x", sceUtilityLoadNetModule(PSP_NET_MODULE_HTTP));
	checkpoint("sceNetInit: %08x", sceNetInit(65536, 30, 0x1000, 30, 0x1000));
	checkpoint("sceHttpInit: %08x", sceHttpInit(20000)); 
//	checkpoint("sceHttpInitCache: %08x", sceHttpInitCache(0x1000)); 

  checkpoint("sceHttpCreateTemplate: %08x", tmpl = sceHttpCreateTemplate("pspautotests", 1, 0));  // 1, 0 = unknown but works

  checkpointNext("Connecting to ppsspp.org:");
  checkpoint("sceHttpCreateConnectionWithURL: %08x", connid = sceHttpCreateConnectionWithURL(tmpl, "http://www.ppsspp.org", 0));
  checkpoint("sceHttpCreateRequestWithURL: %08x", reqid = sceHttpCreateRequestWithURL(connid, PSP_HTTP_METHOD_GET, "/test.txt", 0));
  checkpoint("sceHttpSendRequest: %08x", sceHttpSendRequest(reqid, 0, 0));

  SceULong64 contentLength;
  checkpoint("sceHttpGetContentLength: %08x", sceHttpGetContentLength(reqid, &contentLength));
  checkpoint("response length: %08x", (int)contentLength);
  
  char *buffer = new char[contentLength + 1];
  buffer[contentLength] = 0;
  checkpoint("sceHttpReadData: %08x", reqid, buffer, contentLength);
  checkpoint("content: %s", buffer);
  delete [] buffer;

	checkpointNext("Cleanup:");
  checkpoint("sceHttpDeleteRequest: %08x", sceHttpDeleteRequest(reqid));
	checkpoint("sceHttpDeleteConnection: %08x", sceHttpDeleteConnection(connid));
	checkpoint("sceHttpDeleteTemplate: %08x", sceHttpDeleteTemplate(tmpl));
	checkpoint("sceHttpEnd: %08x", sceHttpEnd());
  checkpoint("sceNetTerm: %08x", sceNetTerm());
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_HTTP));
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_PARSEURI));
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_PARSEHTTP));
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_INET));
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_ADHOC));
	checkpoint("sceUtilityUnloadModule: %08x", sceUtilityUnloadNetModule(PSP_NET_MODULE_COMMON));
  // etc...
	return 0;
}
