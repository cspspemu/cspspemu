using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Controller
{
	unsafe public class PspController : PspEmulatorComponent
	{
		public const int MaxStoredFrames = 128;

		public enum SamplingModeEnum
		{
			Digital = 0,
			Analogic = 1,
		}

		protected List<SceCtrlData> SceCtrlDataBuffer = new List<SceCtrlData>();
		public int SamplingCycle;
		public SamplingModeEnum SamplingMode;
		public int LatchSamplingCount;
		public uint LastTimestamp;

		public override void InitializeComponent()
		{
			for (int n = 0; n < MaxStoredFrames; n++)
			{
				InsertSceCtrlData(default(SceCtrlData));
			}
		}

		public void InsertSceCtrlData(SceCtrlData SceCtrlData)
		{
			lock (this)
			{
				SceCtrlData.TimeStamp = LastTimestamp++;
				SceCtrlDataBuffer.Add(SceCtrlData);
				if (SceCtrlDataBuffer.Count > MaxStoredFrames) SceCtrlDataBuffer.RemoveAt(0);
			}
			LatchSamplingCount++;
		}

		public SceCtrlData GetSceCtrlDataAt(int Index)
		{
			lock (this)
			{
				return SceCtrlDataBuffer[SceCtrlDataBuffer.Count - Index - 1];
			}
		}
	}

	public enum PspCtrlButtons : uint
	{
		None = 0x0000000,

		/// <summary>
		/// Select button.
		/// </summary>
		Select = 0x0000001,

		/// <summary>
		/// Start button.
		/// </summary>
		Start = 0x0000008,

		/// <summary>
		/// Up D-Pad button.
		/// </summary>
		Up = 0x0000010,

		/// <summary>
		/// Right D-Pad button.
		/// </summary>
		Right = 0x0000020,

		/// <summary>
		/// Down D-Pad button.
		/// </summary>
		Down = 0x0000040,

		/// <summary>
		/// Left D-Pad button.
		/// </summary>
		Left = 0x0000080,

		/// <summary>
		/// Left trigger.
		/// </summary>
		LeftTrigger = 0x0000100,

		/// <summary>
		/// Right trigger.
		/// </summary>
		RightTrigger = 0x0000200,

		/// <summary>
		/// Triangle button.
		/// </summary>
		Triangle = 0x0001000,

		/// <summary>
		/// Circle button.
		/// </summary>
		Circle = 0x0002000,

		/// <summary>
		/// Cross button.
		/// </summary>
		Cross = 0x0004000,

		/// <summary>
		/// Square button.
		/// </summary>
		Square = 0x0008000,

		/// <summary>
		/// Home button. In user mode this bit is set if the exit dialog is visible.
		/// </summary>
		Home = 0x0010000,

		/// <summary>
		/// Hold button.
		/// </summary>
		Hold = 0x0020000,

		/// <summary>
		/// Wlan switch up.
		/// </summary>
		WirelessLanUp = 0x0040000,

		/// <summary>
		/// Remote hold position.
		/// </summary>
		Remote = 0x0080000,

		/// <summary>
		/// Volume up button.
		/// </summary>
		VolumeUp = 0x0100000,

		/// <summary>
		/// Volume down button.
		/// </summary>
		VolumeDown = 0x0200000,

		/// <summary>
		/// Screen button.
		/// </summary>
		Screen = 0x0400000,

		/// <summary>
		/// Music Note button.
		/// </summary>
		Note = 0x0800000,

		/// <summary>
		/// Disc present.
		/// </summary>
		DiscPresent = 0x1000000,

		/// <summary>
		/// Memory stick present.
		/// </summary>
		MemoryStickPresent = 0x2000000,
	}

	unsafe public struct SceCtrlData
	{
		/// <summary>
		/// The current read frame.
		/// </summary>
		public uint TimeStamp;

		/// <summary>
		/// Bit mask containing zero or more of ::PspCtrlButtons.
		/// </summary>
		public PspCtrlButtons Buttons;

		/// <summary>
		/// Analogue stick, X axis.
		/// </summary>
		public byte Lx;

		/// <summary>
		/// Analogue stick, Y axis.
		/// </summary>
		public byte Ly;

		/// <summary>
		/// Reserved bytes.
		/// </summary>
		public fixed byte Rsrv[6];

		/// <summary>
		/// Analog X : [-1.0, +1.0]
		/// </summary>
		public float X
		{
			get
			{
				return (((float)Lx / 255.0f) - 0.5f) * 2.0f;
			}
			set
			{
				Lx = (byte)(((value / 2.0f) + 0.5f) * 255.0f);
			}
		}

		/// <summary>
		/// Analog Y : [-1.0, +1.0]
		/// </summary>
		public float Y
		{
			get
			{
				return (((float)Ly / 255.0f) - 0.5f) * 2.0f;
			}
			set
			{
				Ly = (byte)(((value / 2.0f) + 0.5f) * 255.0f);
			}
		}

		public void UpdateButtons(PspCtrlButtons Buttons, bool Pressed)
		{
			if (Pressed)
			{
				this.Buttons |= Buttons;
			}
			else
			{
				this.Buttons &= ~Buttons;
			}
		}
	}
}
