  * Original and outdated svn repository: http://code.google.com/p/pspautotests/
  * New and updated git repository: https://github.com/hrydgard/pspautotests

A repository of PSP programs performing several tests on the PSP platform.

  * It will allow people to see how to use some obscure-newly-discovered APIs and features
  * It will allow PSP emulators to avoid some regressions while performing refactorings and to have a reference while implementing APIs

The main idea behind this is having several files per test unit:
  * _file_*.expected* - File with the expected Kprintf's output, preferably from a real PSP
  * _file_*.elf* - The program that will call Kprintf syscall in order to generate an output
  * _file_*.input* - Optional file specifying automated actions that should simulate user interaction: pressing a key, releasing a key, selecting a file on the save selector, waiting for a function (for example a vsync) to call before continuing...



How to build and use
--------------------

If you just want to run the tests, you just need to run your emulator on the PRX-es and compare with the .expected
files. PPSSPP has a convenient script for this called test.py.

If you want to change tests, you'll need to read the rest. This tutorial is for Windows but can probably be used on Linux and Mac too, you just don't need to install the driver there.

* Prerequisites:
  - A PSP with custom firmware installed (6.60 recommended)
  - A USB cable to use between your PC and PSP
  - PSPSDK installed (on Windows I'd recommend MinPSPW, http://www.jetdrone.com/minpspw. )

The rest of this tutorial will assume that you installed the PSPSDK in C:\pspsdk.

Step 1: Install PSPLink on your PSP
  - Copy the OE version of PSPLink (C:\pspsdk\psplink\psp\oe\psplink) to PSP/GAME on the PSP, and run it on your PSP from the game menu.

Step 2: Prepare the PC.
I had a lot of trouble connecting to PSPLink on Windows 7 x64 and with modern FW, but I figured it out. Here's what you have to do:
- Disconnect the PSP from your PC.
- IMPORTANT on x64: Boot Windows using F8 at bootup to get the boot menu, and select "Disable driver signing verification". You will always need to do this when you want to use PSPLink.
- Connect your PSP running PSPLink to the PC using the USB cable.

Windows will now ask you for a driver for this new device. If it doesn't, go into Device Manager, 
find the PSP in the list, and rightclick it, go into properties and choose Update Driver. In the resulting dialog, coose things like "Don't look for a driver on Windows Update" and "I have my own driver".

The driver you want exists in either C:\pspsdk\bin\driver or C:\pspsdk\bin\driver_x64, depending on your Windows version. Windows should accept and install your driver.

One more step:
- Add C:\pspsdk\bin to your PATH if you haven't already got it.

You are now ready to roll!

In a command prompt in the directory that you want the PSP software to regard as "host0:/" if it tries to read files over the cable, type the following:

> cd pspautotests<br />
> usbhostfs_pc -b 3000

Then in another command prompt:

> pspsh -p 3000

If you now don't see a host0:/ prompt, something is wrong. Most likely the driver has not loaded correctly. If the port 3000 happened to be taken (usbhostfs_pc would have complained), try another port number.

Now you have full access to the PSP from this prompt. You can use gentest.py to run tests (e.g. `gentest.py misc/test.gp`) and update the .expected files.

You can run executables on the PSP that reside on the PC directly from within this the pspsh shell, just cd to the directory and run ./my_program.prx.

Note that you CAN'T run ELF files on modern firmware, you MUST build as .PRX. To do this, set BUILD_PRX = 1 in your makefile.

Also, somewhere in your program, add the following line to get a proper heap size:

unsigned int sce_newlib_heap_kb_size = -1;

For some probably historical reason, by default PSPSDK assumes that you want a 64k heap when you build a PRX.








@TODO: Maybe join .expected and .input file in a single .test file?

Random Ideas for .test file:
```
EXPECTED:CALL(sceDisplay.sceDisplayWaitVblank)
ACTION:BUTTON_PRESS(CROSS)
EXPECTED:OUTPUT('CROSS Pressed')
EXPECTED:CALL(sceDisplay.sceDisplayWaitVblank)
ACTION:BUTTON_RELEASE(CROSS)
```
