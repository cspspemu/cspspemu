#ifndef FILE_INCLUDED
#define FILE_INCLUDED

#include <pspiofilemgr.h>
#include <malloc.h>
#include <stdio.h>
#include <string.h>

#define FILE_SIZE_PATH			0x100

#define	FILE_LIST_FILE			0x1
#define	FILE_LIST_FOLDER		0x2
#define	FILE_LIST_RECURSIVE		0x4
#define	FILE_LIST_WITHCURRENT	0x8
#define	FILE_LIST_WITHPARENT	0x10

typedef u32 (*FileFunc)(void);	// File function called in recursive browse function

typedef struct FileList			// List of file name
{
 char **list;					// Array of string
 u32 count;						// Number of string in array
} FileList;

int fileGetSize (char *filein);											// Return the size of file
u32 fileGetList (const char *, const char *, u8, FileList *);				// Get the file/folder name list of a folder
u32 fileFreeList (FileList *);												// Free the result list

#endif
