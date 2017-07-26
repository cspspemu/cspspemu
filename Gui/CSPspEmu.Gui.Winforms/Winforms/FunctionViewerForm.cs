using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.InstructionCache;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Serializers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;
using CSharpUtils.Drawing;
using CSPspEmu.Core.Cpu.Dynarec.Ast;

namespace CSPspEmu.Gui.Winforms
{
    public partial class FunctionViewerForm : Form
    {
        [Inject] private CpuProcessor CpuProcessor;

        private FunctionViewerForm()
        {
            InitializeComponent();
        }

        public class PCItem
        {
            public MethodCache MethodCache;
            public uint PC;
            public MethodCacheInfo Entry;

            public MethodCacheInfo MethodCacheInfo => MethodCache.GetForPc(PC);

            public Color ItemColor
            {
                get
                {
                    if (Entry.HasSpecialName) return Color.Blue;
                    if (MethodCacheInfo.DynarecFunction.TimeTotal >= TimeSpan.FromMilliseconds(40))
                        return ColorUtils.Mix(Color.White, Color.Red, 1);
                    if (MethodCacheInfo.DynarecFunction.TimeTotal >= TimeSpan.FromMilliseconds(20))
                        return ColorUtils.Mix(Color.White, Color.Red, 0.75);
                    if (MethodCacheInfo.DynarecFunction.TimeTotal >= TimeSpan.FromMilliseconds(10))
                        return ColorUtils.Mix(Color.White, Color.Red, 0.5);
                    return Color.Black;
                }
            }

            public string Message => Entry.Name;

            public override string ToString()
            {
                return Message;
            }
        }

        private void FunctionViewerForm_Load(object sender, EventArgs e)
        {
            PcListBox.SuspendLayout();
            foreach (var PC in CpuProcessor.MethodCache.PCs.OrderBy(Item => Item))
            {
                var Entry = CpuProcessor.MethodCache.GetForPc(PC);
                if (Entry.AstTree != null)
                {
                    PcListBox.Items.Add(new PCItem()
                    {
                        MethodCache = CpuProcessor.MethodCache,
                        Entry = Entry,
                        PC = PC,
                    });
                }
            }
            LanguageComboBox.SelectedIndex = 0;
            if (PcListBox.Items.Count > 0)
            {
                PcListBox.SelectedIndex = 0;
            }
            PcListBox.ResumeLayout();
        }

        private void UpdateText()
        {
            if (PcListBox.SelectedItem != null)
            {
                var PCItem = (PCItem) PcListBox.SelectedItem;
                var MethodCacheInfo = PCItem.MethodCacheInfo;
                var MinPC = MethodCacheInfo.MinPc;
                var MaxPC = MethodCacheInfo.MaxPc;
                var Memory = CpuProcessor.Memory;
                AstNodeStm Node = null;
                if (MethodCacheInfo.AstTree != null) Node = MethodCacheInfo.AstTree.Optimize(CpuProcessor);

                var InfoLines = new List<string>();

                InfoLines.Add($"Name: {MethodCacheInfo.Name}");
                InfoLines.Add($"TotalInstructions: {MethodCacheInfo.TotalInstructions}");
                InfoLines.Add($"DisableOptimizations: {MethodCacheInfo.DynarecFunction.DisableOptimizations}");

                InfoLines.Add($"EntryPC: 0x{MethodCacheInfo.EntryPc:X8}");
                InfoLines.Add($"MinPC: 0x{MethodCacheInfo.MinPc:X8}");
                InfoLines.Add($"MaxPC: 0x{MethodCacheInfo.MaxPc:X8}");
                InfoLines.Add(
                    $"TimeAnalyzeBranches: {MethodCacheInfo.DynarecFunction.TimeAnalyzeBranches.TotalMilliseconds}");
                InfoLines.Add($"TimeGenerateAst: {MethodCacheInfo.DynarecFunction.TimeGenerateAst.TotalMilliseconds}");
                InfoLines.Add($"TimeOptimize: {MethodCacheInfo.DynarecFunction.TimeOptimize.TotalMilliseconds}");
                InfoLines.Add($"TimeGenerateIL: {MethodCacheInfo.DynarecFunction.TimeGenerateIl.TotalMilliseconds}");
                InfoLines.Add(
                    $"TimeCreateDelegate: {MethodCacheInfo.DynarecFunction.TimeCreateDelegate.TotalMilliseconds}");
                InfoLines.Add($"TimeLinking: {MethodCacheInfo.DynarecFunction.TimeLinking.TotalMilliseconds}");
                InfoLines.Add($"TimeTotal: {MethodCacheInfo.DynarecFunction.TimeTotal.TotalMilliseconds}");

                InfoLines.Add(string.Format(""));
                foreach (var Item in MethodCacheInfo.DynarecFunction.InstructionStats.OrderBy(Pair => Pair.Value))
                {
                    InfoLines.Add($"{Item.Key}: {Item.Value}");
                }

                InfoTextBox.Text = string.Join("\r\n", InfoLines);

                var OutString = "";
                switch (LanguageComboBox.SelectedItem.ToString())
                {
                    case "C#":
                        if (Node != null)
                        {
                            OutString = Node.ToCSharpString().Replace("CpuThreadState.", "");
                        }
                        break;
                    case "IL":
                        if (Node != null)
                        {
                            OutString = Node.ToIlString<Action<CpuThreadState>>();
                        }
                        break;
                    case "Ast":
                        if (Node != null)
                        {
                            OutString = AstSerializer.SerializeAsXml(Node);
                        }
                        break;
                    case "Mips":
                    {
                        var MipsDisassembler = new MipsDisassembler();
                        try
                        {
                            for (uint PC = MinPC; PC <= MaxPC; PC += 4)
                            {
                                var Instruction = Memory.ReadSafe<Instruction>(PC);
                                var Result = MipsDisassembler.Disassemble(PC, Instruction);
                                OutString += $"0x{PC:X8}: {Result.ToString()}\r\n";
                            }
                        }
                        catch (Exception Exception)
                        {
                            Console.Error.WriteLine(Exception);
                        }
                    }
                        break;
                    default:
                        break;
                }

                ViewTextBox.Text = OutString.Replace("\n", "\r\n");
            }
        }

        private void saveILAsDLLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DynarecConfig.FunctionCallWithStaticReferences)
            {
                MessageBox.Show("_DynarecConfig.FunctionCallWithStaticReferences enabled. It will break exporting.");
            }

            var nameOfAssembly = "OutputAssembly";
            var nameOfModule = "OutputModule";
            var nameOfDLL = "cspspemu_temp_output.dll";
            var nameOfType = "OutputType";

            var SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.FileName = nameOfDLL;
            SaveFileDialog.DefaultExt = ".dll";
            SaveFileDialog.AddExtension = true;
            var Result = SaveFileDialog.ShowDialog();
            if (Result != System.Windows.Forms.DialogResult.Cancel)
            {
                var AssemblyName = new System.Reflection.AssemblyName {Name = nameOfAssembly};
                var AssemblyBuilder =
                    Thread.GetDomain().DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Save);
                var ModuleBuilder = AssemblyBuilder.DefineDynamicModule(nameOfModule, nameOfDLL);
                var TypeBuilder = ModuleBuilder.DefineType(nameOfType, TypeAttributes.Public | TypeAttributes.Class);

                //FieldBuilder targetWrapedObjectField = typeBuilder.DefineField("_" + targetWrapType.FullName.Replace(".", ""), targetWrapType, System.Reflection.FieldAttributes.Private);
                //MethodAttributes constructorAttributes = System.Reflection.MethodAttributes.Public;
                //
                //Type objType = Type.GetType("System.Object");
                //ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);
                //ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(constructorAttributes, System.Reflection.CallingConventions.Standard, new Type[] { targetWrapType });
                //System.Reflection.Emit.ILGenerator ilConstructor = constructorBuilder.GetILGenerator();

                foreach (var PC in CpuProcessor.MethodCache.PCs.OrderBy(Item => Item))
                {
                    var Entry = CpuProcessor.MethodCache.GetForPc(PC);
                    if (Entry.AstTree != null)
                    {
                        var MethodBuilder = TypeBuilder.DefineMethod("Method_" + Entry.Name,
                            MethodAttributes.Public | MethodAttributes.Static, typeof(void),
                            new[] {typeof(CpuThreadState)});
                        Entry.AstTree.GenerateIl(MethodBuilder, MethodBuilder.GetILGenerator());
                        //MethodBuilder.CreateDelegate(typeof(Action<CpuThreadState>));
                    }

                    //break;
                }

                TypeBuilder.CreateType();

                AssemblyBuilder.Save(nameOfDLL);
                File.Copy(nameOfDLL, SaveFileDialog.FileName, true);
            }
        }

        private void PcListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void PcListBox_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            if (e.Index >= 0)
            {
                var ListBox = (ListBox) sender;
                var item = ListBox.Items[e.Index] as PCItem;

                var Color = item.ItemColor;

                if (item == ListBox.SelectedItem)
                {
                    //Color = SystemColors.HighlightText;
                }

                e.Graphics.DrawString(
                    item.ToString(),
                    ListBox.Font,
                    new SolidBrush(Color),
                    e.Bounds
                );
            }
        }

        private void ViewTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void ViewTextBox_TextChanged_1(object sender, EventArgs e)
        {
        }
    }
}