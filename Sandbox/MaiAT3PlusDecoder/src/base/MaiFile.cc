

#include "SUB/MaiFile.h"


#ifdef MAI_WIN

Mai_Status MaiFile::init(Mai_WChar* src)
{
	this->src = (Mai_WChar*)heap0.alloc(sizeof(Mai_WChar) * (Mai_strlen(src) + 1));
	Mai_strcpy(this->src, src);

	file0 = INVALID_HANDLE_VALUE;

	return 0;
}

MaiFile::MaiFile(Mai_I8* src)
{
	Mai_WChar *usrc = (Mai_WChar*)heap0.alloc(sizeof(Mai_WChar) * (Mai_strlen(src) + 1));
	UTF8ToUC(usrc, src);
	init(usrc);
	heap0.free(usrc);
}

MaiFile::MaiFile(Mai_WChar* src)
{
	init(src);
}

MaiFile::~MaiFile()
{
	heap0.free(src);
}

Mai_Status MaiFile::open(Mai_I32 oflag)
{
	Mai_I32 method = 0;

	if (oflag & MaiFileORdOnly) method |= GENERIC_READ;
	if (oflag & MaiFileOWrOnly) method |= GENERIC_WRITE;

	Mai_I32 creation = 0;

	if (oflag & MaiFileOCreate) creation = OPEN_ALWAYS;
	else creation = OPEN_EXISTING;

	Mai_I32 share_mode = 0;

#ifdef WINCE
	share_mode = 0;
#else
	share_mode = 0x07;
#endif

	file0 = CreateFileW(src, method, share_mode, NULL, creation, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS, NULL);
	if (file0 == INVALID_HANDLE_VALUE) return -1;
	return 0;
}

Mai_Status MaiFile::close()
{
	if (file0 == INVALID_HANDLE_VALUE) return -1;
	if (CloseHandle(file0))
	{
		file0 = INVALID_HANDLE_VALUE;
		return 0;
	}
	else return -1;
}

Mai_Bool MaiFile::is_exist()
{
	if (GetFileAttributesW(src) == 0xFFFFFFFF) return 0;
	else return 1;
}

Mai_Status MaiFile::del()
{
	if (!is_exist()) return -1;
	if (file0 != INVALID_HANDLE_VALUE)
		if (this->close()) return -1;
	if (DeleteFileW(src)) return 0;
	else return -1;
}

Mai_Status MaiFile::moveTo(Mai_I8* dst)
{
	Mai_WChar *udst = (Mai_WChar*)heap0.alloc(sizeof(Mai_WChar) * (Mai_strlen(dst) + 1));
	UTF8ToUC(udst, dst);
	Mai_Status rs = moveTo(udst);
	heap0.free(udst);
	return rs;
}

Mai_Status MaiFile::moveTo(Mai_WChar* dst)
{
	if (!is_exist()) return -1;

	if (file0 != INVALID_HANDLE_VALUE) return -1;

#ifdef WINCE
	Mai_I32 ms0 = ::MoveFileW(src, dst);
#else
	Mai_I32 ms0 = ::MoveFileExW(src, dst, MOVEFILE_COPY_ALLOWED | MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH);
#endif
	if (ms0)
	{
		if (this->src) heap0.free(this->src);
		this->src = (Mai_WChar*)heap0.alloc(sizeof(Mai_WChar) * (Mai_strlen(dst) + 1));
		Mai_strcpy(src, dst);
	}

	if (ms0) return 0;
	return -1;
}

Mai_Status MaiFile::seek(Mai_I64 pos)
{
	if (file0 == INVALID_HANDLE_VALUE) return -1;
	Mai_I32 low = (Mai_I32)pos;
	Mai_I32 high = (Mai_I32)(((Mai_U64)pos) >> 32);
	low = SetFilePointer(file0, low, (PLONG)&high, 0);
	return 0;
}

Mai_I32 MaiFile::skipBytes(Mai_I32 n)
{
	if (file0 == INVALID_HANDLE_VALUE) return 0;
	Mai_I32 low = n;
	Mai_I32 high = 0;
	low = SetFilePointer(file0, low, (PLONG)&high, 1);
	return n;
}

Mai_I64 MaiFile::getFilePointer()
{
	if (file0 == INVALID_HANDLE_VALUE) return 0;
	Mai_I32 low = 0;
	Mai_I32 high = 0;
	Mai_I64 pos;
	low = SetFilePointer(file0, low, (PLONG)&high, 1);
	pos = (((Mai_I64)low) & 0xFFFFFFFFLL) | ((((Mai_I64)high) & 0xFFFFFFFFLL) << 32);
	return pos;
}

Mai_I64 MaiFile::getFileLen()
{
	if (file0 == INVALID_HANDLE_VALUE) return 0;
	DWORD low = 0;
	DWORD high = 0;
	Mai_I64 len;
	low = GetFileSize(file0, &high);
	len = (((Mai_I64)low) & 0xFFFFFFFFLL) | ((((Mai_I64)high) & 0xFFFFFFFFLL) << 32);
	return len;
}

Mai_Status MaiFile::setFileLen(Mai_I64 newLength)
{
	if (file0 == INVALID_HANDLE_VALUE) return -1;
	Mai_I64 fpn;
	fpn = this->getFilePointer();
	this->seek(newLength);
	if (SetEndOfFile(file0))
	{
		seek(fpn);
		return 0;
	}
	else
	{
		seek(fpn);
		return -1;
	}
}

Mai_I32 MaiFile::read(Mai_Void* b, Mai_I32 off, Mai_I32 len)
{
	if (file0 == INVALID_HANDLE_VALUE) return 0;

	Mai_I8 *bb = (Mai_I8*) b;
	Mai_I32 data_read = 0;
	ReadFile(file0, bb + off, len, (LPDWORD)&data_read, NULL);
	return data_read;
}

Mai_I32 MaiFile::write(Mai_Void* b, Mai_I32 off, Mai_I32 len)
{
	if (file0 == INVALID_HANDLE_VALUE) return 0;

	Mai_I8 *bb = (Mai_I8*) b;
	Mai_I32 data_write = 0;
	WriteFile(file0, bb + off, len, (LPDWORD)&data_write, NULL);
	return data_write;
}

#else

#include <fcntl.h>
#include <sys/stat.h>
#include <sys/syscall.h>
//#include <asm/unistd.h>

Mai_Status MaiFile::init(Mai_I8* src)
{
	this->src = (Mai_I8*)heap0.alloc(Mai_strlen(src) + 1);
	Mai_strcpy(this->src, src);

	file0 = -1;

	return 0;
}

MaiFile::MaiFile(Mai_I8* src)
{
	init(src);
}

MaiFile::MaiFile(Mai_WChar* src)
{
	Mai_I8 *asrc = (Mai_I8*)heap0.alloc(sizeof(Mai_I8) * (Mai_strlen(src) + 1) * 4);
	UCToUTF8(asrc, src);
	init(asrc);
	heap0.free(asrc);
}

MaiFile::~MaiFile()
{
	heap0.free(src);
}

Mai_Status MaiFile::open(Mai_I32 oflag)
{
	Mai_I32 method = 0;

	if ( (oflag & MaiFileORdOnly) && (!(oflag & MaiFileOWrOnly)) ) method |= O_RDONLY;
	else if ( (!(oflag & MaiFileORdOnly)) && (oflag & MaiFileOWrOnly) ) method |= O_WRONLY;
	else if ( (oflag & MaiFileORdOnly) && (oflag & MaiFileOWrOnly) ) method |= O_RDWR;

	if (oflag & MaiFileOCreate) method |= O_CREAT;

	file0 = ::open(src, method, 0777);

	if (file0 == -1) return -1;
	return 0;
}

Mai_Status MaiFile::close()
{
	if (file0 == -1) return -1;
	if (!::close(file0))
	{
		file0 = -1;
		return 0;
	}
	else return -1;
}

Mai_Bool MaiFile::is_exist()
{
	return !::access(src, 0);
}

Mai_Status MaiFile::del()
{
	if (!is_exist()) return -1;
	if (file0 != -1)
		if (this->close()) return -1;
	if (!::remove(src)) return 0;
	else return -1;
}

Mai_Status MaiFile::moveTo(Mai_I8* dst)
{
	if (!is_exist()) return -1;

	if (file0 != -1) return -1;

	Mai_Status ms0 = ::rename(src, dst);
	if (!ms0)
	{
		if (this->src) heap0.free(this->src);
		this->src = (Mai_I8*)heap0.alloc(Mai_strlen(dst) + 1);
		Mai_strcpy(src, dst);
	}

	if (!ms0) return 0;
	return -1;
}

Mai_Status MaiFile::moveTo(Mai_WChar* dst)
{
	Mai_I8 *adst = (Mai_I8*)heap0.alloc(sizeof(Mai_I8) * (Mai_strlen(dst) + 1) * 4);
	UCToUTF8(adst, dst);
	Mai_Status rs = this->moveTo(adst);
	heap0.free(adst);
	return rs;
}

Mai_Status MaiFile::seek(Mai_I64 pos)
{
	if (file0 == -1) return -1;

	if (-1 == lseek64(file0, pos, 0)) return -1;
	else return 0;
}

Mai_I32 MaiFile::skipBytes(Mai_I32 n)
{
	if (file0 == -1) return 0;

	lseek(file0, n, 1);
	return n;
}

Mai_I64 MaiFile::getFilePointer()
{
	if (file0 == -1) return 0;

	return (Mai_I64)lseek64(file0, 0, 1);
}

Mai_I64 MaiFile::getFileLen()
{
	if (file0 == -1) return 0;

	Mai_I64 len;

	struct stat64 mystat;
	stat64(src, &mystat);

	len = (Mai_I64)mystat.st_size;
	if (S_ISBLK(mystat.st_mode))
		len = (Mai_I64)(mystat.st_blocks * 512);
	
	return len;
}

Mai_Status MaiFile::setFileLen(Mai_I64 newLength)
{
	if (file0 == -1) return -1;
	Mai_I64 fpn;
	fpn = this->getFilePointer();

	//if (!::ftruncate64(file0, newLength))
	if (!::syscall(__NR_ftruncate64, file0, newLength))
	{
		this->seek(fpn);
		return 0;
	}
	else
	{
		this->seek(fpn);
		return -1;
	}
}

Mai_I32 MaiFile::read(Mai_Void* b, Mai_I32 off, Mai_I32 len)
{
	if (file0 == -1) return 0;

	Mai_I8 *bb = (Mai_I8*) b;
	Mai_I32 data_read = 0;
	data_read = (Mai_I32)::read(file0, bb + off, len); //SSIZE_MAX

	if (data_read < 0) data_read = 0;
	return data_read;
}

Mai_I32 MaiFile::write(Mai_Void* b, Mai_I32 off, Mai_I32 len)
{
	if (file0 == -1) return 0;

	Mai_I8 *bb = (Mai_I8*) b;
	Mai_I32 data_write = 0;
	data_write = (Mai_I32)::write(file0, bb + off, len); //SSIZE_MAX
	
	if (data_write < 0) data_write = 0;
	return data_write;
}

#endif
