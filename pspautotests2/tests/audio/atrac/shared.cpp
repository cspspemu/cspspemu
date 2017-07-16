#include "shared.h"
#include <stdio.h>
#include <malloc.h>
#include <psputility.h>

void LoadAtrac() {
	int success = 0;
	success |= sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	success |= sceUtilityLoadModule(PSP_MODULE_AV_ATRAC3PLUS);
	if (success != 0) {
		printf("TEST FAILURE: unable to load sceAtrac.\n");
		exit(1);
	}
}

void UnloadAtrac() {
	sceUtilityUnloadModule(PSP_MODULE_AV_ATRAC3PLUS);
	sceUtilityUnloadModule(PSP_MODULE_AV_AVCODEC);
}

Atrac3File::Atrac3File(const char *filename) : data_(NULL) {
	Reload(filename);
}

Atrac3File::~Atrac3File() {
	delete [] data_;
	data_ = NULL;
}

void Atrac3File::Reload(const char *filename) {
	delete [] data_;
	data_ = NULL;

	FILE *file = fopen(filename, "rb");
	if (file != NULL) {
		fseek(file, 0, SEEK_END);
		size_ = ftell(file);
		data_ = new char[size_];
		memset(data_, 0, size_);

		fseek(file, 0, SEEK_SET);
		fread(data_, size_, 1, file);

		fclose(file);
	}
}

void Atrac3File::Require() {
	if (!IsValid()) {
		printf("TEST FAILURE: unable to read sample.at3\n");
		exit(1);
	}
}
