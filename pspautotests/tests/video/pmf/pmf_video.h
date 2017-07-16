static unsigned int __attribute__((aligned(64)))  DisplayList[128 * 1024 * 4];

SceInt32 AVSyncStatus(DecoderThreadData* D)
{
	if(D->Audio->m_iFullBuffers == 0 || D->Video->m_iFullBuffers == 0) return 1;

	int iAudioTS = D->Audio->m_iBufferTimeStamp[D->Audio->m_iPlayBuffer];
	int iVideoTS = D->Video->m_iBufferTimeStamp[D->Video->m_iPlayBuffer];

	// if video ahead of audio, do nothing
	if (iVideoTS - iAudioTS > 2 * D->m_iVideoFrameDuration) return 0;

	// if audio ahead of video, skip frame
	if (iAudioTS - iVideoTS > 2 * D->m_iVideoFrameDuration) return 2;

	return 1;
}

int RenderFrame(int width, int height, void* Buffer)
{
	sceGuStart(GU_DIRECT, DisplayList);
	{
		struct Vertex* Vertices = (struct Vertex*)sceGuGetMemory(2 * sizeof(struct Vertex));

		Vertices[0].u = 0;        Vertices[0].v = 0;
		Vertices[0].x = 0;        Vertices[0].y = 0;        Vertices[0].z = 0;
		Vertices[1].u = width;    Vertices[1].v = height;
		Vertices[1].x = SCREEN_W; Vertices[1].y = SCREEN_H; Vertices[1].z = 0;

		sceGuTexImage(0, TEXTURE_W, TEXTURE_H, BUFFER_WIDTH, Buffer);
		sceGuDrawArray(GU_SPRITES, GU_TEXTURE_16BIT | GU_VERTEX_16BIT | GU_TRANSFORM_2D, 2, 0, Vertices);
	}
	sceGuFinish();
	sceGuSync(0, 0);
	sceDisplayWaitVblankStart();
	sceGuSwapBuffers();

	return 0;
}

int T_Video(SceSize _args, void *_argp)
{
	int iSyncStatus = 1;

	DecoderThreadData* D = *((DecoderThreadData**)_argp);

	sceKernelWaitSema(D->Video->m_SemaphoreStart, 1, 0);

	for (;;)
	{
		if (D->Video->m_iAbort != 0) break;

		if (D->Video->m_iFullBuffers > 0)
		{
			iSyncStatus = AVSyncStatus(D);

			if (iSyncStatus > 0)
			{
				if(iSyncStatus == 1) RenderFrame(D->Video->m_iWidth, D->Video->m_iHeight, D->Video->m_pVideoBuffer[D->Video->m_iPlayBuffer]);
				sceKernelWaitSema(D->Video->m_SemaphoreLock, 1, 0);

				D->Video->m_iFullBuffers--;
				sceKernelSignalSema(D->Video->m_SemaphoreLock, 1);
			}
		}
		else
		{
			sceDisplayWaitVblankStart();
		}

		sceKernelSignalSema(D->Video->m_SemaphoreWait, 1);

		sceDisplayWaitVblankStart();
	}

	while (D->Video->m_iFullBuffers > 0)
	{
		RenderFrame(D->Video->m_iWidth, D->Video->m_iHeight, D->Video->m_pVideoBuffer[D->Video->m_iPlayBuffer]);

		sceKernelWaitSema(D->Video->m_SemaphoreLock, 1, 0);

		D->Video->m_iFullBuffers--;

		sceKernelSignalSema(D->Video->m_SemaphoreLock, 1);

		sceDisplayWaitVblankStart();
	}

	sceGuTerm();

	sceKernelExitThread(0);

	return 0;
}

SceInt32 InitVideo()
{
	printf("InitVideo\n");
	
	Video.m_ThreadID = sceKernelCreateThread("video_thread", T_Video, 0x3F, 0x10000, PSP_THREAD_ATTR_USER, NULL);
	if (Video.m_ThreadID < 0)
	{
		printf("sceKernelCreateThread() failed: 0x%08X\n", (int)Video.m_ThreadID);
		return -1;
	}

	Video.m_SemaphoreStart = sceKernelCreateSema("video_start_sema", 0, 0, 1, NULL);
	if (Video.m_SemaphoreStart < 0)
	{
		printf("sceKernelCreateSema() failed: 0x%08X\n", (int)Video.m_SemaphoreStart);
		goto exit0;
	}

	Video.m_SemaphoreWait = sceKernelCreateSema("video_wait_sema", 0, 1, 1, NULL);
	if (Video.m_SemaphoreWait < 0)
	{
		printf("sceKernelCreateSema() failed: 0x%08X\n", (int)Video.m_SemaphoreWait);
		goto exit1;
	}

	Video.m_SemaphoreLock = sceKernelCreateSema("video_lock_sema", 0, 1, 1, NULL);
	if (Video.m_SemaphoreLock < 0)
	{
		printf("sceKernelCreateSema() failed: 0x%08X\n", (int)Video.m_SemaphoreLock);
		goto exit2;
	}

	Video.m_iBufferTimeStamp[0]  = 0;
	Video.m_iNumBuffers          = 1;
	Video.m_iFullBuffers         = 0;
	Video.m_iPlayBuffer          = 0;
	Video.m_iAbort               = 0;

	// not sure how to get these
	Video.m_iWidth               = 480;
	Video.m_iHeight              = 272;

#ifndef DISPLAY_VIDEO
	Video.m_pVideoBuffer[0] = malloc(512 * 272 * 4);
#else
	Video.m_pVideoBuffer[0]      = ((char *)sceGeEdramGetAddr()) + DRAW_BUFFER_SIZE + DISP_BUFFER_SIZE;
	sceGuInit      ();
	sceGuStart     (GU_DIRECT, DisplayList);
	sceGuDrawBuffer(GU_PSM_8888, (void*)0, BUFFER_WIDTH);
	sceGuDispBuffer(SCREEN_W, SCREEN_H, (void*)(DRAW_BUFFER_SIZE), BUFFER_WIDTH);
	sceGuDisable   (GU_SCISSOR_TEST);
	sceGuTexFilter (GU_NEAREST,GU_LINEAR);
	sceGuTexMode   (GU_PSM_8888, 0, 0, GU_FALSE);  
	sceGuEnable    (GU_TEXTURE_2D);
	sceGuColor     (0xFFFFFFFF);
	sceGuFinish    ();
	sceGuSync      (0, 0);
	sceDisplayWaitVblankStart();
	sceGuDisplay   (GU_TRUE);
	sceGuClear     (GU_COLOR_BUFFER_BIT);
#endif
	
	return 0;

exit2:
	sceKernelDeleteSema(Video.m_SemaphoreWait);
exit1:
	sceKernelDeleteSema(Video.m_SemaphoreStart);
exit0:
	sceKernelDeleteThread(Video.m_ThreadID);

	return -1;
}

SceInt32 ShutdownVideo()
{
	printf("ShutdownVideo\n");
	sceKernelDeleteThread(Video.m_ThreadID);

	sceKernelDeleteSema(Video.m_SemaphoreStart);
	sceKernelDeleteSema(Video.m_SemaphoreWait);
	sceKernelDeleteSema(Video.m_SemaphoreLock);

	return 0;
}