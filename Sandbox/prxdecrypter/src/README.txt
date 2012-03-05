// PRXdecrypter 2.6
// by jas0nuk (with an update from FreePlay, thanks to coyotebean's new psardumper)
/////////////////

Can decrypt/decompress/extract:
* firmware modules from official firmwares, for both old-style PSP and PSP Go
* updater modules from official updaters, including some obfuscated ones
* official updater DATA.PSP
* reboot.bin and reboot_02g.bin from all retail firmwares
* EBOOT.BIN and modules from retail games, and some nonretail/debug EBOOT.BIN files
* M33 custom firmware modules
* RLZ files
* KL3E/KL4E files
* meimg.img and me_sdimg.img
* some demos and game sharing DATA.PSP files
* index.dat
* 1SEG.PBP

///// CHANGELOG
/////////////////
2.5 --> 2.6
- Keys and tags for 5.70, 6.00, 6.10, and 6.20 (PSP Go) added (taken from coyotebean's psardumper)
- Some additional keys and tags for 6.xx on the old-style PSP
- Minor code cleanup (more to come, when I get to it)

2.4b --> 2.5
- Keys for tags 0xD91613F0 and 0x2E5E13F0 added

2.4a --> 2.4b
- Decryption error for an old key, tag 0x4C940AF0, fixed.
- Decryption key added for pops_04g.prx from 6.XX firmwares - tag 0x457B1EF0
- Slightly more information when encryption is likely to fail (SHA-1 check warning)
- NOTE: key for tag 0x2E5E10F0 does not appear to be working due to a new encryption method. I'll look into it.
- New EBOOT.BIN key added for tag 0x2E5E12F0. As above, might not work.

2.4 --> 2.4a
- New EBOOT.BIN key added - fixes "tag error 0x2E5E10F0"
- Can extract nand_updater+lfatfs_updater from old (not 6.00+) updater DATA.PSP again

2.3a --> 2.4
- All keys up to 6.20 - thanks bbtgp

2.3 --> 2.3a
- Maintenance update with the new key for 6.00-6.20 - tag 0xD91612F0 - many thanks to he who cannot be named
- Started adding some code to handle the new updater module encryption

2.25 --> 2.3
- "Analyze files" option added to menu - displays info about the files without changing them
- User module keys up to 6.00, not sure if they're the ones used in the firmware though!
- Fixed decryption issue - extra data added to decrypted files
- Rewritten kernel modules for 2.XX+ allowing fewer external files and cleaner code
- Rewritten handling of compressions - 1.50 has RLZ with the correct file, 2.71 to 3.80 have RLZ, 3.80+ has KL3E, KL4E and may have RLZ depending on the exact firmware version

2.1 --> 2.25
- 2.2 - limited release, 5.00 keys
- 2.25 - EBOOT.BIN keys up to 6.00! (Many thanks to an anonymous friend)

2.0b --> 2.1
- GZIP failure bug fixed on 3.80+
- Now bruteforces the XOR key for updater modules (appears in blue at the right of the screen), takes a longer but now it will work on all future updaters until Sony change their updater security
- User is now asked whether they want to decrypt updater modules which have just been extracted
- User is now asked whether they want to overwrite files that are read-only before saving them
- 5.00 keys added (all PSP models)

2.0 --> 2.0b
- Working KL3E decompression on 3.80/3.90 for reboot.bin extraction from these firmwares.
- New menu! Just use the D-pad to select what you want to do, and press X. Used-up or unavailable options appear grey and cannot be selected

1.9 --> 2.0
- Working KL4E decompression in 3.80/3.90
- Press LEFT on the D-Pad to signcheck all ~PSP encrypted files in the ms0:/enc folder
- Totally reorganized the source code. Everything is now in one build. Internally the code is slightly different for 1.50, 2.71~3.71, and 3.80+.
 \ All functions are now loaded from prxdecrypter_0xg.prx (1 = 1.50, 2 = 2.71~3.71, 3 = 3.80+)
 \ At the moment, RLZ decompression is available in 1.50 if you put a decrypted 2.71+ sysmem_rlz.prx into ms0:/enc
- Made the options a little more organized with a few dividers, and changed keys to make more sense
- Press SELECT to switch between the normal output folder (which overwrites your input files) and "ms0:/dec" so the input files are kept separate from the output.

1.8 --> 1.9 (Bugfix release)
- Fixed bug where pressing X for too long caused it to try and decrypt multiple times resulting in a huge log of error messages
- Rewritten method of loading files for RLZ decompression in preparation for 2.0 which will have KLE decompression
- Improvements to the logfile
- Tweak to enable 1SEG.PBP decryption
- More keys added (certain apps which run in pspbtcnf_app mode)
- Fixes to prevent crash on 3.80
 NOTE: 3.80 has removed the RLZ decompression routines and replaced them with KLE. At the moment I have no information about the KL3E/KL4E NIDs and have therefore left them out of this version
 RLZ still works fine in 3.71 and below, and in 1.50 where you can load sysmem_rlz.prx to enable it as usual

1.7 --> 1.8
- Now extracts special PRXs from updater executables - open the updater with PBP Unpacker and put the DATA.PSP (renamed to whatever) in the enc folder.
 ` It will extract all the PRXs - including the hidden ones SCE didn't want us to see (they used a lame XOR encryption to hide the new ones).
 ` A lame XOR encryption was used on the usual ones, and they double-encrypted "sceLoadExecUpdater" with a scrambled header
- Overhauled filetype detection, now works with any file that has a decryption key, not just those with ~PSP in the header.
- Plain 2RLZ decompression works again
- Reduced occurence of unnecessary unsignchecking
- Returns correct unknown tag message
- Handling for double-encrypted files with scrambled headers in order to obtain the output size, such as sceLoadExecUpdater (a PRX hidden inside updaters)
- 2.xx+ game EBOOT.BIN decryption fixed
- Added support for 1.xx (and possibly 2.00-2.50) index.dat decryption

1.6 --> 1.7
- Fully compatible with HEN/2.71/3.xx kernel/3.60 M33 - Just copy PRXdecrypter_3XX folder to GAME. Have fun, firmware modders.
 \ OE and M33 users should also use this version, just put it in GAME352 or GAME (if you have that set to the 3.52 kernel)
 \ 1.50 users (if any of you still exist) or those who want to use the 1.50 kernel, use the kxploited version.
 \ I will maintain this version in case the 3.XX kernel changes significantly which breaks PRXdecrypter
- Added 3.60 and 3.7X keys
- HEN/3.XX version doesn't need sysmem_rlz.prx for RLZ decompression as it uses the function from the 3.xx kernel
- Huge internal cleanup with clearer messages, better error handling and more speed.
- Improved logging (logs almost everything now)
- Press O to UnSignCheck all files in the ms0:/enc folder
- Press RIGHT on the D-PAD to extract reboot_02g.bin from loadexec_reboot_02g.prx (from 3.60 or higher)

1.5b --> 1.6
- Now comes with a HEN compatible EBOOT
- Added 6 more key sets (1.XX, 2.00-2.50, 2.60-2.71, 2.80, 3.00 and 3.10+ gameshare keys)
- Fixed 2.7X demo decryption
- Removed 5 useless keys
- Now uses the entire buffer - previously, files over 4mb would fail
- Added second layer of descrambling to a few files - this would have produced messed up output if you tried in the old version

1.5 --> 1.5b
- Added decryption for:
 \ 1.XX game EBOOT.BIN files
 \ New game EBOOT.BIN types which aren't yet being used (4 different keys)
 \ A few unknown filetypes (keys found in mesg_led)
 \ DATA.PSP from 2.7X, 2.8X and 3.XX demos (4 different keys)
 \ DATA.PSP from official updaters
 \ 2.50/2.60 meimg.img and me_sdimg.img (2 different keys)
- NOTE: If you want to decrypt a file, make sure it has the 4 byte "~PSP" header, or it will be skipped by the decrypt function. You can easily add this using a hex editor.
- Better checks before attempting to "UnSignCheck" files
- Fixed an incorrect error message
- Fixed a logging bug which logged "fail" and "success" at the same time
- Increased buffers to 10mb each to allow large files to be decrypted

1.4b --> 1.5
- More source cleanup and improved error handling: If something fails to decrypt or decompress it saves as much as possible, or avoids saving to prevent memory stick corruption. (No more deletion of files for no reason)
- Can decompress plain 2RLZ files in the ms0:/enc folder (extracted from RCOs or whatever)
- Improved method of initializing 2RLZ decompressor
- Now writes a log of all activities to ms0:/enc/log.txt
- Fixed a bug which prevented the header of extremely small files (less than 208 bytes) being found
- Retail game EBOOT.BIN decryption
- Improved M33 PRX detection to reduce false positives
- SHA-1 check removed to allow files such as vshmain.prx from 3.5X to decrypt

1.4 --> 1.4b
- M33 decompression fixed, no longer interferes with official PRXs
- Source cleanup

1.3 --> 1.4
- Decompresses PRXs from M33 custom firmware
- Improved error handling of corrupted and already decompressed files
- Improved path handling, hopefully solving problems opening files

1.2 --> 1.3
- 3.30 support

1.1 --> 1.2
- 3.10/3.11 support
- Changed name

1.0 --> 1.1
- Supports sigchecked files if they were signed on the PSP they are being decrypted on
- Can decompress RLZ files
- Changed name of external file (reflected in the instructions)
- Much better error handling. Note: if a file entirely fails to decrypt (e.g. EBOOT.BIN) it will be deleted to prevent memory stick corruption.
- To save space, paths are printed as just the filename, e.g 'audio.prx' instead of 'ms0:/enc/audio.prx' whilst decrypting

1.0
- First version

///// INSTRUCTIONS
////////////////////

Make a folder on your memory stick called "enc" and place your encrypted or compressed files there.

Choose the "Decrypt/decompress files" option to decrypt and decompress all files in the ms0:/enc folder

Choose "Analyze files" to print information about all files in ms0:/enc without changing them

You can also choose "Unsigncheck only" which will unsigncheck encrypted files (makes them generic for any PSP)
"Signcheck only" will resigncheck unsignchecked files, making them PSP-specific again.
So for example, if you wanted to restore a flash0 dump from another PSP, you'd have to unsign it on the source PSP, and resign it on the destination PSP, then copy it to flash0.

These options will overwrite the input files with the decrypted/decompressed output. If you don't want this to be the case, choose the "Switch output folder" menu option. All decrypted/decompressed files will then be saved in the ms0:/dec folder, however files in subfolders (except for updaterprx) will not be saved as the subfolders will not exist. You have to set up the folder layout.

Files can only be unsignchecked on the PSP they were signchecked on.

The maximum file size that can be decrypted or decompressed is 10mb due to limitations of the hardware and buffering method, though you will probably never need more than this.

To extract updater modules, place an encrypted DATA.PSP from an official updater in the enc Folder. Press X to decrypt all files as usual.
The modules will be decoded (if necessary) and saved to the 'updaterprx' folder which will be made automatically.
If you are extracting modules from multiple updaters, the first updaterprx folder will be renamed to updaterprx_0000, then updaterprx_0001, and so on.
If you want to decrypt the modules, you are given the option to after extraction has finished.

The app can also extract reboot.bin from a 1.50 loadexec.prx or later, and reboot_02g.bin (for the Slim hardware) from 3.60 loadexec.prx or later:
If using on 1.50 kernel, place a decrypted 2.71-3.30 sysmem.prx in the "ms0:/enc" folder and name it as "sysmem_rlz.prx"
This is not necessary when running on 2.71 or higher, as the firmware already contains the RLZ decompression routines.
Place a decrypted copy of loadexec.prx from the firmware you want reboot.bin for, and rename it to "loadexec_reboot.prx"
For reboot_02g.bin, get a decrypted 3.60+ loadexec.prx and rename it to "loadexec_reboot_02g.prx"
When these files are detected, the option to extract them will become available on the menu.

///// THANKS
/////////////////

Everyone at LAN.st/Malloc.us/Dark-AleX.org/Exophase
Dark_AleX - for unsigncheck, for SE/OE/M33, and for various decryption keys
Team Noobz/C+D - for 3.00~3.52 encryption keys
PspPet - for the original psardumper
SilverSpring - for various pieces of info about keys and decryption
Chilly Willy - for excellent 3.xx code examples and automatic folder renaming


Enjoy ;)

~jas0nuk
