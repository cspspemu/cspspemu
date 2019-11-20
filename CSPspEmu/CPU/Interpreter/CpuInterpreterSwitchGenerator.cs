using System;
using System.Collections.Generic;
using System.Reflection;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu.Switch;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast;

namespace CSPspEmu.CPU.Interpreter
{
    public static class CpuInterpreterSwitchGenerator
    {
        private static readonly AstGenerator Ast = AstGenerator.Instance;
        public static readonly Action<uint, CpuInterpreter> Switch = GenerateSwitch();

        static Action<uint, CpuInterpreter> GenerateSwitch()
        {
            var CIType = typeof(CpuInterpreter);

            var nameToInfo = new Dictionary<string, MethodInfo>();

            foreach (var methodInfo in CIType.GetMethods())
            {
                var instructionNames = methodInfo.GetSingleAttribute<InstructionName>();
                if (instructionNames != null)
                {
                    nameToInfo[instructionNames.Name] = methodInfo;
                }
            }
            
            return EmitLookupGenerator.GenerateSwitch<Action<uint, CpuInterpreter>>(nameof(CpuInterpreterSwitchGenerator),
                InstructionTable.All,
                instructionInfo =>
                {
                    var methodInfo = nameToInfo[InstructionNames.Unknown];
                    if (instructionInfo != null && nameToInfo.ContainsKey(instructionInfo.Name))
                    {
                        methodInfo = nameToInfo[instructionInfo.Name];
                    }
                    

                    Console.WriteLine($"{methodInfo} {instructionInfo}");

                    if (methodInfo == null)
                    {
                    }
                    return Ast.Statements(
                        Ast.Statement(Ast.CallInstance(Ast.Argument<CpuInterpreter>(1), methodInfo)),
                        Ast.Return()
                    );
                }
            );
        }
    }
}