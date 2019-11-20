using System.Linq;
using CSPspEmu.Hle.Media.audio.At3.SUB;

namespace CSPspEmu.Hle.Formats.audio.At3
{
    public sealed class MaiAt3PlusCoreDecoderChnAccDataEntry
    {
        public int NumAcc;
        public int[] Data0 = new int[7];
        public uint[] Data1 = new uint[7];
        public int AccNow;
        public int[] Reserved = new int[0x16];
    }

    public sealed class MaiAt3PlusCoreDecoderChnAccData
    {
        public MaiAt3PlusCoreDecoderChnAccDataEntry[] Table = CreateTable();
        public MaiAt3PlusCoreDecoderPackTable0 Acc = new MaiAt3PlusCoreDecoderPackTable0();

        public static MaiAt3PlusCoreDecoderChnAccDataEntry[] CreateTable()
        {
            return Enumerable.Range(0, 0x10).Select(index => new MaiAt3PlusCoreDecoderChnAccDataEntry()).ToArray();
        }

        public void TableMemset()
        {
            Table = CreateTable();
        }
    }

    public sealed unsafe class MaiAt3PlusCoreDecoderChnAccTableTable
    {
        public int[] Unk = new int[8];
        public int NumUk;
        public ManagedPointer<MaiAt3PlusCoreDecoderChnAccTableData> Ptr0;
    }


    public sealed class MaiAt3PlusCoreDecoderChnAccTable
    {
        public MaiAt3PlusCoreDecoderChnAccTableInner Inner;
        public MaiAt3PlusCoreDecoderChnAccTableTable[] Table = CreateTable();

        public static MaiAt3PlusCoreDecoderChnAccTableTable[] CreateTable()
        {
            return Enumerable.Range(0, 0x10).Select(index => new MaiAt3PlusCoreDecoderChnAccTableTable()).ToArray();
        }

        public void TableReset()
        {
            Table = CreateTable();
        }
    }

    public sealed unsafe class MaiAt3PlusCoreDecoderChnInfo
    {
        public int ChnFlag;
        public MaiAt3PlusCoreDecoderJointChnInfo JointChnInfo = new MaiAt3PlusCoreDecoderJointChnInfo();
        public MaiAt3PlusCoreDecoderChnAccData AccDataNow = new MaiAt3PlusCoreDecoderChnAccData();
        public MaiAt3PlusCoreDecoderChnAccData AccDataOld = new MaiAt3PlusCoreDecoderChnAccData();
        public MaiAt3PlusCoreDecoderChnAccTable AccTableOld = new MaiAt3PlusCoreDecoderChnAccTable();
        public MaiAt3PlusCoreDecoderChnAccTable AccTableNow = new MaiAt3PlusCoreDecoderChnAccTable();
        public MaiAt3PlusCoreDecoderChnInfo ChnRef;

        public uint Var1034;
        public uint[] CheckTable0 = new uint[0x20];

        public int Table0FlagEx;
        public int Table0FlagDataNum;
        public uint Table0DataNum0;
        public uint Table0DataNum1;

        public int Uk1C718;
        public int Uk1C714;

        public uint Uk1B450;

        public uint[] Table0 = new uint[0x20];
        public uint[] Table1 = new uint[0x20];
        public uint[] Table2 = new uint[0x20];
        public short[] Table3 = new short[0x800];
        public uint[] Table4 = new uint[0x10];

        public uint[] InnerPackTable0CheckTable = new uint[0x10];
    }

    public sealed class MaiAt3PlusCoreDecoderChnAccTableData
    {
        public int Unk0;
        public int Unk1;
        public int Unk2;
        public int Unk3;
    }

    public sealed class MaiAt3PlusCoreDecoderChnAccTableInner
    {
        public int Unk0;
        public int Unk1;
        public int Unk2;
        public MaiAt3PlusCoreDecoderChnAccTableData[] Data = new MaiAt3PlusCoreDecoderChnAccTableData[0x30];
        public ManagedPointer<MaiAt3PlusCoreDecoderChnAccTableData> PtrToUseNow;
        public MaiAt3PlusCoreDecoderPackTable0 TableUnk0;
        public MaiAt3PlusCoreDecoderPackTable0 TableUnk1;
        public MaiAt3PlusCoreDecoderPackTable0 TableUnk2;
    }

    public sealed class MaiAt3PlusCoreDecoderPackTable0
    {
        public int CheckData0;
        public int CheckData1;
        public int[] Data = new int[0x10];
    }

    public sealed class MaiAt3PlusCoreDecoderJointChnInfo
    {
        public MaiAt3PlusCoreDecoderPackTable0 Table00 = new MaiAt3PlusCoreDecoderPackTable0();
        public MaiAt3PlusCoreDecoderPackTable0 Table48 = new MaiAt3PlusCoreDecoderPackTable0();

        public uint NumBandSplitedDeclared;
        public uint NumBandSplitedUsed;

        public uint NumBandDeclared;
        public uint NumBandUsed;

        public uint JointFlag;
        public uint Chns;

        public uint Var90;
        public int Var94;
        public int Var98;
        public int Var9C;
        public int Var118;
        public int[] Table11C = new int[0x100];
    }

    public sealed partial class MaiAt3PlusCoreDecoder
    {
        MaiAt3PlusCoreDecoderChnInfo[] _chnInfo = new MaiAt3PlusCoreDecoderChnInfo[2];

        float[][] _synBuf = {new float[0x1000], new float[0x1000]};
        float[][] _dstBuf = {new float[0x800], new float[0x800]};
        float[][] _kyouBuf = {new float[0x800], new float[0x800]};
        int _c900;

        public MaiAt3PlusCoreDecoder()
        {
            MaiAt3PlusCoreDecoderJointChnInfo jointChnInfo = new MaiAt3PlusCoreDecoderJointChnInfo();

            MaiAt3PlusCoreDecoderChnAccTableInner inner0 = new MaiAt3PlusCoreDecoderChnAccTableInner();
            MaiAt3PlusCoreDecoderChnAccTableInner inner1 = new MaiAt3PlusCoreDecoderChnAccTableInner();

            for (int a0 = 0; a0 < 2; a0++)
            {
                _chnInfo[a0] = new MaiAt3PlusCoreDecoderChnInfo();

                _chnInfo[a0].ChnFlag = a0;
                _chnInfo[a0].JointChnInfo = jointChnInfo;

                _chnInfo[a0].AccDataOld = new MaiAt3PlusCoreDecoderChnAccData();
                _chnInfo[a0].AccDataNow = new MaiAt3PlusCoreDecoderChnAccData();
                _chnInfo[a0].AccTableOld = new MaiAt3PlusCoreDecoderChnAccTable();
                _chnInfo[a0].AccTableOld.Inner = inner0;

                _chnInfo[a0].AccTableNow = new MaiAt3PlusCoreDecoderChnAccTable();
                _chnInfo[a0].AccTableNow.Inner = inner1;

                _chnInfo[a0].ChnRef = _chnInfo[0];
            }

            _c900 = 3;

            for (int a0 = 0; a0 < 2; a0++)
            {
                for (int a1 = 0; a1 < 0x1000; a1++)
                {
                    _synBuf[a0][a1] = 0.0f;
                }
            }

            for (int a0 = 0; a0 < 2; a0++)
            {
                for (int a1 = 0; a1 < 0x800; a1++)
                {
                    _kyouBuf[a0][a1] = 0.0f;
                }
            }
        }

        ~MaiAt3PlusCoreDecoder()
        {
            //heap0.free(chn_info[0].acc_table_now.inner);
            //heap0.free(chn_info[0].acc_table_old.inner);
            //heap0.free(chn_info[0].joint_chn_info);
            //for (int a0 = 0; a0 < 2; a0++)
            //{
            //	heap0.free(chn_info[a0].acc_table_now);
            //	heap0.free(chn_info[a0].acc_table_old);
            //	heap0.free(chn_info[a0].acc_data_now);
            //	heap0.free(chn_info[a0].acc_data_old);
            //	heap0.free(chn_info[a0]);
            //}
        }


        public int ParseStream(MaiBitReader mbr0, uint chns, uint jointFlag)
        {
            int rs = 0;

            _chnInfo[0].JointChnInfo.JointFlag = jointFlag;
            _chnInfo[0].JointChnInfo.Chns = chns;

            while (true)
            {
                if (0 != (rs = DecodeBandNum(mbr0, _chnInfo))) break;
                if (0 != (rs = DecodeTable0(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeTable1(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeTable2(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeTable3(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeAcc2Pre(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeAcc2Main(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeAcc6Inner(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;
                if (0 != (rs = DecodeTailInfo(mbr0, _chnInfo, _chnInfo[0].JointChnInfo.Chns))) break;

                break;
            }

            return rs;
        }


        int DecodeBandNum(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos)
        {
            chnInfos[0].JointChnInfo.NumBandSplitedDeclared =
                (uint) mbr0.GetWithI32Buffer(5) + 1;


            chnInfos[0].JointChnInfo.NumBandDeclared =
                (uint) MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_band_num_table0[
                    chnInfos[0].JointChnInfo.NumBandSplitedDeclared] + 1;

            chnInfos[0].JointChnInfo.Var118 =
                mbr0.GetWithI32Buffer(1);

            return 0;
        }

        int DecodeTable0(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                    chnInfos[a0].Table0[a1] = 0;

                if ((rs = MAPCDSF_decodeTable0_func_list0[chnInfos[a0].ChnFlag * 4 + mbr0.GetWithI32Buffer(2)]
                        (mbr0, chnInfos[a0])
                    ) != 0)
                    return rs;
            }

            chnInfos[0].JointChnInfo.NumBandSplitedUsed =
                chnInfos[0].JointChnInfo.NumBandSplitedDeclared; //[B0]

            if (chns == 2)
            {
                while (0 != chnInfos[0].JointChnInfo.NumBandSplitedUsed &&
                       0 == chnInfos[0].Table0[chnInfos[0].JointChnInfo.NumBandSplitedUsed - 1] &&
                       0 == chnInfos[1].Table0[chnInfos[0].JointChnInfo.NumBandSplitedUsed - 1])
                    chnInfos[0].JointChnInfo.NumBandSplitedUsed--;
            }
            else
            {
                while (0 != chnInfos[0].JointChnInfo.NumBandSplitedUsed &&
                       0 == chnInfos[0].Table0[chnInfos[0].JointChnInfo.NumBandSplitedUsed - 1])
                    chnInfos[0].JointChnInfo.NumBandSplitedUsed--;
            }

            chnInfos[0].JointChnInfo.NumBandUsed =
                (uint) MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_band_num_table0[
                    chnInfos[0].JointChnInfo.NumBandSplitedUsed] + 1;

            //check
            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                {
                    if (chnInfos[a0].Table0[a1] < 0 || chnInfos[a0].Table0[a1] >= 8) return -0x10B;
                }
            }

            return rs;
        }

        int DecodeTable1(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            if (chnInfos[0].JointChnInfo.NumBandSplitedUsed != 0) //
            {
                for (uint a0 = 0; a0 < chns; a0++)
                {
                    for (uint a1 = 0; a1 < 0x20; a1++)
                        chnInfos[a0].Table1[a1] = 0;

                    if (0 != (rs = MAPCDSF_decodeTable1_func_list0[
                                chnInfos[a0].ChnFlag * 4 + mbr0.GetWithI32Buffer(2)]
                            (mbr0, chnInfos[a0])
                        ))
                        return rs;
                }
            }

            //check
            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                {
                    if (chnInfos[a0].Table1[a1] < 0 ||
                        chnInfos[a0].Table1[a1] >= 0x40
                    )
                        return -0x110;
                }
            }

            return rs;
        }

        int DecodeTable2(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            if (chnInfos[0].JointChnInfo.NumBandSplitedUsed != 0) //
            {
                chnInfos[0].JointChnInfo.Var90 = (uint) mbr0.GetWithI32Buffer(1); //[90] tmp4 [arg1]

                for (uint a0 = 0; a0 < chns; a0++)
                {
                    for (uint a1 = 0; a1 < 0x20; a1++)
                        chnInfos[a0].Table2[a1] = 0;

                    chnInfos[a0].Var1034 = (uint) mbr0.GetWithI32Buffer(1); //[1034] tmp5

                    MAPCDSF_makeTable0CheckTable(chnInfos[a0], chnInfos[a0].CheckTable0); //check

                    if (0 != (rs = MAPCDSF_decodeTable2_func_list0[
                                chnInfos[a0].ChnFlag * 4 + mbr0.GetWithI32Buffer(2)]
                            (mbr0, chnInfos[a0])
                        ))
                        return rs;
                }

                //comp
            }

            return rs;
        }

        int DecodeTable3(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x800; a1++) chnInfos[a0].Table3[a1] = 0;
                for (uint a1 = 0; a1 < 0x5; a1++) chnInfos[a0].Table4[a1] = 0x0F;

                for (uint a1 = 0; a1 < chnInfos[0].JointChnInfo.NumBandSplitedUsed; a1++)
                {
                    if (chnInfos[a0].Table0[a1] == 0)
                    {
                        for (uint a2 = 0; a2 < MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_bind_table1[a1]; a2++)
                            chnInfos[a0].Table3[MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_bind_table0[a1] + a2] = 0;
                    }
                    else
                    {
                        uint atmp0 = 0;

                        if (0 == chnInfos[a0].JointChnInfo.Var90)
                            atmp0 = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_bind_table2[
                                (chnInfos[a0].Var1034 * 7 + chnInfos[a0].Table0[a1]) * 4 +
                                chnInfos[a0].Table2[a1]]; //tmp4 5 6
                        else atmp0 = chnInfos[a0].Table2[a1];

                        MaiAT3PlusCoreDecoderSearchTableDes huffTableNow =
                            MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_huff_table3[
                                (chnInfos[a0].Var1034 * 8 + atmp0) * 7 + chnInfos[a0].Table0[a1]]; //tmp5 6

                        MAPCDSF_decodeTable3Sub0(mbr0,
                            chnInfos[a0].Table3
                                .GetPointer((int) MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_bind_table0[a1]),
                            MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_bind_table1[a1], huffTableNow);
                    }
                }

                if (chnInfos[0].JointChnInfo.NumBandSplitedUsed > 2)
                {
                    for (uint a1 = 0;
                        a1 < (uint) (MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_band_num_table1[
                                         MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_band_num_table0[
                                             chnInfos[0].JointChnInfo.NumBandSplitedUsed] + 1] + 1);
                        a1++)
                    {
                        chnInfos[a0].Table4[a1] = (uint) mbr0.GetWithI32Buffer(4);
                    }
                }
            }

            if (chns == 2)
            {
                MAPCDSF_readPackTable0(mbr0, chnInfos[0].JointChnInfo.Table48,
                    chnInfos[0].JointChnInfo.NumBandUsed);
                MAPCDSF_readPackTable0(mbr0, chnInfos[0].JointChnInfo.Table00,
                    chnInfos[0].JointChnInfo.NumBandUsed);
            }


            return rs;
        }

        int DecodeAcc2Pre(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                MAPCDSF_readPackTable0(mbr0, chnInfos[a0].AccDataNow.Acc,
                    chnInfos[0].JointChnInfo.NumBandDeclared);
            }

            return rs;
        }

        int DecodeAcc2Main(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                chnInfos[a0].AccDataNow.TableMemset();

                uint uk1B444 = (uint) mbr0.GetWithI32Buffer(1);

                if (uk1B444 != 0)
                {
                    chnInfos[a0].Uk1B450 = (uint) mbr0.GetWithI32Buffer(4) + 1;
                    uint uk1B448 = (uint) mbr0.GetWithI32Buffer(1);

                    uint uk1B44C = chnInfos[a0].Uk1B450;
                    if (uk1B448 != 0) uk1B44C = (uint) mbr0.GetWithI32Buffer(4) + 1;

                    //call 478200
                    if ((rs = MAPCDSF_decodeACC2MainSub0(mbr0, chnInfos[a0])) != 0)
                        break;

                    //call 478270
                    if ((rs = MAPCDSF_decodeACC2MainSub1(mbr0, chnInfos[a0])) != 0)
                        break;

                    //call 478330
                    if ((rs = MAPCDSF_decodeACC2MainSub2(mbr0, chnInfos[a0])) != 0)
                        break;

                    if (uk1B448 != 0)
                    {
                        for (uint b0 = chnInfos[a0].Uk1B450; b0 < uk1B44C; b0++)
                        {
                            chnInfos[a0].AccDataNow.Table[b0].NumAcc =
                                chnInfos[a0].AccDataNow.Table[b0 - 1].NumAcc;
                            for (uint b1 = 0; b1 < (uint) chnInfos[a0].AccDataNow.Table[b0].NumAcc; b1++)
                            {
                                chnInfos[a0].AccDataNow.Table[b0].Data1[b1] =
                                    chnInfos[a0].AccDataNow.Table[b0 - 1].Data1[b1];
                                chnInfos[a0].AccDataNow.Table[b0].Data0[b1] =
                                    chnInfos[a0].AccDataNow.Table[b0 - 1].Data0[b1];
                            }
                        }
                    }
                }
                else
                {
                    uint uk1B44C = 0;
                }
            }

            return rs;
        }

        //extern MaiAT3PlusCoreDecoderSearchTableDes[] MAPCDSD_huff_table_global_11;

        static void Mai_memcpy<T>(out T dst, ref T src)
        {
            dst = src;
        }

        static int DecodeAcc6Inner(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[]chnInfos, uint chns)
        {
            int rs = 0;

            if (chns == 2)
            {
                chnInfos[0].AccTableNow.Inner.TableUnk0 = new MaiAt3PlusCoreDecoderPackTable0();
                chnInfos[0].AccTableNow.Inner.TableUnk1 = new MaiAt3PlusCoreDecoderPackTable0();
                chnInfos[0].AccTableNow.Inner.TableUnk2 = new MaiAt3PlusCoreDecoderPackTable0();
            }

            for (uint a0 = 0; a0 < chns; a0++)
            {
                chnInfos[a0].AccTableNow.TableReset();
                for (uint a1 = 0; a1 < 0x10; a1++)
                {
                    chnInfos[a0].AccTableNow.Table[a1].Unk[7] = 0x20;
                    chnInfos[a0].AccTableNow.Table[a1].Unk[6] = 0;
                }
            }

            chnInfos[0].AccTableNow.Inner.PtrToUseNow =
                chnInfos[0].AccTableNow.Inner.Data;

            chnInfos[0].AccTableNow.Inner.Unk0 =
                mbr0.GetWithI32Buffer(1);

            if (chnInfos[0].AccTableNow.Inner.Unk0 != 0)
            {
                chnInfos[0].AccTableNow.Inner.Unk1 =
                    mbr0.GetWithI32Buffer(1);

                chnInfos[0].AccTableNow.Inner.Unk2 = (int) (
                    MAPCDSF_getHuffValue(MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_huff_table_global_11[0], mbr0)
                    + 1
                );


                if (chns == 2)
                {
                    MAPCDSF_readPackTable0(mbr0, chnInfos[0].AccTableNow.Inner.TableUnk0,
                        (uint) chnInfos[0].AccTableNow.Inner.Unk2);
                    MAPCDSF_readPackTable0(mbr0, chnInfos[0].AccTableNow.Inner.TableUnk2,
                        (uint) chnInfos[0].AccTableNow.Inner.Unk2);
                    MAPCDSF_readPackTable0(mbr0, chnInfos[0].AccTableNow.Inner.TableUnk1,
                        (uint) chnInfos[0].AccTableNow.Inner.Unk2);
                }

                for (uint a0 = 0; a0 < chns; a0++)
                {
                    //call 477e60
                    if ((rs = MAPCDSF_decodeACC6InnerSub0(mbr0, chnInfos[a0])) != 0)
                        break;
                }

                if (chns == 2)
                {
                    for (int a0 = 0; a0 < chnInfos[0].AccTableNow.Inner.Unk2; a0++)
                    {
                        if (chnInfos[0].AccTableNow.Inner.TableUnk0.Data[a0] != 0)
                        {
                            /*for (int a1 = 0; a1 < 0xA; a1++)
                            {
                                chn_infos[1].acc_table_now.table[a0].unk[a1] = 
                                    chn_infos[0].acc_table_now.table[a0].unk[a1];
                                //memcpy?
                            }*/
                            Mai_memcpy(
                                out chnInfos[1].AccTableNow.Table[a0],
                                ref chnInfos[0].AccTableNow.Table[a0]
                            );

                            //left to right acc5 copy 0x10 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x14 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x18 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x1C + 0x28 * a0 0x4 add zumi
                        }

                        if (chnInfos[0].AccTableNow.Inner.TableUnk2.Data[a0] != 0)
                        {
                            //swap?
                            MaiAt3PlusCoreDecoderChnAccTableTable tmpbuf0;

                            Mai_memcpy(
                                out tmpbuf0,
                                ref chnInfos[1].AccTableNow.Table[a0]
                            );

                            Mai_memcpy(
                                out chnInfos[1].AccTableNow.Table[a0],
                                ref chnInfos[0].AccTableNow.Table[a0]
                            );

                            Mai_memcpy(
                                out chnInfos[0].AccTableNow.Table[a0],
                                ref tmpbuf0
                            );
                            /*
                            int tmpbuf0[0xA];
                            for (int a1 = 0; a1 < 0xA; a1++)
                            {
                                tmpbuf0[a1] = *(int*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4];
                            }
                            for (int a1 = 0; a1 < 0xA; a1++)
                            {
                                *(int*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4] = 
                                    *(int*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4];
                            }
                            for (int a1 = 0; a1 < 0xA; a1++)
                            {
                                *(int*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4] = 
                                    tmpbuf0[a1];
                            }*/
                        }
                    }
                }
            }

            return rs;
        }

        int DecodeTailInfo(MaiBitReader mbr0, MaiAt3PlusCoreDecoderChnInfo[] chnInfos, uint chns)
        {
            int rs = 0;

            chnInfos[0].JointChnInfo.Var94 = mbr0.GetWithI32Buffer(1);

            if (chnInfos[0].JointChnInfo.Var94 != 0)
            {
                chnInfos[0].JointChnInfo.Var98 = mbr0.GetWithI32Buffer(4);
                chnInfos[0].JointChnInfo.Var9C = mbr0.GetWithI32Buffer(4);
            }

            return rs;
        }
    }
}