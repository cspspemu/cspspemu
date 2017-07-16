using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System.Runtime.ExceptionServices;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public sealed unsafe partial class OpenglGpuImpl
    {
        private void TransferToFrameBuffer(GpuStateStruct* GpuState)
        {
            Console.WriteLine("TransferToFrameBuffer Not Implemented");
            //var TextureTransferState = GpuState->TextureTransferState;
            //
            //var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
            //
            //GL.PixelZoom(1, -1);
            //GL.WindowPos2(TextureTransferState.DestinationX, 272 - TextureTransferState.DestinationY);
            ////GL.PixelZoom(1, -1);
            ////GL.PixelZoom(1, 1);
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, TextureTransferState.BytesPerPixel);
            //GL.PixelStore(PixelStoreParameter.UnpackRowLength, TextureTransferState.SourceLineWidth);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, TextureTransferState.SourceX);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipRows, TextureTransferState.SourceY);
            //
            //{
            //	GL.DrawPixels(
            //		TextureTransferState.Width,
            //		TextureTransferState.Height,
            //		PixelFormat.Rgba,
            //		GlPixelFormat.OpenglPixelType,
            //		new IntPtr(Memory.PspAddressToPointerSafe(
            //			TextureTransferState.SourceAddress,
            //			TextureTransferState.Width * TextureTransferState.Height * 4
            //		))
            //	);
            //}
            //
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            //GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
        }

        private void TransferGeneric(GpuStateStruct* GpuState)
        {
            Console.WriteLine("TransferGeneric Not Implemented");
            var TextureTransferState = GpuState->TextureTransferState;

            var SourceX = TextureTransferState.SourceX;
            var SourceY = TextureTransferState.SourceY;
            var DestinationX = TextureTransferState.DestinationX;
            var DestinationY = TextureTransferState.DestinationY;
            var BytesPerPixel = TextureTransferState.BytesPerPixel;

            var SourceTotalBytes = TextureTransferState.SourceLineWidth * TextureTransferState.Height * BytesPerPixel;
            var DestinationTotalBytes =
                TextureTransferState.DestinationLineWidth * TextureTransferState.Height * BytesPerPixel;

            var SourcePointer =
                (byte*) Memory.PspAddressToPointerSafe(TextureTransferState.SourceAddress.Address, SourceTotalBytes);
            var DestinationPointer =
                (byte*) Memory.PspAddressToPointerSafe(TextureTransferState.DestinationAddress.Address,
                    DestinationTotalBytes);

            for (uint y = 0; y < TextureTransferState.Height; y++)
            {
                var RowSourceOffset = (uint) (
                    (TextureTransferState.SourceLineWidth * (y + SourceY)) + SourceX
                );
                var RowDestinationOffset = (uint) (
                    (TextureTransferState.DestinationLineWidth * (y + DestinationY)) + DestinationX
                );
                PointerUtils.Memcpy(
                    DestinationPointer + RowDestinationOffset * BytesPerPixel,
                    SourcePointer + RowSourceOffset * BytesPerPixel,
                    TextureTransferState.Width * BytesPerPixel
                );
            }

            /*
            // Generic implementation.
            with (gpu.state.textureTransfer) {
                auto srcAddressHost = cast(ubyte*)gpu.memory.getPointer(srcAddress);
                auto dstAddressHost = cast(ubyte*)gpu.memory.getPointer(dstAddress);

                if (gpu.state.drawBuffer.isAnyAddressInBuffer([srcAddress, dstAddress])) {
                    gpu.performBufferOp(BufferOperation.STORE, BufferType.COLOR);
                }

                for (int n = 0; n < height; n++) {
                    int srcOffset = ((n + srcY) * srcLineWidth + srcX) * bpp;
                    int dstOffset = ((n + dstY) * dstLineWidth + dstX) * bpp;
                    (dstAddressHost + dstOffset)[0.. width * bpp] = (srcAddressHost + srcOffset)[0.. width * bpp];
                    //writefln("%08X <- %08X :: [%d]", dstOffset, srcOffset, width * bpp);
                }
                //std.file.write("buffer", dstAddressHost[0..512 * 272 * 4]);
            
                if (gpu.state.drawBuffer.isAnyAddressInBuffer([dstAddress])) {
                    //gpu.impl.test();
                    //gpu.impl.test("trxkick");
                    gpu.markBufferOp(BufferOperation.LOAD, BufferType.COLOR);
                }
                //gpu.impl.test();
            }
            */
        }

        public override void Transfer(GpuStateStruct* GpuState)
        {
            Console.WriteLine("Transfer Not Implemented");
            //return;
            var TextureTransferState = GpuState->TextureTransferState;

            if (
                (TextureTransferState.DestinationAddress.Address == GpuState->DrawBufferState.Address) &&
                (TextureTransferState.DestinationLineWidth == GpuState->DrawBufferState.Width) &&
                (TextureTransferState.BytesPerPixel == GpuState->DrawBufferState.BytesPerPixel)
            )
            {
                //Console.Error.WriteLine("Writting to DrawBuffer");
                TransferToFrameBuffer(GpuState);
            }
            else
            {
                Console.Error.WriteLine("NOT Writting to DrawBuffer");
                TransferGeneric(GpuState);
                /*
                base.Transfer(GpuStateStruct);
                PrepareWrite(GpuStateStruct);
                {

                }
                PrepareRead(GpuStateStruct);
                */
            }
            Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}",
                GpuState->TextureTransferState.ToStringDefault());
        }

        /*
        readonly byte[] TempBuffer = new byte[512 * 512 * 4];

        struct GlPixelFormat {
            PixelFormats pspFormat;
            float size;
            uint  internal;
            uint  external;
            uint  opengl;
            uint  isize() { return cast(uint)size; }
        }

        static const auto GlPixelFormats = [
            GlPixelFormat(PixelFormats.GU_PSM_5650,   2, 3, GL_RGB,  GL_UNSIGNED_SHORT_5_6_5_REV),
            GlPixelFormat(PixelFormats.GU_PSM_5551,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_1_5_5_5_REV),
            GlPixelFormat(PixelFormats.GU_PSM_4444,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4_REV),
            GlPixelFormat(PixelFormats.GU_PSM_8888,   4, 4, GL_RGBA, GL_UNSIGNED_INT_8_8_8_8_REV),
            GlPixelFormat(PixelFormats.GU_PSM_T4  , 0.5, 1, GL_COLOR_INDEX, GL_COLOR_INDEX4_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T8  ,   1, 1, GL_COLOR_INDEX, GL_COLOR_INDEX8_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T16 ,   2, 4, GL_COLOR_INDEX, GL_COLOR_INDEX16_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T32 ,   4, 4, GL_RGBA, GL_UNSIGNED_INT ), // COLOR_INDEX, GL_COLOR_INDEX32_EXT Not defined.
            GlPixelFormat(PixelFormats.GU_PSM_DXT1,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT1_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_DXT3,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT3_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_DXT5,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT5_EXT),
        ];
        */

        //[HandleProcessCorruptedStateExceptions]
        //private void PrepareRead(GpuStateStruct* GpuState)
        //{
        //	if (true)
        //	{
        //		var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
        //		int Width = (int)GpuState->DrawBufferState.Width;
        //		if (Width == 0) Width = 512;
        //		int Height = 272;
        //		int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //		int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //		//GpuState->DrawBufferState.Format
        //		var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address, 0);
        //		GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //		//Console.WriteLine("PrepareRead: {0:X}", Address);
        //
        //		try
        //		{
        //			GL.WindowPos2(0, 272);
        //			GL.PixelZoom(1, -1);
        //
        //			GL.DrawPixels(Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
        //			//GL.DrawPixels(512, 272, PixelFormat.AbgrExt, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
        //
        //			//GL.WindowPos2(0, 0);
        //			//GL.PixelZoom(1, 1);
        //		}
        //		catch (Exception Exception)
        //		{
        //			Console.WriteLine(Exception);
        //		}
        //	}
        //}

        //int[] pboIds = { -1 };
        //
        //static bool UsePbo = false;
        //
        //private void PreParePbos()
        //{
        //	if (UsePbo)
        //	{
        //		if (pboIds[0] == -1)
        //		{
        //			GL.GenBuffers(1, pboIds);
        //			GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pboIds[0]);
        //			GL.BufferData(BufferTarget.PixelUnpackBuffer, new IntPtr(512 * 272 * 4), IntPtr.Zero, BufferUsageHint.StreamRead);
        //			GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        //		}
        //		GL.BindBuffer(BufferTarget.PixelPackBuffer, pboIds[0]);
        //	}
        //}
        //
        //private void UnPreParePbos()
        //{
        //	if (UsePbo)
        //	{
        //		GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
        //	}
        //}

        //private void SaveFrameBuffer(GpuStateStruct* GpuState, string FileName)
        //{
        //	var GlPixelFormat = GlPixelFormatList[(int)GuPixelFormats.RGBA_8888];
        //	int Width = (int)GpuState->DrawBufferState.Width;
        //	if (Width == 0) Width = 512;
        //	int Height = 272;
        //	int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //	int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //
        //	if (Width == 0) Width = 512;
        //
        //	GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //
        //	var FB = new Bitmap(Width, Height);
        //	var Data = new byte[Width * Height * 4];
        //
        //	fixed (byte* DataPtr = Data)
        //	{
        //		//glBindBufferARB(GL_PIXEL_PACK_BUFFER_ARB, pboIds[index]);
        //		GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(DataPtr));
        //
        //		BitmapUtils.TransferChannelsDataInterleaved(
        //			FB.GetFullRectangle(),
        //			FB,
        //			DataPtr,
        //			BitmapUtils.Direction.FromDataToBitmap,
        //			BitmapChannel.Red,
        //			BitmapChannel.Green,
        //			BitmapChannel.Blue,
        //			BitmapChannel.Alpha
        //		);
        //	}
        //
        //	FB.Save(FileName);
        //}

        //[HandleProcessCorruptedStateExceptions]
        //private void PrepareWrite(GpuStateStruct* GpuState)
        //{
        //	//GL.Flush();
        //	//return;
        //
        //#if true
        //	//if (SwapBuffers)
        //	//{
        //	//	RenderGraphicsContext.SwapBuffers();
        //	//}
        //	//
        //	//GL.PushAttrib(AttribMask.EnableBit);
        //	//GL.PushAttrib(AttribMask.TextureBit);
        //	//{
        //	//	GL.Enable(EnableCap.Texture2D);
        //	//	GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexture);
        //	//	{
        //	//		//GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, 512, 272);
        //	//		GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, 512, 272, 0);
        //	//		//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new uint[] { 0xFFFF00FF });
        //	//	}
        //	//	GL.BindTexture(TextureTarget.Texture2D, 0);
        //	//}
        //	//GL.PopAttrib();
        //	//GL.PopAttrib();
        //#else
        //
        //	//Console.WriteLine("PrepareWrite");
        //	try
        //	{
        //		var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
        //		int Width = (int)GpuState->DrawBufferState.Width;
        //		if (Width == 0) Width = 512;
        //		int Height = 272;
        //		int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //		int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //		//GpuState->DrawBufferState.Format
        //		var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);
        //
        //		//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);
        //
        //		//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);
        //		GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //
        //		fixed (void* _TempBufferPtr = &TempBuffer[0])
        //		{
        //			var Input = (byte*)_TempBufferPtr;
        //			var Output = (byte*)Address;
        //
        //			PreParePbos();
        //			if (this.pboIds[0] > 0)
        //			{
        //				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, IntPtr.Zero);
        //				Input = (byte*)GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly).ToPointer();
        //				GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
        //				if (Input == null)
        //				{
        //					Console.WriteLine("PBO ERROR!");
        //				}
        //			}
        //			else
        //			{
        //				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(_TempBufferPtr));
        //			}
        //			UnPreParePbos();
        //
        //			for (int Row = 0; Row < Height; Row++)
        //			{
        //				var ScanIn = (byte*)&Input[ScanWidth * Row];
        //				var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
        //				//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
        //				PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
        //			}
        //		}
        //	}
        //	catch (Exception Exception)
        //	{
        //		Console.WriteLine(Exception);
        //	}
        //
        //	if (SwapBuffers)
        //	{
        //		RenderGraphicsContext.SwapBuffers();
        //	}
        // #endif
        //}
    }
}