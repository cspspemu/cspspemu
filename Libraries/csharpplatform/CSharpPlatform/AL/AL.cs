﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using CSharpPlatform.Library;

namespace CSharpPlatform.AL
{
    unsafe public partial class AL
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

        public const int AL_INVALID = (-1);
        public const int AL_ILLEGAL_ENUM = AL_INVALID_ENUM;
        public const int AL_ILLEGAL_COMMAND = AL_INVALID_OPERATION;
        public const int AL_VERSION_1_0 = 1;
        public const int AL_VERSION_1_1 = 1;
        public const int AL_NONE = 0;
        public const int AL_FALSE = 0;
        public const int AL_TRUE = 1;
        public const ALenum AL_SOURCE_RELATIVE = 0x202;
        public const ALenum AL_CONE_INNER_ANGLE = 0x1001;
        public const ALenum AL_CONE_OUTER_ANGLE = 0x1002;
        public const ALenum AL_PITCH = 0x1003;
        public const ALenum AL_POSITION = 0x1004;
        public const ALenum AL_DIRECTION = 0x1005;
        public const ALenum AL_VELOCITY = 0x1006;
        public const ALenum AL_LOOPING = 0x1007;
        public const ALenum AL_BUFFER = 0x1009;
        public const ALenum AL_GAIN = 0x100A;
        public const ALenum AL_MIN_GAIN = 0x100D;
        public const ALenum AL_MAX_GAIN = 0x100E;
        public const ALenum AL_ORIENTATION = 0x100F;
        public const ALenum AL_SOURCE_STATE = 0x1010;
        public const ALenum AL_INITIAL = 0x1011;
        public const ALenum AL_PLAYING = 0x1012;
        public const ALenum AL_PAUSED = 0x1013;
        public const ALenum AL_STOPPED = 0x1014;
        public const ALenum AL_BUFFERS_QUEUED = 0x1015;
        public const ALenum AL_BUFFERS_PROCESSED = 0x1016;
        public const ALenum AL_SEC_OFFSET = 0x1024;
        public const ALenum AL_SAMPLE_OFFSET = 0x1025;
        public const ALenum AL_BYTE_OFFSET = 0x1026;
        public const ALenum AL_SOURCE_TYPE = 0x1027;
        public const ALenum AL_STATIC = 0x1028;
        public const ALenum AL_STREAMING = 0x1029;
        public const ALenum AL_UNDETERMINED = 0x1030;
        public const ALenum AL_FORMAT_MONO8 = 0x1100;
        public const ALenum AL_FORMAT_MONO16 = 0x1101;
        public const ALenum AL_FORMAT_STEREO8 = 0x1102;
        public const ALenum AL_FORMAT_STEREO16 = 0x1103;
        public const ALenum AL_REFERENCE_DISTANCE = 0x1020;
        public const ALenum AL_ROLLOFF_FACTOR = 0x1021;
        public const ALenum AL_CONE_OUTER_GAIN = 0x1022;
        public const ALenum AL_MAX_DISTANCE = 0x1023;
        public const ALenum AL_FREQUENCY = 0x2001;
        public const ALenum AL_BITS = 0x2002;
        public const ALenum AL_CHANNELS = 0x2003;
        public const ALenum AL_SIZE = 0x2004;
        public const ALenum AL_UNUSED = 0x2010;
        public const ALenum AL_PENDING = 0x2011;
        public const ALenum AL_PROCESSED = 0x2012;
        public const ALenum AL_NO_ERROR = AL_FALSE;
        public const ALenum AL_INVALID_NAME = 0xA001;
        public const ALenum AL_INVALID_ENUM = 0xA002;
        public const ALenum AL_INVALID_VALUE = 0xA003;
        public const ALenum AL_INVALID_OPERATION = 0xA004;
        public const ALenum AL_OUT_OF_MEMORY = 0xA005;
        public const ALenum AL_VENDOR = 0xB001;
        public const ALenum AL_VERSION = 0xB002;
        public const ALenum AL_RENDERER = 0xB003;
        public const ALenum AL_EXTENSIONS = 0xB004;
        public const ALenum AL_DOPPLER_FACTOR = 0xC000;
        public const ALenum AL_DOPPLER_VELOCITY = 0xC001;
        public const ALenum AL_SPEED_OF_SOUND = 0xC003;
        public const ALenum AL_DISTANCE_MODEL = 0xD000;
        public const ALenum AL_INVERSE_DISTANCE = 0xD001;
        public const ALenum AL_INVERSE_DISTANCE_CLAMPED = 0xD002;
        public const ALenum AL_LINEAR_DISTANCE = 0xD003;
        public const ALenum AL_LINEAR_DISTANCE_CLAMPED = 0xD004;
        public const ALenum AL_EXPONENT_DISTANCE = 0xD005;
        public const ALenum AL_EXPONENT_DISTANCE_CLAMPED = 0xD006;

        static public readonly alGetError alGetError;
        static public readonly alSourcef alSourcef;
        static public readonly alSource3f alSource3f;
        static public readonly alGenSources alGenSources;
        static public readonly alDeleteSources alDeleteSources;
        static public readonly alIsSource alIsSource;
        static public readonly alEnable alEnable;
        static public readonly alDisable alDisable;
        static public readonly alIsEnabled alIsEnabled;
        static public readonly alGetSourcei alGetSourcei;
        static public readonly alBufferData alBufferData;
        static public readonly alGetString alGetString;
        static public readonly alGetBooleanv alGetBooleanv;
        static public readonly alGetIntegerv alGetIntegerv;
        static public readonly alGetFloatv alGetFloatv;
        static public readonly alGetDoublev alGetDoublev;
        static public readonly alGetBoolean alGetBoolean;
        static public readonly alGetInteger alGetInteger;
        static public readonly alGetFloat alGetFloat;
        static public readonly alGetDouble alGetDouble;
        static public readonly alIsExtensionPresent alIsExtensionPresent;
        static public readonly alGetProcAddress alGetProcAddress;
        static public readonly alGetEnumValue alGetEnumValue;
        static public readonly alListenerf alListenerf;
        static public readonly alListener3f alListener3f;
        static public readonly alListenerfv alListenerfv;
        static public readonly alListeneri alListeneri;
        static public readonly alListener3i alListener3i;
        static public readonly alListeneriv alListeneriv;
        static public readonly alGetListenerf alGetListenerf;
        static public readonly alGetListener3f alGetListener3f;
        static public readonly alGetListenerfv alGetListenerfv;
        static public readonly alGetListeneri alGetListeneri;
        static public readonly alGetListener3i alGetListener3i;
        static public readonly alGetListeneriv alGetListeneriv;
        static public readonly alSourcefv alSourcefv;
        static public readonly alSourcei alSourcei;
        static public readonly alSource3i alSource3i;
        static public readonly alSourceiv alSourceiv;
        static public readonly alGetSourcef alGetSourcef;
        static public readonly alGetSource3f alGetSource3f;
        static public readonly alGetSourcefv alGetSourcefv;
        static public readonly alGetSource3i alGetSource3i;
        static public readonly alGetSourceiv alGetSourceiv;
        static public readonly alSourcePlayv alSourcePlayv;
        static public readonly alSourceStopv alSourceStopv;
        static public readonly alSourceRewindv alSourceRewindv;
        static public readonly alSourcePausev alSourcePausev;
        static public readonly alSourcePlay alSourcePlay;
        static public readonly alSourceStop alSourceStop;
        static public readonly alSourceRewind alSourceRewind;
        static public readonly alSourcePause alSourcePause;
        static public readonly alSourceQueueBuffers alSourceQueueBuffers;
        static public readonly alSourceUnqueueBuffers alSourceUnqueueBuffers;
        static public readonly alGenBuffers alGenBuffers;
        static public readonly alDeleteBuffers alDeleteBuffers;
        static public readonly alIsBuffer alIsBuffer;
        static public readonly alBufferf alBufferf;
        static public readonly alBuffer3f alBuffer3f;
        static public readonly alBufferfv alBufferfv;
        static public readonly alBufferi alBufferi;
        static public readonly alBuffer3i alBuffer3i;
        static public readonly alBufferiv alBufferiv;
        static public readonly alGetBufferf alGetBufferf;
        static public readonly alGetBuffer3f alGetBuffer3f;
        static public readonly alGetBufferfv alGetBufferfv;
        static public readonly alGetBufferi alGetBufferi;
        static public readonly alGetBuffer3i alGetBuffer3i;
        static public readonly alGetBufferiv alGetBufferiv;

        static public string alGetErrorString(ALenum error)
        {
            return "Unknown(" + error + ")";
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALenum alGetError();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcef(ALuint sid, ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSource3f(ALuint sid, ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGenSources(ALsizei n, ALuint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alDeleteSources(ALsizei n, ALuint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALboolean alIsSource(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alEnable(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alDisable(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALboolean alIsEnabled(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSourcei(ALuint sid, ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBufferData(ALuint bid, ALenum format, void* data, ALsizei size, ALsizei freq);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALchar* alGetString(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBooleanv(ALenum param, ALboolean* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetIntegerv(ALenum param, ALint* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetFloatv(ALenum param, ALfloat* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetDoublev(ALenum param, ALdouble* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALboolean alGetBoolean(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALint alGetInteger(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALfloat alGetFloat(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALdouble alGetDouble(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALboolean alIsExtensionPresent(ALchar* extname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void* alGetProcAddress(ALchar* fname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALenum alGetEnumValue(ALchar* ename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListenerf(ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListener3f(ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListenerfv(ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListeneri(ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListener3i(ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alListeneriv(ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListenerf(ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListener3f(ALenum param, ALfloat*value1, ALfloat*value2, ALfloat*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListenerfv(ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListeneri(ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListener3i(ALenum param, ALint*value1, ALint*value2, ALint*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetListeneriv(ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcefv(ALuint sid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcei(ALuint sid, ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSource3i(ALuint sid, ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceiv(ALuint sid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSourcef(ALuint sid, ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSource3f(ALuint sid, ALenum param, ALfloat* value1, ALfloat* value2,
        ALfloat* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSourcefv(ALuint sid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSource3i(ALuint sid, ALenum param, ALint* value1, ALint* value2, ALint* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetSourceiv(ALuint sid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcePlayv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceStopv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceRewindv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcePausev(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcePlay(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceStop(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceRewind(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourcePause(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceQueueBuffers(ALuint sid, ALsizei numEntries, ALuint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alSourceUnqueueBuffers(ALuint sid, ALsizei numEntries, ALuint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGenBuffers(ALsizei n, ALuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alDeleteBuffers(ALsizei n, ALuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate ALboolean alIsBuffer(ALuint bid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBufferf(ALuint bid, ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBuffer3f(ALuint bid, ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBufferfv(ALuint bid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBufferi(ALuint bid, ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBuffer3i(ALuint bid, ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alBufferiv(ALuint bid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBufferf(ALuint bid, ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBuffer3f(ALuint bid, ALenum param, ALfloat* value1, ALfloat* value2,
        ALfloat* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBufferfv(ALuint bid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBufferi(ALuint bid, ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBuffer3i(ALuint bid, ALenum param, ALint* value1, ALint* value2, ALint* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate void alGetBufferiv(ALuint bid, ALenum param, ALint* values);
}