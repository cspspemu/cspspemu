using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle.Formats.audio.At3
{
	public sealed unsafe partial class MaiAT3PlusCoreDecoder
	{
		static readonly uint[] table_tmp0_o = { 0x00000000, 0x3F3F7E00, 0x3EE5CC00, 0x3EA42400, 0x3E50E600, 0x3E193200, 0x3D944200, 0x3D11E600 };
		static readonly uint[] table_tmp1_o = { 0x3CE42A00, 0x3D0FBC00, 0x3D351800, 0x3D642A00, 0x3D8FBC00, 0x3DB51800, 0x3DE42A00, 0x3E0FBC00, 0x3E351800, 0x3E642A00, 0x3E8FBC00, 0x3EB51800, 0x3EE42A00, 0x3F0FBC00, 0x3F351800, 0x3F642A00, 0x3F8FBC00, 0x3FB51800, 0x3FE42A00, 0x400FBC00, 0x40351800, 0x40642A00, 0x408FBC00, 0x40B51800, 0x40E42A00, 0x410FBC00, 0x41351800, 0x41642A00, 0x418FBC00, 0x41B51800, 0x41E42A00, 0x420FBC00, 0x42351800, 0x42642A00, 0x428FBC00, 0x42B51800, 0x42E42A00, 0x430FBC00, 0x43351800, 0x43642A00, 0x438FBC00, 0x43B51800, 0x43E42A00, 0x440FBC00, 0x44351800, 0x44642A00, 0x448FBC00, 0x44B51800, 0x44E42A00, 0x450FBC00, 0x45351800, 0x45642A00, 0x458FBC00, 0x45B51800, 0x45E42A00, 0x460FBC00, 0x46351800, 0x46642A00, 0x468FBC00, 0x46B51800, 0x46E42A00, 0x470FBC00, 0x47351800, 0x47642A00 };
		static readonly uint[] table_search0_o = { 0, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80, 0xa0, 0xc0, 0xe0, 0x100, 0x120, 0x140, 0x160, 0x180, 0x1c0, 0x200, 0x240, 0x280, 0x2c0, 0x300, 0x380, 0x400, 0x480, 0x500, 0x580, 0x600, 0x680, 0x700, 0x780, 0x800 };
		static readonly uint[] table_search1_o = { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 };
		static readonly byte[] table_tmp2 = { 0x00, 0x01, 0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04 };
		static readonly float[] table_tmp3 = { 3.968750f, 3.156250f, 2.500000f, 2.000000f, 1.593750f, 1.250000f, 1.000000f, 0.7812500f, 0.6250000f, 0.5000000f, 0.4062500f, 0.3125000f, 0.2500000f, 0.1875000f, 0.1562500f, 0.0f };
		static readonly int[] MAPCDDF_initMDataTable_table_tmp4 = { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		static readonly byte[] MAPCDDF_initMDataTable_table_tmp5 = { 0x02, 0x08, 0x0C, 0x10, 0x12, 0x14, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F };
		static readonly byte[] MAPCDDF_initMDataTable_table_tmp6 = { 0x08, 0x0C, 0x10, 0x12, 0x14, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20 };
		static readonly byte[] MAPCDDF_initMDataTable_table_tmp7 = { 0x00, 0x08, 0x0C, 0x10, 0x12, 0x14, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20 };

		int MAPCDDF_initMDataTable(MaiAT3PlusCoreDecoderChnInfo[] chn_infos, float** pptablef0, uint chns)
		{
			fixed (uint* _table_tmp0 = table_tmp0_o)
			fixed (uint* _table_tmp1 = table_tmp1_o)
			fixed (uint* table_search0 = table_search0_o)
			fixed (uint* table_search1 = table_search1_o)
			{
				var table_tmp0 = (float*)_table_tmp0;
				var table_tmp1 = (float*)_table_tmp1;

				int rs = 0;
				short* l136 = stackalloc short[0x10];

				{
					short l0 = 0;

					for (uint a00 = 0; a00 < chns; a00++)
					{
						for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_splited_used; a0++)
						{
							l0 += (short)chn_infos[a00].table1[a0];
						}
					}

					l0 &= 0x3fc;

					for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_used; a0++)
					{
						l136[a0] = l0;
						l0 += 0x80;
						l0 &= 0x3fc;
					}
				}

				if (chns == 2)
				{
					for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_splited_used; a0++)
					{
						if (chn_infos[1].table0[a0] == 0)
						{
							if (chn_infos[0].table0[a0] != 0)
							{
								for (uint a1 = 0; a1 < table_search1[a0]; a1++)
								{
									chn_infos[1].table3[table_search0[a0] + a1] =
										chn_infos[0].table3[table_search0[a0] + a1];
								}

								chn_infos[1].table0[a0] = chn_infos[0].table0[a0];
							}
						}
					}
				}

				for (int a00 = 0; a00 < chns; a00++)
				{
					var chn_infos_a00 = chn_infos[a00];
					var pptablef0_a00 = pptablef0[a00];
					fixed (uint* chn_infos_a00_table0 = chn_infos_a00.table0)
					fixed (uint* chn_infos_a00_table1 = chn_infos_a00.table1)
					fixed (short* chn_infos_a00_table3 = chn_infos_a00.table3)
					{
						for (uint a0 = 0; a0 < 0x800; a0++) pptablef0_a00[a0] = 0.0f;

						for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_splited_used; a0++)
						{
							if (chn_infos_a00_table0[a0] > 0)
							{
								for (uint a1 = 0; a1 < table_search1[a0]; a1++)
								{
									pptablef0_a00[table_search0[a0] + a1] = chn_infos_a00_table3[table_search0[a0] + a1] * table_tmp0[chn_infos_a00_table0[a0]] * table_tmp1[chn_infos_a00_table1[a0]];
								}
							}
						}
					}

					for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_used; a0++)
					{
						{
							uint[] mtmp0;

							MaiAT3PlusCoreDecoderChnACCData acc_data1;
							MaiAT3PlusCoreDecoderChnACCData acc_data2;
							if ((chn_infos[0].joint_chn_info.chns != 2) || (0 == chn_infos[0].joint_chn_info.table48.data[a0]))
							{
								mtmp0 = chn_infos[a00].table4;

								acc_data1 = chn_infos[a00].acc_data_old;
								acc_data2 = chn_infos[a00].acc_data_now;
							}
							else
							{
								if (a00 == 0)
								{
									mtmp0 = chn_infos[1].table4;
									acc_data1 = chn_infos[1].acc_data_old;
									acc_data2 = chn_infos[1].acc_data_now;//
								}
								else
								{
									mtmp0 = chn_infos[0].table4;
									acc_data1 = chn_infos[0].acc_data_old;
									acc_data2 = chn_infos[0].acc_data_now;//
								}
							}

							if (table_tmp3[mtmp0[table_tmp2[a0]]] > 0.0f)
							{
								float* l127 = stackalloc float[0x80];
								int arg3_1 = l136[a0];
								for (uint a1 = 0; a1 < 0x80; a1++)
								{
									uint tmp0 = 0x38000000;
									l127[a1] = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_0[arg3_1 & 0x3FF] * (*(float*)&tmp0);
									arg3_1++;
								}

								int rt0 = 0;
								{
									int dl_0;
									if (acc_data2.table[a0].num_acc <= 0) dl_0 = 0;
									else dl_0 = 0 - MAPCDDF_initMDataTable_table_tmp4[acc_data2.table[a0].data1[0]];


									int al_0 = 0;

									for (int a1 = 0; a1 < acc_data1.table[a0].num_acc; a1++)
									{
										if (al_0 < (dl_0 - MAPCDDF_initMDataTable_table_tmp4[acc_data1.table[a0].data1[a1]]))
										{
											al_0 = (dl_0 - MAPCDDF_initMDataTable_table_tmp4[acc_data1.table[a0].data1[a1]]);
										}
									}

									for (int a1 = 0; a1 < acc_data2.table[a0].num_acc; a1++)
									{
										if (al_0 < (0 - MAPCDDF_initMDataTable_table_tmp4[acc_data2.table[a0].data1[a1]]))
										{
											al_0 = (0 - MAPCDDF_initMDataTable_table_tmp4[acc_data2.table[a0].data1[a1]]);
										}
									}

									rt0 = al_0;
								}

								int dtmp1 = (rt0 < 0) ? 0 : (1 << rt0);
								float l128 = table_tmp3[mtmp0[table_tmp2[a0]]] / dtmp1;

								for (uint a1 = MAPCDDF_initMDataTable_table_tmp5[a0]; a1 < MAPCDDF_initMDataTable_table_tmp6[a0]; a1++)
								{
									if (chn_infos[a00].table0[a1] > 0)
									{
										float l129 = table_tmp0[chn_infos[a00].table0[a1]]
											* table_tmp1[chn_infos[a00].table1[a1]]
											* l128
											/ (1 << (int)chn_infos[a00].table0[a1]);

										for (int a2 = (int)table_search0[a1], a3 = 0;
											a2 < table_search0[a1 + 1];
											a2++, a3++)
										{
											pptablef0[a00][a2] += l129 * l127[a3];
										}
									}
								}

							}
						}

					}

				}


				if (chns == 2)
				{
					for (uint a0 = 0; a0 < chn_infos[0].joint_chn_info.num_band_used; a0++)
					{
						if (chn_infos[0].joint_chn_info.table48.data[a0] == 1)
						{
							float* l128 = stackalloc float[0x80];
							for (uint a1 = 0; a1 < 0x80; a1++) l128[a1] = pptablef0[0][a0 * 0x80 + a1];
							for (uint a1 = 0; a1 < 0x80; a1++) pptablef0[0][a0 * 0x80 + a1] = pptablef0[1][a0 * 0x80 + a1];
							for (uint a1 = 0; a1 < 0x80; a1++) pptablef0[1][a0 * 0x80 + a1] = l128[a1];
						}
						if (chn_infos[0].joint_chn_info.table00.data[a0] == 1)
						{
							for (uint a1 = MAPCDDF_initMDataTable_table_tmp7[a0]; a1 < MAPCDDF_initMDataTable_table_tmp7[a0 + 1]; a1++)
							{
								for (int a2 = (int)table_search0[a1]; a2 < table_search0[a1 + 1]; a2++)
								{
									pptablef0[1][a2] *= (-1.0f);
								}
							}
						}
					}
				}

				if (chn_infos[0].joint_chn_info.var118 == 1)
				{
					for (uint a0 = 0; a0 < 0x800; a0++) pptablef0[0][a0] = 0.0f;
					for (uint a0 = 0; a0 < 0x800; a0++) pptablef0[1][a0] = 0.0f;
				}

				return rs;
			}
		}

		static readonly byte[] _makeSL128_table_tmp0_o = { 0x00, 0x38, 0x18, 0x3F, 0x00, 0x04, 0x35, 0x3F, 0x00, 0x44, 0x57, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x38, 0x98, 0x3F, 0x00, 0x04, 0xB5, 0x3F, 0x00, 0x44, 0xD7, 0x3F, 0x00, 0x00, 0x00, 0x40, 0x00, 0x38, 0x18, 0x40, 0x00, 0x04, 0x35, 0x40, 0x00, 0x44, 0x57, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x38, 0x98, 0x40, 0x00, 0x04, 0xB5, 0x40, 0x00, 0x44, 0xD7, 0x40, 0x00, 0x00, 0x00, 0x41, 0x00, 0x38, 0x18, 0x41, 0x00, 0x04, 0x35, 0x41, 0x00, 0x44, 0x57, 0x41, 0x00, 0x00, 0x80, 0x41, 0x00, 0x38, 0x98, 0x41, 0x00, 0x04, 0xB5, 0x41, 0x00, 0x44, 0xD7, 0x41, 0x00, 0x00, 0x00, 0x42, 0x00, 0x38, 0x18, 0x42, 0x00, 0x04, 0x35, 0x42, 0x00, 0x44, 0x57, 0x42, 0x00, 0x00, 0x80, 0x42, 0x00, 0x38, 0x98, 0x42, 0x00, 0x04, 0xB5, 0x42, 0x00, 0x44, 0xD7, 0x42, 0x00, 0x00, 0x00, 0x43, 0x00, 0x38, 0x18, 0x43, 0x00, 0x04, 0x35, 0x43, 0x00, 0x44, 0x57, 0x43, 0x00, 0x00, 0x80, 0x43, 0x00, 0x38, 0x98, 0x43, 0x00, 0x04, 0xB5, 0x43, 0x00, 0x44, 0xD7, 0x43, 0x00, 0x00, 0x00, 0x44, 0x00, 0x38, 0x18, 0x44, 0x00, 0x04, 0x35, 0x44, 0x00, 0x44, 0x57, 0x44, 0x00, 0x00, 0x80, 0x44, 0x00, 0x38, 0x98, 0x44, 0x00, 0x04, 0xB5, 0x44, 0x00, 0x44, 0xD7, 0x44, 0x00, 0x00, 0x00, 0x45, 0x00, 0x38, 0x18, 0x45, 0x00, 0x04, 0x35, 0x45, 0x00, 0x44, 0x57, 0x45, 0x00, 0x00, 0x80, 0x45, 0x00, 0x38, 0x98, 0x45, 0x00, 0x04, 0xB5, 0x45, 0x00, 0x44, 0xD7, 0x45, 0x00, 0x00, 0x00, 0x46, 0x00, 0x38, 0x18, 0x46, 0x00, 0x04, 0x35, 0x46, 0x00, 0x44, 0x57, 0x46, 0x00, 0x00, 0x80, 0x46, 0x00, 0x38, 0x98, 0x46, 0x00, 0x04, 0xB5, 0x46, 0x00, 0x44, 0xD7, 0x46, 0x00, 0x00, 0x00, 0x47 };
		static readonly byte[] _makeSL128_table_tmp1_o = { 0x00, 0x5C, 0x87, 0x3D, 0x00, 0x5C, 0x07, 0x3E, 0x00, 0x0A, 0x4B, 0x3E, 0x00, 0x5C, 0x87, 0x3E, 0x00, 0x34, 0xA9, 0x3E, 0x00, 0x0A, 0xCB, 0x3E, 0x00, 0xE2, 0xEC, 0x3E, 0x00, 0x5C, 0x07, 0x3F, 0x00, 0x48, 0x18, 0x3F, 0x00, 0x34, 0x29, 0x3F, 0x00, 0x1E, 0x3A, 0x3F, 0x00, 0x0A, 0x4B, 0x3F, 0x00, 0xF6, 0x5B, 0x3F, 0x00, 0xE2, 0x6C, 0x3F, 0x00, 0xCC, 0x7D, 0x3F, 0x00, 0x5C, 0x87, 0x3F };

		static int makeSL128(MaiAT3PlusCoreDecoderChnACCTableTable acc, float* lo, int num0, int num1, int acc_4, int acc_360, int chn)
		{
			fixed (byte* table_tmp0_o = _makeSL128_table_tmp0_o)
			fixed (byte* table_tmp1_o = _makeSL128_table_tmp1_o)
			{
				float* table_tmp0 = (float*)table_tmp0_o;
				float* table_tmp1 = (float*)table_tmp1_o;

				for (int a0 = 0; a0 < num1; a0++) lo[a0] = 0.0f;

				if (acc_4 == 0)
				{
					for (int a0 = 0; a0 < acc.num_uk; a0++)
					{
						float l259 = table_tmp0[acc.ptr0[a0].unk0]
							* table_tmp1[acc.ptr0[a0].unk1];

						int atmp0 = acc.ptr0[a0].unk2;
						atmp0 &= 0x1F;
						atmp0 <<= 6;

						int ctmp0 = num0 - 0x80;
						ctmp0 *= acc.ptr0[a0].unk3;
						ctmp0 += atmp0;
						ctmp0 &= 0x7FF;

						for (int a1 = 0; a1 < num1; a1++)
						{
							lo[a1] += l259 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_3[ctmp0];
							ctmp0 += acc.ptr0[a0].unk3;
							ctmp0 &= 0x7FF;
						}
					}
				}
				else
				{
					for (int a0 = 0; a0 < acc.num_uk; a0++)
					{
						float l259 = table_tmp0[acc.ptr0[a0].unk0];

						int atmp0 = acc.ptr0[a0].unk2;
						atmp0 &= 0x1F;
						atmp0 <<= 6;

						int ctmp0 = num0 - 0x80;
						ctmp0 *= acc.ptr0[a0].unk3;
						ctmp0 += atmp0;
						ctmp0 &= 0x7FF;

						for (int a1 = 0; a1 < num1; a1++)
						{
							lo[a1] += l259 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_3[ctmp0];
							ctmp0 += acc.ptr0[a0].unk3;
							ctmp0 &= 0x7FF;
						}
					}
				}

				if ((chn != 0) && (acc_360 != 0))
				{
					for (int a1 = 0; a1 < num1; a1++)
					{
						lo[a1] *= -1.0f;
					}
				}

				float* l256 = stackalloc float[0x100];

				{
					for (int a0 = 0; a0 < 0x100; a0++) l256[a0] = 1.0f;
					if (acc.unk[0] != 0)
					{
						for (int a0 = 0; a0 < acc.unk[2]; a0++) l256[a0] = 0.0f;
						l256[acc.unk[2]] = 0.0f;
						l256[acc.unk[2] + 1] = 0.1464233f;
						l256[acc.unk[2] + 2] = 0.5f;
						l256[acc.unk[2] + 3] = 0.8535767f;
					}
					if (acc.unk[1] != 0)
					{
						l256[acc.unk[3] - 4] = 0.8535767f;
						l256[acc.unk[3] - 3] = 0.5f;
						l256[acc.unk[3] - 2] = 0.1464233f;
						l256[acc.unk[3] - 1] = 0.0f;
						for (int a0 = acc.unk[3]; a0 < 0x100; a0++) l256[a0] = 0.0f;
					}
				}

				for (int a1 = 0; a1 < num1; a1++)
				{
					lo[a1] *= l256[num0 + a1];
				}

				return 0;
			}
		}

		int MAPCDDF_makeL128(MaiAT3PlusCoreDecoderChnACCTableTable acc5, int acc5_4, int acc5_360, 
				MaiAT3PlusCoreDecoderChnACCTableTable acc6, int acc6_4, int acc6_360, 
				float* l128, int chn)
		{
			int rs = 0;

			var lo256 = stackalloc float[0x100];
			var lo128 = &lo256[0x80];

			makeSL128(acc5, lo128, 0x80, 0x80, acc5_4, acc5_360, chn);
			makeSL128(acc6, lo256, 0, 0x80, acc6_4, acc6_360, chn);

			if ( (acc5.num_uk > 0) && (acc6.num_uk > 0) && (acc5.unk[3] - 0x80 >= acc6.unk[2]) )
			{
				for (int a0 = 0; a0 < 0x80; a0++)
				{
					lo128[a0] *= MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_5[a0];
					lo256[a0] *= MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_4[a0];
				}
			}
			else
			{
				if (acc5.num_uk > 0)
				{
					if (acc5.unk[1] == 0)
					{
						for (int a0 = 0; a0 < 0x80; a0++)
						{
							lo128[a0] *= MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_5[a0];
						}
					}
				}
				if (acc6.num_uk > 0)
				{
					if (acc6.unk[0] == 0)
					{
						for (int a0 = 0; a0 < 0x80; a0++)
							lo256[a0] *= MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_4[a0];
					}
				}
			}

			for (int a0 = 0; a0 < 0x80; a0++)
			{
				l128[a0] = lo256[a0] + lo128[a0];
			}

			return rs;
		}

		static readonly float[] table_5b3ed0 = { 1.672582e-05f, 2.934863e-05f, 3.487291e-05f, 7.567905e-06f };
		static readonly float[] table_5b3ee0 = { -8.893659e-06f, -1.682840e-05f, -1.481585e-05f, 0.0003469118f };
		static readonly float[] table_5b3d50 = { -1.866759e-05f, -2.579335e-05f, -1.344173e-05f, -1.420818e-06f };
		static readonly float[] table_5b3d60 = { 1.032399e-06f, 1.129874e-06f, 4.051321e-07f, 0.0003365371f };

		static readonly ushort[] search_table_tmp0 = { 0x0003, 0x0043, 0x0063, 0x0023, 0x0033, 0x0073, 0x0053, 0x0013, 0x001B, 0x005B, 0x007B, 0x003B, 0x002B, 0x006B, 0x004B, 0x000B, 0x000F, 0x004F, 0x006F, 0x002F, 0x003F, 0x007F, 0x005F, 0x001F, 0x0017, 0x0057, 0x0077, 0x0037, 0x0027, 0x0067, 0x0047, 0x0007, 0x0005, 0x0045, 0x0065, 0x0025, 0x0035, 0x0075, 0x0055, 0x0015, 0x001D, 0x005D, 0x007D, 0x003D, 0x002D, 0x006D, 0x004D, 0x000D, 0x0009, 0x0049, 0x0069, 0x0029, 0x0039, 0x0079, 0x0059, 0x0019, 0x0011, 0x0051, 0x0071, 0x0031, 0x0021, 0x0061, 0x0041, 0x0001 };
		static readonly ushort[] search_table_tmp1 = { 0x0000, 0x0040, 0x0060, 0x0020, 0x0030, 0x0070, 0x0050, 0x0010, 0x0018, 0x0058, 0x0078, 0x0038, 0x0028, 0x0068, 0x0048, 0x0008, 0x000C, 0x004C, 0x006C, 0x002C, 0x003C, 0x007C, 0x005C, 0x001C, 0x0014, 0x0054, 0x0074, 0x0034, 0x0024, 0x0064, 0x0044, 0x0004, 0x0006, 0x0046, 0x0066, 0x0026, 0x0036, 0x0076, 0x0056, 0x0016, 0x001E, 0x005E, 0x007E, 0x003E, 0x002E, 0x006E, 0x004E, 0x000E, 0x000A, 0x004A, 0x006A, 0x002A, 0x003A, 0x007A, 0x005A, 0x001A, 0x0012, 0x0052, 0x0072, 0x0032, 0x0022, 0x0062, 0x0042, 0x0002 };
		static readonly ushort[] search_table_tmp2 = { 0x0000, 0x0040, 0x0060, 0x0020, 0x0030, 0x0070, 0x0050, 0x0010, 0x0018, 0x0058, 0x0078, 0x0038, 0x0028, 0x0068, 0x0048, 0x0008, 0x000C, 0x004C, 0x006C, 0x002C, 0x003C, 0x007C, 0x005C, 0x001C, 0x0014, 0x0054, 0x0074, 0x0034, 0x0024, 0x0064, 0x0044, 0x0004, 0x0006, 0x0046, 0x0066, 0x0026, 0x0036, 0x0076, 0x0056, 0x0016, 0x001E, 0x005E, 0x007E, 0x003E, 0x002E, 0x006E, 0x004E, 0x000E, 0x000A, 0x004A, 0x006A, 0x002A, 0x003A, 0x007A, 0x005A, 0x001A, 0x0012, 0x0052, 0x0072, 0x0032, 0x0022, 0x0062, 0x0042, 0x0002, 0x0003, 0x0043, 0x0063, 0x0023, 0x0033, 0x0073, 0x0053, 0x0013, 0x001B, 0x005B, 0x007B, 0x003B, 0x002B, 0x006B, 0x004B, 0x000B, 0x000F, 0x004F, 0x006F, 0x002F, 0x003F, 0x007F, 0x005F, 0x001F, 0x0017, 0x0057, 0x0077, 0x0037, 0x0027, 0x0067, 0x0047, 0x0007, 0x0005, 0x0045, 0x0065, 0x0025, 0x0035, 0x0075, 0x0055, 0x0015, 0x001D, 0x005D, 0x007D, 0x003D, 0x002D, 0x006D, 0x004D, 0x000D, 0x0009, 0x0049, 0x0069, 0x0029, 0x0039, 0x0079, 0x0059, 0x0019, 0x0011, 0x0051, 0x0071, 0x0031, 0x0021, 0x0061, 0x0041, 0x0001 };

		static readonly short[] table_59e660 = { 579, 215, 556, 247, 561, 854, 237, 658, 10, 827, 660, 396, 9, 19, 146, 221 };
		static readonly float[] table_626a90 = { 1.997591f, 1.978353f, 1.940063f, 1.883088f, 1.807979f, 1.715457f, 1.606415f, 1.481902f, 1.343118f, 1.191399f, 1.028206f, 0.8551102f, 0.6737797f, 0.4859604f, 0.2934610f, 0.09813535f };
		static readonly float[] table_626af0 = { 1.961571f, 1.662939f, 1.111140f, 0.3901806f };
		static readonly float[] table_626ae0 = { 1.268787f, 0.9427935f, 0.5805694f, 0.1960343f };
		static readonly float[] table_626ad0 = { 1.990369f, 1.913881f, 1.763843f, 1.546021f };
		static readonly uint[] table_5b4050 = { 0x00000000, 0x00000010, 0x00000020, 0x00000008, 0x00000018, 0x00000028, 0x00000018, 0x00000028, 0x00000008, 0x00000007, 0x00000017, 0x00000027, 0x0000001F, 0x0000002F, 0x0000000F, 0x0000000F, 0x0000001F, 0x0000002F, 0x00000020, 0x00000000, 0x00000010, 0x00000000 };

		static readonly int[] table_tmp4 = { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		//static readonly int[] table_tmp4 = { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

		const float pi = 3.14159265f;

		public int decodeStream(uint chns)
		{
			int rs = 0;

			float* mdata0_0 = stackalloc float[0x800];
			float* mdata0_1 = stackalloc float[0x800];
			float** mdata0_table = stackalloc float*[2];
			mdata0_table[0] = mdata0_0;
			mdata0_table[1] = mdata0_1;

			{
				MAPCDDF_initMDataTable(chn_info, mdata0_table, chns);//

				if (true)
				{
					for (int a00 = 0; a00 < chns; a00++)
					{
						
						float** l2080 = stackalloc float*[0x10];
						float* l2048 = stackalloc float[0x800];

						for (int a0 = 0; a0 < 0x800; a0++) l2048[a0] = 0.0f;

						for (int a0 = 0; a0 < 0x10; a0++)
						{
							l2080[a0] = &l2048[a0 * 0x80];
						}

						for (int a0 = 0; a0 < chn_info[0].joint_chn_info.num_band_declared; a0++)
						{
							chn_info[a00].acc_data_now.table[a0].acc_now = 
								chn_info[a00].acc_data_now.acc.data[a0];
						}

						{
							float* l512 = stackalloc float[0x100];
							float* l256 = stackalloc float[0x100];

							for (int a0 = 0; a0 < chn_info[0].joint_chn_info.num_band_declared; a0++)
							{
								ManagedPointer<float> table_shita = null;
						
								if ( chn_info[a00].acc_data_old.table[a0].acc_now
									== 0)
								{
									if ( chn_info[a00].acc_data_now.table[a0].acc_now
										== 0)
									{
										table_shita = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_1.GetPointer(0x000);
									}
									else
									{
										table_shita = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_1.GetPointer(0x100);
									}
								}
								else
								{
									if ( chn_info[a00].acc_data_now.table[a0].acc_now
										== 0)
									{
										table_shita = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_1.GetPointer(0x200);
									}
									else
									{
										table_shita = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_1.GetPointer(0x300);
									}
								}

								{
									float* stmp40 = stackalloc float[0x80];

									for (uint a1 = 0; a1 < 0x40; a1++)
									{
										if ((a0 & 1) != 0)
										{
											stmp40[a1 * 2] = mdata0_table[a00][0x80 * a0 + a1 * 2] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin0[a1]
												+ mdata0_table[a00][0x80 * a0 + 0x80 - a1 * 2 - 1] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos0[a1];
											stmp40[a1 * 2 + 1] = mdata0_table[a00][0x80 * a0 + 0x80 - a1 * 2 - 1] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin0[a1]
												- mdata0_table[a00][0x80 * a0 + a1 * 2] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos0[a1];

										}
										else
										{
											stmp40[a1 * 2] = mdata0_table[a00][0x80 * a0 + 0x80 - a1 * 2 - 1] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin0[a1]
												+ mdata0_table[a00][0x80 * a0 + a1 * 2] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos0[a1];
											stmp40[a1 * 2 + 1] = mdata0_table[a00][0x80 * a0 + a1 * 2] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin0[a1]
												- mdata0_table[a00][0x80 * a0 + 0x80 - a1 * 2 - 1] * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos0[a1];
										}
									}

									for (uint a2 = 0; a2 < 6; a2++)
									{
										for (uint a3 = 0; a3 < (1 << (int)a2); a3++)
										{
											uint base0 = (uint)((0x40 >> (int)a2));
											uint base1 = (uint)((0x80 >> (int)a2) * a3);
											for (uint a1 = 0; a1 < (0x20 >> (int)a2); a1++)
											{
												float ftmp0 = stmp40[base1 + a1 * 2 + 1] + stmp40[base1 + a1 * 2 + 1 + base0];
												float ftmp1 = stmp40[base1 + a1 * 2] - stmp40[base1 + a1 * 2 + base0];
												float ftmp2 = stmp40[base1 + a1 * 2 + 1] - stmp40[base1 + a1 * 2 + 1 + base0];
												float ftmp3 = stmp40[base1 + a1 * 2] + stmp40[base1 + a1 * 2 + base0];

												stmp40[base1 + a1 * 2] = ftmp3;
												stmp40[base1 + a1 * 2 + 1] = ftmp0;

												stmp40[base1 + a1 * 2 + base0] = ftmp2 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin1[a2 * 0x20 + a1]
													+ ftmp1 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos1[a2 * 0x20 + a1];
												stmp40[base1 + a1 * 2 + 1 + base0] = ftmp1 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_sin1[a2 * 0x20 + a1]
													- ftmp2 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_cos1[a2 * 0x20 + a1];
											}
										}
									}

									for (uint a1 = 0; a1 < 0x40; a1++)
									{
										l512[a1] = stmp40[search_table_tmp0[a1]] * table_shita[a1];
										l512[0xC0 + a1] = (-1.0f) * stmp40[search_table_tmp1[a1]] * table_shita[0xc0 + a1];
									}

									for (uint a1 = 0; a1 < 0x80; a1++)
									{
										l512[0x40 + a1] = (-1.0f) * stmp40[search_table_tmp2[0x80 - a1 - 1]] * table_shita[0x40 + a1];
									}

								}

								if ( (chn_info[a00].acc_data_old.table[a0].num_acc != 0) || (chn_info[a00].acc_data_now.table[a0].num_acc != 0) )
								{
									int rt_3 = 0;

									{
										int* l63 = stackalloc int[0x40];
										for (int a1 = 0; a1 < 0x40; a1++) l63[a1] = 0;

										int dtmp0 = 0;
										for (int a1 = 0; a1 < chn_info[a00].acc_data_now.table[a0].num_acc; a1++)
										{
											if (dtmp0 <= 0x20 + 
												chn_info[a00].acc_data_now.table[a0].data0[a1] )
											{
												for (int a2 = dtmp0; a2 < 0x20 + 0x1 + 
													chn_info[a00].acc_data_now.table[a0].data0[a1];
													a2++)
												{
													l63[dtmp0++] = table_tmp4[chn_info[a00].acc_data_now.table[a0].data1[a1]];
												}
											
											}
										}

										int atmp5 = 0;
										for (int a1 = 0; a1 < chn_info[a00].acc_data_old.table[a0].num_acc; a1++)
										{

											for (int a2 = atmp5; a2 < 0x1 + 
													chn_info[a00].acc_data_old.table[a0].data0[a1];
													a2++)
											{
												l63[atmp5++] += table_tmp4[chn_info[a00].acc_data_old.table[a0].data1[a1]];
											}
										}

										float l64 = 0.0f;
										int ctmp1 = 0 - l63[0x3F];
										if (ctmp1 >= 0)
										{
											l64 = (float)(1 << ctmp1);
										}
										else
										{
											l64 = 1.0f / (1 << l63[0x3F]);
										}

										int itmp1 = 0;
										int dtmp1 = 0xFF;
										int atmp1 = 0x100;
										for (int a1 = 0x3f; a1 >= 0; a1--)
										{
											if (l63[a1] == itmp1)
											{
												l256[dtmp1--] = l64;
												l256[dtmp1--] = l64;
												l256[dtmp1--] = l64;
											}
											else
											{
												l64 = 0.0f;
												if (atmp1 == 0x100)
												{
													atmp1 = dtmp1;
												}
										
												if (l63[a1] < itmp1)
												{
													int ctmp2 = 0 - l63[a1];
													int itmp2 = itmp1 - l63[a1] - 1;

													if (ctmp2 >= 0)
													{
														l64 = (float)(1 << ctmp2);
													}
													else
													{
														l64 = 1.0f / (1 << l63[a1]);
													}

													itmp2 *= 3;

													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[itmp2];
													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[itmp2 + 1];
													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[itmp2 + 2];
												}
												else
												{
													int ctmp2 = 0 - itmp1;
													int btmp_2 = l63[a1] - itmp1 - 1;

													if (ctmp2 >= 0)
													{
														l64 = (float)(1 << ctmp2);
													}
													else
													{
														l64 = 1.0f / (1 << itmp1);
													}

													btmp_2 *= 3;

													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[btmp_2 + 2];
													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[btmp_2 + 1];
													l256[dtmp1--] = l64 * MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_2[btmp_2];

													ctmp2 = 0 - l63[a1];
													if (ctmp2 >= 0)
													{
														l64 = (float)(1 << ctmp2);
													}
													else
													{
														l64 = 1.0f / (1 << l63[a1]);
													}
												}

												itmp1 = l63[a1];
											}

											l256[dtmp1--] = l64;
										}

										rt_3 = atmp1;
										if (rt_3 == 0x100) rt_3 = 0xFF;
									}


									for (int a1 = 0x80; a1 <= rt_3; a1++)
									{
										l512[a1] *= l256[a1];
									}

									for (int a1 = 0; a1 < 0x80; a1++)
									{
										l2080[a0][a1] = kyou_buf[a00][a0 * 0x80 + a1] + l512[a1] * l256[a1];
									}
								}
								else
								{
									{
										for (int a1 = 0; a1 < 0x80; a1++)
										{
											l2080[a0][a1] = kyou_buf[a00][a0 * 0x80 + a1] + l512[a1];
										}
									}
								}

								for (int a1 = 0; a1 < 0x80; a1++)
								{
									kyou_buf[a00][a0 * 0x80 + a1] = l512[0x80 + a1];
								}
							}

							for (int a0 = (int)chn_info[0].joint_chn_info.num_band_declared; a0 < 0x10; a0++)
							{
								for (int a1 = 0; a1 < 0x80; a1++)
								{
									kyou_buf[a00][a0 * 0x80 + a1] = 0.0f;
								}
							}

						}

						{
							if ( (chn_info[a00].acc_table_old.inner.unk0 != 0) || (chn_info[a00].acc_table_now.inner.unk0 != 0) )
							{
								for (int a0 = 0; a0 < 0x10; a0++)
								{
									{
										if ( (chn_info[a00].acc_table_now.table[a0].unk[4] != 0) && 
											( (chn_info[a00].acc_table_now.table[a0].unk[6])
											< (chn_info[a00].acc_table_now.table[a0].unk[7]) )
											)
										{
											chn_info[a00].acc_table_now.table[a0].unk[2] = 
												chn_info[a00].acc_table_now.table[a0].unk[6]
												* 4
												+ 0x80;

											chn_info[a00].acc_table_now.table[a0].unk[0] = 1;
										}
										else
										{
											if ((chn_info[a00].acc_table_old.table[a0].unk[4]) == 0)
											{
												chn_info[a00].acc_table_now.table[a0].unk[2] = 0;
												chn_info[a00].acc_table_now.table[a0].unk[0] = 0;
											}
											else
											{
												chn_info[a00].acc_table_now.table[a0].unk[2] = 
													chn_info[a00].acc_table_old.table[a0].unk[6]
													* 4;
												chn_info[a00].acc_table_now.table[a0].unk[0] = 1;
											}
										}

										if ( (chn_info[a00].acc_table_old.table[a0].unk[5] != 0) && (
											(chn_info[a00].acc_table_now.table[a0].unk[2]) <= (4 * (chn_info[a00].acc_table_old.table[a0].unk[7]) )
											) )
										{
											chn_info[a00].acc_table_now.table[a0].unk[3] = 
												4 * (chn_info[a00].acc_table_old.table[a0].unk[7]);

											chn_info[a00].acc_table_now.table[a0].unk[1] = 1;
										}
										else
										{
											if (chn_info[a00].acc_table_now.table[a0].unk[5] != 0)
											{
												chn_info[a00].acc_table_now.table[a0].unk[3] = 
													chn_info[a00].acc_table_now.table[a0].unk[7]
													* 4
													+ 0x80;

												chn_info[a00].acc_table_now.table[a0].unk[1] = 1;
											}
											else
											{
												chn_info[a00].acc_table_now.table[a0].unk[3] = 0x100;

												chn_info[a00].acc_table_now.table[a0].unk[1] = 0;
											}
										}

										chn_info[a00].acc_table_now.table[a0].unk[3] += 4;
										if ( (chn_info[a00].acc_table_now.table[a0].unk[3])
											> 0x100)
										{
											chn_info[a00].acc_table_now.table[a0].unk[3] = 
												0x100;
										}


									}

									if ( (chn_info[a00].acc_table_now.table[a0].num_uk != 0) || (chn_info[a00].acc_table_old.table[a0].num_uk != 0) )
									{
										float* l128 = stackalloc float[0x80];

										{
											MAPCDDF_makeL128(
												chn_info[a00].acc_table_old.table[a0],
												chn_info[a00].acc_table_old.inner.unk1,
												chn_info[a00].acc_table_old.inner.table_unk1.data[a0],
												chn_info[a00].acc_table_now.table[a0],
												chn_info[a00].acc_table_now.inner.unk1,
												chn_info[a00].acc_table_now.inner.table_unk1.data[a0],
												l128,
												a00
											);
										}

										{
											for (int a1 = 0; a1 < 0x80; a1++)
											{
												l2080[a0][a1] += l128[a1];
											}
										}

									}

								}
							}
						}

						{
							if (chn_info[0].joint_chn_info.var94 != 0)
							{
								float tmpf0 = (float)(1 << chn_info[0].joint_chn_info.var98);
								for (int a0 = 0; a0 < 0x10; a0++)
								{
									for (int a1 = 0; a1 < 0x80; a1++)
									{
										// @TODO: Check
										int index1 = table_59e660[chn_info[0].joint_chn_info.var9c];
										//int index1 = table_59e660[chn_info[0].joint_chn_info.var9c % table_59e660.Length];
										l2080[a0][a1] += MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_0[index1 + a1] * tmpf0;
									}
									chn_info[0].joint_chn_info.var9c++;
								}
							}
						}

						{
							float[] buf_ctmp1 = syn_buf[a00];

							float[] dst0 = dst_buf[a00];
							for (int a0 = 0; a0 < 0x800; a0++) dst0[a0] = 0.0f;

							for (int a0 = 0; a0 < 0x80; a0++)
							{
								float* stmp140 = stackalloc float[0x10];
								float* stmp30 = stackalloc float[0x100];

								ulong atmp0 = 0x55555556;
								atmp0 *= (ulong)(a0 + c900);
								uint atmp1 = (uint)(atmp0 >> 32);
								atmp1 = (atmp1 >> 0x1F) + atmp1;
								atmp1 *= 3;
								uint mtmp0 = (uint)(c900 - atmp1 + a0);

								for (int a1 = 0; a1 < 0x10; a1++)
								{
									stmp140[a1] = l2080[a1][a0] * table_626a90[a1];
								}

								for (int a1 = 0; a1 < 4; a1++)
								{
									stmp30[a1] = stmp140[a1] + stmp140[0x7 - a1] + stmp140[0x8 + a1] + stmp140[0xF - a1];
								}

								for (int a1 = 0; a1 < 4; a1++)
								{
									stmp30[0x4 + a1] = stmp140[a1] + stmp140[0xF - a1] - stmp140[0x7 - a1] - stmp140[0x8 + a1];
									stmp30[0x4 + a1] *= table_626af0[a1];
								}

								for (int a1 = 0; a1 < 4; a1++)
								{
									stmp30[0xC + a1] = stmp140[0x7 - a1] - stmp140[0x8 + a1];
									stmp30[0xC + a1] *= table_626ae0[3 - a1];
									stmp30[0xC + a1] = (stmp140[a1] - stmp140[0xF - a1]) * table_626ad0[a1] - stmp30[0xC + a1];
									stmp30[0xC + a1] *= table_626af0[a1];
								}

								for (int a1 = 0; a1 < 4; a1++)
								{
									stmp30[0x8 + a1] = stmp140[0x7 - a1] - stmp140[0x8 + a1];
									stmp30[0x8 + a1] *= table_626ae0[3 - a1];
									stmp30[0x8 + a1] = (stmp140[a1] - stmp140[0xF - a1]) * table_626ad0[a1] + stmp30[0x8 + a1];
								}

								float* locals = stackalloc float[0x100];
								locals[72] = stmp30[0] + stmp30[1] + stmp30[2] + stmp30[3];
								locals[72] *= 0.5f;

								float* calc_buf = stackalloc float[0x100];
								int calc_n = 0;
								calc_buf[++calc_n] = stmp30[0] - stmp30[1] - stmp30[2] + stmp30[3];
								calc_buf[calc_n] *= 0.7071068f;
								calc_buf[++calc_n] = stmp30[0] - stmp30[3];
								calc_buf[calc_n] *= 1.847759f;
								calc_buf[++calc_n] = stmp30[1] - stmp30[2];
								calc_buf[calc_n] *= 0.7653669f;
								calc_buf[++calc_n] = 0.2071068f;
								calc_buf[calc_n] *= calc_buf[calc_n - 2];
								calc_buf[++calc_n] = 1.207107f;
								calc_buf[calc_n] *= calc_buf[calc_n - 2];
								calc_buf[calc_n - 1] -= calc_buf[calc_n];
								calc_n--;
								locals[20] = calc_buf[calc_n--];
								calc_buf[++calc_n] = stmp30[4] - stmp30[7];
								calc_buf[calc_n] *= 1.847759f;
								calc_buf[++calc_n] = stmp30[5] - stmp30[6];
								calc_buf[calc_n] *= 0.7653669f;
								locals[30] = stmp30[4] + stmp30[5] + stmp30[6] + stmp30[7];
								locals[30] *= 0.5f;
								locals[26] = calc_buf[calc_n] + calc_buf[calc_n - 1];
								locals[26] *= 0.5f;
								locals[26] -= locals[30];
								locals[22] = stmp30[4] - stmp30[5] - stmp30[6] + stmp30[7];
								locals[22] *= 0.7071068f;
								locals[22] -= locals[26];
								calc_buf[calc_n] = calc_buf[calc_n - 1] * 0.2071068f - calc_buf[calc_n] * 1.207107f;
								calc_buf[calc_n] -= locals[22];
								locals[18] = calc_buf[calc_n--];
								calc_n--;
								calc_buf[++calc_n] = stmp30[8] + stmp30[9] + stmp30[10] + stmp30[11];
								calc_buf[calc_n] *= 0.5f;
								locals[71] = calc_buf[calc_n] - locals[72];
								locals[70] = locals[30] - locals[71];
								locals[36] = stmp30[8] - stmp30[9] - stmp30[10] + stmp30[11];
								locals[36] *= 0.7071068f;
								calc_buf[++calc_n] = stmp30[8] - stmp30[11];
								calc_buf[calc_n] *= 1.847759f;
								calc_buf[++calc_n] = stmp30[9] - stmp30[10];
								calc_buf[calc_n] *= 0.7653669f;
								calc_buf[++calc_n] = 0.2071068f;
								calc_buf[calc_n] *= calc_buf[calc_n - 2];
								calc_buf[++calc_n] = 1.207107f;
								calc_buf[calc_n] *= calc_buf[calc_n - 2];
								calc_buf[calc_n - 1] -= calc_buf[calc_n];
								calc_n--;
								locals[34] = calc_buf[calc_n--];
								calc_buf[++calc_n] = stmp30[12] + stmp30[13] + stmp30[14] + stmp30[15];
								calc_buf[calc_n] *= 0.5f;
								calc_buf[calc_n] -= calc_buf[calc_n - 3];
								locals[69] = calc_buf[calc_n] - locals[70];
								locals[27] = (calc_buf[calc_n - 1] + calc_buf[calc_n - 2]) * 0.5f - calc_buf[calc_n];
								calc_n -= 4;
								locals[68] = (calc_buf[calc_n] + calc_buf[calc_n - 1]) * 0.5f - locals[69];
								calc_n -= 2;
								calc_buf[++calc_n] = stmp30[12] - stmp30[15];
								calc_buf[calc_n] *= 1.847759f;
								locals[67] = locals[27] - locals[68];
								calc_buf[++calc_n] = stmp30[13] - stmp30[14];
								calc_buf[calc_n] *= 0.7653669f;
								locals[66] = locals[26] - locals[67];
								locals[37] = calc_buf[calc_n] + calc_buf[calc_n - 1] - stmp30[12] - stmp30[13] - stmp30[14] - stmp30[15];
								locals[37] *= 0.5f;
								locals[35] = stmp30[12] - stmp30[13] - stmp30[14] + stmp30[15];
								locals[35] *= 0.7071068f;
								locals[35] -= locals[37];
								calc_buf[calc_n - 1] *= 0.2071068f;
								calc_buf[calc_n] *= 1.207107f;
								calc_buf[calc_n - 1] -= calc_buf[calc_n];
								calc_n--;
								calc_buf[calc_n] -= locals[35];
								calc_buf[++calc_n] = locals[37] - locals[27];
								locals[65] = calc_buf[calc_n] - locals[66];
								calc_buf[calc_n + 1] = calc_buf[calc_n];
								calc_buf[calc_n] = calc_buf[calc_n - 2];
								calc_buf[calc_n - 2] =calc_buf[calc_n + 1];
								locals[64] = calc_buf[calc_n] - locals[65];
								calc_n--;
								calc_buf[calc_n - 1] = locals[36] - calc_buf[calc_n - 1];
								locals[63] = calc_buf[calc_n - 1] - locals[64];
								locals[62] = locals[22] - locals[63];
								calc_buf[calc_n - 1] = locals[35] - calc_buf[calc_n - 1];
								locals[61] = calc_buf[calc_n - 1] - locals[62];
								locals[60] = locals[20] - locals[61];
								calc_buf[calc_n - 1] = locals[34] - calc_buf[calc_n - 1];
								locals[59] = calc_buf[calc_n - 1] - locals[60];
								locals[58] = locals[18] - locals[59];
								calc_buf[calc_n] -= calc_buf[calc_n - 1];
								locals[57] = calc_buf[calc_n] - locals[58];
								calc_n -= 2;

								uint atmp2 = table_5b4050[mtmp0 + 0xF];

								for (int a2 = 0; a2 < 0x10; a2++)
								{
									buf_ctmp1[atmp2 - 0xF + a2] = locals[57 + a2];
								}

								for (int a2 = 0; a2 < 4; a2++)
								{
									buf_ctmp1[table_5b4050[mtmp0] + 0x30 + a2] =  buf_ctmp1[table_5b4050[mtmp0 + 0x12] + 0x4 + 3 - a2] * table_5b3ed0[a2] + table_5b3d50[a2] * buf_ctmp1[table_5b4050[mtmp0 + 3] + a2];
								}

								for (int a2 = 0; a2 < 4; a2++)
								{
									buf_ctmp1[table_5b4050[mtmp0 + 3] + 0x30 + a2] = buf_ctmp1[table_5b4050[mtmp0 + 0xF] - 3 + 3 - a2] * table_5b3ee0[3 - a2] - buf_ctmp1[table_5b4050[mtmp0 + 0x12] + a2] * table_5b3d60[3 - a2];
								}

								for (int a2 = 0; a2 < 4; a2++)
								{
									buf_ctmp1[table_5b4050[mtmp0] + 0x34 + a2] = buf_ctmp1[table_5b4050[mtmp0 + 0x12] + 3 - a2] * table_5b3ee0[a2] + buf_ctmp1[table_5b4050[mtmp0 + 3] + 4 + a2] *  table_5b3d60[a2];

								}
								for (int a2 = 0; a2 < 4; a2++)
								{
									buf_ctmp1[table_5b4050[mtmp0 + 3] + 0x34 + a2] = buf_ctmp1[table_5b4050[mtmp0 + 0xF] - 7 + 3 - a2] * table_5b3ed0[3 - a2] - buf_ctmp1[table_5b4050[mtmp0 + 0x12] + 4 + a2] * table_5b3d50[3 - a2];
								}

								var table_5b4050_mtmp0_0 = table_5b4050[mtmp0 + 0x0];
								var table_5b4050_mtmp0_3 = table_5b4050[mtmp0 + 0x3];
								var table_5b4050_mtmp0_6 = table_5b4050[mtmp0 + 0x6];
								var table_5b4050_mtmp0_9 = table_5b4050[mtmp0 + 0x9];
								var table_5b4050_mtmp0_c = table_5b4050[mtmp0 + 0xC];

								fixed (float* table_6 = MaiAT3PlusCoreDecoder_StaticData.MAPCDSD_table_static_6)
								fixed (float* buf_ctmp1_ptr = buf_ctmp1)
								fixed (float* dst0_ptr = dst0)
								{
									for (int a3 = 0; a3 < 10; a3++)
									{
										var a3_0x8 = a3 * 0x8;
										var a3_0x30 = a3 * 0x30;
										int a3_0x30_0x60 = 0x60 + a3_0x30;
										// @TODO: VERY SLOW
										for (int a2 = 0; a2 < 4; a2++)
										{
											buf_ctmp1_ptr[table_5b4050_mtmp0_0 + a3_0x30_0x60 + 0 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_0 + 0x60 + 0 + 0 + a3_0x30 - 0x30 + a2] + table_6[0 + 0 + a3_0x8 + a2] * buf_ctmp1_ptr[table_5b4050_mtmp0_c + 0x29 + 4 + 3 + a3_0x30 - a2];
											buf_ctmp1_ptr[table_5b4050_mtmp0_3 + a3_0x30_0x60 + 0 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_9 + 0x29 + 4 + 3 + a3_0x30 - 0x00 - a2] * table_6[3 + 4 + a3_0x8 - a2] - buf_ctmp1_ptr[table_5b4050_mtmp0_6 + 0x30 + 0 + 0 + a3_0x30 + a2];
											buf_ctmp1_ptr[table_5b4050_mtmp0_0 + a3_0x30_0x60 + 4 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_0 + 0x60 + 0 + 0 + a3_0x30 - 0x2C + a2] + table_6[0 + 4 + a3_0x8 + a2] * buf_ctmp1_ptr[table_5b4050_mtmp0_c + 0x29 + 0 + 3 + a3_0x30 - a2];
											buf_ctmp1_ptr[table_5b4050_mtmp0_3 + a3_0x30_0x60 + 4 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_9 + 0x29 + 0 + 3 + a3_0x30 - 0x00 - a2] * table_6[3 + 0 + a3_0x8 - a2] - buf_ctmp1_ptr[table_5b4050_mtmp0_6 + 0x30 + 0 + 4 + a3_0x30 + a2];
										}
									}

									// @TODO: SLOW
									for (int a2 = 0; a2 < 4; a2++)
									{
										dst0_ptr[a0 * 0x10 + 0x0 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_c + 0x20D - 0 + 3 - a2] * table_6[0x50 + 0 + a2] + buf_ctmp1_ptr[table_5b4050_mtmp0_0 + 0x210 + 0 + a2];
										dst0_ptr[a0 * 0x10 + 0x8 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_9 + 0x20D - 0 + 3 - a2] * table_6[0x54 + 3 - a2] - buf_ctmp1_ptr[table_5b4050_mtmp0_6 + 0x210 + 0 + a2];
										dst0_ptr[a0 * 0x10 + 0x4 + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_c + 0x20D - 4 + 3 - a2] * table_6[0x54 + 0 + a2] + buf_ctmp1_ptr[table_5b4050_mtmp0_0 + 0x210 + 4 + a2];
										dst0_ptr[a0 * 0x10 + 0xC + a2] = buf_ctmp1_ptr[table_5b4050_mtmp0_9 + 0x20D - 4 + 3 - a2] * table_6[0x50 + 3 - a2] - buf_ctmp1_ptr[table_5b4050_mtmp0_6 + 0x210 + 4 + a2];
									}
								}
							}
						}
					}
				}
			}

			c900--;
			if (0 == c900) c900 = 3;

			for (int a0 = 0; a0 < chns; a0++)
			{
				MaiAT3PlusCoreDecoderChnACCData ad0 = chn_info[a0].acc_data_now;
				chn_info[a0].acc_data_now = chn_info[a0].acc_data_old;
				chn_info[a0].acc_data_old = ad0;

				MaiAT3PlusCoreDecoderChnACCTable at0 = chn_info[a0].acc_table_now;
				chn_info[a0].acc_table_now = chn_info[a0].acc_table_old;
				chn_info[a0].acc_table_old = at0;
			}

			return rs;
		}


		public int getAudioSamplesI16(uint index_chn, ManagedPointer<short> bufs)
		{
			int rs = 0;

			bufs.GetRawPointer((bufs_ptr_intptr) =>
			{
				short* bufs_ptr = (short *)bufs_ptr_intptr.ToPointer();
				fixed (float* dst_buf_chn_ptr = dst_buf[index_chn])
				{
					for (int a0 = 0; a0 < 0x800; a0++)
					{
						int data_now = (int)dst_buf_chn_ptr[a0];
						if (data_now > 0x7FFF) bufs_ptr[a0] = 0x7FFF;
						else if (data_now < (-0x8000)) bufs_ptr[a0] = -0x8000;
						else bufs_ptr[a0] = (short)data_now;
					}
				}
			});

			return rs;
		}

	}
}
