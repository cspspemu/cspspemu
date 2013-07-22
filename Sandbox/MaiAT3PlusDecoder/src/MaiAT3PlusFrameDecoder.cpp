/*
 * MaiAT3PlusFrameDecoder
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
#include "MaiAT3PlusFrameDecoder.h"

MaiAT3PlusFrameDecoder::MaiAT3PlusFrameDecoder()
{
	num_cores = 0;
	Mai_memset(cores, 0, sizeof(MaiAT3PlusCoreDecoder *) * 0x10);
}

MaiAT3PlusFrameDecoder::~MaiAT3PlusFrameDecoder()
{
	for (Mai_I32 a0 = 0; a0 < 0x10; a0++)
	{
		if (cores[a0])
		{
			delete cores[a0];
			cores[a0] = 0;
		}
	}
}

Mai_Status MaiAT3PlusFrameDecoder::decodeFrame(Mai_I8 *p_frame_data, Mai_I32 data_len, Mai_I32 *p_chns, Mai_I16 **pp_sample_buf)
{
	Mai_Status rs = 0;

	MaiBitReader *mbr0 = new MaiBitReader(data_len + 0x10);
	mbr0->addData(p_frame_data, data_len);

	Mai_I8 data_pad[0x10];
	Mai_memset(data_pad, 0, 0x10);
	mbr0->addData(data_pad, 0x10);

	if (mbr0->getWithI32Buffer(1))
	{
		rs = -1;
	}


	Mai_I32 counter_substream = 0;
	Mai_I32 counter_chn = 0;
	while (!rs)
	{
		Mai_I32 substream_type = mbr0->getWithI32Buffer(2);
		Mai_U32 joint_flag = 0;
		Mai_U32 chns = 0;

		if (!substream_type)
		{
			joint_flag = 0;
			chns = 1;
		}
		else if (substream_type == 1)
		{
			joint_flag = 1;
			chns = 2;
		}
		else if (substream_type == 3)
		{
			break;
		}
		else
		{
			rs = -1;
		}
		
		if (!cores[counter_substream])
			cores[counter_substream] = new MaiAT3PlusCoreDecoder();

		if (rs = cores[counter_substream]->parseStream(mbr0, chns, joint_flag))
			break;

		if (rs = cores[counter_substream]->decodeStream(chns))
			break;

		for (Mai_I32 a0 = 0; a0 < chns; a0++)
			cores[counter_substream]->getAudioSamplesI16(a0, &sample_buf_tmp[0x800 * (counter_chn++)]);

		counter_substream++;
	}

	for (Mai_I32 a0 = 0; a0 < 0x800; a0++)
	{
		for (Mai_I32 a1 = 0; a1 < counter_chn; a1++)
		{
			sample_buf[a0 * counter_chn + a1] = sample_buf_tmp[a1 * 0x800 + a0];
		}
	}
	delete mbr0;

	if (p_chns) *p_chns = counter_chn;
	if (pp_sample_buf) *pp_sample_buf = sample_buf;

	return rs;
}
