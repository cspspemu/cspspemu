using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui
{
    [Flags]
    public enum PspCtrlAnalog
    {
        None = 0,
        Left = (1 << 0),
        Right = (1 << 1),
        Up = (1 << 2),
        Down = (1 << 3),
    }

    public class CommonGuiInput
    {
        /// <summary>
        /// 
        /// </summary>
        internal SceCtrlData SceCtrlData;

        IGuiExternalInterface IGuiExternalInterface;

        PspStoredConfig StoredConfig
        {
            get { return IGuiExternalInterface.InjectContext.GetInstance<PspStoredConfig>(); }
        }

        PspController PspController
        {
            get { return IGuiExternalInterface.InjectContext.GetInstance<PspController>(); }
        }

        public CommonGuiInput(IGuiExternalInterface IGuiExternalInterface)
        {
            this.IGuiExternalInterface = IGuiExternalInterface;
        }

        //float AnalogX, AnalogY;
        bool AnalogUp = false;

        bool AnalogDown = false;
        bool AnalogLeft = false;
        bool AnalogRight = false;

        private Dictionary<string, PspCtrlButtons> KeyMap = new Dictionary<string, PspCtrlButtons>();
        private Dictionary<string, PspCtrlAnalog> AnalogKeyMap = new Dictionary<string, PspCtrlAnalog>();
        float AnalogX = 0.0f, AnalogY = 0.0f;

        public string NormalizeKeyName(string Key)
        {
            return Key.ToUpperInvariant();
        }

        public void ReLoadControllerConfig()
        {
            var ControllerConfig = StoredConfig.ControllerConfig;

            AnalogKeyMap = new Dictionary<string, PspCtrlAnalog>();
            {
                AnalogKeyMap[NormalizeKeyName(ControllerConfig.AnalogLeft)] = PspCtrlAnalog.Left;
                AnalogKeyMap[NormalizeKeyName(ControllerConfig.AnalogRight)] = PspCtrlAnalog.Right;
                AnalogKeyMap[NormalizeKeyName(ControllerConfig.AnalogUp)] = PspCtrlAnalog.Up;
                AnalogKeyMap[NormalizeKeyName(ControllerConfig.AnalogDown)] = PspCtrlAnalog.Down;
            }

            KeyMap = new Dictionary<string, PspCtrlButtons>();
            {
                KeyMap[NormalizeKeyName(ControllerConfig.DigitalLeft)] = PspCtrlButtons.Left;
                KeyMap[NormalizeKeyName(ControllerConfig.DigitalRight)] = PspCtrlButtons.Right;
                KeyMap[NormalizeKeyName(ControllerConfig.DigitalUp)] = PspCtrlButtons.Up;
                KeyMap[NormalizeKeyName(ControllerConfig.DigitalDown)] = PspCtrlButtons.Down;

                KeyMap[NormalizeKeyName(ControllerConfig.TriangleButton)] = PspCtrlButtons.Triangle;
                KeyMap[NormalizeKeyName(ControllerConfig.CrossButton)] = PspCtrlButtons.Cross;
                KeyMap[NormalizeKeyName(ControllerConfig.SquareButton)] = PspCtrlButtons.Square;
                KeyMap[NormalizeKeyName(ControllerConfig.CircleButton)] = PspCtrlButtons.Circle;

                KeyMap[NormalizeKeyName(ControllerConfig.StartButton)] = PspCtrlButtons.Start;
                KeyMap[NormalizeKeyName(ControllerConfig.SelectButton)] = PspCtrlButtons.Select;

                KeyMap[NormalizeKeyName(ControllerConfig.LeftTriggerButton)] = PspCtrlButtons.LeftTrigger;
                KeyMap[NormalizeKeyName(ControllerConfig.RightTriggerButton)] = PspCtrlButtons.RightTrigger;
            }

            Console.WriteLine("KeyMapping:");

            foreach (var Map in AnalogKeyMap)
            {
                Console.WriteLine("  '{0}' -> PspCtrlAnalog.{1}", Map.Key, Map.Value);
            }

            foreach (var Map in KeyMap)
            {
                Console.WriteLine("  '{0}' -> PspCtrlButtons.{1}", Map.Key, Map.Value);
            }
        }

        private PspCtrlButtons GetButtonsFromKeys(string Key)
        {
            return KeyMap.GetOrDefault(Key, PspCtrlButtons.None);
        }

        private void TryUpdateAnalog(string Key, bool Press)
        {
            switch (AnalogKeyMap.GetOrDefault(Key, PspCtrlAnalog.None))
            {
                case PspCtrlAnalog.Up:
                    AnalogUp = Press;
                    break;
                case PspCtrlAnalog.Down:
                    AnalogDown = Press;
                    break;
                case PspCtrlAnalog.Left:
                    AnalogLeft = Press;
                    break;
                case PspCtrlAnalog.Right:
                    AnalogRight = Press;
                    break;
            }
        }

        public void SendControllerFrame()
        {
            SceCtrlData.X = 0;
            SceCtrlData.Y = 0;

            bool AnalogXUpdated = false;
            bool AnalogYUpdated = false;
            if (AnalogUp)
            {
                AnalogY -= 0.4f;
                AnalogYUpdated = true;
            }
            if (AnalogDown)
            {
                AnalogY += 0.4f;
                AnalogYUpdated = true;
            }
            if (AnalogLeft)
            {
                AnalogX -= 0.4f;
                AnalogXUpdated = true;
            }
            if (AnalogRight)
            {
                AnalogX += 0.4f;
                AnalogXUpdated = true;
            }
            if (!AnalogXUpdated) AnalogX /= 2.0f;
            if (!AnalogYUpdated) AnalogY /= 2.0f;

            AnalogX = MathFloat.Clamp(AnalogX, -1.0f, 1.0f);
            AnalogY = MathFloat.Clamp(AnalogY, -1.0f, 1.0f);

            //Console.WriteLine("{0}, {1}", AnalogX, AnalogY);

            SceCtrlData.X = AnalogX;
            SceCtrlData.Y = AnalogY;

            PspController.InsertSceCtrlData(SceCtrlData);
            //Console.WriteLine("CommonGuiInput.SendControllerFrame()");
        }

        public void KeyPress(string Key)
        {
            Key = NormalizeKeyName(Key);
            TryUpdateAnalog(Key, true);
            var Buttons = GetButtonsFromKeys(Key);
            //Console.WriteLine("KeyPress: {0}, {1}", Key, Buttons);
            SceCtrlData.UpdateButtons(Buttons, true);
        }

        public void KeyRelease(string Key)
        {
            Key = NormalizeKeyName(Key);
            TryUpdateAnalog(Key, false);
            var Buttons = GetButtonsFromKeys(Key);
            //Console.WriteLine("KeyRelease: {0}, {1}", Key, Buttons);
            SceCtrlData.UpdateButtons(Buttons, false);
        }
    }
}