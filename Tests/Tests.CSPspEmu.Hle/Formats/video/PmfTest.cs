using System;
using System.IO;
using CSharpUtils;
using CSharpUtils.Endian;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats.audio.At3;
using CSPspEmu.Hle.Formats.video;
using CSPspEmu.Hle.Media.audio.At3;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats.video
{
    
    public unsafe class PmfTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var pmf = new Pmf();
            //Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
            //Pmf.Load(File.OpenRead("../../../TestInput/sample.pmf"));
            pmf.Load(File.OpenRead("c:/isos/psp/op.pmf"));
            //Console.WriteLine(Pmf.InfoHeader.ToStringDefault());

            //Assert.Equal(0x800, Pmf.Header.StreamOffset);
            //Assert.Equal(137216, Pmf.Header.StreamSize);
            //Assert.Equal(144, Pmf.InfoHeader.Width);
            //Assert.Equal(80, Pmf.InfoHeader.Height);

            var mpegPs = pmf.GetMpegPs();
            // ReSharper disable once UnusedVariable
            var pmfWidth = pmf.InfoHeader.Width;
            // ReSharper disable once UnusedVariable
            var pmfHeight = pmf.InfoHeader.Height;

            var videoOutStream = new ProduceConsumerBufferStream();
            var audioOutStream = new ProduceConsumerBufferStream();

            try
            {
                while (mpegPs.HasMorePackets)
                {
                    var packet = mpegPs.ReadPacketizedElementaryStreamHeader();
                    var info = mpegPs.ParsePacketizedStream(packet.Stream);

                    if (packet.Type == MpegPsDemuxer.ChunkType.ST_Video1)
                    {
                        info.Stream.CopyToFast(videoOutStream);
                    }
                    if (packet.Type == MpegPsDemuxer.ChunkType.ST_Private1)
                    {
                        //Console.WriteLine(Info.dts);
                        //Info
                        var channel = info.Stream.ReadByte();
                        info.Stream.Skip(3);
                        if (channel >= 0xB0 && channel <= 0xBF)
                        {
                            info.Stream.Skip(1);
                        }

                        //Info.Stream.Skip(8);
                        info.Stream.CopyToFast(audioOutStream);
                    }

                    //if (VideoOutStream.Length >= 1 * 1024 * 1024) break;
                    //Console.WriteLine(Packet.Type);
                }
            }
            catch (EndOfStreamException)
            {
            }

            var at3Data = audioOutStream.ReadAll();
            File.WriteAllBytes(@"c:\isos\psp\out\audio.raw", at3Data);
            fixed (byte* at3DataPtr = at3Data)
            {
                var at3Ptr = at3DataPtr;
                // ReSharper disable once UnusedVariable
                var at3End = &at3DataPtr[at3Data.Length];
                var maiAt3PlusFrameDecoder = new MaiAt3PlusFrameDecoder();
                var frame = 0;

                for (int n = 0; n < 1000; n++, frame++)
                {
                    var frameSize = (*(UshortBe*) &at3Ptr[2] & 0x3FF) * 8 + 8;
                    at3Ptr += 8;
                    //Console.WriteLine(FrameSize);

                    var channels = 0;
                    short[] samplesData;
                    File.WriteAllBytes(@"c:\isos\psp\out\samples" + frame + ".in",
                        new MemoryStream().WriteBytes(PointerUtils.PointerToByteArray(at3Ptr, frameSize)).ToArray());
                    Console.WriteLine("{0}, {1}, {2}", 0, channels, frameSize);
                    // ReSharper disable once UnusedVariable
                    var Out = maiAt3PlusFrameDecoder.DecodeFrame(at3Ptr, frameSize, out channels, out samplesData);
                    //Console.WriteLine("{0}, {1}, {2}", Out, channels, FrameSize);
                    File.WriteAllBytes(@"c:\isos\psp\out\samples" + frame + ".out",
                        new MemoryStream().WriteStructVector(samplesData).ToArray());
                    at3Ptr += frameSize;
                }
            }

            /*
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

            var sample5Reference = new Bitmap(Image.FromFile("../../../TestInput/sample_5.png"));

            var frameDecoder = new H264FrameDecoder(videoOutStream);
            frameNumber = 0;
            while (frameDecoder.HasMorePackets)
            {
                var frame = frameDecoder.DecodeFrame();
                //Console.WriteLine(Frame.imageWidth);
                //Console.WriteLine(Frame.imageWidthWOEdge);
                //Console.WriteLine(Frame.imageHeight);
                //Console.WriteLine(Frame.imageHeightWOEdge);
                if (frameNumber <= 70)
                {
                    if (frameNumber == 70)
                    {
                        var sample5Output = FrameUtils.imageFromFrameWithoutEdges(frame, pmfWidth, pmfHeight);
                        sample5Output.Save(@"c:\isos\psp\out\frame_" + (frameNumber) + ".png");
                        var compareResult = BitmapUtils.CompareBitmaps(sample5Reference, sample5Output);
                    }
                }
                else
                {
                    break;
                }
                frameNumber++;
            }
            */
        }
    }
}