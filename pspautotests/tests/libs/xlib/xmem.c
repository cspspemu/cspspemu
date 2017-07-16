/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdlib.h>

#include "xmem.h"

/* VRAM memory management, by Raphael, slightly modified */

// A MEMORY BLOCK ENTRY IS MADE UP LIKE THAT:
// bit:  31     32    30 - 15    14-0
//		free   block    prev     size
//
// bit 31: free bit, indicating if block is allocated or not
// bit 30: blocked bit, indicating if block is part of a larger block (0) - used for error resilience
// bit 30-15: block index of previous block
// bit 14- 0: size of current block
//
// This management can handle a max amount of 2^15 = 32768 blocks, which resolves to 32MB at blocksize of 1024 bytes
//
#define __BLOCK_GET_SIZE(x)    ((x & 0x7FFF))
#define __BLOCK_GET_PREV(x)    ((x >> 15) & 0x7FFF)
#define __BLOCK_GET_FREE(x)    ((x >> 31))
#define __BLOCK_GET_BLOCK(x)   ((x >> 30) & 0x1)
#define __BLOCK_SET_SIZE(x,y)  x=((x & ~0x7FFF) | ((y) & 0x7FFF))
#define __BLOCK_ADD_SIZE(x,y)  x=((x & ~0x7FFF) | (((x & 0x7FFF)+((y) & 0x7FFF)) & 0x7FFF))
#define __BLOCK_SET_PREV(x,y)  x=((x & ~0x3FFF8000) | (((y) & 0x7FFF)<<15))
#define __BLOCK_SET_FREE(x,y)  x=((x & 0x7FFFFFFF) | (((y) & 0x1)<<31))
#define __BLOCK_SET_BLOCK(x,y) x=((x & 0xBFFFFFFF) | (((y) & 0x1)<<30))
#define __BLOCK_MAKE(s,p,f,n)   (((f & 0x1)<<31) | ((n & 0x1)<<30) | (((p) & 0x7FFF)<<15) | ((s) & 0x7FFF))
#define __BLOCK_GET_FREEBLOCK(x) ((x>>30) & 0x3)		// returns 11b if block is a starting block and free, 10b if block is a starting block and allocated, 0xb if it is a non-starting block (don't change)

// Configure the memory to be managed
#define VRAM_MEM_START (X_MEM_VRAM)
#define VRAM_MEM_SIZE (X_MEM_VRAM_SIZE)

// Configure the block size the memory gets subdivided into (page size)
// __MEM_SIZE/__BLOCK_SIZE may not exceed 2^15 = 32768
// The block size also defines the alignment of allocations
// Larger block sizes perform better, because the blocktable is smaller and therefore fits better into cache
// however the overhead is also bigger and more memory is wasted
#define VRAM_BLOCK_SIZE (512)
#define VRAM_MEM_BLOCKS (VRAM_MEM_SIZE/VRAM_BLOCK_SIZE)
#define VRAM_BLOCKS(x) ((x+VRAM_BLOCK_SIZE-1)/VRAM_BLOCK_SIZE)
#define VRAM_BLOCKSIZE(x) ((x+VRAM_BLOCK_SIZE-1)&~(VRAM_BLOCK_SIZE-1))
#define VRAM_BLOCK0 ((VRAM_MEM_BLOCKS) | (1<<31) | (1<<30))

static unsigned int VRAM_mem_blocks[VRAM_MEM_BLOCKS] = { 0 };

static int VRAM_largest_update = 0;
static int VRAM_largest_block = VRAM_MEM_BLOCKS;
static int VRAM_mem_free = VRAM_MEM_BLOCKS;

static void VRAM_find_largest_block()
{
    int i = 0;
    VRAM_largest_block = 0;
    while (i<VRAM_MEM_BLOCKS)
    {
        int csize = __BLOCK_GET_SIZE(VRAM_mem_blocks[i]);
        if (__BLOCK_GET_FREEBLOCK(VRAM_mem_blocks[i])==3 && csize>VRAM_largest_block) VRAM_largest_block = csize;
        i += csize;
    }
    VRAM_largest_update = 0;
}

static void* VRAM_alloc( u32 size )
{
    // Initialize memory block, if not yet done
    if (VRAM_mem_blocks[0]==0) VRAM_mem_blocks[0] = VRAM_BLOCK0;
    
    int i = 0;
    int j = 0;
    int bsize = VRAM_BLOCKS(size);
    
    if (VRAM_largest_update==0 && VRAM_largest_block<bsize)
    {
        #ifdef VRAM_DEBUG
        X_LOG("Not enough memory to allocate %i bytes (largest: %i)!",size,vlargestblock());
        #endif
        return(0);
    }
    
    #ifdef VRAM_DEBUG
    X_LOG("allocating %i bytes, in %i blocks", size, bsize);
    #endif
    // Find smallest block that still fits the requested size
    int bestblock = -1;
    int bestblock_prev = 0;
    int bestblock_size = VRAM_MEM_BLOCKS+1;
    while (i<VRAM_MEM_BLOCKS)
    {
        int csize = __BLOCK_GET_SIZE(VRAM_mem_blocks[i]);
        if (__BLOCK_GET_FREEBLOCK(VRAM_mem_blocks[i])==3 && csize>=bsize)
        {
            if (csize<bestblock_size)
            {
                bestblock = i;
                bestblock_prev = j;
                bestblock_size = csize;
            }
            
            if (csize==bsize) break;
        }
        j = i;
        i += csize;
    }
    
    if (bestblock<0)
    {
        #ifdef VRAM_DEBUG
        X_LOG("Not enough memory to allocate %i bytes (largest: %i)!",size,vlargestblock());
        #endif
        return(0);
    }
    
    i = bestblock;
    j = bestblock_prev;
    int csize = bestblock_size;
    VRAM_mem_blocks[i] = __BLOCK_MAKE(bsize,j,0,1);
    
    int next = i+bsize;
    if (csize>bsize && next<VRAM_MEM_BLOCKS)
    {
        VRAM_mem_blocks[next] = __BLOCK_MAKE(csize-bsize,i,1,1);
        int nextnext = i+csize;
        if (nextnext<VRAM_MEM_BLOCKS)
        {
            __BLOCK_SET_PREV(VRAM_mem_blocks[nextnext], next);
        }
    }
    
    VRAM_mem_free -= bsize;
    if (VRAM_largest_block==csize)		// if we just allocated from one of the largest blocks
    {
        if ((csize-bsize)>(VRAM_mem_free/2))
        VRAM_largest_block = (csize-bsize);		// there can't be another largest block
        else
        VRAM_largest_update = 1;
    }
    return ((void*)(VRAM_MEM_START + (i*VRAM_BLOCK_SIZE)));
}

static void VRAM_free( void* ptr )
{
    if (ptr==0) return;
    
    int block = ((unsigned int)ptr - VRAM_MEM_START)/VRAM_BLOCK_SIZE;
    if (block<0 || block>VRAM_MEM_BLOCKS)
    {
        #ifdef VRAM_DEBUG
        X_LOG("Block is out of range: %i (0x%08x)", block, (int)ptr);
        #endif
        return;
    }
    int csize = __BLOCK_GET_SIZE(VRAM_mem_blocks[block]);
    #ifdef VRAM_DEBUG
    X_LOG("freeing block %i (0x%08x), size: %i", block, (int)ptr, csize);
    #endif
    
    if (__BLOCK_GET_FREEBLOCK(VRAM_mem_blocks[block])!=1 || csize==0)
    {
        #ifdef VRAM_DEBUG
        X_LOG("Block was not allocated!", 0);
        #endif
        return;
    }
    
    // Mark block as free
    __BLOCK_SET_FREE(VRAM_mem_blocks[block],1);
    VRAM_mem_free += csize;
    
    int next = block+csize;
    // Merge with previous block if possible
    int prev = __BLOCK_GET_PREV(VRAM_mem_blocks[block]);
    if (prev<block)
    {
        if (__BLOCK_GET_FREEBLOCK(VRAM_mem_blocks[prev])==3)
        {
        __BLOCK_ADD_SIZE(VRAM_mem_blocks[prev], csize);
        __BLOCK_SET_BLOCK(VRAM_mem_blocks[block],0);	// mark current block as inter block
        if (next<VRAM_MEM_BLOCKS)
        __BLOCK_SET_PREV(VRAM_mem_blocks[next], prev);
        block = prev;
        }
    }
    
    // Merge with next block if possible
    if (next<VRAM_MEM_BLOCKS)
    {
        if (__BLOCK_GET_FREEBLOCK(VRAM_mem_blocks[next])==3)
        {
            __BLOCK_ADD_SIZE(VRAM_mem_blocks[block], __BLOCK_GET_SIZE(VRAM_mem_blocks[next]));
            __BLOCK_SET_BLOCK(VRAM_mem_blocks[next],0);	// mark next block as inter block
            int nextnext = next + __BLOCK_GET_SIZE(VRAM_mem_blocks[next]);
            if (nextnext<VRAM_MEM_BLOCKS)
            __BLOCK_SET_PREV(VRAM_mem_blocks[nextnext], block);
        }
    }
    
    // Update if a new largest block emerged
    if (VRAM_largest_block<__BLOCK_GET_SIZE(VRAM_mem_blocks[block]))
    {
        VRAM_largest_block = __BLOCK_GET_SIZE(VRAM_mem_blocks[block]);
        VRAM_largest_update = 0;		// No update necessary any more, because update only necessary when largest has shrinked at most
    }
}

/*
static u32 VRAM_memavail()
{
    return VRAM_mem_free * VRAM_BLOCK_SIZE;
}
*/

static u32 VRAM_largestblock()
{
    if (VRAM_largest_update) VRAM_find_largest_block();
    return VRAM_largest_block * VRAM_BLOCK_SIZE;
}

/* Scratchpad memory management, modified vram memory manager */

// Configure the memory to be managed
#define SPAD_MEM_START (X_MEM_SCRATCH)
#define SPAD_MEM_SIZE (X_MEM_SCRATCH_SIZE)

// Configure the block size the memory gets subdivided into (page size)
// __MEM_SIZE/__BLOCK_SIZE may not exceed 2^15 = 32768
// The block size also defines the alignment of allocations
// Larger block sizes perform better, because the blocktable is smaller and therefore fits better into cache
// however the overhead is also bigger and more memory is wasted
#define SPAD_BLOCK_SIZE (128)
#define SPAD_MEM_BLOCKS (SPAD_MEM_SIZE/SPAD_BLOCK_SIZE)
#define SPAD_BLOCKS(x) ((x+SPAD_BLOCK_SIZE-1)/SPAD_BLOCK_SIZE)
#define SPAD_BLOCKSIZE(x) ((x+SPAD_BLOCK_SIZE-1)&~(SPAD_BLOCK_SIZE-1))
#define SPAD_BLOCK0 ((SPAD_MEM_BLOCKS) | (1<<31) | (1<<30))

static unsigned int SPAD_mem_blocks[SPAD_MEM_BLOCKS] = { 0 };

static int SPAD_largest_update = 0;
static int SPAD_largest_block = SPAD_MEM_BLOCKS;
static int SPAD_mem_free = SPAD_MEM_BLOCKS;

static void SPAD_find_largest_block()
{
    int i = 0;
    SPAD_largest_block = 0;
    while (i<SPAD_MEM_BLOCKS)
    {
        int csize = __BLOCK_GET_SIZE(SPAD_mem_blocks[i]);
        if (__BLOCK_GET_FREEBLOCK(SPAD_mem_blocks[i])==3 && csize>SPAD_largest_block) SPAD_largest_block = csize;
        i += csize;
    }
    SPAD_largest_update = 0;
}

static void* SPAD_alloc( u32 size )
{
    // Initialize memory block, if not yet done
    if (SPAD_mem_blocks[0]==0) SPAD_mem_blocks[0] = SPAD_BLOCK0;

    int i = 0;
    int j = 0;
    int bsize = SPAD_BLOCKS(size);

    if (SPAD_largest_update==0 && SPAD_largest_block<bsize)
    {
        #ifdef SPAD_DEBUG
        X_LOG("Not enough memory to allocate %i bytes (largest: %i)!",size,vlargestblock());
        #endif
        return(0);
    }

    #ifdef SPAD_DEBUG
    X_LOG("allocating %i bytes, in %i blocks", size, bsize);
    #endif
    // Find smallest block that still fits the requested size
    int bestblock = -1;
    int bestblock_prev = 0;
    int bestblock_size = SPAD_MEM_BLOCKS+1;
    while (i<SPAD_MEM_BLOCKS)
    {
        int csize = __BLOCK_GET_SIZE(SPAD_mem_blocks[i]);
        if (__BLOCK_GET_FREEBLOCK(SPAD_mem_blocks[i])==3 && csize>=bsize)
        {
            if (csize<bestblock_size)
            {
                bestblock = i;
                bestblock_prev = j;
                bestblock_size = csize;
            }

            if (csize==bsize) break;
        }
        j = i;
        i += csize;
    }

    if (bestblock<0)
    {
        #ifdef SPAD_DEBUG
        X_LOG("Not enough memory to allocate %i bytes (largest: %i)!",size,vlargestblock());
        #endif
        return(0);
    }

    i = bestblock;
    j = bestblock_prev;
    int csize = bestblock_size;
    SPAD_mem_blocks[i] = __BLOCK_MAKE(bsize,j,0,1);

    int next = i+bsize;
    if (csize>bsize && next<SPAD_MEM_BLOCKS)
    {
        SPAD_mem_blocks[next] = __BLOCK_MAKE(csize-bsize,i,1,1);
        int nextnext = i+csize;
        if (nextnext<SPAD_MEM_BLOCKS)
        {
            __BLOCK_SET_PREV(SPAD_mem_blocks[nextnext], next);
        }
    }

    SPAD_mem_free -= bsize;
    if (SPAD_largest_block==csize)		// if we just allocated from one of the largest blocks
    {
        if ((csize-bsize)>(SPAD_mem_free/2))
        SPAD_largest_block = (csize-bsize);		// there can't be another largest block
        else
        SPAD_largest_update = 1;
    }
    return ((void*)(SPAD_MEM_START + (i*SPAD_BLOCK_SIZE)));
}

static void SPAD_free( void* ptr )
{
    if (ptr==0) return;

    int block = ((unsigned int)ptr - SPAD_MEM_START)/SPAD_BLOCK_SIZE;
    if (block<0 || block>SPAD_MEM_BLOCKS)
    {
        #ifdef SPAD_DEBUG
        X_LOG("Block is out of range: %i (0x%08x)", block, (int)ptr);
        #endif
        return;
    }
    int csize = __BLOCK_GET_SIZE(SPAD_mem_blocks[block]);
    #ifdef SPAD_DEBUG
    X_LOG("freeing block %i (0x%08x), size: %i", block, (int)ptr, csize);
    #endif

    if (__BLOCK_GET_FREEBLOCK(SPAD_mem_blocks[block])!=1 || csize==0)
    {
        #ifdef SPAD_DEBUG
        X_LOG("Block was not allocated!", 0);
        #endif
        return;
    }

    // Mark block as free
    __BLOCK_SET_FREE(SPAD_mem_blocks[block],1);
    SPAD_mem_free += csize;

    int next = block+csize;
    // Merge with previous block if possible
    int prev = __BLOCK_GET_PREV(SPAD_mem_blocks[block]);
    if (prev<block)
    {
        if (__BLOCK_GET_FREEBLOCK(SPAD_mem_blocks[prev])==3)
        {
        __BLOCK_ADD_SIZE(SPAD_mem_blocks[prev], csize);
        __BLOCK_SET_BLOCK(SPAD_mem_blocks[block],0);	// mark current block as inter block
        if (next<SPAD_MEM_BLOCKS)
        __BLOCK_SET_PREV(SPAD_mem_blocks[next], prev);
        block = prev;
        }
    }

    // Merge with next block if possible
    if (next<SPAD_MEM_BLOCKS)
    {
        if (__BLOCK_GET_FREEBLOCK(SPAD_mem_blocks[next])==3)
        {
            __BLOCK_ADD_SIZE(SPAD_mem_blocks[block], __BLOCK_GET_SIZE(SPAD_mem_blocks[next]));
            __BLOCK_SET_BLOCK(SPAD_mem_blocks[next],0);	// mark next block as inter block
            int nextnext = next + __BLOCK_GET_SIZE(SPAD_mem_blocks[next]);
            if (nextnext<SPAD_MEM_BLOCKS)
            __BLOCK_SET_PREV(SPAD_mem_blocks[nextnext], block);
        }
    }

    // Update if a new largest block emerged
    if (SPAD_largest_block<__BLOCK_GET_SIZE(SPAD_mem_blocks[block]))
    {
        SPAD_largest_block = __BLOCK_GET_SIZE(SPAD_mem_blocks[block]);
        SPAD_largest_update = 0;		// No update necessary any more, because update only necessary when largest has shrinked at most
    }
}

/*
static u32 SPAD_memavail()
{
    return SPAD_mem_free * SPAD_BLOCK_SIZE;
}
*/

static u32 SPAD_largestblock()
{
    if (SPAD_largest_update) SPAD_find_largest_block();
    return SPAD_largest_block * SPAD_BLOCK_SIZE;
}

/* public mem functions */

void* x_malloc(u32 size)
{
    void* ptr = NULL;
	if (size > 0)
		ptr = malloc(size);
	if (ptr == NULL)
		X_LOG("Failed to allocate %u bytes to main memory.", size);
	else
		X_LOG("Successfully allocated %u bytes to main memory at 0x%08x.", size, (u32)ptr);
    return ptr;
}

void* x_valloc(u32 size)
{
    void* ptr = NULL;
	if (size > 0)
		ptr = VRAM_alloc(size);
	if (ptr == NULL)
		X_LOG("Failed to allocate %u bytes in VRAM, largest block %u bytes.", size, VRAM_largestblock());
	else
		X_LOG("Successfully allocated %u bytes to relative VRAM address 0x%08x, largest block %u bytes.", size, (u32)X_VREL(ptr), VRAM_largestblock());
    return ptr;
}

void* x_salloc(u32 size)
{
    void* ptr = NULL;
	if (size > 0)
		ptr = SPAD_alloc(size);
	if (ptr == NULL)
		X_LOG("Failed to allocate %u bytes on Scratchpad, largest block %u bytes.", size, SPAD_largestblock());
	else
		X_LOG("Successfully allocated %u bytes to Scratchpad at 0x%08x, largest block %u bytes.", size, (u32)ptr, SPAD_largestblock());
    return ptr;
}

void* x_remalloc(void* ptr, u32 size)
{
    void* new_ptr = realloc(ptr, size);
    if (new_ptr == ptr) X_LOG("Resized main memory allocation at 0x%08x to %u bytes.", (u32)ptr, size);
    else X_LOG("Moved main memory allocation at 0x%08x to 0x%08x with size %u bytes.", (u32)ptr, (u32)new_ptr, size);
    return new_ptr;
}

void x_free(void* ptr)
{
    if (ptr == NULL)
    {
        X_LOG("Error, attempting to free invalid pointer.", 0);
    }
    else if ((u32)ptr >= X_MEM_VRAM && (u32)ptr < X_MEM_VRAM + X_MEM_VRAM_SIZE)
    {
        X_LOG("Freeing allocation at relative VRAM address 0x%08x, largest block %u bytes.", (u32)X_VREL(ptr), VRAM_largestblock());
        VRAM_free(ptr);
    }
    else if ((u32)ptr >= X_MEM_SCRATCH && (u32)ptr < X_MEM_SCRATCH + X_MEM_SCRATCH_SIZE)
    {
        X_LOG("Freeing allocation from Scratchpad at 0x%08x, largest block %u bytes.", (u32)ptr, SPAD_largestblock());
        SPAD_free(ptr);
    }
    else
    {
        X_LOG("Freeing allocation from main memory at 0x%08x.", (u32)ptr);
        free(ptr);
    }
}

inline u32 x_vlargest()
{
    return VRAM_largestblock();
}

inline u32 x_slargest()
{
    return SPAD_largestblock();
}
