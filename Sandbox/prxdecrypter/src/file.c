#include "file.h"

char fileSrcString[FILE_SIZE_PATH];
char fileDestString[FILE_SIZE_PATH];
char fileFilterString[32];
FileList *fileFileList;
FileFunc fileFileFunc;
FileFunc fileFolderFunc;
u8 fileRecursive = 1;
u8 fileWithCurrent = 0;
u8 fileWithParent = 0;
int fsize;

// *** FUNCTIONS ***

int fileGetSize(char *filein) {
	SceUID fd = sceIoOpen(filein, PSP_O_RDONLY, 0777);
	if (fd < 0)	return -1;
	fsize = sceIoLseek(fd, 0, SEEK_END);
	sceIoLseek(fd, 0, SEEK_SET);
	sceIoClose(fd);
	return fsize;
}

u32 fileBrowseFolder (void)
{
	SceIoDirent dirEntry;
	int dir, srclen, destlen;
	int patchFlash;

	// Init variables
	srclen = strlen(fileSrcString);
	if (srclen)
	{
		if (fileSrcString[srclen - 1] != '/')
		{
			strcat(fileSrcString,"/");
			srclen++;
		}
	}

	destlen = strlen(fileDestString);
	if (destlen)
	{
		if (fileDestString[destlen - 1] != '/')
		{
			strcat(fileDestString,"/");
			destlen++;
		}
	}

	// Patch for flash device ('.' and '..' always exists even in root)
	fileSrcString[srclen - 1] = 0;
	patchFlash = (strchr(fileSrcString,'/')) ? 1 : 0;
	fileSrcString[srclen - 1] = '/'; 

	// Open directory entry
	dir = sceIoDopen(fileSrcString);
	if (dir < 0) return 1;

	memset(&dirEntry,0,sizeof(SceIoDirent));

	// Entry loop
	while (sceIoDread(dir,&dirEntry) > 0)
	{
		// Create source path
		strcat(fileSrcString,dirEntry.d_name);

		// Create destination path
		strcat(fileDestString,dirEntry.d_name);

		// Verify type
		switch (dirEntry.d_stat.st_mode & FIO_S_IFMT)
		{
			// Folder
			case FIO_S_IFDIR :
				// If current folder
				if (!(strcmp(dirEntry.d_name,".")))
				{
					if ((patchFlash) && (fileWithCurrent)) fileFolderFunc();
						break;
				}

				// If parent folder
				if (!(strcmp(dirEntry.d_name,"..")))
				{
					if ((patchFlash) && (fileWithParent)) fileFolderFunc();
					break;
				}

				// Call the folder function
				if (fileFolderFunc)
				{
					if (fileFolderFunc())
					{
						sceIoDclose(dir);
						return 2;
					}
				}

				// Recursive for folders 
				if (fileRecursive)
				{
					if (fileBrowseFolder())
					{
						sceIoDclose(dir);
						return 1;
					}
				}
			break;

			// File
			case FIO_S_IFREG :
				// If filter, test it
				if (fileFilterString[0])
				{
					if (strlen(dirEntry.d_name) < strlen(fileFilterString)) break;
					if (strcasecmp(&dirEntry.d_name[strlen(dirEntry.d_name) - strlen(fileFilterString)],fileFilterString)) break;
				}

				// Call the file function
				if (fileFileFunc)
				{
					if (fileFileFunc())
					{
						sceIoDclose(dir);
						return 2;
					}
				}
				break;
		}

		// Restore source string
		fileSrcString[srclen] = 0;

		// Restore destination string
		fileDestString[destlen] = 0;
	}

	// Close directory entry
	sceIoDclose(dir);

	return 0;
}

u32 fileGetList_FileFunc (void)

{
 char **temp;


 // Add value in array
 if (!(fileFileList->count % 5))
 {
  // Allocate new value
  temp = (char **) realloc(fileFileList->list,sizeof(char *) * (fileFileList->count + 5));

  // Verify
  if (!(temp)) return 1;

  // Copy temp
  fileFileList->list = temp;
 }

 // Allocate new string
 fileFileList->list[fileFileList->count] = malloc(strlen(fileSrcString) + 1);
 if (!(fileFileList->list[fileFileList->count])) return 2;

 // Copy string
 strcpy(fileFileList->list[fileFileList->count],fileSrcString);

 // Update count
 fileFileList->count++;

 return 0;
}

u32 fileGetList_FolderFunc (void)
{
 // Add last '/'
 strcat(fileSrcString,"/");

 return fileGetList_FileFunc();
}

u32 fileGetList (const char *folder, const char *filter, u8 flags, FileList *list)

{
 // Verify parameters
 if ((!(folder)) || (!(list))) return 1;

 // Reset list
 memset(list,0,sizeof(FileList));

 // Copy parameters functions
 strncpy(fileSrcString,folder,FILE_SIZE_PATH - 1);
 fileSrcString[FILE_SIZE_PATH - 1] = 0;

 fileDestString[0] = 0;

 fileFileList = list;
 fileFileFunc = (flags & FILE_LIST_FILE) ? fileGetList_FileFunc : NULL;
 fileFolderFunc = (flags & FILE_LIST_FOLDER) ? fileGetList_FolderFunc : NULL;
 fileRecursive = (flags & FILE_LIST_RECURSIVE) ? 1 : 0;
 fileWithCurrent = (flags & FILE_LIST_WITHCURRENT) ? 1 : 0;
 fileWithParent = (flags & FILE_LIST_WITHPARENT) ? 1 : 0;

 if (filter)
 {
  strncpy(fileFilterString,filter,31);
  fileFilterString[31] = 0;
 }
 else
  fileFilterString[0] = 0;

 // Call recursive function
 return fileBrowseFolder();
}

u32 fileFreeList (FileList *list)
{
	int x;
	// Verify parameters
	if (!(list)) return 1;

	if (list->list)
	{
		// Delete strings
		for (x=0;x<list->count;x++)
		{
			if (list->list[x]) free(list->list[x]);
		}

		// Delete array
		free(list->list);
	}

	// Init variables
	list->count = 0;
	list->list = NULL;

	return 0;
}
