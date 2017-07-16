using System.Reflection;

namespace CSPspEmu.Core
{
	public static class PspGlobalConfiguration
	{
		private static string _currentVersion;
		private static int? _currentVersionNumeric;

		public static string CurrentVersion
		{
			get
			{
				if (_currentVersion == null)
				{
					_currentVersion = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.version_current.txt").ReadAllContentsAsString();
				}
				return _currentVersion;
			}
		}

		public static int CurrentVersionNumeric
		{
			get
			{
				try
				{
					if (!_currentVersionNumeric.HasValue)
					{
						_currentVersionNumeric = int.Parse(Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.version_current_numeric.txt").ReadAllContentsAsString());
					}
					return _currentVersionNumeric.Value;
				}
				catch
				{
					return 0;
				}
			}
		}

		private static string _gitRevision;

		public static string GitRevision
		{
			get
			{
				if (_gitRevision == null)
				{
					_gitRevision = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.git_revision.txt").ReadAllContentsAsString();
				}
				return _gitRevision;
			}
		}
	}
}
