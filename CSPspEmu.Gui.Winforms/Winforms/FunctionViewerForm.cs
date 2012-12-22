using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms
{
	public partial class FunctionViewerForm : Form
	{
		private CpuProcessor CpuProcessor;

		public FunctionViewerForm()
		{
			InitializeComponent();
		}

		public FunctionViewerForm(CpuProcessor CpuProcessor)
		{
			this.CpuProcessor = CpuProcessor;
			InitializeComponent();
		}

		public class PCItem
		{
			public uint PC;

			public override string ToString()
			{
				return String.Format("0x{0:X8}", PC);
			}
		}

		private void FunctionViewerForm_Load(object sender, EventArgs e)
		{
			PcListBox.SuspendLayout();
			foreach (var PC in CpuProcessor.MethodCache.PCs)
			{
				if (CpuProcessor.MethodCache.GetForPC(PC).AstTree != null)
				{
					PcListBox.Items.Add(new PCItem()
					{
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
				var PCItem = (PCItem)PcListBox.SelectedItem;
				var MethodCacheInfo = CpuProcessor.MethodCache.GetForPC(PCItem.PC);
				var MinPC = MethodCacheInfo.MinPC;
				var MaxPC = MethodCacheInfo.MaxPC;
				var Memory = CpuProcessor.Memory;
				AstNodeStm Node = null;
				if (MethodCacheInfo.AstTree != null) Node = MethodCacheInfo.AstTree.Optimize(CpuProcessor);

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
							OutString = Node.ToILString<Action<CpuThreadState>>();
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
									OutString += String.Format("0x{0:X8}: {1}\r\n", PC, Result.ToString());
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

		private void ViewTextBox_TextChanged(object sender, EventArgs e)
		{

		}

		private void PcListBox_SelectedIndexChanged_1(object sender, EventArgs e)
		{
			UpdateText();
		}

		private void LanguageComboBox_SelectedIndexChanged_1(object sender, EventArgs e)
		{
			UpdateText();
		}

		private void ViewTextBox_TextChanged_1(object sender, EventArgs e)
		{

		}
	}
}
