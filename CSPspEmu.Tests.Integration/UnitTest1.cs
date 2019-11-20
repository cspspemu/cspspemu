using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Emulator.Simple;
using CSPspEmu.Hle;
using CSPspEmu.Utils;
using Xunit;
using Xunit.Abstractions;

namespace CSPspEmu.Tests.Integration
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        public UnitTest1(ITestOutputHelper output) => _output = output;

        [Fact(Timeout = 30_000)]
        public void TestCpuAlu() => TestPrx("cpu/cpu_alu/cpu_alu");

        //[Fact(Timeout = 30_000)]
        //public void TestFpu() => TestPrx("cpu/fpu/fpu");

        #region Internal

        private class HleOutputHandlerMock : HleOutputHandler
        {
            private StringBuilder sb = new StringBuilder(64 * 1024);
            public String OutputString => sb.ToString();

            public override void Output(string outputString) => sb.Append(outputString);
        }

        private void TestPrx(string Base)
        {
            _output.CaptureToTest(() =>
            {
                using var emulator = new SimplifiedPspEmulator(
                    test: true,
                    configure: injector => { injector.SetInstanceType<HleOutputHandler, HleOutputHandlerMock>(); }
                );
                var houtput = (HleOutputHandlerMock) emulator.injector.GetInstance<HleOutputHandler>();
                emulator.LoadAndStart($"../../../../pspautotests/tests/{Base}.prx");
                if (!emulator.Emulator.PspRunner.CpuComponentThread.StoppedEndedEvent.WaitOne(30.Seconds())) ;
                var actual = houtput.OutputString;
                var expected = File.ReadAllText($"../../../../pspautotests/tests/{Base}.expected");
                //File.WriteAllText("/tmp/actual.txt", actual);
                //File.WriteAllText("/tmp/expected.txt", expected);
                Assert.Equal(expected, actual);
            });
        }

        #endregion
    }

    static class ITestOutputHelperExt
    {
        static public void CaptureToTest(this ITestOutputHelper output, Action handler)
        {
            TestOutputWrite.CaptureToTest(output, handler);
        }
    }

    class TestOutputWrite : TextWriter
    {
        private ITestOutputHelper output;

        public TestOutputWrite(ITestOutputHelper output)
        {
            this.output = output;
        }

        StringBuilder sb = new StringBuilder();

        public override void Write(char value)
        {
            if (value == '\n')
            {
                output.WriteLine(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(value);
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        static public void CaptureToTest(ITestOutputHelper output, Action handler)
        {
            var oldOut = Console.Out;
            var oldErr = Console.Error;
            var writer = new TestOutputWrite(output);
            Console.SetOut(writer);
            Console.SetError(writer);
            try
            {
                handler();
            }
            finally
            {
                Console.SetError(oldErr);
                Console.SetOut(oldOut);
            }
        }
    }
}