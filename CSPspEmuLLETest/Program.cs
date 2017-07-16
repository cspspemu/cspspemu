using System;
using System.IO;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
    unsafe class Program
    {
        static public string NandPath
        {
            get
            {
                return @"..\..\..\deploy\cspspemu\nand-dump.bin";
                //return @"..\..\..\deploy\cspspemu\nand-dump-420.bin";
            }
        }

        static public string PreIplPath
        {
            get { return @"..\..\..\deploy\cspspemu\psp_bios.bin"; }
        }

        [Inject] protected DebugPspMemory PspMemory;

        [Inject] protected CpuProcessor CpuProcessor;

        [Inject] protected InjectContext InjectContext;

        public Program()
        {
            var injectContext = new InjectContext();
            {
                injectContext.SetInstanceType<PspMemory, DebugPspMemory>();
                injectContext.SetInstanceType<DebugPspMemory, DebugPspMemory>();
            }
            injectContext.InjectDependencesTo(this);
        }

        public void Run()
        {
            var cpuThreadState = new CpuThreadState(CpuProcessor);
            var dma = new Dma(cpuThreadState);

            Console.SetWindowSize(120, 60);
            Console.SetBufferSize(120, 8000);

            var nandStream = File.OpenRead(NandPath);
            var iplReader = new IplReader(new NandReader(nandStream));
            var info = iplReader.LoadIplToMemory(new PspMemoryStream(PspMemory));
            var startPc = info.EntryFunction;

            var lleState = new LleState();

            dma.LleState = lleState;
            lleState.Gpio = new LleGpio();
            lleState.Nand = new LleNand(nandStream);
            lleState.Cpu = new LlePspCpu("CPU", InjectContext, CpuProcessor, startPc);
            lleState.Me = new LlePspCpu("ME", InjectContext, CpuProcessor, startPc);
            lleState.LleKirk = new LleKirk(PspMemory);
            lleState.Memory = PspMemory;

            lleState.Cpu.Start();

            while (true) Thread.Sleep(int.MaxValue);
        }

        static void Main(string[] args)
        {
            new Program().Run();


            //DebugPspMemory.Write4(0xBFC00FFC, 0x20040420);

            // It doesn't start the ME
            //DebugPspMemory.Write4(0xBFC00FFC, 0xFFFFFFFF);

            /*
            ME:
            li      $t0, 0x40EC19C
            jr      $t0
            nop
            */

            //IplReader.WriteIplToFile(File.Open(NandPath + ".ipl.bin", FileMode.Create, FileAccess.Write));
        }
    }
}