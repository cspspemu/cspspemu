using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct SkinningStateStruct
	{
		public uint CurrentBoneMatrixIndex;
		public GpuMatrix4x3Struct BoneMatrix0, BoneMatrix1, BoneMatrix2, BoneMatrix3, BoneMatrix4, BoneMatrix5, BoneMatrix6, BoneMatrix7;
	}
}
