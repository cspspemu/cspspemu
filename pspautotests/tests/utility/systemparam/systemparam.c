#include <common.h>
#include <pspdebug.h>
#include <pspdisplay.h>
#include <stdio.h>
#include <string.h>
#include <pspmoduleinfo.h>
#include <psputility.h>

#define PSP_SYSTEMPARAM_ID_INT_BUTTON_PREFERENCE 9
#define PSP_SYSTEMPARAM_ID_INT_LOCK_PARENTAL_LEVEL 10

void printSystemParam(int result, int id, int iVal, char *sVal) {
	printf("%x (%08x) - ", id, result);
	switch(id) {
		case(PSP_SYSTEMPARAM_ID_STRING_NICKNAME):
			printf("%-17s: %s\n", "Nickname", sVal);
			break;
		case(PSP_SYSTEMPARAM_ID_INT_ADHOC_CHANNEL):
			printf("%-17s: %d\n", "AdHoc Channel", iVal);
			break;
		case(PSP_SYSTEMPARAM_ID_INT_WLAN_POWERSAVE):
			printf("%-17s: %s\n", "WLAN Powersave", iVal == 0 ? "off" : "on");
			break;
		case(PSP_SYSTEMPARAM_ID_INT_DATE_FORMAT):
			switch(iVal) {
				case(PSP_SYSTEMPARAM_DATE_FORMAT_YYYYMMDD):
					printf("%-17s: YYYYMMDD\n", "Date Format");
					break;
				case(PSP_SYSTEMPARAM_DATE_FORMAT_MMDDYYYY):
					printf("%-17s: MMDDYYYY\n", "Date Format");
					break;
				case(PSP_SYSTEMPARAM_DATE_FORMAT_DDMMYYYY):
					printf("%-17s: DDMMYYYY\n", "Date Format");
					break;
				default:
					printf("%-17s: INVALID\n", "Date Format");
			}
			break;
		case(PSP_SYSTEMPARAM_ID_INT_TIME_FORMAT):
			switch(iVal) {
				case(PSP_SYSTEMPARAM_TIME_FORMAT_24HR):
					printf("%-17s: 24HR\n", "Time Format");
					break;
				case(PSP_SYSTEMPARAM_TIME_FORMAT_12HR):
					printf("%-17s: 12HR\n", "Time Format");
					break;
				default:
					printf("%-17s: INVALID\n", "Time Format");
			}
			break;
		case(PSP_SYSTEMPARAM_ID_INT_TIMEZONE):
			printf("%-17s: %d\n", "Timezone", iVal);
			break;
		case(PSP_SYSTEMPARAM_ID_INT_DAYLIGHTSAVINGS):
			switch(iVal) {
				case(PSP_SYSTEMPARAM_DAYLIGHTSAVINGS_STD):
					printf("%-17s: standard\n", "Daylight Savings");
					break;
				case(PSP_SYSTEMPARAM_DAYLIGHTSAVINGS_SAVING):
					printf("%-17s: saving\n", "Daylight Savings");
					break;
				default:
					printf("%-17s: INVALID\n", "Daylight Savings");
			}
			break;
		case(PSP_SYSTEMPARAM_ID_INT_LANGUAGE):
			switch(iVal) {
				case(PSP_SYSTEMPARAM_LANGUAGE_JAPANESE):
					printf("%-17s: Japanese\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_ENGLISH):
					printf("%-17s: English\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_FRENCH):
					printf("%-17s: French\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_SPANISH):
					printf("%-17s: Spanish\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_GERMAN):
					printf("%-17s: German\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_ITALIAN):
					printf("%-17s: Italian\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_DUTCH):
					printf("%-17s: Dutch\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_PORTUGUESE):
					printf("%-17s: Portuguese\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_RUSSIAN):
					printf("%-17s: Russian\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_KOREAN):
					printf("%-17s: Korean\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_CHINESE_TRADITIONAL):
					printf("%-17s: Chinese (Traditional)\n", "Language");
					break;
				case(PSP_SYSTEMPARAM_LANGUAGE_CHINESE_SIMPLIFIED):
					printf("%-17s: Chinese (Simplified)\n", "Language");
					break;
				default:
					printf("%-17s: INVALID\n", "Language");
			}
			break;
		case(PSP_SYSTEMPARAM_ID_INT_BUTTON_PREFERENCE):
			printf("%-17s: %d\n", "Button Assignment", iVal);
			break;
		case(PSP_SYSTEMPARAM_ID_INT_LOCK_PARENTAL_LEVEL):
			printf("%-17s: %d\n", "Parental Level", iVal);
			break;
		default:
			printf("%-17s: int 0x%08X, string '%s'\n", "Unknown", iVal, sVal);
	}
}

int main(int argc, char **argv) {
    char sVal[256];
    int iVal;
    int i;
	int result;
	for (i = -15; i <= 256; i++) {
    	iVal = 0xDEADBEEF;
    	memset(sVal, 0, 256);
		result = sceUtilityGetSystemParamInt(i, &iVal);
    	if (result != PSP_SYSTEMPARAM_RETVAL_FAIL) {
    		printSystemParam(result, i, iVal, sVal);
		}
		result = sceUtilityGetSystemParamString(i, sVal, 256);
    	if (result != PSP_SYSTEMPARAM_RETVAL_FAIL) {
    		printSystemParam(result, i, iVal, sVal);
		}
    }
	
	printf("\nString lengths:\n");
	static const int lengths[] = {0, 1, 2, 3, 4, 5, 6, 7, 10, 256, -1};
	for (i = 0; i < sizeof(lengths) / sizeof(lengths[0]); ++i) {
		memset(sVal, -1, 256);
		result = sceUtilityGetSystemParamString(PSP_SYSTEMPARAM_ID_STRING_NICKNAME, sVal, lengths[i]);
		printf("   %d: %08x (%s)\n", lengths[i], result, sVal[0] == -1 ? "" : sVal);
	}

	return 0;
}