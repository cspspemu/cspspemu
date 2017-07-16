#include <stdlib.h>
#include <GLES/egl.h>
#include <GLES/glut.h>

#include <pspkerneltypes.h>
#include <pspctrl.h>
#include <psploadexec.h>

#include "pspgl_misc.h"

static unsigned int glut_display_mode = 0;
static unsigned int glut_redisplay_posted = 1;
static void (*glut_display_func) (void);
static void (*glut_reshape_func) (int width, int height);

static void (*glut_keyboard_func [2]) (unsigned char key, int x, int y);
static void (*glut_special_func [2]) (int key, int x, int y);

static void (*glut_mouse_func) (int button, int state, int x, int y);
static void (*glut_motion_func) (int x, int y);
static void (*glut_passive_motion_func) (int x, int y);

static void (*glut_joystick_func) (unsigned int buttonMask, int x, int y, int z);

static void (*glut_idle_func) (void);

#undef psp_log

/* disable verbose logging to "ms0:/log.txt" */
#if 0
	#define psp_log(x...) __pspgl_log(x)
#else
	#define psp_log(x...) do {} while (0)
#endif

/* enable EGLerror logging to "ms0:/log.txt" */
#if 1
	#define EGLCHK(x)							\
	do {									\
		EGLint errcode;							\
		x;								\
		errcode = eglGetError();					\
		if (errcode != EGL_SUCCESS) {					\
			__pspgl_log("%s (%d): EGL error 0x%04x\n",			\
				__FUNCTION__, __LINE__,				\
				(unsigned int) errcode);			\
		}								\
	} while (0)
#else
	#define EGLCHK(x) x
#endif



void glutInit (int *argcp, char **argv)
{
}


void glutInitDisplayMode (unsigned int mode)
{
	glut_display_mode = mode;
}


void glutInitWindowPosition (int x, int y)
{
}


void glutInitWindowSize (int width, int height)
{
}


static EGLDisplay dpy;
static EGLContext ctx;
static EGLSurface surface;
static EGLint width = 480;
static EGLint height = 272;
static int mouse_x;
static int mouse_y;


static EGLint attrib_list [] = {
	EGL_RED_SIZE, 8,	/* 0 */
	EGL_GREEN_SIZE, 8,	/* 2 */
	EGL_BLUE_SIZE, 8,	/* 4 */
	EGL_ALPHA_SIZE, 0,	/* 6 */
	EGL_STENCIL_SIZE, 0,	/* 8 */
	EGL_DEPTH_SIZE, 0,	/* 10 */
	EGL_NONE
};


static
void cleanup (void)
{
	EGLCHK(eglTerminate(dpy));
}


int glutCreateWindow (const char *title)
{
	EGLConfig config;
	EGLint num_configs;

	atexit(cleanup);

	/* pass NativeDisplay=0, we only have one screen... */
	EGLCHK(dpy = eglGetDisplay(0));
	EGLCHK(eglInitialize(dpy, NULL, NULL));

	psp_log("EGL vendor \"%s\"\n", eglQueryString(dpy, EGL_VENDOR));
	psp_log("EGL version \"%s\"\n", eglQueryString(dpy, EGL_VERSION));
	psp_log("EGL extensions \"%s\"\n", eglQueryString(dpy, EGL_EXTENSIONS));

	if (glut_display_mode & GLUT_ALPHA)
		attrib_list[7] = 8;
	if (glut_display_mode & GLUT_STENCIL)
		attrib_list[9] = 8;
	if (glut_display_mode & GLUT_DEPTH)
		attrib_list[11] = 16;

	EGLCHK(eglChooseConfig(dpy, attrib_list, &config, 1, &num_configs));

	if (num_configs == 0) {
		__pspgl_log("glutCreateWindow: eglChooseConfig returned no configurations for display mode %x\n",
			    glut_display_mode);
		return 0;
	}

	psp_log("eglChooseConfig() returned config 0x%04x\n", (unsigned int) config);

	EGLCHK(eglGetConfigAttrib(dpy, config, EGL_WIDTH, &width));
	EGLCHK(eglGetConfigAttrib(dpy, config, EGL_HEIGHT, &height));

	EGLCHK(ctx = eglCreateContext(dpy, config, NULL, NULL));
	EGLCHK(surface = eglCreateWindowSurface(dpy, config, 0, NULL));
	EGLCHK(eglMakeCurrent(dpy, surface, surface, ctx));

	return 0;
}


#define KEY_TYPE_ASCII	  1
#define KEY_TYPE_SPECIAL  2
#define KEY_TYPE_MOUSE	  3

#define KEY_ASCII(x)      ((KEY_TYPE_ASCII   << 24) | (x))
#define KEY_SPECIAL(x)    ((KEY_TYPE_SPECIAL << 24) | (x))
#define KEY_MOUSE(x)      ((KEY_TYPE_MOUSE   << 24) | (x))

#define KEY_TYPE(x)	  (x >> 24)


/* XXX IMPROVE: might get packed tighter in 16bit words */
static const
unsigned long keycode [] = {
	KEY_ASCII('s'),			/* PSP_CTRL_SELECT   = 0x000001 */
	0,				/*                     0x000002 */
	0,				/*                     0x000004 */
	KEY_ASCII('a'),			/* PSP_CTRL_START    = 0x000008 */
	KEY_SPECIAL(GLUT_KEY_UP),	/* PSP_CTRL_UP	     = 0x000010 */
	KEY_SPECIAL(GLUT_KEY_RIGHT),	/* PSP_CTRL_RIGHT    = 0x000020 */
	KEY_SPECIAL(GLUT_KEY_DOWN),	/* PSP_CTRL_DOWN     = 0x000040 */
	KEY_SPECIAL(GLUT_KEY_LEFT),	/* PSP_CTRL_LEFT     = 0x000080 */
	KEY_MOUSE(GLUT_LEFT_BUTTON),	/* PSP_CTRL_LTRIGGER = 0x000100 */
	KEY_MOUSE(GLUT_RIGHT_BUTTON),	/* PSP_CTRL_RTRIGGER = 0x000200 */
	0,				/*                     0x000400 */
	0,				/*                     0x000800 */
	KEY_ASCII('d'),			/* PSP_CTRL_TRIANGLE = 0x001000 */
	KEY_ASCII('o'),			/* PSP_CTRL_CIRCLE   = 0x002000 */
	KEY_ASCII('x'),			/* PSP_CTRL_CROSS    = 0x004000 */
	KEY_ASCII('q'),			/* PSP_CTRL_SQUARE   = 0x008000 */
	KEY_SPECIAL(GLUT_KEY_HOME),	/* PSP_CTRL_HOME     = 0x010000 */
	KEY_ASCII('h'),			/* PSP_CTRL_HOLD     = 0x020000 */
	0,				/*                     0x040000 */
	0,				/*                     0x080000 */
	0,				/*                     0x100000 */
	0,				/*                     0x200000 */
	0,				/*                     0x400000 */
	KEY_ASCII('n'),			/* PSP_CTRL_NOTE     = 0x800000 */
};


static
void key (unsigned long keycode, int pressed)
{
	unsigned long keyval = keycode & 0xffff;

	switch (KEY_TYPE(keycode)) {
	case KEY_TYPE_MOUSE:
		if (glut_mouse_func)
			glut_mouse_func(keyval, pressed ? GLUT_DOWN : GLUT_UP, mouse_x, mouse_y);
		break;
	case KEY_TYPE_SPECIAL:
		if (glut_special_func[pressed])
			glut_special_func[pressed](keyval, mouse_x, mouse_y);
		break;
	case KEY_TYPE_ASCII:
		if (glut_keyboard_func[pressed])
			glut_keyboard_func[pressed](keyval, mouse_x, mouse_y);
	default:
		;
	}
}


void glutMainLoop (void)
{
	if (glut_reshape_func)
		glut_reshape_func(width, height);

	sceCtrlSetSamplingCycle(0);
	sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);

	do {
		if (glut_joystick_func) {
			struct SceCtrlData pad;
			sceCtrlReadBufferPositive(&pad, 1);
			glut_joystick_func(pad.Buttons,
					   (pad.Lx * 2000L) / 256 - 1000,
					   (pad.Ly * 2000L) / 256 - 1000, 0);
		}

//@@@ old part of keypad reading 
/*		while (1) {
			struct SceCtrlLatch latch;
			int i;

			sceCtrlReadLatch(&latch);

			if (latch.uiMake == 0 && latch.uiBreak == 0)
				break;

			for (i=0; i<sizeof(keycode)/sizeof(keycode[0]); i++) {
				if (latch.uiMake & (1 << i))
					key(keycode[i], 1);
				if (latch.uiBreak & (1 << i))
					key(keycode[i], 0);
			}
		};
*/
// @@@ new part from Edorul : permit to repeat special key 
// @@@ and mouse key but not normal keys
		SceCtrlData pad;
		static unsigned int oldbuttons = 0;
		int i;
		
		sceCtrlReadBufferPositive(&pad, 1);
		
		for (i=0; i<sizeof(keycode)/sizeof(keycode[0]); i++) {
			// no key repeat if it's a normal key
			if (KEY_TYPE(keycode[i]) == KEY_TYPE_ASCII) { 
				if (!(oldbuttons & (1 << i))&&(pad.Buttons & (1 << i)))
					key(keycode[i], 1);
			}
			else
				if (pad.Buttons & (1 << i))
					key(keycode[i], 1);
			
			// no repeat when key released
			if (!(pad.Buttons & (1 << i))&&(oldbuttons & (1 << i))) 
				key(keycode[i], 0);
		}
		
		oldbuttons = pad.Buttons;
// @@@ end of the new part
		
		if (glut_display_func && glut_redisplay_posted) {
			glut_redisplay_posted = 0;
			glut_display_func();
		}

		if (glut_idle_func)
			glut_idle_func();
	} while (1);
}


void glutPostRedisplay (void)
{
	glut_redisplay_posted = 1;
}


void glutSwapBuffers(void)
{
	EGLCHK(eglSwapBuffers(dpy, surface));
}


void glutDisplayFunc (void (*func) (void))
{
	glut_display_func = func;
}


void glutReshapeFunc (void (*func) (int width, int height))
{
	glut_reshape_func = func;
}


void glutKeyboardFunc (void (*func) (unsigned char key, int x, int y))
{
	glut_keyboard_func[1] = func;
}


void glutMouseFunc (void (*func) (int button, int state, int x, int y))
{
	glut_mouse_func = func;
}


void glutMotionFunc (void (*func) (int x, int y))
{
	glut_motion_func = func;
}


void glutPassiveMotionFunc (void (*func) (int x, int y))
{
	glut_passive_motion_func = func;
}


void glutIdleFunc (void (*func) (void))
{
	glut_idle_func = func;
}


void glutSpecialFunc (void (*func) (int key, int x, int y))
{
	glut_special_func[1] = func;
}


void glutKeyboardUpFunc (void (*func) (unsigned char key, int x, int y))
{
	glut_keyboard_func[0] = func;
}


void glutSpecialUpFunc (void (*func) (int key, int x, int y))
{
	glut_special_func[0] = func;
}


void glutJoystickFunc (void (*func) (unsigned int buttonMask, int x, int y, int z), int pollInterval)
{
	glut_joystick_func = func;
}

