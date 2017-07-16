/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <string.h>
#include <pspgu.h>
#include <pspgum.h>
#include "xgraphics.h"
#include "xmath.h"
#include "xmem.h"

#include "xtexture.h"

static void swizzle_fast(void* out, const void* in, u32 width, u32 height)
{
    unsigned int blockx, blocky;
    unsigned int j;

    unsigned int width_blocks = (width / 16);
    unsigned int height_blocks = (height / 8);

    unsigned int src_pitch = (width-16)/4;
    unsigned int src_row = width * 8;

    const u8* ysrc = (u8*)in;
    u32* dst = (u32*)out;

    for (blocky = 0; blocky < height_blocks; ++blocky)
    {
        const u8* xsrc = ysrc;
        for (blockx = 0; blockx < width_blocks; ++blockx)
        {
            const u32* src = (u32*)xsrc;
            for (j = 0; j < 8; ++j)
            {
                *(dst++) = *(src++);
                *(dst++) = *(src++);
                *(dst++) = *(src++);
                *(dst++) = *(src++);
                src += src_pitch;
            }
            xsrc += 16;
        }
        ysrc += src_row;
    }
}

static int tex_size(int psm, int width, int height)
{
    int bpp = 0;
    switch(psm)
    {
        case GU_PSM_T4:
            bpp = 4;
            break;
        case GU_PSM_T8:
            bpp = 8;
            break;
        case GU_PSM_5650:
        case GU_PSM_5551:
        case GU_PSM_4444:
        case GU_PSM_T16:
            bpp = 16;
            break;
        case GU_PSM_8888:
        case GU_PSM_T32:
            bpp = 32;
            break;
        default:
            bpp = 0;
            break;
    }
    return (bpp*width*height)/8;
}

static xTexture* tex_initialize()
{
    xTexture* t = x_malloc(sizeof(xTexture));
    if (!t)
        return 0;
    t->cpsm = 0;
    t->psm = 0;
    t->clut_entries = 0;
    t->swizzled = 0;
    t->width = 0;
    t->height = 0;
    t->buf_width = 0;
    t->pow2_height = 0;
    t->u_scale = 1.0f;
    t->v_scale = 1.0f;
    t->clut = 0;
    t->data = 0;
    t->num_mips = 0;
    t->mipmaps = 0;
    return t;
}

#define COL_5650_R(c) (int)(((c)>>0) & 0x1f)
#define COL_5650_G(c) (int)(((c)>>5) & 0x3f)
#define COL_5650_B(c) (int)(((c)>>11) & 0x1f)

#define COL_5551_R(c) (int)(((c)>>0) & 0x1f)
#define COL_5551_G(c) (int)(((c)>>5) & 0x1f)
#define COL_5551_B(c) (int)(((c)>>10) & 0x1f)

#define COL_4444_R(c) (int)(((c)>>0) & 0xf)
#define COL_4444_G(c) (int)(((c)>>4) & 0xf)
#define COL_4444_B(c) (int)(((c)>>8) & 0xf)

#define COL_8888_R(c) (int)(((c)>>0) & 0xff)
#define COL_8888_G(c) (int)(((c)>>8) & 0xff)
#define COL_8888_B(c) (int)(((c)>>16) & 0xff)

static int tex_nearest_index(xTexture* t, u32 color)
{
    int min_index = 0;
    int min_dist = 3*SQR(255);
    int dist = 0;
    int r, g, b;
    int i;
    if (t->cpsm == GU_PSM_8888)
    {
        u32* clut = t->clut;
        for (i = 0; i < t->clut_entries; i++)
        {
            r = COL_8888_R(color) - COL_8888_R(clut[i]);
            g = COL_8888_G(color) - COL_8888_G(clut[i]);
            b = COL_8888_B(color) - COL_8888_B(clut[i]);
            dist = SQR(r) + SQR(g) + SQR(b);
            if (dist < min_dist)
            {
                min_dist = dist;
                min_index = i;
            }
        }
    }
    else
    {
        u16* clut = t->clut;
        for (i = 0; i < t->clut_entries; i++)
        {
            if (t->cpsm == GU_PSM_5650)
            {
                r = COL_5650_R(color) - COL_5650_R(clut[i]);
                g = COL_5650_G(color) - COL_5650_G(clut[i]);
                b = COL_5650_B(color) - COL_5650_B(clut[i]);
            }
            else if (t->cpsm == GU_PSM_5551)
            {
                r = COL_5551_R(color) - COL_5551_R(clut[i]);
                g = COL_5551_G(color) - COL_5551_G(clut[i]);
                b = COL_5551_B(color) - COL_5551_B(clut[i]);
            }
            else if (t->cpsm == GU_PSM_4444)
            {
                r = COL_4444_R(color) - COL_4444_R(clut[i]);
                g = COL_4444_G(color) - COL_4444_G(clut[i]);
                b = COL_4444_B(color) - COL_4444_B(clut[i]);
            }
            else
                return 0;
            dist = SQR(r) + SQR(g) + SQR(b);
            if (dist < min_dist)
            {
                min_dist = dist;
                min_index = i;
            }
        }
    }
    return min_index;
}

static void tex_generate_mipmaps(xTexture* t, int num)
{
    if (!t)
        return;
    //do not make mipmaps if trying to make more than 9 or texture is larger than 512x512
    if (!t->data || t->mipmaps || num <= 0 || num > 9 || t->width > 512 || t->height > 512)
        return;
    xGuSaveStates();
    sceGuEnable(GU_TEXTURE_2D);
    sceGuDisable(GU_BLEND);
    sceGuDisable(GU_DEPTH_TEST);
    sceGuDisable(GU_CULL_FACE);
	sceGuDisable(GU_DITHER);
	sceGuDepthMask(GU_TRUE);
    xGuTexFilter(X_BILINEAR);
    xGuTexMode(X_TFX_REPLACE, 0);
    xGuRenderToTarget((t->clut ? t->cpsm : t->psm), 256, 256, 256, xGuDrawPtr(0, 0));
    t->num_mips = 0;
    t->mipmaps = (x_mipmap*)x_malloc(num*sizeof(x_mipmap));
    if (!t->mipmaps) return;
    int mip_width = t->width;
    int mip_height = t->height;
    int i;
    for (i = 0; i < num; i++)
    {
        mip_width >>= 1;
        mip_height >>= 1;
        t->mipmaps[i].width = mip_width;
        t->mipmaps[i].height = mip_height;
        t->mipmaps[i].buf_width = x_next_pow2(mip_width);
        t->mipmaps[i].pow2_height = x_next_pow2(mip_height);
        t->mipmaps[i].buf_height = x_num_align(mip_height, 8);
        t->mipmaps[i].data = x_malloc(tex_size(t->psm, t->mipmaps[i].buf_width, t->mipmaps[i].buf_height));
        if (t->mipmaps[i].data)
        {
            if (t->clut)
            {
                sceGuClutMode(t->cpsm, 0, 0xff, 0);
                sceGuClutLoad(t->clut_entries>>3, t->clut);
            }
            sceGuTexMode(t->psm, 0, 0, 0);
            if (i == 0) sceGuTexImage(0, t->buf_width, t->pow2_height, t->buf_width, t->data);
            else        sceGuTexImage(0, t->mipmaps[i-1].buf_width, t->mipmaps[i-1].pow2_height, t->mipmaps[i-1].buf_width, t->mipmaps[i-1].data);
            xGuClear(0xffffffff);
            TVertex2D* vertices = (TVertex2D*)sceGuGetMemory(2*sizeof(TVertex2D));
            vertices[0].u = 0;
            vertices[0].v = 0;
            vertices[0].x = 0;
            vertices[0].y = 0;
            vertices[0].z = 0;
            vertices[1].u = (i == 0 ? t->width : t->mipmaps[i-1].width);
            vertices[1].v = (i == 0 ? t->height : t->mipmaps[i-1].height);
            vertices[1].x = mip_width;
            vertices[1].y = mip_height;
            vertices[1].z = 0;
            sceGuDrawArray(GU_SPRITES, TVertex2D_vtype|GU_TRANSFORM_2D, 2, 0, vertices);
            sceGuSync(0, 2);
            if (t->clut)
            {
                u16* src16 = xGuDrawPtr(1, 1);
                u32* src32 = xGuDrawPtr(1, 1);
                u8* dst = t->mipmaps[i].data;
                int x, y;
                for (y = 0; y < mip_height; y++)
                {
                    for (x = 0; x < mip_width; x++)
                    {
                        dst[x] = tex_nearest_index(t, (t->cpsm == GU_PSM_8888 ? src32[x] : src16[x]));
                    }
                    src16 += 256;
                    src32 += 256;
                    dst += t->mipmaps[i].buf_width;
                }
            }
            else
            {
                sceGuCopyImage(t->psm, 0, 0, mip_width, mip_height, 256, xGuDrawPtr(0, 1), 0, 0, t->mipmaps[i].buf_width, t->mipmaps[i].data);
                sceGuTexSync();
            }
            sceKernelDcacheWritebackAll();
            t->num_mips += 1;
        }
    }
    xGuClear(0xffffffff);
    xGuRenderToScreen();
    xGuTexFilter(X_NEAREST);
    sceGuDepthMask(GU_FALSE);
    xGuLoadStates();
}

static void tex_swizzle(xTexture* t)
{
    if (!t)
        return;
    if (!t->data || t->swizzled)
        return;
    void* swizzle_data = x_malloc(tex_size(t->psm, t->buf_width, t->buf_height));
    if (!swizzle_data)
        return;
    swizzle_fast(swizzle_data, t->data, tex_size(t->psm, t->buf_width, 1), t->buf_height);
    x_free(t->data);
    t->data = swizzle_data;
    int i;
    for (i = 0; i < t->num_mips; i++)
    {
        swizzle_data = x_malloc(tex_size(t->psm, t->mipmaps[i].buf_width, t->mipmaps[i].buf_height));
        swizzle_fast(swizzle_data, t->mipmaps[i].data, tex_size(t->psm, t->mipmaps[i].buf_width, 1), t->mipmaps[i].buf_height);
        x_free(t->mipmaps[i].data);
        t->mipmaps[i].data = swizzle_data;
    }
    sceKernelDcacheWritebackAll();
    t->swizzled = 1;
}

static void tex_finish(xTexture* t, int levels, int flags)
{
    if (levels > 0)
    {
        tex_generate_mipmaps(t, levels);
    }
    tex_swizzle(t);
    if (flags & X_TEX_TOP_IN_VRAM)
    {
        xTexUploadVRAM(t, 0);
    }
    if (flags & X_TEX_MIPS_IN_VRAM)
    {
        int i;
        for (i = 0; i < t->num_mips; i++)
        {
            xTexUploadVRAM(t, i+1);
        }
    }
}

#ifdef X_TEX_TGA

#define TGA_NO_DATA    (0)
#define TGA_MAPPED     (1)
#define TGA_RGB        (2)
#define TGA_GRAYSCALE  (3)
#define TGA_RLE_MAPPED (9)
#define TGA_RLE_RGBA   (10)
#define TGA_COMPRESSED_GRAYSCALE (11)
#define TGA_COMPRESSED_MAPPED    (32)
#define TGA_COMPRESSED_QUADTREE  (33)

#define TGA_SHIFT(n) (1<<(n))
#define TGA_DESC_YFLIP (TGA_SHIFT(5))

static u32 convert_tga_8888(u32 color)
{
    u8 r = (color >> 16) & 0xff;
    u8 b = (color >> 0) & 0xff;
    color &= 0xff00ff00;
    color |= (r << 0) | (b << 16);
    return color;
}

static u16 convert_tga_5551(u16 color)
{
    u8 r = (color>>10) & 0x1f;
    u8 b = (color>>0) & 0x1f;
    color &= 0x83e0;
    color |= (1<<15)|(r<<0)|(b<<10);
    return color;
}

typedef struct
{
    u8 idlength;
    u8 colormaptype;
    u8 datatype;
    u16 colormapstart;
    u16 colormaplength;
    u8 colormapdepth;
    u16 x_origin;
    u16 y_origin;
    u16 width;
    u16 height;
    u8 bitsperpixel;
    u8 descriptor;
} __attribute__((packed)) tga_header;

static int tex_load_tga(xTexture* t, char* filename, int levels, int flags)
{
    FILE* file = fopen(filename, "rb");
    if (!file)
        return 1;
    
    tga_header header;
    fread(&header, sizeof(tga_header), 1, file);
    
    t->width = header.width;
    t->height = header.height;
    t->buf_width = x_next_pow2(t->width);
    t->pow2_height = x_next_pow2(t->height);
    t->buf_height = x_num_align(t->height, 8);
    t->u_scale = (float)t->width/t->buf_width;
    t->v_scale = (float)t->height/t->pow2_height;
    
    fseek(file, header.idlength, SEEK_CUR);

	X_LOG("TGA: %s, idlength: %u, colormaptype: %u, datatype: %u, colormapstart: %u, colormaplength: %u, colormapdepth: %u \
		  x_origin: %u, y_origin: %u, width: %u, height: %u, bitsperpixel: %u, descriptor: %u",
		  filename, header.idlength, header.colormaptype, header.datatype, header.colormapstart, header.colormaplength, header.colormapdepth,
		  header.x_origin, header.y_origin, header.width, header.height, header.bitsperpixel, header.descriptor);

    int x, y;
    if (header.datatype == TGA_NO_DATA)
    {
        fclose(file);
        return 1;
    }
    else if (header.datatype == TGA_MAPPED)
    {
        if (header.colormapdepth == 16)
            t->cpsm = GU_PSM_5551;
        else if (header.colormapdepth == 24 || header.colormapdepth == 32)
            t->cpsm = GU_PSM_8888;
        else
        {
            fclose(file);
            return 1;
        }
        /*
        if (header.bitsperpixel == 4)
            t->psm = GU_PSM_T4;
        */
        if (header.bitsperpixel == 8)
            t->psm = GU_PSM_T8;
        /*
        else if (header.bitsperpixel == 16)
            t->psm = GU_PSM_T16;
        else if (header.bitsperpixel == 32)
            t->psm = GU_PSM_T32;
        */
        else
        {
            fclose(file);
            return 1;
        }
        t->clut_entries = header.colormaplength;
        t->clut = x_malloc(tex_size(t->cpsm, header.colormaplength, 1));
        if (!t->clut)
        {
            fclose(file);
            return 1;
        }
        t->data = x_malloc(tex_size(t->psm, t->buf_width, t->buf_height));
        if (!t->data)
        {
            fclose(file);
            return 1;
        }
        if (header.colormapdepth == 24)
        {
            u32* ptr = (u32*)t->clut;
            for (x = 0; x < header.colormaplength; x++)
            {
                fread(&ptr[x], 3, 1, file);
                ptr[x] |= (0xff << 24);
            }
        }
        else
        {
            fread(t->clut, tex_size(t->cpsm, header.colormaplength, 1), 1, file);
        }
        for (y = 0; y < t->height; y++)
        {
            fread((void*)((u32)t->data + tex_size(t->psm, t->buf_width, (header.descriptor & TGA_DESC_YFLIP ? t->height-y-1 : y))), tex_size(t->psm, t->width, 1), 1, file);
        }
        if (t->cpsm == GU_PSM_5551)
        {
            u16* ptr = (u16*)t->clut;
            for (x = 0; x < t->clut_entries; x++)
            {
                ptr[x] = convert_tga_5551(ptr[x]);
            }
        }
        else if (t->cpsm == GU_PSM_8888)
        {
            u32* ptr = (u32*)t->clut;
            for (x = 0; x < t->clut_entries; x++)
            {
                ptr[x] = convert_tga_8888(ptr[x]);
            }
        }
    }
    else if (header.datatype == TGA_RGB)
    {
        if (header.bitsperpixel == 16)
            t->psm = GU_PSM_5551;
        else if (header.bitsperpixel == 24 || header.bitsperpixel == 32)
            t->psm = GU_PSM_8888;
        else
        {
            fclose(file);
            return 1;
        }
        
        t->data = x_malloc(tex_size(t->psm, t->buf_width, t->buf_height));
        if (!t->data)
        {
            fclose(file);
            return 1;
        }
        if (header.bitsperpixel == 24)
        {
            u32* ptr = (u32*)t->data;
            for (y = 0; y < t->height; y++)
            {
                for (x = 0; x < t->width; x++)
                {
                    fread(&ptr[(header.descriptor & TGA_DESC_YFLIP ? t->height-y-1 : y)*t->buf_width + x], 3, 1, file);
                    ptr[(header.descriptor & TGA_DESC_YFLIP ? t->height-y-1 : y)*t->buf_width + x] |= (0xff << 24);
                }
            }
        }
        else
        {
            for (y = 0; y < t->height; y++)
            {
                fread((void*)((u32)t->data + tex_size(t->psm, t->buf_width, (header.descriptor & TGA_DESC_YFLIP ? t->height-y-1 : y))), tex_size(t->psm, t->width, 1), 1, file);
			}
        }
        if (t->psm == GU_PSM_5551)
        {
            u16* ptr = (u16*)t->data;
            for (y = 0; y < t->height; y++)
            {
                for (x = 0; x < t->width; x++)
                {
                    ptr[y*t->buf_width + x] = convert_tga_5551(ptr[y*t->buf_width + x]);
                }
            }
        }
        else if (t->psm == GU_PSM_8888)
        {
            u32* ptr = (u32*)t->data;
            for (y = 0; y < t->height; y++)
            {
                for (x = 0; x < t->width; x++)
                {
					ptr[y*t->buf_width + x] = convert_tga_8888(ptr[y*t->buf_width + x]);
                }
            }
        }
    }
    else if (header.datatype == TGA_GRAYSCALE)
    {
        if (header.bitsperpixel != 8)
        {
            fclose(file);
            return 1;
        }
        t->cpsm = GU_PSM_8888;
        t->psm = GU_PSM_T8;
        t->clut_entries = 256;
        t->clut = x_malloc(256*sizeof(unsigned int));
        t->data = x_malloc(t->buf_width*t->buf_height);
        if (!t->clut || !t->data)
        {
            fclose(file);
            return 1;
        }
        u32* ptr = (u32*)t->clut;
        for (x = 0; x < 256; x++)
        {
            if (flags & X_TEX_GRAY_TO_ALPHA) ptr[x] = GU_RGBA(255,255,255,x);
            else ptr[x] = GU_RGBA(x,x,x,255);
        }
        for (y = 0; y < t->height; y++)
        {
            fread((void*)((u32)t->data + (header.descriptor & TGA_DESC_YFLIP ? t->height-y-1 : y)*t->buf_width), t->width, 1, file);
        }
    }
    else
    {
        //Unsupported type
    }
    fclose(file);
    sceKernelDcacheWritebackAll();
    return 0;
}

#else

static int tex_load_tga(xTexture* t, char* filename, int levels, int flags)
{
    return 1;
}

#endif

#ifdef X_TEX_PNG

#include <png.h>

static inline void user_warning_fn(png_structp png_ptr, png_const_charp warning_msg);

static int tex_load_png(xTexture* t, char* filename, int levels, int flags)
{
	png_structp png_ptr;
	png_infop info_ptr;
	unsigned int sig_read = 0;
	png_uint_32 png_width, png_height;
	int bit_depth, color_type, interlace_type;
	u32* line;

	FILE* file = fopen(filename, "rb");
	if (!file) return 1;

	png_ptr = png_create_read_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);
	if (png_ptr == 0)
    {
		fclose(file);
		return 1;
	}

	png_set_error_fn(png_ptr, (png_voidp) NULL, (png_error_ptr) NULL, user_warning_fn);

	info_ptr = png_create_info_struct(png_ptr);
	if (info_ptr == NULL)
    {
		fclose(file);
		png_destroy_read_struct(&png_ptr, png_infopp_NULL, png_infopp_NULL);
		return 1;
	}
	png_init_io(png_ptr, file);
	png_set_sig_bytes(png_ptr, sig_read);
	png_read_info(png_ptr, info_ptr);
	png_get_IHDR(png_ptr, info_ptr, &png_width, &png_height, &bit_depth, &color_type, &interlace_type, int_p_NULL, int_p_NULL);

	t->width = png_width;
	t->height = png_height;
    t->buf_width = x_next_pow2(t->width);
    t->pow2_height = x_next_pow2(t->height);
    t->buf_height = x_num_align(t->height, 8);

    t->u_scale = (float)t->width/t->buf_width;
    t->v_scale = (float)t->height/t->pow2_height;

	png_set_strip_16(png_ptr);
	png_set_packing(png_ptr);
	if (color_type == PNG_COLOR_TYPE_PALETTE) png_set_palette_to_rgb(png_ptr);
	if (color_type == PNG_COLOR_TYPE_GRAY && bit_depth < 8) png_set_gray_1_2_4_to_8(png_ptr);
	if (png_get_valid(png_ptr, info_ptr, PNG_INFO_tRNS)) png_set_tRNS_to_alpha(png_ptr);

	png_set_filler(png_ptr, 0xff, PNG_FILLER_AFTER);
	
	t->psm = GU_PSM_8888;
	t->cpsm = 0;
	t->data = x_malloc(t->buf_width*t->buf_height*4);
    if (!t->data)
    {
        fclose(file);
        return 1;
    }

	line = (u32*)x_malloc(t->width * 4);
	u32* ptr = (u32*)t->data;
	int x, y;
	for (y = 0; y < t->height; y++)
    {
		png_read_row(png_ptr, (u8*) line, png_bytep_NULL);
		for (x = 0; x < t->width; x++)
        {
            ptr[(t->height-y-1)*t->buf_width + x] = line[x];
		}
	}
	x_free(line);
	png_read_end(png_ptr, info_ptr);
	png_destroy_read_struct(&png_ptr, &info_ptr, png_infopp_NULL);
	fclose(file);
    sceKernelDcacheWritebackAll();
	return 0;
}

#else

static int tex_load_png(xTexture* t, char* filename, int levels, int flags)
{
    return 1;
}

#endif

#ifdef X_TEX_BMP

#define BMP_IDENT (('B'<<0)|('M'<<8)) /* "BM" (0x4d42) */

typedef struct
{
    u16 id;
    u32 filesize;
    u16 reserved0;
    u16 reserved1;
    u32 datastart;
    u32 headerlength;
    u32 t->width;
    u32 t->height;
    u16 colorplanes;
    u16 bitsperpixel;
    u32 compression;
    u32 datasize;
    u32 xresolution;
    u32 yresolution;
    u32 colors;
    u32 importantcolors;
} __attribute__((packed)) bmp_header;

static int tex_load_bmp(xTexture* t, char* filename, int levels, int flags)
{
    /*
    FILE* file = fopen(filename, "rb");
    if (!file) return 1;

    bmp_header header;
    fread(&header, sizeof(bmp_header), 1, file);
    if (header.id != BMP_IDENT)
    {
        fclose(file);
        return 1;
    }

    t->width = header.t->width;
    t->height = header.t->height;
    t->buf_width = x_next_pow2(t->width);
    t->buf_height = x_next_pow2(t->height);
    t->u_scale = (float)t->width/t->buf_width;
    t->v_scale = (float)t->height/t->buf_height;

    fseek(file, header.idlength, SEEK_CUR);
    fseek(file, header.colormaplength, SEEK_CUR);
    int x, y;
    int bytes;
    if (header.datatype == TGA_NO_DATA)
    {
        fclose(file);
        return 1;
    }
    else if (header.datatype == TGA_MAPPED)
    {
        //
    }
    
    //...
    
    fclose(file);
    tex_generate_mipmaps(t, levels);
    tex_swizzle(t);
    if (flags & X_TEX_TOP_IN_VRAM)
        xTexUploadVRAM(t,0);
    if (flags & X_TEX_MIPS_IN_VRAM)
    {
        for (x = 0; x < t->num_mips; x++)
        {
            xTexUploadVRAM(t, x+1);
        }
    }
    return 0;
    */
    return 1;
}

#else

static int tex_load_bmp(xTexture* t, char* filename, int levels, int flags)
{
    return 1;
}

#endif

xTexture* xTexLoadTex(char* filename, int levels, int flags)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load texture \"%s\"...", filename);
    char* ptr = strrchr(filename, '.');
    if (!ptr)
        return 0;
    xTexture* t = 0;
    if (stricmp(".tga", ptr) == 0)
        t = xTexLoadTGA(filename, levels, flags);
    else if (stricmp(".png", ptr) == 0)
        t = xTexLoadPNG(filename, levels, flags);
    else if (stricmp(".bmp", ptr) == 0)
        t = xTexLoadBMP(filename, levels, flags);
    return t;
}

xTexture* xTexLoadTGA(char* filename, int levels, int flags)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load TGA texture \"%s\"...", filename);
    xTexture* t = tex_initialize();
    if (!t)
        return 0;
    if (tex_load_tga(t, filename, levels, flags) != 0)
    {
        xTexFree(t);
        return 0;
    }
    //generate mipmaps, swizzle, put in vram
    tex_finish(t, levels, flags);
	X_LOG("Successfully loaded TGA texture.");
    return t;
}

xTexture* xTexLoadPNG(char* filename, int levels, int flags)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load PNG texture \"%s\"...", filename);
    xTexture* t = tex_initialize();
    if (!t)
        return 0;
    if (tex_load_png(t, filename, levels, flags) != 0)
    {
        xTexFree(t);
        return 0;
    }
	tex_finish(t, levels, flags);
	X_LOG("Successfully loaded PNG texture.");
    return t;
}

xTexture* xTexLoadBMP(char* filename, int levels, int flags)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load BMP texture \"%s\"...", filename);
    xTexture* t = tex_initialize();
    if (!t)
        return 0;
    if (tex_load_bmp(t, filename, levels, flags) != 0)
    {
        xTexFree(t);
        return 0;
    }
	tex_finish(t, levels, flags);
	X_LOG("Successfully loaded BMP texture.");
    return t;
}

void xTexFree(xTexture* t)
{
    if (!t)
        return;
    if (t->data)
        x_free(t->data);
    if (t->clut)
        x_free(t->clut);
    if (t->mipmaps)
    {
        int i;
        for (i = 0; i < t->num_mips; i++)
        {
            if (t->mipmaps[i].data)
                x_free(t->mipmaps[i].data);
        }
    }
    x_free(t);
}

#define IN_VRAM(PTR) ((u32)(PTR) >= X_MEM_VRAM && (u32)(PTR) < X_MEM_VRAM + X_MEM_VRAM_SIZE)

int xTexUploadVRAM(xTexture* t, int level)
{
    if (!t)
        return 1;
    if (!t->data || level < 0)
        return 1;
    int size = 0;
    void* vram_ptr = 0;
    if (level == 0)
    {
        if (IN_VRAM(t->data)) return 1;
        size = tex_size(t->psm, t->buf_width, t->buf_height);
        vram_ptr = x_valloc(size);
        if (!vram_ptr) return 1;
        memcpy(X_UNCACHED(vram_ptr), t->data, size);
        x_free(t->data);
        t->data = vram_ptr;
    }
    else
    {
        if (IN_VRAM(t->mipmaps[level-1].data)) return 1;
        size = tex_size(t->psm, t->mipmaps[level-1].buf_width, t->mipmaps[level-1].buf_height);
        vram_ptr = x_valloc(size);
        if (!vram_ptr) return 1;
        memcpy(X_UNCACHED(vram_ptr), t->mipmaps[level-1].data, size);
        x_free(t->mipmaps[level-1].data);
        t->mipmaps[level-1].data = vram_ptr;
    }
    //sceKernelDcacheWritebackAll();
	return 0;
}

int xTexDownloadVRAM(xTexture* t, int level)
{
    if (!t)
        return 1;
    if (!t->data || level < 0)
        return 1;
    int size = 0;
    void* mem_ptr = 0;
    if (level == 0)
    {
        if (!IN_VRAM(t->data)) return 1;
        size = tex_size(t->psm, t->buf_width, t->buf_height);
        mem_ptr = x_malloc(size);
        if (!mem_ptr) return 1;
        memcpy(mem_ptr, X_UNCACHED(t->data), size);
        x_free(t->data);
        t->data = mem_ptr;
    }
    else
    {
        if (!IN_VRAM(t->mipmaps[level-1].data)) return 1;
        size = tex_size(t->psm, t->mipmaps[level-1].buf_width, t->mipmaps[level-1].buf_height);
        mem_ptr = x_malloc(size);
        if (!mem_ptr) return 1;
        memcpy(mem_ptr, X_UNCACHED(t->mipmaps[level-1].data), size);
        x_free(t->mipmaps[level-1].data);
        t->mipmaps[level-1].data = mem_ptr;
    }
    //sceKernelDcacheWritebackAll();
	return 0;
}

void xTexSetImage(xTexture* t)
{
    if (!t)
	{
        xGuSetDebugTex();
		return;
	}
	if (!t->data)
	{
		xGuSetDebugTex();
		return;
	}
	if (t->clut)
	{
		sceGuClutMode(t->cpsm, 0, 0xff, 0);
		sceGuClutLoad(t->clut_entries>>3, t->clut);
	}
	sceGuTexScale(t->u_scale, t->v_scale);
	sceGuTexMode(t->psm, t->num_mips, 0, t->swizzled);
	sceGuTexImage(0, t->buf_width, t->pow2_height, t->buf_width, t->data);
	if (t->mipmaps)
	{
		int i;
		for (i = 0; i < t->num_mips; i++)
		{
			sceGuTexImage(i+1, t->mipmaps[i].buf_width, t->mipmaps[i].pow2_height, t->mipmaps[i].buf_width, t->mipmaps[i].data);
		}
	}
}

void xTexDraw(xTexture* t, int x, int y, int w, int h, int tx, int ty, int tw, int th)
{
	xTexSetImage(t);
	xGuSaveStates();
	sceGuEnable(GU_TEXTURE_2D);
	sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_DEPTH_TEST);
	xGuDrawTex(x, y, w, h, tx, ty, tw, th);
	xGuLoadStates();
}

void xTexDrawAngle(xTexture* t, float x, float y, float w, float h, int tx, int ty, int tw, int th, float angle)
{
	xTexSetImage(t);
	xGuSaveStates();
	sceGuEnable(GU_TEXTURE_2D);
	sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_DEPTH_TEST);
	float s, c;
	x_sincos(angle, &s, &c);
	w *= 0.5f;
	h *= 0.5f;
	float cw = c*w;
	float sw = s*w;
	float ch = c*h;
	float sh = s*h;
	TVertex2D* vertices = (TVertex2D*)sceGuGetMemory(4*sizeof(TVertex2D));
	vertices[0].u = tx;
	vertices[0].v = ty;
	vertices[0].x = x + (-cw - sh);
	vertices[0].y = y + (-sw + ch);
	vertices[0].z = 0;
	vertices[1].u = tx;
	vertices[1].v = ty + th;
	vertices[1].x = x + (-cw - -sh);
	vertices[1].y = y + (-sw + -ch);
	vertices[1].z = 0;
	vertices[2].u = tx + tw;
	vertices[2].v = ty + th;
	vertices[2].x = x + (cw - -sh);
	vertices[2].y = y + (sw + -ch);
	vertices[2].z = 0;
	vertices[3].u = tx + tw;
	vertices[3].v = ty;
	vertices[3].x = x + (cw - sh);
	vertices[3].y = y + (sw + ch);
	vertices[3].z = 0;
	sceGuDrawArray(GU_TRIANGLE_FAN, TVertex2D_vtype|GU_TRANSFORM_2D, 4, 0, vertices);
	xGuLoadStates();
}

/*
void xTexDrawOrtho(xTexture* t)
{
    if (!t) return;
    xGuTexMode(GU_TFX_REPLACE, 1, 0);
    xTexSetImage(t);
    xGuSaveStates();
    sceGuEnable(GU_TEXTURE_2D);
    sceGuEnable(GU_BLEND);
    sceGuDisable(GU_LIGHTING);
    sceGuDisable(GU_DEPTH_TEST);
    xGumDrawUnitTexQuad();
    xGuLoadStates();
}

void xTexDrawSprite(xTexture* t, ScePspFVector3* pos, float length, float height, int sx, int sy, int sw, int sh)
{
    if (!t) return;
    ScePspFMatrix4 view_mat;
    sceGumMatrixMode(GU_VIEW);
    sceGumStoreMatrix(&view_mat);
    sceGumLoadIdentity();

    ScePspFVector3 translate;
    x_billboard(&translate, pos, &view_mat);
    sceGumMatrixMode(GU_MODEL);
    sceGumPushMatrix();
    sceGumLoadIdentity();
    sceGumTranslate(&translate);
    xGumScale(length, height, 1.0f);

    xGuTexMode(GU_TFX_REPLACE, 1, 1);
    xTexSetImage(t);
    float u0 = (float)sx / t->width;
    float u1 = (float)(sx+sw) / t->width;
    float v0 = (float)sy / t->height;
    float v1 = (float)(sy+sh) / t->height;

    xGuSaveStates();
    sceGuEnable(GU_TEXTURE_2D);
    sceGuEnable(GU_BLEND);
    sceGuDisable(GU_LIGHTING);
    TVertexF* vertices = (TVertexF*)sceGuGetMemory(4*sizeof(TVertexF));
    vertices[0].u = u0;
    vertices[0].v = v0;
    vertices[0].x = -0.5f;
    vertices[0].y = -0.5f;
    vertices[0].z = 0.0f;
    vertices[1].u = u1;
    vertices[1].v = v1;
    vertices[1].x = 0.5f;
    vertices[1].y = 0.5f;
    vertices[1].z = 0.0f;
    sceGumDrawArray(GU_SPRITES, TVertexF_vtype|GU_TRANSFORM_3D, 2, 0, vertices);
    xGuLoadStates();

    sceGumPopMatrix();
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadMatrix(&view_mat);
    sceGumMatrixMode(GU_MODEL);
}
*/
