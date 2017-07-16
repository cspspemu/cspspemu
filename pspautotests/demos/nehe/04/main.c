//From cygwin version of Nehe tutorial lesson 4

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
//#include <unistd.h>     // Header File For sleeping. @@@

/* ASCII code for the escape key. */
//#define ESCAPE 27 @@@

/* The number of our GLUT window */
int window; 

/* rotation angle for the triangle. */
float rtri = 0.0f;

/* rotation angle for the quadrilateral. */
float rquad = 0.0f;

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

  gluPerspective(45.0f,(GLfloat)Width/(GLfloat)Height,0.1f,100.0f);	// Calculate The Aspect Ratio Of The Window

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

  gluPerspective(45.0f,(GLfloat)Width/(GLfloat)Height,0.1f,100.0f);
  glMatrixMode(GL_MODELVIEW);
}

/* The main drawing function. */
void DrawGLScene()
{
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);		// Clear The Screen And The Depth Buffer
  glLoadIdentity();				// Reset The View

  glTranslatef(-1.5f,0.0f,-6.0f);		// Move Left 1.5 Units And Into The Screen 6.0
	
  glRotatef(rtri,0.0f,1.0f,0.0f);		// Rotate The Triangle On The Y axis 
  // draw a triangle (in smooth coloring mode)
  glBegin(GL_POLYGON);				// start drawing a polygon
  glColor3f(1.0f,0.0f,0.0f);			// Set The Color To Red
  glVertex3f( 0.0f, 1.0f, 0.0f);		// Top
  glColor3f(0.0f,1.0f,0.0f);			// Set The Color To Green
  glVertex3f( 1.0f,-1.0f, 0.0f);		// Bottom Right
  glColor3f(0.0f,0.0f,1.0f);			// Set The Color To Blue
  glVertex3f(-1.0f,-1.0f, 0.0f);		// Bottom Left	
  glEnd();					// we're done with the polygon (smooth color interpolation)

  glLoadIdentity();				// make sure we're no longer rotated.
  glTranslatef(1.5f,0.0f,-6.0f);		// Move Right 3 Units, and back into the screen 6.0
	
  glRotatef(rquad,1.0f,0.0f,0.0f);		// Rotate The Quad On The X axis 
  // draw a square (quadrilateral)
  glColor3f(0.5f,0.5f,1.0f);			// set color to a blue shade.
  glBegin(GL_QUADS);				// start drawing a polygon (4 sided)
  glVertex3f(-1.0f, 1.0f, 0.0f);		// Top Left
  glVertex3f( 1.0f, 1.0f, 0.0f);		// Top Right
  glVertex3f( 1.0f,-1.0f, 0.0f);		// Bottom Right
  glVertex3f(-1.0f,-1.0f, 0.0f);		// Bottom Left	
  glEnd();					// done with the polygon

  rtri+=1.50f;					// Increase The Rotation Variable For The Triangle
  rquad-=1.50f;					// Decrease The Rotation Variable For The Quad 

  // swap the buffers to display, since double buffering is used.
  glutSwapBuffers();
}

/* The function called whenever a key is pressed. */
// @@@ this function is  modified to use PSP pad
void keyPressed(unsigned char key, int x, int y) 
{
	switch (key) {
	case 'a':			/* startbutton */ /* If START is pressed, kill everything. */
		/* shut down our window */
		// but don't exist in pspgl
//		glutDestroyWindow(window);  @@@
	
		/* exit the program...normal termination. */
		exit(0);                   
	default:
		;
	}
}

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
  glutInitWindowSize(480, 272);  // @@@

  /* the window starts at the upper left corner of the screen */
  glutInitWindowPosition(0, 0);  

  /* Open a window */  
  window = glutCreateWindow("Jeff Molofee's GL Code Tutorial ... NeHe '99");  

  /* Register the function to do all our OpenGL drawing. */
  glutDisplayFunc(&DrawGLScene);  

  /* Go fullscreen.  This is as soon as possible. */
//  glutFullScreen(); @@@ there is no glutFullScreen in pspgl

  /* Even if there are no events, redraw our gl scene. */
  glutIdleFunc(&DrawGLScene);

  /* Register the function called when our window is resized. */
  glutReshapeFunc(&ReSizeGLScene);

  /* Register the function called when the keyboard is pressed. */
  glutKeyboardFunc(&keyPressed);

  /* Initialize our window. */
  InitGL(480, 272);  // @@@
  
  /* Start Event Processing Engine */  
  glutMainLoop();  

  return 1;
}
