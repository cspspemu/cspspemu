using System;
using System.Runtime.InteropServices;

namespace CSharpPlatform.GL.Utils
{
    public enum TextureFormat
    {
        UNSET = 0,
        RGBA = 1,
        DEPTH = 2,
        STENCIL = 3,
        RG = 4,
        R = 5,
        RGB = 6
    }

    public unsafe class GLTexture : IDisposable
    {
        private bool CapturedAndMustDispose;
        private uint _Texture;
        public int Width { get; private set; }
        public int Height { get; private set; }
        private TextureFormat TextureFormat = TextureFormat.RGBA;
        private byte[] Data;

        public uint Texture
        {
            get { return _Texture; }
        }

        private GLTexture(uint _Texture)
        {
            this._Texture = _Texture;
            CapturedAndMustDispose = false;
            Bind();
        }

        private GLTexture()
        {
            Initialize();
        }

        public static GLTexture Create()
        {
            return new GLTexture();
        }

        public static GLTexture Wrap(uint Texture, int Width = 0, int Height = 0)
        {
            return new GLTexture(Texture) {Width = Width, Height = Height};
        }


        private void Initialize()
        {
            fixed (uint* TexturePtr = &_Texture) GL.glGenTextures(1, TexturePtr);
            CapturedAndMustDispose = true;
            Bind();
        }

        public void BindUnbind(Action Action)
        {
            var OldTexture = (uint) GL.glGetInteger(GL.GL_TEXTURE_BINDING_2D);
            try
            {
                Bind();
                Action();
            }
            finally
            {
                GL.glBindTexture(GL.GL_TEXTURE_2D, OldTexture);
            }
        }

        public GLTexture Bind()
        {
            //GL.glGetIntegerv(GL.GL_TEXTURE_BINDING_2D, 
            //GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, _Texture);
            return this;
        }

        public static void Unbind()
        {
            //GL.glDisable(GL.GL_TEXTURE_2D);
        }

        public GLTexture SetFormat(TextureFormat TextureFormat)
        {
            this.TextureFormat = TextureFormat;
            _SetTexture();
            return this;
        }

        public GLTexture SetSize(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            _SetTexture();
            return this;
        }

        public GLTexture SetData(void* Pointer)
        {
            var Size = Width * Height * 4;
            Data = new byte[Size];
            Marshal.Copy(new IntPtr(Pointer), Data, 0, Size);
            _SetTexture();
            return this;
        }

        public GLTexture SetData<T>(T[] SetData)
        {
            var SetDataHandle = GCHandle.Alloc(SetData, GCHandleType.Pinned);
            try
            {
                int Size = SetData.Length * Marshal.SizeOf(typeof(T));
                Data = new byte[Size];
                Marshal.Copy(
                    SetDataHandle.AddrOfPinnedObject(),
                    Data,
                    0,
                    Size
                );
            }
            finally
            {
                SetDataHandle.Free();
            }
            _SetTexture();
            return this;
        }

        //public GLTexture Upload()
        //{
        //	_SetTexture();
        //	return this;
        //}

        const int GL_R8 = 0x8229;
        const int GL_RED = 0x1903;

        const int GL_RG = 0x8227;
        const int GL_RG8 = 0x822B;

        private void _SetTexture()
        {
            if (TextureFormat == TextureFormat.UNSET) return;
            if (Width == 0 || Height == 0) return;

            Bind();

            //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
            //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
            //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_TEXTURE_WRAP_S);
            //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_TEXTURE_WRAP_T);

            fixed (byte* DataPtr = Data)
            {
                //Console.WriteLine("{0}:{1}: {2}x{3}: {4}", Texture, TextureFormat, Width, Height, new IntPtr(DataPtr));
                //if (this.Data != null) Console.WriteLine(String.Join(",", this.Data));
                switch (TextureFormat)
                {
                    case TextureFormat.RGBA:
                        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, 4, Width, Height, 0, GetOpenglFormat(),
                            GL.GL_UNSIGNED_BYTE, DataPtr);
                        break;
                    case TextureFormat.RGB:
                        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, 3, Width, Height, 0, GetOpenglFormat(),
                            GL.GL_UNSIGNED_BYTE, DataPtr);
                        break;
                    case TextureFormat.RG:
                        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, 0x822B /*GL.GL_RG8*/, Width, Height, 0,
                            GetOpenglFormat(), GL.GL_UNSIGNED_BYTE, DataPtr);
                        break;
                    case TextureFormat.R:
                        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL_R8, Width, Height, 0, GetOpenglFormat(),
                            GL.GL_UNSIGNED_BYTE, DataPtr);
                        break;
                    case TextureFormat.DEPTH:
                        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, Width, Height, 0,
                            GetOpenglFormat(), GL.GL_UNSIGNED_SHORT, DataPtr);
                        break;
                    //case TextureFormat.STENCIL: GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, this.Width, this.Height, 0, GL.GL_DEPTH_COMPONENT, GL.GL_UNSIGNED_SHORT, DataPtr); break;
                    default: throw (new InvalidOperationException("Unsupported " + TextureFormat));
                }
            }
        }

        private int GetOpenglFormat()
        {
            switch (TextureFormat)
            {
                case TextureFormat.RGBA: return GL.GL_RGBA;
                case TextureFormat.DEPTH: return GL.GL_DEPTH_COMPONENT;
                case TextureFormat.RGB: return GL.GL_RGB;
                case TextureFormat.RG: return GL_RG;
                case TextureFormat.R: return GL_RED;
                //case TextureFormat.STENCIL: GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, this.Width, this.Height, 0, GL.GL_DEPTH_COMPONENT, GL.GL_UNSIGNED_SHORT, DataPtr); break;
                default: throw (new InvalidOperationException("Unsupported " + TextureFormat));
            }
        }

        public void Dispose()
        {
            if (CapturedAndMustDispose)
            {
                fixed (uint* TexturePtr = &_Texture) GL.glDeleteTextures(1, TexturePtr);
            }
            _Texture = 0;
        }

        public byte[] GetDataFromCached()
        {
            return Data;
        }

        public byte[] GetDataFromGpu()
        {
            var Data = new byte[Width * Height * 4];
            fixed (byte* DataPtr = Data)
            {
                Bind();
                GL.glGetTexImage(GL.GL_TEXTURE_2D, 0, GetOpenglFormat(), GL.GL_UNSIGNED_BYTE, DataPtr);
            }
            return Data;
        }
    }
}