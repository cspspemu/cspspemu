using System.Reflection;

namespace CSPspEmu.Core
{
	public class PspGlobalConfiguration
	{
		static private string _CurrentVersion;
		static private int? _CurrentVersionNumeric;

		static public string CurrentVersion
		{
			get
			{
				if (_CurrentVersion == null)
				{
					_CurrentVersion = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.version_current.txt").ReadAllContentsAsString();
				}
				return _CurrentVersion;
			}
		}

		static public int CurrentVersionNumeric
		{
			get
			{
				try
				{
					if (!_CurrentVersionNumeric.HasValue)
					{
						_CurrentVersionNumeric = int.Parse(Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.version_current_numeric.txt").ReadAllContentsAsString());
					}
					return _CurrentVersionNumeric.Value;
				}
				catch
				{
					return 0;
				}
			}
		}

		static private string _GitRevision;

		static public string GitRevision
		{
			get
			{
				if (_GitRevision == null)
				{
					_GitRevision = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.git_revision.txt").ReadAllContentsAsString();
				}
				return _GitRevision;
			}
		}
	}
}
