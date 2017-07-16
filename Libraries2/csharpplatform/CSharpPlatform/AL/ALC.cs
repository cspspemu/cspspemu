using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ALCdevice = System.IntPtr;
using ALCcontext = System.IntPtr;

using ALCboolean = System.Boolean;
using ALCchar = System.Byte;

using ALCbyte = System.SByte;
using ALCubyte = System.Byte;

using ALCshort = System.Int16;
using ALCushort = System.UInt16;

using ALCint = System.Int32;
using ALCuint = System.UInt32;

using ALCsizei = System.Int32;
using ALCenum = System.Int32;

using ALCfloat = System.Single;
using ALCdouble = System.Double;
using System.Runtime.InteropServices;
using System.Security;


namespace CSharpPlatform.AL
{
	unsafe public partial class AL
	{
		public const int ALC_INVALID = 0;
		public const int ALC_VERSION_0_1 = 1;
		public const int ALC_FALSE                                = 0;
		public const int ALC_TRUE                                 = 1;
		public const int ALC_FREQUENCY                            = 0x1007;
		public const int ALC_REFRESH                              = 0x1008;
		public const int ALC_SYNC                                 = 0x1009;
		public const int ALC_MONO_SOURCES                         = 0x1010;
		public const int ALC_STEREO_SOURCES                       = 0x1011;
		public const int ALC_NO_ERROR                             = ALC_FALSE;
		public const int ALC_INVALID_DEVICE                       = 0xA001;
		public const int ALC_INVALID_CONTEXT                      = 0xA002;
		public const int ALC_INVALID_ENUM                         = 0xA003;
		public const int ALC_INVALID_VALUE                        = 0xA004;
		public const int ALC_OUT_OF_MEMORY                        = 0xA005;
		public const int ALC_DEFAULT_DEVICE_SPECIFIER             = 0x1004;
		public const int ALC_DEVICE_SPECIFIER                     = 0x1005;
		public const int ALC_EXTENSIONS                           = 0x1006;
		public const int ALC_MAJOR_VERSION                        = 0x1000;
		public const int ALC_MINOR_VERSION                        = 0x1001;
		public const int ALC_ATTRIBUTES_SIZE                      = 0x1002;
		public const int ALC_ALL_ATTRIBUTES                       = 0x1003;
		public const int ALC_CAPTURE_DEVICE_SPECIFIER             = 0x310;
		public const int ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER     = 0x311;
		public const int ALC_CAPTURE_SAMPLES                      = 0x312;

		static public readonly alcCreateContext alcCreateContext;
		static public readonly alcMakeContextCurrent alcMakeContextCurrent;
		static public readonly alcProcessContext alcProcessContext;
		static public readonly alcSuspendContext alcSuspendContext;
		static public readonly alcDestroyContext alcDestroyContext;
		static public readonly alcGetCurrentContext alcGetCurrentContext;
		static public readonly alcGetContextsDevice alcGetContextsDevice;
		static public readonly alcOpenDevice alcOpenDevice;
		static public readonly alcCloseDevice alcCloseDevice;
		static public readonly alcGetError alcGetError;
		static public readonly alcIsExtensionPresent alcIsExtensionPresent;
		static public readonly alcGetProcAddress alcGetProcAddress;
		static public readonly alcGetEnumValue alcGetEnumValue;
		static public readonly alcGetString alcGetString;
		static public readonly alcGetIntegerv alcGetIntegerv;
		static public readonly alcCaptureOpenDevice alcCaptureOpenDevice;
		static public readonly alcCaptureCloseDevice alcCaptureCloseDevice;
		static public readonly alcCaptureStart alcCaptureStart;
		static public readonly alcCaptureStop alcCaptureStop;
		static public readonly alcCaptureSamples alcCaptureSamples;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCcontext *    alcCreateContext( ALCdevice *device, ALCint* attrlist );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCboolean      alcMakeContextCurrent( ALCcontext *context );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcProcessContext( ALCcontext *context );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcSuspendContext( ALCcontext *context );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcDestroyContext( ALCcontext *context );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCcontext *    alcGetCurrentContext( );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCdevice*      alcGetContextsDevice( ALCcontext *context );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCdevice *     alcOpenDevice( ALCchar *devicename );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCboolean      alcCloseDevice( ALCdevice *device );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCenum         alcGetError( ALCdevice *device );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCboolean      alcIsExtensionPresent( ALCdevice *device, ALCchar *extname );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void  *         alcGetProcAddress( ALCdevice *device, ALCchar *funcname );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCenum         alcGetEnumValue( ALCdevice *device, ALCchar *enumname );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCchar *       alcGetString( ALCdevice *device, ALCenum param );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcGetIntegerv( ALCdevice *device, ALCenum param, ALCsizei size, ALCint *data );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCdevice*      alcCaptureOpenDevice( ALCchar *devicename, ALCuint frequency, ALCenum format, ALCsizei buffersize );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate ALCboolean      alcCaptureCloseDevice( ALCdevice *device );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcCaptureStart( ALCdevice *device );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcCaptureStop( ALCdevice *device );
	[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate void            alcCaptureSamples( ALCdevice *device, void *buffer, ALCsizei samples );
}
