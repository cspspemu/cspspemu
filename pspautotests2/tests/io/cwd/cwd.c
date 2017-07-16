/*
 * PSP Software Development Kit - http://www.pspdev.org
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE in PSPSDK root for details.
 *
 * main.c - Basic sample to demonstrate some fileio functionality.
 *
 * Copyright (c) 2005 Marcus R. Brown <mrbrown@ocgnet.org>
 * Copyright (c) 2005 James Forshaw <tyranid@gmail.com>
 * Copyright (c) 2005 John Kelley <ps2dev@kelley.ca>
 * Copyright (c) 2005 Jim Paris <jim@jtan.com>
 *
 * $Id: main.c 1175 2005-10-20 15:41:33Z chip $
 */
#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <pspdebug.h>
#include <pspdisplay.h>
#include <psptypes.h>
#include <pspiofilemgr.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/unistd.h>
#include <sys/stat.h>

void try(const char *dest)
{
	char buf[MAXPATHLEN];

	printf("%16s --> ", dest);
	if(chdir(dest) < 0) {
		printf("(chdir error)\n");
	} else {
		printf("%s\n", getcwd(buf, MAXPATHLEN) ?: "(getcwd error)");
	}
}

int main(int argc, char *argv[])
{
	int n;
	char buf[MAXPATHLEN];
	
	printf("Working Directory Examples\n");
	// Don't compare, run on different dir on PSP and PPSSPP
	/*printf("Arguments: %d\n", argc);
	for (n = 0; n < argc; n++) {
		printf("Argument[%d]: '%s'\n", n, argv[n]);
	}*/
	
	mkdir("ms0:/PSP/GAME/__autotest", 0777);
	chdir("ms0:/PSP/GAME/__autotest");
	
	printf("Initial dir: %s\n\n", getcwd(buf, MAXPATHLEN) ?: "(error)");

	printf("%16s --> %s\n", "chdir() attempt", "resulting getcwd()");
	printf("%16s --> %s\n", "---------------", "------------------");
	try("");		   /* empty string                */
	try("hello");		   /* nonexistent path            */
	try("..");		   /* parent dir                  */
	try("../SAVEDATA");	   /* parent dir and subdir       */
	try("../..");		   /* multiple parents            */
	try(".");		   /* current dir                 */
	try("./././//PSP");        /* current dirs, extra slashes */
	try("/PSP/./GAME");	   /* absolute with no drive      */
	try("/");                  /* root with no drive          */
	try("ms0:/PSP/GAME");      /* absolute with drive         */
	//try("umd0:/");           /* different drive             */
	try("ms0:/PSP/../PSP/");   /* mixed                       */

	rmdir("ms0:/PSP/GAME/__autotest");

	printf("\nAll done!\n");

	return 0;
}
