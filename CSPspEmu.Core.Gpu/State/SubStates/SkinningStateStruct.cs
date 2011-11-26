using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct SkinningStateStruct
	{
		public uint CurrentBoneMatrixIndex;
		public GpuMatrix4x4Struct BoneMatrix0;
		public GpuMatrix4x4Struct BoneMatrix1;
		public GpuMatrix4x4Struct BoneMatrix2;
		public GpuMatrix4x4Struct BoneMatrix3;
		public GpuMatrix4x4Struct BoneMatrix4;
		public GpuMatrix4x4Struct BoneMatrix5;
		public GpuMatrix4x4Struct BoneMatrix6;
		public GpuMatrix4x4Struct BoneMatrix7;
	}
}
