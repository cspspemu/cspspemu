using System;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu.Switch;
using CSPspEmu.Core.Cpu.Table;
using Xunit;

namespace Tests.CSPspEmu.Core.Cpu.Switch
{
    
    public class EmitLookupGeneratorTest
    {
        public class HandlerClass
        {
            public List<int> Values = new List<int>();

            public void Test1()
            {
                Values.Add(1);
            }

            public void Test2()
            {
                Values.Add(2);
            }

            public void Test3()
            {
                Values.Add(3);
            }

            public void Unknown()
            {
                Values.Add(0);
            }
        }

        [Fact(Skip = "check")]
        public void GenerateSwitchDelegateTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new EmitLookupGenerator();
            
            var callback = EmitLookupGenerator.GenerateSwitchDelegate<HandlerClass>("GenerateSwitchDelegateTest",
                new[]
                {
                    new InstructionInfo()
                    {
                        BinaryEncoding = "----------------------------0001",
                        Name = "test1"
                    },
                    new InstructionInfo()
                    {
                        BinaryEncoding = "----------------------------0010",
                        Name = "test2"
                    },
                    new InstructionInfo()
                    {
                        BinaryEncoding = "----------------------------01--",
                        Name = "test3"
                    },
                });

            var handlerClass = new HandlerClass();
            callback(Convert.ToUInt32("0000", 2), handlerClass);
            callback(Convert.ToUInt32("0001", 2), handlerClass);
            callback(Convert.ToUInt32("0010", 2), handlerClass);
            callback(Convert.ToUInt32("0011", 2), handlerClass);
            callback(Convert.ToUInt32("0100", 2), handlerClass);
            callback(Convert.ToUInt32("0110", 2), handlerClass);
            callback(Convert.ToUInt32("1110", 2), handlerClass);
            Assert.Equal("0,1,2,0,3,3,0", string.Join(",", handlerClass.Values));
        }
    }
}