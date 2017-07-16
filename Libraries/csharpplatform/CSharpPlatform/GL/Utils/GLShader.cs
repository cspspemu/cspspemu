using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Extensions;

namespace CSharpPlatform.GL.Utils
{
    public enum GLValueType
    {
        GL_BYTE = 0x1400,
        GL_UNSIGNED_BYTE = 0x1401,
        GL_SHORT = 0x1402,
        GL_UNSIGNED_SHORT = 0x1403,
        GL_INT = 0x1404,
        GL_UNSIGNED_INT = 0x1405,
        GL_FLOAT = 0x1406,
        GL_FIXED = 0x140C,

        GL_FLOAT_VEC2 = 0x8B50,
        GL_FLOAT_VEC3 = 0x8B51,
        GL_FLOAT_VEC4 = 0x8B52,
        GL_INT_VEC2 = 0x8B53,
        GL_INT_VEC3 = 0x8B54,
        GL_INT_VEC4 = 0x8B55,
        GL_BOOL = 0x8B56,
        GL_BOOL_VEC2 = 0x8B57,
        GL_BOOL_VEC3 = 0x8B58,
        GL_BOOL_VEC4 = 0x8B59,
        GL_FLOAT_MAT2 = 0x8B5A,
        GL_FLOAT_MAT3 = 0x8B5B,
        GL_FLOAT_MAT4 = 0x8B5C,
        GL_SAMPLER_2D = 0x8B5E,
        GL_SAMPLER_CUBE = 0x8B60
    }

    public enum GLGeometry
    {
        GL_POINTS = 0x0000,
        GL_LINES = 0x0001,
        GL_LINE_LOOP = 0x0002,
        GL_LINE_STRIP = 0x0003,
        GL_TRIANGLES = 0x0004,
        GL_TRIANGLE_STRIP = 0x0005,
        GL_TRIANGLE_FAN = 0x0006
    }

    public unsafe class GLShader : IDisposable
    {
        uint Program;
        uint VertexShader;
        uint FragmentShader;

        [DebuggerHidden]
        public GLShader(string VertexShaderSource, string FragmentShaderSource)
        {
            Initialize();

            int VertexShaderCompileStatus;
            ShaderSource(VertexShader, VertexShaderSource);
            GL.glCompileShader(VertexShader);
            GL.CheckError();
            GL.glGetShaderiv(VertexShader, GL.GL_COMPILE_STATUS, &VertexShaderCompileStatus);
            var VertexShaderInfo = GetShaderInfoLog(VertexShader);

            int FragmentShaderCompileStatus;
            ShaderSource(FragmentShader, FragmentShaderSource);
            GL.glCompileShader(FragmentShader);
            GL.CheckError();
            GL.glGetShaderiv(FragmentShader, GL.GL_COMPILE_STATUS, &FragmentShaderCompileStatus);
            var FragmentShaderInfo = GetShaderInfoLog(FragmentShader);

            if (!String.IsNullOrEmpty(VertexShaderInfo))
                Console.Out.WriteLineColored(ConsoleColor.Blue, "{0}", VertexShaderInfo);
            if (!String.IsNullOrEmpty(FragmentShaderInfo))
                Console.Out.WriteLineColored(ConsoleColor.Blue, "{0}", FragmentShaderInfo);

            if (VertexShaderCompileStatus == 0 || FragmentShaderCompileStatus == 0)
            {
                //throw (new Exception(String.Format("Shader ERROR (I): {0}, {1}", VertexShaderInfo, FragmentShaderInfo)));
                Console.Error.WriteLineColored(ConsoleColor.Red, "Shader ERROR (I): {0}, {1}", VertexShaderInfo,
                    FragmentShaderInfo);
            }

            Console.Out.WriteLineColored(
                ConsoleColor.Blue,
                "Compiled Shader! : {0}, {1}",
                VertexShaderSource, FragmentShaderSource
            );

            GL.glAttachShader(Program, VertexShader);
            GL.glAttachShader(Program, FragmentShader);
            GL.glDeleteShader(VertexShader);
            GL.glDeleteShader(FragmentShader);

            Link();
        }

        public GlAttribute GetAttribute(string Name)
        {
            return _Attributes.GetOrDefault(Name, new GlAttribute(this, Name, -1, 0, GLValueType.GL_BYTE));
        }

        public GlUniform GetUniform(string Name)
        {
            if (_Uniforms.ContainsKey(Name + "[0]")) Name = Name + "[0]";
            return _Uniforms.GetOrDefault(Name, new GlUniform(this, Name, -1, 0, GLValueType.GL_BYTE));
        }

        private void Link()
        {
            int LinkStatus;
            GL.glLinkProgram(Program);
            GL.glGetProgramiv(Program, GL.GL_LINK_STATUS, &LinkStatus);
            var ProgramInfo = GetProgramInfoLog(Program);

            if (LinkStatus == 0)
            {
                Console.Error.WriteLineColored(ConsoleColor.Red, "Shader ERROR (II): {0}", ProgramInfo);
                //throw (new Exception(String.Format("Shader ERROR: {0}", ProgramInfo)));
            }

            GetAllUniforms();
            GetAllAttributes();
        }

        private readonly Dictionary<string, GlUniform> _Uniforms = new Dictionary<string, GlUniform>();
        private readonly Dictionary<string, GlAttribute> _Attributes = new Dictionary<string, GlAttribute>();

        public IEnumerable<GlUniform> Uniforms
        {
            get { return _Uniforms.Values; }
        }

        public IEnumerable<GlAttribute> Attributes
        {
            get { return _Attributes.Values; }
        }

        private void GetAllUniforms()
        {
            const int NameMaxSize = 1024;
            var NameTemp = stackalloc byte[NameMaxSize];
            int Total = -1;
            GL.glGetProgramiv(Program, GL.GL_ACTIVE_UNIFORMS, &Total);
            for (uint n = 0; n < Total; n++)
            {
                int name_len = -1, num = -1;
                int type = GL.GL_ZERO;
                GL.glGetActiveUniform(Program, n, NameMaxSize - 1, &name_len, &num, &type, NameTemp);
                NameTemp[name_len] = 0;
                var Name = Marshal.PtrToStringAnsi(new IntPtr(NameTemp));
                int location = GL.glGetUniformLocation(Program, Name);
                _Uniforms[Name] = new GlUniform(this, Name, location, num, (GLValueType) type);
                //Console.WriteLine(Uniforms[Name]);
            }
        }

        private void GetAllAttributes()
        {
            const int NameMaxSize = 1024;
            var NameTemp = stackalloc byte[NameMaxSize];
            int Total = -1;
            GL.glGetProgramiv(Program, GL.GL_ACTIVE_ATTRIBUTES, &Total);
            for (uint n = 0; n < Total; n++)
            {
                int name_len = -1, num = -1;
                int type = GL.GL_ZERO;
                GL.glGetActiveAttrib(Program, n, NameMaxSize - 1, &name_len, &num, &type, NameTemp);
                NameTemp[name_len] = 0;
                var Name = Marshal.PtrToStringAnsi(new IntPtr(NameTemp));
                int location = GL.glGetAttribLocation(Program, Name);
                _Attributes[Name] = new GlAttribute(this, Name, location, num, (GLValueType) type);
                //Console.WriteLine(Attributes[Name]);
            }
        }

        public bool IsUsing
        {
            get
            {
                int CurrentProgram;
                GL.glGetIntegerv(GL.GL_CURRENT_PROGRAM, &CurrentProgram);
                return CurrentProgram == Program;
            }
        }

        public void Use()
        {
            GL.glUseProgram(Program);
        }

        private void Initialize()
        {
            GL.ClearError();
            Program = GL.glCreateProgram();
            GL.CheckError();
            VertexShader = GL.glCreateShader(GL.GL_VERTEX_SHADER);
            GL.CheckError();
            FragmentShader = GL.glCreateShader(GL.GL_FRAGMENT_SHADER);
            GL.CheckError();
        }

        private static void ShaderSource(uint Shader, string Source)
        {
            var SourceBytes = new UTF8Encoding(false, true).GetBytes(Source);
            var SourceLength = SourceBytes.Length;
            fixed (byte* _SourceBytesPtr = SourceBytes)
            {
                byte* SourceBytesPtr = _SourceBytesPtr;
                GL.glShaderSource(Shader, 1, &SourceBytesPtr, &SourceLength);
            }
        }

        private static string GetShaderInfoLog(uint Shader)
        {
            int Length;
            var Data = new byte[1024];
            fixed (byte* DataPtr = Data)
            {
                GL.glGetShaderInfoLog(Shader, Data.Length, &Length, DataPtr);
                return Marshal.PtrToStringAnsi(new IntPtr(DataPtr), Length);
            }
        }

        private static string GetProgramInfoLog(uint Program)
        {
            int Length;
            var Data = new byte[1024];
            fixed (byte* DataPtr = Data)
            {
                GL.glGetProgramInfoLog(Program, Data.Length, &Length, DataPtr);
                return Marshal.PtrToStringAnsi(new IntPtr(DataPtr), Length);
            }
        }

        public void Dispose()
        {
            GL.glDeleteProgram(Program);
            Program = 0;
            VertexShader = 0;
            FragmentShader = 0;
        }

        public void Draw(GLGeometry Geometry, int Count, Action SetDataCallback, int Offset = 0)
        {
            Use();
            SetDataCallback();
            GL.glDrawArrays((int) Geometry, Offset, Count);
        }

        public void Draw(GLGeometry Geometry, uint[] Indices, int Count, Action SetDataCallback, int IndicesOffset = 0)
        {
            Use();
            SetDataCallback();
            fixed (uint* IndicesPtr = &Indices[IndicesOffset])
            {
                GL.glDrawElements((int) Geometry, Count, GL.GL_UNSIGNED_INT, IndicesPtr);
            }
        }

        public void BindUniformsAndAttributes(object Object)
        {
            foreach (var Field in Object.GetType().GetFields())
            {
                if (Field.FieldType == typeof(GlAttribute))
                {
                    Field.SetValue(Object, GetAttribute(Field.Name));
                }
                else if (Field.FieldType == typeof(GlUniform))
                {
                    Field.SetValue(Object, GetUniform(Field.Name));
                }
            }
        }
    }
}