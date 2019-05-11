//#define DEBUG_TRACE_INSTRUCTIONS
//#define DISABLE_JUMP_GOTO

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using System.Diagnostics;
using CSPspEmu.Core.Memory;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Switch;

namespace CSPspEmu.Core.Cpu.Dynarec
{
    public partial class DynarecFunctionCompiler
    {
        internal class InternalFunctionCompiler
        {
            public static readonly Func<uint, CpuEmitter, AstNodeStm> CpuEmitterInstruction =
                EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuEmitter, AstNodeStm>("CpuEmitterInstruction",
                    InstructionTable.All);

            static MipsDisassembler _mipsDisassembler = new MipsDisassembler();
            CpuEmitter _cpuEmitter;
            DynarecFunctionCompiler _dynarecFunctionCompiler;
            IInstructionReader _instructionReader;

            [Inject] CpuProcessor _cpuProcessor;

            [Inject] PspMemory _memory;

            MipsMethodEmitter _mipsMethodEmitter;

            uint _entryPc;
            SortedDictionary<uint, AstLabel> _labels = new SortedDictionary<uint, AstLabel>();
            SortedDictionary<uint, AstLabel> _labelsJump = new SortedDictionary<uint, AstLabel>();

            //const int MaxNumberOfInstructions = 3000;
            const int MaxNumberOfInstructions = 32 * 1024;
            //const int MaxNumberOfInstructions = 64 * 1024;
            //const int MaxNumberOfInstructions = 128 * 1024;
            //const int MaxNumberOfInstructions = 60;

            Dictionary<string, uint> _globalInstructionStats;
            Dictionary<string, uint> _instructionStats;
            Dictionary<string, bool> _newInstruction;

            bool _doLog;
            Action<uint> _exploreNewPcCallback;

            uint _minPc;
            uint _maxPc;

            public List<uint> CallingPCs = new List<uint>();

            uint _instructionsProcessed;

            HashSet<uint> _analyzedPc;

            /// <summary>
            /// Instructions to SKIP code generation, because they have been grouped in other instruction.
            /// </summary>
            HashSet<uint> _skipPc;

            public void ExploreNewPcCallback(uint pc)
            {
                _exploreNewPcCallback?.Invoke(pc);
            }

            internal InternalFunctionCompiler(InjectContext injectContext, MipsMethodEmitter mipsMethodEmitter,
                DynarecFunctionCompiler dynarecFunctionCompiler, IInstructionReader instructionReader,
                Action<uint> exploreNewPcCallback, uint entryPc, bool doLog, bool checkValidAddress = true)
            {
                injectContext.InjectDependencesTo(this);
                _exploreNewPcCallback = exploreNewPcCallback;
                _mipsMethodEmitter = mipsMethodEmitter;
                _cpuEmitter = new CpuEmitter(injectContext, mipsMethodEmitter, instructionReader);
                _globalInstructionStats = _cpuProcessor.GlobalInstructionStats;
                //this.InstructionStats = MipsMethodEmitter.InstructionStats;
                _instructionStats = new Dictionary<string, uint>();
                _newInstruction = new Dictionary<string, bool>();
                _doLog = doLog;

                _dynarecFunctionCompiler = dynarecFunctionCompiler;
                _instructionReader = instructionReader;
                _entryPc = entryPc;

                if (checkValidAddress && !PspMemory.IsAddressValid(entryPc))
                {
                    throw (new InvalidOperationException($"Trying to get invalid function 0x{entryPc:X8}"));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal DynarecFunction CreateFunction()
            {
                _cpuEmitter.SpecialName = "";

                var time0 = DateTime.UtcNow;

                AnalyzeBranches();

                var time1 = DateTime.UtcNow;

                var nodes = GenerateCode();

                nodes = _ast.Statements(
                    _ast.Comment($"Function {_minPc:X8}-{_maxPc:X8}. Entry: {_entryPc:X8}"),
                    //ast.DebugWrite(String.Format("Dynarec:PC:{0:X8}", EntryPC)),
                    //ast.Comment("Returns immediately when argument CpuThreadState is null, so we can call it on the generation thread to do prelinking."),
                    _ast.If(
                        _ast.Binary(_ast.CpuThreadStateExpr, "==", _ast.Null<CpuThreadState>()),
                        _ast.Return()
                    ),
                    nodes
                );

                var time2 = DateTime.UtcNow;

                //var MipsMethodEmitterResult = MipsMethodEmitter.CreateDelegate(Nodes, (int)((MaxPC - MinPC) / 4));
                var mipsMethodEmitterResult = _mipsMethodEmitter.CreateDelegate(nodes, _analyzedPc.Count);

                return new DynarecFunction()
                {
                    Name = _cpuEmitter.SpecialName,
                    CallingPCs = CallingPCs,
                    EntryPc = _entryPc,
                    MinPc = _minPc,
                    MaxPc = _maxPc,
                    AstNode = nodes,
                    DisableOptimizations = mipsMethodEmitterResult.DisableOptimizations,
                    Delegate = mipsMethodEmitterResult.Delegate,
                    InstructionStats = _instructionStats,
                    TimeOptimize = mipsMethodEmitterResult.TimeOptimize,
                    TimeGenerateIl = mipsMethodEmitterResult.TimeGenerateIl,
                    TimeCreateDelegate = mipsMethodEmitterResult.TimeCreateDelegate,
                    TimeAnalyzeBranches = time1 - time0,
                    TimeGenerateAst = time2 - time1,
                };
            }


            private void LogInstruction(uint pc, Instruction instruction)
            {
                if (_cpuProcessor.CpuConfig.LogInstructionStats)
                {
                    var disassembledInstruction = _mipsDisassembler.Disassemble(pc, instruction);
                    //Console.WriteLine("{0}", DisassembledInstruction);
                    //var InstructionName = GetInstructionName(CpuEmitter.Instruction.Value, null);
                    var instructionName = disassembledInstruction.InstructionInfo.Name;

                    if (!_instructionStats.ContainsKey(instructionName)) _instructionStats[instructionName] = 0;
                    _instructionStats[instructionName]++;

                    if (!_globalInstructionStats.ContainsKey(instructionName))
                    {
                        _newInstruction[instructionName] = true;
                        _globalInstructionStats[instructionName] = 0;
                    }

                    _globalInstructionStats[instructionName]++;
                    //var GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
                    //var InstructionStats = new Dictionary<string, uint>();
                    //var NewInstruction = new Dictionary<string, bool>();
                }
            }

            private bool AddressInsideFunction(uint pc)
            {
                return pc >= _minPc && pc <= _maxPc;
            }

            private void UpdateMinMax(uint pc)
            {
                _minPc = Math.Min(_minPc, pc);
                _maxPc = Math.Max(_maxPc, pc);
            }

            /// <summary>
            /// PASS 1: Analyze Branches
            /// </summary>
            private void AnalyzeBranches()
            {
                _skipPc = new HashSet<uint>();
                _analyzedPc = new HashSet<uint>();
                var branchesToAnalyze = new Queue<uint>();

                _labels[_entryPc] = AstLabel.CreateLabel("EntryPoint");

                var endPc = _instructionReader.EndPc;
                _pc = _entryPc;
                _minPc = uint.MaxValue;
                _maxPc = uint.MinValue;

                branchesToAnalyze.Enqueue(_entryPc);

                while (true)
                {
                    HandleNewBranch:
                    var endOfBranchFound = false;

                    if (branchesToAnalyze.Count == 0) break;

                    for (_pc = branchesToAnalyze.Dequeue(); _pc <= endPc; _pc += 4)
                    {
                        // If already analyzed, stop scanning this branch.
                        if (_analyzedPc.Contains(_pc)) break;
                        _analyzedPc.Add(_pc);
                        //Console.WriteLine("%08X".Sprintf(PC));

                        if (_analyzedPc.Count > MaxNumberOfInstructions)
                        {
                            throw (new InvalidDataException(
                                $"Code sequence too long: >= {MaxNumberOfInstructions} at 0x{_entryPc:X8}"));
                        }

                        UpdateMinMax(_pc);

                        //Console.WriteLine("    PC:{0:X}", PC);

                        var instruction = _instructionReader[_pc];

                        var branchInfo = DynarecBranchAnalyzer.GetBranchInfo(instruction);
                        var disassemblerInfo = _mipsDisassembler.Disassemble(_pc, instruction);

                        LogInstruction(_pc, instruction);

                        // Break
                        if (disassemblerInfo.InstructionInfo.Name == "break")
                        {
                            break;
                        }
                        // Branch instruction.
                        //else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpAlways))
                        else if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
                        {
                            //Console.WriteLine("Instruction");

                            var jumpAddress = instruction.GetJumpAddress(_memory, _pc);

                            // Located a jump-always instruction with a delayed slot. Process next instruction too.
                            if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.AndLink))
                            {
                                // Just a function call. Continue analyzing.
                            }
                            else
                            {
                                if (PspMemory.IsAddressValid(jumpAddress))
                                {
                                    if (!_labelsJump.ContainsKey(jumpAddress))
                                    {
                                        if (AddressInsideFunction(jumpAddress))
                                        {
                                            //Console.WriteLine("JumpAddress: {0:X8}", JumpAddress);
                                            _labelsJump[jumpAddress] =
                                                AstLabel.CreateLabel($"Jump_0x{jumpAddress:X8}");
                                            branchesToAnalyze.Enqueue(jumpAddress);
                                        }
                                    }
                                }

                                endOfBranchFound = true;
                                continue;
                            }
                        }
                        else if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction))
                        {
                            var branchAddress = instruction.GetBranchAddress(_pc);
                            if (!_labels.ContainsKey(branchAddress))
                            {
                                //Console.WriteLine("BranchAddress: {0:X8}", BranchAddress);
                                UpdateMinMax(branchAddress);
                                _labels[branchAddress] =
                                    AstLabel.CreateLabel($"Label_0x{branchAddress:X8}");
                                branchesToAnalyze.Enqueue(branchAddress);
                            }
                        }
                        else if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.SyscallInstruction))
                        {
                            // On this special Syscall
                            if (instruction.Code == SyscallInfo.NativeCallSyscallCode)
                            {
                                //PC += 4;
                                goto HandleNewBranch;
                            }
                        }

                        // Jump-Always found. And we have also processed the delayed branch slot. End the branch.
                        if (endOfBranchFound)
                        {
                            endOfBranchFound = false;
                            goto HandleNewBranch;
                        }
                    }
                }

                //Console.WriteLine("FunctionSegment({0:X8}-{1:X8})", MinPC, MaxPC);

                foreach (var labelAddress in _labelsJump.Keys.ToArray())
                {
                    if (!AddressInsideFunction(labelAddress))
                    {
                        _labelsJump.Remove(labelAddress);
                    }
                }

                _cpuEmitter.BranchCount = _labels.Count;
            }

            private AstNodeStm ProcessGeneratedInstruction(MipsDisassembler.Result disasm, AstNodeStm astNodeStm)
            {
                var pc = disasm.InstructionPc;
                return _ast.Statements(
#if DEBUG_TRACE_INSTRUCTIONS
					ast.DebugWrite(String.Format("0x{0:X8}: {1}", PC, Disasm)),
#endif
                    astNodeStm
                );
            }

            private AstNodeStmPspInstruction _GetAstCpuInstructionAT(uint pc)
            {
                // Skip emit instruction.
                if (_skipPc.Contains(pc)) return null;

                /*
                if (CpuProcessor.CpuConfig.TraceJIT)
                {
                    SafeILGenerator.LoadArgument<CpuThreadState>(0);
                    SafeILGenerator.Push((int)_PC);
                    SafeILGenerator.Call((Action<uint>)CpuThreadState.Methods.Trace);
                    Console.WriteLine("     PC=0x{0:X}", _PC);
                }
                */

                var instruction = _cpuEmitter.LoadAt(pc);
                var disassembleInstruction = _mipsDisassembler.Disassemble(pc, instruction);
                var call = CpuEmitterInstruction(instruction, _cpuEmitter);
                var astNodeStm = ProcessGeneratedInstruction(disassembleInstruction, call);
                return new AstNodeStmPspInstruction(disassembleInstruction, astNodeStm);
            }

            uint _pc;
            private static AstMipsGenerator _ast = AstMipsGenerator.Instance;

            private AstNodeStm StorePc()
            {
                //MipsMethodEmiter.SavePC(PC);
                //Console.Error.WriteLine("Not implemented!");
                return _ast.Statement();
            }

            private AstNodeStm EmitInstructionCountIncrement(bool checkForYield)
            {
                // CountInstructionsAndYield
                if (!_cpuProcessor.CpuConfig.CountInstructionsAndYield)
                {
                    return _ast.Statement();
                }

                throw (new NotImplementedException("EmitInstructionCountIncrement"));

                ////Console.WriteLine("EmiteInstructionCountIncrement: {0},{1}", InstructionsEmitedSinceLastWaypoint, CheckForYield);
                //if (InstructionsEmitedSinceLastWaypoint > 0)
                //{
                //	MipsMethodEmiter.SaveStepInstructionCount(() =>
                //	{
                //		MipsMethodEmiter.LoadStepInstructionCount();
                //		SafeILGenerator.Push((int)InstructionsEmitedSinceLastWaypoint);
                //		SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
                //	});
                //	//ILGenerator.Emit(OpCodes.Ldc_I4, 100);
                //	//ILGenerator.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine"), new Type[] { typeof(int) });
                //	InstructionsEmitedSinceLastWaypoint = 0;
                //}
                //
                //if (CheckForYield)
                //{
                //	if (!CpuProcessor.CpuConfig.BreakInstructionThreadSwitchingForSpeed)
                //	{
                //		var NoYieldLabel = SafeILGenerator.DefineLabel("NoYieldLabel");
                //		MipsMethodEmiter.LoadStepInstructionCount();
                //		SafeILGenerator.Push((int)0);
                //		SafeILGenerator.BranchBinaryComparison(SafeBinaryComparison.GreaterThanSigned, NoYieldLabel);
                //		//ILGenerator.Emit(OpCodes.Ldc_I4, 1000000);
                //		//ILGenerator.Emit(OpCodes.Blt, NoYieldLabel);
                //		MipsMethodEmiter.SaveStepInstructionCount(() =>
                //		{
                //			SafeILGenerator.Push((int)0);
                //		});
                //		StorePC();
                //		SafeILGenerator.LoadArgument0CpuThreadState();
                //		SafeILGenerator.Call((Action)CpuThreadState.Methods.Yield);
                //		//ILGenerator.Emit(OpCodes.Call, typeof(GreenThread).GetMethod("Yield"));
                //		NoYieldLabel.Mark();
                //	}
                //}
            }

            private void TryPutLabelAt(uint pc, AstNodeStmContainer nodes)
            {
                if (_labels.ContainsKey(pc))
                {
                    nodes.AddStatement(EmitInstructionCountIncrement(false));
                    nodes.AddStatement(_ast.Label(_labels[pc]));
                }
                if (_labelsJump.ContainsKey(pc)) nodes.AddStatement(_ast.Label(_labelsJump[pc]));
            }

            public static void IsDebuggerPresentDebugBreak()
            {
                if (Debugger.IsAttached) Debugger.Break();
            }

            private AstNodeStm EmitCpuInstruction()
            {
                try
                {
                    var nodes = _ast.Statements();

                    if (_cpuProcessor.NativeBreakpoints.Contains(_pc))
                    {
                        nodes.AddStatement(_ast.Statement(_ast.CallStatic((Action) IsDebuggerPresentDebugBreak)));
                    }

                    TryPutLabelAt(_pc, nodes);

                    if (DynarecConfig.UpdatePCEveryInstruction)
                    {
                        nodes.AddStatement(_ast.AssignPc(_pc));
                    }

                    nodes.AddStatement(_GetAstCpuInstructionAT(_pc));

                    return nodes;
                }
                finally
                {
                    _pc += 4;
                    _instructionsEmitedSinceLastWaypoint++;
                }
            }

            uint _instructionsEmitedSinceLastWaypoint;
            private TimeSpan _createDelegateTime;
            private TimeSpan _generateIlTime;

            //static int DummyTempCounter = 0;

            /// <summary>
            /// PASS 2: Generate code and put labels;
            /// </summary>
            private AstNodeStmContainer GenerateCode()
            {
                foreach (var label in _labels.ToArray())
                {
                    if (!(label.Key >= _minPc && label.Key <= _maxPc))
                    {
                        _labels.Remove(label.Key);
                    }
                }
                //AnalyzedPC

                _instructionsEmitedSinceLastWaypoint = 0;

                //Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

                // Jumps to the entry point.
                var nodes = new AstNodeStmContainer();

                if (!_labels.ContainsKey(_entryPc))
                {
                    throw new KeyNotFoundException($"Can't find key {_entryPc:X} in list [{_labels.Select(it => $"{it.Key:X}").JoinToString(",")}]");
                }
                nodes.AddStatement(_ast.GotoAlways(_labels[_entryPc]));

                for (_pc = _minPc; _pc <= _maxPc;)
                {
                    if (!_analyzedPc.Contains(_pc))
                    {
                        _pc += 4;
                        continue;
                    }
                    uint currentInstructionPc = _pc;
                    Instruction currentInstruction = _instructionReader[_pc];
                    _instructionsProcessed++;

                    var branchInfo = DynarecBranchAnalyzer.GetBranchInfo(currentInstruction.Value);

                    // Delayed branch instruction.
                    if ((branchInfo & DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction) != 0)
                    {
                        _instructionsEmitedSinceLastWaypoint += 2;
                        nodes.AddStatement(EmitInstructionCountIncrement(true));

                        var branchAddress = currentInstruction.GetBranchAddress(_pc);

                        if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
                        {
                            TryPutLabelAt(_pc, nodes);

                            var delayedBranchInstruction = _GetAstCpuInstructionAT(_pc + 4); // Delayed
                            var jumpInstruction = _GetAstCpuInstructionAT(_pc + 0); // Jump

#if !DISABLE_JUMP_GOTO
                            var jumpInstruction2 = _cpuEmitter.LoadAt(_pc + 0);
                            var jumpDisasm = _mipsDisassembler.Disassemble(_pc + 0, jumpInstruction2);
                            var jumpJumpPc = jumpDisasm.Instruction.GetJumpAddress(_memory, jumpDisasm.InstructionPc);

                            // An internal jump.
                            if (
                                (jumpDisasm.InstructionInfo.Name == "j")
                                && (_labelsJump.ContainsKey(jumpJumpPc))
                            )
                            {
                                jumpInstruction = new AstNodeStmPspInstruction(jumpDisasm,
                                    _ast.GotoAlways(_labelsJump[jumpJumpPc]));

                                //Console.WriteLine(
                                //	"{0}: {1} : Function({2:X8}-{3:X8})",
                                //	DummyTempCounter,
                                //	GeneratorCSharpPsp.GenerateString<GeneratorCSharpPsp>(AstOptimizerPsp.GlobalOptimize(CpuProcessor, JumpInstruction)),
                                //	MinPC, MaxPC
                                //);

                                //DummyTempCounter++;
                            }
                            else if (jumpDisasm.InstructionInfo.Name == "j" || jumpDisasm.InstructionInfo.Name == "jal")
                            {
                                CallingPCs.Add(jumpJumpPc);
                            }
#endif

                            // Put delayed instruction first.
                            nodes.AddStatement(delayedBranchInstruction);
                            // A jump outside the current function.
                            nodes.AddStatement(jumpInstruction);

                            _pc += 8;
                        }
                        else
                        {
                            // Branch instruction.
                            nodes.AddStatement(EmitCpuInstruction());

                            //if ((BranchInfo & CpuBranchAnalyzer.Flags.Likely) != 0)
                            if (branchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.Likely))
                            {
                                //Console.WriteLine("Likely");
                                // Delayed instruction.
                                nodes.AddStatement(_cpuEmitter._branch_likely(EmitCpuInstruction()));
                            }
                            else
                            {
                                //Console.WriteLine("Not Likely");
                                // Delayed instruction.
                                nodes.AddStatement(EmitCpuInstruction());
                            }

                            if (currentInstructionPc + 4 != branchAddress)
                            {
                                if (_labels.ContainsKey(branchAddress))
                                {
                                    nodes.AddStatement(_cpuEmitter._branch_post(_labels[branchAddress], branchAddress));
                                }
                                // Code not reached.
                                else
                                {
                                    throw (new InvalidOperationException("!Labels.ContainsKey(BranchAddress)"));
                                }
                            }
                            else
                            {
                                throw (new InvalidOperationException("Invalid branch!"));
                            }
                        }
                    }
                    // Normal instruction.
                    else
                    {
                        // Syscall instruction.
                        if ((branchInfo & DynarecBranchAnalyzer.JumpFlags.SyscallInstruction) != 0)
                        {
                            nodes.AddStatement(StorePc());
                        }
                        nodes.AddStatement(EmitCpuInstruction());
                        if ((branchInfo & DynarecBranchAnalyzer.JumpFlags.SyscallInstruction) != 0)
                        {
                            // On this special Syscall
                            if (currentInstruction.Code == SyscallInfo.NativeCallSyscallCode)
                            {
                                //PC += 4;
                                break;
                            }
                        }
                    }
                }

                //MipsMethodEmiter.GenerateIL(Nodes);
                ShowInstructionStats();

                //if (BreakPoint) IsDebuggerPresentDebugBreak();

                return nodes;
            }

            private void ShowInstructionStats()
            {
                if (_cpuProcessor.CpuConfig.ShowInstructionStats)
                {
                    bool hasNew = false;
                    foreach (var pair in _instructionStats.OrderByDescending(item => item.Value))
                    {
                        if (_newInstruction.ContainsKey(pair.Key))
                        {
                            hasNew = true;
                        }
                    }

                    if (!_cpuProcessor.CpuConfig.ShowInstructionStatsJustNew || hasNew)
                    {
                        Console.Error.WriteLine("-------------------------- {0:X}-{1:X} ", _minPc, _maxPc);
                        ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.White, () =>
                        {
                            foreach (var pair in _instructionStats.OrderByDescending(item => item.Value))
                            {
                                var isNew = _newInstruction.ContainsKey(pair.Key);
                                if (!_cpuProcessor.CpuConfig.ShowInstructionStatsJustNew || isNew)
                                {
                                    Console.Error.Write("{0} : {1}", pair.Key, pair.Value);
                                    if (isNew) Console.Error.Write(" <-- NEW!");
                                    Console.Error.WriteLine("");
                                }
                            }
                        });
                    }
                }

                //if (DoLog)
                //{
                //	Console.WriteLine("----------------------------");
                //	foreach (var Instruction in MipsMethodEmiter.SafeILGenerator.GetEmittedInstructions()) Console.WriteLine(Instruction);
                //}
            }
        }
    }
}