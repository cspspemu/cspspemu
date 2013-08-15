#include <iostream>
#include <fstream>

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#undef near
#undef far
#endif

#include "GL/glew.h" //the mighty GLEW :)
#include "SFML/Window.hpp"
#include "SFML/Audio.hpp"

#include "smaa_glsl.h"

#ifdef __unix__
#include "unistd.h"
#endif

/*
 * Global variables
 */

sf::Window the_window;
sf::Event the_event;

float fps = 1.0f;
int frames = 0;
sf::Clock the_clock;
std::string app_path;
#define SCREEN_WIDTH 800
#define SCREEN_HEIGHT 600

bool do_effect = true;

#define AREATEX_WIDTH 160
#define AREATEX_HEIGHT 560
#define SEARCHTEX_WIDTH 66
#define SEARCHTEX_HEIGHT 33

GLuint albedo_tex;
GLuint edge_tex;
GLuint blend_tex;
GLuint area_tex;
GLuint search_tex;

GLuint albedo_fbo;
GLuint edge_fbo;
GLuint blend_fbo;

GLuint edge_shader;
GLuint blend_shader;
GLuint neighborhood_shader;

#define INFOLOG_SIZE 4096 * sizeof(GLchar)

/*
 * Function declarations
 */

void get_opengl_error( bool ignore = false );

void shader_include( std::string& shader );

void replace_all( std::string& str, const std::string& from, const std::string& to );

void create_shader( std::string* vs_text, std::string* ps_text, GLuint* program );

void validate_program( GLuint program );

void draw_quad();

void check_fbo();

void get_app_path();

int main( int argc, char* args[] )
{
  /*
   * Initialize OpenGL context
   */

  the_window.create( sf::VideoMode( SCREEN_WIDTH, SCREEN_HEIGHT, 32 ), "SMAA", sf::Style::Default );

  if( !the_window.isOpen() )
  {
    std::cerr << "Couldn't initialize SFML.\n";
    the_window.close();
    exit( 1 );
  }

  GLenum glew_error = glewInit();

  if( glew_error != GLEW_OK )
  {
    std::cerr << "Error initializing GLEW: " << glewGetErrorString( glew_error ) << "\n";
    the_window.close();
    exit( 1 );
  }

  if( !GLEW_VERSION_4_1 )
  {
    std::cerr << "Error: OpenGL 4.1 is required\n";
    the_window.close();
    exit( 1 );
  }

  get_app_path();

  /*
   * Initialize and load textures
   */

  glEnable( GL_TEXTURE_2D );

  glGenTextures( 1, &albedo_tex );
  glBindTexture( GL_TEXTURE_2D, albedo_tex );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
  glTexImage2D( GL_TEXTURE_2D, 0, GL_RGBA8, SCREEN_WIDTH, SCREEN_HEIGHT, 0, GL_RGBA, GL_FLOAT, 0 );

  glGenTextures( 1, &edge_tex );
  glBindTexture( GL_TEXTURE_2D, edge_tex );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
  glTexImage2D( GL_TEXTURE_2D, 0, GL_RGBA8, SCREEN_WIDTH, SCREEN_HEIGHT, 0, GL_RGBA, GL_FLOAT, 0 );

  glGenTextures( 1, &blend_tex );
  glBindTexture( GL_TEXTURE_2D, blend_tex );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
  glTexImage2D( GL_TEXTURE_2D, 0, GL_RGBA8, SCREEN_WIDTH, SCREEN_HEIGHT, 0, GL_RGBA, GL_FLOAT, 0 );

  unsigned char* buffer = 0;
  FILE* f = 0;

  buffer = new unsigned char[1024 * 1024];
  f = fopen( ( app_path + "smaa_area.raw" ).c_str(), "rb" ); //rb stands for "read binary file"

  if( !f )
  {
    std::cerr << "Couldn't open smaa_area.raw.\n";
    the_window.close();
    exit( 1 );
  }

  fread( buffer, AREATEX_WIDTH * AREATEX_HEIGHT * 2, 1, f );
  fclose( f );

  f = 0;

  glGenTextures( 1, &area_tex );
  glBindTexture( GL_TEXTURE_2D, area_tex );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
  glTexImage2D( GL_TEXTURE_2D, 0, GL_RG8, ( GLsizei )AREATEX_WIDTH, ( GLsizei )AREATEX_HEIGHT, 0, GL_RG, GL_UNSIGNED_BYTE, buffer );

  f = fopen( ( app_path + "smaa_search.raw" ).c_str(), "rb" );

  if( !f )
  {
    std::cerr << "Couldn't open smaa_search.raw.\n";
    the_window.close();
    exit( 1 );
  }

  fread( buffer, SEARCHTEX_WIDTH * SEARCHTEX_HEIGHT, 1, f );
  fclose( f );

  f = 0;

  glGenTextures( 1, &search_tex );
  glBindTexture( GL_TEXTURE_2D, search_tex );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST );
  glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST );
  glTexImage2D( GL_TEXTURE_2D, 0, GL_R8, ( GLsizei )SEARCHTEX_WIDTH, ( GLsizei )SEARCHTEX_HEIGHT, 0, GL_RED, GL_UNSIGNED_BYTE, buffer );

  delete [] buffer;

  get_opengl_error();

  /*
   * Initialize FBOs
   */

  GLenum modes[] = { GL_COLOR_ATTACHMENT0 };

  glGenFramebuffers( 1, &albedo_fbo );
  glBindFramebuffer( GL_FRAMEBUFFER, albedo_fbo );
  glDrawBuffers( 1, modes );
  glFramebufferTexture2D( GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, albedo_tex, 0 );

  check_fbo();

  glGenFramebuffers( 1, &edge_fbo );
  glBindFramebuffer( GL_FRAMEBUFFER, edge_fbo );
  glDrawBuffers( 1, modes );
  glFramebufferTexture2D( GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, edge_tex, 0 );

  check_fbo();

  glGenFramebuffers( 1, &blend_fbo );
  glBindFramebuffer( GL_FRAMEBUFFER, blend_fbo );
  glDrawBuffers( 1, modes );
  glFramebufferTexture2D( GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, blend_tex, 0 );

  check_fbo();

  glBindFramebuffer( GL_FRAMEBUFFER, 0 );

  glBindTexture( GL_TEXTURE_2D, 0 );

  get_opengl_error();

  /*
   * Set up shaders
   */

  /*
   * EDGE SHADER
   */

  create_shader( &edge_vs, &edge_ps, &edge_shader );

  //SET UNIFORMS
  glUseProgram( edge_shader );
  glUniform1i( glGetUniformLocation( edge_shader, "albedo_tex" ), 0 );
  glUseProgram( 0 );

  //VALIDATE
  validate_program( edge_shader );

  get_opengl_error();

  /*
   * BLEND SHADER
   */

  create_shader( &blend_vs, &blend_ps, &blend_shader );

  //SET UNIFORMS
  glUseProgram( blend_shader );
  glUniform1i( glGetUniformLocation( blend_shader, "edge_tex" ), 0 );
  glUniform1i( glGetUniformLocation( blend_shader, "area_tex" ), 1 );
  glUniform1i( glGetUniformLocation( blend_shader, "search_tex" ), 2 );
  glUseProgram( 0 );

  //VALIDATE
  validate_program( blend_shader );

  get_opengl_error();

  /*
   * NEIGHBORHOOD SHADER
   */

  create_shader( &neighborhood_vs, &neighborhood_ps, &neighborhood_shader );

  //SET UNIFORMS
  glUseProgram( neighborhood_shader );
  glUniform1i( glGetUniformLocation( neighborhood_shader, "albedo_tex" ), 0 );
  glUniform1i( glGetUniformLocation( neighborhood_shader, "blend_tex" ), 1 );
  glUseProgram( 0 );

  //VALIDATE
  validate_program( neighborhood_shader );

  get_opengl_error();


  /*
   * Set up matrices
   */

  glViewport( 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT );
  glMatrixMode( GL_PROJECTION );
  //x_min:0, x_max:1, y_min:0, y_max:1, z_min:0, z_max:-1
  float ortho_matrix[] = { 2, 0, 0, 0,
                           0, 2, 0, 0,
                           0, 0, 2, 0,
                           -1, -1, -1, 1
                         };
  glLoadMatrixf( ortho_matrix );
  glMatrixMode( GL_MODELVIEW );
  glLoadIdentity();

  get_opengl_error();

  /*
   * Generate input
   */

  glBindFramebuffer( GL_FRAMEBUFFER, albedo_fbo );

  glClearColor( 0, 0, 0, 0 );
  glClear( GL_COLOR_BUFFER_BIT );

  glPushMatrix();
  glBegin( GL_QUADS );
  glColor3f( 0, 0, 0 );
  glVertex2f( 0, 0 );
  glVertex2f( 1, 0 );
  glVertex2f( 1, 1 );
  glVertex2f( 0, 1 );
  glEnd();
  glPopMatrix();

  glPushMatrix();
  glTranslatef( 0.5f, 0.5f, 0.0f );
  glBegin( GL_TRIANGLES );
  glColor3f( 1, 1, 1 );
  glVertex2f( -0.25f, -0.25f );
  glVertex2f( 0, 0.25f );
  glVertex2f( 0.25f, -0.25f );
  glEnd();
  glPopMatrix();

  glBindFramebuffer( GL_FRAMEBUFFER, 0 );

  get_opengl_error();

  /*
   * Do the work
   */

  //MAIN LOOP
  the_clock.restart();

  while( true )
  {
    /*
     * Handle events
     */

    while( the_window.pollEvent( the_event ) )
    {
      if( the_event.type == sf::Event::Closed )
      {
        the_window.close();
        exit( 0 );
      }
      else if( the_event.type == sf::Event::KeyPressed )
      {
        if( the_event.key.code == sf::Keyboard::Escape )
        {
          the_window.close();
          exit( 0 );
        }
        else if( the_event.key.code == sf::Keyboard::Space )
        {
          do_effect = !do_effect;
        }
      }
    }

    /*
     * EDGE DETECTION PASS
     */

    glBindFramebuffer( GL_FRAMEBUFFER, edge_fbo );

    glClearColor( 0, 0, 0, 0 );
    glClear( GL_COLOR_BUFFER_BIT );

    glUseProgram( edge_shader );

    glActiveTexture( GL_TEXTURE0 );
    glBindTexture( GL_TEXTURE_2D, albedo_tex );

    draw_quad();

    glUseProgram( 0 );

    glBindFramebuffer( GL_FRAMEBUFFER, 0 );

    /*
     * BLENDING WEIGHTS PASS
     */

    glBindFramebuffer( GL_FRAMEBUFFER, blend_fbo );

    glClearColor( 0, 0, 0, 0 );
    glClear( GL_COLOR_BUFFER_BIT );

    glUseProgram( blend_shader );

    glActiveTexture( GL_TEXTURE0 );
    glBindTexture( GL_TEXTURE_2D, edge_tex );
    glActiveTexture( GL_TEXTURE1 );
    glBindTexture( GL_TEXTURE_2D, area_tex );
    glActiveTexture( GL_TEXTURE2 );
    glBindTexture( GL_TEXTURE_2D, search_tex );

    draw_quad();

    glUseProgram( 0 );

    glBindFramebuffer( GL_FRAMEBUFFER, 0 );

    /*
     * NEIGHBORHOOD BLENDING PASS
     */

    glUseProgram( neighborhood_shader );

    glActiveTexture( GL_TEXTURE0 );
    glBindTexture( GL_TEXTURE_2D, albedo_tex );
    glActiveTexture( GL_TEXTURE1 );
    glBindTexture( GL_TEXTURE_2D, blend_tex );

    glEnable( GL_FRAMEBUFFER_SRGB );

    draw_quad();

    glDisable( GL_FRAMEBUFFER_SRGB );

    glUseProgram( 0 );

    /*
     * Toggle effect
     */

    if( !do_effect )
    {
      glActiveTexture( GL_TEXTURE0 );
      glBindTexture( GL_TEXTURE_2D, albedo_tex );
      draw_quad();
    }

    /*
     * Debugging
     */

    glActiveTexture( GL_TEXTURE0 );
    //glBindTexture( GL_TEXTURE_2D, edge_tex );
    glBindTexture( GL_TEXTURE_2D, blend_tex );
    //glBindTexture( GL_TEXTURE_2D, albedo_tex );

    //uncomment this to see the result of one of the passes
    //draw_quad();

    /*
     * Show the result
     */

    the_window.display();

    //some benchmarking :D
    frames++;

    if( the_clock.getElapsedTime().asMilliseconds() > 1000.0f )
    {
      int timepassed = the_clock.getElapsedTime().asMilliseconds();
      fps = 1000.0f / ( ( float ) timepassed / ( float ) frames );
      std::cout << "FPS: " << fps << " Time: " << ( float ) timepassed / ( float ) frames << "\n";
      frames = 0;
      timepassed = 0;
      the_clock.restart();
    }
  }

  return 0;
}

/*
 * Function definitions, not that important
 */

//this function grabs the app path, so that we can load the needed files at runtime
void get_app_path()
{
  char fullpath[1024];

  /* /proc/self is a symbolic link to the process-ID subdir
   * of /proc, e.g. /proc/4323 when the pid of the process
   * of this program is 4323.
   *
   * Inside /proc/<pid> there is a symbolic link to the
   * executable that is running as this <pid>.  This symbolic
   * link is called "exe".
   *
   * So if we read the path where the symlink /proc/self/exe
   * points to we have the full path of the executable.
   */

#ifdef __unix__
  int length;
  length = readlink( "/proc/self/exe", fullpath, sizeof( fullpath ) );

  /* Catch some errors: */

  if( length < 0 )
  {
    std::cerr << "Couldnt read app path. Error resolving symlink /proc/self/exe.\n";
    the_window.close();
  }

  if( length >= 1024 )
  {
    std::cerr << "Couldnt read app path. Path too long. Truncated.\n";
    the_window.close();
  }

  /* I don't know why, but the string this readlink() function
   * returns is appended with a '@'.
   */
  fullpath[length] = '\0';       /* Strip '@' off the end. */

#endif

#ifdef _WIN32

  if( GetModuleFileName( 0, ( char* )&fullpath, sizeof( fullpath ) ) == 0 )
  {
    std::cerr << "Couldn't get the app path.\n";
    the_window.close();
  }

#endif

  app_path = fullpath;

#ifdef _WIN32
  app_path = app_path.substr( 0, app_path.rfind( "\\" ) + 1 );
  //when the exe is located in {source}/build/Debug/smaa.exe and we need the {source}
  app_path += "../../";
#endif

#ifdef __unix__
  app_path = app_path.substr( 0, app_path.rfind( "/" ) + 1 );
  //when the exe is located in {source}/build/smaa and we need the {source}
  app_path += "../";
#endif
}

//this function adds #include functionality to GLSL
void shader_include( std::string& shader )
{
  size_t start_pos = 0;
  std::string include_dir = "#include ";

  while( ( start_pos = shader.find( include_dir, start_pos ) ) != std::string::npos )
  {
    int pos = start_pos + include_dir.length() + 1;
    int length = shader.find( "\"", pos );
    std::string file = shader.substr( pos, length - pos );
    std::string content = "";

    std::ifstream f;
    f.open( ( app_path + file ).c_str() );

    if( f.is_open() )
    {
      char buffer[1024];

      while( !f.eof() )
      {
        f.getline( buffer, 1024 );
        content += buffer;
        content += "\n";
      }
    }
    else
    {
      std::cerr << "Couldn't include shader file: " << app_path + file << "\n";
      the_window.close();
    }

    shader.replace( start_pos, ( length + 1 ) - start_pos, content );
    start_pos += content.length();
  }
}

//replaces all occurances of a string in another string
void replace_all( std::string& str, const std::string& from, const std::string& to )
{
  size_t start_pos = 0;

  while( ( start_pos = str.find( from, start_pos ) ) != std::string::npos )
  {
    str.replace( start_pos, from.length(), to );
    start_pos += to.length(); // In case 'to' contains 'from', like replacing 'x' with 'yx'
  }
}

//do all the create and compile stuff that shaders need
void create_shader( std::string* vs_text, std::string* ps_text, GLuint* program )
{
  GLuint shader;
  const GLchar* text_ptr[1];
  GLchar infolog[INFOLOG_SIZE];
  std::string log;

  shader_include( *vs_text );
  replace_all( *vs_text, "hash ", "#" );
  shader_include( *ps_text );
  replace_all( *ps_text, "hash ", "#" );

  *program = glCreateProgram();

  //VERTEX SHADER
  text_ptr[0] = vs_text->c_str();

  shader = glCreateShader( GL_VERTEX_SHADER );
  glShaderSource( shader, 1, text_ptr, 0 );
  glCompileShader( shader );

  glGetShaderInfoLog( shader, INFOLOG_SIZE, 0, infolog );
  log = infolog;
  std::cerr << log;

  glAttachShader( *program, shader );
  glDeleteShader( shader );

  //PIXEL SHADER
  text_ptr[0] = ps_text->c_str();

  shader = glCreateShader( GL_FRAGMENT_SHADER );
  glShaderSource( shader, 1, text_ptr, 0 );
  glCompileShader( shader );

  glGetShaderInfoLog( shader, INFOLOG_SIZE, 0, infolog );
  log = infolog;
  std::cerr << log;

  glAttachShader( *program, shader );
  glDeleteShader( shader );

  //LINK
  glLinkProgram( *program );

  glGetProgramInfoLog( *program, INFOLOG_SIZE, 0, infolog );
  log = infolog;
  std::cerr << log;
}

//this needs to be a separate step, as we need to set uniforms etc.
void validate_program( GLuint program )
{
  GLchar infolog[INFOLOG_SIZE];
  std::string log;

  glValidateProgram( program );

  glGetProgramInfoLog( program, INFOLOG_SIZE, 0, infolog );
  log = infolog;
  std::cerr << log;
}

//draws a full-screen quad
void draw_quad()
{
  glBegin( GL_QUADS );
  glTexCoord2f( 0, 0 );
  glVertex2f( 0, 0 );
  glTexCoord2f( 1, 0 );
  glVertex2f( 1, 0 );
  glTexCoord2f( 1, 1 );
  glVertex2f( 1, 1 );
  glTexCoord2f( 0, 1 );
  glVertex2f( 0, 1 );
  glEnd();
}

void check_fbo()
{
  if( glCheckFramebufferStatus( GL_FRAMEBUFFER ) != GL_FRAMEBUFFER_COMPLETE )
  {
    std::cerr << "FBO not complete.\n";
    the_window.close();
    exit( 1 );
  }
}

void get_opengl_error( bool ignore )
{
  bool got_error = false;
  GLenum error = 0;
  error = glGetError();
  std::string errorstring = "";

  while( error != GL_NO_ERROR )
  {
    if( error == GL_INVALID_ENUM )
    {
      //An unacceptable value is specified for an enumerated argument. The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: invalid enum...\n";
      got_error = true;
    }

    if( error == GL_INVALID_VALUE )
    {
      //A numeric argument is out of range. The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: invalid value...\n";
      got_error = true;
    }

    if( error == GL_INVALID_OPERATION )
    {
      //The specified operation is not allowed in the current state. The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: invalid operation...\n";
      got_error = true;
    }

    if( error == GL_STACK_OVERFLOW )
    {
      //This command would cause a stack overflow. The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: stack overflow...\n";
      got_error = true;
    }

    if( error == GL_STACK_UNDERFLOW )
    {
      //This command would cause a stack underflow. The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: stack underflow...\n";
      got_error = true;
    }

    if( error == GL_OUT_OF_MEMORY )
    {
      //There is not enough memory left to execute the command. The state of the GL is undefined, except for the state of the error flags, after this error is recorded.
      errorstring += "OpenGL error: out of memory...\n";
      got_error = true;
    }

    if( error == GL_TABLE_TOO_LARGE )
    {
      //The specified table exceeds the implementation's maximum supported table size.  The offending command is ignored and has no other side effect than to set the error flag.
      errorstring += "OpenGL error: table too large...\n";
      got_error = true;
    }

    error = glGetError();
  }

  if( got_error && !ignore )
  {
    std::cerr << errorstring;
    the_window.close();
    return;
  }
}
