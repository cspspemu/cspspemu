using System.Linq;

using CSharpUtils;
using CSharpUtils.Extensions;
using Xunit;

namespace CSPspEmu.Tests
{
    
    public unsafe class PointerUtilsTest
    {
        [Fact]
        public void TestMemset()
        {
            var Data = new byte[131];
            PointerUtils.Memset(Data, 0x3E, Data.Length);

            Assert.Equal(
                ((byte) 0x3E).Repeat(Data.Length),
                Data
            );
        }

        [Fact]
        public void TestMemcpy()
        {
            var sizeStart = 17;
            var sizeMiddle = 77;
            var sizeEnd = 17;
            var dst = new byte[sizeStart + sizeMiddle + sizeEnd];
            fixed (byte* DstPtr = &dst[sizeStart])
            {
                PointerUtils.Memcpy(DstPtr, ((byte) 0x1D).Repeat(sizeMiddle).ToArray(), sizeMiddle);
            }

            var Expected = ((byte) 0x00).Repeat(sizeStart).Concat(((byte) 0x1D).Repeat(sizeMiddle))
                .Concat(((byte) 0x00).Repeat(sizeEnd)).ToArray();

            //Console.WriteLine(BitConverter.ToString(Dst));
            //Console.WriteLine(BitConverter.ToString(Expected));

            Assert.Equal(
                Expected,
                dst
            );
        }

        [Fact]
        public void TestMemcpy4()
        {
            int TotalSize = 12;
            var Source = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
            var Dest = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

            foreach (var Set64 in new[] {false, true})
            {
                //PointerUtils.Is64 = Set64;

                fixed (byte* sourcePtr = Source)
                fixed (byte* destPtr = Dest)
                {
                    for (var count = 0; count < TotalSize; count++)
                    {
                        for (var m = 0; m < TotalSize; m++) Dest[m] = 0;
                        PointerUtils.Memcpy(destPtr, sourcePtr, count);
                        for (var m = 0; m < TotalSize; m++) Assert.Equal((m < count) ? m : 0, Dest[m]);
                    }
                }
            }
        }

        [Fact]
        public void TestMemset4()
        {
            int TotalSize = 65;
            var Dest = new byte[TotalSize];

            foreach (var Set64 in new[] {false, true})
            {
                //PointerUtils.Is64 = Set64;

                fixed (byte* DestPtr = Dest)
                {
                    for (int count = 0; count < TotalSize; count++)
                    {
                        for (int m = 0; m < TotalSize; m++) Dest[m] = 0;

                        PointerUtils.Memset(DestPtr, 1, count);

                        for (int m = 0; m < TotalSize; m++)
                        {
                            Assert.Equal((m < count) ? 1 : 0, Dest[m]);
                        }
                    }
                }
            }
        }

        [Fact(Skip = "Skip")]
        public void TestMemcpyOverlapping()
        {
            var _Data = new byte[] {1, 0, 0, 0, 0, 0};
            var Expected = new byte[] {1, 1, 1, 1, 1, 1};
            fixed (byte* Data = _Data)
            {
                PointerUtils.Memcpy(&Data[1], &Data[0], 5);
            }
            Assert.Equal(Expected, _Data);
        }

        [Fact]
        public void FastHash()
        {
            var A = new byte[] {1, 2, 3};
            var B = new byte[] {1, 2, 4};
            fixed (byte* _A = A)
            fixed (byte* _B = B)
            {
                Assert.NotEqual(PointerUtils.FastHash(_A, 3), PointerUtils.FastHash(_B, 3));
            }
        }

        [Fact]
        public void FindLargestMatch0()
        {
            var A = new byte[] {0, 2, 3, 4, 5, 6, 7};
            var B = new byte[] {1, 2, 3, 4, 5, 6, 7};
            fixed (byte* _A = A)
            fixed (byte* _B = B)
            {
                Assert.Equal(0, PointerUtils.FindLargestMatch(_A, _B, 5));
            }
        }

        [Fact]
        public void FindLargestMatch1()
        {
            var A = new byte[] {1, 0, 3, 4, 5, 6, 7};
            var B = new byte[] {1, 2, 3, 4, 5, 6, 7};
            fixed (byte* _A = A)
            fixed (byte* _B = B)
            {
                Assert.Equal(1, PointerUtils.FindLargestMatch(_A, _B, 5));
            }
        }

        [Fact]
        public void FindLargestMatch5()
        {
            var A = new byte[] {1, 2, 3, 4, 5, 0, 7};
            var B = new byte[] {1, 2, 3, 4, 5, 6, 7};
            fixed (byte* _A = A)
            fixed (byte* _B = B)
            {
                Assert.Equal(5, PointerUtils.FindLargestMatch(_A, _B, A.Length));
            }
        }

        [Fact]
        public void FindLargestMatchByte1()
        {
            var A = new byte[] {1, 0, 2, 2, 2};
            fixed (byte* _A = A)
            {
                Assert.Equal(1, PointerUtils.FindLargestMatchByte(_A, (byte) 1, A.Length - 3));
            }
        }

        [Fact]
        public void FindLargestMatchByte13()
        {
            var A = new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2};
            fixed (byte* _A = A)
            {
                Assert.Equal(13, PointerUtils.FindLargestMatchByte(_A, (byte) 1, A.Length));
            }
        }
        
        [Fact]
        public void FindLargestMatchByte3()
        {
            var A = new byte[] {3, 3, 3};
            fixed (byte* _A = A)
            {
                Assert.Equal(3, PointerUtils.FindLargestMatchByte(_A, (byte) 3, A.Length));
            }
        }
    }
}