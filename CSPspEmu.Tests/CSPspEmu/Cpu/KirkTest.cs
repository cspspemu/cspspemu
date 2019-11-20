using System;
using Xunit;
using Kirk = CSPspEmu.Core.Components.Crypto.Kirk;

namespace CSPspEmu.Core.Tests.Crypto
{
    
    public unsafe class KirkTest
    {
        [Fact]
        public void TestSha1()
        {
            var Kirk = new Kirk();
            Kirk.kirk_init();

            var Input = new byte[]
            {
                // Size
                0x20, 0x00, 0x00, 0x00,
                // Data
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            };

            var ExpectedOutput = new byte[]
            {
                0xDE, 0x8A, 0x84, 0x7B, 0xFF, 0x8C, 0x34, 0x3D, 0x69, 0xB8, 0x53, 0xA2,
                0x15, 0xE6, 0xEE, 0x77, 0x5E, 0xF2, 0xEF, 0x96
            };

            var Output = new byte[0x14];

            Assert.Equal(0x24, Input.Length);

            fixed (byte* OutputPtr = Output)
            fixed (byte* InputPtr = Input)
            {
                Kirk.KirkSha1(OutputPtr, InputPtr, Input.Length);
            }

            Assert.Equal(ExpectedOutput, Output);
            //Console.WriteLine(BitConverter.ToString(Hash));
        }

        [Fact]
        public void TestCmd1()
        {
            var Kirk = new Kirk();
            Kirk.kirk_init();

            var _src = new byte[0x100];
            var _dst = new byte[0x10];

            var ExpectedOutput = new byte[]
            {
                0xE3, 0x17, 0x49, 0x84, 0xAE, 0xB9, 0xB5, 0xAF, 0x7D, 0x9F, 0x73, 0xAD,
                0x93, 0x66, 0x62, 0xD5
            };

            fixed (byte* src = _src)
            fixed (byte* dst = _dst)
            {
                *(uint*) &src[0x60] = 1; // Mode
                *(uint*) &src[0x70] = 0x10; // DataSize
                *(uint*) &src[0x74] = 0; // DataOffset

                Kirk.kirk_CMD1(dst, src, 0x100, false);

                //Console.WriteLine(BitConverter.ToString(_src));
                //Console.WriteLine(BitConverter.ToString(_dst));

                Assert.Equal(ExpectedOutput, _dst);
            }
        }


        [Fact]
        public void TestCmd7()
        {
            var Kirk = new Kirk();
            Kirk.kirk_init();

            var _src = new byte[0x20 + 0x14];
            var _dst = new byte[0x20];

            var ExpectedOutput = new byte[]
            {
                0xE4, 0x8B, 0x57, 0x4C, 0x6E, 0xAF, 0x62, 0x51, 0x5C, 0x44, 0x52, 0x1D,
                0xBA, 0x7D, 0xD4, 0x32, 0xE4, 0x8B, 0x57, 0x4C, 0x6E, 0xAF, 0x62, 0x51,
                0x5C, 0x44, 0x52, 0x1D, 0xBA, 0x7D, 0xD4, 0x32
            };

            fixed (byte* src = _src)
            fixed (byte* dst = _dst)
            {
                *(uint*) &src[0x00] = 5; // Mode
                *(uint*) &src[0x0C] = 0x03; // KeySeed
                *(uint*) &src[0x10] = 1; // DataSize

                Kirk.kirk_CMD7(dst, src, 0x20);

                Console.WriteLine("_src: {0}", BitConverter.ToString(_src));
                Console.WriteLine("_dst: {0}", BitConverter.ToString(_dst));

                Assert.Equal(ExpectedOutput, _dst);
            }
        }
    }
}