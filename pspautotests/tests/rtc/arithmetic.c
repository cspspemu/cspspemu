#include <common.h>
#include "rtc_common.h"

void checkRtcCompareTick()
{
	printf("Checking sceRtcCompareTick\n");

	u64 tickbig1 = 62135596800000000ULL;
	u64 tickbig2 = 62135596800000000ULL;
	u64 ticksmall1 = 500;
	u64 ticksmall2 = 500;

	printf("big small :%d\n",sceRtcCompareTick(&tickbig1, &ticksmall1));
	printf("big big :%d\n",sceRtcCompareTick(&tickbig1, &tickbig2));
	printf("small big :%d\n",sceRtcCompareTick(&ticksmall1, &tickbig2));
	printf("small small :%d\n",sceRtcCompareTick(&ticksmall1, &ticksmall2));
}

void checkRtcTickAddTicks()
{
	printf("Checking sceRtcTickAddTicks\n");

	u64 sourceTick = 62135596800000445ULL;
	u64 destTick = 0;
	pspTime pt;

	printf("62135596800000445 adding -62135596800000445 ticks:%d\n", sceRtcTickAddTicks(&destTick, &sourceTick,(u64)-62135596800000445ULL));
	printf("source tick %llu\n", sourceTick);
	sceRtcSetTick(&pt, &sourceTick);
	DumpPSPTime("",&pt);

	printf("dest tick %llu\n", destTick);
	sceRtcSetTick(&pt, &destTick);
	DumpPSPTime("",&pt);

	sourceTick = 62135596800000445ULL;
	destTick = 0;

	printf("62135596800000445 adding +62135596800000445 ticks: %d\n", sceRtcTickAddTicks(&destTick, &sourceTick, sourceTick));
	sceRtcSetTick(&pt, &sourceTick);
	printf("source tick %llu\n", sourceTick);
	DumpPSPTime("",&pt);

	printf("dest tick%llu\n", destTick);
	sceRtcSetTick(&pt, &destTick);
	DumpPSPTime("",&pt);

	sourceTick = 62135596800000445ULL;
	destTick = 0;

	printf("62135596800000445 adding +621355968000 ticks: %d\n", sceRtcTickAddTicks(&destTick, &sourceTick, 621355968000));
	printf("source tick %llu\n", sourceTick);
	sceRtcSetTick(&pt, &sourceTick);
	DumpPSPTime("",&pt);

	printf("dest tick %llu\n", destTick);
	sceRtcSetTick(&pt, &destTick);
	DumpPSPTime("",&pt);

}

void checkAddDateValue(int year, int month, int day, int hour, int min, int sec, int micro, int type, long long int value_add)
{
	pspTime pt;
	u64 sourceTick;

	FillPSPTime(&pt,year, month, day, hour, min, sec, micro);
	sceRtcGetTick(&pt, &sourceTick);
	u64 destTick = 0;

	switch(type)
	{
	case 0:
		printf("%llu adding %lld years: %08x\n", sourceTick, value_add, sceRtcTickAddYears(&destTick, &sourceTick, value_add));
		break;
	case 1:
		printf("%llu adding %lld months: %08x\n", sourceTick, value_add, sceRtcTickAddMonths(&destTick, &sourceTick, value_add));
		break;
	case 2:
		printf("%llu adding %lld days: %08x\n", sourceTick, value_add, sceRtcTickAddDays(&destTick, &sourceTick, value_add));
		break;
	case 3:
		printf("%llu adding %lld hours: %08x\n", sourceTick, value_add, sceRtcTickAddHours(&destTick, &sourceTick, value_add));
		break;
	case 4:
		printf("%llu adding %lld minutes: %08x\n", sourceTick, value_add, sceRtcTickAddMinutes(&destTick, &sourceTick, value_add));
		break;
	case 5:
		printf("%llu adding %lld seconds: %08x\n", sourceTick, value_add, sceRtcTickAddSeconds(&destTick, &sourceTick, value_add));
		break;
	case 6:
		printf("%llu adding %lld microseconds: %08x\n", sourceTick, value_add, sceRtcTickAddMicroseconds(&destTick, &sourceTick, value_add));
		break;
	case 7:
		printf("%llu adding %lld weeks: %08x\n", sourceTick, value_add, sceRtcTickAddWeeks(&destTick, &sourceTick, value_add));
		break;
	default:
		break;
	}
	DumpTick("source tick", sourceTick);
	DumpTick("dest tick", destTick);
}

void checkRtcTickAddMicroseconds()
{
	printf("Checking sceRtcTickAddMicroseconds\n");

	checkAddDateValue(1970,1,1,0,0,0,445,6,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,6,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,6,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,6,62135596800000445);
	checkAddDateValue(1,1,1,0,0,0,10,6,-10);
	checkAddDateValue(1,1,1,0,0,0,10,6,-11);
	checkAddDateValue(9999,12,31,23,59,59,99999998,6,1);
	checkAddDateValue(9999,12,31,23,59,59,99999998,6,2);
}

void checkRtcTickAddSeconds()
{
	printf("Checking sceRtcTickAddSeconds\n");

	checkAddDateValue(1970,1,1,0,0,0,445,5,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,5,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,5,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,5,62135596800000445);
	checkAddDateValue(1,1,1,0,0,10,0,5,-10);
	checkAddDateValue(1,1,1,0,0,10,0,5,-11);
	checkAddDateValue(9999,12,31,23,59,50,0,5,9);
	checkAddDateValue(9999,12,31,23,59,50,0,5,10);

	int i;
	for(i = 0; i < 70; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,5,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,5,+i);
	}
}

void checkRtcTickAddMinutes()
{
	printf("Checking sceRtcTickAddMinutes\n");

	checkAddDateValue(1970,1,1,0,0,0,445,4,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,4,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,4,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,4,62135596800000445);
	checkAddDateValue(1,1,1,0,10,0,0,4,-10);
	checkAddDateValue(1,1,1,0,10,0,0,4,-11);
	checkAddDateValue(9999,12,31,23,50,0,0,4,9);
	checkAddDateValue(9999,12,31,23,50,0,0,4,10);

	int i;
	for(i = 0; i < 70; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,4,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,4,+i);
	}
}

void checkRtcTickAddHours()
{
	printf("Checking sceRtcTickAddHours\n");

	checkAddDateValue(1970,1,1,0,0,0,445,3,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,3,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,3,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,3,62135596800000445);
	checkAddDateValue(1,1,1,10,0,0,0,3,-10);
	checkAddDateValue(1,1,1,10,0,0,0,3,-11);
	checkAddDateValue(9999,12,31,20,0,0,0,3,3);
	checkAddDateValue(9999,12,31,20,0,0,0,3,4);
	
	int i;
	for(i = 0; i < 30; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,3,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,3,+i);
	}
}

void checkRtcTickAddDays()
{
	printf("Checking sceRtcTickAddDays\n");

	checkAddDateValue(1970,1,1,0,0,0,445,2,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,2,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,2,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,2,62135596800000445);
	checkAddDateValue(1,1,10,0,0,0,0,2,-9);
	checkAddDateValue(1,1,10,0,0,0,0,2,-10);
	checkAddDateValue(9999,12,20,0,0,0,0,2,11);
	checkAddDateValue(9999,12,20,0,0,0,0,2,12);
	
	int i;
	for(i = 0; i < 35; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,2,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,2,+i);
	}
}

void checkRtcTickAddWeeks()
{
	printf("Checking sceRtcTickAddWeeks\n");
	checkAddDateValue(1970,1,1,0,0,0,445,7,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,7,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,7,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,7,62135596800000445);
	checkAddDateValue(1,1,8,0,0,0,0,7,-1);
	checkAddDateValue(1,1,8,0,0,0,0,7,-2);
	checkAddDateValue(9999,12,20,0,0,0,0,7,1);
	checkAddDateValue(9999,12,20,0,0,0,0,7,2);
	
	int i;
	for(i = 0; i < 55; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,7,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,7,+i);
	}
}

void checkRtcTickAddMonths()
{
	printf("Checking sceRtcTickAddWeeks\n");
	checkAddDateValue(1970,1,1,0,0,0,445,1,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,1,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,1,-62135596800000445);
	checkAddDateValue(1970,1,1,0,0,0,445,1,62135596800000445);
	checkAddDateValue(1,2,1,0,0,0,0,1,-1);
	checkAddDateValue(1,2,1,0,0,0,0,1,-2);
	checkAddDateValue(9999,11,1,0,0,0,0,1,1);
	checkAddDateValue(9999,11,1,0,0,0,0,1,2);

	int i;
	for(i = 0; i < 15; i++)
	{
		checkAddDateValue(1970,1,1,0,0,0,445,1,-i);
		checkAddDateValue(1970,1,1,0,0,0,445,1,+i);
	}
}

void checkRtcTickAddYears()
{
	printf("Checking checkRtcTickAddYears\n");

	checkAddDateValue(1970,1,1,0,0,0,445,0,-2000);
	checkAddDateValue(1970,1,1,0,0,0,445,0,2000);
	checkAddDateValue(1970,1,1,0,0,0,445,0,-20);
	checkAddDateValue(10,1,1,0,0,0,0,0,-8);		// Check limit down
	checkAddDateValue(10,1,1,0,0,0,0,0,-9);
	checkAddDateValue(10,1,1,0,0,0,0,0,-10);
	checkAddDateValue(9998,1,1,0,0,0,0,0,1);	// Check limit up
	checkAddDateValue(9998,1,1,0,0,0,0,0,2);
}


int main(int argc, char **argv) {
	checkRtcCompareTick();
	checkRtcTickAddTicks();
	checkRtcTickAddMicroseconds();
	checkRtcTickAddSeconds();
	checkRtcTickAddMinutes();
	checkRtcTickAddHours();
	checkRtcTickAddWeeks();
	checkRtcTickAddMonths();
	checkRtcTickAddYears();
	return 0;
}