#include <common.h>

#include <pspkernel.h>
#include <pspsdk.h>
#include <psptypes.h>
#include <psppower.h>

#define DISPLAY_VIDEO 1

//#include "pmfplayer.h"
#include <psputilsforkernel.h>
#include <pspdisplay.h>
#include <pspge.h>
#include <pspgu.h>
#include <pspctrl.h>
#include <pspaudio.h>
#include <psputility.h>

#include <stdio.h>
#include <malloc.h>
#include <string.h>

#include <pspmpeg.h>

#define BUFFER_WIDTH 512
#define SWAPINT(x) (((x)<<24) | (((uint)(x)) >> 24) | (((x) & 0x0000FF00) << 8) | (((x) & 0x00FF0000) >> 8))

int retVal;
SceMpegAvcMode m_MpegAvcMode;

typedef struct SceMpegRingbuffer2
{
	s32 packetsTotal;
	s32 packetsRead;
	s32 packetsWritten;
	s32 packetsAvail;
	s32 packetSize;
	void *data;
	sceMpegRingbufferCB callback;
	void *callbackArg;
	void *dataEnd;
	int unknownValue;
	SceMpeg mpeg;
	u32 gp;
} SceMpegRingbuffer2;

SceUID                              m_FileHandle;
SceInt32                            m_MpegStreamOffset;
SceInt32                            m_MpegStreamSize;

SceMpeg                             m_Mpeg;
SceInt32                            m_MpegMemSize;
ScePVoid                            m_MpegMemData;

SceInt32                            m_RingbufferPackets;
SceInt32                            m_RingbufferSize;
ScePVoid                            m_RingbufferData;
SceMpegRingbuffer2                  m_Ringbuffer;

SceMpegStream*                      m_MpegStreamAVC;
ScePVoid                            m_pEsBufferAVC;
SceMpegAu                           m_MpegAuAVC;

SceMpegStream*                      m_MpegStreamAtrac;
ScePVoid                            m_pEsBufferAtrac;
SceMpegAu                           m_MpegAuAtrac;

SceInt32                            m_MpegAtracEsSize;
SceInt32                            m_MpegAtracOutSize;

SceInt32                            m_iLastTimeStamp;

SceInt32 RingbufferCallback(ScePVoid pData, SceInt32 iNumPackets, ScePVoid pParam)
{
	int retVal, iPackets;
	SceUID hFile = *(SceUID*)pParam;

	printf("RingbufferCallback(pData=0x%08X, iNumPackets=0x%08X, pParam=0x%08X)\n", (unsigned int)pData, (unsigned int)iNumPackets, (unsigned int)pParam);

	retVal = sceIoRead(hFile, pData, iNumPackets * 2048);
	if(retVal < 0) return -1;

	iPackets = retVal / 2048;

	return iPackets;
}

SceInt32 ParseHeader()
{
	int retVal;
	char * pHeader = (char *)malloc(2048);

	sceIoLseek(m_FileHandle, 0, SEEK_SET);

	retVal = sceIoRead(m_FileHandle, pHeader, 2048);
	if (retVal < 2048)
	{
		printf("sceIoRead() failed!\n");
		goto error;
	}

	printf("sceMpegQueryStreamOffset     :0x%08X\n", (unsigned int)(retVal = sceMpegQueryStreamOffset(&m_Mpeg, pHeader, &m_MpegStreamOffset)));
	printf("  value: 0x%08X\n", (unsigned int)m_MpegStreamOffset);
	if (retVal != 0)
	{
		printf("sceMpegQueryStreamOffset() failed: 0x%08X\n", retVal);
		goto error;
	}

	printf("sceMpegQueryStreamSize       :0x%08X\n", (unsigned int)(retVal = sceMpegQueryStreamSize(pHeader, &m_MpegStreamSize)));
	printf("  value: 0x%08X\n", (unsigned int)m_MpegStreamSize);
	if (retVal != 0)
	{
		printf("sceMpegQueryStreamSize() failed: 0x%08X\n", retVal);
		goto error;
	}

	m_iLastTimeStamp = *(int*)(pHeader + 80 + 12);
	m_iLastTimeStamp = SWAPINT(m_iLastTimeStamp);

	printf("m_iLastTimeStamp             :0x%08X\n", (unsigned int)(m_iLastTimeStamp));

	free(pHeader);

	sceIoLseek(m_FileHandle, m_MpegStreamOffset, SEEK_SET);

	return 0;

error:
	free(pHeader);
	return -1;
}

typedef struct {
	int   packets;
	uint  packetsRead;
	uint  packetsWritten;
	uint  packetsFree;
	uint  packetSize;
	void* data;
	uint  callback;
	void* callbackParameter;
	void* dataUpperBound;
	int   semaId;
	SceMpeg* mpeg;
} _SceMpegRingbuffer;
typedef struct {
	uint   magic1;
	uint   magic2;
	uint   magic3;
	uint   unk_m1;
	void*  ringbuffer_start;
	void*  ringbuffer_end;
} _SceMpeg;

void DumpSceMpegRingbuffer(SceMpegRingbuffer *ringBuffer) {
	printf("    iPackets  : 0x%08X\n", (unsigned int)ringBuffer->iPackets);
	printf("    iUnk0     : 0x%08X\n", (unsigned int)ringBuffer->iUnk0);
	printf("    iUnk1     : 0x%08X\n", (unsigned int)ringBuffer->iUnk1);
	printf("    iUnk2     : 0x%08X\n", (unsigned int)ringBuffer->iUnk2);
	printf("    iUnk3     : 0x%08X\n", (unsigned int)ringBuffer->iUnk3);
	printf("    pData     : 0x%08X\n", (unsigned int)ringBuffer->pData);
	printf("    Callback  : 0x%08X\n", (unsigned int)ringBuffer->Callback);
	printf("    pCBparam  : 0x%08X\n", (unsigned int)ringBuffer->pCBparam);
	printf("    iUnk4     : 0x%08X\n", (unsigned int)ringBuffer->iUnk4);
	printf("    iUnk5     : 0x%08X\n", (unsigned int)ringBuffer->iUnk5);
	printf("    pSceMpeg  : 0x%08X\n", (unsigned int)ringBuffer->pSceMpeg);
	/*
	printf("    packets          : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->packets);
	printf("    packetsRead      : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->packetsRead);
	printf("    packetsWritten   : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->packetsWritten);
	printf("    packetsFree      : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->packetsFree);
	printf("    packetSize       : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->packetSize);
	printf("    data             : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->data);
	printf("    callback         : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->callback);
	printf("    callbackParameter: 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->callbackParameter);
	printf("    dataUpperBound   : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->dataUpperBound);
	printf("    semaId           : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->semaId);
	printf("    mpeg             : 0x%08X\n", (unsigned int)((_SceMpegRingbuffer *)&ringBuffer)->mpeg);
	*/
	fflush(stdout);
}

void DumpSceMpegAu(SceMpegAu *mpegAu) {
	printf("     iPtsMSB        : 0x%08X\n", (unsigned int)(mpegAu->iPtsMSB));
	printf("     iPts           : 0x%08X\n", (unsigned int)(mpegAu->iPts));
	printf("     iDtsMSB        : 0x%08X\n", (unsigned int)(mpegAu->iDtsMSB));
	printf("     iDts           : 0x%08X\n", (unsigned int)(mpegAu->iDts));
	printf("     iEsBuffer      : 0x%08X\n", (unsigned int)(mpegAu->iEsBuffer));
	printf("     iAuSize        : 0x%08X\n", (unsigned int)(mpegAu->iAuSize));
}

void LoadAndDecode(char * pFileName) {
	m_RingbufferPackets = 0x3C0;

	int status = 0;
	status |= sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_ATRAC3PLUS);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_MP3);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_MPEGBASE);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_VAUDIO);
	printf("sceUtilityLoadModule         :0x%08x\n", (unsigned int)status);
	
	// Init.
	{
		printf("sceMpegInit                  :0x%08X\n", (unsigned int)sceMpegInit());
		printf("sceMpegRingbufferQueryMemSize:0x%08X\n", (unsigned int)(m_RingbufferSize = sceMpegRingbufferQueryMemSize(m_RingbufferPackets)));
		
		printf("sceMpegQueryMemSize          :0x%08X\n", (unsigned int)(m_MpegMemSize    = sceMpegQueryMemSize(0)));
		printf("m_RingbufferData             :0x%08X\n", (unsigned int)(m_RingbufferData = malloc(m_RingbufferSize)));
		printf("m_MpegMemData                :0x%08X\n", (unsigned int)(m_MpegMemData    = malloc(m_MpegMemSize)));
		printf("sceMpegRingbufferConstruct   :0x%08X\n", (unsigned int)(sceMpegRingbufferConstruct((SceMpegRingbuffer *) &m_Ringbuffer, m_RingbufferPackets, m_RingbufferData, m_RingbufferSize, &RingbufferCallback, &m_FileHandle)));
		DumpSceMpegRingbuffer((SceMpegRingbuffer *) &m_Ringbuffer);
		printf("sceMpegCreate                :0x%08X\n", (unsigned int)(sceMpegCreate(&m_Mpeg, m_MpegMemData, m_MpegMemSize, (SceMpegRingbuffer *) &m_Ringbuffer, BUFFER_WIDTH, 0, 0)));
		printf("    pointer          : '%s'\n"  , (char *)m_Mpeg);
		printf("    unk_m1           : 0x%08X\n", (unsigned int)((_SceMpeg *)m_Mpeg)->unk_m1);
		printf("    ringbuffer_start : 0x%08X\n", (unsigned int)((_SceMpeg *)m_Mpeg)->ringbuffer_start);
		printf("    ringbuffer_end   : 0x%08X\n", (unsigned int)((_SceMpeg *)m_Mpeg)->ringbuffer_end);
		
		m_MpegAvcMode.iUnk0 = -1;
		m_MpegAvcMode.iPixelFormat = 3;
		printf("sceMpegAvcDecodeMode         :0x%08X\n", (unsigned int)(sceMpegAvcDecodeMode(&m_Mpeg, &m_MpegAvcMode)));
	}
	// Load.
	{
		printf("sceIoOpen                    :0x%08X\n", (unsigned int)(m_FileHandle = sceIoOpen(pFileName, PSP_O_RDONLY, 0777)));
		ParseHeader();
		printf("sceMpegRegistStream          :0x%08X\n", (unsigned int)(m_MpegStreamAVC = sceMpegRegistStream(&m_Mpeg, 0, 0)));
		printf("sceMpegRegistStream          :0x%08X\n", (unsigned int)(m_MpegStreamAtrac = sceMpegRegistStream(&m_Mpeg, 1, 0)));
		printf("sceMpegMallocAvcEsBuf        :0x%08X\n", (unsigned int)(m_pEsBufferAVC = sceMpegMallocAvcEsBuf(&m_Mpeg)));
		printf("sceMpegInitAu                :0x%08X\n", (unsigned int)(retVal = sceMpegInitAu(&m_Mpeg, m_pEsBufferAVC, &m_MpegAuAVC)));
		printf("   iPtsMSB           : 0x%08X\n", (unsigned int)m_MpegAuAVC.iPtsMSB);
		printf("   iPts              : 0x%08X\n", (unsigned int)m_MpegAuAVC.iPts);
		printf("   iDtsMSB           : 0x%08X\n", (unsigned int)m_MpegAuAVC.iDtsMSB);
		printf("   iDts              : 0x%08X\n", (unsigned int)m_MpegAuAVC.iDts);
		printf("   iEsBuffer         : 0x%08X\n", (unsigned int)m_MpegAuAVC.iEsBuffer);
		printf("   iAuSize           : 0x%08X\n", (unsigned int)m_MpegAuAVC.iAuSize);
		printf("sceMpegQueryAtracEsSize      :0x%08X\n", (unsigned int)(retVal = sceMpegQueryAtracEsSize(&m_Mpeg, &m_MpegAtracEsSize, &m_MpegAtracOutSize)));
		printf("   m_MpegAtracEsSize : %d\n", (int)m_MpegAtracEsSize);
		printf("   m_MpegAtracOutSize: %d\n", (int)m_MpegAtracOutSize);
		printf("m_pEsBufferAtrac             :0x%08X\n", (unsigned int)(m_pEsBufferAtrac = memalign(64, m_MpegAtracEsSize)));
		printf("sceMpegInitAu                :0x%08X\n", (unsigned int)(retVal = sceMpegInitAu(&m_Mpeg, m_pEsBufferAtrac, &m_MpegAuAtrac)));
		printf("   iPtsMSB           : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iPtsMSB);
		printf("   iPts              : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iPts);
		printf("   iDtsMSB           : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iDtsMSB);
		printf("   iDts              : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iDts);
		printf("   iEsBuffer         : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iEsBuffer);
		printf("   iAuSize           : 0x%08X\n", (unsigned int)m_MpegAuAtrac.iAuSize);
	}
	// Decode.
	{
		int iInitAudio = 1;
		int iVideoStatus = 0;
		int iFreePackets;
		int iReadPackets;
		int iPackets;
		int result;
		int n;
		int m_MpegAvcOutSize = 512 * 272 * 4;
		SceInt32 atrac3PlusPointer = 0;
		unsigned char *m_pVideoBuffer = memalign(64, m_MpegAvcOutSize);
		//unsigned char *m_pVideoBuffer = malloc( m_MpegAvcOutSize);
		unsigned char *m_pAudioBuffer = memalign(64, m_MpegAtracOutSize);
		memset(m_pVideoBuffer, 0x77, m_MpegAvcOutSize);
		memset(m_pAudioBuffer, 0x77, m_MpegAtracOutSize);

		for (n = 0; n < 60; n++) {
			printf("sceMpegRingbufferAvailableSize: 0x%08X\n", (unsigned int)(iFreePackets = sceMpegRingbufferAvailableSize((SceMpegRingbuffer *) &m_Ringbuffer)));
			iReadPackets = iFreePackets;
			//if (iFreePackets > 0)
			{
				printf("sceMpegRingbufferPut...\n");
				printf("sceMpegRingbufferPut          : 0x%08X\n", (unsigned int)(iPackets = sceMpegRingbufferPut((SceMpegRingbuffer *) &m_Ringbuffer, iReadPackets, iFreePackets)));
				DumpSceMpegRingbuffer((SceMpegRingbuffer *) &m_Ringbuffer);
				printf("sceMpegRingbufferAvailableSize: 0x%08X\n", (unsigned int)(iFreePackets = sceMpegRingbufferAvailableSize((SceMpegRingbuffer *) &m_Ringbuffer)));
			}
			
			fflush(stdout);
			printf("sceMpegGetAtracAu             : 0x%08X\n", (unsigned int)(result = sceMpegGetAtracAu(&m_Mpeg, &m_MpegStreamAtrac, &m_MpegAuAtrac, &atrac3PlusPointer)));
			DumpSceMpegAu(&m_MpegAuAtrac);
			fflush(stdout);
			printf("sceMpegAtracDecode            : 0x%08X\n", (unsigned int)(result = sceMpegAtracDecode(&m_Mpeg, &m_MpegAuAtrac, m_pAudioBuffer, &iInitAudio)));
			fflush(stdout);
			printf("     DATA: %02X %02X %02X %02X\n", m_pAudioBuffer[0], m_pAudioBuffer[1], m_pAudioBuffer[2], m_pAudioBuffer[3]);
			DumpSceMpegAu(&m_MpegAuAtrac);
			fflush(stdout);
			iInitAudio = 0;

			fflush(stdout);
			printf("sceMpegGetAvcAu               : 0x%08X\n", (unsigned int)(result = sceMpegGetAvcAu (&m_Mpeg, m_MpegStreamAVC, &m_MpegAuAVC, &atrac3PlusPointer)));
			DumpSceMpegAu(&m_MpegAuAVC);
			fflush(stdout);
			printf("sceMpegAvcDecode              : 0x%08X\n", (unsigned int)(result = sceMpegAvcDecode(&m_Mpeg, &m_MpegAuAVC, BUFFER_WIDTH, &m_pVideoBuffer, &iVideoStatus)));
			fflush(stdout);
			printf("     Init: %d\n", iVideoStatus);
			printf("     DATA: %02X %02X %02X %02X\n", m_pVideoBuffer[0], m_pVideoBuffer[1], m_pVideoBuffer[2], m_pVideoBuffer[3]);
			DumpSceMpegAu(&m_MpegAuAVC);
			fflush(stdout);
		}
		
		printf("sceMpegAvcDecodeStop          : 0x%08X\n", (unsigned int)(result = sceMpegAvcDecodeStop (&m_Mpeg, BUFFER_WIDTH, &m_pVideoBuffer, &iVideoStatus)));
		printf("sceMpegFlushAllStream         : 0x%08X\n", (unsigned int)(result = sceMpegFlushAllStream(&m_Mpeg)));
	}
	// Shutdown.
	{
		if (m_pEsBufferAtrac  != NULL) free(m_pEsBufferAtrac);
		if (m_pEsBufferAVC    != NULL) sceMpegFreeAvcEsBuf(&m_Mpeg, m_pEsBufferAVC);
		if (m_MpegStreamAVC   != NULL) sceMpegUnRegistStream(&m_Mpeg, m_MpegStreamAVC);
		if (m_MpegStreamAtrac != NULL) sceMpegUnRegistStream(&m_Mpeg, m_MpegStreamAtrac);
		if (m_FileHandle      > -1   ) sceIoClose(m_FileHandle);

		sceMpegDelete(&m_Mpeg);
		sceMpegRingbufferDestruct((SceMpegRingbuffer *) &m_Ringbuffer);
		sceMpegFinish();

		if (m_RingbufferData != NULL) free(m_RingbufferData);
		if (m_MpegMemData    != NULL) free(m_MpegMemData);
	}
}

int main(int argc, char *argv[]) {
	LoadAndDecode("test.pmf");

	return 0;
}
