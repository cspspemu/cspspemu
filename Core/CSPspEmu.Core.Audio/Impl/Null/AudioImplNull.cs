using System;

namespace CSPspEmu.Core.Audio.Impl.Null
{
    public class AudioImplNull : PspAudioImpl
    {
       
        public override void Update(Action<short[]> readStream)
        {
            //throw new NotImplementedException();
        }

        public override void StopSynchronized()
        {
            //throw new NotImplementedException();
        }

        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "Null",
            Version = "1.0",
        };

        public override bool IsWorking => true;
    }
}