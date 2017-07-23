using CSPspEmu.Hle.Formats.audio.At3;
using CSPspEmu.Hle.Media.audio.At3.SUB;

namespace CSPspEmu.Hle.Media.audio.At3
{
    public sealed unsafe class MaiAt3PlusFrameDecoder
    {
        private readonly MaiAt3PlusCoreDecoder[] _cores = new MaiAt3PlusCoreDecoder[0x10];
        private readonly short[] _sampleBuf = new short[0x8000];

        private readonly short[] _sampleBufTmp = new short[0x8000];
        //int num_cores = 0;

        public MaiAt3PlusFrameDecoder()
        {
        }

        public int DecodeFrame(byte* pFrameData, int dataLen, out int pChns, out short[] ppSampleBuf)
        {
            int rs = 0;

            var mbr0 = new MaiBitReader(dataLen + 0x10);
            mbr0.AddData(pFrameData, dataLen);
            var pad = stackalloc byte[0x10];
            mbr0.AddData(pad, 0x10);

            if (mbr0.GetWithI32Buffer(1) != 0)
            {
                rs = -1;
            }


            int counterSubstream = 0;
            int counterChn = 0;
            while (rs == 0)
            {
                int substreamType = mbr0.GetWithI32Buffer(2);
                uint jointFlag = 0;
                uint chns = 0;

                if (substreamType == 0)
                {
                    jointFlag = 0;
                    chns = 1;
                }
                else if (substreamType == 1)
                {
                    jointFlag = 1;
                    chns = 2;
                }
                else if (substreamType == 3)
                {
                    break;
                }
                else
                {
                    rs = -1;
                }

                if (_cores[counterSubstream] == null)
                    _cores[counterSubstream] = new MaiAt3PlusCoreDecoder();

                if (0 != (rs = _cores[counterSubstream].ParseStream(mbr0, chns, jointFlag)))
                    break;

                if (0 != (rs = _cores[counterSubstream].decodeStream(chns)))
                    break;

                for (int a0 = 0; a0 < chns; a0++)
                    _cores[counterSubstream].getAudioSamplesI16((uint) a0,
                        new ManagedPointer<short>(_sampleBufTmp, 0x800 * (counterChn++)));

                counterSubstream++;
            }

            for (int a0 = 0; a0 < 0x800; a0++)
            {
                for (int a1 = 0; a1 < counterChn; a1++)
                {
                    _sampleBuf[a0 * counterChn + a1] = _sampleBufTmp[a1 * 0x800 + a0];
                }
            }
            mbr0.Dispose();

            pChns = counterChn;
            ppSampleBuf = _sampleBuf;

            return rs;
        }
    }
}