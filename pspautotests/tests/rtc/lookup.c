#include <common.h>
#include "rtc_common.h"

void checkDaysInMonth() {
	printf("Checking sceRtcGetDaysInMonth\n");
	printf("sceRtcGetDaysInMonth:2010, 4\n");
	printf("%d\n", sceRtcGetDaysInMonth(2010, 4));
}

void checkDayOfWeek() {
	printf("Checking sceRtcGetDayOfWeek\n");
	printf("sceRtcGetDayOfWeek:2010, 4, 27\n");
	printf("%d\n", sceRtcGetDayOfWeek(2010, 4, 27));
	
	//A game does this: sceRtcGetDayOfWeek(166970016, 1024, 0)
	printf("sceRtcGetDayOfWeek:166970016, 1024, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(166970016, 1024, 0));
	
	// test random no valid date
	// leap year
	printf("sceRtcGetDayOfWeek:2000, 0, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2000, 0, 0));
	printf("sceRtcGetDayOfWeek:2000, 1, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2000, 1, 0));
	printf("sceRtcGetDayOfWeek:2000, 573, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2000, 573, 0));
	printf("sceRtcGetDayOfWeek:2000, 1, 2458\n");
	printf("%d\n", sceRtcGetDayOfWeek(2000, 1, 2458));
	printf("sceRtcGetDayOfWeek:2000, 4587, 2458\n");
	printf("%d\n", sceRtcGetDayOfWeek(2000, 4587, 2458));
	
	// standard year
	printf("sceRtcGetDayOfWeek:2001, 0, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2001, 0, 0));
	printf("sceRtcGetDayOfWeek:2001, 1, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2001, 1, 0));
	printf("sceRtcGetDayOfWeek:2001, 573, 0\n");
	printf("%d\n", sceRtcGetDayOfWeek(2001, 573, 0));
	printf("sceRtcGetDayOfWeek:2001, 1, 2458\n");
	printf("%d\n", sceRtcGetDayOfWeek(2001, 1, 2458));
	printf("sceRtcGetDayOfWeek:2001, 4587, 2458\n");
	printf("%d\n", sceRtcGetDayOfWeek(2001, 4587, 2458));
}

void checkIsLeapYear()
{
	printf("Checking sceRtcIsLeapYear\n");
	printf("Leap Years:\n");
	printf("1636: %08x\n", sceRtcIsLeapYear(1636));
	printf("1776: %08x\n", sceRtcIsLeapYear(1776));
	printf("1872: %08x\n", sceRtcIsLeapYear(1872));
	printf("1948: %08x\n", sceRtcIsLeapYear(1948));
	printf("2004: %08x\n", sceRtcIsLeapYear(2004));
	printf("2096: %08x\n", sceRtcIsLeapYear(2096));
	printf("Non-Leap Years:\n");
	printf("1637: %08x\n", sceRtcIsLeapYear(1637));
	printf("1777: %08x\n", sceRtcIsLeapYear(1777));
	printf("1873: %08x\n", sceRtcIsLeapYear(1873));
	printf("1949: %08x\n", sceRtcIsLeapYear(1949));
	printf("2005: %08x\n", sceRtcIsLeapYear(2005));
	printf("2097: %08x\n", sceRtcIsLeapYear(2097));
}

void checkMaxYear()
{
	printf("Checking sceRtcCheckValid for maximum year\n");

	int result, y;

	pspTime pt;
	FillPSPTime(&pt,1,1,1,0,0,0,1);
	for (y = 1; y < SHRT_MAX; y++) 
	{
		pt.year = y;
		result = sceRtcCheckValid(&pt);
		if (result != 0) 
		{
			printf("Max year: %d, struct year: %d, result: %d\n", y, pt.year, result);
			break;
		}
	}
}

void checkRtcCheckValid()
{
	printf("Checking sceRtcCheckValid\n");

	pspTime pt;
	
	FillPSPTime(&pt,2012,9,20,7,0,0,0);
	printf("Valid Date:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,-98,56,100,26,61,61,100000000);
	printf("Very invalid Date:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,-98,9,20,7,0,0,0);
	printf("invalid year:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,-9,20,7,0,0,0);
	printf("invalid month:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,9,-20,7,0,0,0);
	printf("invalid day:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,9,20,-7,0,0,0);
	printf("invalid hour:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,9,20,7,-10,10,10);
	printf("invalid minutes:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,9,20,7,10,-10,10);
	printf("invalid seconds:%d\n", sceRtcCheckValid(&pt));

	FillPSPTime(&pt,2012,9,20,7,10,10,-10);
	printf("invalid microseconds:%d\n", sceRtcCheckValid(&pt));
}


int main(int argc, char **argv) {
	checkDaysInMonth();
	checkDayOfWeek();
	checkIsLeapYear();
	checkMaxYear();
	checkRtcCheckValid();

	return 0;
}