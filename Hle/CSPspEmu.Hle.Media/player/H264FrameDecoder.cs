using cscodec.av;
using cscodec.h264.decoder;
using System;
using System.IO;

namespace cscodec.h264.player
{
    public sealed unsafe class H264FrameDecoder : FrameDecoder<AVFrame>
    {
        H264Decoder Codec;
        MpegEncContext Context = null;
        int[] got_picture = new int[1];

        AVFrame picture;
        //private int[] buffer = null;

        public H264FrameDecoder(Stream Stream) : base(Stream)
        {
        }

        protected override void InitProtected()
        {
            Codec = new H264Decoder();
            if (Codec == null)
            {
                throw (new Exception("codec not found"));
            }

            Context = MpegEncContext.avcodec_alloc_context();
            picture = AVFrame.avcodec_alloc_frame();

            // We do not send complete frames
            if ((Codec.capabilities & H264Decoder.CODEC_CAP_TRUNCATED) != 0)
            {
                Context.flags |= MpegEncContext.CODEC_FLAG_TRUNCATED;
            }

            // For some codecs, such as msmpeg4 and mpeg4, width and height
            // MUST be initialized there because this information is not
            // available in the bitstream.

            // Open it
            if (Context.avcodec_open(Codec) < 0)
            {
                throw (new Exception("could not open codec"));
            }
        }

        protected override void Close()
        {
            Context.avcodec_close();
            Context = null;
            picture = null;
        }

        protected override AVFrame DecodeFrameFromPacket(AVPacket avpkt, out int len)
        {
            len = Context.avcodec_decode_video2(picture, got_picture, avpkt);
            //Console.WriteLine(FrameCrc.GetFrameLine(avpkt));
            if (len < 0)
            {
                //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Error while decoding frame: {0}, error: {1}", frame, this.len); });
                // Discard current packet and proceed to next packet
                return null;
            }

            if (got_picture[0] != 0)
            {
                picture = Context.priv_data.displayPicture;

                //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () => { Console.WriteLine("Generated picture"); });
                //int bufferSize = picture.imageWidth * picture.imageHeight;
                //if (buffer == null || bufferSize != buffer.Length)
                //{
                //	buffer = new int[bufferSize];
                //}
                return picture;
            }
            else
            {
                //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.DarkMagenta, () => { Console.WriteLine("NOT Generated picture"); });
                return null;
            }
        }
    }
}