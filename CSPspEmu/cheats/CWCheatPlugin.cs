using CSharpUtils;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Memory;
using CSPspEmu.Inject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.cheats
{
	public class CWCheatPlugin :IInjectInitialize
	{
		[Inject]
		PspDisplay PspDisplay;

		[Inject]
		PspMemory PspMemory;

		[Inject]
		MessageBus MessageBus;

		protected List<CWCheat> CWCheats = new List<CWCheat>();
		//public bool UseFastMemory;

		void PspEmulator_VBlankEventCall()
		{
			foreach (var CWCheat in CWCheats)
			{
				if (PspMemory != null)
				{
					CWCheat.Patch(PspMemory);
				}
			}
			//Console.Error.WriteLine("VBlank!");
		}

		void IInjectInitialize.Initialize()
		{
			PspDisplay.VBlankEventCall += new Action(PspEmulator_VBlankEventCall);

			MessageBus.Register<LoadFileMessage>((LoadFileMessage) =>
			{
				if (File.Exists(LoadFileMessage.FileName + ".cwcheat"))
				{
					ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
					{
						Console.WriteLine("Loaded... {0}", LoadFileMessage.FileName);
					});
					ParseCwCheat(File.ReadAllLines(LoadFileMessage.FileName + ".cwcheat"));
				}
			});
		}

		public void AddCwCheat(uint Code, uint Value)
		{
			CWCheats.Add(new CWCheat()
			{
				Code = Code,
				Value = Value,
			});
			//throw new NotImplementedException();
		}

		public void ParseCwCheat(string[] Lines)
		{
			CWCheats.Clear();
			foreach (var LineRaw in Lines)
			{
				var Line = LineRaw.Trim();
				var Parts = Line.Split(' ', '\t');
				if (Parts.Length >= 3)
				{
					if (Parts[0] == "_L")
					{
						var Code = (uint)NumberUtils.ParseIntegerConstant(Parts[1]);
						var Value = (uint)NumberUtils.ParseIntegerConstant(Parts[2]);
						AddCwCheat(Code, Value);
						//Console.WriteLine("{0} {1:X} {2:X}", Line, Code, Value);
					}
				}
			}
			//Console.ReadKey();
		}
	}
}
