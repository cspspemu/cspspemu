/*
 * MaiAT3PlusCoreDecoder_SubFunc
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


Mai_U32 MAPCDSF_getHuffValue(MaiAT3PlusCoreDecoderSearchTableDes *huff_table, MaiBitReader *mbr0)//Mai_U16 *table0, Mai_U8 *table1, Mai_U32 max_len,  MaiBitReader *mbr0)
{
	Mai_U32 value = mbr0->getWithI32Buffer(huff_table->max_bit_len, 0);
	value = huff_table->table1[value];
	mbr0->getWithI32Buffer(huff_table->table0[value * 2 + 1]);
	return value;
}

Mai_Status MAPCDSF_readPackTable0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderPackTable0 *table, Mai_U32 counter)
{
	for (Mai_U32 a0 = 0; a0 < 0x10; a0++) table->data[a0] = 0;

	table->check_data0 = mbr0->getWithI32Buffer(1);

	if (table->check_data0)
	{
		table->check_data1 = mbr0->getWithI32Buffer(1);
		if (table->check_data1)
		{
			for (Mai_U32 a0 = 0; a0 < counter; a0++)
				table->data[a0] = 
				mbr0->getWithI32Buffer(1);
		}
		else
		{
			for (Mai_U32 a0 = 0; a0 < counter; a0++)
				table->data[a0] = 1;
		}
	}

	return 0;
}

Mai_Status MAPCDSF_decodeTable0DataNum(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	chn_info->table0_data_num0 = 
		chn_info->joint_chn_info->num_band_splited_declared;

	chn_info->table0_data_num1 = 0;

	if (chn_info->table0_flag_data_num)
	{
		chn_info->table0_data_num0 = mbr0->getWithI32Buffer(5);

		if (chn_info->table0_data_num0 > chn_info->joint_chn_info->num_band_splited_declared)
		{
			return -5;
		}

		if (chn_info->table0_flag_data_num == 3)
		{
			chn_info->table0_data_num1 = mbr0->getWithI32Buffer(2) + 1; 
			if (chn_info->chn_flag) chn_info->table0_data_num1 = chn_info->table0_data_num1 - 1 + 3;
		}
	}
	
	return 0;
}

Mai_Status MAPCDSF_padTable0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	if (chn_info->table0_flag_data_num == 0)
	{
	}
	else if (chn_info->table0_flag_data_num == 1)
	{
		for (Mai_U32 a0 = chn_info->table0_data_num0; a0 < chn_info->joint_chn_info->num_band_splited_declared; a0++)
			chn_info->table0[a0] = 0;
	}
	else if (chn_info->table0_flag_data_num == 2)
	{
		if (!chn_info->chn_flag)
		{
			for (Mai_U32 a0 = chn_info->table0_data_num0; a0 < chn_info->joint_chn_info->num_band_splited_declared; a0++)
				chn_info->table0[a0] = 1;
		}
		else
		{
			for (Mai_U32 a0 = chn_info->table0_data_num0; a0 < chn_info->joint_chn_info->num_band_splited_declared; a0++)
				chn_info->table0[a0] = mbr0->getWithI32Buffer(1);
		}
	}
	else if (chn_info->table0_flag_data_num == 3)
	{
		if (!chn_info->chn_flag)
		{
			for (Mai_U32 a0 = chn_info->table0_data_num0; a0 < chn_info->joint_chn_info->num_band_splited_declared - chn_info->table0_data_num1; a0++)
				chn_info->table0[a0] = 1;
		}
		else
		{
			for (Mai_U32 a0 = chn_info->table0_data_num0; a0 < chn_info->table0_data_num0 + chn_info->table0_data_num1; a0++)
				chn_info->table0[a0] = 1;
		}
	}
	
	return 0;
}

Mai_Status MAPCDSF_exTable0Value(MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	if (chn_info->joint_chn_info->num_band_splited_declared)
	{
		Mai_U8 ex_table0[] = {0x06, 0x06, 0x06, 0x06, 0x06, 0x02, 0x02, 0x03, 0x04, 0x05, 0x05, 0x06, 0x06, 0x06, 0x02, 0x03, 0x03, 0x04, 0x06, 0x06, 0x06, 0x06, 0x06, 0x01, 0x03, 0x04, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x05, 0x05, 0x04, 0x04, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x05, 0x04, 0x04, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
		for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_declared; a0++)
		{
			chn_info->table0[a0] += ex_table0[((chn_info->chn_flag * 3 + chn_info->table0_flag_ex) << 5) + a0];
		}
	}
	
	return 0;
}

Mai_Status MAPCDSF_makeTable0CheckTable(MaiAT3PlusCoreDecoderChnInfo *chn_info, Mai_U32 *check_table)
{
	if (!chn_info->chn_flag)
	{
		for (Mai_I32 a0 = 0; a0 < 0x20; a0++)
			if (chn_info->table0[a0]) check_table[a0] = 1;
			else check_table[a0] = 0;
	}
	else
	{
		for (Mai_I32 a0 = 0; a0 < 0x20; a0++)
			if (chn_info->table0[a0]) check_table[a0] = 1;
			else if (chn_info->chn_ref->table0[a0]) check_table[a0] = 2;
			else check_table[a0] = 0;
	}

	return 0;
}

//used in decodeTable0 0:0 1:0
Mai_Status MAPCDSF_decodeTable0_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_declared; a0++)
	{
			chn_info->table0[a0] = mbr0->getWithI32Buffer(3);
	}
	
	return 0;
}

//used in decodeTable0 0:1
Mai_Status MAPCDSF_decodeTable0_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	chn_info->table0_flag_ex = mbr0->getWithI32Buffer(2);
	chn_info->table0_flag_data_num = mbr0->getWithI32Buffer(2);

	if (rs = MAPCDSF_decodeTable0DataNum(mbr0, chn_info)) return rs;

	if (chn_info->table0_data_num0)
	{
		Mai_U32 uk1c6d0 = mbr0->getWithI32Buffer(5);
		Mai_U32 uk1c6d4 = mbr0->getWithI32Buffer(2);
		Mai_U32 uk1c6d8 = mbr0->getWithI32Buffer(3);

		for (Mai_U32 a0 = 0; a0 < uk1c6d0; a0++)
		{
			chn_info->table0[a0] = mbr0->getWithI32Buffer(3);
		}

		if (!uk1c6d4)
		{
			for (Mai_U32 a0 = uk1c6d0; a0 < chn_info->table0_data_num0; a0++)
			{
				chn_info->table0[a0] = uk1c6d8;
			}
		}
		else
		{
			for (Mai_U32 a0 = uk1c6d0; a0 < chn_info->table0_data_num0; a0++)
			{
				chn_info->table0[a0] = mbr0->getWithI32Buffer(uk1c6d4) + uk1c6d8;
			}
		}
	}

	if (rs = MAPCDSF_padTable0(mbr0, chn_info)) return rs;

	if (chn_info->table0_flag_ex)
	{
		MAPCDSF_exTable0Value(chn_info);
	}
	
	return rs;
}

//used in decodeTable0 0:2
Mai_Status MAPCDSF_decodeTable0_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	chn_info->table0_flag_data_num = mbr0->getWithI32Buffer(2);

	if (rs = MAPCDSF_decodeTable0DataNum(mbr0, chn_info)) return rs;

	if (chn_info->table0_data_num0)
	{
		Mai_U32 uk1c6f8 = mbr0->getWithI32Buffer(1);
		Mai_U32 uk1c6e0 = mbr0->getWithI32Buffer(1);
		Mai_U32 uk1c6f4 = mbr0->getWithI32Buffer(3);
		Mai_U32 uk1c6f0 = mbr0->getWithI32Buffer(4);

		if (chn_info->table0_data_num0)
		{
			Mai_U8 table_s0[] = {0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x09, 0x00, 0x00, 0x00, 0x00};
			Mai_U8 table_s1[] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0xFB, 0xFF, 0x00, 0x00, 0x00, 0xF9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0xF9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0xFB, 0x00, 0x00, 0x00, 0x00, 0xF9, 0xF9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0xF9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFE, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0xFE, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE,   0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xF9, 0xFE,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0xFB, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFB, 0xFE, 0x00, 0x00, 0x00, 0x00,   0xFF, 0xFB, 0xFD, 0xFE, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xFE, 0xFB, 0xFD, 0xFD, 0xFE, 0xFF, 0xFF,   0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFD, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFD, 0xFF,   0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0xFF, 0xFB, 0xFD, 0xFD, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0xFF,   0xFF, 0xFC, 0xFE, 0xFE, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFD, 0xFE, 0xFD, 0xFF, 0xFF, 0xFF,   0x00, 0xFF, 0xFC, 0xFE, 0xFD, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE, 0xFE, 0xFF, 0xFF,   0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFD, 0xFE, 0xFE, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFD, 0xFE, 0xFE, 0xFF, 0xFF, 0xFF, 0x00,   0xFF, 0xFE, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0xFF, 0x01, 0x01, 0x01, 0x01, 0x01,   0x01, 0x01, 0x00, 0xFE, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0xFE, 0x00, 0x01, 0x01,   0x01, 0x01, 0x01, 0x01, 0xFF, 0xFF, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0xFF,   0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0xFF, 0xFF, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00,   0x00, 0xFF, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0xFF, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01,   0x01, 0xFF, 0xFF, 0xFF, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,   0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0xFF, 0xFF, 0x01,   0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0xFD,   0xFE, 0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0xFD, 0xFB, 0xFD, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,   0xFF, 0xFE, 0x00, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xFF, 0xFE, 0x00, 0x01, 0x02, 0x02, 0x02,   0x02, 0x02, 0x00, 0xFE, 0x00, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xFF, 0x00, 0x01, 0x02, 0x02,   0x02, 0x02, 0x02, 0x02, 0x00, 0x00, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0xFE, 0x00,   0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0xFF, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xFF,   0xFF, 0x00, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xFF, 0xFF, 0x00, 0x01, 0x02, 0x02, 0x02, 0x02,   0x02, 0xFF, 0xFE, 0xFF, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0xFF, 0x00, 0x02, 0x02, 0x02,   0x02, 0x02, 0x02, 0x01, 0x01, 0x00, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x01, 0x02, 0x02,   0x02, 0x02, 0x02, 0x02, 0x02, 0x01, 0x00, 0x00, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00,   0x00, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0xFF, 0xFF, 0xFF, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02,   0x00, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x01, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03,   0x03, 0x03, 0xFF, 0x00, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x00, 0x00, 0x02, 0x03, 0x03,   0x03, 0x03, 0x03, 0x03, 0xFF, 0x00, 0x01, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x00, 0x00, 0x01,   0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x01,   0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x00, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x03,   0x03, 0x00, 0x00, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0xFF, 0x01, 0x02, 0x03, 0x03, 0x03,   0x03, 0x03, 0x03, 0xFF, 0x00, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x03, 0x03,   0x03, 0x03, 0x03, 0x03, 0x03, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x00, 0x02,   0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x00, 0x01, 0x01, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03,   0x00, 0x01, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04,   0x04, 0x04, 0x00, 0x00, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01, 0x01, 0x02, 0x04, 0x04,   0x04, 0x04, 0x04, 0x04, 0x00, 0x01, 0x02, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0xFF, 0x00, 0x01,   0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x00, 0x01, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01,   0x01, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x01, 0x01, 0x03, 0x04, 0x04, 0x04, 0x04,   0x04, 0x02, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01, 0x01, 0x03, 0x04, 0x04, 0x04,   0x04, 0x04, 0x04, 0x01, 0x02, 0x02, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0xFF, 0x00, 0x02, 0x03,   0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x01, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01, 0x02,   0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x02, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04,   0x01, 0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x05,   0x05, 0x05, 0x00, 0x01, 0x02, 0x03, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x01, 0x03, 0x04, 0x05,   0x05, 0x05, 0x05, 0x05, 0x01, 0x01, 0x02, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x02, 0x02,   0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x01, 0x02, 0x03, 0x05, 0x05, 0x05, 0x05, 0x05, 0x02,   0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x00, 0x01, 0x02, 0x04, 0x05, 0x05, 0x05, 0x05,   0x05, 0x02, 0x02, 0x03, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x02, 0x03, 0x05, 0x05, 0x05,   0x05, 0x05, 0x05, 0x00, 0x01, 0x03, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x02, 0x02, 0x03,   0x05, 0x05, 0x05, 0x05, 0x05, 0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x00, 0x02,   0x03, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x01, 0x01, 0x01, 0x03, 0x04, 0x05, 0x05, 0x05, 0x05,   0x01, 0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x06, 0x06, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x06,   0x06, 0x06, 0x02, 0x03, 0x04, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x01, 0x02, 0x03, 0x04, 0x06,   0x06, 0x06, 0x06, 0x06, 0x02, 0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x06, 0x06, 0x01, 0x02, 0x03,   0x04, 0x05, 0x05, 0x06, 0x06, 0x06, 0x02, 0x02, 0x03, 0x04, 0x06, 0x06, 0x06, 0x06, 0x06, 0x02,   0x02, 0x03, 0x04, 0x05, 0x06, 0x06, 0x06, 0x06, 0x02, 0x02, 0x04, 0x05, 0x06, 0x06, 0x06, 0x06,   0x06, 0x02, 0x02, 0x03, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x01, 0x02, 0x03, 0x05, 0x06, 0x06,   0x06, 0x06, 0x06, 0x02, 0x03, 0x03, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x01, 0x02, 0x04, 0x05,   0x06, 0x06, 0x06, 0x06, 0x06, 0x02, 0x02, 0x03, 0x04, 0x05, 0x05, 0x06, 0x06, 0x06, 0x02, 0x03,   0x03, 0x04, 0x06, 0x06, 0x06, 0x06, 0x06, 0x01, 0x03, 0x04, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06,   0x05, 0x05, 0x04, 0x04, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x06, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01,   0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,   0x05, 0x05, 0x04, 0x04, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x06, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x01, 0x01,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x38, 0x18, 0x3F, 0x00, 0x04, 0x35, 0x3F, 0x00, 0x44, 0x57, 0x3F, 0x00, 0x04, 0xB5, 0x3E,   0x00, 0x00, 0x00, 0x3F, 0x00, 0x04, 0x35, 0x3F, 0x00, 0x48, 0x57, 0x3E, 0x00, 0x04, 0xB5, 0x3E,   0x00, 0x38, 0x18, 0x3F, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x80, 0x3E, 0x00, 0x00, 0x00, 0x3F,   0x00, 0x30, 0x98, 0x3D, 0x00, 0x08, 0x35, 0x3E, 0x00, 0x44, 0xD7, 0x3E, 0x00, 0x00, 0x35, 0x3D,   0x00, 0x00, 0x00, 0x3E, 0x00, 0x04, 0xB5, 0x3E, 0x00, 0x40, 0xD7, 0x3C, 0x00, 0x00, 0xB5, 0x3D,   0x00, 0x38, 0x98, 0x3E, 0x00, 0x00, 0x80, 0x3C, 0x00, 0x00, 0x80, 0x3D, 0x00, 0x00, 0x80, 0x3E,   0x00, 0x00, 0x18, 0x3C, 0x00, 0x00, 0x35, 0x3D, 0x00, 0x48, 0x57, 0x3E, 0x00, 0x00, 0xB5, 0x3B,   0x00, 0x00, 0x00, 0x3D, 0x00, 0x08, 0x35, 0x3E, 0x00, 0x00, 0x58, 0x3B, 0x00, 0x00, 0xB5, 0x3C,   0x00, 0x38, 0x18, 0x3E, 0x00, 0x00, 0x00, 0x3B, 0x00, 0x00, 0x80, 0x3C, 0x00, 0x00, 0x00, 0x3E,   0x00, 0x00, 0x98, 0x3A, 0x00, 0x00, 0x35, 0x3C, 0x00, 0x40, 0xD7, 0x3D, 0x00, 0x00, 0x38, 0x3A,   0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0xB5, 0x3D, 0x00, 0x00, 0xD0, 0x39, 0x00, 0x00, 0xB5, 0x3B,   0x00, 0x30, 0x98, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x15, 0x3E, 0x00, 0x00, 0x00, 0x3F,   0x00, 0x84, 0x5A, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF,   0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x56, 0x98, 0x9C, 0xB7, 0xCE, 0x5E, 0xD8, 0xB7,   0xCC, 0x83, 0x61, 0xB7, 0xF2, 0xB2, 0xBE, 0xB5, 0xF6, 0x90, 0x8A, 0x35, 0x2C, 0xA6, 0x97, 0x35,   0xED, 0x80, 0xD9, 0x34, 0x3D, 0x71, 0xB0, 0x39, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,   0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x6F, 0x4E, 0x8C, 0x37, 0xB6, 0x31, 0xF6, 0x37};

			Mai_U32 tmptable[0x20];
			tmptable[0] = uk1c6f4;
			for (Mai_U32 a0 = 1; a0 < table_s0[chn_info->table0_data_num0 - 1] + 1; a0++)
			{
				tmptable[a0] = uk1c6f4 - table_s1[( (uk1c6f4 << 4) + uk1c6f0 ) * 9 + a0 - 1];
			}

			for (Mai_U32 a0 = 0; a0 < chn_info->table0_data_num0; a0++)
			{
				chn_info->table0[a0] = tmptable[table_s0[a0]];
			}
		}

		MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = &MAPCDSD_huff_table0[uk1c6e0];

		if (uk1c6f8)
		{
			for (Mai_U32 a0 = 0; a0 < (chn_info->table0_data_num0 >> 1); a0++)
			{
				Mai_U32 loc0 = mbr0->getWithI32Buffer(1);

				if (!loc0)
				{
					chn_info->table0[a0 * 2] += MAPCDSF_getHuffValue(huff_table_now, mbr0);
					chn_info->table0[a0 * 2 + 1] += MAPCDSF_getHuffValue(huff_table_now, mbr0);
				}
			}
			for (Mai_U32 a0 = ((chn_info->table0_data_num0 >> 1) << 1); a0 < chn_info->table0_data_num0; a0++)
			{
				chn_info->table0[a0] += MAPCDSF_getHuffValue(huff_table_now, mbr0);
			}
		}
		else
		{
			for (Mai_U32 a0 = 0; a0 < chn_info->table0_data_num0; a0++)
			{
				chn_info->table0[a0] += MAPCDSF_getHuffValue(huff_table_now, mbr0);
			}
		}

		for (Mai_U32 a0 = 0; a0 < chn_info->table0_data_num0; a0++)
		{
			chn_info->table0[a0] &= 0x7;
		}
	}

	if (rs = MAPCDSF_padTable0(mbr0, chn_info)) return rs;
	
	return rs;
}

//used in decodeTable0 0:3 1:3
Mai_Status MAPCDSF_decodeTable0_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	chn_info->table0_flag_ex = mbr0->getWithI32Buffer(2);
	chn_info->table0_flag_data_num = mbr0->getWithI32Buffer(2);

	if (rs = MAPCDSF_decodeTable0DataNum(mbr0, chn_info)) return rs;

	if (chn_info->table0_data_num0 > 0)
	{
		Mai_U32 huff_table_type0 = mbr0->getWithI32Buffer(2);

		chn_info->table0[0] = mbr0->getWithI32Buffer(3);

		for (Mai_U32 a0 = 1; a0 < chn_info->table0_data_num0; a0++)
		{
			chn_info->table0[a0] = chn_info->table0[a0 - 1] + MAPCDSF_getHuffValue(&MAPCDSD_huff_table0[huff_table_type0], mbr0);
			chn_info->table0[a0] &= 0x7;
		}
	}

	if (rs = MAPCDSF_padTable0(mbr0, chn_info)) return rs;

	if (chn_info->table0_flag_ex)
	{
		MAPCDSF_exTable0Value(chn_info);
	}
	
	return rs;
}

//used in decodeTable0 1:1
Mai_Status MAPCDSF_decodeTable0_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	chn_info->table0_flag_data_num = mbr0->getWithI32Buffer(2);

	if (rs = MAPCDSF_decodeTable0DataNum(mbr0, chn_info)) return rs;

	if (chn_info->table0_data_num0 > 0)
	{
		Mai_U32 huff_table_type1 = mbr0->getWithI32Buffer(2);

		for (Mai_U32 a0 = 0; a0 < chn_info->table0_data_num0; a0++)
		{
			chn_info->table0[a0] = chn_info->chn_ref->table0[a0] + MAPCDSF_getHuffValue(&MAPCDSD_huff_table0[huff_table_type1], mbr0);
			chn_info->table0[a0] &= 0x7;
		}
	}

	if (rs = MAPCDSF_padTable0(mbr0, chn_info)) return rs;
	
	return rs;
}

//used in decodeTable0 1:2
Mai_Status MAPCDSF_decodeTable0_Route5(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	chn_info->table0_flag_data_num = mbr0->getWithI32Buffer(2);

	if (rs = MAPCDSF_decodeTable0DataNum(mbr0, chn_info)) return rs;

	if (chn_info->table0_data_num0)
	{
		Mai_U32 uk1c6e0 = mbr0->getWithI32Buffer(2);

		MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = &MAPCDSD_huff_table0[uk1c6e0];

		chn_info->table0[0] = chn_info->chn_ref->table0[0] + MAPCDSF_getHuffValue(huff_table_now, mbr0);
		chn_info->table0[0] &= 0x7;

		for (Mai_U32 a0 = 1; a0 < chn_info->table0_data_num0; a0++)
		{
			chn_info->table0[a0] = chn_info->chn_ref->table0[a0] - chn_info->chn_ref->table0[a0 - 1] + chn_info->table0[a0 - 1] + MAPCDSF_getHuffValue(huff_table_now, mbr0);
			chn_info->table0[a0] &= 0x7;
		}
	}

	if (rs = MAPCDSF_padTable0(mbr0, chn_info)) return rs;
	
	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable0_func_list0[8] = 
	{
		MAPCDSF_decodeTable0_Route0,
		MAPCDSF_decodeTable0_Route1,
		MAPCDSF_decodeTable0_Route2,
		MAPCDSF_decodeTable0_Route3,
		MAPCDSF_decodeTable0_Route0,
		MAPCDSF_decodeTable0_Route4,
		MAPCDSF_decodeTable0_Route5,
		MAPCDSF_decodeTable0_Route3
	};




Mai_Status MAPCDSF_initTable1(MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	if (chn_info->joint_chn_info->num_band_splited_used)
	{
		Mai_U8 table_s0[] = {0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x09, 0x00, 0x00, 0x00, 0x00};
		Mai_U8 table_s1[] = {0xFD, 0xFE, 0xFF, 0x00, 0x03, 0x05, 0x06, 0x08, 0x28, 0xFD, 0xFE, 0x00, 0x01, 0x07, 0x09, 0x0B,   0x0D, 0x14, 0xFF, 0x00, 0x00, 0x01, 0x06, 0x08, 0x0A, 0x0D, 0x29, 0x00, 0x00, 0x00, 0x02, 0x05,   0x05, 0x06, 0x08, 0x0E, 0x00, 0x00, 0x00, 0x02, 0x06, 0x07, 0x08, 0x0B, 0x2F, 0x00, 0x00, 0x01,   0x02, 0x05, 0x07, 0x08, 0x0A, 0x20, 0x00, 0x00, 0x01, 0x03, 0x08, 0x0A, 0x0C, 0x0E, 0x2F, 0x00,   0x00, 0x02, 0x04, 0x09, 0x0A, 0x0C, 0x0E, 0x28, 0x00, 0x00, 0x03, 0x05, 0x09, 0x0A, 0x0C, 0x0E,   0x16, 0x00, 0x01, 0x03, 0x05, 0x0A, 0x0E, 0x12, 0x16, 0x1F, 0x00, 0x02, 0x05, 0x06, 0x0A, 0x0A,   0x0A, 0x0C, 0x2E, 0x00, 0x02, 0x05, 0x07, 0x0C, 0x0E, 0x0F, 0x12, 0x2C, 0x01, 0x01, 0x04, 0x05,   0x07, 0x07, 0x08, 0x09, 0x0F, 0x01, 0x02, 0x02, 0x02, 0x04, 0x05, 0x07, 0x09, 0x1A, 0x01, 0x02,   0x02, 0x03, 0x06, 0x07, 0x07, 0x08, 0x2F, 0x01, 0x02, 0x02, 0x03, 0x06, 0x08, 0x0A, 0x0D, 0x16,   0x01, 0x03, 0x04, 0x07, 0x0D, 0x11, 0x15, 0x18, 0x29, 0x01, 0x04, 0x00, 0x04, 0x0A, 0x0C, 0x0D,   0x0E, 0x11, 0x02, 0x03, 0x03, 0x03, 0x06, 0x08, 0x0A, 0x0D, 0x30, 0x02, 0x03, 0x03, 0x04, 0x09,   0x0C, 0x0E, 0x11, 0x2F, 0x02, 0x03, 0x03, 0x05, 0x0A, 0x0C, 0x0E, 0x11, 0x19, 0x02, 0x03, 0x05,   0x07, 0x08, 0x09, 0x09, 0x09, 0x0D, 0x02, 0x03, 0x05, 0x09, 0x10, 0x15, 0x19, 0x1C, 0x21, 0x02,   0x04, 0x05, 0x08, 0x0C, 0x0E, 0x11, 0x13, 0x1A, 0x02, 0x04, 0x06, 0x08, 0x0C, 0x0D, 0x0D, 0x0F,   0x14, 0x02, 0x04, 0x07, 0x0C, 0x14, 0x1A, 0x1E, 0x20, 0x23, 0x03, 0x03, 0x05, 0x06, 0x0C, 0x0E,   0x10, 0x13, 0x22, 0x03, 0x04, 0x04, 0x05, 0x07, 0x09, 0x0A, 0x0B, 0x30, 0x03, 0x04, 0x05, 0x06,   0x08, 0x09, 0x0A, 0x0B, 0x10, 0x03, 0x05, 0x05, 0x05, 0x07, 0x09, 0x0A, 0x0D, 0x23, 0x03, 0x05,   0x05, 0x07, 0x0A, 0x0C, 0x0D, 0x0F, 0x31, 0x03, 0x05, 0x07, 0x07, 0x08, 0x07, 0x09, 0x0C, 0x15,   0x03, 0x05, 0x07, 0x08, 0x0C, 0x0E, 0x0F, 0x0F, 0x18, 0x03, 0x05, 0x07, 0x0A, 0x10, 0x15, 0x18,   0x1B, 0x2C, 0x03, 0x05, 0x08, 0x0E, 0x15, 0x1A, 0x1C, 0x1D, 0x2A, 0x03, 0x06, 0x0A, 0x0D, 0x12,   0x13, 0x14, 0x16, 0x1B, 0x03, 0x06, 0x0B, 0x10, 0x18, 0x1B, 0x1C, 0x1D, 0x1F, 0x04, 0x05, 0x04,   0x03, 0x04, 0x06, 0x08, 0x0B, 0x12, 0x04, 0x06, 0x05, 0x06, 0x09, 0x0A, 0x0C, 0x0E, 0x14, 0x04,   0x06, 0x07, 0x06, 0x06, 0x06, 0x07, 0x08, 0x2E, 0x04, 0x06, 0x07, 0x09, 0x0D, 0x10, 0x12, 0x14,   0x30, 0x04, 0x06, 0x07, 0x09, 0x0E, 0x11, 0x14, 0x17, 0x1F, 0x04, 0x06, 0x09, 0x0B, 0x0E, 0x0F,   0x0F, 0x11, 0x15, 0x04, 0x08, 0x0D, 0x14, 0x1B, 0x20, 0x23, 0x24, 0x26, 0x05, 0x06, 0x06, 0x04,   0x05, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x07, 0x08, 0x09, 0x09, 0x0A, 0x0C, 0x31, 0x05, 0x08,   0x09, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x2A, 0x05, 0x08, 0x0A, 0x0C, 0x0F, 0x10, 0x11, 0x13, 0x2A,   0x05, 0x08, 0x0C, 0x11, 0x1A, 0x1F, 0x20, 0x21, 0x2C, 0x05, 0x09, 0x0D, 0x10, 0x14, 0x16, 0x17,   0x17, 0x23, 0x06, 0x08, 0x08, 0x07, 0x06, 0x05, 0x06, 0x08, 0x0F, 0x06, 0x08, 0x08, 0x08, 0x09,   0x0A, 0x0C, 0x10, 0x18, 0x06, 0x08, 0x08, 0x09, 0x0A, 0x0A, 0x0B, 0x0B, 0x0D, 0x06, 0x08, 0x0A,   0x0D, 0x13, 0x15, 0x18, 0x1A, 0x20, 0x06, 0x09, 0x0A, 0x0B, 0x0D, 0x0D, 0x0E, 0x10, 0x31, 0x07,   0x09, 0x09, 0x0A, 0x0D, 0x0E, 0x10, 0x13, 0x1B, 0x07, 0x0A, 0x0C, 0x0D, 0x10, 0x10, 0x11, 0x11,   0x1B, 0x07, 0x0A, 0x0C, 0x0E, 0x11, 0x13, 0x14, 0x16, 0x30, 0x08, 0x09, 0x0A, 0x09, 0x0A, 0x0B,   0x0B, 0x0B, 0x13, 0x08, 0x0B, 0x0C, 0x0C, 0x0D, 0x0D, 0x0D, 0x0D, 0x11, 0x08, 0x0B, 0x0D, 0x0E,   0x10, 0x11, 0x13, 0x14, 0x1B, 0x08, 0x0C, 0x11, 0x16, 0x1A, 0x1C, 0x1D, 0x1E, 0x21, 0x0A, 0x0E,   0x10, 0x13, 0x15, 0x16, 0x16, 0x18, 0x1C, 0x0A, 0x0F, 0x11, 0x12, 0x15, 0x16, 0x17, 0x19, 0x2B,   0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x02, 0x00,   0x00, 0x00, 0x01, 0x00, 0x04, 0x00, 0x03, 0x00, 0x05, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, 0x00, 0x07, 0x00, 0x03, 0x00};

		Mai_U32 tmptable[0x20];
		tmptable[0] = chn_info->uk1c718;
		for (Mai_U32 a0 = 1; a0 < table_s0[chn_info->joint_chn_info->num_band_splited_used - 1] + 1; a0++)
		{
			tmptable[a0] = chn_info->uk1c718 - table_s1[chn_info->uk1c714 * 9 + a0 - 1];
		}

		for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			chn_info->table1[a0] = tmptable[table_s0[a0]];
		}
	}
	
	return 0;
}

//used in decodeTable1 0:0 1:0
Mai_Status MAPCDSF_decodeTable1_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
	{
		chn_info->table1[a0] = mbr0->getWithI32Buffer(6);
	}
	
	return rs;
}

//used in decodeTable1 0:1
Mai_Status MAPCDSF_decodeTable1_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	Mai_I32 tmp3 = mbr0->getWithI32Buffer(2);

	if (tmp3 == 3)
	{
		chn_info->uk1c718 = mbr0->getWithI32Buffer(6);
		chn_info->uk1c714 = mbr0->getWithI32Buffer(6);

		MAPCDSF_initTable1(chn_info);

		Mai_I32 uk1c700 = mbr0->getWithI32Buffer(5);

		Mai_I32 uk1c704 = mbr0->getWithI32Buffer(2);

		Mai_I32 uk1c708 = mbr0->getWithI32Buffer(4) - 7;

		for (Mai_U32 a0 = 0; a0 < uk1c700; a0++)
		{
			chn_info->table1[a0] += mbr0->getWithI32Buffer(4) - 7;
		}

		if (uk1c704)
		{
			for (Mai_U32 a0 = uk1c700; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
			{
				chn_info->table1[a0] += mbr0->getWithI32Buffer(uk1c704);
			}
		}

		for (Mai_U32 a0 = uk1c700; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			chn_info->table1[a0] += uk1c708;
		}

		for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			chn_info->table1[a0] &= 0x3F;
		}
	}
	else
	{
		Mai_I32 uk1c700 = mbr0->getWithI32Buffer(5);

		Mai_I32 uk1c704 = mbr0->getWithI32Buffer(3);

		Mai_I32 uk1c708 = mbr0->getWithI32Buffer(6);

		for (Mai_U32 a0 = 0; a0 < uk1c700; a0++)
		{
			chn_info->table1[a0] = mbr0->getWithI32Buffer(6);
		}

		if (!uk1c704)
		{
			for (Mai_U32 a0 = uk1c700; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
			{
				chn_info->table1[a0] = uk1c708;
			}
		}
		else
		{
			for (Mai_U32 a0 = uk1c700; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
			{
				chn_info->table1[a0] = mbr0->getWithI32Buffer(uk1c704) + uk1c708;
			}
		}

		if (tmp3)
		{
			Mai_U32 table_tmpx[] = {0, 0, 1, 1,        2, 2, 3, 3,         4, 4, 5, 5,         6, 6, 7, 7,        8, 8, 9, 9,       10, 10, 11, 11,        12, 12, 13, 13,       14, 14, 15, 15,        0, 0, 0, 1,       1, 1, 2, 2,         2, 3, 3, 3,      4, 4, 4, 5,         5, 5, 6, 6,         6, 7, 7, 7,         8, 8, 8, 9,         9, 9, 10, 10};
			for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
			{
				chn_info->table1[a0] -= table_tmpx[(tmp3 - 1) * 0x20 + a0];
			}
		}

	}
	
	return rs;
}

//used in decodeTable1 0:2
Mai_Status MAPCDSF_decodeTable1_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	Mai_I32 uk1c70c = mbr0->getWithI32Buffer(2);
	chn_info->uk1c718 = mbr0->getWithI32Buffer(6);
	chn_info->uk1c714 = mbr0->getWithI32Buffer(6);
	
	MAPCDSF_initTable1(chn_info);

	if (chn_info->joint_chn_info->num_band_splited_used)
	{
		MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = &MAPCDSD_huff_table1_2[uk1c70c];
		for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			Mai_U32 value_tmp = (Mai_U32)MAPCDSF_getHuffValue(huff_table_now, mbr0);
			if (value_tmp & 0x8) value_tmp |= 0xFFFFFFF0;
			else value_tmp &= 0xF;
			chn_info->table1[a0] += value_tmp;
			chn_info->table1[a0] &= 0x3F;
		}
	}
	
	return rs;
}

//used in decodeTable1 0:3
Mai_Status MAPCDSF_decodeTable1_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	Mai_I32 tmp3 = mbr0->getWithI32Buffer(2);

	Mai_U32 huff_table_type2 = mbr0->getWithI32Buffer(2);

	if (tmp3 == 3)
	{
		chn_info->uk1c718 = mbr0->getWithI32Buffer(6);
		chn_info->uk1c714 = mbr0->getWithI32Buffer(6);

		MAPCDSF_initTable1(chn_info);

		Mai_U32 table_tmp2x[0x40];
		table_tmp2x[0] = mbr0->getWithI32Buffer(4);
		table_tmp2x[0] -= 8;
		table_tmp2x[0] &= 0x3F;

		MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = &MAPCDSD_huff_table1_2[huff_table_type2];
		for (Mai_U32 a0 = 1; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			Mai_U32 value_tmp = (Mai_U32)MAPCDSF_getHuffValue(huff_table_now, mbr0);
			if (value_tmp & 0x8) value_tmp |= 0xFFFFFFF0;
			else value_tmp &= 0xF;

			table_tmp2x[a0] = table_tmp2x[a0 - 1] + value_tmp;
			table_tmp2x[a0] &= 0x3F;
		}

		for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			chn_info->table1[a0] += table_tmp2x[a0];
			chn_info->table1[a0] &= 0x3F;
		}
	}
	else
	{
		chn_info->table1[0] = mbr0->getWithI32Buffer(6);

		for (Mai_U32 a0 = 1; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		{
			chn_info->table1[a0] = chn_info->table1[a0 - 1] + MAPCDSF_getHuffValue(&MAPCDSD_huff_table1[huff_table_type2], mbr0);
			chn_info->table1[a0] &= 0x3F;
		}

		if (tmp3)
		{
			Mai_U32 table_tmpx[] = {0, 0, 1, 1,        2, 2, 3, 3,         4, 4, 5, 5,         6, 6, 7, 7,        8, 8, 9, 9,       10, 10, 11, 11,        12, 12, 13, 13,       14, 14, 15, 15,        0, 0, 0, 1,       1, 1, 2, 2,         2, 3, 3, 3,      4, 4, 4, 5,         5, 5, 6, 6,         6, 7, 7, 7,         8, 8, 8, 9,         9, 9, 10, 10};
			for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
			{
				chn_info->table1[a0] -= table_tmpx[(tmp3 - 1) * 0x20 + a0];
			}
		}
	}
	
	return rs;
}

//used in decodeTable1 1:1
Mai_Status MAPCDSF_decodeTable1_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	Mai_U32 huff_table_type3 = mbr0->getWithI32Buffer(2);

	for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
	{
		chn_info->table1[a0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table1[huff_table_type3], mbr0);
		chn_info->table1[a0] += chn_info->chn_ref->table1[a0];
		chn_info->table1[a0] &= 0x3F;

	}
	
	return rs;
}

//used in decodeTable1 1:2
Mai_Status MAPCDSF_decodeTable1_Route5(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	Mai_U32 huff_table_type3 = mbr0->getWithI32Buffer(2);

	chn_info->table1[0] = chn_info->chn_ref->table1[0] + MAPCDSF_getHuffValue(&MAPCDSD_huff_table1[huff_table_type3], mbr0);
	chn_info->table1[0] &= 0x3F;

	for (Mai_U32 a0 = 1; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
	{
		chn_info->table1[a0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table1[huff_table_type3], mbr0);
		chn_info->table1[a0] = chn_info->chn_ref->table1[a0] - chn_info->chn_ref->table1[a0 - 1] + chn_info->table1[a0 - 1] + chn_info->table1[a0];
		chn_info->table1[a0] &= 0x3F;
	}
	
	return rs;
}

//used in decodeTable1 1:3
Mai_Status MAPCDSF_decodeTable1_Route6(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;
	
	for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
		chn_info->table1[a0] = chn_info->chn_ref->table1[a0];
	
	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable1_func_list0[8] = 
	{
		MAPCDSF_decodeTable1_Route0,
		MAPCDSF_decodeTable1_Route1,
		MAPCDSF_decodeTable1_Route2,
		MAPCDSF_decodeTable1_Route3,
		MAPCDSF_decodeTable1_Route0,
		MAPCDSF_decodeTable1_Route4,
		MAPCDSF_decodeTable1_Route5,
		MAPCDSF_decodeTable1_Route6
	};



//used in decodeTable2 0:0 1:0
Mai_Status MAPCDSF_decodeTable2_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 read_counter = chn_info->joint_chn_info->num_band_splited_used;
	if (mbr0->getWithI32Buffer(1))
	{
		read_counter = mbr0->getWithI32Buffer(5);
	}

	for (Mai_U32 a0 = 0; a0 < read_counter; a0++)
	{
		if (chn_info->check_table0[a0] == 1) chn_info->table2[a0] = mbr0->getWithI32Buffer( ( (chn_info->joint_chn_info->var90) ? 3 : 2 ) ); //tmp4
		else if (chn_info->check_table0[a0] == 0) chn_info->table2[a0] = 0;
		else if (chn_info->check_table0[a0] == 2) chn_info->table2[a0] = mbr0->getWithI32Buffer(1);
		else
		{
			rs = -12;
			break;
		}
	}

	return rs;
}

//used in decodeTable2 0:1 1:1
Mai_Status MAPCDSF_decodeTable2_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 read_counter = chn_info->joint_chn_info->num_band_splited_used;
	if (mbr0->getWithI32Buffer(1))
	{
		read_counter = mbr0->getWithI32Buffer(5);
	}

	for (Mai_U32 a0 = 0; a0 < read_counter; a0++)
	{
		if (chn_info->check_table0[a0] == 1) chn_info->table2[a0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table2[chn_info->joint_chn_info->var90], mbr0); //tmp4
		else if (chn_info->check_table0[a0] == 0) chn_info->table2[a0] = 0;
		else if (chn_info->check_table0[a0] == 2) chn_info->table2[a0] = mbr0->getWithI32Buffer(1);
		else
		{
			rs = -13;
			break;
		}
	}

	return rs;
}

//used in decodeTable2 0:2 1:2
Mai_Status MAPCDSF_decodeTable2_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 read_counter = chn_info->joint_chn_info->num_band_splited_used;
	if (mbr0->getWithI32Buffer(1))
	{
		read_counter = mbr0->getWithI32Buffer(5);
	}

	MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now0;
	MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now1;

	if (!chn_info->joint_chn_info->var90)
	{
		huff_table_now0 = &MAPCDSD_huff_table2[0];
		huff_table_now1 = &MAPCDSD_huff_table2[0];
	}
	else
	{
		huff_table_now0 = &MAPCDSD_huff_table2[1];
		huff_table_now1 = &MAPCDSD_huff_table2[2];
	}

	Mai_U32 ptmp = 0;
	while (read_counter)
	{
		if (chn_info->check_table0[0] == 1)
		{
			chn_info->table2[0] = MAPCDSF_getHuffValue(huff_table_now0, mbr0);
			ptmp = chn_info->table2[0];
		}
		else if (chn_info->check_table0[0] == 0) chn_info->table2[0] = 0;
		else if (chn_info->check_table0[0] == 2) chn_info->table2[0] = mbr0->getWithI32Buffer(1);
		else
		{
			rs = -14;
			break;
		}

		for (Mai_U32 a0 = 1; a0 < read_counter; a0++)
		{
			if (chn_info->check_table0[a0] == 1)
			{
				chn_info->table2[a0] = MAPCDSF_getHuffValue(huff_table_now1, mbr0);
				chn_info->table2[a0] += ptmp;
				chn_info->table2[a0] &= huff_table_now1->mask;
				ptmp = chn_info->table2[a0];
			}
			else if (chn_info->check_table0[a0] == 0) chn_info->table2[a0] = 0;
			else if (chn_info->check_table0[a0] == 2) chn_info->table2[a0] = mbr0->getWithI32Buffer(1);
			else
			{
				rs = -15;
				break;
			}
		}

		break;
	}

	return rs;
}

//used in decodeTable2 0:3
Mai_Status MAPCDSF_decodeTable2_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->joint_chn_info->num_band_splited_used; a0++)
	{
		chn_info->table2[a0] = 0;
	}

	return rs;
}

//used in decodeTable2 1:3
Mai_Status MAPCDSF_decodeTable2_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 read_counter = chn_info->joint_chn_info->num_band_splited_used;
	if (mbr0->getWithI32Buffer(1))
	{
		read_counter = mbr0->getWithI32Buffer(5);
	}

	for (Mai_U32 a0 = 0; a0 < read_counter; a0++)
	{
		if (chn_info->check_table0[a0] == 1)
		{
			chn_info->table2[a0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table2_2[chn_info->joint_chn_info->var90], mbr0); //tmp4
			chn_info->table2[a0] += chn_info->chn_ref->table2[a0];
			chn_info->table2[a0] &= MAPCDSD_huff_table2_2[chn_info->joint_chn_info->var90].mask;
		}
		else if (chn_info->check_table0[a0] == 0) chn_info->table2[a0] = 0;
		else if (chn_info->check_table0[a0] == 2) chn_info->table2[a0] = mbr0->getWithI32Buffer(1);
		else
		{
			rs = -17;
			break;
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable2_func_list0[8] = 
	{
		MAPCDSF_decodeTable2_Route0,
		MAPCDSF_decodeTable2_Route1,
		MAPCDSF_decodeTable2_Route2,
		MAPCDSF_decodeTable2_Route3,
		MAPCDSF_decodeTable2_Route0,
		MAPCDSF_decodeTable2_Route1,
		MAPCDSF_decodeTable2_Route2,
		MAPCDSF_decodeTable2_Route4
	};

//used in decodeTable3
Mai_Status MAPCDSF_decodeTable3Sub0(MaiBitReader *mbr0, Mai_I16 *buf_to_read, Mai_U32 num_to_read, MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now)
{
	Mai_Status rs = 0;

	if (huff_table_now->uk3 == 1)
	{
		Mai_U32 tcounter0 = 0;
		for (Mai_U32 b0 = 0; b0 < (num_to_read >> (huff_table_now->uk4)); b0++)
		{
			Mai_U32 group_value = MAPCDSF_getHuffValue(huff_table_now, mbr0);

			for (Mai_U32 b1 = 0; b1 < huff_table_now->uk2; b1++)
			{
				Mai_U32 value_now = group_value >> ((huff_table_now->uk2 - b1 - 1) * huff_table_now->uk6);
				value_now &= huff_table_now->mask;

				if (!huff_table_now->uk5)
				{
					if (value_now & ( 1 << (huff_table_now->uk6 - 1) ) )
						buf_to_read[tcounter0++] = (Mai_I16)(value_now | ( ~( ( 1 << (huff_table_now->uk6) ) - 1 ) ));
					else
						buf_to_read[tcounter0++] = (Mai_I16)(value_now);
				}
				else
				{
					if ( (value_now) && (mbr0->getWithI32Buffer(1)) )
						buf_to_read[tcounter0++] = ((Mai_I16)(value_now)) * (-1);
					else
						buf_to_read[tcounter0++] = (Mai_I16)(value_now);
				}
			}
		}
	}
	else if (huff_table_now->uk3 > 1)
	{
		Mai_U32 tcounter0 = 0;
		for (Mai_U32 b0 = 0; b0 < (num_to_read >> (huff_table_now->uk4)); b0 += huff_table_now->uk3)
		{
			Mai_U32 l320 = mbr0->getWithI32Buffer(1);
			if (!l320)
			{
				for (Mai_U32 b1 = 0; b1 < huff_table_now->uk3 * huff_table_now->uk2; b1++)
				{
					buf_to_read[tcounter0++] = 0;
				}
			}
			else for (Mai_U32 b2 = 0; b2 < huff_table_now->uk3; b2++)
			{
				Mai_U32 group_value = MAPCDSF_getHuffValue(huff_table_now, mbr0);

				for (Mai_U32 b1 = 0; b1 < huff_table_now->uk2; b1++)
				{
					Mai_U32 value_now = group_value >> ((huff_table_now->uk2 - b1 - 1) * huff_table_now->uk6);
					value_now &= huff_table_now->mask;

					if (!huff_table_now->uk5)
					{
						if (value_now & ( 1 << (huff_table_now->uk6 - 1) ) )
							buf_to_read[tcounter0++] = (Mai_I16)(value_now | ( ~( ( 1 << (huff_table_now->uk6) ) - 1 ) ));
						else
							buf_to_read[tcounter0++] = (Mai_I16)(value_now);
					}
					else
					{
						if ( (value_now) && (mbr0->getWithI32Buffer(1)) )
							buf_to_read[tcounter0++] = ((Mai_I16)(value_now)) * (-1);
						else
							buf_to_read[tcounter0++] = (Mai_I16)(value_now);
					}
				}
			}
		}
	}
	else
	{
		rs = -20;
	}

	return rs;
}


//used in MAPCDSF_decodeACC2MainSub0 0:0 1:0
Mai_Status MAPCDSF_decodeACC2Main0_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		chn_info->acc_data_now->table[a0].num_acc = mbr0->getWithI32Buffer(3);
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub0 0:1 1:1
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_0[];
Mai_Status MAPCDSF_decodeACC2Main0_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		chn_info->acc_data_now->table[a0].num_acc = MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_0[0], mbr0);
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub0 0:2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_0[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_1[];
Mai_Status MAPCDSF_decodeACC2Main0_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	chn_info->acc_data_now->table[0].num_acc = MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_0[0], mbr0);

	for (Mai_U32 a0 = 1; a0 < chn_info->uk1b450; a0++)
	{
		chn_info->acc_data_now->table[a0].num_acc = chn_info->acc_data_now->table[a0 - 1].num_acc + MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_1[0], mbr0);
		chn_info->acc_data_now->table[a0].num_acc &= 0x7;
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub0 0:3
Mai_Status MAPCDSF_decodeACC2Main0_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 uk1b458 = mbr0->getWithI32Buffer(2);
	Mai_U32 uk1b45c = mbr0->getWithI32Buffer(3);

	if (uk1b458)
	{
		for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
		{
			chn_info->acc_data_now->table[a0].num_acc = uk1b45c + mbr0->getWithI32Buffer(uk1b458);
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
		{
			chn_info->acc_data_now->table[a0].num_acc = uk1b45c;
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub0 1:2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_1[];
Mai_Status MAPCDSF_decodeACC2Main0_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		chn_info->acc_data_now->table[a0].num_acc = 
			chn_info->chn_ref->acc_data_now->table[a0].num_acc
			+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_1[0], mbr0);

		chn_info->acc_data_now->table[a0].num_acc &= 0x7;
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub0 1:3
Mai_Status MAPCDSF_decodeACC2Main0_Route5(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		chn_info->acc_data_now->table[a0].num_acc = 
			chn_info->chn_ref->acc_data_now->table[a0].num_acc;
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeACC2Main_func_list0[8] = 
	{
		MAPCDSF_decodeACC2Main0_Route0,
		MAPCDSF_decodeACC2Main0_Route1,
		MAPCDSF_decodeACC2Main0_Route2,
		MAPCDSF_decodeACC2Main0_Route3,
		MAPCDSF_decodeACC2Main0_Route0,
		MAPCDSF_decodeACC2Main0_Route1,
		MAPCDSF_decodeACC2Main0_Route4,
		MAPCDSF_decodeACC2Main0_Route5
	};


//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	MAPCDSF_decodeACC2Main_func_list0[chn_info->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
		(mbr0, chn_info);

	for (Mai_U32 a0 = 0; a0 < 0x10; a0++)
	{
		if (chn_info->acc_data_now->table[a0].num_acc > 0x7)
		{
			rs = -21;
			break;
		}
	}

	return rs;
}


//used in MAPCDSF_decodeACC2MainSub1 0:0 1:0
Mai_Status MAPCDSF_decodeACC2Main1_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			chn_info->acc_data_now->table[a0].data1[a1] = 
				mbr0->getWithI32Buffer(4);
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 0:1
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_2[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_3[];
Mai_Status MAPCDSF_decodeACC2Main1_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
		{
			chn_info->acc_data_now->table[a0].data1[0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_2[0], mbr0);

			for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					chn_info->acc_data_now->table[a0].data1[a1 - 1]
					+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_3[0], mbr0);
						
				chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 0:2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_2[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_3[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_4[];
Mai_Status MAPCDSF_decodeACC2Main1_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if ((Mai_U32)chn_info->acc_data_now->table[0].num_acc)
	{
		chn_info->acc_data_now->table[0].data1[0] = MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_2[0], mbr0);
		for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[0].num_acc; a1++)
		{
			chn_info->acc_data_now->table[0].data1[a1] = 
				chn_info->acc_data_now->table[0].data1[a1 - 1]
				+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_3[0], mbr0);
						
			chn_info->acc_data_now->table[0].data1[a1] &= 0xF;
		}
	}

	for (Mai_U32 a0 = 1; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			if (a1 < (Mai_U32)chn_info->acc_data_now->table[a0 - 1].num_acc)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					chn_info->acc_data_now->table[a0 - 1].data1[a1]
					+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_4[0], mbr0);

				chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
			}
			else
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					7
					+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_4[0], mbr0);

				chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 0:3
Mai_Status MAPCDSF_decodeACC2Main1_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 uk1b464 = mbr0->getWithI32Buffer(2);
	Mai_U32 uk1b468 = mbr0->getWithI32Buffer(4);

	if (uk1b464)
	{
		for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
		{
			for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					uk1b468
					+ mbr0->getWithI32Buffer(uk1b464);
			}
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
		{
			for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					uk1b468;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 1:1
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_5[];
Mai_Status MAPCDSF_decodeACC2Main1_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			if (a1 < (Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					chn_info->chn_ref->acc_data_now->table[a0].data1[a1]
					+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_5[0], mbr0);

				chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
			}
			else
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					0x7
					+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_5[0], mbr0);

				chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 1:2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_2[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_3[];
Mai_Status MAPCDSF_decodeACC2Main1_Route5(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
		{
			Mai_U32 uk1b46c_x = mbr0->getWithI32Buffer(1);

			if (uk1b46c_x)
			{
				chn_info->acc_data_now->table[a0].data1[0] = 
					MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_2[0], mbr0);

				for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
				{
					chn_info->acc_data_now->table[a0].data1[a1] = 
						chn_info->acc_data_now->table[a0].data1[a1 - 1]
						+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_3[0], mbr0);

					chn_info->acc_data_now->table[a0].data1[a1] &= 0xF;
				}
			}
			else
			{
				for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
				{
					if (a1 < (Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
					{
						chn_info->acc_data_now->table[a0].data1[a1] = 
							chn_info->chn_ref->acc_data_now->table[a0].data1[a1];
					}
					else
					{
						chn_info->acc_data_now->table[a0].data1[a1] = 
							0x7;
					}
				}
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub1 1:3
Mai_Status MAPCDSF_decodeACC2Main1_Route6(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			if (a1 < (Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					chn_info->chn_ref->acc_data_now->table[a0].data1[a1];
			}
			else
			{
				chn_info->acc_data_now->table[a0].data1[a1] = 
					0x7;
			}
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeACC2Main_func_list1[8] = 
	{
		MAPCDSF_decodeACC2Main1_Route0,
		MAPCDSF_decodeACC2Main1_Route1,
		MAPCDSF_decodeACC2Main1_Route2,
		MAPCDSF_decodeACC2Main1_Route3,
		MAPCDSF_decodeACC2Main1_Route0,
		MAPCDSF_decodeACC2Main1_Route4,
		MAPCDSF_decodeACC2Main1_Route5,
		MAPCDSF_decodeACC2Main1_Route6
	};

//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	MAPCDSF_decodeACC2Main_func_list1[chn_info->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
		(mbr0, chn_info);

	//rep check
	for (Mai_I32 a0 = 0; a0 < 0x10; a0++)
	{
		for (Mai_I32 a1 = 0; a1 < 0x7; a1++)
		{
			if (chn_info->acc_data_now->table[a0].data1[a1] >= 0x10)
			{
				rs = -1;
				break;
			}
		}

		if (rs) break;

		for (Mai_I32 a1 = 0; a1 < (Mai_I32)chn_info->acc_data_now->table[a0].num_acc - 1; a1++)
		{
			if (chn_info->acc_data_now->table[a0].data1[a1]
				==
				chn_info->acc_data_now->table[a0].data1[a1 + 1])
			{
				rs = -1;
				break;
			}
		}

		if (rs) break;
	}

	return rs;
}


Mai_Status MAPCDSF_parseACCDataMemberUsingBitRead(Mai_U32 a0, Mai_U32 a1, MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if (a1 == 0)
	{
		chn_info->acc_data_now->table[a0].data0[0] = 
			mbr0->getWithI32Buffer(5);
	}
	else
	{
		if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			< 0xF)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				mbr0->getWithI32Buffer(5);
		}
		else if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			< 0x17)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				(chn_info->acc_data_now->table[a0].data0[a1 - 1])
				+ mbr0->getWithI32Buffer(4)
				+ 1;
		}
		else if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			< 0x1B)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				(chn_info->acc_data_now->table[a0].data0[a1 - 1])
				+ mbr0->getWithI32Buffer(3)
				+ 1;
		}
		else if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			< 0x1D)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				(chn_info->acc_data_now->table[a0].data0[a1 - 1])
				+ mbr0->getWithI32Buffer(2)
				+ 1;
		}
		else if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			== 0x1D)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				(chn_info->acc_data_now->table[a0].data0[a1 - 1])
				+ mbr0->getWithI32Buffer(1)
				+ 1;
		}
		else if ( ((Mai_U32)chn_info->acc_data_now->table[a0].data0[a1 - 1])
			== 0x1E)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				0x1F;
		}

	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 0:0 1:0
Mai_Status MAPCDSF_decodeACC2Main2_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			MAPCDSF_parseACCDataMemberUsingBitRead(a0, a1, mbr0, chn_info);
		}
	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_6[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_7[];
Mai_Status MAPCDSF_parseACCDataMemberUsingHuffTable0(Mai_U32 a0, MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
	{
		chn_info->acc_data_now->table[a0].data0[0] = 
			mbr0->getWithI32Buffer(5);

		for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = 0;
			
			if ( ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1])
				- ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1 - 1])
				<= 0)
			{
				huff_table_now = &MAPCDSD_huff_table_global_6[0];
			}
			else
			{
				huff_table_now = &MAPCDSD_huff_table_global_7[0];
			}

			chn_info->acc_data_now->table[a0].data0[a1] = 
				chn_info->acc_data_now->table[a0].data0[a1 - 1]
				+ MAPCDSF_getHuffValue(huff_table_now, mbr0);
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 0:1
Mai_Status MAPCDSF_decodeACC2Main2_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		MAPCDSF_parseACCDataMemberUsingHuffTable0(a0, mbr0, chn_info);
	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_6[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_7[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_8[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_9[];
Mai_Status MAPCDSF_parseACCDataMemberUsingHuffTable1(Mai_U32 a0, MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
	{
		if ((Mai_U32)chn_info->acc_data_now->table[a0 - 1].num_acc)
		{
			chn_info->acc_data_now->table[a0].data0[0] = 
				chn_info->acc_data_now->table[a0 - 1].data0[0]
				+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_8[0], mbr0);

			chn_info->acc_data_now->table[a0].data0[0] &= 0x1F;
		}
		else
		{
			chn_info->acc_data_now->table[a0].data0[0] = 
				MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_8[0], mbr0);
		}

		for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = 0;

			Mai_U32 check_value0 = 0;
			if (a1 >= ((Mai_U32)chn_info->acc_data_now->table[a0 - 1].num_acc))
				check_value0 = 1;

			if ( ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1])
				- ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1 - 1])
				<= 0)
			{
				if (check_value0)
				{
					huff_table_now = &MAPCDSD_huff_table_global_6[0];
				}
				else
				{
					huff_table_now = &MAPCDSD_huff_table_global_8[0];
				}
			}
			else
			{
				if (check_value0)
				{
					huff_table_now = &MAPCDSD_huff_table_global_7[0];
				}
				else
				{
					huff_table_now = &MAPCDSD_huff_table_global_9[0];
				}
			}
								
			if (check_value0)
			{
				chn_info->acc_data_now->table[a0].data0[a1] = 
					chn_info->acc_data_now->table[a0].data0[a1 - 1]
					+ MAPCDSF_getHuffValue(huff_table_now, mbr0);
			}
			else
			{
				chn_info->acc_data_now->table[a0].data0[a1] = 
					chn_info->acc_data_now->table[a0 - 1].data0[a1]
					+ MAPCDSF_getHuffValue(huff_table_now, mbr0);

				chn_info->acc_data_now->table[a0].data0[a1] &= 0x1F;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 0:2
Mai_Status MAPCDSF_decodeACC2Main2_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[0].num_acc; a1++)
	{
		MAPCDSF_parseACCDataMemberUsingBitRead(0, a1, mbr0, chn_info);
	}

	for (Mai_U32 a0 = 1; a0 < chn_info->uk1b450; a0++)
	{
		MAPCDSF_parseACCDataMemberUsingHuffTable1(a0, mbr0, chn_info);
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 0:3
Mai_Status MAPCDSF_decodeACC2Main2_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 uk1b4b0 = mbr0->getWithI32Buffer(2) + 1;
	Mai_U32 uk1b4b4 = mbr0->getWithI32Buffer(5);

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			chn_info->acc_data_now->table[a0].data0[a1] = 
				mbr0->getWithI32Buffer(uk1b4b0)
				+ uk1b4b4
				+ a1;
		}
	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_6[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_7[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_10[];
Mai_Status MAPCDSF_parseACCDataMemberUsingHuffTable2(Mai_U32 a0, MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
	{
		if ((Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
		{
			chn_info->acc_data_now->table[a0].data0[0] = 
				chn_info->chn_ref->acc_data_now->table[a0].data0[0]
				+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_10[0], mbr0);

			chn_info->acc_data_now->table[a0].data0[0] &= 0x1F;
		}
		else
		{
			chn_info->acc_data_now->table[a0].data0[0] = 
				MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_10[0], mbr0);
		}

		for (Mai_U32 a1 = 1; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			Mai_U32 check_value0 = 0;
			if (a1 >= (Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
				check_value0 = 1;

			if ( ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1])
				- ((Mai_I32)chn_info->acc_data_now->table[a0].data1[a1 - 1])
				<= 0)
			{
				MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now = 0;

				if (check_value0)
				{
					huff_table_now = &MAPCDSD_huff_table_global_6[0];
				}
				else
				{
					huff_table_now = &MAPCDSD_huff_table_global_10[0];
				}

				if (!check_value0)
				{
					chn_info->acc_data_now->table[a0].data0[a1] = 
						chn_info->chn_ref->acc_data_now->table[a0].data0[a1]
						+ MAPCDSF_getHuffValue(huff_table_now, mbr0);

					chn_info->acc_data_now->table[a0].data0[a1] &= 0x1F;
				}
				else
				{
					chn_info->acc_data_now->table[a0].data0[a1] = 
						chn_info->acc_data_now->table[a0].data0[a1 - 1]
						+ MAPCDSF_getHuffValue(huff_table_now, mbr0);
				}
			}
			else
			{
				if (!check_value0)
				{
					if (mbr0->getWithI32Buffer(1))
					{
						MAPCDSF_parseACCDataMemberUsingBitRead(a0, a1, mbr0, chn_info);
					}
					else
					{
						chn_info->acc_data_now->table[a0].data0[a1] = 
							chn_info->chn_ref->acc_data_now->table[a0].data0[a1];
					}
				}
				else
				{
					chn_info->acc_data_now->table[a0].data0[a1] = 
						chn_info->acc_data_now->table[a0].data0[a1 - 1]
						+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_7[0], mbr0);
				}
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 1:1
Mai_Status MAPCDSF_decodeACC2Main2_Route4(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		MAPCDSF_parseACCDataMemberUsingHuffTable2(a0, mbr0, chn_info);
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 1:2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_6[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_7[];
Mai_Status MAPCDSF_decodeACC2Main2_Route5(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		if ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
		{
			if ( ( ((Mai_U32)chn_info->acc_data_now->table[a0].num_acc)
				<= ((Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc) )
				&& (!mbr0->getWithI32Buffer(1))
				)
			{
				for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
				{
					chn_info->acc_data_now->table[a0].data0[a1] = 
						chn_info->chn_ref->acc_data_now->table[a0].data0[a1];
				}
			}
			else
			{
				MAPCDSF_parseACCDataMemberUsingHuffTable0(a0, mbr0, chn_info);
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC2MainSub2 1:3
Mai_Status MAPCDSF_decodeACC2Main2_Route6(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < chn_info->uk1b450; a0++)
	{
		for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_data_now->table[a0].num_acc; a1++)
		{
			if (a1 < (Mai_U32)chn_info->chn_ref->acc_data_now->table[a0].num_acc)
			{
				chn_info->acc_data_now->table[a0].data0[a1] = 
					chn_info->chn_ref->acc_data_now->table[a0].data0[a1];
			}
			else
			{
				MAPCDSF_parseACCDataMemberUsingBitRead(a0, a1, mbr0, chn_info);
			}
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeACC2Main_func_list2[8] = 
	{
		MAPCDSF_decodeACC2Main2_Route0,
		MAPCDSF_decodeACC2Main2_Route1,
		MAPCDSF_decodeACC2Main2_Route2,
		MAPCDSF_decodeACC2Main2_Route3,
		MAPCDSF_decodeACC2Main2_Route0,
		MAPCDSF_decodeACC2Main2_Route4,
		MAPCDSF_decodeACC2Main2_Route5,
		MAPCDSF_decodeACC2Main2_Route6
	};

//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	MAPCDSF_decodeACC2Main_func_list2[chn_info->chn_flag * 4 + mbr0->getWithI32Buffer(2)]
		(mbr0, chn_info);

	//rep check

	for (Mai_I32 a0 = 0; a0 < 0x10; a0++)
	{
		for (Mai_I32 a1 = 0; a1 < 0x7; a1++)
		{
			if ( (chn_info->acc_data_now->table[a0].data0[a1] < 0)
				||
				(chn_info->acc_data_now->table[a0].data0[a1] >= 0x20) )
			{
				rs = -1;
				break;
			}
		}

		if (rs) break;

		for (Mai_I32 a1 = 0; a1 < (Mai_I32)chn_info->acc_data_now->table[a0].num_acc - 1; a1++)
		{
			if (chn_info->acc_data_now->table[a0].data0[a1]
				>=
				chn_info->acc_data_now->table[a0].data0[a1 + 1])
			{
				rs = -1;
				break;
			}
		}

		if (rs) break;
	}

	return rs;
}



Mai_Status MAPCDSF_makeInnerPackTable0CheckTable(MaiAT3PlusCoreDecoderChnInfo *chn_info, Mai_I32 arg2)
{
	Mai_Status rs = 0;

	MaiAT3PlusCoreDecoderChnACCTable *acc_table_to_use = 
		(arg2) ? chn_info->acc_table_now : chn_info->acc_table_old;

	if (!chn_info->chn_flag)
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)acc_table_to_use->inner->unk2; a0++)
		{
			chn_info->inner_pack_table0_check_table[a0] = 1;
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)acc_table_to_use->inner->unk2; a0++)
		{
			if (!acc_table_to_use->inner->table_unk0.data[a0])
			{
				chn_info->inner_pack_table0_check_table[a0] = 1;
			}
			else
			{
				chn_info->inner_pack_table0_check_table[a0] = 0;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC6InnerSub0 0
Mai_Status MAPCDSF_decodeACC6Inner0_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			chn_info->acc_table_now->table[a0].unk[4] = 
				mbr0->getWithI32Buffer(1);
			if (chn_info->acc_table_now->table[a0].unk[4])
			{
				chn_info->acc_table_now->table[a0].unk[6] = 
					mbr0->getWithI32Buffer(5);
			}
			else
			{
				chn_info->acc_table_now->table[a0].unk[6] = 
					-1;
			}

			chn_info->acc_table_now->table[a0].unk[5] = 
				mbr0->getWithI32Buffer(1);

			if (chn_info->acc_table_now->table[a0].unk[5])
			{
				chn_info->acc_table_now->table[a0].unk[7] = 
					mbr0->getWithI32Buffer(5);
			}
			else
			{
				chn_info->acc_table_now->table[a0].unk[7] = 
					0x20;
			}
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC6InnerSub0 1
Mai_Status MAPCDSF_decodeACC6Inner0_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			chn_info->acc_table_now->table[a0].unk[4] = 
				chn_info->chn_ref->acc_table_now->table[a0].unk[4];
			chn_info->acc_table_now->table[a0].unk[5] = 
				chn_info->chn_ref->acc_table_now->table[a0].unk[5];
			chn_info->acc_table_now->table[a0].unk[6] = 
				chn_info->chn_ref->acc_table_now->table[a0].unk[6];
			chn_info->acc_table_now->table[a0].unk[7] = 
				chn_info->chn_ref->acc_table_now->table[a0].unk[7];
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeACC6InnerSub0_func_list0[2] = 
	{
		MAPCDSF_decodeACC6Inner0_Route0,
		MAPCDSF_decodeACC6Inner0_Route1
	};


//used in MAPCDSF_decodeACC6InnerSub0 0
Mai_Status MAPCDSF_decodeACC6Inner1_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			chn_info->acc_table_now->table[a0].num_uk = 
				mbr0->getWithI32Buffer(4);
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC6InnerSub0 1
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_12[];
Mai_Status MAPCDSF_decodeACC6Inner1_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			chn_info->acc_table_now->table[a0].num_uk = 
				MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_12[0], mbr0);
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC6InnerSub0 2
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_13[];
Mai_Status MAPCDSF_decodeACC6Inner1_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			Mai_I32 atmp0 = MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_13[0], mbr0);

			if (atmp0 & 0x4)
			{
				atmp0 |= (Mai_I32)0xFFFFFFF8;
			}
			else
			{
				atmp0 &= 0x7;
			}

			chn_info->acc_table_now->table[a0].num_uk = 
				chn_info->chn_ref->acc_table_now->table[a0].num_uk
				+ atmp0;

			chn_info->acc_table_now->table[a0].num_uk &= 0xF;
		}
	}

	return rs;
}

//used in MAPCDSF_decodeACC6InnerSub0 3
Mai_Status MAPCDSF_decodeACC6Inner1_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			chn_info->acc_table_now->table[a0].num_uk = 
				chn_info->chn_ref->acc_table_now->table[a0].num_uk;
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeACC6InnerSub0_func_list1[4] = 
	{
		MAPCDSF_decodeACC6Inner1_Route0,
		MAPCDSF_decodeACC6Inner1_Route1,
		MAPCDSF_decodeACC6Inner1_Route2,
		MAPCDSF_decodeACC6Inner1_Route3
	};

Mai_Status MAPCDSF_calcACCTableTableUnk3ByLastValue(MaiAT3PlusCoreDecoderChnACCTableTable *table, MaiBitReader *mbr0)
{
	Mai_Status rs = 0;

	for (Mai_I32 a0 = 0; a0 < table->num_uk; a0++)
	{
		if (!a0)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0xA);
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x200)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0xA);
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x300)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x9)
				+ 0x200;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x380)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x8)
				+ 0x300;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x3C0)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x7)
				+ 0x380;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x3E0)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x6)
				+ 0x3C0;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x3F0)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x5)
				+ 0x3E0;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x3F8)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x4)
				+ 0x3F0;
		}
		else if ( table->ptr0[a0 - 1].unk3
			< 0x3FC)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x3)
				+ 0x3F8;
		}
		else if (table->ptr0[a0 - 1].unk3
			< 0x3FE)
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x2)
				+ 0x3FC;
		}
		else
		{
			table->ptr0[a0].unk3 = mbr0->getWithI32Buffer(0x1)
				+ 0x3FE;
		}
	}

	return rs;
}

Mai_Status MAPCDSF_calcACCTableTableUnk3ByAfterValue(MaiAT3PlusCoreDecoderChnACCTableTable *table, MaiBitReader *mbr0)
{
	Mai_Status rs = 0;

	Mai_I32 btmp0 = table->num_uk - 1;

	while (btmp0 >= 0)
	{
		if (btmp0 == (table->num_uk - 1) )
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0xA);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 2)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x1);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 4)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x2);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 8)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x3);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x10)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x4);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x20)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x5);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x40)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x6);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x80)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x7);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x100)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x8);
		}
		else if ( table->ptr0[btmp0 + 1].unk3
			< 0x200)
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0x9);
		}
		else
		{
			table->ptr0[btmp0].unk3 = 
				mbr0->getWithI32Buffer(0xA);
		}

		btmp0--;
	}

	return rs;
}

//used in MAPCDSF_splitePack 0
Mai_Status MAPCDSF_splitePack0_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_U32 uk1c730;
	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			if ( ((Mai_U32)chn_info->acc_table_now->table[a0].num_uk)
				<= 1)
			{
				uk1c730 = 0;
				MAPCDSF_calcACCTableTableUnk3ByLastValue(&chn_info->acc_table_now->table[a0], mbr0);
			}
			else
			{
				uk1c730 = mbr0->getWithI32Buffer(1);
				if (!uk1c730)
				{
					MAPCDSF_calcACCTableTableUnk3ByLastValue(&chn_info->acc_table_now->table[a0], mbr0);
				}
				else
				{
					MAPCDSF_calcACCTableTableUnk3ByAfterValue(&chn_info->acc_table_now->table[a0], mbr0);
				}
			}
		}
	}

	return rs;
}

//used in MAPCDSF_splitePack 1
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_14[];
Mai_Status MAPCDSF_splitePack0_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			for (Mai_U32 a1 = 0; a1 < (Mai_U32)chn_info->acc_table_now->table[a0].num_uk; a1++)
			{
				Mai_I32 atmp0 = 
					MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_14[0], mbr0);
				if (atmp0 & 0x80) atmp0 |= (Mai_I32)0xFFFFFF00;
				else atmp0 &= 0xFF;

				if (a1 < (Mai_U32)chn_info->chn_ref->acc_table_now->table[a0].num_uk)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk3 = 
						chn_info->chn_ref->acc_table_now->table[a0].ptr0[a1].unk3
						+ atmp0;

					chn_info->acc_table_now->table[a0].ptr0[a1].unk3 &= 0x3FF;
				}
				else
				{
					if (chn_info->chn_ref->acc_table_now->table[a0].num_uk <= 0)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk3 = 
							atmp0;

						chn_info->acc_table_now->table[a0].ptr0[a1].unk3 &= 0x3FF;
					}
					else
					{
						Mai_I32 tmp0 = chn_info->chn_ref->acc_table_now->table[a0].num_uk - 1;
						chn_info->acc_table_now->table[a0].ptr0[a1].unk3 = 
							chn_info->chn_ref->acc_table_now->table[a0].ptr0[tmp0].unk3
							+ atmp0;

						chn_info->acc_table_now->table[a0].ptr0[a1].unk3 &= 0x3FF;
					}
				}
			}
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_splitePack_func_list0[2] = 
	{
		MAPCDSF_splitePack0_Route0,
		MAPCDSF_splitePack0_Route1
	};

//used in MAPCDSF_splitePack
Mai_Status MAPCDSF_makeTable11C(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 stmp34[0x80];
	Mai_I32 l22 = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0])
		{
			if (chn_info->acc_table_now->table[a0].num_uk > 0)
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->chn_ref->acc_table_now->table[a0].num_uk; a1++)
				{
					stmp34[a1] = 
						chn_info->chn_ref->acc_table_now->table[a0].ptr0[a1].unk3;
				}

				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					Mai_I32 btmp0 = chn_info->acc_table_now->table[a0].ptr0[a1].unk3;
					Mai_I32 dtmp0 = 0x400;
					Mai_I32 mtmp0 = 0;
					if ( chn_info->chn_ref->acc_table_now->table[a0].num_uk > 0)
					{
						for (Mai_I32 a2 = 0; a2 < chn_info->chn_ref->acc_table_now->table[a0].num_uk; a2++)
						{
							Mai_I32 atmp0 = btmp0 - stmp34[a2];
							if (atmp0 < 0) atmp0 = 0 - atmp0;
							if (dtmp0 > atmp0)
							{
								dtmp0 = atmp0;
								mtmp0 = a2;
							}
						}
						if (dtmp0 < 8)
						{
							chn_info->joint_chn_info->table11c[l22] = mtmp0;
						}
						else if (a1 < chn_info->chn_ref->acc_table_now->table[a0].num_uk)
						{
							chn_info->joint_chn_info->table11c[l22] = a1;
						}
						else
						{
							chn_info->joint_chn_info->table11c[l22] = -1;
						}
					}
					else
					{
						if (a1 < chn_info->chn_ref->acc_table_now->table[a0].num_uk)
						{
							chn_info->joint_chn_info->table11c[l22] = a1;
						}
						else
						{
							chn_info->joint_chn_info->table11c[l22] = -1;
						}
					}


					l22++;
				}
			}
		}
	}

	return rs;
}


Mai_Status MAPCDSF_splitePack1_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if (! chn_info->acc_table_now->inner->unk1 )
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				if (chn_info->acc_table_now->table[a0].num_uk)
				{
					chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
						mbr0->getWithI32Buffer(6);

					for (Mai_I32 a1 = 1; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->acc_table_now->table[a0].ptr0[a1 - 1].unk0;
					}
				}
			}
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
						mbr0->getWithI32Buffer(6);
				}
			}
		}
	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_15[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_16[];
Mai_Status MAPCDSF_splitePack1_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	if (! chn_info->acc_table_now->inner->unk1 )
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				if (chn_info->acc_table_now->table[a0].num_uk)
				{
					chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
						0x18
						+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_15[0], mbr0);
					
					for (Mai_I32 a1 = 1; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->acc_table_now->table[a0].ptr0[a1 - 1].unk0;
					}
				}
			}
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
						0x14
						+ MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_16[0], mbr0);
				}
			}
		}
	}


	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_17[];
Mai_Status MAPCDSF_splitePack1_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 l5 = 0;

	if (! chn_info->acc_table_now->inner->unk1 )
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				if (  chn_info->acc_table_now->table[a0].num_uk > 0  )
				{
					Mai_I32 atmp0 = 
						MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_17[0], mbr0);

					if (atmp0 & 0x10)
					{
						atmp0 |= (Mai_I32)0xFFFFFFE0;
					}
					else
					{
						atmp0 &= 0x1F;
					}

					if ( chn_info->chn_ref->acc_table_now->table[a0].num_uk > 0  )
					{
						chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
							chn_info->chn_ref->acc_table_now->table[a0].ptr0[0].unk0
							+ atmp0;
					}
					else
					{
						chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
							0x2C
							+ atmp0;
					}

					chn_info->acc_table_now->table[a0].ptr0[0].unk0 &= 0x3F;

					for (Mai_I32 a1 = 1; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->acc_table_now->table[a0].ptr0[a1 - 1].unk0;
					}

				}
			}
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					Mai_I32 atmp0 = 
						MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_17[0], mbr0);

					if (atmp0 & 0x10)
					{
						atmp0 |= (Mai_I32)0xFFFFFFE0;
					}
					else
					{
						atmp0 &= 0x1F;
					}

					if (chn_info->joint_chn_info->table11c[l5 + a1] >= 0)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->chn_ref->acc_table_now->table[a0].ptr0[chn_info->joint_chn_info->table11c[l5 + a1]].unk0
							+ atmp0;
					}
					else
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							0x22
							+ atmp0;
					}

					chn_info->acc_table_now->table[a0].ptr0[a1].unk0 &= 0x3F;
				}

				l5 += chn_info->acc_table_now->table[a0].num_uk;
			}
		}
	}

	return rs;
}

Mai_Status MAPCDSF_splitePack1_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 l4 = 0;

	if (! chn_info->acc_table_now->inner->unk1 )
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				if (  chn_info->acc_table_now->table[a0].num_uk > 0  )
				{
					if (  chn_info->chn_ref->acc_table_now->table[a0].num_uk > 0  )
					{
						chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
							chn_info->chn_ref->acc_table_now->table[a0].ptr0[0].unk0;
					}
					else
					{
						chn_info->acc_table_now->table[a0].ptr0[0].unk0 = 
							0x31;
					}

					for (Mai_I32 a1 = 1; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->acc_table_now->table[a0].ptr0[a1 - 1].unk0;
					}
				}
			}
		}
	}
	else
	{
		for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
		{
			if (chn_info->inner_pack_table0_check_table[a0] != 0)
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					if (chn_info->joint_chn_info->table11c[l4 + a1] >= 0)
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							chn_info->chn_ref->acc_table_now->table[a0].ptr0[chn_info->joint_chn_info->table11c[l4 + a1]].unk0;
					}
					else
					{
						chn_info->acc_table_now->table[a0].ptr0[a1].unk0 = 
							0x20;
					}
				}

				l4 += chn_info->acc_table_now->table[a0].num_uk;
			}
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_splitePack_func_list1[4] = 
	{
		MAPCDSF_splitePack1_Route0,
		MAPCDSF_splitePack1_Route1,
		MAPCDSF_splitePack1_Route2,
		MAPCDSF_splitePack1_Route3
	};



Mai_Status MAPCDSF_splitePack2_Route0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0] != 0)
		{
			for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
			{
				chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
					mbr0->getWithI32Buffer(4);
			}
		}
	}

	return rs;
}


extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_18[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_19[];
Mai_Status MAPCDSF_splitePack2_Route1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0] != 0)
		{
			if ( chn_info->acc_table_now->table[a0].num_uk == 1)
			{
				chn_info->acc_table_now->table[a0].ptr0[0].unk1 = 
					MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_18[0], mbr0);
			}
			else
			{
				for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
						MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_19[0], mbr0);
				}
			}
		}
	}

	return rs;
}

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table_global_20[];
Mai_Status MAPCDSF_splitePack2_Route2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 l5 = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0] != 0)
		{
			for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
			{
				Mai_I32 atmp0 = 
					MAPCDSF_getHuffValue(&MAPCDSD_huff_table_global_20[0], mbr0);

				if (atmp0 & 0x4)
				{
					atmp0 |= (Mai_I32)0xFFFFFFF8;
				}
				else
				{
					atmp0 &= 0x7;
				}

				if (chn_info->joint_chn_info->table11c[l5 + a1] >= 0)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
						chn_info->chn_ref->acc_table_now->table[a0].ptr0[ chn_info->joint_chn_info->table11c[l5 + a1] ].unk1
						+ atmp0;

					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 &= 0xF;
				}
				else
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
						atmp0
						- 4;

					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 &= 0xF;
				}
			}

			l5 += chn_info->acc_table_now->table[a0].num_uk;
		}
	}

	return rs;
}


Mai_Status MAPCDSF_splitePack2_Route3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 l4 = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0] != 0)
		{
			for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
			{
				if (chn_info->joint_chn_info->table11c[l4 + a1] >= 0)
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
						chn_info->chn_ref->acc_table_now->table[a0].ptr0[chn_info->joint_chn_info->table11c[l4 + a1]].unk1;
				}
				else
				{
					chn_info->acc_table_now->table[a0].ptr0[a1].unk1 = 
						0xE;
				}
			}

			l4 += chn_info->acc_table_now->table[a0].num_uk;
		}
	}

	return rs;
}

MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_splitePack_func_list2[4] = 
	{
		MAPCDSF_splitePack2_Route0,
		MAPCDSF_splitePack2_Route1,
		MAPCDSF_splitePack2_Route2,
		MAPCDSF_splitePack2_Route3
	};

Mai_Status MAPCDSF_readSplitePackMemberNum(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		if (chn_info->inner_pack_table0_check_table[a0] != 0)
		{
			for (Mai_I32 a1 = 0; a1 < chn_info->acc_table_now->table[a0].num_uk; a1++)
			{
				chn_info->acc_table_now->table[a0].ptr0[a1].unk2 = 
					mbr0->getWithI32Buffer(5);
			}
		}
	}

	return rs;
}

Mai_Status MAPCDSF_splitePack(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	Mai_I32 atmp0 = 0;
	for (Mai_U32 a0 = 0; a0 < (Mai_U32)chn_info->acc_table_now->inner->unk2; a0++)
	{
		chn_info->acc_table_now->table[a0].ptr0 = 
			(atmp0)
			+ chn_info->acc_table_now->inner->ptr_to_use_now;
		atmp0 += chn_info->acc_table_now->table[a0].num_uk;
	}

	chn_info->acc_table_now->inner->ptr_to_use_now += atmp0;

	Mai_U32 tmp0 = 0;
	if (chn_info->chn_flag == 1)
	{
		tmp0 = mbr0->getWithI32Buffer(1);
	}

	MAPCDSF_splitePack_func_list0[tmp0]
		(mbr0, chn_info);

	if (chn_info->chn_flag == 1)
	{
		MAPCDSF_makeTable11C(mbr0, chn_info);
	}

	Mai_U32 tmp1 = mbr0->getWithI32Buffer(chn_info->chn_flag + 1);

	MAPCDSF_splitePack_func_list1[tmp1]
		(mbr0, chn_info);

	if (! chn_info->acc_table_now->inner->unk1 )
	{
		Mai_U32 tmp2 = mbr0->getWithI32Buffer(chn_info->chn_flag + 1);

		MAPCDSF_splitePack_func_list2[tmp2]
			(mbr0, chn_info);
	}


	MAPCDSF_readSplitePackMemberNum(mbr0, chn_info);

	return rs;
}

//used in decodeACC6Inner
Mai_Status MAPCDSF_decodeACC6InnerSub0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info)
{
	Mai_Status rs = 0;

	MAPCDSF_makeInnerPackTable0CheckTable(chn_info, 1);

	Mai_U32 route_flag0 = 0;

	if (chn_info->chn_flag == 1)
		route_flag0 = mbr0->getWithI32Buffer(1);

	MAPCDSF_decodeACC6InnerSub0_func_list0[route_flag0]
		(mbr0, chn_info);

	{
		Mai_U32 route_flag1 = mbr0->getWithI32Buffer(chn_info->chn_flag + 1);

		MAPCDSF_decodeACC6InnerSub0_func_list1[route_flag1]
			(mbr0, chn_info);
	}

	MAPCDSF_splitePack(mbr0, chn_info);

	return rs;
}
