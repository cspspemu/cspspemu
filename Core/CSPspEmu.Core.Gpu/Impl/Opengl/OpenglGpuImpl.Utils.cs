using CSharpPlatform.GL;
namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed partial class OpenglGpuImpl
	{
		private static bool GlEnableDisable(int EnableCap, bool EnableDisable)
		{
			if (EnableDisable)
			{
				GL.glEnable(EnableCap);
			}
			else
			{
				GL.glDisable(EnableCap);
			}
			return EnableDisable;
		}
	}
}
