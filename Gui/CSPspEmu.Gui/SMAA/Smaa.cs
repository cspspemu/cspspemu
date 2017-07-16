using CSharpPlatform.GL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui.SMAA
{
    public class Smaa : IDisposable
    {
        const int AREATEX_WIDTH = 160;
        const int AREATEX_HEIGHT = 560;
        const int SEARCHTEX_WIDTH = 66;
        const int SEARCHTEX_HEIGHT = 33;

        GLShaderFilter edge_shader;
        GLShaderFilter blend_shader;
        GLShaderFilter neighborhood_shader;

        GLTextureUnit area_tex_unit;
        GLTextureUnit search_tex_unit;

        private static readonly string SMAA_H = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CSPspEmu.Gui.SMAA.SMAA.h").ReadAllContentsAsString();

        private static readonly byte[] SMAA_AREA_RAW = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CSPspEmu.Gui.SMAA.smaa_area.raw").ReadAll();

        private static readonly byte[] SMAA_SEARCH_RAW = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CSPspEmu.Gui.SMAA.smaa_search.raw").ReadAll();

        string CommonHeader;
        string VertexHeader;
        string FragmentHeader;

        int Width = 1;
        int Height = 1;

        public Smaa()
        {
            this.area_tex_unit = GLTextureUnit.Create()
                .SetTexture(GLTexture.Create().SetFormat(TextureFormat.RG).SetSize(AREATEX_WIDTH, AREATEX_HEIGHT)
                    .SetData(SMAA_AREA_RAW)).SetWrap(GLWrap.ClampToEdge).SetFiltering(GLScaleFilter.Linear);
            this.search_tex_unit = GLTextureUnit.Create()
                .SetTexture(GLTexture.Create().SetFormat(TextureFormat.R).SetSize(SEARCHTEX_WIDTH, SEARCHTEX_HEIGHT)
                    .SetData(SMAA_SEARCH_RAW)).SetWrap(GLWrap.ClampToEdge).SetFiltering(GLScaleFilter.Nearest);

            CommonHeader = @"
				#version 410 compatibility
				#define SMAA_PIXEL_SIZE u_pixelSize.xy
				#define SMAA_PRESET_ULTRA 1
				#define SMAA_GLSL_4 1
				//#define SMAA_PREDICATION 0
				#define SMAA_PREDICATION 1
				#define attribute in
			";

            VertexHeader = CommonHeader + "\n" + "#define SMAA_ONLY_COMPILE_VS 1" + "\n" + @"
				uniform   vec4 u_pixelSize;
				attribute vec4 a_position;
				attribute vec4 a_texcoords;
			" + SMAA_H + "\n";

            FragmentHeader = CommonHeader + "\n" + "#define SMAA_ONLY_COMPILE_PS 1" + "\n" + @"
				uniform vec4 u_pixelSize;
			" + "\n" + SMAA_H;
        }

        private Smaa SetSize(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            if (edge_shader != null) edge_shader.SetSize(Width, Height);
            if (blend_shader != null) blend_shader.SetSize(Width, Height);
            if (neighborhood_shader != null) neighborhood_shader.SetSize(Width, Height);
            return this;
        }

        private GLTexture edge_pass(GLTexture albedo_tex, GLTexture depthTex)
        {
            if (edge_shader == null)
            {
                edge_shader = GLShaderFilter.Create(Width, Height, new GLShader(
                    VertexHeader + @"
						out vec2 texcoord;
						out vec4 offset[3];
						out vec4 dummy2;
						void main()
						{
							texcoord = a_texcoords.xy;
							vec4 dummy1 = vec4(0);
							SMAAEdgeDetectionVS(dummy1, dummy2, texcoord, offset);
							gl_Position = a_position;
						}
					",
                    FragmentHeader + @"
						uniform sampler2D albedo_tex;
						uniform sampler2D depthTex;

						in vec2 texcoord;
						in vec4 offset[3];
						in vec4 dummy2;

						void main()
						{
							#if SMAA_PREDICATION == 1
								gl_FragColor = SMAAColorEdgeDetectionPS(texcoord, offset, albedo_tex, depthTex);
							#else
								gl_FragColor = SMAAColorEdgeDetectionPS(texcoord, offset, albedo_tex);
							#endif
						}
					"
                ));
            }

            return edge_shader.Process((Shader) =>
            {
                Shader.GetUniform("albedo_tex").Set(GLTextureUnit.CreateAtIndex(0).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Linear).SetTexture(albedo_tex));
                Shader.GetUniform("depthTex").NoWarning().Set(GLTextureUnit.CreateAtIndex(1).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Linear).SetTexture(depthTex));
            });
        }

        private GLTexture pass_blend(GLTexture edge_tex)
        {
            if (blend_shader == null)
            {
                blend_shader = GLShaderFilter.Create(Width, Height, new GLShader(
                    VertexHeader + @"
						out vec2 texcoord;
						out vec2 pixcoord;
						out vec4 offset[3];
						out vec4 dummy2;
						void main()
						{
							texcoord = a_texcoords.xy;
							vec4 dummy1 = vec4(0);
							SMAABlendingWeightCalculationVS(dummy1, dummy2, texcoord, pixcoord, offset);
							gl_Position = a_position;
						}
					",
                    FragmentHeader + @"
						uniform sampler2D edge_tex;
						uniform sampler2D area_tex;
						uniform sampler2D search_tex;
						in vec2 texcoord;
						in vec2 pixcoord;
						in vec4 offset[3];
						in vec4 dummy2;
						void main()
						{
							gl_FragColor = SMAABlendingWeightCalculationPS(texcoord, pixcoord, offset, edge_tex, area_tex, search_tex, ivec4(1, 1, 1, 0));
							//gl_FragColor = SMAABlendingWeightCalculationPS(texcoord, pixcoord, offset, edge_tex, area_tex, search_tex, ivec4(0));
						}
					"
                ));
            }

            return blend_shader.Process((Shader) =>
            {
                Shader.GetUniform("edge_tex").Set(GLTextureUnit.CreateAtIndex(0).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Linear).SetTexture(edge_tex));
                Shader.GetUniform("area_tex").Set(area_tex_unit.SetIndex(1));
                Shader.GetUniform("search_tex").Set(search_tex_unit.SetIndex(2));
            });
        }

        private GLTexture pass_neighborhood(GLTexture albedo_tex, GLTexture blend_tex)
        {
            if (neighborhood_shader == null)
            {
                neighborhood_shader = GLShaderFilter.Create(Width, Height, new GLShader(
                    VertexHeader + @"
						out vec2 texcoord;
						out vec4 offset[2];
						out vec4 dummy2;
						void main()
						{
							texcoord = a_texcoords.xy;
							vec4 dummy1 = vec4(0);
							SMAANeighborhoodBlendingVS(dummy1, dummy2, texcoord, offset);
							gl_Position = a_position;
						}
					",
                    FragmentHeader + @"
						uniform sampler2D albedo_tex;
						uniform sampler2D blend_tex;
						in vec2 texcoord;
						in vec4 offset[2];
						in vec4 dummy2;
						void main()
						{
							gl_FragColor = SMAANeighborhoodBlendingPS(texcoord, offset, albedo_tex, blend_tex);
						}
					"
                ));
            }
            return neighborhood_shader.Process((Shader) =>
            {
                Shader.GetUniform("albedo_tex").Set(GLTextureUnit.CreateAtIndex(0).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Linear).SetTexture(albedo_tex));
                Shader.GetUniform("blend_tex").Set(GLTextureUnit.CreateAtIndex(1).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Linear).SetTexture(blend_tex));
            });
        }

        public GLTexture Process(GLTexture InputColor, GLTexture InputDepth)
        {
            if (InputColor.Width == 0 || InputColor.Height == 0)
                throw (new Exception("Smaa can't handle empty textures"));
            //if (InputColor.Width != InputDepth.Width || InputColor.Height != InputDepth.Height) throw (new Exception("Color.Size != Texture.Size"));
            SetSize(InputColor.Width, InputColor.Height);

            var edge_tex = edge_pass(InputColor, InputDepth);
            var blend_tex = pass_blend(edge_tex);
            var neighborhood_tex = pass_neighborhood(InputColor, blend_tex);

            //return edge_tex;
            //return blend_tex;
            return neighborhood_tex;
        }

        public void Dispose()
        {
        }
    }
}