/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_MATH_H__
#define __X_MATH_H__

#include "xconfig.h"
#include <math.h>

#ifdef __cplusplus
extern "C" {
#endif

#define X_EPSILON 1e-6

#define DEG_TO_RAD(DEG) ((DEG)*M_PI/180)
#define RAD_TO_DEG(RAD) ((RAD)*180/M_PI)
#define SQR(X) ((X)*(X))

/* possibly add an error variable to see if a domain or other error occurred */

void  x_srand(u32 s);
void  x_auto_srand();
int   x_randi(int min, int max);
float x_randf(float min, float max);
float x_sqrtf(float x);
float x_modf(float x, float y);
float x_sinf(float x);
float x_cosf(float x);
float x_tanf(float x);
float x_asinf(float x);
float x_acosf(float x);
float x_atanf(float x);
float x_atan2f(float y, float x);
float x_sinhf(float x);
float x_coshf(float x);
float x_tanhf(float x);
void  x_sincos(float rad, float* sin, float* cos);
float x_expf(float x);
float x_log10f(float x);
float x_logbf(float b, float x);
float x_logef(float x);
float x_powf(float x, float pow);
int   x_next_pow2(int x);
int   x_num_align(int x, int num);
int   x_absi(int x);
float x_absf(float x);
int   x_mini(int x, int y);
float x_minf(float x, float y);
int   x_maxi(int x, int y);
float x_maxf(float x, float y);
float x_floorf(float x);
float x_ceilf(float x);
float x_ipart(float x);
float x_fpart(float x);
float x_roundf(float x);
float x_angle_to_target(float eye_x, float eye_y, float target_x, float target_y);

void  x_lerp(ScePspFVector3* r, ScePspFVector3* v0, ScePspFVector3* v1, float t);
float x_magnitude(ScePspFVector3* v);
void  x_normalize(ScePspFVector3* v, float mag);
float x_dotproduct(ScePspFVector3* v0, ScePspFVector3* v1);
void  x_crossproduct(ScePspFVector3* r, ScePspFVector3* v0, ScePspFVector3* v1);
void  x_normal(ScePspFVector3* r, ScePspFVector3* p1, ScePspFVector3* p2, ScePspFVector3* p3);
void  x_translate(ScePspFVector3* v, ScePspFVector3* trans);
void  x_rotatex(ScePspFVector3* v, float x);
void  x_rotatey(ScePspFVector3* v, float x);
void  x_rotatez(ScePspFVector3* v, float x);
void  x_billboard(ScePspFVector3* r, ScePspFVector3* pos, ScePspFMatrix4* view_mat);
void  x_billboard_dir(ScePspFVector3* r, ScePspFVector3* eye, ScePspFVector3* pos, ScePspFVector3* dir);

float x_ease_to_target(float cur, float target, float p, float dt);
void  x_ease_to_target2(float* cur_x, float* cur_y, float target_x, float target_y, float p, float dt);
void  x_ease_to_target3(ScePspFVector3* cur, ScePspFVector3* target, float p, float dt);

float x_dist2(float x1, float y1, float x2, float y2);
float x_dist3(ScePspFVector3* p1, ScePspFVector3* p2);
int   x_dist_test2(float x1, float y1, float x2, float y2, float d);
int   x_dist_test3(ScePspFVector3* p1, ScePspFVector3* p2, float d);


xVector3f* xVec3Set(xVector3f* a, float x, float y, float z);
xVector3f* xVec3Add(xVector3f* r, xVector3f* a, xVector3f* b);
xVector3f* xVec3Sub(xVector3f* r, xVector3f* a, xVector3f* b);
xVector3f* xVec3Mul(xVector3f* r, xVector3f* a, xVector3f* b);
xVector3f* xVec3Div(xVector3f* r, xVector3f* a, xVector3f* b);
xVector3f* xVec3Scale(xVector3f* r, xVector3f* a, float s);
xVector3f* xVec3Normalize(xVector3f* r, xVector3f* a);
float xVec3Length(xVector3f* a);
float xVec3SqLength(xVector3f* a);
xVector3f* xVec3Lerp(xVector3f* r, xVector3f* a, xVector3f* b, float t);
float xVec3Dot(xVector3f* a, xVector3f* b);
xVector3f* xVec3Cross(xVector3f* r, xVector3f* a, xVector3f* b);

xColor4f* xCol4Set(xColor4f* c, float r, float g, float b, float a);
xColor4f* xCol4Lerp(xColor4f* r, xColor4f* a, xColor4f* b, float t);

#ifdef __cplusplus
}
#endif

#endif
