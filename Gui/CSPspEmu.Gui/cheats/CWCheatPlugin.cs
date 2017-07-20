using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Inject;
using System;
using System.Collections.Generic;
using System.IO;
using CSPspEmu.Core.Components.Display;

namespace CSPspEmu.cheats
{
    public class CWCheatPlugin : IInjectInitialize
    {
        [Inject] PspDisplay PspDisplay;

        [Inject] PspMemory PspMemory;

        [Inject] MessageBus MessageBus;

        protected List<CWCheatEntry> CWCheats = new List<CWCheatEntry>();
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

        private string _Cheats;
        private string LinkedCwcheatsFile;
        private bool LinkedCwcheatsFileMustWrite;

        public string Cheats
        {
            get { return _Cheats; }
            set
            {
                _Cheats = value.Trim();
                if (!string.IsNullOrEmpty(_Cheats) || LinkedCwcheatsFileMustWrite)
                {
                    File.WriteAllText(LinkedCwcheatsFile, _Cheats);
                }
                ParseCwCheat(_Cheats.Split('\n', '\r'));
            }
        }

        void IInjectInitialize.Initialize()
        {
            PspDisplay.VBlankEventCall += new Action(PspEmulator_VBlankEventCall);

            MessageBus.Register<LoadFileMessage>((LoadFileMessage) =>
            {
                LinkedCwcheatsFile = LoadFileMessage.FileName + ".cwcheat";
                if (File.Exists(LinkedCwcheatsFile))
                {
                    ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta,
                        () => { Console.WriteLine("Loaded... {0}", LoadFileMessage.FileName); });
                    LinkedCwcheatsFileMustWrite = true;
                    this.Cheats = File.ReadAllText(LoadFileMessage.FileName + ".cwcheat");
                }
                else
                {
                    this.Cheats = "";
                }
            });
        }

        //public void AddCwCheat(uint Code, uint Value)
        //{
        //	CWCheats.Add(new CWCheatEntry()
        //	{
        //		Code = Code,
        //		Value = Value,
        //	});
        //	//throw new NotImplementedException();
        //}

        public void ParseCwCheat(string[] Lines)
        {
            CWCheats.Clear();
            var Values = new Queue<uint>();
            foreach (var LineRaw in Lines)
            {
                var Line = LineRaw.Trim();
                if (Line.Substr(0, 1) == ";") continue;
                if (Line.Substr(0, 1) == "#") continue;
                var Parts = Line.Split(' ', '\t');
                foreach (var Part in Parts)
                {
                    if (Part.Substr(0, 2) == "0x")
                    {
                        Values.Enqueue((uint) NumberUtils.ParseIntegerConstant(Part));
                    }
                }
            }
            while (Values.Count > 0)
            {
                var Entry = new CWCheatEntry();
                Entry.Read(Values);
                CWCheats.Add(Entry);
            }
            //Console.ReadKey();
        }
    }
}