/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <pspgu.h>
#include <pspgum.h>
#include "xgraphics.h"
#include "xmath.h"
#include "xmem.h"

#include "xparticle.h"

xParticleSystem* xParticleSystemConstruct(u16 max)
{
    xParticleSystem* s = (xParticleSystem*)x_malloc(sizeof(xParticleSystem));
    if (!s) return 0;
    s->particles = (xParticle*)x_malloc(max*sizeof(xParticle));
    s->particle_stack = (u16*)x_malloc(max*sizeof(u16));
    if (!s->particles || !s->particle_stack)
    {
        xParticleSystemDestroy(s);
        return 0;
    }
    
    xVec3Set(&s->pos, 0.0f, 0.0f, 0.0f);
    xVec3Set(&s->pos_rand, 0.0f, 0.0f, 0.0f);
    xVec3Set(&s->vel, 0.0f, 0.0f, 0.0f);
    xVec3Set(&s->vel_rand, 0.0f, 0.0f, 0.0f);
    xVec3Set(&s->accel, 0.0f, 0.0f, 0.0f);
    xCol4Set(&s->colors[0], 1.0f, 1.0f, 1.0f, 1.0f);
    xCol4Set(&s->colors[1], 1.0f, 1.0f, 1.0f, 0.0f);
	s->num_cols = 0;
	s->sizes[0] = 1.0f;
	s->sizes[1] = 1.0f;
    s->num_sizes = 1;
    s->size_rand = 0.0f;
    s->life = 1.0f;
    s->life_rand = 0.0f;
	s->friction = 0.0f;
    s->rate = 1;
    s->prim = X_PARTICLE_SPRITES;
    s->time = 0.0f;
    s->num_particles = 0;
    s->max_particles = max;
	s->render = 0;
    
    int i;
    for (i = 0; i < s->max_particles; i++)
    {
        s->particle_stack[i] = i;
    }
    
    return s;
}

void xParticleSystemDestroy(xParticleSystem* s)
{
    if (!s) return;
    if (s->particles) x_free(s->particles);
	if (s->particle_stack) x_free(s->particle_stack);
    x_free(s);
}

static void remove_particle(xParticleSystem* s, u16 idx)
{
    if (s->num_particles <= 0) return;
    s->num_particles -= 1;
    if (s->num_particles == idx) return;
    u16 temp = s->particle_stack[s->num_particles];
    s->particle_stack[s->num_particles] = s->particle_stack[idx];
    s->particle_stack[idx] = temp;
}

static void create_particle(xParticleSystem* s)
{
    if (s->num_particles >= s->max_particles)
        return;
    xParticle* p = &s->particles[s->particle_stack[s->num_particles]];
    p->pos = s->pos;
    p->pos.x += x_randf(-s->pos_rand.x, s->pos_rand.x);
    p->pos.y += x_randf(-s->pos_rand.y, s->pos_rand.y);
    p->pos.z += x_randf(-s->pos_rand.z, s->pos_rand.z);
    p->vel = s->vel;
    p->vel.x += x_randf(-s->vel_rand.x, s->vel_rand.x);
    p->vel.y += x_randf(-s->vel_rand.y, s->vel_rand.y);
    p->vel.z += x_randf(-s->vel_rand.z, s->vel_rand.z);
    p->size_rand = x_randf(-s->size_rand, s->size_rand);
    p->age = 0.0f;
    p->inv_life = 1.0f/(s->life + x_randf(-s->life_rand, s->life_rand));
    s->num_particles += 1;
}

void xParticleSystemBurst(xParticleSystem* s, xParticleEmitter* e, int num)
{
    if (s == NULL || e == NULL) return;
	s->pos = e->particle_pos;
	if (e->new_velocity)
		s->vel = e->particle_vel;
    while (num > 0)
    {
        create_particle(s);
        num -= 1;
    }
}

void xParticleSystemUpdate(xParticleSystem* s, float dt)
{
    if (!s) return;
    int i;
    xParticle* p;
    xVector3f temp_vec;
    for (i = 0; i < s->num_particles; i++)
    {
        p = &s->particles[s->particle_stack[i]];
        p->age += dt;
        if (p->age*p->inv_life >= 1.0f)
        {
            //particle is dead
            remove_particle(s, i);
            i -= 1;
        }
        else
        {
            //forces/friction/binding/loosen
			//d = v*t + (1/2)a*t^2
			xVec3Scale(&temp_vec, &s->accel, 0.5f*dt);
			xVec3Add(&temp_vec, &temp_vec, &p->vel);
			xVec3Scale(&temp_vec, &temp_vec, dt);
			xVec3Add(&p->pos, &p->pos, &temp_vec);
			xVec3Add(&p->vel, &p->vel, xVec3Scale(&temp_vec, &s->accel, dt));
			if (s->friction > 0.0f)
				xVec3Scale(&p->vel, &p->vel, (1.0f - s->friction*dt));
            //p->size += s->growth*dt;
        }
    }
    s->time += dt;
	if (s->rate > 0.0f)
	{
		float inv_rate = 1.0f/s->rate;
		while (s->time >= inv_rate)
		{
			create_particle(s);
			s->time -= inv_rate;
		}
	}
}

typedef struct {
	s8 u, v;
	u32 c;
    float x, y, z;
} sprite_vertex;

#define sprite_vertex_vtype GU_TEXTURE_8BIT|GU_COLOR_8888|GU_VERTEX_32BITF

typedef struct {
	u32 c;
    float x, y, z;
} primitive_vertex;

#define primitive_vertex_vtype GU_COLOR_8888|GU_VERTEX_32BITF

static inline u32 col4_to_8888(xColor4f* c)
{
    return GU_COLOR(c->r, c->g, c->b, c->a);
}

void xParticleSystemRender(xParticleSystem* s, ScePspFMatrix4* view)
{
    if (s == NULL || view == NULL) return;
	if (s->num_sizes <= 0) return;
    xGuSaveStates();
    sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_DITHER);
	sceGuDepthMask(GU_TRUE);
	sceGuShadeModel(GU_FLAT);

    xParticle* p;
    xColor4f color4f;
	u32 color32;
	float size;
	int i;
	if (s->render != 0)
	{
		for (i = 0; i < s->num_particles; i++)
		{
			s->render(s, &s->particles[s->particle_stack[i]], view);
		}
	}
	else
	{
		switch (s->prim)
		{
		case X_PARTICLE_SPRITES:
			{
				xVector3f up_left = {-view->x.x + view->y.x, -view->x.y + view->y.y, -view->x.z + view->y.z};
				sprite_vertex* vertices = (sprite_vertex*)sceGuGetMemory(2*s->num_particles*sizeof(sprite_vertex));
				for (i = 0; i < s->num_particles; i++)
				{
					p = &s->particles[s->particle_stack[i]];
					if (s->num_cols > 1)
					{
						float t = p->age * p->inv_life * (s->num_cols-1);
						u8 idx = (u8)t;
						t -= idx;
						color32 = col4_to_8888(xCol4Lerp(&color4f, &s->colors[idx], &s->colors[idx+1], t));
					}
					else if (s->num_cols == 1)
					{
						color32 = col4_to_8888(&s->colors[0]);
					}
					else
					{
						color32 = ((u8)(p->age*p->inv_life*255.0f) << 24) | 0x00ffffff;
					}
					if (s->num_sizes == 1)
					{
						size = s->sizes[0] + p->size_rand;
					}
					else
					{
						float t = p->age * p->inv_life * (s->num_sizes-1);
						u8 idx = (u8)t;
						t -= idx;
						size = s->sizes[idx] + t*(s->sizes[idx+1] - s->sizes[idx]);
					}
					xVector3f temp;
					xVec3Scale(&temp, &up_left, 0.5f*size);
					vertices[i*2+0].u = 0;
					vertices[i*2+0].v = 127;
					vertices[i*2+0].c = color32;
					vertices[i*2+0].x = p->pos.x + temp.x;
					vertices[i*2+0].y = p->pos.y + temp.y;
					vertices[i*2+0].z = p->pos.z + temp.z;
					vertices[i*2+1].u = 127;
					vertices[i*2+1].v = 0;
					vertices[i*2+1].c = color32;
					vertices[i*2+1].x = p->pos.x - temp.x;
					vertices[i*2+1].y = p->pos.y - temp.y;
					vertices[i*2+1].z = p->pos.z - temp.z;
				}
				sceGumDrawArray(GU_SPRITES, sprite_vertex_vtype|GU_TRANSFORM_3D, 2*s->num_particles, 0, vertices);
			}
			break;
		case X_PARTICLE_LINE_SPRITES:
			//
			break;
		case X_PARTICLE_LINES:
			{
				sceGuDisable(GU_TEXTURE_2D);
				primitive_vertex* vertices = (primitive_vertex*)sceGuGetMemory(2*s->num_particles*sizeof(primitive_vertex));
				for (i = 0; i < s->num_particles; i++)
				{
					p = &s->particles[s->particle_stack[i]];
					if (s->num_cols > 1)
					{
						float t = p->age * p->inv_life * (s->num_cols-1);
						u8 idx = (u8)t;
						t -= idx;
						color32 = col4_to_8888(xCol4Lerp(&color4f, &s->colors[idx], &s->colors[idx+1], t));
					}
					else if (s->num_cols == 1)
					{
						color32 = col4_to_8888(&s->colors[0]);
					}
					else
					{
						color32 = ((u8)(p->age*p->inv_life*255.0f) << 24) | 0x00ffffff;
					}
					vertices[i*2+0].c = color32;
					vertices[i*2+0].x = p->pos.x;
					vertices[i*2+0].y = p->pos.y;
					vertices[i*2+0].z = p->pos.z;
					vertices[i*2+1].c = color32;
					vertices[i*2+1].x = p->pos.x + p->vel.x;
					vertices[i*2+1].y = p->pos.y + p->vel.y;
					vertices[i*2+1].z = p->pos.z + p->vel.z;
				}
				sceGumDrawArray(GU_LINES, primitive_vertex_vtype|GU_TRANSFORM_3D, 2*s->num_particles, 0, vertices);
			}
			break;
		default: //X_PARTICLE_POINTS
			{
				sceGuDisable(GU_TEXTURE_2D);
				primitive_vertex* vertices = (primitive_vertex*)sceGuGetMemory(s->num_particles*sizeof(primitive_vertex));
				for (i = 0; i < s->num_particles; i++)
				{
					p = &s->particles[s->particle_stack[i]];
					if (s->num_cols > 1)
					{
						float t = p->age * p->inv_life * (s->num_cols-1);
						u8 idx = (u8)t;
						t -= idx;
						color32 = col4_to_8888(xCol4Lerp(&color4f, &s->colors[idx], &s->colors[idx+1], t));
					}
					else if (s->num_cols == 1)
					{
						color32 = col4_to_8888(&s->colors[0]);
					}
					else
					{
						color32 = ((u8)(p->age*p->inv_life*255.0f) << 24) | 0x00ffffff;
					}
					vertices[i].c = color32;
					vertices[i].x = p->pos.x;
					vertices[i].y = p->pos.y;
					vertices[i].z = p->pos.z;
				}
				sceGumDrawArray(GU_POINTS, primitive_vertex_vtype|GU_TRANSFORM_3D, s->num_particles, 0, vertices);
			}
			break;
		}
	}
    
	sceGuShadeModel(GU_SMOOTH);
    sceGuDepthMask(GU_FALSE);
    xGuLoadStates();
}
