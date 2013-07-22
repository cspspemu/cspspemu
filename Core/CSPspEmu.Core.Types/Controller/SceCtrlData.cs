using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Types
{
	public struct SceCtrlData
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
		public byte Rsrv0;
		public byte Rsrv1;
		public byte Rsrv2;
		public byte Rsrv3;
		public byte Rsrv4;
		public byte Rsrv5;

		public SceCtrlData Init()
		{
			this.X = 0;
			this.Y = 0;
			return this;
		}

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
