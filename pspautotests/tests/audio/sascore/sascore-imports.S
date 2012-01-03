.set noreorder

#include "pspimport.s"

IMPORT_START "sceSasCore",0x40090011
IMPORT_FUNC	 "sceSasCore",0x42778A9F,__sceSasInit
IMPORT_FUNC	 "sceSasCore",0xE855BF76,__sceSasSetOutputmode
IMPORT_FUNC	 "sceSasCore",0x99944089,__sceSasSetVoice
IMPORT_FUNC	 "sceSasCore",0xAD84D37F,__sceSasSetPitch
IMPORT_FUNC	 "sceSasCore",0x440CA7D8,__sceSasSetVolume
IMPORT_FUNC	 "sceSasCore",0x76F01ACA,__sceSasSetKeyOn
IMPORT_FUNC	 "sceSasCore",0xA3589D81,__sceSasCore
