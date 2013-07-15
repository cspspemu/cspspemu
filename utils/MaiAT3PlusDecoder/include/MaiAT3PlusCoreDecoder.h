/*
 * MaiAT3PlusCoreDecoder Header
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
#ifndef MaiAT3PlusCoreDecoder_H
#define MaiAT3PlusCoreDecoder_H

#include "Mai_Base0.h"


typedef struct _MaiAT3PlusCoreDecoderSearchTableDes
{
	Mai_U16 *table0;
	Mai_U8 *table1;
	Mai_I32 uk0, uk1;
	Mai_U8 max_bit_len;
	Mai_U8 uk2;
	Mai_U8 uk3;
	Mai_U8 uk4;
	Mai_U8 uk5;
	Mai_U8 uk6;
	Mai_U8 mask;
	Mai_U8 uk8;
} MaiAT3PlusCoreDecoderSearchTableDes;





typedef struct _MaiAT3PlusCoreDecoderPackTable0
{
	Mai_I32 check_data0;
	Mai_I32 check_data1;
	Mai_I32 data[0x10];
} MaiAT3PlusCoreDecoderPackTable0;

typedef struct _MaiAT3PlusCoreDecoderJointChnInfo
{
	MaiAT3PlusCoreDecoderPackTable0 table00;
	MaiAT3PlusCoreDecoderPackTable0 table48;

	Mai_U32 num_band_splited_declared;
	Mai_U32 num_band_splited_used;
	
	Mai_U32 num_band_declared;
	Mai_U32 num_band_used;

	Mai_U32 joint_flag;
	Mai_U32 chns;

	Mai_U32 var90;
	Mai_I32 var94;
	Mai_I32 var98;
	Mai_I32 var9c;
	Mai_I32 var118;
	Mai_I32 table11c[0x100];
} MaiAT3PlusCoreDecoderJointChnInfo;

typedef struct _MaiAT3PlusCoreDecoderChnACCData
{
	struct
	{
		Mai_I32 num_acc;
		Mai_I32 data0[7];
		Mai_U32 data1[7];
		Mai_I32 acc_now;
		Mai_I32 reserved[0x16];
	} table[0x10];
	MaiAT3PlusCoreDecoderPackTable0 acc;
} MaiAT3PlusCoreDecoderChnACCData;

typedef struct _MaiAT3PlusCoreDecoderChnACCTableData
{
	Mai_I32 unk0;
	Mai_I32 unk1;
	Mai_I32 unk2;
	Mai_I32 unk3;
} MaiAT3PlusCoreDecoderChnACCTableData;

typedef struct _MaiAT3PlusCoreDecoderChnACCTableInner
{
	Mai_I32 unk0;
	Mai_I32 unk1;
	Mai_I32 unk2;
	MaiAT3PlusCoreDecoderChnACCTableData data[0x30];
	MaiAT3PlusCoreDecoderChnACCTableData *ptr_to_use_now;
	MaiAT3PlusCoreDecoderPackTable0 table_unk0;
	MaiAT3PlusCoreDecoderPackTable0 table_unk1;
	MaiAT3PlusCoreDecoderPackTable0 table_unk2;
} MaiAT3PlusCoreDecoderChnACCTableInner;

typedef struct _MaiAT3PlusCoreDecoderChnACCTableTable
{
	Mai_I32 unk[8];
	Mai_I32 num_uk;
	MaiAT3PlusCoreDecoderChnACCTableData *ptr0;
} MaiAT3PlusCoreDecoderChnACCTableTable;

typedef struct _MaiAT3PlusCoreDecoderChnACCTable
{
	MaiAT3PlusCoreDecoderChnACCTableInner *inner;
	MaiAT3PlusCoreDecoderChnACCTableTable table[0x10];
} MaiAT3PlusCoreDecoderChnACCTable;

typedef struct _MaiAT3PlusCoreDecoderChnInfo
{
	Mai_U32 chn_flag;
	MaiAT3PlusCoreDecoderJointChnInfo *joint_chn_info;
	MaiAT3PlusCoreDecoderChnACCData *acc_data_now;
	MaiAT3PlusCoreDecoderChnACCData *acc_data_old;
	MaiAT3PlusCoreDecoderChnACCTable *acc_table_old;
	MaiAT3PlusCoreDecoderChnACCTable *acc_table_now;
	struct _MaiAT3PlusCoreDecoderChnInfo *chn_ref;

	Mai_U32 var1034;
	Mai_U32 check_table0[0x20];
	
	Mai_I32 table0_flag_ex;
	Mai_I32 table0_flag_data_num;
	Mai_U32 table0_data_num0;
	Mai_U32 table0_data_num1;
	
	Mai_I32 uk1c718;
	Mai_I32 uk1c714;

	Mai_U32 uk1b450;
	
	Mai_U32 table0[0x20];
	Mai_U32 table1[0x20];
	Mai_U32 table2[0x20];
	Mai_I16 table3[0x800];
	Mai_U32 table4[0x10];

	Mai_U32 inner_pack_table0_check_table[0x10];
} MaiAT3PlusCoreDecoderChnInfo;



//static funcs

Mai_U32 MAPCDSF_getHuffValue(MaiAT3PlusCoreDecoderSearchTableDes *huff_table, MaiBitReader *mbr0);

//check func
Mai_Status MAPCDSF_makeTable0CheckTable(MaiAT3PlusCoreDecoderChnInfo *chn_info, Mai_U32 *check_table);

//used in decodeTable3
Mai_Status MAPCDSF_decodeTable3Sub0(MaiBitReader *mbr0, Mai_I16 *buf_to_read, Mai_U32 num_to_read, MaiAT3PlusCoreDecoderSearchTableDes *huff_table_now);

//read table*
Mai_Status MAPCDSF_readPackTable0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderPackTable0 *table, Mai_U32 counter);

//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info);

//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info);

//used in decodeACC2Main
Mai_Status MAPCDSF_decodeACC2MainSub2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info);

//used in decodeACC6Inner
Mai_Status MAPCDSF_decodeACC6InnerSub0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info);


//static data

extern Mai_U8 MAPCDSD_band_num_table0[];
extern Mai_U8 MAPCDSD_band_num_table1[];

extern Mai_U32 MAPCDSD_bind_table0[];
extern Mai_U32 MAPCDSD_bind_table1[];

extern Mai_U8 MAPCDSD_bind_table2[];

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table0[];

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table1[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table1_2[];

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table2[];
extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table2_2[];

extern MaiAT3PlusCoreDecoderSearchTableDes MAPCDSD_huff_table3[];

typedef Mai_Status (*MaiAT3PlusCoreDecoderSubFuncType0)(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo *chn_info);

extern MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable0_func_list0[];
extern MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable1_func_list0[];
extern MaiAT3PlusCoreDecoderSubFuncType0 MAPCDSF_decodeTable2_func_list0[];



class MaiAT3PlusCoreDecoder
{
private:
	MaiAT3PlusCoreDecoderChnInfo *chn_info[2];

	float syn_buf[2][0x1000];
	float dst_buf[2][0x800];
	float kyou_buf[2][0x800];
	Mai_I32 c900;

	Heap_Alloc0 heap0;
	
private:
	Mai_Status decodeBandNum(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos);
	Mai_Status decodeTable0(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeTable1(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeTable2(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeTable3(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeACC2Pre(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeACC2Main(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeACC6Inner(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);
	Mai_Status decodeTailInfo(MaiBitReader *mbr0, MaiAT3PlusCoreDecoderChnInfo **chn_infos, Mai_U32 chns);

private:
public:
	MaiAT3PlusCoreDecoder();
	~MaiAT3PlusCoreDecoder();
	Mai_Status parseStream(MaiBitReader *mbr0, Mai_U32 chns, Mai_U32 joint_flag);
	Mai_Status decodeStream(Mai_U32 chns);
	Mai_Status getAudioSamplesI16(Mai_U32 index_chn, Mai_I16 *bufs);
};

#endif

