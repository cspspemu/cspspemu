#include <common.h>
#include "rtc_common.h"


void checkRtcConvertLocalTimeToUTC()
{
	printf("Checking sceRtcConvertLocalTimeToUTC\n");
	u64 tick1=62135596800000000ULL;
	u64 tick2=0; 
	sceRtcConvertLocalTimeToUTC(&tick1, &tick2);
	printf("epoch : %llu\n", tick1);
	printf("epoch as UTC: %llu\n", tick2);
}

void checkRtcConvertUtcToLocalTime()
{
	printf("Checking checkRtcConvertUtcToLocalTime\n");
	u64 tick1=62135596800000000ULL;
	u64 tick2=0; 
	sceRtcConvertUtcToLocalTime(&tick1, &tick2);
	printf("epoch as UTC: %llu\n", tick1);
	printf("epoch as UTC as Local : %llu\n", tick2);
}

void checkRtcSetTime_t()
{
	printf("Checking sceRtcSetTime_t\n");

	pspTime pt;
	printf("from 0:%d\n", sceRtcSetTime_t(&pt, 0));
	DumpPSPTime("", &pt);
	printf("from epoc:%d\n",sceRtcSetTime_t(&pt, 62135596800ULL));
	DumpPSPTime("", &pt);
	printf("from epoc(64):%d\n",sceRtcSetTime64_t(&pt, 62135596800000000ULL));
	DumpPSPTime("", &pt);
	printf("from 2012, 9, 20, 7, 12, 15, 500:%d\n",sceRtcSetTime_t(&pt, 1348125135));
	DumpPSPTime("", &pt);

	printf("from epoc&0xffffffff:%d\n",sceRtcSetTime_t(&pt, 62135596800ULL&0xffffffff));
	DumpPSPTime("", &pt);
}

void checkRtcGetTime_t()
{
	printf("Checking sceRtcGetTime_t\n");
	pspTime pt;
	u64 ticks=0;
	FillPSPTime(&pt,2012,9,20,7,12,15,500);
	printf("from epoc:%d\n",sceRtcGetTime_t( &pt, &ticks));
	DumpPSPTime("", &pt);
	printf("ticks: %llu\n", ticks);
	printf("from epoc (64):%d\n",sceRtcGetTime64_t( &pt, &ticks));
	DumpPSPTime("", &pt);
	printf("ticks: %llu\n", ticks);
	sceRtcSetTime_t(&pt, ticks);
	DumpPSPTime("time_t time : ",&pt);
}

void checkRtcSetDosTime()
{
	printf("Checking sceRtcSetDosTime\n");

	pspTime pt;
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 0));
	DumpPSPTime("0 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 1));
	DumpPSPTime("1 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 10));
	DumpPSPTime("10 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 100));
	DumpPSPTime("100 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 1000));
	DumpPSPTime("1000 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 10000));
	DumpPSPTime("10000 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 100000));
	DumpPSPTime("100000 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 1000000));
	DumpPSPTime("1000000 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 10000000));
	DumpPSPTime("10000000 = ",&pt);
	printf("from epoc:%d\n",sceRtcSetDosTime(&pt, 62135596800000000ULL));
	DumpPSPTime("62135596800000000ULL = ",&pt);
}


// Getting warnings but it seems sceRtcGetDosTime is simply declared wrong
void checkRtcGetDosTime()
{
	printf("Checking sceRtcGetDosTime\n");

	pspTime pt;
	u64 ticks=0;
	FillPSPTime(&pt, 2107, 9, 11, 24, 0, 0, 0);
	printf("from epoc:%d\n",sceRtcGetDosTime( &pt, &ticks));
	DumpPSPTime("", &pt);
	printf("ticks: %llu\n", ticks);

	// false date
	pt.year = 1979;
	printf("from epoc:%d\n",sceRtcGetDosTime( &pt, &ticks));
	pt.year = 2108;
	printf("from epoc:%d\n",sceRtcGetDosTime( &pt, &ticks));

}

void checkRtcSetWin32FileTime()
{
	pspTime pt;
	u64 ft;
	int result;
	memset(&pt, 0, sizeof(pt));

	printf("Checking sceRtcSetWin32FileTime\n");
	// Crash.
	//printf("  NULL args: %08x\n", sceRtcSetWin32FileTime(NULL, NULL));
	ft = 0;
	result = sceRtcSetWin32FileTime(&pt, ft);
	printf("  Zero time (1601 January 01): (%08x)", result);
	DumpPSPTime("", &pt);
	ft = 127779156600000010ULL;
	result = sceRtcSetWin32FileTime(&pt, ft);
	printf("  Arbitrary date/time: (%08x)", result);
	DumpPSPTime("", &pt);

	// This gives weird results.  Games unlikely to call it, so let's ignore for now.
	//ft = -(365ULL * 86400ULL * 10000000ULL);
	//result = sceRtcSetWin32FileTime(&pt, ft);
	//printf("  Before 1601 January 01: (%08x)", result);
	//DumpPSPTime("", &pt);
	printf("\n");
}

void checkRtcGetWin32FileTime()
{
	pspTime pt;
	u64 ft = -1337;
	int result;

	printf("Checking sceRtcGetWin32FileTime\n");

	// It's not clear, but it seems like sceRtcGetWin32FileTime() fails to return error messages
	// properly if it hasn't processed at least one error properly.
	// Trying to emulate this isn't necessarily a bad idea, but it's hard to determine its rules...
	FillPSPTime(&pt, 1600, 1, 1, 0, 0, 0, 0);
	sceRtcGetWin32FileTime(&pt, &ft);

	// Crash.
	//printf("  NULL args: %08x\n", sceRtcGetWin32FileTime(NULL, NULL));
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, NULL);
	printf("  NULL filetime: %lld (%08x)\n", ft, result);
	memset(&pt, 0, sizeof(pt));
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, &ft);
	printf("  Zeroed time: %lld (%08x)\n", ft, result);
	FillPSPTime(&pt, 2005, 11, 31, 13, 01, 00, 1);
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, &ft);
	printf("  Arbitrary date/time: %lld (%08x)\n", ft, result);
	FillPSPTime(&pt, 1601, 1, 1, 0, 0, 0, 0);
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, &ft);
	printf("  1601 January 01: %lld (%08x)\n", ft, result);
	FillPSPTime(&pt, 1600, 1, 1, 0, 0, 0, 0);
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, &ft);
	printf("  1600 January 01: %lld (%08x)\n", ft, result);
	FillPSPTime(&pt, 0, 1, 1, 0, 0, 0, 0);
	ft = -1337;
	result = sceRtcGetWin32FileTime(&pt, &ft);
	printf("  1 January 01: %lld (%08x)\n", ft, result);
	printf("\n");
}

void checkSetTick()
{
	pspTime pt;
	u64 ticks = 835072;

	memset(&pt, 0, sizeof(pt));

	printf("checkSetTick: empty small value: %08x\n", sceRtcSetTick(&pt, &ticks));
	DumpPSPTime("",&pt);

	ticks = 62135596800000000ULL;
	memset(&pt, 0, sizeof(pt));
	printf("checkSetTick: empty rtcMagicOffset: %08x\n", sceRtcSetTick(&pt, &ticks));
	DumpPSPTime("",&pt);

	FillPSPTime(&pt,2012,9,20,7,12,15,500);
	printf("Normal: %08x\n", sceRtcGetTick(&pt, &ticks)); // if this does depend on timezone the next bit might have differnt results
	printf("checkSetTick: 2012, 09, 20, 7, 12, 15, 500: %08x\n", sceRtcSetTick(&pt, &ticks));
	DumpPSPTime("",&pt);


	FillPSPTime(&pt,2010,9,20,7,12,15,500);
	printf("preset\n");
	DumpPSPTime("",&pt);
	printf("checkSetTick: not empty:%08x\n", sceRtcSetTick(&pt, &ticks));
	DumpPSPTime("",&pt);
}

void checkGetTick() {
	pspTime pt;
	u64 ticks;

	printf("Checking sceRtcGetTick\n");

	FillPSPTime(&pt,10,1,1,0,0,0,0);
	printf("Normal: %08x\n", sceRtcGetTick(&pt, &ticks));
	printf("Ticks : %llu\n",ticks);

	FillPSPTime(&pt,9998,1,1,0,0,0,0);
	printf("Bad date: %08x\n", sceRtcGetTick(&pt, &ticks));
	printf("Ticks : %llu\n",ticks);

	FillPSPTime(&pt,0,1,1,0,0,0,0);
	printf("Min year: %08x\n", sceRtcGetTick(&pt, &ticks));
	printf("Ticks : %llu\n",ticks);

	FillPSPTime(&pt,9999,1,1,0,0,0,0);
	printf("Max year: %08x\n", sceRtcGetTick(&pt, &ticks));
	printf("Ticks : %llu\n",ticks);

	FillPSPTime(&pt,10000,1,1,0,0,0,0);
	printf("Year overflow: %08x\n", sceRtcGetTick(&pt, &ticks));
	printf("Ticks : %llu\n",ticks);
}

void checkRFC2822() {
	pspTime pt;
	char buffer[256];

	FillPSPTime(&pt,2012,1,30,2,12,15,900);
	printf("Normal:\n");
	DumpPSPTime("",&pt);
	sceRtcFormatRFC2822(&pt, buffer, sizeof(buffer));
	printf("RFC 2822: %s\n", buffer);

	FillPSPTime(&pt,2010,9,20,7,12,15,500);
	printf("Normal:\n");
	DumpPSPTime("",&pt);
	sceRtcFormatRFC2822(&pt, buffer, sizeof(buffer));
	printf("RFC 2822: %s\n", buffer);
}

void checkRFC3339() {
	pspTime pt;
	char buffer[256];

	FillPSPTime(&pt,2012,1,30,2,12,15,900);
	printf("Normal:\n");
	DumpPSPTime("",&pt);
	sceRtcFormatRFC3339(&pt, buffer, sizeof(buffer));
	printf("RFC 3339: %s\n", buffer);

	FillPSPTime(&pt,2010,9,20,7,12,15,500);
	printf("Normal:\n");
	DumpPSPTime("",&pt);
	sceRtcFormatRFC3339(&pt, buffer, sizeof(buffer));
	printf("RFC 3339: %s\n", buffer);
}

void checkRtcParseDateTime()
{
	printf("Checking sceRtcParseDateTime\n");
	printf("UNTESTED!\n");
}

int main(int argc, char **argv) {
	checkRtcConvertLocalTimeToUTC();
	checkRtcConvertUtcToLocalTime();

	checkRtcSetTime_t();
	checkRtcGetTime_t();
	checkRtcSetDosTime();
	checkRtcGetDosTime();
	checkRtcSetWin32FileTime();
	checkRtcGetWin32FileTime();

	checkGetTick();
	checkSetTick();	

	checkRFC2822();
	checkRFC3339();
	
	checkRtcParseDateTime();

	return 0;
}