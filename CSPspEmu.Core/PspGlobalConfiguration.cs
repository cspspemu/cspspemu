using System.Reflection;

namespace CSPspEmu.Core
{
	public static class PspGlobalConfiguration
	{
		private static string _CurrentVersion;
		private static int? _CurrentVersionNumeric;

		public static string CurrentVersion
		{
			get
			{
				if (_CurrentVersion == null)
				{
					_CurrentVersion = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.version_current.txt").ReadAllContentsAsString();
				}
				return _CurrentVersion;
			}
		}

		public static int CurrentVersionNumeric
		{
			get
			{
				try
				{
					if (!_CurrentVersionNumeric.HasValue)
					{
						_CurrentVersionNumeric = int.Parse(Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.version_current_numeric.txt").ReadAllContentsAsString());
					}
					return _CurrentVersionNumeric.Value;
				}
				catch
				{
					return 0;
				}
			}
		}

		private static string _GitRevision;

		public static string GitRevision
		{
			get
			{
				if (_GitRevision == null)
				{
					_GitRevision = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.git_revision.txt").ReadAllContentsAsString();
				}
				return _GitRevision;
			}
		}
	}
}
