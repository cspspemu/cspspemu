.set noreorder

#include "pspimport.s"

# Not sure what the flags should be?
IMPORT_START "scePsmfPlayer",0x40090011

IMPORT_FUNC  "scePsmfPlayer",0x235d8787,scePsmfPlayerCreate
IMPORT_FUNC  "scePsmfPlayer",0x3ea82a4b,scePsmfPlayerGetAudioOutSize
IMPORT_FUNC  "scePsmfPlayer",0x3d6d25a9,scePsmfPlayerSetPsmf
IMPORT_FUNC  "scePsmfPlayer",0x58B83577,scePsmfPlayerSetPsmfCB
IMPORT_FUNC  "scePsmfPlayer",0xdf089680,scePsmfPlayerGetPsmfInfo
IMPORT_FUNC  "scePsmfPlayer",0x95a84ee5,scePsmfPlayerStart
IMPORT_FUNC  "scePsmfPlayer",0x46f61f8b,scePsmfPlayerGetVideoData
IMPORT_FUNC  "scePsmfPlayer",0xf8ef08a6,scePsmfPlayerGetCurrentStatus
IMPORT_FUNC  "scePsmfPlayer",0x3ed62233,scePsmfPlayerGetCurrentPts
IMPORT_FUNC  "scePsmfPlayer",0xb9848a74,scePsmfPlayerGetAudioData
IMPORT_FUNC  "scePsmfPlayer",0xa0b8ca55,scePsmfPlayerUpdate
IMPORT_FUNC  "scePsmfPlayer",0x2D0E4E0A,scePsmfPlayerSetTempBuf
IMPORT_FUNC  "scePsmfPlayer",0x1078c008,scePsmfPlayerStop
IMPORT_FUNC  "scePsmfPlayer",0xe792cd94,scePsmfPlayerReleasePsmf
IMPORT_FUNC  "scePsmfPlayer",0x9b71a274,scePsmfPlayerDelete
IMPORT_FUNC  "scePsmfPlayer",0x2beb1569,scePsmfPlayerBreak


IMPORT_START "scePsmf",0x40090011
IMPORT_FUNC  "scePsmf",0x5b70fcc1,scePsmfQueryStreamOffset
IMPORT_FUNC  "scePsmf",0x9553cc91,scePsmfQueryStreamSize