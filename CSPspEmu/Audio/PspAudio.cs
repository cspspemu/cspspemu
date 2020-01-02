using System;
using System.Linq;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Audio
{
    public sealed unsafe class PspAudio : IInjectInitialize, IDisposable
    {
        /// Output formats for PSP audio.
        public enum FormatEnum
        {
            /// Channel is set to stereo output (2 channels).
            Stereo = 0x00,

            /// Channel is set to mono output (1 channel).
            Mono = 0x10,
        }

        /// The maximum output volume.
        public const int MaxVolume = 0x8000;

        public const int SamplesMax = 0x10000 - 64;

        /// Used to request the next available hardware channel.
        /// Maximum number of allowed audio channels
        public const int MaxChannels = 8;
        //public const int MaxChannels = 32;

        /// Number of audio channels
        public PspAudioChannel[] Channels;

        public PspAudioChannel SrcOutput2Channel;

        [Inject] public PspAudioImpl PspAudioImpl;

        private PspAudio()
        {
        }

        void IInjectInitialize.Initialize()
        {
            Channels = new PspAudioChannel[MaxChannels];
            for (int n = 0; n < MaxChannels; n++)
            {
                Channels[n] = new PspAudioChannel(this)
                {
                    Index = n,
                    Available = true,
                };
            }

            SrcOutput2Channel = new PspAudioChannel(this)
            {
                Index = MaxChannels,
                Available = true,
            };
        }

        public PspAudioChannel GetFreeChannel()
        {
            if (!Channels.Any(Channel => Channel.Available)) throw new NoChannelsAvailableException();
            return Channels.Reverse().First(Channel => Channel.Available);
        }

        private void CheckChannelId(int ChannelId)
        {
            if (ChannelId < 0 || ChannelId >= Channels.Length)
            {
                throw new InvalidChannelException();
            }
        }

        public PspAudioChannel GetChannel(int ChannelId, bool CanAlloc = false)
        {
            PspAudioChannel Channel;
            if (CanAlloc && ChannelId < 0)
            {
                Channel = GetFreeChannel();
            }
            else
            {
                CheckChannelId(ChannelId);
                Channel = Channels[ChannelId];
            }
            return Channel;
        }

        public void Update()
        {
            PspAudioImpl.Update((MixedSamples) =>
            {
                var RequiredSamples = MixedSamples.Length;
                fixed (short* MixedSamplesPtr = MixedSamples)
                {
                    var MixedSamplesDenormalized = stackalloc int[RequiredSamples];

                    foreach (var Channel in Channels)
                    {
                        var ChannelSamples = Channel.Read(RequiredSamples);

                        fixed (short* ChannelSamplesPtr = ChannelSamples)
                        {
                            for (int n = 0; n < ChannelSamples.Length; n++)
                            {
                                MixedSamplesDenormalized[n] += ChannelSamplesPtr[n];
                            }
                        }
                    }

                    for (int n = 0; n < RequiredSamples; n++)
                    {
                        MixedSamplesPtr[n] = StereoShortSoundSample.Clamp(MixedSamplesDenormalized[n]);
                    }
                }
            });
        }

        private bool Disposed = false;

        public void StopSynchronized()
        {
            if (!Disposed)
            {
                Disposed = true;
                PspAudioImpl.StopSynchronized();
            }
        }

        void IDisposable.Dispose()
        {
            StopSynchronized();
        }
    }

    public class InvalidChannelException : Exception
    {
    }

    public class NoChannelsAvailableException : Exception
    {
    }
}