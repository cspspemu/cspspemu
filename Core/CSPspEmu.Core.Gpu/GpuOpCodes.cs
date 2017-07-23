namespace CSPspEmu.Core.Gpu
{
    public enum GpuOpCodes : byte
    {
        /// <summary>
        /// 0x00 - 0 - NOP
        /// </summary>
        NOP,

        /// <summary>
        /// 0x01 - 1 - Vertex List (BASE)
        /// </summary>
        VADDR,

        /// <summary>
        /// 0x02 - 2 - Index List (BASE)
        /// </summary>
        IADDR,

        /// <summary>
        /// 0x03 - 3 - 
        /// </summary>
        Unknown0x03,

        /// <summary>
        /// 0x04 - 4 - Primitive Kick
        /// </summary>
        PRIM,

        /// <summary>
        /// 0x05 - 5 - Bezier Patch Kick
        /// </summary>
        BEZIER,

        /// <summary>
        /// 0x06 - 6 - Spline Surface Kick
        /// </summary>
        SPLINE,

        /// <summary>
        /// 0x07 - 7 - Bounding Box
        /// </summary>
        BBOX,

        /// <summary>
        /// 0x08 - 8 - Jump To New Address (BASE)
        /// </summary>
        JUMP,

        /// <summary>
        /// 0x09 - 9 - Conditional Jump (BASE)
        /// </summary>
        BJUMP,

        /// <summary>
        /// 0x0A - 10 - Call Address (BASE)
        /// </summary>
        CALL,

        /// <summary>
        /// 0x0B - 11 - Return From Call
        /// </summary>
        RET,

        /// <summary>
        /// 0x0C - 12 - Stop Execution
        /// </summary>
        END,

        /// <summary>
        /// 0x0D - 13 - 
        /// </summary>
        Unknown0x0D,

        /// <summary>
        /// 0x0E - 14 - Raise Signal Interrupt
        /// </summary>
        SIGNAL,

        /// <summary>
        /// 0x0F - 15 - Complete Rendering
        /// </summary>
        FINISH,

        /// <summary>
        /// 0x10 - 16 - Base Address Register
        /// </summary>
        BASE,

        /// <summary>
        /// 0x11 - 17 -
        /// </summary>
        Unknown0x11,

        /// <summary>
        /// 0x12 - 18 - Vertex Type
        /// </summary>
        VTYPE,

        /// <summary>
        /// 0x13 - 19 - Offset Address (BASE)
        /// </summary>
        OFFSET_ADDR,

        /// <summary>
        /// 0x14 - 20 - Origin Address (BASE)
        /// </summary>
        ORIGIN_ADDR,

        /// <summary>
        /// 0x15 - 21 - Draw Region Start
        /// </summary>
        REGION1,

        /// <summary>
        /// 0x16 - 22 - Draw Region End
        /// </summary>
        REGION2,

        /// <summary>
        /// 0x17 - 23 - Lighting Enable
        /// </summary>
        LTE,

        /// <summary>
        /// 0x18 - 24 - Light 0 Enable
        /// </summary>
        LTE0,

        /// <summary>
        /// 0x19 - 25 - Light 1 Enable
        /// </summary>
        LTE1,

        /// <summary>
        /// 0x1A - 26 - Light 2 Enable
        /// </summary>
        LTE2,

        /// <summary>
        /// 0x1B - 27 - Light 3 Enable
        /// </summary>
        LTE3,

        /// <summary>
        /// 0x1C - Clip Plane Enable
        /// </summary>
        CPE,

        /// <summary>
        /// 0x1D - Backface Culling Enable
        /// </summary>
        BCE,

        /// <summary>
        /// 0x1E - Texture Mapping Enable
        /// </summary>
        TME,

        /// <summary>
        /// 0x1F - Fog Enable
        /// </summary>
        FGE,

        /// <summary>
        /// 0x20 - Dither Enable
        /// </summary>
        DTE,

        /// <summary>
        /// 0x21 - Alpha Blend Enable
        /// </summary>
        ABE,

        /// <summary>
        /// 0x22 - Alpha Test Enable
        /// </summary>
        ATE,

        /// <summary>
        /// 0x23 - Depth Test Enable
        /// </summary>
        ZTE,

        /// <summary>
        /// 0x24 - Stencil Test Enable
        /// </summary>
        STE,

        /// <summary>
        /// 0x25 - Anitaliasing Enable
        /// </summary>
        AAE,

        /// <summary>
        /// 0x26 - Patch Cull Enable
        /// </summary>
        PCE,

        /// <summary>
        /// 0x27 - Color Test Enable
        /// </summary>
        CTE,

        /// <summary>
        /// 0x28 - Logical Operation Enable
        /// </summary>
        LOE,

        /// <summary>
        /// 0x29 - 
        /// </summary>
        Unknown0x29,

        /// <summary>
        /// 0x2A - Bone Matrix Offset
        /// </summary>
        BOFS,

        /// <summary>
        /// 0x2B - Bone Matrix Upload
        /// </summary>
        BONE,

        /// <summary>
        /// 0x2C - Morph Weight 0
        /// </summary>
        MW0,

        /// <summary>
        /// 0x2D - Morph Weight 1
        /// </summary>
        MW1,

        /// <summary>
        /// 0x2E - Morph Weight 2
        /// </summary>
        MW2,

        /// <summary>
        /// 0x2F - Morph Weight 3
        /// </summary>
        MW3,

        /// <summary>
        /// 0x30 - Morph Weight 4
        /// </summary>
        MW4,

        /// <summary>
        /// 0x31 - Morph Weight 5
        /// </summary>
        MW5,

        /// <summary>
        /// 0x32 - Morph Weight 6
        /// </summary>
        MW6,

        /// <summary>
        /// 0x33 - Morph Weight 7
        /// </summary>
        MW7,

        /// <summary>
        /// 0x34 - 
        /// </summary>
        Unknown0x34,

        /// <summary>
        /// 0x35 - 
        /// </summary>
        Unknown0x35,

        /// <summary>
        /// 0x36 - Patch Subdivision
        /// </summary>
        PSUB,

        /// <summary>
        /// 0x37 - Patch Primitive
        /// </summary>
        PPRIM,

        /// <summary>
        /// 0x38 - Patch Front Face
        /// </summary>
        PFACE,

        /// <summary>
        /// 0x39 - 
        /// </summary>
        Unknown0x39,

        /// <summary>
        /// 0x3A - World Matrix Select
        /// </summary>
        WMS,

        /// <summary>
        /// 0x3B - World Matrix Upload
        /// </summary>
        WORLD,

        /// <summary>
        /// 0x3C - View Matrix Select
        /// </summary>
        VMS,

        /// <summary>
        /// 0x3D - View Matrix upload
        /// </summary>
        VIEW,

        /// <summary>
        /// 0x3E - Projection matrix Select
        /// </summary>
        PMS,

        /// <summary>
        /// 0x3F - Projection Matrix upload
        /// </summary>
        PROJ,

        /// <summary>
        /// 0x40 - Texture Matrix Select
        /// </summary>
        TMS,

        /// <summary>
        /// 0x41 - Texture Matrix Upload
        /// </summary>
        TMATRIX,

        /// <summary>
        /// 0x42 - 66 - Viewport Width Scale
        /// </summary>
        XSCALE,

        /// <summary>
        /// 0x43 - 67 - Viewport Height Scale
        /// </summary>
        YSCALE,

        /// <summary>
        /// 0x44 - 68 - Depth Scale
        /// </summary>
        ZSCALE,

        /// <summary>
        /// 0x45 - 69 - Viewport X Position
        /// </summary>
        XPOS,

        /// <summary>
        /// 0x46 - 90 - Viewport Y Position
        /// </summary>
        YPOS,

        /// <summary>
        /// 0x47 - Depth Position
        /// </summary>
        ZPOS,

        /// <summary>
        /// 0x48 - Texture Scale U
        /// </summary>
        USCALE,

        /// <summary>
        /// 0x49 - Texture Scale V
        /// </summary>
        VSCALE,

        /// <summary>
        /// 0x4A - Texture Offset U
        /// </summary>
        UOFFSET,

        /// <summary>
        /// 0x4B - Texture Offset V
        /// </summary>
        VOFFSET,

        /// <summary>
        /// 0x4C - Viewport offset (X)
        /// </summary>
        OFFSETX,

        /// <summary>
        /// 0x4D - Viewport offset (Y)
        /// </summary>
        OFFSETY,

        /// <summary>
        /// 0x4E - 
        /// </summary>
        Unknown0x4E,

        /// <summary>
        /// 0x4F - 
        /// </summary>
        Unknown0x4F,

        /// <summary>
        /// 0x50 - Shade Model
        /// </summary>
        SHADE,

        /// <summary>
        /// 0x51 - Reverse Face Normals Enable
        /// </summary>
        RNORM,

        /// <summary>
        /// 0x52 - 
        /// </summary>
        Unknown0x52,

        /// <summary>
        /// 0x53 - Color Material
        /// </summary>
        CMAT,

        /// <summary>
        /// 0x54 - Emissive Model Color
        /// </summary>
        EMC,

        /// <summary>
        /// 0x55 - Ambient Model Color
        /// </summary>
        AMC,

        /// <summary>
        /// 0x56 - Diffuse Model Color
        /// </summary>
        DMC,

        /// <summary>
        /// 0x57 - Specular Model Color
        /// </summary>
        SMC,

        /// <summary>
        /// 0x58 - Ambient Model Alpha
        /// </summary>
        AMA,

        /// <summary>
        /// 0x59 - 
        /// </summary>
        Unknown0x59,

        /// <summary>
        /// 0x5A - 
        /// </summary>
        Unknown0x5A,

        /// <summary>
        /// 0x5B - Specular Power
        /// </summary>
        SPOW,

        /// <summary>
        /// 0x5C - Ambient Light Color
        /// </summary>
        ALC,

        /// <summary>
        /// 0x5D - Ambient Light Alpha
        /// </summary>
        ALA,

        /// <summary>
        /// 0x5E - Light Model
        /// </summary>
        LMODE,

        /// <summary>
        /// 0x5F - Light Type 0
        /// </summary>
        LT0,

        /// <summary>
        /// 0x60 - Light Type 1
        /// </summary>
        LT1,

        /// <summary>
        /// 0x61 - Light Type 2
        /// </summary>
        LT2,

        /// <summary>
        /// 0x62 - Light Type 3
        /// </summary>
        LT3,

        /// <summary>
        /// 0x63 - Light X Position 0
        /// </summary>
        LXP0,

        /// <summary>
        /// 0x64 - Light Y Position 0
        /// </summary>
        LYP0,

        /// <summary>
        /// 0x65 - Light Z Position 0
        /// </summary>
        LZP0,

        /// <summary>
        /// 0x66 - Light X Position 1
        /// </summary>
        LXP1,

        /// <summary>
        /// 0x67 - Light Y Position 1
        /// </summary>
        LYP1,

        /// <summary>
        /// 0x68 - Light Z Position 1
        /// </summary>
        LZP1,

        /// <summary>
        /// 0x69 - Light X Position 2
        /// </summary>
        LXP2,

        /// <summary>
        /// 0x6A - Light Y Position 2
        /// </summary>
        LYP2,

        /// <summary>
        /// 0x6B - Light Z Position 2
        /// </summary>
        LZP2,

        /// <summary>
        /// 0x6C - Light X Position 3
        /// </summary>
        LXP3,

        /// <summary>
        /// 0x6D - Light Y Position 3
        /// </summary>
        LYP3,

        /// <summary>
        /// 0x6E - Light Z Position 3
        /// </summary>
        LZP3,

        /// <summary>
        /// 0x6F - Light X Direction 0
        /// </summary>
        LXD0,

        /// <summary>
        /// 0x70 - Light Y Direction 0
        /// </summary>
        LYD0,

        /// <summary>
        /// 0x71 - Light Z Direction 0
        /// </summary>
        LZD0,

        /// <summary>
        /// 0x72 - Light X Direction 1
        /// </summary>
        LXD1,

        /// <summary>
        /// 0x73 - Light Y Direction 1
        /// </summary>
        LYD1,

        /// <summary>
        /// 0x74 - Light Z Direction 1
        /// </summary>
        LZD1,

        /// <summary>
        /// 0x75 - Light X Direction 2
        /// </summary>
        LXD2,

        /// <summary>
        /// 0x76 - Light Y Direction 2
        /// </summary>
        LYD2,

        /// <summary>
        /// 0x77 - Light Z Direction 2
        /// </summary>
        LZD2,

        /// <summary>
        /// 0x78 - Light X Direction 3
        /// </summary>
        LXD3,

        /// <summary>
        /// 0x79 - Light Y Direction 3
        /// </summary>
        LYD3,

        /// <summary>
        /// 0x7A - Light Z Direction 3
        /// </summary>
        LZD3,

        /// <summary>
        /// 0x7B - Light Constant Attenuation 0
        /// </summary>
        LCA0,

        /// <summary>
        /// 0x7C - Light Linear Attenuation 0
        /// </summary>
        LLA0,

        /// <summary>
        /// 0x7D - Light Quadratic Attenuation 0
        /// </summary>
        LQA0,

        /// <summary>
        /// 0x7E - Light Constant Attenuation 1
        /// </summary>
        LCA1,

        /// <summary>
        /// 0x7F - Light Linear Attenuation 1
        /// </summary>
        LLA1,

        /// <summary>
        /// 0x80 - Light Quadratic Attenuation 1
        /// </summary>
        LQA1,

        /// <summary>
        /// 0x81 - Light Constant Attenuation 2
        /// </summary>
        LCA2,

        /// <summary>
        /// 0x82 - Light Linear Attenuation 2
        /// </summary>
        LLA2,

        /// <summary>
        /// 0x83 - Light Quadratic Attenuation 2
        /// </summary>
        LQA2,

        /// <summary>
        /// 0x84 - Light Constant Attenuation 3
        /// </summary>
        LCA3,

        /// <summary>
        /// 0x85 - Light Linear Attenuation 3
        /// </summary>
        LLA3,

        /// <summary>
        /// 0x86 - Light Quadratic Attenuation 3
        /// </summary>
        LQA3,

        /// <summary>
        /// 0x87 - Spot light 0 exponent
        /// </summary>
        SPOTEXP0,

        /// <summary>
        /// 0x88 - Spot light 1 exponent
        /// </summary>
        SPOTEXP1,

        /// <summary>
        /// 0x89 - Spot light 2 exponent
        /// </summary>
        SPOTEXP2,

        /// <summary>
        /// 0x8A - Spot light 3 exponent
        /// </summary>
        SPOTEXP3,

        /// <summary>
        /// 0x8B - Spot light 0 cutoff
        /// </summary>
        SPOTCUT0,

        /// <summary>
        /// 0x8C - Spot light 1 cutoff
        /// </summary>
        SPOTCUT1,

        /// <summary>
        /// 0x8D - Spot light 2 cutoff
        /// </summary>
        SPOTCUT2,

        /// <summary>
        /// 0x8E - Spot light 3 cutoff
        /// </summary>
        SPOTCUT3,

        /// <summary>
        /// 0x8F - Ambient Light Color 0
        /// </summary>
        ALC0,

        /// <summary>
        /// 0x90 - Diffuse Light Color 0
        /// </summary>
        DLC0,

        /// <summary>
        /// 0x91 - Specular Light Color 0
        /// </summary>
        SLC0,

        /// <summary>
        /// 0x92 - Ambient Light Color 1
        /// </summary>
        ALC1,

        /// <summary>
        /// 0x93 - Diffuse Light Color 1
        /// </summary>
        DLC1,

        /// <summary>
        /// 0x94 - Specular Light Color 1
        /// </summary>
        SLC1,

        /// <summary>
        /// 0x95 - Ambient Light Color 2
        /// </summary>
        ALC2,

        /// <summary>
        /// 0x96 - Diffuse Light Color 2
        /// </summary>
        DLC2,

        /// <summary>
        /// 0x97 - Specular Light Color 2
        /// </summary>
        SLC2,

        /// <summary>
        /// 0x98 - Ambient Light Color 3
        /// </summary>
        ALC3,

        /// <summary>
        /// 0x99 - Diffuse Light Color 3
        /// </summary>
        DLC3,

        /// <summary>
        /// 0x9A - Specular Light Color 3
        /// </summary>
        SLC3,

        /// <summary>
        /// 0x9B - Front Face Culling Order
        /// </summary>
        FFACE,

        /// <summary>
        /// 0x9C - 156  Frame Buffer Pointer
        /// </summary>
        FBP,

        /// <summary>
        /// 0x9D - 157 - Frame Buffer Width
        /// </summary>
        FBW,

        /// <summary>
        /// 0x9E - Depth Buffer Pointer
        /// </summary>
        ZBP,

        /// <summary>
        /// 0x9F - Depth Buffer Width
        /// </summary>
        ZBW,

        /// <summary>
        /// 0xA0 - Texture Buffer Pointer 0
        /// </summary>
        TBP0,

        /// <summary>
        /// 0xA1 - Texture Buffer Pointer 1
        /// </summary>
        TBP1,

        /// <summary>
        /// 0xA2 - Texture Buffer Pointer 2
        /// </summary>
        TBP2,

        /// <summary>
        /// 0xA3 - Texture Buffer Pointer 3
        /// </summary>
        TBP3,

        /// <summary>
        /// 0xA4 - Texture Buffer Pointer 4
        /// </summary>
        TBP4,

        /// <summary>
        /// 0xA5 - Texture Buffer Pointer 5
        /// </summary>
        TBP5,

        /// <summary>
        /// 0xA6 - Texture Buffer Pointer 6
        /// </summary>
        TBP6,

        /// <summary>
        /// 0xA7 - Texture Buffer Pointer 7
        /// </summary>
        TBP7,

        /// <summary>
        /// 0xA8 - Texture Buffer Width 0
        /// </summary>
        TBW0,

        /// <summary>
        /// 0xA9 - Texture Buffer Width 1
        /// </summary>
        TBW1,

        /// <summary>
        /// 0xAA - Texture Buffer Width 2
        /// </summary>
        TBW2,

        /// <summary>
        /// 0xAB - Texture Buffer Width 3
        /// </summary>
        TBW3,

        /// <summary>
        /// 0xAC - Texture Buffer Width 4
        /// </summary>
        TBW4,

        /// <summary>
        /// 0xAD - Texture Buffer Width 5
        /// </summary>
        TBW5,

        /// <summary>
        /// 0xAE - Texture Buffer Width 6
        /// </summary>
        TBW6,

        /// <summary>
        /// 0xAF - Texture Buffer Width 7
        /// </summary>
        TBW7,

        /// <summary>
        /// 0xB0 - CLUT Buffer Pointer
        /// </summary>
        CBP,

        /// <summary>
        /// 0xB1 - CLUT Buffer Pointer H
        /// </summary>
        CBPH,

        /// <summary>
        /// 0xB2 - Transmission Source Buffer Pointer
        /// </summary>
        TRXSBP,

        /// <summary>
        /// 0xB3 - Transmission Source Buffer Width
        /// </summary>
        TRXSBW,

        /// <summary>
        /// 0xB4 - Transmission Destination Buffer Pointer
        /// </summary>
        TRXDBP,

        /// <summary>
        /// 0xB5 - Transmission Destination Buffer Width
        /// </summary>
        TRXDBW,

        /// <summary>
        /// 0xB6 - 
        /// </summary>
        Unknown0xB6,

        /// <summary>
        /// 0xB7 - 
        /// </summary>
        Unknown0xB7,

        /// <summary>
        /// 0xB8 - Texture Size Level 0
        /// </summary>
        TSIZE0,

        /// <summary>
        /// 0xB9 - Texture Size Level 1
        /// </summary>
        TSIZE1,

        /// <summary>
        /// 0xBA - Texture Size Level 2
        /// </summary>
        TSIZE2,

        /// <summary>
        /// 0xBB - Texture Size Level 3
        /// </summary>
        TSIZE3,

        /// <summary>
        /// 0xBC - Texture Size Level 4
        /// </summary>
        TSIZE4,

        /// <summary>
        /// 0xBD - Texture Size Level 5
        /// </summary>
        TSIZE5,

        /// <summary>
        /// 0xBE - Texture Size Level 6
        /// </summary>
        TSIZE6,

        /// <summary>
        /// 0xBF - Texture Size Level 7
        /// </summary>
        TSIZE7,

        /// <summary>
        /// 0xC0 - Texture Projection Map Mode + Texture Map Mode
        /// </summary>
        TMAP,

        /// <summary>
        /// 0xC1 - Environment Map Matrix
        /// </summary>
        TEXTURE_ENV_MAP_MATRIX,

        /// <summary>
        /// 0xC2 - Texture Mode
        /// </summary>
        TMODE,

        /// <summary>
        /// 0xC3 - Texture Pixel Storage Mode
        /// </summary>
        TPSM,

        /// <summary>
        /// 0xC4 - CLUT Load
        /// </summary>
        CLOAD,

        /// <summary>
        /// 0xC5 - CLUT Mode
        /// </summary>
        CMODE,

        /// <summary>
        /// 0xC6 - Texture Filter
        /// </summary>
        TFLT,

        /// <summary>
        /// 0xC7 - Texture Wrapping
        /// </summary>
        TWRAP,

        /// <summary>
        /// 0xC8 - Texture Level Bias (???)
        /// </summary>
        TBIAS,

        /// <summary>
        /// 0xC9 - Texture Function
        /// </summary>
        TFUNC,

        /// <summary>
        /// 0xCA - Texture Environment Color
        /// </summary>
        TEC,

        /// <summary>
        /// 0xCB - Texture Flush
        /// </summary>
        TFLUSH,

        /// <summary>
        /// 0xCC - Texture Sync
        /// </summary>
        TSYNC,

        /// <summary>
        /// 0xCD - Fog Far (???)
        /// </summary>
        FFAR,

        /// <summary>
        /// 0xCE - Fog Range
        /// </summary>
        FDIST,

        /// <summary>
        /// 0xCF - Fog Color
        /// </summary>
        FCOL,

        /// <summary>
        /// 0xD0 - Texture Slope
        /// </summary>
        TSLOPE,

        /// <summary>
        /// 0xD1 - 
        /// </summary>
        Unknown0xD1,

        /// <summary>
        /// 0xD2 - Frame Buffer Pixel Storage Mode
        /// </summary>
        PSM,

        /// <summary>
        /// 0xD3 - Clear Flags
        /// </summary>
        CLEAR,

        /// <summary>
        /// 0xD4 - Scissor Region Start
        /// </summary>
        SCISSOR1,

        /// <summary>
        /// 0xD5 - Scissor Region End
        /// </summary>
        SCISSOR2,

        /// <summary>
        /// 0xD6 - Near Depth Range
        /// </summary>
        NEARZ,

        /// <summary>
        /// 0xD7 - Far Depth Range
        /// </summary>
        FARZ,

        /// <summary>
        /// 0xD8 - Color Test Function
        /// </summary>
        CTST,

        /// <summary>
        /// 0xD9 - Color Reference
        /// </summary>
        CREF,

        /// <summary>
        /// 0xDA - Color Mask
        /// </summary>
        CMSK,

        /// <summary>
        /// 0xDB - Alpha Test
        /// </summary>
        ATST,

        /// <summary>
        /// 0xDC - Stencil Test
        /// </summary>
        STST,

        /// <summary>
        /// 0xDD - Stencil Operations
        /// </summary>
        SOP,

        /// <summary>
        /// 0xDE - Depth Test Function
        /// </summary>
        ZTST,

        /// <summary>
        /// 0xDF - Alpha Blend
        /// </summary>
        ALPHA,

        /// <summary>
        /// 0xE0 - Source Fix Color
        /// </summary>
        SFIX,

        /// <summary>
        /// 0xE1 - Destination Fix Color
        /// </summary>
        DFIX,

        /// <summary>
        /// 0xE2 - Dither Matrix Row 0
        /// </summary>
        DTH0,

        /// <summary>
        /// 0xE3 - Dither Matrix Row 1
        /// </summary>
        DTH1,

        /// <summary>
        /// 0xE4 - Dither Matrix Row 2
        /// </summary>
        DTH2,

        /// <summary>
        /// 0xE5 - Dither Matrix Row 3
        /// </summary>
        DTH3,

        /// <summary>
        /// 0xE6 - Logical Operation
        /// </summary>
        LOP,

        /// <summary>
        /// 0xE7 - Depth Mask
        /// </summary>
        ZMSK,

        /// <summary>
        /// 0xE8 - Pixel Mask Color
        /// </summary>
        PMSKC,

        /// <summary>
        /// 0xE9 - Pixel Mask Alpha
        /// </summary>
        PMSKA,

        /// <summary>
        /// 0xEA - Transmission Kick
        /// </summary>
        TRXKICK,

        /// <summary>
        /// 0xEB - Transfer Source Position
        /// </summary>
        TRXSPOS,

        /// <summary>
        /// 0xEC - Transfer Destination Position
        /// </summary>
        TRXDPOS,

        /// <summary>
        /// 0xED - 
        /// </summary>
        Unknown0xED,

        /// <summary>
        /// 0xEE - Transfer Size
        /// </summary>
        TRXSIZE,

        /// <summary>
        /// 0xEF - 
        /// </summary>
        Unknown0xEF,

        /// <summary>
        /// 0xF0 - 
        /// </summary>
        Unknown0xF0,

        /// <summary>
        /// 0xF1 - 
        /// </summary>
        Unknown0xF1,

        /// <summary>
        /// 0xF2 - 
        /// </summary>
        Unknown0xF2,

        /// <summary>
        /// 0xF3 - 
        /// </summary>
        Unknown0xF3,

        /// <summary>
        /// 0xF4 - 
        /// </summary>
        Unknown0xF4,

        /// <summary>
        /// 0xF5 - 
        /// </summary>
        Unknown0xF5,

        /// <summary>
        /// 0xF6 - 
        /// </summary>
        Unknown0xF6,

        /// <summary>
        /// 0xF7 - 
        /// </summary>
        Unknown0xF7,

        /// <summary>
        /// 0xF8 - 
        /// </summary>
        Unknown0xF8,

        /// <summary>
        /// 0xF9 - 
        /// </summary>
        Unknown0xF9,

        /// <summary>
        /// 0xFA - 
        /// </summary>
        Unknown0xFA,

        /// <summary>
        /// 0xFB - 
        /// </summary>
        Unknown0xFB,

        /// <summary>
        /// 0xFC - 
        /// </summary>
        Unknown0xFC,

        /// <summary>
        /// 0xFD - 
        /// </summary>
        Unknown0xFD,

        /// <summary>
        /// 0xFE - 
        /// </summary>
        Unknown0xFE,

        /// <summary>
        /// 0xFF - 
        /// </summary>
        DUMMY,

        UNKNOWN = unchecked((byte) -1),
    }
}