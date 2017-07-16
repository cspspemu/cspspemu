/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <string.h>
#include <stdarg.h>
#include <pspgu.h>
#include <pspgum.h>
#include "xmath.h"
#include "xgraphics.h"
#include "xtexture.h"
#include "xmem.h"

#include "xtext.h"

static xBitmapFont* x_current_font = 0;
static u32 x_font_color = 0xffffffff;
static float x_font_scale = 1.0f;
static int x_font_align = X_ALIGN_LEFT;

static u16 default_widths[256] =
{
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10, 
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10,
    10, 10, 10, 10,
    
    10,  6,  8, 10, //   ! " #
    10, 10, 10,  6, // $ % & '
    10, 10, 10, 10, // ( ) * +
     6, 10,  6, 10, // , - . /
    
    8,   6,  8,  8, // 0 1 2 3
    8,   8,  8,  8, // 6 5 8 7
    8,   8,  6,  6, // 8 9 : ;
    10, 10, 10, 10, // < = > ?
    
    16, 10, 10, 10, // @ A B C
    10, 10, 10, 10, // D E F G
    10,  6,  8, 10, // H I J K
     8, 10, 10, 10, // L M N O
    
    10, 10, 10, 10, // P Q R S
    10, 10, 10, 12, // T U V W
    10, 10, 10, 10, // X Y Z [
    10, 10,  8, 10, // \ ] ^ _
    
     6,  8,  8,  8, // ` a b c
     8,  8,  6,  8, // d e f g
     8,  6,  6,  8, // h i j k
     6, 10,  8,  8, // l m n o
    
     8,  8,  6,  8, // p q r s
     6,  8,  8, 12, // t u v w
     8,  8,  8, 10, // x y z {
     8, 10,  8, 12, // | } ~  
     
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10, 
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10,
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10, 
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10,
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10, 
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10,
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10, 
    10, 10, 10, 10,
    
    10, 10, 10, 10, 
    10, 10, 10, 10,
    10, 10, 10, 10,
    10, 10, 10, 10,
};

static void copy_widths(xBitmapFont* font)
{
    if (!font) return;
    if (!font->texture) return;
    memcpy(font->widths, default_widths, 256*sizeof(u16));
    float scale = font->texture->width/128.0f;
    int i;
    for (i = 0; i < 256; i++)
    {
        font->widths[i] = (u16)(scale*font->widths[i]);
    }
}

xBitmapFont* xTextLoadFont(xTexture* tex, char* widths_filename)
{
	xBitmapFont* font = (xBitmapFont*)x_malloc(sizeof(xBitmapFont));
	font->texture = NULL;
    if (tex == NULL)
    {
		xTextFreeFont(font);
        return NULL;
    }
    font->texture = tex;
    if (font->texture->width < 64 || font->texture->width != x_next_pow2(font->texture->width) || font->texture->height != font->texture->width)
    {
		xTextFreeFont(font);
        return NULL;
    }
    if (!widths_filename)
    {
        copy_widths(font);
        return font;
    }
    FILE* file = fopen(widths_filename, "rb");
    if (!file)
	{
        copy_widths(font);
		goto end;
    }
    fread(font->widths, 256*sizeof(u16), 1, file);
    fclose(file);
end:
    if (!x_current_font)
    {
        xTextSetFont(font);
        xTextSetColor(0xffffffff);
        xTextSetScale(1.0f);
        xTextSetAlign(X_ALIGN_LEFT);
    }
	return font;
}

void xTextFreeFont(xBitmapFont* font)
{
	if (font != NULL)
	{
		x_free(font);
	}
}

void xTextSetFont(xBitmapFont* font)
{
    x_current_font = font;
}

void xTextSetColor(u32 color)
{
    x_font_color = color;
}

void xTextSetScale(float scale)
{
    x_font_scale = scale;
}

void xTextSetAlign(int align)
{
    x_font_align = align;
}

int xTextLength(char* text, int num)
{
    if (!x_current_font) return 0;
    
    float length = 0.0f;
    char* cur_char = text;
    while (cur_char != '\0' && num > 0)
    {
        length += x_font_scale*x_current_font->widths[(u8)*cur_char];
        cur_char += 1;
        num -= 1;
    }
    return (int)length;
}

int xTextNumWithLength(char* src, int length)
{
    if (!x_current_font || !src || length <= 0)
    {
        return 0;
    }
    float new_length = 0;
    while (new_length < length && src != '\0')
    {
        new_length += x_font_scale*x_current_font->widths[(u8)*src];
        src += 1;
    }
    return (int)new_length;
}

typedef struct {
	s16 u, v;
	u32 color;
    s16 x, y, z;
} Text_Vert;

#define Text_Vert_vtype (GU_TEXTURE_16BIT|GU_COLOR_8888|GU_VERTEX_16BIT)

int xTextPrint(int x, int y, char* text)
{
    if (!x_current_font) return 0;
    if (!x_current_font->texture) return 0;
    
    float pos = (float)x;
	int len = strlen(text);
    int text_length = xTextLength(text, len);
    if (x_font_align == X_ALIGN_CENTER) pos -= 0.5f*text_length;
    else if (x_font_align == X_ALIGN_RIGHT) pos -= text_length;
    
    u16 char_width = x_current_font->texture->width/16;

    Text_Vert* vertices = (Text_Vert*)sceGuGetMemory(2*len*sizeof(Text_Vert));
    Text_Vert* vert_ptr = vertices;
    int i = 0;
    while (/* *text != '\0' && num >= 0 */ i < len)
    {
        int tx = (((u8)*text >> 0) & 0x0f) * char_width;
        int ty = (((u8)*text >> 4) & 0x0f) * char_width;

        vert_ptr->u = (s16)(tx);
        vert_ptr->v = (s16)(x_current_font->texture->height - ty);
		vert_ptr->color = x_font_color;
        vert_ptr->x = (int)(pos);
        vert_ptr->y = (int)(y);
        vert_ptr->z = 0.0f;
        
        vert_ptr += 1;
        pos += x_font_scale*x_current_font->widths[(u8)*text];

        vert_ptr->u = (s16)(tx + x_current_font->widths[(u8)*text]);
		vert_ptr->v = (s16)(x_current_font->texture->height - ty - char_width);
		vert_ptr->color = x_font_color;
        vert_ptr->x = (int)(pos);
        vert_ptr->y = (int)(y + x_font_scale*char_width);
        vert_ptr->z = 0.0f;
        
        vert_ptr += 1;
        text += 1;
        //num -= 1;
        i += 1;
    }

    xTexSetImage(x_current_font->texture);
    xGuSaveStates();
    sceGuEnable(GU_TEXTURE_2D);
    sceGuEnable(GU_BLEND);
    sceGuBlendFunc(GU_ADD, GU_SRC_ALPHA, GU_ONE_MINUS_SRC_ALPHA, 0, 0);
    sceGuDisable(GU_DEPTH_TEST);
    sceGuDrawArray(GU_SPRITES, Text_Vert_vtype|GU_TRANSFORM_2D, 2*len, 0, vertices);
    xGuLoadStates();

    return (int)text_length;
}

int xTextPrintf(int x, int y, char* text, ... )
{
    char buffer[512];
    va_list ap;
    va_start(ap, text);
    vsnprintf(buffer, sizeof(buffer), text, ap);
    va_end(ap);
    return xTextPrint(x, y, buffer);
}

/*
float xText3DPrint(xBitmapFont* font, float x, float y, float z, float fw_scale, float height, unsigned int color, char* text)
{
    int len = strlen(text);
    if(len <= 0) return 0;
    
    float text_length = xTextLength(font, fw_scale, text);
    float pos = -0.5f*text_length;
    u16 char_width = font->texture->width >> 4;

    Text_Vert* vertices = (Text_Vert*)sceGuGetMemory(6*len*sizeof(Text_Vert));
    int i;
    for(i = 0; i < len; i++)
    {
        unsigned char index = (unsigned char)text[i];
        int tx = (index & 0x0F) * char_width;
        int ty = ((index & 0xF0) >> 4) * char_width;
        int offset = (char_width - font->widths[index]) >> 1;

        vertices[i*6+0].u = (s16)(tx + offset);
        vertices[i*6+0].v = (s16)(font->texture->height - ty - char_width);
        vertices[i*6+0].x = (float)(pos);
        vertices[i*6+0].y = (float)(y - 0.5f*height*char_width);
        vertices[i*6+0].z = 0.0f;
        
        vertices[i*6+1].u = (s16)(tx + offset);
        vertices[i*6+1].v = (s16)(font->texture->height - ty - 1);
        vertices[i*6+1].x = (float)(pos);
        vertices[i*6+1].y = (float)(y + 0.5f*height*char_width);
        vertices[i*6+1].z = 0.0f;
        
        vertices[i*6+2].u = (s16)(tx + offset + font->widths[index] + 1);
        vertices[i*6+2].v = (s16)(font->texture->height - ty - 1);
        vertices[i*6+2].x = (float)(pos + fw_scale*font->widths[index]);
        vertices[i*6+2].y = (float)(y + 0.5f*height*char_width);
        vertices[i*6+2].z = 0.0f;

        vertices[i*6+3].u = (s16)(tx + offset + font->widths[index] + 1);
        vertices[i*6+3].v = (s16)(font->texture->height - ty - 1);
        vertices[i*6+3].x = (float)(pos + fw_scale*font->widths[index]);
        vertices[i*6+3].y = (float)(y + 0.5f*height*char_width);
        vertices[i*6+3].z = 0.0f;

        vertices[i*6+4].u = (s16)(tx + offset + font->widths[index] + 1);
        vertices[i*6+4].v = (s16)(font->texture->height - ty - char_width);
        vertices[i*6+4].x = (float)(pos + fw_scale*font->widths[index]);
        vertices[i*6+4].y = (float)(y - 0.5f*height*char_width);
        vertices[i*6+4].z = 0.0f;

        vertices[i*6+5].u = (s16)(tx + offset);
        vertices[i*6+5].v = (s16)(font->texture->height - ty - char_width);
        vertices[i*6+5].x = (float)(pos);
        vertices[i*6+5].y = (float)(y - 0.5f*height*char_width);
        vertices[i*6+5].z = 0.0f;

        pos += fw_scale*font->widths[index];
    }
    
    ScePspFMatrix4 view_mat;
    sceGumMatrixMode(GU_VIEW);
    sceGumStoreMatrix(&view_mat);
    sceGumLoadIdentity();

    ScePspFVector3 pos3d = {x,y,z};
    ScePspFVector3 translate;
    x_billboard(&translate, &pos3d, &view_mat);
    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();
    sceGumTranslate(&translate);
    
    xGuTexMode(X_TFX_MODULATE, 1, 1);
    xTexSetImage(font->texture);

    xGuSaveStates();
    sceGuEnable(GU_TEXTURE_2D);
    sceGuEnable(GU_BLEND);
    sceGuDisable(GU_LIGHTING);
    sceGuColor(color);
    sceGumDrawArray(GU_TRIANGLES, Text_Vert_vtype|GU_TRANSFORM_3D, 6*len, 0, vertices);
    xGuLoadStates();
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadMatrix(&view_mat);
    sceGumMatrixMode(GU_MODEL);
    
    return text_length;
}

float xText3DPrintf(xBitmapFont* font, float x, float y, float z, float fw_scale, float height, unsigned int color, char* text, ... )
{
    char buffer[512];
    va_list ap;
    va_start(ap, text);
    vsnprintf(buffer, sizeof(buffer), text, ap);
    va_end(ap);
    return xText3DPrint(font, x, y, z, fw_scale, height, color, buffer);
}
*/
