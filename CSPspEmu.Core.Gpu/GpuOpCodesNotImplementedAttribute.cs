using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class GpuOpCodesNotImplementedAttribute : Attribute
	{
	}
}
