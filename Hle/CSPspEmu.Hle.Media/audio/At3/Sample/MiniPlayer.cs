using System;
using System.IO;
using CSharpUtils.Endian;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats.audio.At3;

namespace CSPspEmu.Hle.Media.audio.At3.Sample
{
    public unsafe class MiniPlayer
    {
        public static int Play(Stream stream, Stream outStream)
        {
            var strt = stream.ReadString(3);
            stream.Position = 0;

            ushort bztmp;
            if (strt == "ea3")
            {
                //we get ea3 header
                Console.WriteLine("ea3 header\n");
                stream.Position = 0x6;
                var tmp = stream.ReadBytes(4);
                var skipbytes = 0;
                for (var a0 = 0; a0 < 4; a0++)
                {
                    skipbytes <<= 7;
                    skipbytes += tmp[a0] & 0x7F;
                }
                stream.Skip(skipbytes);
            }

            if (strt == "RIF") //RIFF
            {
                //we get RIFF header
                Console.WriteLine("RIFF header\n");
                stream.Position = 0x10;
                var fmtSize = stream.ReadStruct<int>();
                var fmt = stream.ReadStruct<ushort>();
                if (fmt != 0xFFFE)
                {
                    Console.WriteLine("RIFF File fmt error\n");
                    return -1;
                }
                stream.Skip(0x28);
                bztmp = stream.ReadStruct<UshortBe>();
                stream.Skip(fmtSize - 0x2c);

                //search the data chunk
                for (var a0 = 0; a0 < 0x100; a0++)
                {
                    if (stream.ReadString(4) == "atad") break;
                }
                // ReSharper disable once UnusedVariable
                var tmpr = stream.ReadStruct<int>();
            }
            else
            {
                //EA3 block that contains at3+ stream
                Console.WriteLine("EA3 header");
                stream.Skip(0x22);

                Console.WriteLine("{0:X}", stream.Position);
                bztmp = stream.ReadStruct<UshortBe>();
                stream.Skip(0x3c);
            }
            var blocksz = bztmp & 0x3FF;

            var buf0 = new byte[0x3000];
            var chns = 0;
            fixed (byte* buf0Ptr = buf0)
            {
                //calculate the frame block size here
                var blockSize = blocksz * 8 + 8;

                Console.WriteLine("frame_block_size 0x{0:X}\n", blockSize);

                //Console.ReadKey();

                //so we make a new at3+ frame decoder
                var d2 = new MaiAt3PlusFrameDecoder();

                stream.Read(buf0, 0, blockSize);
                short[] pBuf;
                int rs;
                //decode the first frame and get channel num
                //for (int n = 0; n < block_size; n++) Console.Write(buf0[n]);
                if ((rs = d2.DecodeFrame(buf0Ptr, blockSize, out chns, out pBuf)) != 0)
                {
                    Console.WriteLine("decode error {0}", rs);
                }
                Console.WriteLine("channels: {0}\n", chns);
                if (chns > 2) Console.WriteLine("warning: waveout doesn't support {0} chns\n", chns);

                //just waveout
                //MaiWaveOutI *mwo0 = new MaiWaveOutI(chns, 44100, 16);

                //mwo0.play();
                while (!stream.Eof())
                {
                    stream.Read(buf0, 0, blockSize);

                    //decode frame and get sample data
                    if ((rs = d2.DecodeFrame(buf0Ptr, blockSize, out chns, out pBuf)) != 0)
                        Console.WriteLine("decode error {0}", rs);
                    //play it

                    outStream.WriteStructVector(pBuf, 0x800 * chns);

                    //mwo0.enqueue((Mai_I8*)p_buf, 0x800 * chns * 2);
                }

                //while (mwo0.getRemainBufSize()) Mai_Sleep(1);
                return 0;
            }
        }
    }
}