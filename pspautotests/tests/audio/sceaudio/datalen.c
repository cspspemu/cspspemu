#include "shared.h"

int main(int argc, char *argv[]) {
	int i;
	int channel = sceAudioChReserve(PSP_AUDIO_NEXT_CHANNEL, 4096, PSP_AUDIO_FORMAT_STEREO);

	checkpointNext("sceAudioSetChannelDataLen:");
	checkpoint("  Normal: %08x", sceAudioSetChannelDataLen(channel, 4096));
	checkpoint("  Bad channel: %08x", sceAudioSetChannelDataLen(-1, 4096));
	checkpoint("  Unreseved channel: %08x", sceAudioSetChannelDataLen(5, 4096));

	const static int sampleCounts[] = {-1, 0, 1, 32, 64, 96, 256, 1024, 4111, 4112, 65472, 65536};
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		checkpoint("  %d samples: %08x", sampleCounts[i], sceAudioSetChannelDataLen(channel, sampleCounts[i]));
	}

	checkpointNext("sceAudioOutput2ChangeLength:");
	checkpoint("  Reserve: %08x", sceAudioOutput2Reserve(1024));
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		checkpoint("  %d samples: %08x", sampleCounts[i], sceAudioOutput2ChangeLength(sampleCounts[i]));
	}
	checkpoint("  Release: %08x", sceAudioOutput2Release());

	return 0;
}