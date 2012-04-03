#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "../libkirk/kirk_engine.h"

//IPL-DECRYPTER SAMPLE

typedef struct
{
    void *loadaddr;
    u32 blocksize;
    void (* entry)(void);
    u32 checksum;
    u8 data[0xF50];
} IplBlock;

void printHEX(int hex)
{
	if(hex < 0x10) printf("0%X", hex);
	else printf("%X", hex);
}

void PrintKIRK1Header(u8* buf)
{
    KIRK_CMD1_HEADER* header = (KIRK_CMD1_HEADER*)buf;
    printf("AES encrypted key:\n");
    int i;
    for(i = 0; i < 16; i++)
    {
		printHEX(header->AES_key[i]);
    }
    printf("\nCMAC encrypted key:\n");
    for(i = 0; i < 16; i++)
    {
		printHEX(header->CMAC_key[i]);
    }
    printf("\nCMAC header hash:\n");
    for(i = 0; i < 16; i++)
    {
		printHEX(header->CMAC_header_hash[i]);
    }
    printf("\nCMAC data hash:\n");
    for(i = 0; i < 16; i++)
    {
		printHEX(header->CMAC_data_hash[i]);
    }
    printf("\nmode: %d, data_size 0x%X, data_offset 0x%X\n", header->mode, header->data_size, header->data_offset);
}

int DecryptIplBlock(void *dst, const void *src)
{
#ifdef DEBUG
	PrintKIRK1Header((void*)src);
#endif
    int ret = kirk_CMD1(dst, (void*)src, 0x1000);
    if(ret == KIRK_NOT_ENABLED){ printf("KIRK not enabled!\n"); return -1;}
    else if(ret == KIRK_INVALID_MODE){ printf("Mode in header not CMD1\n"); return -1;}
    else if(ret == KIRK_HEADER_HASH_INVALID){ printf("header hash check failed\n"); return -1;}
    else if(ret == KIRK_DATA_HASH_INVALID){ printf("data hash check failed\n"); return -1;}
    else if(ret == KIRK_DATA_SIZE_ZERO){ printf("data size = 0\n"); return -1;}
#ifdef DEBUG
    else printf("Decrypt Success!\n\n");
#endif
    return 0;
}

u32 _memcpy(void *dst, const void *src, int size)
{
	int i;
	u32 checksum = 0;

	for (i=0; i<size; i+=4)
	{
		*(u32*)(dst+i) = *(u32*)(src+i);
		checksum += *(u32*)(src+i);
	}

	return(checksum);
}

#define MAX_NUM_IPLBLOCKS    (0x80)
#define MAX_IPL_SIZE         (0x80000)

u8 ipl[MAX_IPL_SIZE]; // buffer for IPL
u8 buf[0x1000];       // temp buffer for one 4KB encrypted IPL block
IplBlock decblk;      // decrypted IPL block

int main(int argc, char* argv[])
{
	if(argc < 3)
	{
		printf("Usage: ipl-decrypt input output\n");
		return 0;
	}

    int i;
    int size = 0;
    int error = 0;
    u32 checksum = 0;
	//Open the file to decrypt, get it's size
    FILE *in = fopen(argv[1], "rb");
    fseek(in, 0, SEEK_END);
    int size_enc = ftell(in);
    rewind(in);
    
    fread(ipl, MAX_IPL_SIZE, 1, in);
    
    //init KIRK crypto engine
    kirk_init(); 
    
    //decrypt all encrypted IPL blocks
    for (i=0; i<size_enc/0x1000; i++)
    {
        // load a single encrypted IPL block (4KB block)
        _memcpy(buf, ipl + i*0x1000, 0x1000);

        // decrypt the ipl block
        if (DecryptIplBlock(&decblk, buf) != 0)
        {
            printf("IPL block decryption failed! iplblk - %d \n", i);
            error = 1;
            break;
        }

        // note first block has zero as its checksum
        if (decblk.checksum != checksum)
        {
            printf("ipl block checksum failed: iplblk - %d, checksum - 0x%08X \n", i, decblk.checksum);
            error = 1;
            break;
        }

        // copy the 'data' section of the decrypted IPL block
        if (decblk.loadaddr)
        {
            checksum = _memcpy(ipl+size, decblk.data, decblk.blocksize);
            size += decblk.blocksize;
        }

        // reached the last IPL block, save it
        if (decblk.entry && !error)
        {
            FILE *out = fopen(argv[2], "wb");
            fwrite(ipl, size, 1, out);
            fclose(out);
            printf("\nIPL decrypted successfully. \n");
	        return 0;
        }
    }
    printf("Decryption failed. \n");
    
    system("PAUSE");
	return 0;
}
