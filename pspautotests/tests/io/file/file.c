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
#include <dirent.h>

const char* fileText = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

void dumpFileStat(SceIoStat d_stat, int dump_date)
{
	printf(
			"attr : %d\n",
			d_stat.st_attr
		);
	printf(
			"mode : %d\n",
			d_stat.st_mode
		);
	if(dump_date == 1)
	{
		printf(
			"creation date : %d-%d-%d-%d-%d-%d-%d\n",
			d_stat.st_ctime.year,
			d_stat.st_ctime.month,
			d_stat.st_ctime.day,
			d_stat.st_ctime.hour,
			d_stat.st_ctime.minute,
			d_stat.st_ctime.second,
			d_stat.st_ctime.microsecond
		);
		// Don't dump acces, it change every time and fail test
		/*
		printf(
			"acces date : %d-%d-%d-%d-%d-%d-%d\n",
			d_stat.st_atime.year,
			d_stat.st_atime.month,
			d_stat.st_atime.day,
			d_stat.st_atime.hour,
			d_stat.st_atime.minute,
			d_stat.st_atime.second,
			d_stat.st_atime.microsecond
		);
		*/
		printf(
			"modif date : %d-%d-%d-%d-%d-%d-%d\n",
			d_stat.st_mtime.year,
			d_stat.st_mtime.month,
			d_stat.st_mtime.day,
			d_stat.st_mtime.hour,
			d_stat.st_mtime.minute,
			d_stat.st_mtime.second,
			d_stat.st_mtime.microsecond
		);
	}
}

int main(int argc, char** argv) {

	char readBuf[128];

	sceIoMkdir("workdir", 0777);
	sceIoChdir("workdir");
	
	//sceIoGetstat+
	//sceIoClose+
	//sceIoLseek+
	//sceIoLseek32+
	//sceIoOpen+
	//sceIoRemove
	//sceIoRename
	//sceIoRmdir
	//sceIoMkdir
	//sceIoWrite+
	//sceIoRead+
	
	SceIoStat stat;
	
	int ret = sceIoGetstat("nofile",&stat);
	printf("0x%08x = sceIoGetstat(\"nofile\")\n",ret);
	
	SceUID fd = sceIoOpen("nofile", PSP_O_RDONLY, 0777);
	printf("0x%08x = sceIoOpen(\"nofile\", PSP_O_RDONLY)\n",ret);
	
	ret = sceIoClose(9999);
	printf("0x%08x = sceIoClose(9999)\n",ret);
	
	ret = sceIoGetstat("../file.c",&stat);
	dumpFileStat(stat,1);
	
	fd = sceIoOpen("testfile", PSP_O_WRONLY|PSP_O_CREAT, 0777);
	if(fd > 0)
	{
		printf("sceIoOpen(\"testfile\", PSP_O_WRONLY|PSP_O_CREAT)\n");
		
		int write_num = sceIoWrite(fd,fileText,strlen(fileText));
		printf("%d = sceIoWrite(%d)\n", write_num, strlen(fileText));
		
		ret = sceIoGetstat("testfile",&stat);
		dumpFileStat(stat,0);
		
		memset(readBuf,0,128);
		int read_num = sceIoRead(fd,&readBuf, 128);
		printf("0x%08x = sceIoRead(128)\n", read_num);
		
		ret = sceIoClose(fd);
		printf("%d = sceIoClose()\n",ret);
	}
	else
	{
		printf("0x%08x = sceIoOpen(\"testfile\", PSP_O_WRONLY|PSP_O_CREAT)\n",fd);
	}
	
	fd = sceIoOpen("testfile", PSP_O_RDONLY, 0777);
	if(fd > 0)
	{
		printf("sceIoOpen(\"testfile\", PSP_O_RDONLY)\n");
		
		memset(readBuf,0,128);
		int read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek(fd,-10, SEEK_END);
		printf("%d = sceIoLseek(fd,-10, SEEK_END)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek(fd,-10, SEEK_CUR);
		printf("%d = sceIoLseek(fd,-10, SEEK_CUR)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 5);
		printf("%d = sceIoRead(5) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek(fd,-10, SEEK_CUR);
		printf("%d = sceIoLseek(fd,-10, SEEK_CUR)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek(fd,-10, SEEK_SET);
		printf("%d = sceIoLseek(fd,-10, SEEK_SET)\n", ret);

		ret = sceIoLseek(fd,200, SEEK_SET);
		printf("%d = sceIoLseek(fd,200, SEEK_SET)\n", ret);

		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);

		ret = sceIoLseek(fd,-10, SEEK_CUR);
		printf("%d = sceIoLseek(fd,-10, SEEK_CUR)\n", ret);

		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek(fd,0, SEEK_END);
		printf("%d = sceIoLseek(fd,0, SEEK_END)\n", ret);
		
		
		ret = sceIoLseek32(fd,-10, SEEK_END);
		printf("%d = sceIoLseek32(fd,-10, SEEK_END)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek32(fd,-10, SEEK_CUR);
		printf("%d = sceIoLseek32(fd,-10, SEEK_CUR)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 5);
		printf("%d = sceIoRead(5) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek32(fd,-10, SEEK_CUR);
		printf("%d = sceIoLseek32(fd,-10, SEEK_CUR)\n", ret);
		
		memset(readBuf,0,128);
		read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
		
		ret = sceIoLseek32(fd,-10, SEEK_SET);
		printf("%d = sceIoLseek32(fd,-10, SEEK_CUR)\n", ret);
		
		
		int write_num = sceIoWrite(fd,fileText,strlen(fileText));
		printf("0x%08x = sceIoWrite(%d)\n", write_num, strlen(fileText));
		
		ret = sceIoClose(fd);
		printf("%d = sceIoClose()\n",ret);
	}
	else
	{
		printf("0x%08x = sceIoOpen(\"testfile\", PSP_O_RDONLY)\n",fd);
	}
	
	fd = sceIoOpen("testfile", PSP_O_RDONLY | PSP_O_WRONLY | PSP_O_APPEND, 0777);
	if(fd > 0)
	{
		printf("sceIoOpen(\"testfile\", PSP_O_RDONLY | PSP_O_WRONLY | PSP_O_APPEND)\n");
		
		int write_num = sceIoWrite(fd,fileText,strlen(fileText));
		printf("%d = sceIoWrite(%d)\n", write_num, strlen(fileText));
		
		ret = sceIoLseek32(fd,0, SEEK_SET);
		printf("%d = sceIoLseek32(fd,0, SEEK_SET)\n", ret);
		
		memset(readBuf,0,128);
		int read_num = sceIoRead(fd,&readBuf, 128);
		printf("%d = sceIoRead(128) = %s\n", read_num, readBuf);
	}
	else
	{
		printf("0x%08x = sceIoOpen(\"testfile\", PSP_O_RDONLY)\n",fd);
	}
	
	ret = sceIoRename("testfile", "testfile2");
	printf("%d = sceIoRename(\"testfile\", \"testfile2\")\n",ret);
	
	ret = sceIoRename("nofile", "nofile2");
	printf("0x%08x = sceIoRename(\"nofile\", \"nofile2\")\n",ret);
	
	ret = sceIoGetstat("testfile2",&stat);
	dumpFileStat(stat,0);
	
	ret = sceIoRemove("testfile2");
	printf("%d = sceIoRemove(\"testfile2\")\n",ret);
	
	ret = sceIoRemove("testfile2");
	printf("0x%08x = sceIoRemove(\"testfile2\")\n",ret);
	
	sceIoChdir("..");
	sceIoRmdir("workdir");

	
	return 0;
}
