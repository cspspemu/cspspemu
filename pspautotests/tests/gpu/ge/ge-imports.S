.set noreorder

#include "pspimport.s"

IMPORT_START "sceGe_user",0x40010000
IMPORT_FUNC  "sceGe_user",0x1F6752AD,sceGeEdramGetSize
IMPORT_FUNC  "sceGe_user",0xE47E40E4,sceGeEdramGetAddr
IMPORT_FUNC  "sceGe_user",0xB77905EA,sceGeEdramSetAddrTranslation
IMPORT_FUNC  "sceGe_user",0xDC93CFEF,sceGeGetCmd
IMPORT_FUNC  "sceGe_user",0x57C8945B,sceGeGetMtx
IMPORT_FUNC  "sceGe_user",0xE66CB92E,sceGeGetStack
IMPORT_FUNC  "sceGe_user",0x438A385A,sceGeSaveContext
IMPORT_FUNC  "sceGe_user",0x0BF608FB,sceGeRestoreContext
IMPORT_FUNC  "sceGe_user",0xAB49E76A,sceGeListEnQueue
IMPORT_FUNC  "sceGe_user",0x1C0D95A6,sceGeListEnQueueHead
IMPORT_FUNC  "sceGe_user",0x5FB86AB0,sceGeListDeQueue
IMPORT_FUNC  "sceGe_user",0xE0D68148,sceGeListUpdateStallAddr
IMPORT_FUNC  "sceGe_user",0x03444EB4,sceGeListSync
IMPORT_FUNC  "sceGe_user",0xB287BD61,sceGeDrawSync
IMPORT_FUNC  "sceGe_user",0xB448EC0D,sceGeBreak
IMPORT_FUNC  "sceGe_user",0x4C06E472,sceGeContinue
IMPORT_FUNC  "sceGe_user",0xA4FC06A4,sceGeSetCallback
IMPORT_FUNC  "sceGe_user",0x05DB22CE,sceGeUnsetCallback
