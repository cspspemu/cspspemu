/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_HEIGHTMAP__
#define __X_HEIGHTMAP__

/* TODO:
	change to a better OO design...
*/

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct hmap_vertex {
    s16 u, v;
    ScePspFVector3 p;
} hmap_vertex;

typedef struct hmap_bbox {
	ScePspFVector3 p[8];
} hmap_bbox;

typedef struct xHeightmap {
    int width;
	int height;
    float tile_scale;
	float z_scale;
    hmap_vertex* vertices; // width*height
    u16* indices_static; //2*width
} xHeightmap;

typedef struct xHeightmapLOD {
	xHeightmap* h;
	u16* indices_buffer; //2*width*(height-1) + 8*patches*patches
	void* list_buffer; //patches*patches*LIST_SIZE
	void** list_ptrs;
	hmap_bbox* bboxes; //patches*patches
	int patches;
	int patch_width;
	int ready;
} xHeightmapLOD;

//loads raw 16-bit height data, little endian, stored bottom to top
int xHeightmapLoad(xHeightmap* h, char* filename, int width, float tile_scale, float zscale);

void xHeightmapFree(xHeightmap* h);

//min_level = log2(patches)
int xHeightmapSetupLOD(xHeightmapLOD* l, xHeightmap* h, int min_level);

void xHeightmapFreeLOD(xHeightmapLOD* l);

void xHeightmapGetNormal(xHeightmap* h, ScePspFVector3* out, float x, float y);

float xHeightmapGetHeight(xHeightmap* h, ScePspFVector3* normal, float x, float y);

void xHeightmapDraw(xHeightmap* h);

//p = viewpoint, start = lod start distance, spacing = lod spacing
void xHeightmapBuildLOD(xHeightmapLOD* l, ScePspFVector3* p, float start, float spacing);

/* only x and y of p are used */
void xHeightmapDrawLOD(xHeightmapLOD* l);

int xHeightmapGetPatchID(xHeightmapLOD* l, float x, float y);

void xHeightmapDrawPatchID(xHeightmapLOD* l, int id);

//not for you!
ScePspFVector3* point_from_grid(xHeightmap* h, int x, int y);

void normal_from_grid(xHeightmap* h, ScePspFVector3* out, int x, int y, int which);

#ifdef __cplusplus
}
#endif

#endif
