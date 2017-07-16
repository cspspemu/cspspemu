Hi !

In this archive you'll find :

- this file :D

- a mini tutorial to install PSPSDK and PSPGL

- "glut.c" which is a modified version of the one in pspgl. In pspgl there is no key repitition, you need to release and push again a key if you want your program to detect it again. With this modified version you can push arrows or triggers (but not start, select, cross, triangle, square and circle) and 'til they remain pushed they are detected and can be used by your program. (used in Nehe07, 08, 09 and 10). 
To use it you need to replace the original one, compile pspgl and intall it again.

- Nehe01 : it's not a real Nehe tutorial but a skeleton for your future programs with a working Makefile. It initialises pspgl and give you all the functions in order to use PSP arrows pad, triggers, keys and joystick.

- Nehe02 to Nehe10 : they are adaptation of cygwin version of Nehe tutorials modified in order to run on a PSP. 
You will notice in each folder a working Makefile, "psp-setup.c" needed to compile pspgl programs and "copy.sh" that permits to copy directly your program to your PSP without living cygwin, just type "./copy.sh". 
All parts I've modified are maked by "@@@".

For the moment they are some bugs :
Nehe08 : if you use blend and light at the same time the cube disapear.
Nehe10 : Idem as Nehe08, and there is a clipping problem (as in all Nehe tutorials I have modified, but here it's very visible) -> if a point of a triangle is out of the clipping area the whole triangle is not drawn. It's specialy significant with the cell and floor.

Your feedbacks are welcome if you find a way to fix those bugs ;)

I hope this archive will be usefull to someone.

Edorul.


history :
v0.1: first release

