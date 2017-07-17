using cscodec;
using cscodec.h264.player;
using CSharpUtils;
using CSharpUtils.Endian;
using CSPspEmu.Hle.Formats.audio.At3;
using CSPspEmu.Hle.Formats.video;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Tests
{
    [TestFixture]
    public unsafe class PmfTest
    {
        [Test]
        public void LoadTest()
        {
            var Pmf = new Pmf();
            //Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
            //Pmf.Load(File.OpenRead("../../../TestInput/sample.pmf"));
            Pmf.Load(File.OpenRead("c:/isos/psp/op.pmf"));
            //Console.WriteLine(Pmf.InfoHeader.ToStringDefault());

            //Assert.AreEqual(0x800, Pmf.Header.StreamOffset);
            //Assert.AreEqual(137216, Pmf.Header.StreamSize);
            //Assert.AreEqual(144, Pmf.InfoHeader.Width);
            //Assert.AreEqual(80, Pmf.InfoHeader.Height);

            var MpegPs = Pmf.GetMpegPs();
            int PmfWidth = Pmf.InfoHeader.Width;
            int PmfHeight = Pmf.InfoHeader.Height;

            var VideoOutStream = new ProduceConsumerBufferStream();
            var AudioOutStream = new ProduceConsumerBufferStream();
            int FrameNumber = 0;

            try
            {
                while (MpegPs.HasMorePackets)
                {
                    var Packet = MpegPs.ReadPacketizedElementaryStreamHeader();
                    var Info = MpegPs.ParsePacketizedStream(Packet.Stream);

                    if (Packet.Type == MpegPsDemuxer.ChunkType.ST_Video1)
                    {
                        Info.Stream.CopyToFast(VideoOutStream);
                    }
                    if (Packet.Type == MpegPsDemuxer.ChunkType.ST_Private1)
                    {
                        //Console.WriteLine(Info.dts);
                        //Info
                        var Channel = Info.Stream.ReadByte();
                        Info.Stream.Skip(3);
                        if (Channel >= 0xB0 && Channel <= 0xBF)
                        {
                            Info.Stream.Skip(1);
                        }

                        //Info.Stream.Skip(8);
                        Info.Stream.CopyToFast(AudioOutStream);
                    }

                    //if (VideoOutStream.Length >= 1 * 1024 * 1024) break;
                    //Console.WriteLine(Packet.Type);
                }
            }
            catch (EndOfStreamException)
            {
            }

            var At3Data = AudioOutStream.ReadAll();
            File.WriteAllBytes(@"c:\isos\psp\out\audio.raw", At3Data);
            fixed (byte* At3DataPtr = At3Data)
            {
                byte* At3Ptr = At3DataPtr;
                byte* At3End = &At3DataPtr[At3Data.Length];
                var MaiAT3PlusFrameDecoder = new MaiAT3PlusFrameDecoder();
                int frame = 0;

                for (int n = 0; n < 1000; n++, frame++)
                {
                    var FrameSize = (*(UshortBe*) &At3Ptr[2] & 0x3FF) * 8 + 8;
                    At3Ptr += 8;
                    //Console.WriteLine(FrameSize);

                    int channels = 0;
                    short[] SamplesData;
                    File.WriteAllBytes(@"c:\isos\psp\out\samples" + frame + ".in",
                        new MemoryStream().WriteBytes(PointerUtils.PointerToByteArray(At3Ptr, FrameSize)).ToArray());
                    Console.WriteLine("{0}, {1}, {2}", 0, channels, FrameSize);
                    int Out = MaiAT3PlusFrameDecoder.decodeFrame(At3Ptr, FrameSize, out channels, out SamplesData);
                    //Console.WriteLine("{0}, {1}, {2}", Out, channels, FrameSize);
                    File.WriteAllBytes(@"c:\isos\psp\out\samples" + frame + ".out",
                        new MemoryStream().WriteStructVector(SamplesData).ToArray());
                    At3Ptr += FrameSize;
                }
            }

            return;

            //FileUtils.CreateAndAppendStream(@"c:/isos/psp/out/opening.h264", VideoOutStream.Slice());
            //FileUtils.CreateAndAppendStream(@"c:/isos/psp/out/opening.at3", AudioOutStream.Slice());

            //return;
            //
            //VideoOutStream.Slice().CopyToFile(@"c:/isos/psp/opening.h264");
            //AudioOutStream.Slice().CopyToFile(@"c:/isos/psp/opening.at3");
            //

            //var AudioDecoder = new At3pFrameDecoder(AudioOutStream.Slice());
            //FrameNumber = 0;
            //while (AudioDecoder.HasMorePackets)
            //{
            //	var AudioFrame = AudioDecoder.DecodeFrame();
            //	var Stream = new MemoryStream();
            //	Stream.WriteStructVector(AudioFrame);
            //	File.WriteAllBytes(@"c:\isos\psp\samples" + (FrameNumber++) + ".raw", Stream.ToArray());
            //	//Console.WriteLine(AudioFrame);
            //}

            //var Data = AudioOutStream2.ReadBytes((int)2048);
            //int BlockSize = Data.Length;
            //short[] buf;
            //int channels;
            //fixed (byte* DataPtr = Data)
            //{
            //	int Decoded = MaiAT3PlusFrameDecoder.decodeFrame(DataPtr, BlockSize, out channels, out buf);
            //	Console.WriteLine("[1] : {0} : {1} : {2}", channels, BlockSize, Decoded);
            //}


            //return;

            var Sample5Reference = new Bitmap(Image.FromFile("../../../TestInput/sample_5.png"));

            var FrameDecoder = new H264FrameDecoder(VideoOutStream);
            FrameNumber = 0;
            while (FrameDecoder.HasMorePackets)
            {
                var Frame = FrameDecoder.DecodeFrame();
                //Console.WriteLine(Frame.imageWidth);
                //Console.WriteLine(Frame.imageWidthWOEdge);
                //Console.WriteLine(Frame.imageHeight);
                //Console.WriteLine(Frame.imageHeightWOEdge);
                if (FrameNumber <= 70)
                {
                    if (FrameNumber == 70)
                    {
                        var Sample5Output = FrameUtils.imageFromFrameWithoutEdges(Frame, PmfWidth, PmfHeight);
                        Sample5Output.Save(@"c:\isos\psp\out\frame_" + (FrameNumber) + ".png");
                        var CompareResult = BitmapUtils.CompareBitmaps(Sample5Reference, Sample5Output);
                    }
                }
                else
                {
                    break;
                }
                FrameNumber++;
            }
        }
    }
}