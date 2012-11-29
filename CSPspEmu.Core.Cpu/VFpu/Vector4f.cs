namespace CSPspEmu.Core.Cpu.VFPu
{
	struct Vector4f
	{
		public float x, y, z, w;

		public static Vector4f Create(float x, float y, float z, float w)
		{
			return new Vector4f() { x = x, y = y, z = z, w = w };
		}

		public void CopyTo(out float x, out float y, out float z, out float w)
		{
			x = this.x;
			y = this.y;
			z = this.z;
			w = this.w;
		}

		public static Vector4f Add(Vector4f l, Vector4f r)
		{
			return Create(l.x + r.x, l.y + r.y, l.z + r.z, l.w + r.w);
		}
	}
}
