.set noreorder
.set noat

.global vfpu_vc2i_s
.ent    vfpu_vc2i_s
vfpu_vc2i_s:
    lvl.q  C000.q, 12($a0) 
    lvr.q  C000.q, 0($a0) 
	.word 0xD0398081 // vc2i.q  C010.q, S000.s
    svl.q  C010.q, 12($a1) 
    svr.q  C010.q, 0($a1) 
    jr $ra
	nop
.end vfpu_vc2i_s

.global vfpu_vuc2i_s
.ent    vfpu_vuc2i_s
vfpu_vuc2i_s:
    lvl.q  C000.q, 12($a0) 
    lvr.q  C000.q, 0($a0) 
	.word 0xD0388081 // vuc2i.q C010.q, S000.s
    svl.q  C010.q, 12($a1) 
    svr.q  C010.q, 0($a1) 
    jr $ra
	nop
.end vfpu_vuc2i_s

.global vfpu_vs2i_s
.ent    vfpu_vs2i_s
vfpu_vs2i_s:
    lvl.q  C000.q, 12($a0) 
    lvr.q  C000.q, 0($a0) 
    vs2i.s C010.p, S000.s
    svl.q  C010.q, 12($a1) 
    svr.q  C010.q, 0($a1) 
    jr $ra
	nop
.end vfpu_vs2i_s

.global vfpu_vs2i_p
.ent    vfpu_vs2i_p
vfpu_vs2i_p:
    lvl.q  C000.q, 12($a0) 
    lvr.q  C000.q, 0($a0) 
    vs2i.p C010.q, C000.p
    svl.q  C010.q, 12($a1) 
    svr.q  C010.q, 0($a1) 
    jr $ra
	nop
.end vfpu_vs2i_p

.global vfpu_vi2f_q
.ent    vfpu_vi2f_q
vfpu_vi2f_q:
    lvl.q  C000.q, 12($a0) 
    lvr.q  C000.q, 0($a0)
    vi2f.q C010.q, C000.q, 4
    svl.q  C010.q, 12($a1) 
    svr.q  C010.q, 0($a1) 
    jr $ra
	nop
.end vfpu_vi2f_q
