#pragma once

#define sceRtcSetWin32FileTime sceRtcSetWin32FileTime_WRONG

#include <pspkernel.h>
#include <psprtc.h>

#include <limits.h>

#undef sceRtcSetWin32FileTime

int sceRtcSetWin32FileTime(pspTime *date, u64 filetime);

#include <inttypes.h>
// These are not in the pspsdk
int sceRtcSetTime64_t(pspTime* date, const uint64_t*time);
int sceRtcGetTime64_t(const pspTime* date, uint64_t*time);

// Pure guess
int sceRtcFormatRFC2822(pspTime *date, const char *ptr, int bufsize);
int sceRtcFormatRFC2822LocalTime(const char *ptr, int bufsize);
int sceRtcFormatRFC3339(pspTime *date, const char *ptr, int bufsize);
int sceRtcFormatRFC3339LocalTime(const char *ptr, int bufsize);

static void DumpPSPTime(const char* name, const pspTime* pt)
{
	printf("%s %d, %d, %d, %d, %d, %d, %d\n", name, pt->year, pt->month, pt->day, pt->hour, pt->minutes, pt->seconds, (int)pt->microseconds);
}

static void DumpTick(const char* name, u64 ticks)
{
	pspTime pt;
	printf("%s %llu\n", name, ticks);
	sceRtcSetTick(&pt, &ticks);
	DumpPSPTime("",&pt);
}

static void FillPSPTime(pspTime* pt, int year, int month, int day, int hour, int min, int sec, int micro)
{
	pt->year = year;
	pt->month = month;
	pt->day = day;
	pt->hour = hour;
	pt->minutes = min;
	pt->seconds = sec;
	pt->microseconds = micro;
}

static void checkPspTime(pspTime pt) {
	if (pt.year > 1980) {
		printf("Year: OK\n");
	} else {
		printf("Year: Failed, or great job on that time machine to %d\n", pt.year);
	}

	if (pt.month < 1 || pt.month > 12 || pt.day < 1 || pt.day > 31) {
		printf("Date: Failed %04d-%02d-%02d\n", pt.year, pt.month, pt.day);
	} else {
		printf("Date: OK\n");
	}

	if (pt.hour < 0 || pt.hour > 23 || pt.minutes < 0 || pt.minutes > 59 || pt.seconds < 0 || pt.seconds > 59) {
		printf("Time: Failed %02d:%02d:%02d\n", pt.hour, pt.minutes, pt.seconds);
	} else {
		printf("Time: OK\n");
	}

	if (pt.microseconds >= 1000000) {
		printf("Microseconds: Failed, impossibly high: %d\n", (int)pt.microseconds);
	} else {
		printf("Microseconds: OK\n");
	}
}
