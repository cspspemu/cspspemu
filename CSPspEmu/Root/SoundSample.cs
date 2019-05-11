using System;
using System.Runtime;

namespace CSPspEmu.Core.Types
{
    public struct MonoShortSoundSample
    {
        public short Value; // Left/Right audio

        public MonoShortSoundSample(short value) => Value = value;

        public static MonoShortSoundSample Mix(MonoShortSoundSample a, MonoShortSoundSample b) =>
            new MonoShortSoundSample((short) ((a.Value + b.Value) / 2));

        public short MonoLeftRight
        {
            set => Value = value;
        }
    }

    public struct StereoShortSoundSample
    {
        public short Left; // Left audio
        public short Right; // Right audio

        public short GetByIndex(int index) => index == 0 ? Left : Right;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StereoShortSoundSample(int left, int right)
        {
            Left = Clamp(left);
            Right = Clamp(right);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StereoShortSoundSample(int leftRight)
        {
            Left = Clamp(leftRight);
            Right = Clamp(leftRight);
        }

        public int MaxAmplitudeLeftRight
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get => Math.Min(short.MaxValue, Math.Max(Math.Abs((int) Left), Math.Abs((int) Right)));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static StereoShortSoundSample Mix(StereoShortSoundSample a, StereoShortSoundSample b) =>
            new StereoShortSoundSample(Clamp((a.Left + b.Left) / 2), Clamp((a.Right + b.Right) / 2));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static implicit operator StereoIntSoundSample(StereoShortSoundSample stereoShortSoundSample) =>
            new StereoIntSoundSample(stereoShortSoundSample.Left, stereoShortSoundSample.Right);

        public short MonoLeftRight
        {
            set
            {
                Left = value;
                Right = value;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static short Clamp(int value)
        {
            if (value < short.MinValue) return short.MinValue;
            if (value > short.MaxValue) return short.MaxValue;
            return (short) value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StereoShortSoundSample ApplyVolumes(int leftVolume, int rightVolume)
        {
            return new StereoShortSoundSample(
                Clamp(Left * leftVolume / 0x1000),
                Clamp(Right * rightVolume / 0x1000)
            );
        }
    }

    public struct StereoIntSoundSample
    {
        public int Left; // Left audio
        public int Right; // Right audio

        public StereoIntSoundSample(int left, int right)
        {
            Left = left;
            Right = right;
        }

        public int MaxAmplitudeLeftRight => Math.Max(Math.Abs(Left), Math.Abs(Right));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static StereoIntSoundSample Mix(StereoIntSoundSample a, StereoIntSoundSample b) =>
            new StereoIntSoundSample((a.Left + b.Left) / 2, (a.Right + b.Right) / 2);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static StereoIntSoundSample operator +(StereoIntSoundSample a, StereoIntSoundSample b) =>
            new StereoIntSoundSample((a.Left + b.Left), (a.Right + b.Right));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static StereoIntSoundSample operator /(StereoIntSoundSample a, int div) =>
            new StereoIntSoundSample((a.Left / div), (a.Right / div));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static StereoIntSoundSample operator *(StereoIntSoundSample a, int mult) =>
            new StereoIntSoundSample(a.Left * mult, a.Right * mult);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static implicit operator StereoShortSoundSample(StereoIntSoundSample stereoIntSoundSample) =>
            new StereoShortSoundSample(stereoIntSoundSample.Left, stereoIntSoundSample.Right);

        public int MonoLeftRight
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set
            {
                Left = value;
                Right = value;
            }
        }
    }
}