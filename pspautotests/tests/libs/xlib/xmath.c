/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdio.h>
#include <psptypes.h>
#include <psprtc.h>

#include "xmath.h"

//#define ASM_SAFE

static SceKernelUtilsMt19937Context x_mt19937_ctx;

void x_srand(u32 s)
{
    sceKernelUtilsMt19937Init(&x_mt19937_ctx, s);
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %0, S000\n"
        "vrnds.s    S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        :: "r"(s)
    );
}

void x_auto_srand()
{
    u64 time;
    sceRtcGetCurrentTick(&time);
    x_srand(time);
}

int x_randi(int min, int max)
{
    return (min >= max ? min : min + sceKernelUtilsMt19937UInt(&x_mt19937_ctx) % (max - min + 1));
}

float x_randf(float min, float max)
{
    if (min >= max) return min;
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "mtv        %2, S001\n"
        "vsub.s     S001, S001, S000\n"
        "vone.s     S002\n"
        "vrndf1.s   S003\n"
        "vsub.s     S003, S003, S002\n"
        "vmul.s     S001, S003, S001\n"
        "vadd.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(min), "r"(max)
    );
    return result;
}

float x_sqrtf(float x)
{
    float result;
    #if 0
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vsqrt.s    S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    #else
    result = sqrtf(x);
    #endif
    return result;
}

float x_modf(float x, float y)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %2, S001\n"
        "mtv        %1, S000\n"
        "vrcp.s     S002, S001\n"
        "vmul.s     S003, S000, S002\n"
        "vf2iz.s    S002, S003, 0\n"
        "vi2f.s     S003, S002, 0\n"
        "vmul.s     S003, S003, S001\n"
        "vsub.s     S000, S000, S003\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x), "r"(y)
    );
    return result;
}

float x_sinf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_2_PI\n"
        "vmul.s     S000, S000, S001\n"
        "vsin.s     S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_cosf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_2_PI\n"
        "vmul.s     S000, S000, S001\n"
        "vcos.s     S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_tanf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_2_PI\n"
        "vmul.s     S000, S000, S001\n"
        "vrot.p     C002, S000, [s, c]\n"
        "vdiv.s     S000, S002, S003\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_asinf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_PI_2\n"
        "vasin.s    S000, S000\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_acosf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_PI_2\n"
        "vasin.s    S000, S000\n"
        "vocp.s     S000, S000\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_atanf(float x)
{
	float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vmul.s     S001, S000, S000\n"
        "vadd.s     S001, S001, S001[1]\n"
        "vrsq.s     S001, S001\n"
        "vmul.s     S000, S000, S001\n"
        "vasin.s    S000, S000\n"
        "vcst.s     S001, VFPU_PI_2\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_atan2f(float y, float x)
{
    if (x == 0 && y == 0) return 0;
    float result;
    if (fabsf(x) >= fabsf(y))
    {
        result = x_atanf(y/x);
        if (x < 0.0f) result += (y>=0.0f ? M_PI : -M_PI);
    }
    else
    {
        result = -x_atanf(x/y);
        result += (y < 0.0f ? -M_PI/2 : M_PI/2);
    }
    return result;
}

float x_sinhf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_LN2\n"
        "vrcp.s     S001, S001\n"
        "vmov.s     S002, S000[|x|]\n"
        "vcmp.s     NE, S000, S002\n"
        "vmul.s     S002, S001, S002\n"
        "vexp2.s    S002, S002\n"
        "vrcp.s     S003, S002\n"
        "vsub.s     S002, S002, S003\n"
        "vmul.s     S002, S002, S002[1/2]\n"
        "vcmov.s    S002, S002[-x], 0\n"
        "mfv        %0, S002\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_coshf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_LN2\n"
        "vrcp.s     S001, S001\n"
        "vmov.s     S002, S000[|x|]\n"
        "vmul.s     S002, S001, S002\n"
        "vexp2.s    S002, S002\n"
        "vrcp.s     S003, S002\n"
        "vadd.s     S002, S002, S003\n"
        "vmul.s     S002, S002, S002[1/2]\n"
        "mfv        %0, S002\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_tanhf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %0, S000\n"
        "vadd.s     S000, S000, S000\n"
        "vcst.s     S001, VFPU_LN2\n"
        "vrcp.s     S001, S001\n"
        "vmul.s     S000, S000, S001\n"
        "vexp2.s    S000, S000\n"
        "vone.s     S001\n"
        "vbfy1.p    C002, C000\n"
        "vdiv.s     S000, S003, S002\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

void x_sincos(float rad, float* sin, float* cos)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %2, S002\n"
        "vcst.s     S003, VFPU_2_PI\n"
        "vmul.s     S002, S002, S003\n"
        "vrot.p     C000, S002, [s, c]\n"
        "mfv        %0, S000\n"
        "mfv        %1, S001\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(*sin), "=r"(*cos)
        : "r"(rad)
    );
}

float x_expf(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_LN2\n"
        "vrcp.s     S001, S001\n"
        "vmul.s     S000, S000, S001\n"
        "vexp2.s    S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_log10f(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_LOG2TEN\n"
        "vrcp.s     S001, S001\n"
        "vlog2.s    S000, S000\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_logbf(float b, float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "mtv        %2, S001\n"
        "vlog2.s    S000, S000\n"
        "vlog2.s    S001, S001\n"
        "vrcp.s     S001, S001\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x), "r"(b)
    );
    return result;
}

float x_logef(float x)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "vcst.s     S001, VFPU_LOG2E\n"
        "vrcp.s     S001, S001\n"
        "vlog2.s    S000, S000\n"
        "vmul.s     S000, S000, S001\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x)
    );
    return result;
}

float x_powf(float x, float pow)
{
    float result;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %1, S000\n"
        "mtv        %2, S001\n"
        "vlog2.s    S001, S001\n"
        "vmul.s     S000, S000, S001\n"
        "vexp2.s    S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "r"(x), "r"(pow)
    );
    return result;
}

inline int x_next_pow2(int x)
{
    if (x <= 0) return 0;
	int pow2 = 1;
	while (pow2 < x) pow2 <<= 1;
	return pow2;
}

inline int x_num_align(int x, int num)
{
    return (x % num == 0 ? x : x + num - (x % num));
}

inline int x_absi(int x)
{
    return (x < 0 ? -x : x);
}

inline float x_absf(float x)
{
    return fabsf(x);
}

inline int x_mini(int x, int y)
{
    return (x < y ? x : y);
}

inline float x_minf(float x, float y)
{
    return (x < y ? x : y);
}

inline int x_maxi(int x, int y)
{
    return (x > y ? x : y);
}

inline float x_maxf(float x, float y)
{
    return (x > y ? x : y);
}

inline float x_floorf(float x)
{
    return floorf(x);
}

inline float x_ceilf(float x)
{
    return ceilf(x);
}

inline float x_ipart(float x)
{
    return (float)((int)x);
}

inline float x_fpart(float x)
{
    return x - x_ipart(x);
}

float x_roundf(float x)
{
    float dec = x_absf(x_fpart(x));
    float result = x_floorf(x);
    if ((x >= 0 && dec >= 0.5f) || (x < 0 && dec < 0.5f)) result += 1;
    return result;
}

inline float x_angle_to_target(float eye_x, float eye_y, float target_x, float target_y)
{
    return x_atan2f(target_y - eye_y, target_x - eye_x);
}

void x_lerp(ScePspFVector3* r, ScePspFVector3* v0, ScePspFVector3* v1, float t)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "mtv        %3, S020\n"
        "vsub.t     C010, C010, C000\n"
        "vscl.t     C010, C010, S020\n"
        "vadd.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*v0), "m"(*v1), "r"(t)
    );
}

float x_magnitude(ScePspFVector3* v)
{
    float result;
    #if 0
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "vdot.t     S000, C000, C000\n"
        "vsqrt.s    S000, S000\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "m"(*v)
    );
    #else
    result = x_sqrtf(SQR(v->x) + SQR(v->y) + SQR(v->z));
    #endif
    return result;
}

void x_normalize(ScePspFVector3* v, float mag)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %0\n"
        "lv.s       S001, 4 + %0\n"
        "lv.s       S002, 8 + %0\n"
        "vdot.t     S010, C000, C000\n"
        "vrsq.s     S010, S010\n"
        "mtv        %1, S020\n"
        "vmul.s     S010, S010, S020\n"
        "vscl.t     C000, C000, S010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "+m"(*v)
        : "r"(mag)
    );
}

float x_dotproduct(ScePspFVector3* v0, ScePspFVector3* v1)
{
    float result;
    #if 1
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vdot.t     S000, C000, C010\n"
        "mfv        %0, S000\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(result)
        : "m"(*v0), "m"(*v1)
    );
    #else
    result = v0->x*v1->x + v0->y*v1->y + v0->z*v1->z;
    #endif
    return result;
}

void x_crossproduct(ScePspFVector3* r, ScePspFVector3* v0, ScePspFVector3* v1)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vcrsp.t    C020, C000, C010\n"
        "sv.s       S020, 0 + %0\n"
        "sv.s       S021, 4 + %0\n"
        "sv.s       S022, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*v0), "m"(*v1)
    );
}

/* calculates normal from 3 points and normalizes it */
void x_normal(ScePspFVector3* r, ScePspFVector3* p1, ScePspFVector3* p2, ScePspFVector3* p3)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "lv.s       S020, 0 + %3\n"
        "lv.s       S021, 4 + %3\n"
        "lv.s       S022, 8 + %3\n"
        "vsub.t     C000, C000, C010\n"
        "vsub.t     C010, C020, C010\n"
        "vcrsp.t    C020, C000, C010\n"
        "vdot.t     S000, C020, C020\n"
        "vrsq.s     S000, S000\n"
        "vscl.t     C020, C020, S000\n"
        "sv.s       S020, 0 + %0\n"
        "sv.s       S021, 4 + %0\n"
        "sv.s       S022, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*p1), "m"(*p2), "m"(*p3)
    );
}

void x_translate(ScePspFVector3* v, ScePspFVector3* trans)
{
    v->x += trans->x;
    v->y += trans->y;
    v->z += trans->z;
}

void x_rotatex(ScePspFVector3* v, float x)
{
    float sin, cos;
    x_sincos(x, &sin, &cos);
    ScePspFVector3 vec = *v;
    v->y = (cos * vec.y) - (sin * vec.z);
    v->z = (sin * vec.y) + (cos * vec.z);
}

void x_rotatey(ScePspFVector3* v, float x)
{
    float sin, cos;
    x_sincos(x, &sin, &cos);
    ScePspFVector3 vec = *v;
    v->x = (cos * vec.x) + (sin * vec.z);
    v->z = -(sin * vec.x) + (cos * vec.z);
}

void x_rotatez(ScePspFVector3* v, float x)
{
    float sin, cos;
    x_sincos(x, &sin, &cos);
    ScePspFVector3 vec = *v;
    v->x = (cos * vec.x) - (sin * vec.y);
    v->y = (sin * vec.x) + (cos * vec.y);
}

void x_billboard(ScePspFVector3* r, ScePspFVector3* pos, ScePspFMatrix4* view_mat)
{
    r->x = pos->x*view_mat->x.x + pos->y*view_mat->y.x + pos->z*view_mat->z.x + view_mat->w.x;
    r->y = pos->x*view_mat->x.y + pos->y*view_mat->y.y + pos->z*view_mat->z.y + view_mat->w.y;
    r->z = pos->x*view_mat->x.z + pos->y*view_mat->y.z + pos->z*view_mat->z.z + view_mat->w.z;
}

void x_billboard_dir(ScePspFVector3* r, ScePspFVector3* eye, ScePspFVector3* pos, ScePspFVector3* dir)
{
    ScePspFVector3 fwd = {eye->x - pos->x, eye->y - pos->y, eye->z - pos->z};
    x_crossproduct(r, dir, &fwd);
    x_normalize(r, 0.5f);
}

inline float x_dist2(float x1, float y1, float x2, float y2)
{
    return x_sqrtf(SQR(x2 - x1) + SQR(y2 - y1));
}

inline float x_dist3(ScePspFVector3* p1, ScePspFVector3* p2)
{
    return x_sqrtf(SQR(p2->x - p1->x) + SQR(p2->y - p1->y) + SQR(p2->z - p1->z));
}

inline float x_ease_to_target(float cur, float target, float p, float dt)
{
    return cur + (target - cur) * p * dt;
}

inline void x_ease_to_target2(float* cur_x, float* cur_y, float target_x, float target_y, float p, float dt)
{
    *cur_x = x_ease_to_target(*cur_x, target_x, p, dt);
    *cur_y = x_ease_to_target(*cur_y, target_y, p, dt);
}

inline void x_ease_to_target3(ScePspFVector3* cur, ScePspFVector3* target, float p, float dt)
{
    cur->x = x_ease_to_target(cur->x, target->x, p, dt);
    cur->y = x_ease_to_target(cur->y, target->y, p, dt);
    cur->z = x_ease_to_target(cur->z, target->z, p, dt);
}

int x_dist_test2(float x1, float y1, float x2, float y2, float d)
{
    return (SQR(x2-x1) + SQR(y2-y1) < SQR(d));
}

int x_dist_test3(ScePspFVector3* p1, ScePspFVector3* p2, float d)
{
    return (SQR(p1->x - p2->x) + SQR(p1->y - p2->y) + SQR(p1->z - p2->z) < SQR(d));
}

xVector3f* xVec3Set(xVector3f* a, float x, float y, float z)
{
    a->x = x;
    a->y = y;
    a->z = z;
    return a;
}

xVector3f* xVec3Add(xVector3f* r, xVector3f* a, xVector3f* b)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vadd.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xVector3f* xVec3Sub(xVector3f* r, xVector3f* a, xVector3f* b)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vsub.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xVector3f* xVec3Mul(xVector3f* r, xVector3f* a, xVector3f* b)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vmul.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xVector3f* xVec3Div(xVector3f* r, xVector3f* a, xVector3f* b)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vdiv.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xVector3f* xVec3Scale(xVector3f* r, xVector3f* a, float s)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "mtv        %2, S010\n"
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "vscl.t     C000, C000, S010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "r"(s)
    );
    return r;
}

xVector3f* xVec3Normalize(xVector3f* r, xVector3f* a)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "vdot.t     S010, C000, C000\n"
        "vrsq.s     S010, S010\n"
        "vscl.t     C000, C000, S010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a)
    );
    return r;
}

float xVec3Length(xVector3f* a)
{
    float r;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "vdot.t     S010, C000, C000\n"
        "vsqrt.s    S010, S010\n"
        "mfv        %0, S010\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(r)
        : "m"(*a)
    );
    return r;
}

float xVec3SqLength(xVector3f* a)
{
    float r;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "vdot.t     S010, C000, C000\n"
        "mfv        %0, S010\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(r)
        : "m"(*a)
    );
    return r;
}

xVector3f* xVec3Lerp(xVector3f* r, xVector3f* a, xVector3f* b, float t)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "mtv        %3, S020\n"
        "vsub.t     C010, C010, C000\n"
        "vscl.t     C010, C010, S020\n"
        "vadd.t     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*a), "r"(t)
    );
    return r;
}

float xVec3Dot(xVector3f* a, xVector3f* b)
{
    float r;
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vdot.t     S020, C000, C010\n"
        "mfv        %0, S020\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=r"(r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xVector3f* xVec3Cross(xVector3f* r, xVector3f* a, xVector3f* b)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "vcrsp.t    C020, C000, C010\n"
        "sv.s       S020, 0 + %0\n"
        "sv.s       S021, 4 + %0\n"
        "sv.s       S022, 8 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b)
    );
    return r;
}

xColor4f* xCol4Set(xColor4f* c, float r, float g, float b, float a)
{
    c->r = r;
    c->g = g;
    c->b = b;
    c->a = a;
    return c;
}

xColor4f* xCol4Lerp(xColor4f* r, xColor4f* a, xColor4f* b, float t)
{
    __asm__ volatile (
        #ifdef ASM_SAFE
        ".set       push\n"
        ".set       noreorder\n"
        #endif
        "lv.s       S000, 0 + %1\n"
        "lv.s       S001, 4 + %1\n"
        "lv.s       S002, 8 + %1\n"
        "lv.s       S003, 12 + %1\n"
        "lv.s       S010, 0 + %2\n"
        "lv.s       S011, 4 + %2\n"
        "lv.s       S012, 8 + %2\n"
        "lv.s       S013, 12 + %2\n"
        "mtv        %3, S020\n"
        "vsub.q     C010, C010, C000\n"
        "vscl.q     C010, C010, S020\n"
        "vadd.q     C000, C000, C010\n"
        "sv.s       S000, 0 + %0\n"
        "sv.s       S001, 4 + %0\n"
        "sv.s       S002, 8 + %0\n"
        "sv.s       S003, 12 + %0\n"
        #ifdef ASM_SAFE
        ".set       pop\n"
        #endif
        : "=m"(*r)
        : "m"(*a), "m"(*b), "r"(t)
    );
    return r;
}
