/*
 * MaiAT3PlusCoreDecoder
 * Copyright (c) 2013 ryuukazenomai <ryuukazenomai@foxmail.com>
 *
 * This file is part of MaiAT3PlusDecoder.
 *
 * MaiAT3PlusDecoder is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * MaiAT3PlusDecoder is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with MaiAT3PlusDecoder; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */
#include "MaiAT3PlusCoreDecoder.h"

MaiAT3PlusCoreDecoder::MaiAT3PlusCoreDecoder()
{

	MaiAT3PlusCoreDecoderJointChnInfo *joint_chn_info =
		(MaiAT3PlusCoreDecoderJointChnInfo*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderJointChnInfo));
	Mai_memset(joint_chn_info, 0, sizeof(MaiAT3PlusCoreDecoderJointChnInfo));

	MaiAT3PlusCoreDecoderChnACCTableInner *inner0 = 
		(MaiAT3PlusCoreDecoderChnACCTableInner*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCTableInner));
	Mai_memset(inner0, 0, sizeof(MaiAT3PlusCoreDecoderChnACCTableInner));
	MaiAT3PlusCoreDecoderChnACCTableInner *inner1 = 
		(MaiAT3PlusCoreDecoderChnACCTableInner*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCTableInner));
	Mai_memset(inner1, 0, sizeof(MaiAT3PlusCoreDecoderChnACCTableInner));

	for (Mai_I32 a0 = 0; a0 < 2; a0++)
	{
		chn_info[a0] = (MaiAT3PlusCoreDecoderChnInfo*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnInfo));
		Mai_memset(chn_info[a0], 0, sizeof(MaiAT3PlusCoreDecoderChnInfo));
		
		chn_info[a0]->chn_flag = a0;
		chn_info[a0]->joint_chn_info = joint_chn_info;

		chn_info[a0]->acc_data_old = 
			(MaiAT3PlusCoreDecoderChnACCData*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCData));
		Mai_memset(chn_info[a0]->acc_data_old, 0, sizeof(MaiAT3PlusCoreDecoderChnACCData));
		chn_info[a0]->acc_data_now = 
			(MaiAT3PlusCoreDecoderChnACCData*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCData));
		Mai_memset(chn_info[a0]->acc_data_now, 0, sizeof(MaiAT3PlusCoreDecoderChnACCData));

		chn_info[a0]->acc_table_old = 
			(MaiAT3PlusCoreDecoderChnACCTable*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCTable));
		Mai_memset(chn_info[a0]->acc_table_old, 0, sizeof(MaiAT3PlusCoreDecoderChnACCTable));
		chn_info[a0]->acc_table_old->inner = inner0;

		chn_info[a0]->acc_table_now = 
			(MaiAT3PlusCoreDecoderChnACCTable*)heap0.alloc(sizeof(MaiAT3PlusCoreDecoderChnACCTable));
		Mai_memset(chn_info[a0]->acc_table_now, 0, sizeof(MaiAT3PlusCoreDecoderChnACCTable));
		chn_info[a0]->acc_table_now->inner = inner1;

		chn_info[a0]->chn_ref = chn_info[0];
	}

	c900 = 3;
	
	for (Mai_I32 a0 = 0; a0 < 2; a0++)
	{
		for (Mai_I32 a1 = 0; a1 < 0x1000; a1++)
		{
			syn_buf[a0][a1] = 0.0f;
		}
	}

	for (Mai_I32 a0 = 0; a0 < 2; a0++)
	{
		for (Mai_I32 a1 = 0; a1 < 0x800; a1++)
		{
			kyou_buf[a0][a1] = 0.0f;
		}
	}
}

MaiAT3PlusCoreDecoder::~MaiAT3PlusCoreDecoder()
{
	heap0.free(chn_info[0]->acc_table_now->inner);
	heap0.free(chn_info[0]->acc_table_old->inner);
	heap0.free(chn_info[0]->joint_chn_info);
	for (Mai_I32 a0 = 0; a0 < 2; a0++)
	{
		heap0.free(chn_info[a0]->acc_table_now);
		heap0.free(chn_info[a0]->acc_table_old);
		heap0.free(chn_info[a0]->acc_data_now);
		heap0.free(chn_info[a0]->acc_data_old);
		heap0.free(chn_info[a0]);
	}
}


Mai_Status MaiAT3PlusCoreDecoder::parseStream(MaiBitReader *mbr0, Mai_U32 chns, Mai_U32 joint_flag)
{
	Mai_Status rs = 0;
	
	chn_info[0]->joint_chn_info->joint_flag = joint_flag;
	chn_info[0]->joint_chn_info->chns = chns;

	while (1)
	{
		if (rs = decodeBandNum(mbr0, chn_info)) break;
		if (rs = decodeTable0(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeTable1(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeTable2(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeTable3(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeACC2Pre(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeACC2Main(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeACC6Inner(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;
		if (rs = decodeTailInfo(mbr0, chn_info, chn_info[0]->joint_chn_info->chns)) break;

		break;
	}

	return rs;
}


Mai_Status MaiAT3PlusCoreDecoder::decodeBandNum(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos)
{
	chn_infos[0]->joint_chn_info->num_band_splited_declared = 
		mbr0->getWithI32Buffer(5) + 1;
	
	chn_infos[0]->joint_chn_info->num_band_declared = 
		MAPCDSD_band_num_table0[chn_infos[0]->joint_chn_info->num_band_splited_declared] + 1;
	
	chn_infos[0]->joint_chn_info->var118 = 
		mbr0->getWithI32Buffer(1);
	
	return 0;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeTable0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;
	
	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < 0x20; a1++)
			chn_infos[a0]->table0[a1] = 0;
		
		if (rs = MAPCDSF_decodeTable0_func_list0[chn_infos[a0]->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
			(mbr0, chn_infos[a0])
			)
			return rs;
	}
	
	chn_infos[0]->joint_chn_info->num_band_splited_used = chn_infos[0]->joint_chn_info->num_band_splited_declared; //[B0]
	
	if (chns == 2)
	{
		while ( (chn_infos[0]->joint_chn_info->num_band_splited_used) &&
			(!chn_infos[0]->table0[chn_infos[0]->joint_chn_info->num_band_splited_used - 1]) &&
			(!chn_infos[1]->table0[chn_infos[0]->joint_chn_info->num_band_splited_used - 1]) )
			chn_infos[0]->joint_chn_info->num_band_splited_used--;
	}
	else
	{
		while ( (chn_infos[0]->joint_chn_info->num_band_splited_used) &&
			(!chn_infos[0]->table0[chn_infos[0]->joint_chn_info->num_band_splited_used - 1]) )
			chn_infos[0]->joint_chn_info->num_band_splited_used--;
	}


	chn_infos[0]->joint_chn_info->num_band_used = 
		MAPCDSD_band_num_table0[chn_infos[0]->joint_chn_info->num_band_splited_used] + 1;
		
	//check
	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < 0x20; a1++)
		{
			if ( (chn_infos[a0]->table0[a1] < 0) ||
				(chn_infos[a0]->table0[a1] >= 8)
				)
				return -0x10B;
		}
	}
	
	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeTable1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;
	
	if (chn_infos[0]->joint_chn_info->num_band_splited_used) //
	{
		for (Mai_U32 a0 = 0; a0 < chns; a0++)
		{
			for (Mai_U32 a1 = 0; a1 < 0x20; a1++)
				chn_infos[a0]->table1[a1] = 0;
				
			if (rs = MAPCDSF_decodeTable1_func_list0[chn_infos[a0]->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
				(mbr0, chn_infos[a0])
				)
				return rs;
		}
	}
	
	//check
	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < 0x20; a1++)
		{
			if ( (chn_infos[a0]->table1[a1] < 0) ||
				(chn_infos[a0]->table1[a1] >= 0x40)
				)
				return -0x110;
		}
	}
	
	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeTable2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	if (chn_infos[0]->joint_chn_info->num_band_splited_used) //
	{
		chn_infos[0]->joint_chn_info->var90 = mbr0->getWithI32Buffer(1);  //[90] tmp4 [arg1]

		for (Mai_U32 a0 = 0; a0 < chns; a0++)
		{
			for (Mai_U32 a1 = 0; a1 < 0x20; a1++)
				chn_infos[a0]->table2[a1] = 0;

			chn_infos[a0]->var1034 = mbr0->getWithI32Buffer(1);  //[1034] tmp5

			MAPCDSF_makeTable0CheckTable(chn_infos[a0], chn_infos[a0]->check_table0); //check

			if (rs = MAPCDSF_decodeTable2_func_list0[chn_infos[a0]->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
				(mbr0, chn_infos[a0])
				)
				return rs;
		}

		//comp
	}

	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeTable3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < 0x800; a1++) chn_infos[a0]->table3[a1] = 0;
		for (Mai_U32 a1 = 0; a1 < 0x5; a1++) chn_infos[a0]->table4[a1] = 0x0F;

		for (Mai_U32 a1 = 0; a1 < chn_infos[0]->joint_chn_info->num_band_splited_used; a1++)
		{
			if (!chn_infos[a0]->table0[a1])
			{
				for (Mai_U32 a2 = 0; a2 < MAPCDSD_bind_table1[a1]; a2++) chn_infos[a0]->table3[MAPCDSD_bind_table0[a1] + a2] = 0;
			}
			else
			{
				Mai_U32 atmp0 = 0;

				if (!chn_infos[a0]->joint_chn_info->var90) atmp0 = MAPCDSD_bind_table2[ (chn_infos[a0]->var1034 * 7 + chn_infos[a0]->table0[a1]) * 4 + chn_infos[a0]->table2[a1] ]; //tmp4 5 6
				else atmp0 = chn_infos[a0]->table2[a1];

				MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = &MAPCDSD_huff_table3[(chn_infos[a0]->var1034 * 8 + atmp0) * 7 + chn_infos[a0]->table0[a1]]; //tmp5 6
				
				MAPCDSF_decodeTable3Sub0(mbr0, &(chn_infos[a0]->table3[MAPCDSD_bind_table0[a1]]), MAPCDSD_bind_table1[a1], huff_table_now);
			}
		}

		if (chn_infos[0]->joint_chn_info->num_band_splited_used > 2)
		{
			for (Mai_U32 a1 = 0; a1 < (Mai_U32)(MAPCDSD_band_num_table1[MAPCDSD_band_num_table0[chn_infos[0]->joint_chn_info->num_band_splited_used] + 1] + 1); a1++)
			{
				chn_infos[a0]->table4[a1] = mbr0->getWithI32Buffer(4);
			}
		}
	}

	if (chns == 2)
	{
		MAPCDSF_readPackTable0(mbr0, &(chn_infos[0]->joint_chn_info->table48), chn_infos[0]->joint_chn_info->num_band_used);
		MAPCDSF_readPackTable0(mbr0, &(chn_infos[0]->joint_chn_info->table00), chn_infos[0]->joint_chn_info->num_band_used);
	}


	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeACC2Pre(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		MAPCDSF_readPackTable0(mbr0, &(chn_infos[a0]->acc_data_now->acc), chn_infos[0]->joint_chn_info->num_band_declared);
	}

	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeACC2Main(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		Mai_memset(chn_infos[a0]->acc_data_now->table,
			0,
			sizeof(chn_infos[a0]->acc_data_now->table)
			);

		Mai_U32 uk1b444 = mbr0->getWithI32Buffer(1);

		if (uk1b444)
		{
			chn_infos[a0]->uk1b450 = mbr0->getWithI32Buffer(4) + 1;
			Mai_U32 uk1b448 = mbr0->getWithI32Buffer(1);

			Mai_U32 uk1b44c = chn_infos[a0]->uk1b450;
			if (uk1b448) uk1b44c = mbr0->getWithI32Buffer(4) + 1;

			//call 478200
			if (rs = MAPCDSF_decodeACC2MainSub0(mbr0, chn_infos[a0]))
				break;

			//call 478270
			if (rs = MAPCDSF_decodeACC2MainSub1(mbr0, chn_infos[a0]))
				break;

			//call 478330
			if (rs = MAPCDSF_decodeACC2MainSub2(mbr0, chn_infos[a0]))
				break;

			if (uk1b448)
			{
				for (Mai_U32 b0 = chn_infos[a0]->uk1b450; b0 < uk1b44c; b0++)
				{
					chn_infos[a0]->acc_data_now->table[b0].num_acc = 
						chn_infos[a0]->acc_data_now->table[b0 - 1].num_acc;
					for (Mai_U32 b1 = 0; b1 < (Mai_U32)chn_infos[a0]->acc_data_now->table[b0].num_acc; b1++)
					{
						chn_infos[a0]->acc_data_now->table[b0].data1[b1] = 
							chn_infos[a0]->acc_data_now->table[b0 - 1].data1[b1];
						chn_infos[a0]->acc_data_now->table[b0].data0[b1] = 
							chn_infos[a0]->acc_data_now->table[b0 - 1].data0[b1];
					}
				}
			}
		}
		else
		{
			Mai_U32 uk1b44c = 0;
		}

	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_11[];
Mai_Status MaiAT3PlusCoreDecoder::decodeACC6Inner(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	if (chns == 2)
	{
		Mai_memset(&chn_infos[0]->acc_table_now->inner->table_unk0, 0, sizeof(MaiAT3PlusCoreDecoderPackTable0));
		Mai_memset(&chn_infos[0]->acc_table_now->inner->table_unk1, 0, sizeof(MaiAT3PlusCoreDecoderPackTable0));
		Mai_memset(&chn_infos[0]->acc_table_now->inner->table_unk2, 0, sizeof(MaiAT3PlusCoreDecoderPackTable0));
	}

	for (Mai_U32 a0 = 0; a0 < chns; a0++)
	{
		Mai_memset(chn_infos[a0]->acc_table_now->table, 0, sizeof(chn_infos[a0]->acc_table_now->table));
		for (Mai_U32 a1 = 0; a1 < 0x10; a1++)
		{
			chn_infos[a0]->acc_table_now->table[a1].unk[7] = 0x20;
			chn_infos[a0]->acc_table_now->table[a1].unk[6] = 0;
		}
	}

	chn_infos[0]->acc_table_now->inner->ptr_to_use_now = 
			chn_infos[0]->acc_table_now->inner->data;

	chn_infos[0]->acc_table_now->inner->unk0 = 
		mbr0->getWithI32Buffer(1);

	if (chn_infos[0]->acc_table_now->inner->unk0)
	{
		chn_infos[0]->acc_table_now->inner->unk1 = 
			mbr0->getWithI32Buffer(1);

		chn_infos[0]->acc_table_now->inner->unk2 = 
			MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_11[0], mbr0)
			+ 1;

		if (chns == 2)
		{
			MAPCDSF_readPackTable0(mbr0, &chn_infos[0]->acc_table_now->inner->table_unk0, chn_infos[0]->acc_table_now->inner->unk2);
			MAPCDSF_readPackTable0(mbr0, &chn_infos[0]->acc_table_now->inner->table_unk2, chn_infos[0]->acc_table_now->inner->unk2);
			MAPCDSF_readPackTable0(mbr0, &chn_infos[0]->acc_table_now->inner->table_unk1, chn_infos[0]->acc_table_now->inner->unk2);
		}

		for (Mai_U32 a0 = 0; a0 < chns; a0++)
		{
			//call 477e60
			if (rs = MAPCDSF_decodeACC6InnerSub0(mbr0, chn_infos[a0]))
				break;
		}

		if (chns == 2)
		{
			for (Mai_I32 a0 = 0; a0 < chn_infos[0]->acc_table_now->inner->unk2; a0++)
			{
				if (chn_infos[0]->acc_table_now->inner->table_unk0.data[a0])
				{
					/*for (Mai_I32 a1 = 0; a1 < 0xA; a1++)
					{
						chn_infos[1]->acc_table_now->table[a0].unk[a1] = 
							chn_infos[0]->acc_table_now->table[a0].unk[a1];
						//memcpy?
					}*/
					Mai_memcpy(
						&chn_infos[1]->acc_table_now->table[a0],
						&chn_infos[0]->acc_table_now->table[a0],
						sizeof(MaiAT3PlusCoreDecoderChnACCTableTable));

					//left to right acc5 copy 0x10 + 0x28 * a0 0x4 add zumi
					//left to right acc5 copy 0x14 + 0x28 * a0 0x4 add zumi
					//left to right acc5 copy 0x18 + 0x28 * a0 0x4 add zumi
					//left to right acc5 copy 0x1C + 0x28 * a0 0x4 add zumi
				}
					
				if (chn_infos[0]->acc_table_now->inner->table_unk2.data[a0])
				{
					//swap?
					MaiAT3PlusCoreDecoderChnACCTableTable tmpbuf0;

					Mai_memcpy(
						&tmpbuf0,
						&chn_infos[1]->acc_table_now->table[a0],
						sizeof(MaiAT3PlusCoreDecoderChnACCTableTable));

					Mai_memcpy(
						&chn_infos[1]->acc_table_now->table[a0],
						&chn_infos[0]->acc_table_now->table[a0],
						sizeof(MaiAT3PlusCoreDecoderChnACCTableTable));

					Mai_memcpy(
						&chn_infos[0]->acc_table_now->table[a0],
						&tmpbuf0,
						sizeof(MaiAT3PlusCoreDecoderChnACCTableTable));
					/*
					Mai_I32 tmpbuf0[0xA];
					for (Mai_I32 a1 = 0; a1 < 0xA; a1++)
					{
						tmpbuf0[a1] = *(Mai_I32*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4];
					}
					for (Mai_I32 a1 = 0; a1 < 0xA; a1++)
					{
						*(Mai_I32*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4] = 
							*(Mai_I32*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4];
					}
					for (Mai_I32 a1 = 0; a1 < 0xA; a1++)
					{
						*(Mai_I32*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4] = 
							tmpbuf0[a1];
					}*/
				}
			}
		}


	}

	return rs;
}

Mai_Status MaiAT3PlusCoreDecoder::decodeTailInfo(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns)
{
	Mai_Status rs = 0;

	chn_infos[0]->joint_chn_info->var94 = mbr0->getWithI32Buffer(1);

	if (chn_infos[0]->joint_chn_info->var94)
	{
		chn_infos[0]->joint_chn_info->var98 = mbr0->getWithI32Buffer(4);
		chn_infos[0]->joint_chn_info->var9c = mbr0->getWithI32Buffer(4);
	}

	return rs;
}

