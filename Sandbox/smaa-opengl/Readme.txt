SMAA (Subpixel morphological antialiasing) demo for openGL.

Needs SFML (at least 2.0 RC) and GLEW (latest) to compile.

Do not use block interfaces for the out variables when you are implementing this on ATI/AMD.
It will probably crash the video card driver. I've reported the issue to AMD, they'll probably fix this soon. 
See:
http://devgurus.amd.com/thread/159290

You can toggle the effect by pressing space, and exit with either clicking "x" or pressing escape.

This demo should be built in a sub directory:

*cd to this directory*
mkdir build
cd build
cmake ..
make
./smaa