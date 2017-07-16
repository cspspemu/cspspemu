/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <math.h>
#include <stdio.h>
#include <string.h>
#include <pspgu.h>
#include <pspgum.h>
#include <pspkernel.h>
#include "xmem.h"
#include "xmath.h"
#include "xgraphics.h"

#include "xobj.h"

#define OBJ_TNV_UNOPT	0
#define OBJ_TV_UNOPT	1
#define OBJ_NV_UNOPT	2
#define OBJ_V_UNOPT		3
#define OBJ_TNV_OPT		4
#define OBJ_TV_OPT		5
#define OBJ_NV_OPT		6
#define OBJ_V_OPT		7

typedef struct {
	float u, v;
	float nx, ny, nz;
	float x, y, z;
} tnv_unopt_vertex;

typedef struct {
	float u, v;
	float x, y, z;
} tv_unopt_vertex;

typedef struct {
	float nx, ny, nz;
	float x, y, z;
} nv_unopt_vertex;

typedef struct {
	float x, y, z;
} v_unopt_vertex;

typedef struct {
	s16 u, v;
	s8 nx, ny, nz;
	s16 x, y, z;
} tnv_opt_vertex;

typedef struct {
	s16 u, v;
	s16 x, y, z;
} tv_opt_vertex;

typedef struct {
	s8 nx, ny, nz;
	s16 x, y, z;
} nv_opt_vertex;

typedef struct {
	s16 x, y, z;
} v_opt_vertex;

#define FLOAT_TO_S16(x) ((s16)(32767*(x)))
#define FLOAT_TO_S8(x) ((s8)(127*(x)))

static int obj_optimize(xObj* object)
{
	X_LOG("Attempting to optimize OBJ mesh...");
    if (object == NULL) return 1;
	if (object->type >= OBJ_TNV_OPT) return 1;

	int i;
	if (object->type == OBJ_TNV_UNOPT)
	{
		tnv_unopt_vertex* old_vertices = object->vertices;
		tnv_opt_vertex* new_vertices = (tnv_opt_vertex*)x_malloc(object->num_verts*sizeof(tnv_opt_vertex));
		for (i = 0; i < object->num_verts; i++)
		{
			new_vertices[i].u = FLOAT_TO_S16(old_vertices[i].u);
			new_vertices[i].v = FLOAT_TO_S16(old_vertices[i].v);
			new_vertices[i].nx = FLOAT_TO_S8(old_vertices[i].nx);
			new_vertices[i].ny = FLOAT_TO_S8(old_vertices[i].ny);
			new_vertices[i].nz = FLOAT_TO_S8(old_vertices[i].nz);
			new_vertices[i].x = FLOAT_TO_S16(old_vertices[i].x/object->scale.x);
			new_vertices[i].y = FLOAT_TO_S16(old_vertices[i].y/object->scale.y);
			new_vertices[i].z = FLOAT_TO_S16(old_vertices[i].z/object->scale.z);
		}
		object->type = OBJ_TNV_OPT;
		object->vtype = GU_TEXTURE_16BIT|GU_NORMAL_8BIT|GU_VERTEX_16BIT;
		x_free(object->vertices);
		object->vertices = new_vertices;
	}
	else if (object->type == OBJ_TV_UNOPT)
	{
		tv_unopt_vertex* old_vertices = object->vertices;
		tv_opt_vertex* new_vertices = (tv_opt_vertex*)x_malloc(object->num_verts*sizeof(tv_opt_vertex));
		for (i = 0; i < object->num_verts; i++)
		{
			new_vertices[i].u = FLOAT_TO_S16(old_vertices[i].u);
			new_vertices[i].v = FLOAT_TO_S16(old_vertices[i].v);
			new_vertices[i].x = FLOAT_TO_S16(old_vertices[i].x/object->scale.x);
			new_vertices[i].y = FLOAT_TO_S16(old_vertices[i].y/object->scale.y);
			new_vertices[i].z = FLOAT_TO_S16(old_vertices[i].z/object->scale.z);
		}
		object->type = OBJ_TV_OPT;
		object->vtype = GU_TEXTURE_16BIT|GU_VERTEX_16BIT;
		x_free(object->vertices);
		object->vertices = new_vertices;
	}
	else if (object->type == OBJ_NV_UNOPT)
	{
		nv_unopt_vertex* old_vertices = object->vertices;
		nv_opt_vertex* new_vertices = (nv_opt_vertex*)x_malloc(object->num_verts*sizeof(nv_opt_vertex));
		for (i = 0; i < object->num_verts; i++)
		{
			new_vertices[i].nx = FLOAT_TO_S8(old_vertices[i].nx);
			new_vertices[i].ny = FLOAT_TO_S8(old_vertices[i].ny);
			new_vertices[i].nz = FLOAT_TO_S8(old_vertices[i].nz);
			new_vertices[i].x = FLOAT_TO_S16(old_vertices[i].x/object->scale.x);
			new_vertices[i].y = FLOAT_TO_S16(old_vertices[i].y/object->scale.y);
			new_vertices[i].z = FLOAT_TO_S16(old_vertices[i].z/object->scale.z);
		}
		object->type = OBJ_NV_OPT;
		object->vtype = GU_NORMAL_8BIT|GU_VERTEX_16BIT;
		x_free(object->vertices);
		object->vertices = new_vertices;
	}
	else //if (object->type == OBJ_V_UNOPT)
	{
		v_unopt_vertex* old_vertices = object->vertices;
		v_opt_vertex* new_vertices = (v_opt_vertex*)x_malloc(object->num_verts*sizeof(v_opt_vertex));
		for (i = 0; i < object->num_verts; i++)
		{
			new_vertices[i].x = FLOAT_TO_S16(old_vertices[i].x/object->scale.x);
			new_vertices[i].y = FLOAT_TO_S16(old_vertices[i].y/object->scale.y);
			new_vertices[i].z = FLOAT_TO_S16(old_vertices[i].z/object->scale.z);
		}
		object->type = OBJ_V_OPT;
		object->vtype = GU_VERTEX_16BIT;
		x_free(object->vertices);
		object->vertices = new_vertices;
	}

	X_LOG("Successfully optimized OBJ mesh.");
    return 0;
}

#define MAX(X,Y) ((X) > (Y) ? (X) : (Y))

xObj* xObjLoad(char* filename, int optimize)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load OBJ mesh \"%s\"...", filename);
    xObj* object = (xObj*)x_malloc(sizeof(xObj));
    if (object == NULL) return NULL;
	object->vertices = NULL;
	object->pos.x = 0.0f;
	object->pos.y = 0.0f;
	object->pos.z = 0.0f;
    FILE* file = fopen(filename, "r");
    if (file == NULL)
	{
        xObjFree(object);
        return NULL;
    }
    
    int i;
    char buffer[256];
    int num_texcoords = 0;
    int num_normals = 0;
    int num_verts = 0;
    int num_tris = 0;

    while(!feof(file))
    {
        fgets(buffer, sizeof(buffer), file);
        if (strncmp(buffer, "vt", 2) == 0)
        {
            num_texcoords += 1;
        }
        else if (strncmp(buffer, "vn", 2) == 0)
        {
            num_normals += 1;
        }
        else if (strncmp(buffer, "vp", 2) == 0)
        {
            //unsupported
        }
        else if (strncmp(buffer, "v", 1) == 0)
        {
            num_verts += 1;
        }
        else if (strncmp(buffer, "f", 1) == 0)
        {
            num_tris += 1;
        }
    }

	X_LOG("OBJ: %i triangles, %i vertices, %i tex coords, %i normals", num_tris, num_verts, num_texcoords, num_normals);

	if (num_verts <= 0 || num_tris <= 0)
	{
		fclose(file);
		xObjFree(object);
		return NULL;
	}
    
    ScePspFVector2* texcoords = (ScePspFVector2*)x_malloc(num_texcoords*sizeof(ScePspFVector2));
    ScePspFVector3* normals = (ScePspFVector3*)x_malloc(num_normals*sizeof(ScePspFVector3));
    ScePspFVector3* vertices = (ScePspFVector3*)x_malloc(num_verts*sizeof(ScePspFVector3));
	if ((num_texcoords > 0 && texcoords == NULL) || (num_normals > 0 && normals == NULL) || (num_verts > 0 && vertices == NULL))
	{
		fclose(file);
		if (texcoords != NULL) x_free(texcoords);
		if (normals != NULL) x_free(normals);
		if (vertices != NULL) x_free(vertices);
		xObjFree(object);
		return NULL;
	}
    
	int size = 0;
    if (num_texcoords > 0 && num_normals > 0)
    {
		object->type = OBJ_TNV_UNOPT;
		object->vtype = GU_TEXTURE_32BITF|GU_NORMAL_32BITF|GU_VERTEX_32BITF;
		size = sizeof(tnv_unopt_vertex);
	}
	else if (num_texcoords > 0)
	{
		object->type = OBJ_TV_UNOPT;
		object->vtype = GU_TEXTURE_32BITF|GU_VERTEX_32BITF;
		size = sizeof(tv_unopt_vertex);
	}
	else if (num_normals > 0)
	{
		object->type = OBJ_NV_UNOPT;
		object->vtype = GU_NORMAL_32BITF|GU_VERTEX_32BITF;
		size = sizeof(nv_unopt_vertex);
	}
    else
	{
		object->type = OBJ_V_UNOPT;
		object->vtype = GU_VERTEX_32BITF;
		size = sizeof(v_unopt_vertex);
    }
    
    object->num_verts = num_tris*3;
    object->vertices = x_malloc(object->num_verts*size);
	if (object->vertices == NULL)
	{
		fclose(file);
		x_free(texcoords);
		x_free(normals);
		x_free(vertices);
		xObjFree(object);
		return NULL;
	}

    int index_texcoords = 0;
    int index_normals = 0;
    int index_verts = 0;
    int indices[9];
    void* ptr = object->vertices;

	ScePspFVector3 min = {HUGE_VAL, HUGE_VAL, HUGE_VAL};
	ScePspFVector3 max = {-HUGE_VAL, -HUGE_VAL, -HUGE_VAL};
	
    rewind(file);

    while(!feof(file))
    {
        fgets(buffer, sizeof(buffer), file);
        if (strncmp(buffer, "vt", 2) == 0)
        {
            sscanf(buffer, "vt %f %f", &texcoords[index_texcoords].x, &texcoords[index_texcoords].y);
            index_texcoords += 1;
        }
        else if (strncmp(buffer, "vn", 2) == 0)
        {
            sscanf(buffer, "vn %f %f %f", &normals[index_normals].x, &normals[index_normals].y, &normals[index_normals].z);
            x_normalize(&normals[index_normals], 1.0f);
            index_normals += 1;
        }
        else if (strncmp(buffer, "vp", 2) == 0)
        {
            //unsupported
        }
        else if (strncmp(buffer, "v", 1) == 0)
        {
            sscanf(buffer, "v %f %f %f", &vertices[index_verts].x, &vertices[index_verts].y, &vertices[index_verts].z);
            index_verts += 1;
        }
        else if (strncmp(buffer, "f", 1) == 0)
        {
            if (object->type == OBJ_V_UNOPT)
            {
                sscanf(buffer, "f %i %i %i", &indices[0], &indices[1], &indices[2]);
                for (i = 0; i < 3; i++)
                {
					v_unopt_vertex* verts = ptr;
					verts->x = vertices[indices[i]-1].x;
					verts->y = vertices[indices[i]-1].y;
					verts->z = vertices[indices[i]-1].z;
					if (verts->x < min.x) min.x = verts->x;
					if (verts->x > max.x) max.x = verts->x;
					if (verts->y < min.y) min.y = verts->y;
					if (verts->y > max.y) max.y = verts->y;
					if (verts->z < min.z) min.z = verts->z;
					if (verts->z > max.z) max.z = verts->z;
                    ptr += size;
                }
            }
            else if (object->type == OBJ_TV_UNOPT)
			{
				sscanf(buffer, "f %i/%i %i/%i %i/%i", &indices[0], &indices[1], &indices[2], &indices[3], &indices[4], &indices[5]);
				for (i = 0; i < 3; i++)
				{
					tv_unopt_vertex* verts = ptr;
					verts->u = texcoords[indices[i*2+1]-1].x;
					verts->v = texcoords[indices[i*2+1]-1].y;
					verts->x = vertices[indices[i*2]-1].x;
					verts->y = vertices[indices[i*2]-1].y;
					verts->z = vertices[indices[i*2]-1].z;
					if (verts->x < min.x) min.x = verts->x;
					if (verts->x > max.x) max.x = verts->x;
					if (verts->y < min.y) min.y = verts->y;
					if (verts->y > max.y) max.y = verts->y;
					if (verts->z < min.z) min.z = verts->z;
					if (verts->z > max.z) max.z = verts->z;
					ptr += size;
				}
            }
			else if (object->type == OBJ_NV_UNOPT)
			{
				sscanf(buffer, "f %i//%i %i//%i %i//%i", &indices[0], &indices[1], &indices[2], &indices[3], &indices[4], &indices[5]);
				for (i = 0; i < 3; i++)
				{
					nv_unopt_vertex* verts = ptr;
					verts->nx = normals[indices[i*2+1]-1].x;
					verts->ny = normals[indices[i*2+1]-1].y;
					verts->nz = normals[indices[i*2+1]-1].z;
					verts->x = vertices[indices[i*2]-1].x;
					verts->y = vertices[indices[i*2]-1].y;
					verts->z = vertices[indices[i*2]-1].z;
					if (verts->x < min.x) min.x = verts->x;
					if (verts->x > max.x) max.x = verts->x;
					if (verts->y < min.y) min.y = verts->y;
					if (verts->y > max.y) max.y = verts->y;
					if (verts->z < min.z) min.z = verts->z;
					if (verts->z > max.z) max.z = verts->z;
					ptr += size;
				}
			}
			else if (object->type == OBJ_TNV_UNOPT)
			{
				sscanf(buffer, "f %i/%i/%i %i/%i/%i %i/%i/%i",
					&indices[0], &indices[1], &indices[2],
					&indices[3], &indices[4], &indices[5],
					&indices[6], &indices[7], &indices[8]);
				for (i = 0; i < 3; i++)
				{
					tnv_unopt_vertex* verts = ptr;
					verts->u = texcoords[indices[i*3+1]-1].x;
					verts->v = texcoords[indices[i*3+1]-1].y;
					verts->nx = normals[indices[i*3+2]-1].x;
					verts->ny = normals[indices[i*3+2]-1].y;
					verts->nz = normals[indices[i*3+2]-1].z;
					verts->x = vertices[indices[i*3+0]-1].x;
					verts->y = vertices[indices[i*3+0]-1].y;
					verts->z = vertices[indices[i*3+0]-1].z;
					if (verts->x < min.x) min.x = verts->x;
					if (verts->x > max.x) max.x = verts->x;
					if (verts->y < min.y) min.y = verts->y;
					if (verts->y > max.y) max.y = verts->y;
					if (verts->z < min.z) min.z = verts->z;
					if (verts->z > max.z) max.z = verts->z;
					ptr += size;
				}
			}
        }
		else if (strncmp(buffer, "pos", 1) == 0)
		{
			sscanf(buffer, "pos %f %f %f", &object->pos.x, &object->pos.y, &object->pos.z);
		}
		else
		{
			//unsupported
		}
    }

	x_free(vertices);
	x_free(normals);
	x_free(texcoords);

	object->bbox[0].x = min.x; object->bbox[0].y = min.y; object->bbox[0].z = min.z;
	object->bbox[1].x = max.x; object->bbox[1].y = min.y; object->bbox[1].z = min.z;
	object->bbox[2].x = min.x; object->bbox[2].y = max.y; object->bbox[2].z = min.z;
	object->bbox[3].x = min.x; object->bbox[3].y = min.y; object->bbox[3].z = max.z;
	object->bbox[4].x = max.x; object->bbox[4].y = max.y; object->bbox[4].z = min.z;
	object->bbox[5].x = min.x; object->bbox[5].y = max.y; object->bbox[5].z = max.z;
	object->bbox[6].x = max.x; object->bbox[6].y = min.y; object->bbox[6].z = max.z;
	object->bbox[7].x = max.x; object->bbox[7].y = max.y; object->bbox[7].z = max.z;

	object->scale.x = MAX(x_absf(min.x), x_absf(max.x));
	object->scale.y = MAX(x_absf(min.y), x_absf(max.y));
	object->scale.z = MAX(x_absf(min.z), x_absf(max.z));

	if (optimize)
	{
		obj_optimize(object);
	}

	sceKernelDcacheWritebackAll();

    fclose(file);
	
	X_LOG("Successfully loaded OBJ mesh.");
    return object;
}

void xObjFree(xObj* object)
{
	X_LOG("Freeing OBJ mesh.");
	if (!object) return;
    if (object->vertices) x_free(object->vertices);
    x_free(object);
}

void xObjTranslate(xObj* object)
{
	if (!object) return;
	sceGumTranslate(&object->pos);
}

void xObjDraw(xObj* object, int reverse_frontface)
{
	if (object == NULL) return;
	sceGumUpdateMatrix();
	sceGuBeginObject(GU_VERTEX_32BITF, 8, 0, object->bbox);
	sceGumPushMatrix();
	if (object->type >= OBJ_TNV_OPT)
		sceGumScale(&object->scale);
    if (!reverse_frontface)
		sceGuFrontFace(GU_CCW);
	sceGumDrawArray(GU_TRIANGLES, object->vtype|GU_TRANSFORM_3D, object->num_verts, 0, object->vertices);
	if (!reverse_frontface)
		sceGuFrontFace(GU_CW);
	sceGumPopMatrix();
	sceGuEndObject();
}
