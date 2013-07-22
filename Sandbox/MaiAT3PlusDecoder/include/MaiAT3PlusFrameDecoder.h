/*
 * MaiAT3PlusFrameDecoder Header
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
#ifndef MaiAT3PlusFrameDecoder_H
#define MaiAT3PlusFrameDecoder_H

#include "MaiAT3PlusCoreDecoder.h"

class MaiAT3PlusFrameDecoder
{

private:
	MaiAT3PlusCoreDecoder *cores[0x10];
	Mai_I16 sample_buf[0x8000];
	Mai_I16 sample_buf_tmp[0x8000];
	Mai_I32 num_cores;

	Heap_Alloc0 heap0;
public:
	MaiAT3PlusFrameDecoder();
	~MaiAT3PlusFrameDecoder();
	
/**
 * decode at3+ frame
 *
 * @param p_frame_data(INPUT) pointer to the frame data
 * @param data_len(INPUT) the length of the frame data
 * @param p_chns(OUTPUT) pointer to a Mai_I32 to receive
 * channel num(Can be NULL if you don't want to receive)
 * @param pp_sample_buf(OUTPUT) pointer to a buffer pointer to
 * get sample buffer. every frame has 0x800 samples.
 * the length of the buffer is 0x800*chns*sizeof(Mai_I16)
 * (Can be NULL if you don't want to receive)
 * 
 * @return 0 if success; otherwise on error
 */
	Mai_Status decodeFrame(Mai_I8 *p_frame_data, Mai_I32 data_len, Mai_I32 *p_chns, Mai_I16 **pp_sample_buf);
};


#endif
