using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSPspEmu.Hle.Types
{
	public class PspVersion
	{
		private Version Version;

		public int Major { get { return Version.Major; } }
		public int Minor { get { return Version.Minor; } }
		public int Revision { get { return Version.Revision; } }

		public PspVersion(string VersionString)
		{
			this.SetVersion(VersionString);
		}

		static public implicit operator PspVersion(string String)
		{
			return new PspVersion(String);
		}

		private void SetVersion(string VersionString)
		{
			var Parts = VersionString.Split('.');
			int Major = 0, Minor = 0, Revision = 0;
			if (Parts.Length >= 1) Major = int.Parse(Parts[0]);
			if (Parts.Length >= 2) Minor = int.Parse(Parts[1]);
			if (Parts.Length >= 3) Revision = int.Parse(Parts[2]);
			Version = new Version(Major, Minor, Revision);
		}
	}
}
