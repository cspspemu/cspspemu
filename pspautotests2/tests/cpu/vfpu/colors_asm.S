.set noreorder
.set noat

.global vfpu_rgba8888to4444
.ent    vfpu_rgba8888to4444
vfpu_rgba8888to4444:
    lvl.q C000.q, 12($a0) 
    lvr.q C000.q, 0($a0) 
    vt4444.q C010.p, C000.q 
    svl.q C010.q, 12($a1) 
    jr $ra
    svr.q C010.q, 0($a1) 
.end vfpu_rgba8888to4444

.global vfpu_rgba8888to5551
.ent    vfpu_rgba8888to5551
vfpu_rgba8888to5551:
    lvl.q C000.q, 12($a0) 
    lvr.q C000.q, 0($a0) 
    vt5551.q C010.p, C000.q 
    svl.q C010.q, 12($a1) 
    jr $ra
    svr.q C010.q, 0($a1) 
.end vfpu_rgba8888to5551

.global vfpu_rgba8888to5650
.ent    vfpu_rgba8888to5650
vfpu_rgba8888to5650:
    lvl.q C000.q, 12($a0) 
    lvr.q C000.q, 0($a0) 
    vt5650.q C010.p, C000.q 
    svl.q C010.q, 12($a1) 
    jr $ra
    svr.q C010.q, 0($a1) 
.end vfpu_rgba8888to5650