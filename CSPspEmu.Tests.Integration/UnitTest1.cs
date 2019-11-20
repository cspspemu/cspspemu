using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CSharpUtils;
using CSPspEmu.Emulator.Simple;
using CSPspEmu.Utils;
using Xunit;
using Xunit.Abstractions;

namespace CSPspEmu.Tests.Integration
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact(Timeout = 10_000)]
        public void Test1()
        {
            //output.WriteLine("hello");
            //output.WriteLine(Directory.GetCurrentDirectory());
            //output.WriteLine("hello");
            TestOutputWrite.CaptureToTest(output, () =>
            {
                Console.WriteLine("test");
                //using var emulator = new SimplifiedPspEmulator();
                //emulator.LoadAndStart("../../../../pspautotests/tests/cpu/cpu_alu/cpu_alu.elf");
                //if (!emulator.Emulator.PspRunner.CpuComponentThread.StoppedEndedEvent.WaitOne(10.Seconds())) ;
            });
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
            sb.Append(value);
            if (value == '\n')
            {
                output.WriteLine(sb.ToString());
                sb.Clear();
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
