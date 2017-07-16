#include <common.h>
#include <inttypes.h>
#include <psputils.h>

const char *random_data = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

struct MD5Digest {
	unsigned int data[4];
};

struct Sha1Digest {
	unsigned int data[5];
};

// No idea how big this is so let's make it big!
struct MD5Context {
	char data[1024];
};

struct Sha1Context {
	char data[1024];
};

struct MD5Context md5ctx;
struct Sha1Context sha1ctx;

void checkMD5() {
	const int random_data_len = strlen(random_data);
	sceKernelUtilsMd5BlockInit(&md5ctx);
	sceKernelUtilsMd5BlockUpdate(&md5ctx, random_data, 16);
	sceKernelUtilsMd5BlockUpdate(&md5ctx, random_data + 16, random_data_len - 16);

	struct MD5Digest digest;
	sceKernelUtilsMd5BlockResult(&md5ctx, &digest);

	// Probably not the right byte order.
	printf("MD5 in parts: %08x%08x%08x%08x\n", digest.data[0], digest.data[1], digest.data[2], digest.data[3]);
	memset(&digest, 0, sizeof(digest));
	sceKernelUtilsMd5Digest(random_data, random_data_len, &digest);
	printf("MD5 digest: %08x%08x%08x%08x\n", digest.data[0], digest.data[1], digest.data[2], digest.data[3]);
}


void checkSha1() {
	const int random_data_len = strlen(random_data);
	sceKernelUtilsSha1BlockInit(&sha1ctx);
	sceKernelUtilsSha1BlockUpdate(&sha1ctx, random_data, 16);
	sceKernelUtilsSha1BlockUpdate(&sha1ctx, random_data + 16, random_data_len - 16);

	struct Sha1Digest digest;
	sceKernelUtilsSha1BlockResult(&sha1ctx, &digest);

	// Probably not the right byte order.
	printf("Sha1 in parts: %08x%08x%08x%08x%08x\n", digest.data[0], digest.data[1], digest.data[2], digest.data[3], digest.data[4]);
	memset(&digest, 0, sizeof(digest));
	sceKernelUtilsSha1Digest(random_data, random_data_len, &digest);
	printf("Sha1 digest: %08x%08x%08x%08x%08x\n", digest.data[0], digest.data[1], digest.data[2], digest.data[3], digest.data[4]);
}

int main(int argc, char **argv) {
	checkMD5();
	printf("-----\n");
	checkSha1();
	return 0;
}
