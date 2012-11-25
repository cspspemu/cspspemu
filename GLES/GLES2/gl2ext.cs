using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GLeglImageOES = System.IntPtr;
using GLenum = System.Int32;

namespace GLES
{
	public unsafe partial class GLExt
	{
		/*
		 * This document is licensed under the SGI Free Software B License Version
		 * 2.0. For details, see http://oss.sgi.com/projects/FreeB/ .
		 */

		/*------------------------------------------------------------------------*
		 * OES extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_OES_compressed_ETC1_RGB8_texture */
		public const GLenum GL_ETC1_RGB8_OES                                      = 0x8D64;

		/* GL_OES_compressed_paletted_texture */
		public const GLenum GL_PALETTE4_RGB8_OES                                    = 0x8B90;
		public const GLenum GL_PALETTE4_RGBA8_OES                                   = 0x8B91;
		public const GLenum GL_PALETTE4_R5_G6_B5_OES                                = 0x8B92;
		public const GLenum GL_PALETTE4_RGBA4_OES                                   = 0x8B93;
		public const GLenum GL_PALETTE4_RGB5_A1_OES                                 = 0x8B94;
		public const GLenum GL_PALETTE8_RGB8_OES                                    = 0x8B95;
		public const GLenum GL_PALETTE8_RGBA8_OES                                   = 0x8B96;
		public const GLenum GL_PALETTE8_R5_G6_B5_OES                                = 0x8B97;
		public const GLenum GL_PALETTE8_RGBA4_OES                                   = 0x8B98;
		public const GLenum GL_PALETTE8_RGB5_A1_OES                                 = 0x8B99;

		/* GL_OES_depth24 */
		public const GLenum GL_DEPTH_COMPONENT24_OES                                = 0x81A6;

		/* GL_OES_depth32 */
		public const GLenum GL_DEPTH_COMPONENT32_OES                                = 0x81A7;

		/* GL_OES_depth_texture */
		/* No new tokens introduced by this extension. */

		/* GL_OES_EGL_image */

		/* GL_OES_EGL_image_external */
		/* GLeglImageOES defined in GL_OES_EGL_image already. */
		public const GLenum GL_TEXTURE_EXTERNAL_OES                                 = 0x8D65;
		public const GLenum GL_SAMPLER_EXTERNAL_OES                                 = 0x8D66;
		public const GLenum GL_TEXTURE_BINDING_EXTERNAL_OES                         = 0x8D67;
		public const GLenum GL_REQUIRED_TEXTURE_IMAGE_UNITS_OES                     = 0x8D68;

		/* GL_OES_element_index_uint */
		public const GLenum GL_UNSIGNED_INT                                         = 0x1405;

		/* GL_OES_get_program_binary */
		public const GLenum GL_PROGRAM_BINARY_LENGTH_OES                            = 0x8741;
		public const GLenum GL_NUM_PROGRAM_BINARY_FORMATS_OES                       = 0x87FE;
		public const GLenum GL_PROGRAM_BINARY_FORMATS_OES                           = 0x87FF;

		/* GL_OES_mapbuffer */
		public const GLenum GL_WRITE_ONLY_OES                                       = 0x88B9;
		public const GLenum GL_BUFFER_ACCESS_OES                                    = 0x88BB;
		public const GLenum GL_BUFFER_MAPPED_OES                                    = 0x88BC;
		public const GLenum GL_BUFFER_MAP_POINTER_OES                               = 0x88BD;

		/* GL_OES_packed_depth_stencil */
		public const GLenum GL_DEPTH_STENCIL_OES                                    = 0x84F9;
		public const GLenum GL_UNSIGNED_INT_24_8_OES                                = 0x84FA;
		public const GLenum GL_DEPTH24_STENCIL8_OES                                 = 0x88F0;

		/* GL_OES_rgb8_rgba8 */
		public const GLenum GL_RGB8_OES                                             = 0x8051;
		public const GLenum GL_RGBA8_OES                                            = 0x8058;

		/* GL_OES_standard_derivatives */
		public const GLenum GL_FRAGMENT_SHADER_DERIVATIVE_HINT_OES                  = 0x8B8B;

		/* GL_OES_stencil1 */
		public const GLenum GL_STENCIL_INDEX1_OES                                   = 0x8D46;

		/* GL_OES_stencil4 */
		public const GLenum GL_STENCIL_INDEX4_OES                                   = 0x8D47;

		/* GL_OES_texture_3D */
		public const GLenum GL_TEXTURE_WRAP_R_OES                                   = 0x8072;
		public const GLenum GL_TEXTURE_3D_OES                                       = 0x806F;
		public const GLenum GL_TEXTURE_BINDING_3D_OES                               = 0x806A;
		public const GLenum GL_MAX_3D_TEXTURE_SIZE_OES                              = 0x8073;
		public const GLenum GL_SAMPLER_3D_OES                                       = 0x8B5F;
		public const GLenum GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_3D_ZOFFSET_OES        = 0x8CD4;

		/* GL_OES_texture_float */
		/* No new tokens introduced by this extension. */

		/* GL_OES_texture_float_linear */
		/* No new tokens introduced by this extension. */

		/* GL_OES_texture_half_float */
		public const GLenum GL_HALF_FLOAT_OES                                       = 0x8D61;

		/* GL_OES_texture_half_float_linear */
		/* No new tokens introduced by this extension. */

		/* GL_OES_texture_npot */
		/* No new tokens introduced by this extension. */

		/* GL_OES_vertex_array_object */
		public const GLenum GL_VERTEX_ARRAY_BINDING_OES                             = 0x85B5;

		/* GL_OES_vertex_half_float */
		/* GL_HALF_FLOAT_OES defined in GL_OES_texture_half_float already. */

		/* GL_OES_vertex_type_10_10_10_2 */
		public const GLenum GL_UNSIGNED_INT_10_10_10_2_OES                          = 0x8DF6;
		public const GLenum GL_INT_10_10_10_2_OES                                   = 0x8DF7;

		/*------------------------------------------------------------------------*
		 * AMD extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_AMD_compressed_3DC_texture */
		public const GLenum GL_3DC_X_AMD                                            = 0x87F9;
		public const GLenum GL_3DC_XY_AMD                                           = 0x87FA;

		/* GL_AMD_compressed_ATC_texture */
		public const GLenum GL_ATC_RGB_AMD                                          = 0x8C92;
		public const GLenum GL_ATC_RGBA_EXPLICIT_ALPHA_AMD                          = 0x8C93;
		public const GLenum GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD                      = 0x87EE;

		/* GL_AMD_performance_monitor */
		public const GLenum GL_COUNTER_TYPE_AMD                                     = 0x8BC0;
		public const GLenum GL_COUNTER_RANGE_AMD                                    = 0x8BC1;
		public const GLenum GL_UNSIGNED_INT64_AMD                                   = 0x8BC2;
		public const GLenum GL_PERCENTAGE_AMD                                       = 0x8BC3;
		public const GLenum GL_PERFMON_RESULT_AVAILABLE_AMD                         = 0x8BC4;
		public const GLenum GL_PERFMON_RESULT_SIZE_AMD                              = 0x8BC5;
		public const GLenum GL_PERFMON_RESULT_AMD                                   = 0x8BC6;

		/* GL_AMD_program_binary_Z400 */
		public const GLenum GL_Z400_BINARY_AMD                                      = 0x8740;

		/*------------------------------------------------------------------------*
		 * ANGLE extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_ANGLE_framebuffer_blit */
		public const GLenum GL_READ_FRAMEBUFFER_ANGLE                               = 0x8CA8;
		public const GLenum GL_DRAW_FRAMEBUFFER_ANGLE                               = 0x8CA9;
		public const GLenum GL_DRAW_FRAMEBUFFER_BINDING_ANGLE                       = 0x8CA6;
		public const GLenum GL_READ_FRAMEBUFFER_BINDING_ANGLE                       = 0x8CAA;

		/* GL_ANGLE_framebuffer_multisample */
		public const GLenum GL_RENDERBUFFER_SAMPLES_ANGLE                           = 0x8CAB;
		public const GLenum GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE_ANGLE             = 0x8D56;
		public const GLenum GL_MAX_SAMPLES_ANGLE                                    = 0x8D57;

		/* GL_ANGLE_pack_reverse_row_order */
		public const GLenum GL_PACK_REVERSE_ROW_ORDER_ANGLE                         = 0x93A4;

		/* GL_ANGLE_texture_compression_dxt3 */
		public const GLenum GL_COMPRESSED_RGBA_S3TC_DXT3_ANGLE                      = 0x83F2;

		/* GL_ANGLE_texture_compression_dxt5 */
		public const GLenum GL_COMPRESSED_RGBA_S3TC_DXT5_ANGLE                      = 0x83F3;

		/* GL_ANGLE_translated_shader_source */
		public const GLenum GL_TRANSLATED_SHADER_SOURCE_LENGTH_ANGLE                = 0x93A0;

		/* GL_ANGLE_texture_usage */
		public const GLenum GL_TEXTURE_USAGE_ANGLE                                  = 0x93A2;
		public const GLenum GL_FRAMEBUFFER_ATTACHMENT_ANGLE                         = 0x93A3;

		/* GL_ANGLE_instanced_arrays */
		public const GLenum GL_VERTEX_ATTRIB_ARRAY_DIVISOR_ANGLE                    = 0x88FE;

		/* GL_ANGLE_program_binary */
		public const GLenum GL_PROGRAM_BINARY_ANGLE                                 = 0x93A6;

		/*------------------------------------------------------------------------*
		 * APPLE extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_APPLE_rgb_422 */
		public const GLenum GL_RGB_422_APPLE                                        = 0x8A1F;
		public const GLenum GL_UNSIGNED_SHORT_8_8_APPLE                             = 0x85BA;
		public const GLenum GL_UNSIGNED_SHORT_8_8_REV_APPLE                         = 0x85BB;

		/* GL_APPLE_framebuffer_multisample */
		public const GLenum GL_RENDERBUFFER_SAMPLES_APPLE                           = 0x8CAB;
		public const GLenum GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE_APPLE             = 0x8D56;
		public const GLenum GL_MAX_SAMPLES_APPLE                                    = 0x8D57;
		public const GLenum GL_READ_FRAMEBUFFER_APPLE                               = 0x8CA8;
		public const GLenum GL_DRAW_FRAMEBUFFER_APPLE                               = 0x8CA9;
		public const GLenum GL_DRAW_FRAMEBUFFER_BINDING_APPLE                       = 0x8CA6;
		public const GLenum GL_READ_FRAMEBUFFER_BINDING_APPLE                       = 0x8CAA;

		/* GL_APPLE_texture_format_BGRA8888 */
		//public const GLenum GL_BGRA_EXT                                             = 0x80E1;

		/* GL_APPLE_texture_max_level */
		public const GLenum GL_TEXTURE_MAX_LEVEL_APPLE                              = 0x813D;

		/*------------------------------------------------------------------------*
		 * ARM extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_ARM_mali_shader_binary */
		public const GLenum GL_MALI_SHADER_BINARY_ARM                               = 0x8F60;

		/* GL_ARM_rgba8 */
		/* No new tokens introduced by this extension. */

		/*------------------------------------------------------------------------*
		 * EXT extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_EXT_blend_minmax */
		public const GLenum GL_MIN_EXT                                              = 0x8007;
		public const GLenum GL_MAX_EXT                                              = 0x8008;

		/* GL_EXT_color_buffer_half_float */
		public const GLenum GL_RGBA16F_EXT                                          = 0x881A;
		public const GLenum GL_RGB16F_EXT                                           = 0x881B;
		public const GLenum GL_RG16F_EXT                                            = 0x822F;
		public const GLenum GL_R16F_EXT                                             = 0x822D;
		public const GLenum GL_FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE_EXT            = 0x8211;
		public const GLenum GL_UNSIGNED_NORMALIZED_EXT                              = 0x8C17;

		/* GL_EXT_debug_label */
		public const GLenum GL_PROGRAM_PIPELINE_OBJECT_EXT                          = 0x8A4F;
		public const GLenum GL_PROGRAM_OBJECT_EXT                                   = 0x8B40;
		public const GLenum GL_SHADER_OBJECT_EXT                                    = 0x8B48;
		public const GLenum GL_BUFFER_OBJECT_EXT                                    = 0x9151;
		public const GLenum GL_QUERY_OBJECT_EXT                                     = 0x9153;
		public const GLenum GL_VERTEX_ARRAY_OBJECT_EXT                              = 0x9154;

		/* GL_EXT_debug_marker */
		/* No new tokens introduced by this extension. */

		/* GL_EXT_discard_framebuffer */
		public const GLenum GL_COLOR_EXT                                            = 0x1800;
		public const GLenum GL_DEPTH_EXT                                            = 0x1801;
		public const GLenum GL_STENCIL_EXT                                          = 0x1802;

		/* GL_EXT_multisampled_render_to_texture */
		public const GLenum GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_SAMPLES_EXT           = 0x8D6C;
		public const GLenum GL_RENDERBUFFER_SAMPLES_EXT                             = 0x9133;
		public const GLenum GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE_EXT               = 0x9134;
		public const GLenum GL_MAX_SAMPLES_EXT                                      = 0x9135;

		/* GL_EXT_multi_draw_arrays */
		/* No new tokens introduced by this extension. */

		/* GL_EXT_occlusion_query_boolean */
		public const GLenum GL_ANY_SAMPLES_PASSED_EXT                               = 0x8C2F;
		public const GLenum GL_ANY_SAMPLES_PASSED_CONSERVATIVE_EXT                  = 0x8D6A;
		public const GLenum GL_CURRENT_QUERY_EXT                                    = 0x8865;
		public const GLenum GL_QUERY_RESULT_EXT                                     = 0x8866;
		public const GLenum GL_QUERY_RESULT_AVAILABLE_EXT                           = 0x8867;

		/* GL_EXT_read_format_bgra */
		public const GLenum GL_BGRA_EXT                                             = 0x80E1;
		public const GLenum GL_UNSIGNED_SHORT_4_4_4_4_REV_EXT                       = 0x8365;
		public const GLenum GL_UNSIGNED_SHORT_1_5_5_5_REV_EXT                       = 0x8366;

		/* GL_EXT_robustness */
		/* reuse GL_NO_ERROR */
		public const GLenum GL_GUILTY_CONTEXT_RESET_EXT                             = 0x8253;
		public const GLenum GL_INNOCENT_CONTEXT_RESET_EXT                           = 0x8254;
		public const GLenum GL_UNKNOWN_CONTEXT_RESET_EXT                            = 0x8255;
		public const GLenum GL_CONTEXT_ROBUST_ACCESS_EXT                            = 0x90F3;
		public const GLenum GL_RESET_NOTIFICATION_STRATEGY_EXT                      = 0x8256;
		public const GLenum GL_LOSE_CONTEXT_ON_RESET_EXT                            = 0x8252;
		public const GLenum GL_NO_RESET_NOTIFICATION_EXT                            = 0x8261;

		/* GL_EXT_separate_shader_objects */
		public const GLenum GL_VERTEX_SHADER_BIT_EXT                                = 0x00000001;
		public const GLenum GL_FRAGMENT_SHADER_BIT_EXT                              = 0x00000002;
		public const GLenum GL_ALL_SHADER_BITS_EXT                                  = unchecked((int)0xFFFFFFFF);
		public const GLenum GL_PROGRAM_SEPARABLE_EXT                                = 0x8258;
		public const GLenum GL_ACTIVE_PROGRAM_EXT                                   = 0x8259;
		public const GLenum GL_PROGRAM_PIPELINE_BINDING_EXT                         = 0x825A;

		/* GL_EXT_shader_texture_lod */
		/* No new tokens introduced by this extension. */

		/* GL_EXT_shadow_samplers */
		public const GLenum GL_TEXTURE_COMPARE_MODE_EXT                             = 0x884C;
		public const GLenum GL_TEXTURE_COMPARE_FUNC_EXT                             = 0x884D;
		public const GLenum GL_COMPARE_REF_TO_TEXTURE_EXT                           = 0x884E;

		/* GL_EXT_sRGB */
		public const GLenum GL_SRGB_EXT                                             = 0x8C40;
		public const GLenum GL_SRGB_ALPHA_EXT                                       = 0x8C42;
		public const GLenum GL_SRGB8_ALPHA8_EXT                                     = 0x8C43;
		public const GLenum GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING_EXT            = 0x8210;

		/* GL_EXT_texture_compression_dxt1 */
		public const GLenum GL_COMPRESSED_RGB_S3TC_DXT1_EXT                         = 0x83F0;
		public const GLenum GL_COMPRESSED_RGBA_S3TC_DXT1_EXT                        = 0x83F1;

		/* GL_EXT_texture_filter_anisotropic */
		public const GLenum GL_TEXTURE_MAX_ANISOTROPY_EXT                           = 0x84FE;
		public const GLenum GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT                       = 0x84FF;

		/* GL_EXT_texture_format_BGRA8888 */
		//public const GLenum GL_BGRA_EXT                                             = 0x80E1;

		/* GL_EXT_texture_rg */
		public const GLenum GL_RED_EXT                                              = 0x1903;
		public const GLenum GL_RG_EXT                                               = 0x8227;
		public const GLenum GL_R8_EXT                                               = 0x8229;
		public const GLenum GL_RG8_EXT                                              = 0x822B;

		/* GL_EXT_texture_storage */
		public const GLenum GL_TEXTURE_IMMUTABLE_FORMAT_EXT                         = 0x912F;
		public const GLenum GL_ALPHA8_EXT                                           = 0x803C;  
		public const GLenum GL_LUMINANCE8_EXT                                       = 0x8040;
		public const GLenum GL_LUMINANCE8_ALPHA8_EXT                                = 0x8045;
		public const GLenum GL_RGBA32F_EXT                                          = 0x8814;  
		public const GLenum GL_RGB32F_EXT                                           = 0x8815;
		public const GLenum GL_ALPHA32F_EXT                                         = 0x8816;
		public const GLenum GL_LUMINANCE32F_EXT                                     = 0x8818;
		public const GLenum GL_LUMINANCE_ALPHA32F_EXT                               = 0x8819;
		/* reuse GL_RGBA16F_EXT */
		//public const GLenum GL_RGB16F_EXT                                           = 0x881B;
		public const GLenum GL_ALPHA16F_EXT                                         = 0x881C;
		public const GLenum GL_LUMINANCE16F_EXT                                     = 0x881E;
		public const GLenum GL_LUMINANCE_ALPHA16F_EXT                               = 0x881F;
		public const GLenum GL_RGB10_A2_EXT                                         = 0x8059;  
		public const GLenum GL_RGB10_EXT                                            = 0x8052;
		public const GLenum GL_BGRA8_EXT                                            = 0x93A1;

		/* GL_EXT_texture_type_2_10_10_10_REV */
		public const GLenum GL_UNSIGNED_INT_2_10_10_10_REV_EXT                      = 0x8368;

		/* GL_EXT_unpack_subimage */
		public const GLenum GL_UNPACK_ROW_LENGTH                                    = 0x0CF2;
		public const GLenum GL_UNPACK_SKIP_ROWS                                     = 0x0CF3;
		public const GLenum GL_UNPACK_SKIP_PIXELS                                   = 0x0CF4;

		/*------------------------------------------------------------------------*
		 * DMP extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_DMP_shader_binary */
		public const GLenum GL_SHADER_BINARY_DMP                                    = 0x9250;

		/*------------------------------------------------------------------------*
		 * IMG extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_IMG_program_binary */
		public const GLenum GL_SGX_PROGRAM_BINARY_IMG                               = 0x9130;

		/* GL_IMG_read_format */
		public const GLenum GL_BGRA_IMG                                             = 0x80E1;
		public const GLenum GL_UNSIGNED_SHORT_4_4_4_4_REV_IMG                       = 0x8365;

		/* GL_IMG_shader_binary */
		public const GLenum GL_SGX_BINARY_IMG                                       = 0x8C0A;

		/* GL_IMG_texture_compression_pvrtc */
		public const GLenum GL_COMPRESSED_RGB_PVRTC_4BPPV1_IMG                      = 0x8C00;
		public const GLenum GL_COMPRESSED_RGB_PVRTC_2BPPV1_IMG                      = 0x8C01;
		public const GLenum GL_COMPRESSED_RGBA_PVRTC_4BPPV1_IMG                     = 0x8C02;
		public const GLenum GL_COMPRESSED_RGBA_PVRTC_2BPPV1_IMG                     = 0x8C03;

		/* GL_IMG_multisampled_render_to_texture */
		public const GLenum GL_RENDERBUFFER_SAMPLES_IMG                             = 0x9133;
		public const GLenum GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE_IMG               = 0x9134;
		public const GLenum GL_MAX_SAMPLES_IMG                                      = 0x9135;
		public const GLenum GL_TEXTURE_SAMPLES_IMG                                  = 0x9136;

		/*------------------------------------------------------------------------*
		 * NV extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_NV_coverage_sample */
		public const GLenum GL_COVERAGE_COMPONENT_NV                                = 0x8ED0;
		public const GLenum GL_COVERAGE_COMPONENT4_NV                               = 0x8ED1;
		public const GLenum GL_COVERAGE_ATTACHMENT_NV                               = 0x8ED2;
		public const GLenum GL_COVERAGE_BUFFERS_NV                                  = 0x8ED3;
		public const GLenum GL_COVERAGE_SAMPLES_NV                                  = 0x8ED4;
		public const GLenum GL_COVERAGE_ALL_FRAGMENTS_NV                            = 0x8ED5;
		public const GLenum GL_COVERAGE_EDGE_FRAGMENTS_NV                           = 0x8ED6;
		public const GLenum GL_COVERAGE_AUTOMATIC_NV                                = 0x8ED7;
		public const GLenum GL_COVERAGE_BUFFER_BIT_NV                               = 0x8000;

		/* GL_NV_depth_nonlinear */
		public const GLenum GL_DEPTH_COMPONENT16_NONLINEAR_NV                       = 0x8E2C;

		/* GL_NV_draw_buffers */
		public const GLenum GL_MAX_DRAW_BUFFERS_NV                                  = 0x8824;
		public const GLenum GL_DRAW_BUFFER0_NV                                      = 0x8825;
		public const GLenum GL_DRAW_BUFFER1_NV                                      = 0x8826;
		public const GLenum GL_DRAW_BUFFER2_NV                                      = 0x8827;
		public const GLenum GL_DRAW_BUFFER3_NV                                      = 0x8828;
		public const GLenum GL_DRAW_BUFFER4_NV                                      = 0x8829;
		public const GLenum GL_DRAW_BUFFER5_NV                                      = 0x882A;
		public const GLenum GL_DRAW_BUFFER6_NV                                      = 0x882B;
		public const GLenum GL_DRAW_BUFFER7_NV                                      = 0x882C;
		public const GLenum GL_DRAW_BUFFER8_NV                                      = 0x882D;
		public const GLenum GL_DRAW_BUFFER9_NV                                      = 0x882E;
		public const GLenum GL_DRAW_BUFFER10_NV                                     = 0x882F;
		public const GLenum GL_DRAW_BUFFER11_NV                                     = 0x8830;
		public const GLenum GL_DRAW_BUFFER12_NV                                     = 0x8831;
		public const GLenum GL_DRAW_BUFFER13_NV                                     = 0x8832;
		public const GLenum GL_DRAW_BUFFER14_NV                                     = 0x8833;
		public const GLenum GL_DRAW_BUFFER15_NV                                     = 0x8834;
		public const GLenum GL_COLOR_ATTACHMENT0_NV                                 = 0x8CE0;
		public const GLenum GL_COLOR_ATTACHMENT1_NV                                 = 0x8CE1;
		public const GLenum GL_COLOR_ATTACHMENT2_NV                                 = 0x8CE2;
		public const GLenum GL_COLOR_ATTACHMENT3_NV                                 = 0x8CE3;
		public const GLenum GL_COLOR_ATTACHMENT4_NV                                 = 0x8CE4;
		public const GLenum GL_COLOR_ATTACHMENT5_NV                                 = 0x8CE5;
		public const GLenum GL_COLOR_ATTACHMENT6_NV                                 = 0x8CE6;
		public const GLenum GL_COLOR_ATTACHMENT7_NV                                 = 0x8CE7;
		public const GLenum GL_COLOR_ATTACHMENT8_NV                                 = 0x8CE8;
		public const GLenum GL_COLOR_ATTACHMENT9_NV                                 = 0x8CE9;
		public const GLenum GL_COLOR_ATTACHMENT10_NV                                = 0x8CEA;
		public const GLenum GL_COLOR_ATTACHMENT11_NV                                = 0x8CEB;
		public const GLenum GL_COLOR_ATTACHMENT12_NV                                = 0x8CEC;
		public const GLenum GL_COLOR_ATTACHMENT13_NV                                = 0x8CED;
		public const GLenum GL_COLOR_ATTACHMENT14_NV                                = 0x8CEE;
		public const GLenum GL_COLOR_ATTACHMENT15_NV                                = 0x8CEF;

		/* GL_NV_fbo_color_attachments */
		public const GLenum GL_MAX_COLOR_ATTACHMENTS_NV                             = 0x8CDF;
		/* GL_COLOR_ATTACHMENT{0-15}_NV defined in GL_NV_draw_buffers already. */

		/* GL_NV_fence */
		public const GLenum GL_ALL_COMPLETED_NV                                     = 0x84F2;
		public const GLenum GL_FENCE_STATUS_NV                                      = 0x84F3;
		public const GLenum GL_FENCE_CONDITION_NV                                   = 0x84F4;

		/* GL_NV_read_buffer */
		public const GLenum GL_READ_BUFFER_NV                                       = 0x0C02;

		/* GL_NV_read_buffer_front */
		/* No new tokens introduced by this extension. */

		/* GL_NV_read_depth */
		/* No new tokens introduced by this extension. */

		/* GL_NV_read_depth_stencil */
		/* No new tokens introduced by this extension. */

		/* GL_NV_read_stencil */
		/* No new tokens introduced by this extension. */

		/* GL_NV_texture_compression_s3tc_update */
		/* No new tokens introduced by this extension. */

		/* GL_NV_texture_npot_2D_mipmap */
		/* No new tokens introduced by this extension. */

		/*------------------------------------------------------------------------*
		 * QCOM extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_QCOM_alpha_test */
		public const GLenum GL_ALPHA_TEST_QCOM                                      = 0x0BC0;
		public const GLenum GL_ALPHA_TEST_FUNC_QCOM                                 = 0x0BC1;
		public const GLenum GL_ALPHA_TEST_REF_QCOM                                  = 0x0BC2;

		/* GL_QCOM_driver_control */
		/* No new tokens introduced by this extension. */

		/* GL_QCOM_extended_get */
		public const GLenum GL_TEXTURE_WIDTH_QCOM                                   = 0x8BD2;
		public const GLenum GL_TEXTURE_HEIGHT_QCOM                                  = 0x8BD3;
		public const GLenum GL_TEXTURE_DEPTH_QCOM                                   = 0x8BD4;
		public const GLenum GL_TEXTURE_INTERNAL_FORMAT_QCOM                         = 0x8BD5;
		public const GLenum GL_TEXTURE_FORMAT_QCOM                                  = 0x8BD6;
		public const GLenum GL_TEXTURE_TYPE_QCOM                                    = 0x8BD7;
		public const GLenum GL_TEXTURE_IMAGE_VALID_QCOM                             = 0x8BD8;
		public const GLenum GL_TEXTURE_NUM_LEVELS_QCOM                              = 0x8BD9;
		public const GLenum GL_TEXTURE_TARGET_QCOM                                  = 0x8BDA;
		public const GLenum GL_TEXTURE_OBJECT_VALID_QCOM                            = 0x8BDB;
		public const GLenum GL_STATE_RESTORE                                        = 0x8BDC;

		/* GL_QCOM_extended_get2 */
		/* No new tokens introduced by this extension. */

		/* GL_QCOM_perfmon_global_mode */
		public const GLenum GL_PERFMON_GLOBAL_MODE_QCOM                             = 0x8FA0;

		/* GL_QCOM_writeonly_rendering */
		public const GLenum GL_WRITEONLY_RENDERING_QCOM                             = 0x8823;

		/* GL_QCOM_tiled_rendering */
		public const GLenum GL_COLOR_BUFFER_BIT0_QCOM                               = 0x00000001;
		public const GLenum GL_COLOR_BUFFER_BIT1_QCOM                               = 0x00000002;
		public const GLenum GL_COLOR_BUFFER_BIT2_QCOM                               = 0x00000004;
		public const GLenum GL_COLOR_BUFFER_BIT3_QCOM                               = 0x00000008;
		public const GLenum GL_COLOR_BUFFER_BIT4_QCOM                               = 0x00000010;
		public const GLenum GL_COLOR_BUFFER_BIT5_QCOM                               = 0x00000020;
		public const GLenum GL_COLOR_BUFFER_BIT6_QCOM                               = 0x00000040;
		public const GLenum GL_COLOR_BUFFER_BIT7_QCOM                               = 0x00000080;
		public const GLenum GL_DEPTH_BUFFER_BIT0_QCOM                               = 0x00000100;
		public const GLenum GL_DEPTH_BUFFER_BIT1_QCOM                               = 0x00000200;
		public const GLenum GL_DEPTH_BUFFER_BIT2_QCOM                               = 0x00000400;
		public const GLenum GL_DEPTH_BUFFER_BIT3_QCOM                               = 0x00000800;
		public const GLenum GL_DEPTH_BUFFER_BIT4_QCOM                               = 0x00001000;
		public const GLenum GL_DEPTH_BUFFER_BIT5_QCOM                               = 0x00002000;
		public const GLenum GL_DEPTH_BUFFER_BIT6_QCOM                               = 0x00004000;
		public const GLenum GL_DEPTH_BUFFER_BIT7_QCOM                               = 0x00008000;
		public const GLenum GL_STENCIL_BUFFER_BIT0_QCOM                             = 0x00010000;
		public const GLenum GL_STENCIL_BUFFER_BIT1_QCOM                             = 0x00020000;
		public const GLenum GL_STENCIL_BUFFER_BIT2_QCOM                             = 0x00040000;
		public const GLenum GL_STENCIL_BUFFER_BIT3_QCOM                             = 0x00080000;
		public const GLenum GL_STENCIL_BUFFER_BIT4_QCOM                             = 0x00100000;
		public const GLenum GL_STENCIL_BUFFER_BIT5_QCOM                             = 0x00200000;
		public const GLenum GL_STENCIL_BUFFER_BIT6_QCOM                             = 0x00400000;
		public const GLenum GL_STENCIL_BUFFER_BIT7_QCOM                             = 0x00800000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT0_QCOM                         = 0x01000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT1_QCOM                         = 0x02000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT2_QCOM                         = 0x04000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT3_QCOM                         = 0x08000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT4_QCOM                         = 0x10000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT5_QCOM                         = 0x20000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT6_QCOM                         = 0x40000000;
		public const GLenum GL_MULTISAMPLE_BUFFER_BIT7_QCOM                         = unchecked((int)0x80000000);

		/*------------------------------------------------------------------------*
		 * VIV extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_VIV_shader_binary */
		public const GLenum GL_SHADER_BINARY_VIV                                    = 0x8FC4;

		/*------------------------------------------------------------------------*
		 * End of extension tokens, start of corresponding extension functions
		 *------------------------------------------------------------------------*/

		/*------------------------------------------------------------------------*
		 * OES extension functions
		 *------------------------------------------------------------------------*/

		/* GL_OES_compressed_ETC1_RGB8_texture */
		public const GLenum GL_OES_compressed_ETC1_RGB8_texture = 1;

		/* GL_OES_compressed_paletted_texture */
		public const GLenum GL_OES_compressed_paletted_texture = 1;

		/* GL_OES_depth24 */
		public const GLenum GL_OES_depth24 = 1;

		/* GL_OES_depth32 */
		public const GLenum GL_OES_depth32 = 1;

		/* GL_OES_depth_texture */
		public const GLenum GL_OES_depth_texture = 1;

		/* GL_OES_EGL_image */
		/*
		#ifndef GL_OES_EGL_image
		public const GLenum GL_OES_EGL_image 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glEGLImageTargetTexture2DOES (GLenum target, GLeglImageOES image);
		GL_APICALL void GL_APIENTRY glEGLImageTargetRenderbufferStorageOES (GLenum target, GLeglImageOES image);
		#endif
		typedef void (GL_APIENTRYP PFNGLEGLIMAGETARGETTEXTURE2DOESPROC) (GLenum target, GLeglImageOES image);
		typedef void (GL_APIENTRYP PFNGLEGLIMAGETARGETRENDERBUFFERSTORAGEOESPROC) (GLenum target, GLeglImageOES image);
		#endif
		*/

		/* GL_OES_EGL_image_external */
		public const GLenum GL_OES_EGL_image_external = 1;
		/* glEGLImageTargetTexture2DOES defined in GL_OES_EGL_image already. */

		/* GL_OES_element_index_uint */
		public const GLenum GL_OES_element_index_uint = 1;

		/* GL_OES_fbo_render_mipmap */
		public const GLenum GL_OES_fbo_render_mipmap = 1;

		/* GL_OES_fragment_precision_high */
		public const GLenum GL_OES_fragment_precision_high = 1;

		/* GL_OES_get_program_binary */
		/*
		#ifndef GL_OES_get_program_binary
		public const GLenum GL_OES_get_program_binary 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glGetProgramBinaryOES (GLuint program, GLsizei bufSize, GLsizei *length, GLenum *binaryFormat, GLvoid *binary);
		GL_APICALL void GL_APIENTRY glProgramBinaryOES (GLuint program, GLenum binaryFormat, const GLvoid *binary, GLint length);
		#endif
		typedef void (GL_APIENTRYP PFNGLGETPROGRAMBINARYOESPROC) (GLuint program, GLsizei bufSize, GLsizei *length, GLenum *binaryFormat, GLvoid *binary);
		typedef void (GL_APIENTRYP PFNGLPROGRAMBINARYOESPROC) (GLuint program, GLenum binaryFormat, const GLvoid *binary, GLint length);
		#endif
		*/

		/* GL_OES_mapbuffer */
		/*
		#ifndef GL_OES_mapbuffer
		public const GLenum GL_OES_mapbuffer 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void* GL_APIENTRY glMapBufferOES (GLenum target, GLenum access);
		GL_APICALL GLboolean GL_APIENTRY glUnmapBufferOES (GLenum target);
		GL_APICALL void GL_APIENTRY glGetBufferPointervOES (GLenum target, GLenum pname, GLvoid** params);
		#endif
		typedef void* (GL_APIENTRYP PFNGLMAPBUFFEROESPROC) (GLenum target, GLenum access);
		typedef GLboolean (GL_APIENTRYP PFNGLUNMAPBUFFEROESPROC) (GLenum target);
		typedef void (GL_APIENTRYP PFNGLGETBUFFERPOINTERVOESPROC) (GLenum target, GLenum pname, GLvoid** params);
		#endif
		*/

		/* GL_OES_packed_depth_stencil */
		public const GLenum GL_OES_packed_depth_stencil = 1;

		/* GL_OES_rgb8_rgba8 */
		public const GLenum GL_OES_rgb8_rgba8 = 1;

		/* GL_OES_standard_derivatives */
		public const GLenum GL_OES_standard_derivatives = 1;

		/* GL_OES_stencil1 */
		public const GLenum GL_OES_stencil1 = 1;

		/* GL_OES_stencil4 */
		public const GLenum GL_OES_stencil4 = 1;

		/* GL_OES_texture_3D */
		/*
		#ifndef GL_OES_texture_3D
		public const GLenum GL_OES_texture_3D 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glTexImage3DOES (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLenum format, GLenum type, const GLvoid* pixels);
		GL_APICALL void GL_APIENTRY glTexSubImage3DOES (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, const GLvoid* pixels);
		GL_APICALL void GL_APIENTRY glCopyTexSubImage3DOES (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height);
		GL_APICALL void GL_APIENTRY glCompressedTexImage3DOES (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLsizei imageSize, const GLvoid* data);
		GL_APICALL void GL_APIENTRY glCompressedTexSubImage3DOES (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize, const GLvoid* data);
		GL_APICALL void GL_APIENTRY glFramebufferTexture3DOES (GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLint zoffset);
		#endif
		typedef void (GL_APIENTRYP PFNGLTEXIMAGE3DOESPROC) (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLenum format, GLenum type, const GLvoid* pixels);
		typedef void (GL_APIENTRYP PFNGLTEXSUBIMAGE3DOESPROC) (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, const GLvoid* pixels);
		typedef void (GL_APIENTRYP PFNGLCOPYTEXSUBIMAGE3DOESPROC) (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLCOMPRESSEDTEXIMAGE3DOESPROC) (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLsizei imageSize, const GLvoid* data);
		typedef void (GL_APIENTRYP PFNGLCOMPRESSEDTEXSUBIMAGE3DOESPROC) (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize, const GLvoid* data);
		typedef void (GL_APIENTRYP PFNGLFRAMEBUFFERTEXTURE3DOES) (GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLint zoffset);
		#endif
		*/

		/* GL_OES_texture_float */
		public const GLenum GL_OES_texture_float = 1;

		/* GL_OES_texture_float_linear */
		public const GLenum GL_OES_texture_float_linear = 1;

		/* GL_OES_texture_half_float */
		public const GLenum GL_OES_texture_half_float = 1;

		/* GL_OES_texture_half_float_linear */
		public const GLenum GL_OES_texture_half_float_linear = 1;

		/* GL_OES_texture_npot */
		public const GLenum GL_OES_texture_npot = 1;

		/* GL_OES_vertex_array_object */
		/*
		#ifndef GL_OES_vertex_array_object
		public const GLenum GL_OES_vertex_array_object 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glBindVertexArrayOES (GLuint array);
		GL_APICALL void GL_APIENTRY glDeleteVertexArraysOES (GLsizei n, const GLuint *arrays);
		GL_APICALL void GL_APIENTRY glGenVertexArraysOES (GLsizei n, GLuint *arrays);
		GL_APICALL GLboolean GL_APIENTRY glIsVertexArrayOES (GLuint array);
		#endif
		typedef void (GL_APIENTRYP PFNGLBINDVERTEXARRAYOESPROC) (GLuint array);
		typedef void (GL_APIENTRYP PFNGLDELETEVERTEXARRAYSOESPROC) (GLsizei n, const GLuint *arrays);
		typedef void (GL_APIENTRYP PFNGLGENVERTEXARRAYSOESPROC) (GLsizei n, GLuint *arrays);
		typedef GLboolean (GL_APIENTRYP PFNGLISVERTEXARRAYOESPROC) (GLuint array);
		#endif
		*/

		/* GL_OES_vertex_half_float */
		public const GLenum GL_OES_vertex_half_float = 1;

		/* GL_OES_vertex_type_10_10_10_2 */
		public const GLenum GL_OES_vertex_type_10_10_10_2 = 1;

		/*------------------------------------------------------------------------*
		 * AMD extension functions
		 *------------------------------------------------------------------------*/

		/* GL_AMD_compressed_3DC_texture */
		public const GLenum GL_AMD_compressed_3DC_texture = 1;

		/* GL_AMD_compressed_ATC_texture */
		public const GLenum GL_AMD_compressed_ATC_texture = 1;

		/* AMD_performance_monitor */
		/*
		#ifndef GL_AMD_performance_monitor
		public const GLenum GL_AMD_performance_monitor 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glGetPerfMonitorGroupsAMD (GLint *numGroups, GLsizei groupsSize, GLuint *groups);
		GL_APICALL void GL_APIENTRY glGetPerfMonitorCountersAMD (GLuint group, GLint *numCounters, GLint *maxActiveCounters, GLsizei counterSize, GLuint *counters);
		GL_APICALL void GL_APIENTRY glGetPerfMonitorGroupStringAMD (GLuint group, GLsizei bufSize, GLsizei *length, GLchar *groupString);
		GL_APICALL void GL_APIENTRY glGetPerfMonitorCounterStringAMD (GLuint group, GLuint counter, GLsizei bufSize, GLsizei *length, GLchar *counterString);
		GL_APICALL void GL_APIENTRY glGetPerfMonitorCounterInfoAMD (GLuint group, GLuint counter, GLenum pname, GLvoid *data);
		GL_APICALL void GL_APIENTRY glGenPerfMonitorsAMD (GLsizei n, GLuint *monitors);
		GL_APICALL void GL_APIENTRY glDeletePerfMonitorsAMD (GLsizei n, GLuint *monitors);
		GL_APICALL void GL_APIENTRY glSelectPerfMonitorCountersAMD (GLuint monitor, GLboolean enable, GLuint group, GLint numCounters, GLuint *countersList);
		GL_APICALL void GL_APIENTRY glBeginPerfMonitorAMD (GLuint monitor);
		GL_APICALL void GL_APIENTRY glEndPerfMonitorAMD (GLuint monitor);
		GL_APICALL void GL_APIENTRY glGetPerfMonitorCounterDataAMD (GLuint monitor, GLenum pname, GLsizei dataSize, GLuint *data, GLint *bytesWritten);
		#endif
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORGROUPSAMDPROC) (GLint *numGroups, GLsizei groupsSize, GLuint *groups);
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORCOUNTERSAMDPROC) (GLuint group, GLint *numCounters, GLint *maxActiveCounters, GLsizei counterSize, GLuint *counters);
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORGROUPSTRINGAMDPROC) (GLuint group, GLsizei bufSize, GLsizei *length, GLchar *groupString);
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORCOUNTERSTRINGAMDPROC) (GLuint group, GLuint counter, GLsizei bufSize, GLsizei *length, GLchar *counterString);
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORCOUNTERINFOAMDPROC) (GLuint group, GLuint counter, GLenum pname, GLvoid *data);
		typedef void (GL_APIENTRYP PFNGLGENPERFMONITORSAMDPROC) (GLsizei n, GLuint *monitors);
		typedef void (GL_APIENTRYP PFNGLDELETEPERFMONITORSAMDPROC) (GLsizei n, GLuint *monitors);
		typedef void (GL_APIENTRYP PFNGLSELECTPERFMONITORCOUNTERSAMDPROC) (GLuint monitor, GLboolean enable, GLuint group, GLint numCounters, GLuint *countersList);
		typedef void (GL_APIENTRYP PFNGLBEGINPERFMONITORAMDPROC) (GLuint monitor);
		typedef void (GL_APIENTRYP PFNGLENDPERFMONITORAMDPROC) (GLuint monitor);
		typedef void (GL_APIENTRYP PFNGLGETPERFMONITORCOUNTERDATAAMDPROC) (GLuint monitor, GLenum pname, GLsizei dataSize, GLuint *data, GLint *bytesWritten);
		#endif
		*/

		/* GL_AMD_program_binary_Z400 */
		public const GLenum GL_AMD_program_binary_Z400 = 1;

		/*------------------------------------------------------------------------*
		 * ANGLE extension functions
		 *------------------------------------------------------------------------*/

		/* GL_ANGLE_framebuffer_blit */
		/*
		#ifndef GL_ANGLE_framebuffer_blit
		public const GLenum GL_ANGLE_framebuffer_blit 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glBlitFramebufferANGLE (GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter);
		#endif
		typedef void (GL_APIENTRYP PFNGLBLITFRAMEBUFFERANGLEPROC) (GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter);
		#endif
		*/

		/* GL_ANGLE_framebuffer_multisample */
		/*
		#ifndef GL_ANGLE_framebuffer_multisample
		public const GLenum GL_ANGLE_framebuffer_multisample 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glRenderbufferStorageMultisampleANGLE (GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);
		#endif
		typedef void (GL_APIENTRYP PFNGLRENDERBUFFERSTORAGEMULTISAMPLEANGLEPROC) (GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);
		#endif
		*/

		/* GL_ANGLE_pack_reverse_row_order */
		public const GLenum GL_ANGLE_pack_reverse_row_order = 1;

		/* GL_ANGLE_texture_compression_dxt3 */
		public const GLenum GL_ANGLE_texture_compression_dxt3 = 1;

		/* GL_ANGLE_texture_compression_dxt5 */
		public const GLenum GL_ANGLE_texture_compression_dxt5 = 1;

		/* GL_ANGLE_translated_shader_source */
		/*
		#ifndef GL_ANGLE_translated_shader_source
		public const GLenum GL_ANGLE_translated_shader_source 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glGetTranslatedShaderSourceANGLE (GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source);
		#endif
		typedef void (GL_APIENTRYP PFNGLGETTRANSLATEDSHADERSOURCEANGLEPROC) (GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source);
		#endif
		*/

		/* GL_ANGLE_texture_usage */
		public const GLenum GL_ANGLE_texture_usage = 1;

		/* GL_ANGLE_instanced_arrays */
		/*
		#ifndef GL_ANGLE_instanced_arrays
		public const GLenum GL_ANGLE_instanced_arrays 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glVertexAttribDivisorANGLE(GLuint index, GLuint divisor);
		GL_APICALL void GL_APIENTRY glDrawArraysInstancedANGLE(GLenum mode, GLint first, GLsizei count, GLsizei primcount);
		GL_APICALL void GL_APIENTRY glDrawElementsInstancedANGLE(GLenum mode, GLsizei count, GLenum type, const GLvoid *indices, GLsizei primcount);
		#endif
		typedef void (GL_APIENTRYP PFNGLVERTEXATTRIBDIVISORANGLEPROC) (GLuint index, GLuint divisor);
		typedef void (GL_APIENTRYP PFNGLDRAWARRAYSINSTANCEDANGLEPROC) (GLenum mode, GLint first, GLsizei count, GLsizei primcount);
		typedef void (GL_APIENTRYP PFNGLDRAWELEMENTSINSTANCEDANGLEPROC) (GLenum mode, GLsizei count, GLenum type, const GLvoid *indices, GLsizei primcount);
		#endif
		*/

		/*------------------------------------------------------------------------*
		 * APPLE extension functions
		 *------------------------------------------------------------------------*/

		/* GL_APPLE_rgb_422 */
		public const GLenum GL_APPLE_rgb_422 = 1;

		/* GL_APPLE_framebuffer_multisample */
		/*
		#ifndef GL_APPLE_framebuffer_multisample
		public const GLenum GL_APPLE_framebuffer_multisample 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glRenderbufferStorageMultisampleAPPLE (GLenum, GLsizei, GLenum, GLsizei, GLsizei);
		GL_APICALL void GL_APIENTRY glResolveMultisampleFramebufferAPPLE (void);
		#endif // GL_GLEXT_PROTOTYPES
		typedef void (GL_APIENTRYP PFNGLRENDERBUFFERSTORAGEMULTISAMPLEAPPLEPROC) (GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLRESOLVEMULTISAMPLEFRAMEBUFFERAPPLEPROC) (void);
		#endif
		*/

		/* GL_APPLE_texture_format_BGRA8888 */
		public const GLenum GL_APPLE_texture_format_BGRA8888 = 1;

		/* GL_APPLE_texture_max_level */
		public const GLenum GL_APPLE_texture_max_level = 1;

		/*------------------------------------------------------------------------*
		 * ARM extension functions
		 *------------------------------------------------------------------------*/

		/* GL_ARM_mali_shader_binary */
		public const GLenum GL_ARM_mali_shader_binary = 1;

		/* GL_ARM_rgba8 */
		public const GLenum GL_ARM_rgba8 = 1;

		/*------------------------------------------------------------------------*
		 * EXT extension functions
		 *------------------------------------------------------------------------*/

		/* GL_EXT_blend_minmax */
		public const GLenum GL_EXT_blend_minmax = 1;

		/* GL_EXT_color_buffer_half_float */
		public const GLenum GL_EXT_color_buffer_half_float = 1;

		/* GL_EXT_debug_label */
		/*
		#ifndef GL_EXT_debug_label
		public const GLenum GL_EXT_debug_label 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glLabelObjectEXT (GLenum type, GLuint object, GLsizei length, const GLchar *label);
		GL_APICALL void GL_APIENTRY glGetObjectLabelEXT (GLenum type, GLuint object, GLsizei bufSize, GLsizei *length, GLchar *label);
		#endif
		typedef void (GL_APIENTRYP PFNGLLABELOBJECTEXTPROC) (GLenum type, GLuint object, GLsizei length, const GLchar *label);
		typedef void (GL_APIENTRYP PFNGLGETOBJECTLABELEXTPROC) (GLenum type, GLuint object, GLsizei bufSize, GLsizei *length, GLchar *label);
		#endif
		*/

		/* GL_EXT_debug_marker */
		/*
		#ifndef GL_EXT_debug_marker
		public const GLenum GL_EXT_debug_marker 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glInsertEventMarkerEXT (GLsizei length, const GLchar *marker);
		GL_APICALL void GL_APIENTRY glPushGroupMarkerEXT (GLsizei length, const GLchar *marker);
		GL_APICALL void GL_APIENTRY glPopGroupMarkerEXT (void);
		#endif
		typedef void (GL_APIENTRYP PFNGLINSERTEVENTMARKEREXTPROC) (GLsizei length, const GLchar *marker);
		typedef void (GL_APIENTRYP PFNGLPUSHGROUPMARKEREXTPROC) (GLsizei length, const GLchar *marker);
		typedef void (GL_APIENTRYP PFNGLPOPGROUPMARKEREXTPROC) (void);
		#endif
		*/

		/* GL_EXT_discard_framebuffer */
		/*
		#ifndef GL_EXT_discard_framebuffer
		public const GLenum GL_EXT_discard_framebuffer 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glDiscardFramebufferEXT (GLenum target, GLsizei numAttachments, public const GLenum *attachments);
		#endif
		typedef void (GL_APIENTRYP PFNGLDISCARDFRAMEBUFFEREXTPROC) (GLenum target, GLsizei numAttachments, public const GLenum *attachments);
		#endif
		*/

		/* GL_EXT_multisampled_render_to_texture */
		/*
		#ifndef GL_EXT_multisampled_render_to_texture
		public const GLenum GL_EXT_multisampled_render_to_texture 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glRenderbufferStorageMultisampleEXT (GLenum, GLsizei, GLenum, GLsizei, GLsizei);
		GL_APICALL void GL_APIENTRY glFramebufferTexture2DMultisampleEXT (GLenum, GLenum, GLenum, GLuint, GLint, GLsizei);
		#endif
		typedef void (GL_APIENTRYP PFNGLRENDERBUFFERSTORAGEMULTISAMPLEEXTPROC) (GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLFRAMEBUFFERTEXTURE2DMULTISAMPLEEXTPROC) (GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLsizei samples);
		#endif
		*/

		/*
		#ifndef GL_EXT_multi_draw_arrays
		public const GLenum GL_EXT_multi_draw_arrays 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glMultiDrawArraysEXT (GLenum, GLint *, GLsizei *, GLsizei);
		GL_APICALL void GL_APIENTRY glMultiDrawElementsEXT (GLenum, const GLsizei *, GLenum, const GLvoid* *, GLsizei);
		#endif // GL_GLEXT_PROTOTYPES
		typedef void (GL_APIENTRYP PFNGLMULTIDRAWARRAYSEXTPROC) (GLenum mode, GLint *first, GLsizei *count, GLsizei primcount);
		typedef void (GL_APIENTRYP PFNGLMULTIDRAWELEMENTSEXTPROC) (GLenum mode, const GLsizei *count, GLenum type, const GLvoid* *indices, GLsizei primcount);
		#endif
		*/

		/* GL_EXT_occlusion_query_boolean */
		/*
		#ifndef GL_EXT_occlusion_query_boolean
		public const GLenum GL_EXT_occlusion_query_boolean 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glGenQueriesEXT (GLsizei n, GLuint *ids);
		GL_APICALL void GL_APIENTRY glDeleteQueriesEXT (GLsizei n, const GLuint *ids);
		GL_APICALL GLboolean GL_APIENTRY glIsQueryEXT (GLuint id);
		GL_APICALL void GL_APIENTRY glBeginQueryEXT (GLenum target, GLuint id);
		GL_APICALL void GL_APIENTRY glEndQueryEXT (GLenum target);
		GL_APICALL void GL_APIENTRY glGetQueryivEXT (GLenum target, GLenum pname, GLint *params);
		GL_APICALL void GL_APIENTRY glGetQueryObjectuivEXT (GLuint id, GLenum pname, GLuint *params);
		#endif
		typedef void (GL_APIENTRYP PFNGLGENQUERIESEXTPROC) (GLsizei n, GLuint *ids);
		typedef void (GL_APIENTRYP PFNGLDELETEQUERIESEXTPROC) (GLsizei n, const GLuint *ids);
		typedef GLboolean (GL_APIENTRYP PFNGLISQUERYEXTPROC) (GLuint id);
		typedef void (GL_APIENTRYP PFNGLBEGINQUERYEXTPROC) (GLenum target, GLuint id);
		typedef void (GL_APIENTRYP PFNGLENDQUERYEXTPROC) (GLenum target);
		typedef void (GL_APIENTRYP PFNGLGETQUERYIVEXTPROC) (GLenum target, GLenum pname, GLint *params);
		typedef void (GL_APIENTRYP PFNGLGETQUERYOBJECTUIVEXTPROC) (GLuint id, GLenum pname, GLuint *params);
		#endif
		*/

		/* GL_EXT_read_format_bgra */
		public const GLenum GL_EXT_read_format_bgra = 1;

		/* GL_EXT_robustness */
		/*
		#ifndef GL_EXT_robustness
		public const GLenum GL_EXT_robustness 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL GLenum GL_APIENTRY glGetGraphicsResetStatusEXT (void);
		GL_APICALL void GL_APIENTRY glReadnPixelsEXT (GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLsizei bufSize, void *data);
		GL_APICALL void GL_APIENTRY glGetnUniformfvEXT (GLuint program, GLint location, GLsizei bufSize, float *params);
		GL_APICALL void GL_APIENTRY glGetnUniformivEXT (GLuint program, GLint location, GLsizei bufSize, GLint *params);
		#endif
		typedef GLenum (GL_APIENTRYP PFNGLGETGRAPHICSRESETSTATUSEXTPROC) (void);
		typedef void (GL_APIENTRYP PFNGLREADNPIXELSEXTPROC) (GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLsizei bufSize, void *data);
		typedef void (GL_APIENTRYP PFNGLGETNUNIFORMFVEXTPROC) (GLuint program, GLint location, GLsizei bufSize, float *params);
		typedef void (GL_APIENTRYP PFNGLGETNUNIFORMIVEXTPROC) (GLuint program, GLint location, GLsizei bufSize, GLint *params);
		#endif
		*/

		/* GL_EXT_separate_shader_objects */
		/*
		#ifndef GL_EXT_separate_shader_objects
		public const GLenum GL_EXT_separate_shader_objects 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glUseProgramStagesEXT (GLuint pipeline, GLbitfield stages, GLuint program);
		GL_APICALL void GL_APIENTRY glActiveShaderProgramEXT (GLuint pipeline, GLuint program);
		GL_APICALL GLuint GL_APIENTRY glCreateShaderProgramvEXT (GLenum type, GLsizei count, const GLchar **strings);
		GL_APICALL void GL_APIENTRY glBindProgramPipelineEXT (GLuint pipeline);
		GL_APICALL void GL_APIENTRY glDeleteProgramPipelinesEXT (GLsizei n, const GLuint *pipelines);
		GL_APICALL void GL_APIENTRY glGenProgramPipelinesEXT (GLsizei n, GLuint *pipelines);
		GL_APICALL GLboolean GL_APIENTRY glIsProgramPipelineEXT (GLuint pipeline);
		GL_APICALL void GL_APIENTRY glProgramParameteriEXT (GLuint program, GLenum pname, GLint value);
		GL_APICALL void GL_APIENTRY glGetProgramPipelineivEXT (GLuint pipeline, GLenum pname, GLint *params);
		GL_APICALL void GL_APIENTRY glProgramUniform1iEXT (GLuint program, GLint location, GLint x);
		GL_APICALL void GL_APIENTRY glProgramUniform2iEXT (GLuint program, GLint location, GLint x, GLint y);
		GL_APICALL void GL_APIENTRY glProgramUniform3iEXT (GLuint program, GLint location, GLint x, GLint y, GLint z);
		GL_APICALL void GL_APIENTRY glProgramUniform4iEXT (GLuint program, GLint location, GLint x, GLint y, GLint z, GLint w);
		GL_APICALL void GL_APIENTRY glProgramUniform1fEXT (GLuint program, GLint location, GLfloat x);
		GL_APICALL void GL_APIENTRY glProgramUniform2fEXT (GLuint program, GLint location, GLfloat x, GLfloat y);
		GL_APICALL void GL_APIENTRY glProgramUniform3fEXT (GLuint program, GLint location, GLfloat x, GLfloat y, GLfloat z);
		GL_APICALL void GL_APIENTRY glProgramUniform4fEXT (GLuint program, GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w);
		GL_APICALL void GL_APIENTRY glProgramUniform1ivEXT (GLuint program, GLint location, GLsizei count, const GLint *value);
		GL_APICALL void GL_APIENTRY glProgramUniform2ivEXT (GLuint program, GLint location, GLsizei count, const GLint *value);
		GL_APICALL void GL_APIENTRY glProgramUniform3ivEXT (GLuint program, GLint location, GLsizei count, const GLint *value);
		GL_APICALL void GL_APIENTRY glProgramUniform4ivEXT (GLuint program, GLint location, GLsizei count, const GLint *value);
		GL_APICALL void GL_APIENTRY glProgramUniform1fvEXT (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniform2fvEXT (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniform3fvEXT (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniform4fvEXT (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniformMatrix2fvEXT (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniformMatrix3fvEXT (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glProgramUniformMatrix4fvEXT (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		GL_APICALL void GL_APIENTRY glValidateProgramPipelineEXT (GLuint pipeline);
		GL_APICALL void GL_APIENTRY glGetProgramPipelineInfoLogEXT (GLuint pipeline, GLsizei bufSize, GLsizei *length, GLchar *infoLog);
		#endif
		typedef void (GL_APIENTRYP PFNGLUSEPROGRAMSTAGESEXTPROC) (GLuint pipeline, GLbitfield stages, GLuint program);
		typedef void (GL_APIENTRYP PFNGLACTIVESHADERPROGRAMEXTPROC) (GLuint pipeline, GLuint program);
		typedef GLuint (GL_APIENTRYP PFNGLCREATESHADERPROGRAMVEXTPROC) (GLenum type, GLsizei count, const GLchar **strings);
		typedef void (GL_APIENTRYP PFNGLBINDPROGRAMPIPELINEEXTPROC) (GLuint pipeline);
		typedef void (GL_APIENTRYP PFNGLDELETEPROGRAMPIPELINESEXTPROC) (GLsizei n, const GLuint *pipelines);
		typedef void (GL_APIENTRYP PFNGLGENPROGRAMPIPELINESEXTPROC) (GLsizei n, GLuint *pipelines);
		typedef GLboolean (GL_APIENTRYP PFNGLISPROGRAMPIPELINEEXTPROC) (GLuint pipeline);
		typedef void (GL_APIENTRYP PFNGLPROGRAMPARAMETERIEXTPROC) (GLuint program, GLenum pname, GLint value);
		typedef void (GL_APIENTRYP PFNGLGETPROGRAMPIPELINEIVEXTPROC) (GLuint pipeline, GLenum pname, GLint *params);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM1IEXTPROC) (GLuint program, GLint location, GLint x);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM2IEXTPROC) (GLuint program, GLint location, GLint x, GLint y);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM3IEXTPROC) (GLuint program, GLint location, GLint x, GLint y, GLint z);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM4IEXTPROC) (GLuint program, GLint location, GLint x, GLint y, GLint z, GLint w);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM1FEXTPROC) (GLuint program, GLint location, GLfloat x);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM2FEXTPROC) (GLuint program, GLint location, GLfloat x, GLfloat y);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM3FEXTPROC) (GLuint program, GLint location, GLfloat x, GLfloat y, GLfloat z);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM4FEXTPROC) (GLuint program, GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM1IVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLint *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM2IVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLint *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM3IVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLint *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM4IVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLint *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM1FVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM2FVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM3FVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORM4FVEXTPROC) (GLuint program, GLint location, GLsizei count, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORMMATRIX2FVEXTPROC) (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORMMATRIX3FVEXTPROC) (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLPROGRAMUNIFORMMATRIX4FVEXTPROC) (GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value);
		typedef void (GL_APIENTRYP PFNGLVALIDATEPROGRAMPIPELINEEXTPROC) (GLuint pipeline);
		typedef void (GL_APIENTRYP PFNGLGETPROGRAMPIPELINEINFOLOGEXTPROC) (GLuint pipeline, GLsizei bufSize, GLsizei *length, GLchar *infoLog);
		#endif
		*/

		/* GL_EXT_shader_texture_lod */
		public const GLenum GL_EXT_shader_texture_lod = 1;

		/* GL_EXT_shadow_samplers */
		public const GLenum GL_EXT_shadow_samplers = 1;

		/* GL_EXT_sRGB */
		public const GLenum GL_EXT_sRGB = 1;

		/* GL_EXT_texture_compression_dxt1 */
		public const GLenum GL_EXT_texture_compression_dxt1 = 1;

		/* GL_EXT_texture_filter_anisotropic */
		public const GLenum GL_EXT_texture_filter_anisotropic = 1;

		/* GL_EXT_texture_format_BGRA8888 */
		public const GLenum GL_EXT_texture_format_BGRA8888 = 1;

		/* GL_EXT_texture_rg */
		public const GLenum GL_EXT_texture_rg = 1;

		/* GL_EXT_texture_storage */
		/*
		#ifndef GL_EXT_texture_storage
		public const GLenum GL_EXT_texture_storage 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glTexStorage1DEXT (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width);
		GL_APICALL void GL_APIENTRY glTexStorage2DEXT (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
		GL_APICALL void GL_APIENTRY glTexStorage3DEXT (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth);
		GL_APICALL void GL_APIENTRY glTextureStorage1DEXT (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width);
		GL_APICALL void GL_APIENTRY glTextureStorage2DEXT (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
		GL_APICALL void GL_APIENTRY glTextureStorage3DEXT (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth);
		#endif
		typedef void (GL_APIENTRYP PFNGLTEXSTORAGE1DEXTPROC) (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width);
		typedef void (GL_APIENTRYP PFNGLTEXSTORAGE2DEXTPROC) (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLTEXSTORAGE3DEXTPROC) (GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth);
		typedef void (GL_APIENTRYP PFNGLTEXTURESTORAGE1DEXTPROC) (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width);
		typedef void (GL_APIENTRYP PFNGLTEXTURESTORAGE2DEXTPROC) (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLTEXTURESTORAGE3DEXTPROC) (GLuint texture, GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth);
		#endif
		*/

		/* GL_EXT_texture_type_2_10_10_10_REV */
		public const GLenum GL_EXT_texture_type_2_10_10_10_REV = 1;

		/* GL_EXT_unpack_subimage */
		public const GLenum GL_EXT_unpack_subimage = 1;

		/*------------------------------------------------------------------------*
		 * DMP extension functions
		 *------------------------------------------------------------------------*/

		/* GL_DMP_shader_binary */
		public const GLenum GL_DMP_shader_binary = 1;

		/*------------------------------------------------------------------------*
		 * IMG extension functions
		 *------------------------------------------------------------------------*/

		/* GL_IMG_program_binary */
		public const GLenum GL_IMG_program_binary = 1;

		/* GL_IMG_read_format */
		public const GLenum GL_IMG_read_format = 1;

		/* GL_IMG_shader_binary */
		public const GLenum GL_IMG_shader_binary = 1;

		/* GL_IMG_texture_compression_pvrtc */
		public const GLenum GL_IMG_texture_compression_pvrtc = 1;

		/* GL_IMG_multisampled_render_to_texture */
		/*
		#ifndef GL_IMG_multisampled_render_to_texture
		public const GLenum GL_IMG_multisampled_render_to_texture 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glRenderbufferStorageMultisampleIMG (GLenum, GLsizei, GLenum, GLsizei, GLsizei);
		GL_APICALL void GL_APIENTRY glFramebufferTexture2DMultisampleIMG (GLenum, GLenum, GLenum, GLuint, GLint, GLsizei);
		#endif
		typedef void (GL_APIENTRYP PFNGLRENDERBUFFERSTORAGEMULTISAMPLEIMG) (GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);
		typedef void (GL_APIENTRYP PFNGLFRAMEBUFFERTEXTURE2DMULTISAMPLEIMG) (GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLsizei samples);
		#endif
		*/

		/*------------------------------------------------------------------------*
		 * NV extension functions
		 *------------------------------------------------------------------------*/

		/* GL_NV_coverage_sample */
		/*
		#ifndef GL_NV_coverage_sample
		public const GLenum GL_NV_coverage_sample 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glCoverageMaskNV (GLboolean mask);
		GL_APICALL void GL_APIENTRY glCoverageOperationNV (GLenum operation);
		#endif
		typedef void (GL_APIENTRYP PFNGLCOVERAGEMASKNVPROC) (GLboolean mask);
		typedef void (GL_APIENTRYP PFNGLCOVERAGEOPERATIONNVPROC) (GLenum operation);
		#endif
		*/

		/* GL_NV_depth_nonlinear */
		public const GLenum GL_NV_depth_nonlinear = 1;

		/* GL_NV_draw_buffers */
		/*
		#ifndef GL_NV_draw_buffers
		public const GLenum GL_NV_draw_buffers 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glDrawBuffersNV (GLsizei n, public const GLenum *bufs);
		#endif
		typedef void (GL_APIENTRYP PFNGLDRAWBUFFERSNVPROC) (GLsizei n, public const GLenum *bufs);
		#endif
		*/

		/* GL_NV_fbo_color_attachments */
		public const GLenum GL_NV_fbo_color_attachments = 1;

		/* GL_NV_fence */
		/*
		#ifndef GL_NV_fence
		public const GLenum GL_NV_fence 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glDeleteFencesNV (GLsizei, const GLuint *);
		GL_APICALL void GL_APIENTRY glGenFencesNV (GLsizei, GLuint *);
		GL_APICALL GLboolean GL_APIENTRY glIsFenceNV (GLuint);
		GL_APICALL GLboolean GL_APIENTRY glTestFenceNV (GLuint);
		GL_APICALL void GL_APIENTRY glGetFenceivNV (GLuint, GLenum, GLint *);
		GL_APICALL void GL_APIENTRY glFinishFenceNV (GLuint);
		GL_APICALL void GL_APIENTRY glSetFenceNV (GLuint, GLenum);
		#endif
		typedef void (GL_APIENTRYP PFNGLDELETEFENCESNVPROC) (GLsizei n, const GLuint *fences);
		typedef void (GL_APIENTRYP PFNGLGENFENCESNVPROC) (GLsizei n, GLuint *fences);
		typedef GLboolean (GL_APIENTRYP PFNGLISFENCENVPROC) (GLuint fence);
		typedef GLboolean (GL_APIENTRYP PFNGLTESTFENCENVPROC) (GLuint fence);
		typedef void (GL_APIENTRYP PFNGLGETFENCEIVNVPROC) (GLuint fence, GLenum pname, GLint *params);
		typedef void (GL_APIENTRYP PFNGLFINISHFENCENVPROC) (GLuint fence);
		typedef void (GL_APIENTRYP PFNGLSETFENCENVPROC) (GLuint fence, GLenum condition);
		#endif
		 * */

		/* GL_NV_read_buffer */
		/*
		#ifndef GL_NV_read_buffer
		public const GLenum GL_NV_read_buffer 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glReadBufferNV (GLenum mode);
		#endif
		typedef void (GL_APIENTRYP PFNGLREADBUFFERNVPROC) (GLenum mode);
		#endif
		*/

		/* GL_NV_read_buffer_front */
		public const GLenum GL_NV_read_buffer_front = 1;

		/* GL_NV_read_depth */
		public const GLenum GL_NV_read_depth = 1;

		/* GL_NV_read_depth_stencil */
		public const GLenum GL_NV_read_depth_stencil = 1;

		/* GL_NV_read_stencil */
		public const GLenum GL_NV_read_stencil = 1;

		/* GL_NV_texture_compression_s3tc_update */
		public const GLenum GL_NV_texture_compression_s3tc_update = 1;

		/* GL_NV_texture_npot_2D_mipmap */
		public const GLenum GL_NV_texture_npot_2D_mipmap = 1;

		/*------------------------------------------------------------------------*
		 * QCOM extension functions
		 *------------------------------------------------------------------------*/

		/* GL_QCOM_alpha_test */
		/*
		#ifndef GL_QCOM_alpha_test
		public const GLenum GL_QCOM_alpha_test 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glAlphaFuncQCOM (GLenum func, GLclampf ref);
		#endif
		typedef void (GL_APIENTRYP PFNGLALPHAFUNCQCOMPROC) (GLenum func, GLclampf ref);
		#endif
		*/

		/* GL_QCOM_driver_control */
		/*
		#ifndef GL_QCOM_driver_control
		public const GLenum GL_QCOM_driver_control 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glGetDriverControlsQCOM (GLint *num, GLsizei size, GLuint *driverControls);
		GL_APICALL void GL_APIENTRY glGetDriverControlStringQCOM (GLuint driverControl, GLsizei bufSize, GLsizei *length, GLchar *driverControlString);
		GL_APICALL void GL_APIENTRY glEnableDriverControlQCOM (GLuint driverControl);
		GL_APICALL void GL_APIENTRY glDisableDriverControlQCOM (GLuint driverControl);
		#endif
		typedef void (GL_APIENTRYP PFNGLGETDRIVERCONTROLSQCOMPROC) (GLint *num, GLsizei size, GLuint *driverControls);
		typedef void (GL_APIENTRYP PFNGLGETDRIVERCONTROLSTRINGQCOMPROC) (GLuint driverControl, GLsizei bufSize, GLsizei *length, GLchar *driverControlString);
		typedef void (GL_APIENTRYP PFNGLENABLEDRIVERCONTROLQCOMPROC) (GLuint driverControl);
		typedef void (GL_APIENTRYP PFNGLDISABLEDRIVERCONTROLQCOMPROC) (GLuint driverControl);
		#endif
		*/

		/* GL_QCOM_extended_get */
		/*
		#ifndef GL_QCOM_extended_get
		public const GLenum GL_QCOM_extended_get 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glExtGetTexturesQCOM (GLuint *textures, GLint maxTextures, GLint *numTextures);
		GL_APICALL void GL_APIENTRY glExtGetBuffersQCOM (GLuint *buffers, GLint maxBuffers, GLint *numBuffers);
		GL_APICALL void GL_APIENTRY glExtGetRenderbuffersQCOM (GLuint *renderbuffers, GLint maxRenderbuffers, GLint *numRenderbuffers);
		GL_APICALL void GL_APIENTRY glExtGetFramebuffersQCOM (GLuint *framebuffers, GLint maxFramebuffers, GLint *numFramebuffers);
		GL_APICALL void GL_APIENTRY glExtGetTexLevelParameterivQCOM (GLuint texture, GLenum face, GLint level, GLenum pname, GLint *params);
		GL_APICALL void GL_APIENTRY glExtTexObjectStateOverrideiQCOM (GLenum target, GLenum pname, GLint param);
		GL_APICALL void GL_APIENTRY glExtGetTexSubImageQCOM (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, GLvoid *texels);
		GL_APICALL void GL_APIENTRY glExtGetBufferPointervQCOM (GLenum target, GLvoid **params);
		#endif
		typedef void (GL_APIENTRYP PFNGLEXTGETTEXTURESQCOMPROC) (GLuint *textures, GLint maxTextures, GLint *numTextures);
		typedef void (GL_APIENTRYP PFNGLEXTGETBUFFERSQCOMPROC) (GLuint *buffers, GLint maxBuffers, GLint *numBuffers);
		typedef void (GL_APIENTRYP PFNGLEXTGETRENDERBUFFERSQCOMPROC) (GLuint *renderbuffers, GLint maxRenderbuffers, GLint *numRenderbuffers);
		typedef void (GL_APIENTRYP PFNGLEXTGETFRAMEBUFFERSQCOMPROC) (GLuint *framebuffers, GLint maxFramebuffers, GLint *numFramebuffers);
		typedef void (GL_APIENTRYP PFNGLEXTGETTEXLEVELPARAMETERIVQCOMPROC) (GLuint texture, GLenum face, GLint level, GLenum pname, GLint *params);
		typedef void (GL_APIENTRYP PFNGLEXTTEXOBJECTSTATEOVERRIDEIQCOMPROC) (GLenum target, GLenum pname, GLint param);
		typedef void (GL_APIENTRYP PFNGLEXTGETTEXSUBIMAGEQCOMPROC) (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, GLvoid *texels);
		typedef void (GL_APIENTRYP PFNGLEXTGETBUFFERPOINTERVQCOMPROC) (GLenum target, GLvoid **params);
		#endif
		*/

		/* GL_QCOM_extended_get2 */
		/*
		#ifndef GL_QCOM_extended_get2
		public const GLenum GL_QCOM_extended_get2 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glExtGetShadersQCOM (GLuint *shaders, GLint maxShaders, GLint *numShaders);
		GL_APICALL void GL_APIENTRY glExtGetProgramsQCOM (GLuint *programs, GLint maxPrograms, GLint *numPrograms);
		GL_APICALL GLboolean GL_APIENTRY glExtIsProgramBinaryQCOM (GLuint program);
		GL_APICALL void GL_APIENTRY glExtGetProgramBinarySourceQCOM (GLuint program, GLenum shadertype, GLchar *source, GLint *length);
		#endif
		typedef void (GL_APIENTRYP PFNGLEXTGETSHADERSQCOMPROC) (GLuint *shaders, GLint maxShaders, GLint *numShaders);
		typedef void (GL_APIENTRYP PFNGLEXTGETPROGRAMSQCOMPROC) (GLuint *programs, GLint maxPrograms, GLint *numPrograms);
		typedef GLboolean (GL_APIENTRYP PFNGLEXTISPROGRAMBINARYQCOMPROC) (GLuint program);
		typedef void (GL_APIENTRYP PFNGLEXTGETPROGRAMBINARYSOURCEQCOMPROC) (GLuint program, GLenum shadertype, GLchar *source, GLint *length);
		#endif
		*/

		/* GL_QCOM_perfmon_global_mode */
		public const GLenum GL_QCOM_perfmon_global_mode = 1;

		/* GL_QCOM_writeonly_rendering */
		public const GLenum GL_QCOM_writeonly_rendering = 1;

		/* GL_QCOM_tiled_rendering */
		/*
		#ifndef GL_QCOM_tiled_rendering
		public const GLenum GL_QCOM_tiled_rendering 1
		#ifdef GL_GLEXT_PROTOTYPES
		GL_APICALL void GL_APIENTRY glStartTilingQCOM (GLuint x, GLuint y, GLuint width, GLuint height, GLbitfield preserveMask);
		GL_APICALL void GL_APIENTRY glEndTilingQCOM (GLbitfield preserveMask);
		#endif
		typedef void (GL_APIENTRYP PFNGLSTARTTILINGQCOMPROC) (GLuint x, GLuint y, GLuint width, GLuint height, GLbitfield preserveMask);
		typedef void (GL_APIENTRYP PFNGLENDTILINGQCOMPROC) (GLbitfield preserveMask);
		#endif
		*/

		/*------------------------------------------------------------------------*
		 * VIV extension tokens
		 *------------------------------------------------------------------------*/

		/* GL_VIV_shader_binary */
		public const GLenum GL_VIV_shader_binary = 1;

		/* GL_ANGLE_program_binary */
		public const GLenum GL_ANGLE_program_binary = 1;
	}
}
