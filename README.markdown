# CSharp Psp Emulator

![Kaiten Patissier PSP](http://soywiz.github.com/cspspemu/screenshoots/kaiten_patissier.png)

A PSP Emulator for >= .NET 4.5 made in C# by soywiz - Carlos Ballesteros Velasco

Work In Progress emulator

## EMULATOR STATUS:

* Cpu, Fpu and VFpu are almost fully implemented
* Audio is implemented
* Gpu still requires lots of works
* Hle emulation has implemented lots of APIs (still lot of work left)
* There are lot of homebrew games running
* There are a growing number of commercial games running at full speed


## KEY FEATURES:

### Fully Managed and Disengaged

	Being fully managed (thought it will need unsafe access) will allow to port to lots of platforms in the future: AKA: Linux, Mac, PS3, Android, future devices...

### Very Fast Dynamic Recompilation

	The quality of the generated code is fantastic.
	Lots of MIPs instructions encodes in a single X86/64 instruction.
	The registers are inlined in the CpuThreadState class, so access
	to a register is as fast as accessing a non-virtual field.

### Fast As Hell Memory Access (Currently on Windows/Linux only)

	Using VirtualAlloc/mmap, allows all the PSP Memory Segments to be in a virtual space that fits the PSP Memory.
	And since there is no DMA access on a HLE emulation, we can get a pointer just making an ADD operation between the base and the PSP address.
	This is as fast as possible.

	MainPtr = VirtualAlloc(Base + MainOffset, MainSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	
### Very Fast Function Cache

	Detect and access to a function cache features O(1),
	adding a function cache O(log(n)),
	and flushing a range or the whole Function Cache is as fast as possible too.
	This is achieved with a Array that contains all the possible memory addresses and a SortedSet
	that allows to know which functions have been generated and should be purged when invalidating instruction cache.

### Very Fast Thread Switching

	Each PSP Thread is a native Green Thread / Fiber. Since the PSP has a single core, and some games requres
	thread scheduling, we simulate thread scheduling with fibers. Each PspThread have its own registers in an object,
	so instead of restoring the registers, it just resumes the thread execution. Skiping completely the register restoring.
	This model also allows to pause the execution of the code at any time very easy, even inside an HleFunction or even
	in the future, if we optimize the function generation to detect JAL instructions and call delegates directly, inside
	nested function calls.

### Very clean HLE Emulation using CustomAttributes, LINQ, method mapping and XML Documentation

	LINQ + functional-programming helps to make the code very clean, simple and easy to understand.

	/// <summary>
	/// Get the size of the largest free memory block.
	/// </summary>
	/// <returns>The size of the largest free memory block, in bytes.</returns>
	[HlePspFunction(NID = 0xA291F107, FirmwareVersion = 150)]
	public int sceKernelMaxFreeMemSize()
	{
		return HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).ChildPartitions
			.Where(Partition => !Partition.Allocated)
			.OrderByDescending(Partition => Partition.Size)
			.First()
			.Size
		;
	}
	
	C# support of structs and pointers, allow the HLE functions to use pointers directly and seamlessly.
	
	/// <summary>
	/// Reads an entry from an opened file descriptor.
	/// </summary>
	/// <param name="FileHandle">Already opened file descriptor (using sceIoDopen)</param>
	/// <param name="IoDirent">Pointer to an io_dirent_t structure to hold the file information</param>
	/// <returns>
	///		Read status
	///		Equal to   0 - No more directory entries left
	///		Great than 0 - More directory entired to go
	///		Less  than 0 - Error
	/// </returns>
	[HlePspFunction(NID = 0xE3EB004C, FirmwareVersion = 150)]
	public int sceIoDread(int FileHandle, HleIoDirent* IoDirent)
	{
		var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
		return HleIoDrvFileArg.HleIoDriver.IoDread(HleIoDrvFileArg, IoDirent);
	}

### Faster than the original PSP, than DPspEmu and than JPCSP

	Most CPU/FPU/VFPU operations are something like 8x times faster than the original PSP on a modern computer.
	Still the GPU implementation is still limited because of the Cpu <-> Gpu memory BUS. But probably will be enough in most cases.
	
### KISS and DRY

	With the "Keep It Simple Stupid" and "Don't Repeat Yourself" philosophies always in mind,
	the quality of the core is a priority always over new features.
	
	CPU/Gpu Instruction decoding, assembler and disassembler share a single table with the instruction descriptions that will allow generating the required code on the fly.
	
		// Arithmetic operations.
		ID("add",    VM("000000:rs:rt:rd:00000:100000"), "%d, %s, %t", ADDR_TYPE_NONE, 0),
		ID("addu",   VM("000000:rs:rt:rd:00000:100001"), "%d, %s, %t", ADDR_TYPE_NONE, 0),
		ID("addi",   VM("001000:rs:rt:imm16"          ), "%t, %s, %i", ADDR_TYPE_NONE, 0),

### UnitTesting and Integration Tests

	The idea is to create a full set of tests, testing the emulator interface, code generation, and HLE APIs.
	UnitTests are integrated in the code, and HLE APIs are external integration tests in form of PSP executables than can be executed on any emulator to test APIs.
	There is a project called pspautotests used by all the active emulators to test regressions and new APIs.
	
### Fully Open Source

	Project is on GitHub, so anyone can branch the project and perform Pull Requests helping to improve the code.

### Created after 4 tries

	I'm the creator of the D PSP Emulator. That emulator had 4 versions, every one was created almost from the scratch with different approaches and trying
	to improve the quality of the code in each iteration.
	The lack of a good IDE, the complicated structure of the D language, the horrible compilation times, caused that it taked too much time for everything,
	and made it impossible to refactoring the code without days or weeks of work.
	In this time I have learned lot of things, and since the .NET platform is fast enough, have "mono", and solves all my problems with D, it will be my final choice.

### Final Words

	The first emulator made by Noxa was a C# emulator too. It was for .NET 2.0. .NET and C# have evolved a lot since that 2.0 version. And now it is probably the best
	platform/language out there. It has the right balance between ease, power and speed. Of course it lacks some stuff, but it is the language where less programming features I have missed.
	I didn't used the pspplayer code as base because I wanted to make things with all the stuff I have learned this time and with a different approach.
	I started the first version of my emulator using Noxa's pspplayer as base. I didn't managed to compile his code that time because of a mixed DLL x86/x64, and I wanted to learn to make an emulator.
	I want to acknowledge all the great work of Noxa, because without all his first hard work, I hadn't learnt this all stuff.
