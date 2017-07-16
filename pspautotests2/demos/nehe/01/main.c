//This code is just a skeleton for your own programs

// This code was created by Jeff Molofee '99 (ported to Linux/GLUT by Richard Campbell '99)
//
// If you've found this code useful, please let me know.
//
// Visit me at www.demonews.com/hosted/nehe 
// (email Richard Campbell at ulmont@bellsouth.net)
//

// this was modified to work on PSP by Edorul (edorul@free.fr)
// Many Thanks to  jared bruni (jared@lostsidedead.com) for is
// MasterPiece3D port to PSP : it gave me a good sample and not
// the least a working makefile !

// important notes :  - all modified portion of code from cygwin version
//                                  of Nehe tutorial are marked with @@@

// Used keys :
// START = exit 

#include <stdlib.h> // needed in order to have "exit" function @@@

#include <GL/glut.h>    // Header File For The GLUT Library 
#include <GL/gl.h>	// Header File For The OpenGL32 Library
#include <GL/glu.h>	// Header File For The GLu32 Library

/* The number of our GLUT window */
int window; 

/* A general OpenGL initialization function.  Sets all of the initial parameters. */
void InitGL(int Width, int Height)	        // We call this right after our OpenGL window is created.
{
  glClearColor(0.0f, 0.0f, 0.0f, 0.0f);		// This Will Clear The Background Color To Black
  glClearDepth(1.0);				// Enables Clearing Of The Depth Buffer
  glDepthFunc(GL_LESS);				// The Type Of Depth Test To Do
  glEnable(GL_DEPTH_TEST);			// Enables Depth Testing
  glShadeModel(GL_SMOOTH);			// Enables Smooth Color Shading

  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();				// Reset The Projection Matrix

  gluPerspective(45.0f,(float)Width/(float)Height,0.1f,100.0f);	// Calculate The Aspect Ratio Of The Window

  glMatrixMode(GL_MODELVIEW);
}

/* The function called when our window is resized (which shouldn't happen, because we're fullscreen) */
void ReSizeGLScene(int Width, int Height)
{
  if (Height==0)				// Prevent A Divide By Zero If The Window Is Too Small
    Height=1;

  glViewport(0, 0, Width, Height);		// Reset The Current Viewport And Perspective Transformation

  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();

  gluPerspective(45.0f,(float)Width/(float)Height,0.1f,100.0f);
  glMatrixMode(GL_MODELVIEW);
}

/* The main drawing function. */
void DrawGLScene()
{
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);		// Clear The Screen And The Depth Buffer
	glLoadIdentity();				// Reset The View

	// swap buffers to display, since we're double buffered.
	glutSwapBuffers();
}

/* The function called whenever a key is pressed. */
void keyPressed(unsigned char key, int x, int y) 
{
	switch (key) {
	case 'd':			/* delta, triangle */
		break;
	case 'o':			/* round */
		break;
	case 'q':			/* square*/
		break;
	case 'x':			/* cross */
		break;
	case 'n':			/* key with the music note */
		break;
	case 's':			/* select */
		break;
	case 'a':			/* startbutton */  /* If START is pressed, kill everything. */
 		/* exit the program...normal termination. */
		exit(0);                   
	default:
		;
	}
 }

/* The function called whenever a key is released. */
void keyReleased(unsigned char key, int x, int y) 
{
	switch (key) {
	case 'd':			/* delta, triangle */
		break;
	case 'o':			/* round */
		break;
	case 'q':			/* square*/
		break;
	case 'x':			/* cross */
		break;
	case 'n':			/* key with the music note */
		break;
	case 's':			/* select */
		break;
	case 'a':			/* startbutton */
		break;
	default:
		;
	}
 }

/* The function called whenever a special key is pressed. */
void specialKeyPressed(int key, int x, int y) 
{
    switch (key) {    
    case GLUT_KEY_UP: // pad arrow up
	break;

    case GLUT_KEY_DOWN: //  pad arrow down
	break;

    case GLUT_KEY_LEFT: //  pad arrow left
	break;
    
    case GLUT_KEY_RIGHT: //  pad arrow right
	break;

    case GLUT_KEY_HOME: // home
	break;
	
    default:
	break;
    }	
}

/* The function called whenever a special key is released. */
void specialKeyReleased(int key, int x, int y) 
{
    switch (key) {    
    case GLUT_KEY_UP: // pad arrow up
	break;

    case GLUT_KEY_DOWN: //  pad arrow down
	break;

    case GLUT_KEY_LEFT: //  pad arrow left
	break;
    
    case GLUT_KEY_RIGHT: //  pad arrow right
	break;

    case GLUT_KEY_HOME: // home
	break;
	
    default:
	break;
    }	
}

/* The function called whenever the joystick is moved. */
void joystickMoved (unsigned int buttonMask, int x, int y, int z)
{
	if (abs(x) > 150) // dead zone
	{	
		// use x value
	}

	if (abs(y) > 150) // dead zone
	{	
		// use y value
	}
}

/* The function called whenever the triggers are pressed. */
void triggerHandle (int button, int state, int x, int y)
{
	if (button == GLUT_LEFT_BUTTON) {  // left trigger...
		if (state == GLUT_DOWN) {  // ...is pressed
		}
		if (state == GLUT_UP) {  // ...is released
		}
	}

	if (button == GLUT_RIGHT_BUTTON) {  // right trigger...
		if (state == GLUT_DOWN) {  // ...is pressed
		}
		if (state == GLUT_DOWN) {  // ...is released
		}
	}
}

/* main function */
int main(int argc, char **argv) 
{  
  /* Initialize GLUT state - glut will take any command line arguments that pertain to it or 
     X Windows - look at its documentation at http://reality.sgi.com/mjk/spec3/spec3.html */  
  glutInit(&argc, argv);  

  /* Select type of Display mode:   
     Double buffer 
     RGBA color
     Alpha components supported 
     Depth buffer */  
  glutInitDisplayMode(GLUT_RGBA | GLUT_DOUBLE | GLUT_ALPHA | GLUT_DEPTH);  

  /* get a 640 x 480 window */
  glutInitWindowSize(480, 272); 

  /* the window starts at the upper left corner of the screen */
  glutInitWindowPosition(0, 0);  

  /* Open a window */  
  window = glutCreateWindow("Jeff Molofee's GL Code Tutorial ... NeHe '99");  

  /* Register the function to do all our OpenGL drawing. */
  glutDisplayFunc(&DrawGLScene);  

  /* Even if there are no events, redraw our gl scene. */
  glutIdleFunc(&DrawGLScene);

  /* Register the function called when our window is resized. */
  glutReshapeFunc(&ReSizeGLScene);

  /* Register the function called when the keyboard is pressed. */
  glutKeyboardFunc(&keyPressed);
  /* Register the function called when the keyboard is released. */
  glutKeyboardUpFunc(&keyReleased);
  /* Register the function called when special keys (arrows, page down, etc) are pressed. */
  glutSpecialFunc(&specialKeyPressed);
  /* Register the function called when special keys (arrows, page down, etc) are released. */
  glutSpecialUpFunc(&specialKeyReleased);
  /* Register the function called when joystick is moved. */
  glutJoystickFunc(&joystickMoved, 0); // 0 = Joystick polling interval in milliseconds
  /* Register the function called when Trigger_left or Trigger_right is pressed */
  glutMouseFunc(&triggerHandle);
  
  /* Initialize our window. */
  InitGL(480, 272); 
  
  /* Start Event Processing Engine */  
  glutMainLoop();  

  return (0);
}
