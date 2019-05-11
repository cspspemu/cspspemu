namespace CSharpUtilsTests
{
    class Config
    {
        public static string RemoteIp => "192.168.1.45";
        public static string ProjectPath => System.AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..";
        public static string ProjectTestInputPath => ProjectPath + @"\TestInput";
        public static string ProjectTestInputMountedPath => ProjectPath + @"\TestInputMounted";
        public static string ProjectTestOutputPath => ProjectPath + @"\TestOutput";
    }
}