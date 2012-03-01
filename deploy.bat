@echo off
copy /Y cspspemu.exe deploy\cspspemu\cspspemu.exe
PUSHD deploy
DEL /Q cspspemu.7z
RD /S /Q cspspemu\ms
..\utils\7z\7z a -t7z cspspemu.7z cspspemu -aoa -m8=LZMA
POPD