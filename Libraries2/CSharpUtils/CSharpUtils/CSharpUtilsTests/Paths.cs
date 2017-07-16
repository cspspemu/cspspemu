using System;

namespace CSharpUtilsTests
{
	class Config
	{
		public static String RemoteIp
		{
			get
			{
				return "192.168.1.45";
			}
		}

		public static String ProjectPath
		{
			get
			{
				return System.AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..";
			}
		}

		public static String ProjectTestInputPath
		{
			get
			{
				return ProjectPath + @"\TestInput";
			}
		}

		public static String ProjectTestInputMountedPath
		{
			get
			{
				return ProjectPath + @"\TestInputMounted";
			}
		}

		public static String ProjectTestOutputPath
		{
			get
			{
				return ProjectPath + @"\TestOutput";
			}
		}
	}
}
