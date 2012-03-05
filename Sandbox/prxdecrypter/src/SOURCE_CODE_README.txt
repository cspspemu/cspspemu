prxdecrypter source code release, 27 june 2010

here it is - finally. time for someone else to take up the reigns and update this to a more fitting standard for the current psp scene.

the code is an absolute monster and messy in places, uses various tricks which are not particularly good coding practice, so have a good look through it before you start making any changes.

to update the build number, update it in the makefile, PSP_EBOOT_TITLE = PRXdecrypter 2.5 *AND* update it in the main.c, at the top where it says #define VERSION "2.5" - remember to update README.TXT which ive also included. ive included make.bat which was necessary on the weird toolchain i had on my machine, compile it however you like.

you'll notice that the majority of the code is based on the excellent psardumper which was originally coded by PspPet from ps2dev all those years ago.

enjoy, and keep in touch.

jas0nuk

twitter, @jason_manc
irc, irc.dark-alex.org #wtf