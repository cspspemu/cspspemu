using CSharpUtils;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.VFpu
{
	public class VfpuPrefix
	{
		public uint DeclaredPC;
		public uint UsedPC;
		public uint Value;
		public bool Enabled;
		public int UsedCount;

		public void CheckPrefixUsage(uint PC)
		{
			// Disable the prefix once it have been used.
			if (this.Enabled)
			{
				if (this.UsedCount > 0 && this.UsedPC != PC)
				{
					this.Enabled = false;
				}

				this.UsedPC = PC;
				this.UsedCount++;
			}
		}

		public static implicit operator uint(VfpuPrefix Value) { return Value.Value; }
		public static implicit operator VfpuPrefix(uint Value) { return new VfpuPrefix() { Value = Value }; }

		// swz(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (0 + i * 2)) & 3;
		public uint SourceIndex(int i) { return BitUtils.Extract(Value, 0 + i * 2, 2); }
		public void SourceIndex(int i, uint ValueToInsert) { BitUtils.Insert(ref Value, 0 + i * 2, 2, ValueToInsert); }

		// abs(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (8 + i * 1)) & 1;
		public bool SourceAbsolute(int i) { return BitUtils.Extract(Value, 8 + i * 1, 1) != 0; }
		public void SourceAbsolute(int i, bool ValueToInsert) { BitUtils.Insert(ref Value, 8 + i * 1, 1, ValueToInsert ? 1U : 0U); }

		// cst(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (12 + i * 1)) & 1;
		public bool SourceConstant(int i) { return BitUtils.Extract(Value, 12 + i * 1, 1) != 0; }
		public void SourceConstant(int i, bool ValueToInsert) { BitUtils.Insert(ref Value, 12 + i * 1, 1, ValueToInsert ? 1U : 0U); }

		// neg(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (16 + i * 1)) & 1;
		public bool SourceNegate(int i) { return BitUtils.Extract(Value, 16 + i * 1, 1) != 0; }
		public void SourceNegate(int i, bool ValueToInsert) { BitUtils.Insert(ref Value, 16 + i * 1, 1, ValueToInsert ? 1U : 0U); }

		public void EnableAndSetValueAndPc(uint Value, uint PC)
		{
			this.Enabled = true;
			this.Value = Value;
			this.DeclaredPC = PC;
			this.UsedCount = 0;
		}

		public string Format
		{
			get
			{
				var Parts = new List<string>();
				for (int Index = 0; Index < 4; Index++)
				{
					string Part = "";
					if (SourceConstant(Index))
					{
						switch (SourceIndex(Index))
						{
							case 0: Part = SourceAbsolute(Index) ? "3" : "0"; break;
							case 1: Part = SourceAbsolute(Index) ? "1/3" : "1"; break;
							case 2: Part = SourceAbsolute(Index) ? "1/4" : "2"; break;
							case 3: Part = SourceAbsolute(Index) ? "1/6" : "1/2"; break;
							default: throw (new InvalidOperationException());
						}
					}
					else
					{
						Part = ComponentNames[SourceIndex(Index)];
						if (SourceAbsolute(Index)) Part = "|" + Part + "|";
						if (SourceNegate(Index)) Part = "-" + Part;
					}

					Parts.Add(Part);
				}
				return "[" + String.Join(", ", Parts) + "]";
			}
		}


		public static readonly string[] ComponentNames = new string[] { "x", "y", "z", "w" };

		public override string ToString()
		{
			return String.Format(
				"VfpuPrefix(Enabled={0}, UsedPC=0x{1:X}, DeclaredPC=0x{2:X})({3})",
				Enabled, UsedPC, DeclaredPC, Format
			);
		}

		public bool IsValidIndex(int Index)
		{
			return (Index >= 0) && (Index < 4);
		}
	}

	public class VfpuDestinationPrefix
	{
		public uint DeclaredPC;
		public uint UsedPC;
		public uint Value;
		public bool Enabled;
		public int UsedCount;

		public void CheckPrefixUsage(uint PC)
		{
			// Disable the prefix once it have been used.
			if (this.Enabled)
			{
				if (this.UsedCount > 0 && this.UsedPC != PC)
				{
					this.Enabled = false;
				}
			}
		}
	
		public static implicit operator uint(VfpuDestinationPrefix Value) { return Value.Value; }
		public static implicit operator VfpuDestinationPrefix(uint Value) { return new VfpuDestinationPrefix() { Value = Value }; }
	
		// sat(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (0 + i * 2)) & 3;
		public uint DestinationSaturation(int i) { return BitUtils.Extract(Value, 0 + i * 2, 2); }
		public void DestinationSaturation(int i, uint ValueToInsert) { BitUtils.Insert(ref Value, 0 + i * 2, 2, ValueToInsert); }
	
		// msk(xyzw)
		//assert(i >= 0 && i < 4);
		//return (value >> (8 + i * 1)) & 1;
		public bool DestinationMask(int i) { return BitUtils.Extract(Value, 8 + i * 1, 1) != 0; }
		public void DestinationMask(int i, bool ValueToInsert) { BitUtils.Insert(ref Value, 8 + i * 1, 1, ValueToInsert ? 1U : 0U); }
	
		public void EnableAndSetValueAndPc(uint Value, uint PC)
		{
			this.Enabled = true;
			this.Value = Value;
			this.DeclaredPC = PC;
			this.UsedCount = 0;
		}
	
		public string Format
		{
			get
			{
				var Parts = new List<string>();
				for (int Index = 0; Index < 4; Index++)
				{
					string Part = "";
					if (!DestinationMask(Index))
					{
						switch (DestinationSaturation(Index))
						{
							case 1: Part = "0:1"; break;
							case 3: Part = "-1:1"; break;
							default: throw (new InvalidOperationException());
						}
					}
					else
					{
						Part = "M";
					}
	
					Parts.Add(Part);
				}
				return "[" + String.Join(", ", Parts) + "]";
			}
		}
	
		public override string ToString()
		{
			return String.Format(
				"VfpuDestinationPrefix(Enabled={0}, UsedPC=0x{1:X}, DeclaredPC=0x{2:X})({3})",
				Enabled, UsedPC, DeclaredPC, Format
			);
		}

		public bool IsValidIndex(int Index)
		{
			return (Index >= 0) && (Index < 4);
		}
	}
}
