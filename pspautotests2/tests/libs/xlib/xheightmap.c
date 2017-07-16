/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdio.h>
#include <math.h>
#include <pspgu.h>
#include <pspgum.h>
#include "xmem.h"
#include "xmath.h"
#include "xgraphics.h"
#include "xheightmap.h"

#define LIST_SIZE 512

//#define FRUSTUM_CULL

#define DRAW_PRIM GU_TRIANGLE_STRIP

#define FLOAT_TO_S16(x) ((s16)(32767*(x)))

int xHeightmapLoad(xHeightmap* h, char* filename, int width, float tile_scale, float zscale)
{
    if (!h) return 1;
	int height = width;
    h->width = width;
	h->height = width;
    h->tile_scale = tile_scale;
	h->z_scale = zscale;
    FILE* file = fopen(filename, "r");
    if (!file) return 1;
    h->vertices = (hmap_vertex*)x_malloc(width*height*sizeof(hmap_vertex));
    h->indices_static = (u16*)x_malloc(2*width*sizeof(u16));
    if (!h->vertices || !h->indices_static)
	{
		fclose(file);
        xHeightmapFree(h);
        return 1;
    }
    u16 temp_height = 0;
    int x, y;
    for (y = 0; y < height; y++)
    {
        for (x = 0; x < width; x++)
        {
            fread(&temp_height, 2, 1, file);
            h->vertices[y*width+x].u = FLOAT_TO_S16((float)x/(width-1));
            h->vertices[y*width+x].v = FLOAT_TO_S16((float)y/(height-1));
            h->vertices[y*width+x].p.x = tile_scale * x;
            h->vertices[y*width+x].p.y = tile_scale * y;
            h->vertices[y*width+x].p.z = zscale * (temp_height/65535.0f);
        }
    }
    int i;
    for (i = 0; i < 2*width; i++)
    {
        //goes from 1 row up, 1 row down, right 1, 1 row up, 1 row down, etc
        h->indices_static[i] = (1 - i%2)*width + i/2;
    }

	X_LOG("heightmap: %i, %i, %f", h->width, h->height, h->tile_scale);
    sceKernelDcacheWritebackAll();
    fclose(file);
    return 0;
}

void xHeightmapFree(xHeightmap* h)
{
    if (!h) return;
    if (h->vertices)
	{
		x_free(h->vertices);
		h->vertices = 0;
	}
    if (h->indices_static)
	{
		x_free(h->indices_static);
		h->indices_static = 0;
	}
}

int xHeightmapSetupLOD(xHeightmapLOD* l, xHeightmap* h, int min_level)
{
	l->h = h;
	l->patches = 1 << min_level;
	l->patch_width = (h->width-1)/l->patches;
	l->indices_buffer = (u16*)x_malloc((2*h->width*(h->width-1) + 8*l->patches*l->patches)*sizeof(u16));
	l->list_buffer = x_malloc(l->patches*l->patches*LIST_SIZE);
	l->list_ptrs = (void**)x_malloc(l->patches*l->patches*sizeof(void*));
	l->bboxes = (hmap_bbox*)x_malloc(l->patches*l->patches*sizeof(hmap_bbox));
	l->ready = 0;
    if (!l->indices_buffer || !l->list_buffer || !l->list_ptrs || !l->bboxes)
    {
        xHeightmapFreeLOD(l);
        return 1;
    }
	
	int x0, y0, x, y;
	int index = 0;
	for (y0 = 0; y0 < l->patches; y0++)
	{
		for (x0 = 0; x0 < l->patches; x0++)
		{
			//initialize to opposite extremities
			ScePspFVector3 min = {HUGE_VAL, HUGE_VAL, HUGE_VAL};
			ScePspFVector3 max = {-HUGE_VAL, -HUGE_VAL, -HUGE_VAL};
			for (y = 0; y < l->patch_width+1; y++)
			{
				for (x = 0; x < l->patch_width+1; x++)
				{
					ScePspFVector3* vert = &l->h->vertices[y0*l->patch_width*l->h->width + y*l->h->width + x0*l->patch_width + x].p;
					if (vert->x < min.x) min.x = vert->x;
					if (vert->x > max.x) max.x = vert->x;
					if (vert->y < min.y) min.y = vert->y;
					if (vert->y > max.y) max.y = vert->y;
					if (vert->z < min.z) min.z = vert->z;
					if (vert->z > max.z) max.z = vert->z;
				}
			}
			l->bboxes[index].p[0].x = min.x; l->bboxes[index].p[0].y = min.y; l->bboxes[index].p[0].z = min.z;
			l->bboxes[index].p[1].x = max.x; l->bboxes[index].p[1].y = min.y; l->bboxes[index].p[1].z = min.z;
			l->bboxes[index].p[2].x = min.x; l->bboxes[index].p[2].y = max.y; l->bboxes[index].p[2].z = min.z;
			l->bboxes[index].p[3].x = min.x; l->bboxes[index].p[3].y = min.y; l->bboxes[index].p[3].z = max.z;
			l->bboxes[index].p[4].x = max.x; l->bboxes[index].p[4].y = max.y; l->bboxes[index].p[4].z = min.z;
			l->bboxes[index].p[5].x = min.x; l->bboxes[index].p[5].y = max.y; l->bboxes[index].p[5].z = max.z;
			l->bboxes[index].p[6].x = max.x; l->bboxes[index].p[6].y = min.y; l->bboxes[index].p[6].z = max.z;
			l->bboxes[index].p[7].x = max.x; l->bboxes[index].p[7].y = max.y; l->bboxes[index].p[7].z = max.z;
			index += 1;
		}
	}
	
	return 0;
}

void xHeightmapFreeLOD(xHeightmapLOD* l)
{
	if (!l) return;
	if (l->indices_buffer)
	{
		x_free(l->indices_buffer);
		l->indices_buffer = 0;
	}
	if (l->list_buffer)
	{
		x_free(l->list_buffer);
		l->list_buffer = 0;
	}
	if (l->list_ptrs)
	{
		x_free(l->list_ptrs);
		l->list_ptrs = 0;
	}
	if (l->bboxes)
	{
		x_free(l->bboxes);
		l->bboxes = 0;
	}
}

ScePspFVector3* point_from_grid(xHeightmap* h, int x, int y)
{
    return &h->vertices[y*h->width + x].p;
}

//which - bottom right (0) or top left (1)
void normal_from_grid(xHeightmap* h, ScePspFVector3* out, int x, int y, int which)
{
    if (!h) return;
    if (!h->vertices) return;
    if (which)
        x_normal(out, point_from_grid(h, x, y), point_from_grid(h, x, y+1), point_from_grid(h, x+1, y+1));
    else
        x_normal(out, point_from_grid(h, x, y), point_from_grid(h, x+1, y+1), point_from_grid(h, x+1, y));
}

void xHeightmapGetNormal(xHeightmap* h, ScePspFVector3* out, float x, float y)
{
    if (!h) return;
    if (!h->vertices) return;
	int height = h->width;
    float inv_scale = 1.0f/h->tile_scale;
    float gridx = x*inv_scale;
    float gridy = y*inv_scale;
    if (gridx < 0.0f || gridx >= h->width-1 || gridy < 0.0f || gridy >= height-1)
    {
        out->x = 0.0f;
        out->y = 0.0f;
        out->z = 1.0f;
    }
    else
        normal_from_grid(h, out, (int)gridx, (int)gridy, (gridy - (int)gridy > gridx - (int)gridx));
}

float xHeightmapGetHeight(xHeightmap* h, ScePspFVector3* normal, float x, float y)
{
    if (!h) return 0.0f;
    if (!h->vertices) return 0.0f;
	int height = h->width;
    float inv_scale = 1.0f/h->tile_scale;
    float gridx = x*inv_scale;
    float gridy = y*inv_scale;
    if (gridx < 0.0f || gridx >= h->width-1 || gridy < 0.0f || gridy >= height-1)
    {
        if (normal)
        {
            normal->x = 0.0f;
            normal->y = 0.0f;
            normal->z = 1.0f;
        }
        return 0.0f;
    }
    ScePspFVector3 n;
    normal_from_grid(h, &n, (int)gridx, (int)gridy, (gridy - (int)gridy > gridx - (int)gridx));
    if (normal)
        *normal = n;
    ScePspFVector3* p = point_from_grid(h, (int)gridx, (int)gridy);
    return (n.x*(x - p->x) + n.y*(y - p->y))/-n.z + p->z;
}

void xHeightmapDraw(xHeightmap* h)
{
    if (!h) return;
    if (!h->vertices) return;
	int height = h->width;
    xGuSaveStates();
    //sceGuDisable(GU_BLEND);
    sceGuDisable(GU_LIGHTING);
    sceGuFrontFace(GU_CCW);
    hmap_vertex* vertices = h->vertices;
    int i;
    for (i = 0; i < height-1; i++)
    {
        sceGumDrawArray(DRAW_PRIM, GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D, 2*h->width, h->indices_static, vertices);
        vertices += h->width;
    }
    sceGuFrontFace(GU_CW);
    xGuLoadStates();
}

static int subdivisions(float dist_sq, float start, float spacing, int patch_width)
{
    if (dist_sq < SQR(start))
        return 0;
    int r = 1;
    while (SQR(start + r*spacing) < dist_sq && patch_width/(1<<r) > 1)
        r += 1;
    return r;
}

//#define FRUSTUM_CULL
#define INDEX_H(h,x,y) ((y)*h->width + (x))

void xHeightmapBuildLOD(xHeightmapLOD* l, ScePspFVector3* p, float start, float spacing)
{
	if (!l) return;
	if (!l->h) return;
	if (!l->indices_buffer || !l->list_buffer || !l->h->vertices) return;
	xHeightmap* h = l->h;
	int patches = l->patches;
	int patch_width = l->patch_width;
    float dist_sq;
    s16 lod_width;
    int num_tiles;
    ScePspFVector3* center;
    s16 subdiv[5]; //center, up, right, down, left
    s16 length;
    hmap_vertex* vertices;
    u16* indices = (u16*)X_UNCACHED(l->indices_buffer);
	void* list = l->list_buffer;
    int x, y, i, j;
	int index = 0;
    for (y = 0; y < patches*patch_width; y += patch_width)
    {
        for (x = 0; x < patches*patch_width; x += patch_width)
        {
			sceGuStart(GU_CALL, list);
			sceGuBeginObject(GU_VERTEX_32BITF, 8, 0, &l->bboxes[index]);
            center = point_from_grid(h, x + patch_width/2, y + patch_width/2);
            dist_sq = SQR(center->x - p->x) + SQR(center->y - p->y);
            if (dist_sq < SQR(start))
            {
                for (i = 0; i < patch_width; i++)
                {
                    vertices = h->vertices + INDEX_H(h, x, y+i);
                    sceGuDrawArray(DRAW_PRIM, GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D, 2*(patch_width+1), h->indices_static, vertices);
                }
            }
            else
            {
                subdiv[0] = subdivisions(dist_sq, start, spacing, patch_width);
                //up
                if (y/patch_width >= patches-1)
                    subdiv[1] = subdiv[0];
                else
                {
                    center = point_from_grid(h, x + patch_width/2, y + 3*patch_width/2);
                    subdiv[1] = subdivisions(SQR(center->x - p->x) + SQR(center->y - p->y), start, spacing, patch_width);
                }
                //right
                if (x/patch_width >= patches-1)
                    subdiv[2] = subdiv[0];
                else
                {
                    center = point_from_grid(h, x + 3*patch_width/2, y + patch_width/2);
                    subdiv[2] = subdivisions(SQR(center->x - p->x) + SQR(center->y - p->y), start, spacing, patch_width);
                }
                //down
                if (y/patch_width <= 0)
                    subdiv[3] = subdiv[0];
                else
                {
                    center = point_from_grid(h, x + patch_width/2, y - patch_width/2);
                    subdiv[3] = subdivisions(SQR(center->x - p->x) + SQR(center->y - p->y), start, spacing, patch_width);
                }
                //left
                if (x/patch_width <= 0)
                    subdiv[4] = subdiv[0];
                else
                {
                    center = point_from_grid(h, x - patch_width/2, y + patch_width/2);
                    subdiv[4] = subdivisions(SQR(center->x - p->x) + SQR(center->y - p->y), start, spacing, patch_width);
                }
                //number of base tiles in one LOD tile
                num_tiles = 1 << subdiv[0];
                //number of LOD tiles in one square
                lod_width = patch_width / num_tiles;
                if (lod_width == 1 && (subdiv[0] > subdiv[1] || subdiv[0] > subdiv[2] || subdiv[0] > subdiv[3] || subdiv[0] > subdiv[4]))
                {
                    indices[0] = INDEX_H(h, patch_width/2, patch_width/2);
                    j = 1;
                    for (i = 0; i < 1<<(subdiv[0]-subdiv[1]); i++)
                    {
                        indices[j] = INDEX_H(h, patch_width - i*patch_width/(1<<(subdiv[0]-subdiv[1])), patch_width);
                        j += 1;
                    }
                    for (i = 0; i < 1<<(subdiv[0]-subdiv[4]); i++)
                    {
                        indices[j] = INDEX_H(h, 0, patch_width - i*patch_width/(1<<(subdiv[0]-subdiv[4])));
                        j += 1;
                    }
                    for (i = 0; i < 1<<(subdiv[0]-subdiv[3]); i++)
                    {
                        indices[j] = INDEX_H(h, i*patch_width/(1<<(subdiv[0]-subdiv[3])), 0);
                        j += 1;
                    }
                    for (i = 0; i < 1<<(subdiv[0]-subdiv[2]); i++)
                    {
                        indices[j] = INDEX_H(h, patch_width, i*patch_width/(1<<(subdiv[0]-subdiv[2])));
                        j += 1;
                    }
                    indices[j] = INDEX_H(h, patch_width, patch_width);
                    vertices = h->vertices + INDEX_H(h, x, y);
                    sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                    GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                    (1<<(subdiv[0]-subdiv[1])) + (1<<(subdiv[0]-subdiv[2])) + (1<<(subdiv[0]-subdiv[3])) + (1<<(subdiv[0]-subdiv[4])) + 2,
                                    indices, vertices);
                    indices += (1<<(subdiv[0]-subdiv[1])) + (1<<(subdiv[0]-subdiv[2])) + (1<<(subdiv[0]-subdiv[3])) + (1<<(subdiv[0]-subdiv[4])) + 2;
                }
                else
                {
                    for (i = 0; i < 2*(lod_width+1); i++)
                    {
                        //setup indices
                        indices[i] = INDEX_H(h, (i/2)*num_tiles, num_tiles - (i%2)*num_tiles);
                    }
                    //draw the inner grid, avoiding sections containing seams
                    for (i = (subdiv[0] > subdiv[3] ? 1 : 0); i < (subdiv[0] > subdiv[1] ? lod_width-1 : lod_width); i++)
                    {
                        vertices = h->vertices + INDEX_H(h, x, y+i*num_tiles);
                        length = lod_width+1;
                        if (subdiv[0] > subdiv[4])
                        {
                            vertices += num_tiles;
                            length -= 1;
                        }
                        if (subdiv[0] > subdiv[2])
                            length -= 1;
                        sceGuDrawArray(DRAW_PRIM, GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D, 2*length, indices, vertices);
                    }
                    indices += 2*(lod_width+1);
                    
                    //draw sections containing seams as triangle fans
                    //up
                    if (subdiv[0] > subdiv[1])
                    {
                        indices[0] = INDEX_H(h, num_tiles, 0);
                        j = 1;
                        for (i = 0; i < 1<<(subdiv[0]-subdiv[1]); i++)
                        {
                            indices[j] = INDEX_H(h, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[1])), num_tiles);
                            j += 1;
                        }
                        indices[j] = INDEX_H(h, 0, num_tiles);
                        indices[j+1] = INDEX_H(h, 0, 0);
                        for (i = 0; i < lod_width; i++)
                        {
                            if (!((i == 0 && subdiv[0] > subdiv[4]) || (i == lod_width-1 && subdiv[0] > subdiv[2])))
                            {
                                vertices = h->vertices + INDEX_H(h, x + i*num_tiles, y + patch_width-num_tiles);
                                sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                                GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                                (1<<(subdiv[0]-subdiv[1])) + 3, indices, vertices);
                            }
                        }
                        indices += (1<<(subdiv[0]-subdiv[1])) + 3;
                    }
                    //right
                    if (subdiv[0] > subdiv[2])
                    {
                        indices[0] = INDEX_H(h, 0, 0);
                        j = 1;
                        for (i = 0; i < 1<<(subdiv[0]-subdiv[2]); i++)
                        {
                            indices[j] = INDEX_H(h, num_tiles, i*num_tiles/(1<<(subdiv[0]-subdiv[2])));
                            j += 1;
                        }
                        indices[j] = INDEX_H(h, num_tiles, num_tiles);
                        indices[j+1] = INDEX_H(h, 0, num_tiles);
                        for (i = 0; i < lod_width; i++)
                        {
                            if (!((i == 0 && subdiv[0] > subdiv[3]) || (i == lod_width-1 && subdiv[0] > subdiv[1])))
                            {
                                vertices = h->vertices + INDEX_H(h, x + patch_width-num_tiles, y + i*num_tiles);
                                sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                                GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                                (1<<(subdiv[0]-subdiv[2])) + 3, indices, vertices);
                            }
                        }
                        indices += (1<<(subdiv[0]-subdiv[2])) + 3;
                    }
                    //down
                    if (subdiv[0] > subdiv[3])
                    {
                        indices[0] = INDEX_H(h, 0, num_tiles);
                        j = 1;
                        for (i = 0; i < 1<<(subdiv[0]-subdiv[3]); i++)
                        {
                            indices[j] = INDEX_H(h, i*num_tiles/(1<<(subdiv[0]-subdiv[3])), 0);
                            j += 1;
                        }
                        indices[j] = INDEX_H(h, num_tiles, 0);
                        indices[j+1] = INDEX_H(h, num_tiles, num_tiles);
                        for (i = 0; i < lod_width; i++)
                        {
                            if (!((i == 0 && subdiv[0] > subdiv[4]) || (i == lod_width-1 && subdiv[0] > subdiv[2])))
                            {
                                vertices = h->vertices + INDEX_H(h, x + i*num_tiles, y);
                                sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                                GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                                (1<<(subdiv[0]-subdiv[3])) + 3, indices, vertices);
                            }
                        }
                        indices += (1<<(subdiv[0]-subdiv[3])) + 3;
                    }
                    //left
                    if (subdiv[0] > subdiv[4])
                    {
                        indices[0] = INDEX_H(h, num_tiles, num_tiles);
                        j = 1;
                        for (i = 0; i < 1<<(subdiv[0]-subdiv[4]); i++)
                        {
                            indices[j] = INDEX_H(h, 0, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[4])));
                            j += 1;
                        }
                        indices[j] = INDEX_H(h, 0, 0);
                        indices[j+1] = INDEX_H(h, num_tiles, 0);
                        for (i = 0; i < lod_width; i++)
                        {
                            if (!((i == 0 && subdiv[0] > subdiv[3]) || (i == lod_width-1 && subdiv[0] > subdiv[1])))
                            {
                                vertices = h->vertices + INDEX_H(h, x, y + i*num_tiles);
                                sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                                GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                                (1<<(subdiv[0]-subdiv[4])) + 3, indices, vertices);
                            }
                        }
                        indices += (1<<(subdiv[0]-subdiv[4])) + 3;
                    }
                    //last "corner" fans here...
                    if (subdiv[0] > subdiv[2])
                    {
                        //top right
                        if (subdiv[0] > subdiv[1])
                        {
                            indices[0] = INDEX_H(h, 0, 0);
                            j = 1;
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[2]); i++)
                            {
                                indices[j] = INDEX_H(h, num_tiles, i*num_tiles/(1<<(subdiv[0]-subdiv[2])));
                                j += 1;
                            }
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[1]); i++)
                            {
                                indices[j] = INDEX_H(h, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[1])), num_tiles);
                                j += 1;
                            }
                            indices[j] = INDEX_H(h, 0, num_tiles);
                            vertices = h->vertices + INDEX_H(h, x + (lod_width-1)*num_tiles, y + (lod_width-1)*num_tiles);
                            sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                            GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                            (1<<(subdiv[0]-subdiv[2])) + (1<<(subdiv[0]-subdiv[1])) + 2, indices, vertices);
                            indices += (1<<(subdiv[0]-subdiv[2])) + (1<<(subdiv[0]-subdiv[1])) + 2;
                        }
                        //bottom right
                        if (subdiv[0] > subdiv[3])
                        {
                            indices[0] = INDEX_H(h, 0, num_tiles);
                            j = 1;
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[3]); i++)
                            {
                                indices[j] = INDEX_H(h, i*num_tiles/(1<<(subdiv[0]-subdiv[3])), 0);
                                j += 1;
                            }
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[2]); i++)
                            {
                                indices[j] = INDEX_H(h, num_tiles, i*num_tiles/(1<<(subdiv[0]-subdiv[2])));
                                j += 1;
                            }
                            indices[j] = INDEX_H(h, num_tiles, num_tiles);
                            vertices = h->vertices + INDEX_H(h, x + (lod_width-1)*num_tiles, y);
                            sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                            GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                            (1<<(subdiv[0]-subdiv[3])) + (1<<(subdiv[0]-subdiv[2])) + 2, indices, vertices);
                            indices += (1<<(subdiv[0]-subdiv[3])) + (1<<(subdiv[0]-subdiv[2])) + 2;
                        }
                    }
                    if (subdiv[0] > subdiv[4])
                    {
                        //bottom left
                        if (subdiv[0] > subdiv[3])
                        {
                            indices[0] = INDEX_H(h, num_tiles, num_tiles);
                            j = 1;
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[4]); i++)
                            {
                                indices[j] = INDEX_H(h, 0, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[4])));
                                j += 1;
                            }
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[3]); i++)
                            {
                                indices[j] = INDEX_H(h, i*num_tiles/(1<<(subdiv[0]-subdiv[3])), 0);
                                j += 1;
                            }
                            indices[j] = INDEX_H(h, num_tiles, 0);
                            vertices = h->vertices + INDEX_H(h, x, y);
                            sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                            GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                            (1<<(subdiv[0]-subdiv[4])) + (1<<(subdiv[0]-subdiv[3])) + 2, indices, vertices);
                            indices += (1<<(subdiv[0]-subdiv[4])) + (1<<(subdiv[0]-subdiv[3])) + 2;
                        }
                        //top left
                        if (subdiv[0] > subdiv[1])
                        {
                            indices[0] = INDEX_H(h, num_tiles, 0);
                            j = 1;
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[1]); i++)
                            {
                                indices[j] = INDEX_H(h, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[1])), num_tiles);
                                j += 1;
                            }
                            for (i = 0; i < 1<<(subdiv[0]-subdiv[4]); i++)
                            {
                                indices[j] = INDEX_H(h, 0, num_tiles - i*num_tiles/(1<<(subdiv[0]-subdiv[4])));
                                j += 1;
                            }
                            indices[j] = INDEX_H(h, 0, 0);
                            vertices = h->vertices + INDEX_H(h, x, y + (lod_width-1)*num_tiles);
                            sceGuDrawArray((DRAW_PRIM == GU_TRIANGLE_STRIP ? GU_TRIANGLE_FAN : DRAW_PRIM),
                                            GU_TEXTURE_16BIT|GU_VERTEX_32BITF|GU_INDEX_16BIT|GU_TRANSFORM_3D,
                                            (1<<(subdiv[0]-subdiv[1])) + (1<<(subdiv[0]-subdiv[4])) + 2, indices, vertices);
                            indices += (1<<(subdiv[0]-subdiv[1])) + (1<<(subdiv[0]-subdiv[4])) + 2;
                        }
                    }
                }
			}
			sceGuEndObject();
			l->list_ptrs[index] = list;
			index += 1;
			list += sceGuFinish();
			sceGuSync(0, 2);
        }
    }
	l->ready = 1;
}

void xHeightmapDrawLOD(xHeightmapLOD* l)
{
	if (!l) return;
	if (!l->h || !l->ready) return;
	if (!l->indices_buffer || !l->list_buffer || !l->h->vertices) return;
	sceGuFrontFace(GU_CCW);
	sceGumUpdateMatrix();
	int i;
	for (i = 0; i < l->patches*l->patches; i++)
	{
		sceGuCallList(l->list_ptrs[i]);
	}
	sceGuFrontFace(GU_CW);
}

int xHeightmapGetPatchID(xHeightmapLOD* l, float x, float y)
{
	if (!l) return -1;
	if (!l->h) return -1;
	return (int)(y/(l->patch_width*l->h->tile_scale))*l->patches + (int)(x/(l->patch_width*l->h->tile_scale));
}

void xHeightmapDrawPatchID(xHeightmapLOD* l, int id)
{
	if (!l || id < 0) return;
	if (!l->h) return;
	sceGuFrontFace(GU_CCW);
	sceGumUpdateMatrix();
	sceGuCallList(l->list_ptrs[id]);
	sceGuFrontFace(GU_CW);
}
