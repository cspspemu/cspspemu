using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	public struct VertexTypeStruct
	{
		static public int[] TypeSize = new[] { 0, sizeof(byte), sizeof(short), sizeof(float) };
		static public int[] ColorSize = new[] { 0, 1, 1, 1, 2, 2, 2, 4 };

		public uint Value;

		uint VertexSize
		{
			get
			{
				uint Size = 0;
				/*
				Size += skinningWeightCount * TypeSize[weight];
				Size += 1 * ColorSize[color];
				Size += 2 * TypeSize[texture ];
				Size += 3 * TypeSize[position];
				Size += 3 * TypeSize[normal  ];
				*/
				return Size;
			}
		}
	}

	public enum TransformModeEnum
	{
		Normal = 0,
		Raw = 1,
	}

	public struct VertexStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x4Struct ProjectionMatrix;
		
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x3Struct WorldMatrix;
		
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x3Struct ViewMatrix;

		/// <summary>
		/// 
		/// </summary>
		public TransformModeEnum TransformMode;

		/// <summary>
		/// here because of transform2d
		/// </summary>
		public VertexTypeStruct Type;
	}
}
