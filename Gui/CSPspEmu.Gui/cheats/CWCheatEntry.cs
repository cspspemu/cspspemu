using CSharpUtils;
using CSPspEmu.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="http://www.codemasters-project.net/guides/showentry.php?e=846"/>
    public struct CWCheatEntry
    {
        byte OpCode
        {
            get { return (byte) ((this.Code & 0xF0000000) >> 28); }
        }

        public uint Code;
        public uint[] Values;

        public void Read(Queue<uint> List)
        {
            Code = List.Dequeue();
            switch (OpCode)
            {
                case 0x8:
                    Values = new[] {List.Dequeue(), List.Dequeue(), List.Dequeue()};
                    break;
                default:
                    Values = new[] {List.Dequeue()};
                    break;
            }
        }

        // NB: the codes are in the relative format from the start of the user ram area.
        // So the absolute adress is relative adress +0x08800000
        // To convert some cheat from the absolute format you need to
        // subtract 0x08800000  from the adress of the code

        // 16-bit Greater Than : Multiple Skip 	Ennndddd 3aaaaaaa
        // 16-bit Less Than : Multiple Skip 0xEnnndddd 0x2aaaaaaa
        // 16-bit Not Equal : Multiple Skip 0xEnnndddd 0x1aaaaaaa
        // 16-bit Equal : Multiple Skip	0xEnnndddd 0x0aaaaaaa
        // 16-bit greater than - TEST CODE - 0xDaaaaaaa 0x0030dddd
        // 16-bit less than - TEST CODE - 0xDaaaaaaa 0x0020dddd
        // 16-bit not equal - TEST CODE - 0xDaaaaaaa 0x0010dddd
        // 16-bit equal - TEST CODE -   	0xDaaaaaaa 0x0000dddd
        // code stopper  0xCaaaaaaa 0xvvvvvvvv
        // Time Command  0xB0000000 0xnnnnnnnn (based on cheat delay)
        // [pointer command] 32-bit write	0x6aaaaaaa 0xvvvvvvvv 0x0002nnnn 0xiiiiiiii
        // [pointer command] 16-bit write	0x6aaaaaaa 0x0000vvvv 0x0001nnnn 0xiiiiiiii
        // [pointer command] 8-bit write	0x6aaaaaaa 0x000000vv 0x0000nnnn 0xiiiiiiii
        // copy byte	0x5aaaaaaa 0xnnnnnnnn 0xbbbbbbbb 0x00000000
        // [tp]32-bit Multi-Address Write 	0x4aaaaaaa 0xxxxxyyyy 0xdddddddd 0x00000000
        // 32-bit decrement 0x30500000 0xaaaaaaaa 0xnnnnnnnn 0x00000000
        // 32-bit increment 0x30400000 0xaaaaaaaa 0xnnnnnnnn 0x00000000
        // 16-bit decrement 0x3030nnnn 0xaaaaaaaa
        // 16-bit increment 0x3020nnnn 0xaaaaaaaa
        // 8-bit decrement 0x301000nn 0xaaaaaaaa
        // 8-bit increment 0x300000nn 0xaaaaaaaa

        public void Patch(PspMemory PspMemory)
        {
            try
            {
                _Patch(PspMemory);
            }
            catch (Exception Exception)
            {
                Console.WriteLine(Exception);
            }
        }

        private void _Patch(PspMemory PspMemory)
        {
            uint Info = this.Code & 0x0FFFFFFF;
            //uint Address = 0x08804000 + Info;
            uint Address = 0x08800000 + Info;
            try
            {
                switch (OpCode)
                {
                    // [t]8-bit Constant Write 0x0aaaaaaa 0x000000dd
                    case 0x0:
                        PspMemory.WriteSafe(Address, (byte) BitUtils.Extract(Values[0], 0, 8));
                        break;
                    // [t]16-bit Constant write 0x1aaaaaaa 0x0000dddd
                    case 0x1:
                        PspMemory.WriteSafe(Address, (ushort) BitUtils.Extract(Values[0], 0, 16));
                        break;
                    // [t]32-bit Constant write 0x2aaaaaaa 0xdddddddd
                    case 0x2:
                        PspMemory.WriteSafe(Address, (uint) BitUtils.Extract(Values[0], 0, 32));
                        break;
                    // 32-bit Multi-Address Write/Value increase	0x4aaaaaaa 0xxxxxyyyy 0xdddddddd 0xIIIIIIII
                    case 0x4:
                    {
                        var Count = BitUtils.Extract(Values[0], 16, 16);
                        var Increment = BitUtils.Extract(Values[0], 0, 16);
                        var Value = BitUtils.Extract(Values[1], 0, 32);
                        var IncrementValue = BitUtils.Extract(Values[2], 0, 32);
                        for (int n = 0; n < Count; n++)
                        {
                            PspMemory.WriteSafe((uint) (Address + n * Increment), (uint) (Value + IncrementValue * n));
                        }
                    }
                        break;
                    case 0x8:
                        // 16-bit Multi-Address Write/Value increas	0x8aaaaaaa 0xxxxxyyyy 0x1000dddd 0xIIIIIIII
                        if (BitUtils.Extract(Values[1], 28, 4) == 1)
                        {
                            var Count = BitUtils.Extract(Values[0], 16, 16);
                            var Increment = BitUtils.Extract(Values[0], 0, 16);
                            var Value = BitUtils.Extract(Values[1], 0, 16);
                            var IncrementValue = BitUtils.Extract(Values[2], 0, 32);
                            for (int n = 0; n < Count; n++)
                            {
                                PspMemory.WriteSafe((uint) (Address + n * Increment),
                                    (ushort) (Value + IncrementValue * n));
                            }
                        }
                        // 8-bit Multi-Address Write/Value increase	0x8aaaaaaa 0xxxxxyyyy 0x000000dd 0xIIIIIIII	
                        else
                        {
                            var Count = BitUtils.Extract(Values[0], 16, 16);
                            var Increment = BitUtils.Extract(Values[0], 0, 16);
                            var Value = BitUtils.Extract(Values[1], 0, 8);
                            var IncrementValue = BitUtils.Extract(Values[2], 0, 32);
                            for (int n = 0; n < Count; n++)
                            {
                                PspMemory.WriteSafe((uint) (Address + n * Increment),
                                    (byte) (Value + IncrementValue * n));
                            }
                        }
                        break;
                    // 16-bit XOR - 0x7aaaaaaa 0x0005vvvv
                    // 8-bit  XOR - 0x7aaaaaaa 0x000400vv
                    // 16-bit AND - 0x7aaaaaaa 0x0003vvvv
                    // 8-bit  AND - 0x7aaaaaaa 0x000200vv
                    // 16-bit OR  - 0x7aaaaaaa 0x0001vvvv
                    // 8-bit  OR  - 0x7aaaaaaa 0x000000vv
                    case 0x7:
                    {
                        uint SubOpCode = (Values[0] >> 16) & 0xFFFF;
                        uint SubValue = (Values[0] >> 0) & 0xFFFF;
                        switch (SubOpCode)
                        {
                            // 8-bit  OR  - 0x7aaaaaaa 0x000000vv
                            case 0:
                                PspMemory.WriteSafe(Address,
                                    (byte) (PspMemory.ReadSafe<byte>(Address) | (SubValue & 0xFF)));
                                break;
                            default:
                                Console.Error.WriteLine("Invalid CWCheatOpCode: 0x{0:X} : 0x{1:X}", OpCode, SubOpCode);
                                break;
                        }
                    }
                        break;
                    default:
                        Console.Error.WriteLine("Invalid CWCheatOpCode: 0x{0:X}", OpCode);
                        break;
                }
            }
            catch (Exception Exception)
            {
                throw (new Exception(String.Format("At Address: 0x{0:X}", Address), Exception));
            }
        }
    }
}