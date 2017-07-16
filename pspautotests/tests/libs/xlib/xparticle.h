/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_PARTICLE_H__
#define __X_PARTICLE_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

#define X_PARTICLE_SPRITES		(0)
#define X_PARTICLE_LINE_SPRITES (1)
#define X_PARTICLE_LINES        (2)
#define X_PARTICLE_POINTS       (3)

/* TODO:
	- particle trails
	- rotation
	- radial movement -- theta, r, omega, alpha -- rotates about axis using a rotation/position matrix
*/

typedef struct xParticle {
    xVector3f pos;
    xVector3f vel;
    float size_rand;
    float age;
    float inv_life;
} xParticle;

typedef struct xParticleSystem xParticleSystem;

typedef void (*xParticleRender)(xParticleSystem* s, xParticle* p, ScePspFMatrix4* view);

//not fully implemented yet
typedef struct xParticleEmitter {
	xVector3f particle_pos;
	xVector3f particle_vel;
	xVector3f emitter_pos;
	xVector3f emitter_vel;
	float age;
	float inv_life;
	int new_velocity;
} xParticleEmitter;

struct xParticleSystem {
    xVector3f pos;
    xVector3f pos_rand;
    xVector3f vel;
    xVector3f vel_rand;
    xVector3f accel;
	//xVector3f emitter_accel;
    xColor4f colors[8];		// colors - interpolates between num_cols colors during its life
    u16 num_cols;
	float sizes[8];			//sizes to interpolate between during the life of the particle
	u16 num_sizes;
	float size_rand;		//size variance
    float life;				// life between lifemax-life_rand and lifemax+life_rand
    float life_rand;
	float friction;			//air friction
    u16 rate;				// particles emitted per second
    int prim;				// see above. sprite, line sprite, line, or point
    //internal variables - do not edit
    float time;
    u16 num_particles;
    u16 max_particles;
    xParticle* particles;
    u16* particle_stack;
	xParticleRender render;
};

xParticleSystem* xParticleSystemConstruct(u16 max);

void xParticleSystemDestroy(xParticleSystem* s);

void xParticleSystemBurst(xParticleSystem* s, xParticleEmitter* e, int num);

void xParticleSystemUpdate(xParticleSystem* s, float dt);

void xParticleSystemRender(xParticleSystem* s, ScePspFMatrix4* view);

#ifdef __cplusplus
}
#endif

#endif
