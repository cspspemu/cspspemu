@echo off
SET OBJS=
SET OBJS=%OBJS% 
SET OBJS=%OBJS% MiniPlayer\MiniPlayer.cpp
SET OBJS=%OBJS% MiniPlayer\MaiWaveOut.cpp
SET OBJS=%OBJS% src\base\Heap_Alloc0.cc
SET OBJS=%OBJS% src\base\MaiFile.cc
SET OBJS=%OBJS% src\base\MaiBufIO.cc
SET OBJS=%OBJS% src\base\Mai_Sleep.cc
SET OBJS=%OBJS% src\base\MaiThread.cc
SET OBJS=%OBJS% src\base\MaiCriticalSection.cc
SET OBJS=%OBJS% src\base\MaiString.cc
SET OBJS=%OBJS% src\base\Unicode.cc
SET OBJS=%OBJS% src\base\Mai_mem.cc
SET OBJS=%OBJS% src\base\MaiQueue0.cc
SET OBJS=%OBJS% src\base\MaiBitReader.cc
SET OBJS=%OBJS% src\MaiAT3PlusCoreDecoder.cpp
SET OBJS=%OBJS% src\MaiAT3PlusCoreDecoder_DecFunc.cpp
SET OBJS=%OBJS% src\MaiAT3PlusCoreDecoder_StaticData.cpp
SET OBJS=%OBJS% src\MaiAT3PlusCoreDecoder_SubFunc.cpp
SET OBJS=%OBJS% src\MaiAT3PlusFrameDecoder.cpp

cl /Iinclude kernel32.lib user32.lib winmm.lib %OBJS%
