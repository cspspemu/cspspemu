/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <string.h>
#include "xini.h"

char buffer[256];

int xIniGetInt(FILE* f, char* param, int notfound)
{
    if (!f || !param)
        return notfound;
    rewind(f);
    while(!feof(f))
    {
        fgets(buffer, sizeof(buffer), f);
        if (strncmp(buffer, param, strlen(param)) == 0)
		{
			int r;
			sscanf(buffer, "%*s = %i", &r);
			return r;
        }
    }
    return notfound;
}

float xIniGetFloat(FILE* f, char* param, float notfound)
{
    if (!f || !param)
        return notfound;
    rewind(f);
    while(!feof(f))
    {
        fgets(buffer, sizeof(buffer), f);
        if (strncmp(buffer, param, strlen(param)) == 0)
        {
			float r;
			sscanf(buffer, "%*s = %f", &r);
			return r;
        }
    }
    return notfound;
}

void xIniGetVectorf(FILE* f, char* param, float* x, float* y, float* z)
{
    if (!f || !param)
        return;
    rewind(f);
    while(!feof(f))
    {
        fgets(buffer, sizeof(buffer), f);
        if (strncmp(buffer, param, strlen(param)) == 0)
        {
            float vals[3];
            sscanf(buffer, "%*s = %f %f %f", &vals[0], &vals[1], &vals[2]);
            if (x)
                *x = vals[0];
            if (y)
                *y = vals[1];
            if (z)
				*z = vals[2];
			return;
        }
    }
}

void xIniGetVectori(FILE* f, char* param, int* x, int* y, int* z)
{
	if (!f || !param)
		return;
	rewind(f);
	while(!feof(f))
	{
		fgets(buffer, sizeof(buffer), f);
		if (strncmp(buffer, param, strlen(param)) == 0)
		{
			int vals[3];
			sscanf(buffer, "%*s = %i %i %i", &vals[0], &vals[1], &vals[2]);
			if (x)
				*x = vals[0];
			if (y)
				*y = vals[1];
			if (z)
				*z = vals[2];
			return;
		}
	}
}

char* xIniGetString(FILE* f, char* param, char* str, char* notfound)
{
    if (!str)
        return NULL;
    if (!f || !param)
    {
        if (notfound)
            strcpy(str, notfound);
        return str;
    }
    rewind(f);
    while(!feof(f))
    {
        fgets(buffer, sizeof(buffer), f);
        if (strncmp(buffer, param, strlen(param)) == 0)
        {
            if (sscanf(buffer, "%*s = %s", str) != 1)
            {
                if (notfound)
                    strcpy(str, notfound);
                else
                    str[0] = '\0';
			}
			return str;
        }
	}
	if (notfound)
		strcpy(str, notfound);
	else
		str[0] = '\0';
    return str;
}
