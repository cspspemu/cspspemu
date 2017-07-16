
#include <pspkernel.h>
#include <stdio.h>
#include <string.h>
#include <math.h>

#define NOINLINE __attribute__((noinline))
#define ALIGN16  __attribute__((aligned (16))) 


void printVectorLowP(const char *title, ScePspFVector4 *v);
void printVector(const char *title, ScePspFVector4 *v);
void printMatrix(const char *title, ScePspFMatrix4 *m);

void nonsense(ScePspFMatrix4 *m);

void resetAllMatrices();