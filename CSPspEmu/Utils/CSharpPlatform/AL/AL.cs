using System.Runtime.InteropServices;
using System.Security;
using ALboolean = System.Boolean;
using ALchar = System.Byte;
using ALbyte = System.SByte;
using ALubyte = System.Byte;
using ALshort = System.Int16;
using ALushort = System.UInt16;
using ALint = System.Int32;
using ALuint = System.UInt32;
using ALsizei = System.Int32;
using ALenum = System.Int32;
using ALfloat = System.Single;
using ALdouble = System.Double;
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
using CSharpPlatform.Library;

namespace CSharpPlatform.AL
{
    public class AL
    {
        internal const string DllWindows = "OpenAL32";
        internal const string DllLinux = "libopenal.so.1";
        internal const string DllMac = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
        internal const string DllAndroid = "libopenal.so.1";

        static AL()
        {
            DynamicLibraryFactory.MapLibraryToType<AL>(
                DynamicLibraryFactory.CreateForLibrary(DllWindows, DllLinux, DllMac, DllAndroid));
        }

        public const int AL_INVALID = -1;
        public const int AL_ILLEGAL_ENUM = AL_INVALID_ENUM;
        public const int AL_ILLEGAL_COMMAND = AL_INVALID_OPERATION;
        public const int AL_VERSION_1_0 = 1;
        public const int AL_VERSION_1_1 = 1;
        public const int AL_NONE = 0;
        public const int AL_FALSE = 0;
        public const int AL_TRUE = 1;
        public const int AL_SOURCE_RELATIVE = 0x202;
        public const int AL_CONE_INNER_ANGLE = 0x1001;
        public const int AL_CONE_OUTER_ANGLE = 0x1002;
        public const int AL_PITCH = 0x1003;
        public const int AL_POSITION = 0x1004;
        public const int AL_DIRECTION = 0x1005;
        public const int AL_VELOCITY = 0x1006;
        public const int AL_LOOPING = 0x1007;
        public const int AL_BUFFER = 0x1009;
        public const int AL_GAIN = 0x100A;
        public const int AL_MIN_GAIN = 0x100D;
        public const int AL_MAX_GAIN = 0x100E;
        public const int AL_ORIENTATION = 0x100F;
        public const int AL_SOURCE_STATE = 0x1010;
        public const int AL_INITIAL = 0x1011;
        public const int AL_PLAYING = 0x1012;
        public const int AL_PAUSED = 0x1013;
        public const int AL_STOPPED = 0x1014;
        public const int AL_BUFFERS_QUEUED = 0x1015;
        public const int AL_BUFFERS_PROCESSED = 0x1016;
        public const int AL_SEC_OFFSET = 0x1024;
        public const int AL_SAMPLE_OFFSET = 0x1025;
        public const int AL_BYTE_OFFSET = 0x1026;
        public const int AL_SOURCE_TYPE = 0x1027;
        public const int AL_STATIC = 0x1028;
        public const int AL_STREAMING = 0x1029;
        public const int AL_UNDETERMINED = 0x1030;
        public const int AL_FORMAT_MONO8 = 0x1100;
        public const int AL_FORMAT_MONO16 = 0x1101;
        public const int AL_FORMAT_STEREO8 = 0x1102;
        public const int AL_FORMAT_STEREO16 = 0x1103;
        public const int AL_REFERENCE_DISTANCE = 0x1020;
        public const int AL_ROLLOFF_FACTOR = 0x1021;
        public const int AL_CONE_OUTER_GAIN = 0x1022;
        public const int AL_MAX_DISTANCE = 0x1023;
        public const int AL_FREQUENCY = 0x2001;
        public const int AL_BITS = 0x2002;
        public const int AL_CHANNELS = 0x2003;
        public const int AL_SIZE = 0x2004;
        public const int AL_UNUSED = 0x2010;
        public const int AL_PENDING = 0x2011;
        public const int AL_PROCESSED = 0x2012;
        public const int AL_NO_ERROR = AL_FALSE;
        public const int AL_INVALID_NAME = 0xA001;
        public const int AL_INVALID_ENUM = 0xA002;
        public const int AL_INVALID_VALUE = 0xA003;
        public const int AL_INVALID_OPERATION = 0xA004;
        public const int AL_OUT_OF_MEMORY = 0xA005;
        public const int AL_VENDOR = 0xB001;
        public const int AL_VERSION = 0xB002;
        public const int AL_RENDERER = 0xB003;
        public const int AL_EXTENSIONS = 0xB004;
        public const int AL_DOPPLER_FACTOR = 0xC000;
        public const int AL_DOPPLER_VELOCITY = 0xC001;
        public const int AL_SPEED_OF_SOUND = 0xC003;
        public const int AL_DISTANCE_MODEL = 0xD000;
        public const int AL_INVERSE_DISTANCE = 0xD001;
        public const int AL_INVERSE_DISTANCE_CLAMPED = 0xD002;
        public const int AL_LINEAR_DISTANCE = 0xD003;
        public const int AL_LINEAR_DISTANCE_CLAMPED = 0xD004;
        public const int AL_EXPONENT_DISTANCE = 0xD005;
        public const int AL_EXPONENT_DISTANCE_CLAMPED = 0xD006;

        public static readonly alGetError alGetError;
        public static readonly alSourcef alSourcef;
        public static readonly alSource3f alSource3f;
        public static readonly alGenSources alGenSources;
        public static readonly alDeleteSources alDeleteSources;
        public static readonly alIsSource alIsSource;
        public static readonly alEnable alEnable;
        public static readonly alDisable alDisable;
        public static readonly alIsEnabled alIsEnabled;
        public static readonly alGetSourcei alGetSourcei;
        public static readonly alBufferData alBufferData;
        public static readonly alGetString alGetString;
        public static readonly alGetBooleanv alGetBooleanv;
        public static readonly alGetIntegerv alGetIntegerv;
        public static readonly alGetFloatv alGetFloatv;
        public static readonly alGetDoublev alGetDoublev;
        public static readonly alGetBoolean alGetBoolean;
        public static readonly alGetInteger alGetInteger;
        public static readonly alGetFloat alGetFloat;
        public static readonly alGetDouble alGetDouble;
        public static readonly alIsExtensionPresent alIsExtensionPresent;
        public static readonly alGetProcAddress alGetProcAddress;
        public static readonly alGetEnumValue alGetEnumValue;
        public static readonly alListenerf alListenerf;
        public static readonly alListener3f alListener3f;
        public static readonly alListenerfv alListenerfv;
        public static readonly alListeneri alListeneri;
        public static readonly alListener3i alListener3i;
        public static readonly alListeneriv alListeneriv;
        public static readonly alGetListenerf alGetListenerf;
        public static readonly alGetListener3f alGetListener3f;
        public static readonly alGetListenerfv alGetListenerfv;
        public static readonly alGetListeneri alGetListeneri;
        public static readonly alGetListener3i alGetListener3i;
        public static readonly alGetListeneriv alGetListeneriv;
        public static readonly alSourcefv alSourcefv;
        public static readonly alSourcei alSourcei;
        public static readonly alSource3i alSource3i;
        public static readonly alSourceiv alSourceiv;
        public static readonly alGetSourcef alGetSourcef;
        public static readonly alGetSource3f alGetSource3f;
        public static readonly alGetSourcefv alGetSourcefv;
        public static readonly alGetSource3i alGetSource3i;
        public static readonly alGetSourceiv alGetSourceiv;
        public static readonly alSourcePlayv alSourcePlayv;
        public static readonly alSourceStopv alSourceStopv;
        public static readonly alSourceRewindv alSourceRewindv;
        public static readonly alSourcePausev alSourcePausev;
        public static readonly alSourcePlay alSourcePlay;
        public static readonly alSourceStop alSourceStop;
        public static readonly alSourceRewind alSourceRewind;
        public static readonly alSourcePause alSourcePause;
        public static readonly alSourceQueueBuffers alSourceQueueBuffers;
        public static readonly alSourceUnqueueBuffers alSourceUnqueueBuffers;
        public static readonly alGenBuffers alGenBuffers;
        public static readonly alDeleteBuffers alDeleteBuffers;
        public static readonly alIsBuffer alIsBuffer;
        public static readonly alBufferf alBufferf;
        public static readonly alBuffer3f alBuffer3f;
        public static readonly alBufferfv alBufferfv;
        public static readonly alBufferi alBufferi;
        public static readonly alBuffer3i alBuffer3i;
        public static readonly alBufferiv alBufferiv;
        public static readonly alGetBufferf alGetBufferf;
        public static readonly alGetBuffer3f alGetBuffer3f;
        public static readonly alGetBufferfv alGetBufferfv;
        public static readonly alGetBufferi alGetBufferi;
        public static readonly alGetBuffer3i alGetBuffer3i;
        public static readonly alGetBufferiv alGetBufferiv;

        public const int ALC_INVALID = 0;
        public const int ALC_VERSION_0_1 = 1;
        public const int ALC_FALSE = 0;
        public const int ALC_TRUE = 1;
        public const int ALC_FREQUENCY = 0x1007;
        public const int ALC_REFRESH = 0x1008;
        public const int ALC_SYNC = 0x1009;
        public const int ALC_MONO_SOURCES = 0x1010;
        public const int ALC_STEREO_SOURCES = 0x1011;
        public const int ALC_NO_ERROR = ALC_FALSE;
        public const int ALC_INVALID_DEVICE = 0xA001;
        public const int ALC_INVALID_CONTEXT = 0xA002;
        public const int ALC_INVALID_ENUM = 0xA003;
        public const int ALC_INVALID_VALUE = 0xA004;
        public const int ALC_OUT_OF_MEMORY = 0xA005;
        public const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;
        public const int ALC_DEVICE_SPECIFIER = 0x1005;
        public const int ALC_EXTENSIONS = 0x1006;
        public const int ALC_MAJOR_VERSION = 0x1000;
        public const int ALC_MINOR_VERSION = 0x1001;
        public const int ALC_ATTRIBUTES_SIZE = 0x1002;
        public const int ALC_ALL_ATTRIBUTES = 0x1003;
        public const int ALC_CAPTURE_DEVICE_SPECIFIER = 0x310;
        public const int ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x311;
        public const int ALC_CAPTURE_SAMPLES = 0x312;

        public static readonly alcCreateContext alcCreateContext;
        public static readonly alcMakeContextCurrent alcMakeContextCurrent;
        public static readonly alcProcessContext alcProcessContext;
        public static readonly alcSuspendContext alcSuspendContext;
        public static readonly alcDestroyContext alcDestroyContext;
        public static readonly alcGetCurrentContext alcGetCurrentContext;
        public static readonly alcGetContextsDevice alcGetContextsDevice;
        public static readonly alcOpenDevice alcOpenDevice;
        public static readonly alcCloseDevice alcCloseDevice;
        public static readonly alcGetError alcGetError;
        public static readonly alcIsExtensionPresent alcIsExtensionPresent;
        public static readonly alcGetProcAddress alcGetProcAddress;
        public static readonly alcGetEnumValue alcGetEnumValue;
        public static readonly alcGetString alcGetString;
        public static readonly alcGetIntegerv alcGetIntegerv;
        public static readonly alcCaptureOpenDevice alcCaptureOpenDevice;
        public static readonly alcCaptureCloseDevice alcCaptureCloseDevice;
        public static readonly alcCaptureStart alcCaptureStart;
        public static readonly alcCaptureStop alcCaptureStop;
        public static readonly alcCaptureSamples alcCaptureSamples;

        public static string alGetErrorString(int error)
        {
            return "Unknown(" + error + ")";
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate int alGetError();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourcef(uint sid, int param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSource3f(uint sid, int param, float value1, float value2, float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGenSources(int n, uint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alDeleteSources(int n, uint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate bool alIsSource(uint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alEnable(int capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alDisable(int capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate bool alIsEnabled(int capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcei(uint sid, int param, int* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferData(uint bid, int format, void* data, int size, int freq);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate byte* alGetString(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBooleanv(int param, bool* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetIntegerv(int param, int* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetFloatv(int param, float* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetDoublev(int param, double* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate bool alGetBoolean(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate int alGetInteger(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate float alGetFloat(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate double alGetDouble(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate bool alIsExtensionPresent(byte* extname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void* alGetProcAddress(byte* fname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate int alGetEnumValue(byte* ename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alListenerf(int param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alListener3f(int param, float value1, float value2, float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListenerfv(int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alListeneri(int param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alListener3i(int param, int value1, int value2, int value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListeneriv(int param, int* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListenerf(int param, float* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListener3f(int param, float*value1, float*value2, float*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListenerfv(int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListeneri(int param, int* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListener3i(int param, int*value1, int*value2, int*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListeneriv(int param, int* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcefv(uint sid, int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourcei(uint sid, int param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSource3i(uint sid, int param, int value1, int value2, int value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceiv(uint sid, int param, int* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcef(uint sid, int param, float* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSource3f(uint sid, int param, float* value1, float* value2,
        float* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcefv(uint sid, int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSource3i(uint sid, int param, int* value1, int* value2, int* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourceiv(uint sid, int param, int* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePlayv(int ns, uint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceStopv(int ns, uint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceRewindv(int ns, uint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePausev(int ns, uint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourcePlay(uint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourceStop(uint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourceRewind(uint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alSourcePause(uint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceQueueBuffers(uint sid, int numEntries, uint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceUnqueueBuffers(uint sid, int numEntries, uint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGenBuffers(int n, uint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alDeleteBuffers(int n, uint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate bool alIsBuffer(uint bid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alBufferf(uint bid, int param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alBuffer3f(uint bid, int param, float value1, float value2, float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferfv(uint bid, int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alBufferi(uint bid, int param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void alBuffer3i(uint bid, int param, int value1, int value2, int value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferiv(uint bid, int param, int* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferf(uint bid, int param, float* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBuffer3f(uint bid, int param, float* value1, float* value2,
        float* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferfv(uint bid, int param, float* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferi(uint bid, int param, int* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBuffer3i(uint bid, int param, int* value1, int* value2, int* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferiv(uint bid, int param, int* values);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALCcontext* alcCreateContext(ALCdevice*device, int* attrlist);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate bool alcMakeContextCurrent(ALCcontext*context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcProcessContext(ALCcontext*context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcSuspendContext(ALCcontext*context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcDestroyContext(ALCcontext*context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALCcontext* alcGetCurrentContext();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALCdevice* alcGetContextsDevice(ALCcontext*context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALCdevice* alcOpenDevice(byte*devicename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate bool alcCloseDevice(ALCdevice*device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate int alcGetError(ALCdevice*device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate bool alcIsExtensionPresent(ALCdevice*device, byte*extname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void* alcGetProcAddress(ALCdevice*device, byte*funcname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate int alcGetEnumValue(ALCdevice*device, byte*enumname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate byte* alcGetString(ALCdevice*device, int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcGetIntegerv(ALCdevice*device, int param, int size, int*data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALCdevice* alcCaptureOpenDevice(byte*devicename, uint frequency, int format,
        int buffersize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate bool alcCaptureCloseDevice(ALCdevice*device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcCaptureStart(ALCdevice*device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcCaptureStop(ALCdevice*device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alcCaptureSamples(ALCdevice*device, void*buffer, int samples);
}