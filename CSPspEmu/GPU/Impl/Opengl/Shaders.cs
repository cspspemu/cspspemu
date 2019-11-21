namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public class Shaders
    {
	    //language=c++
	    static public string ShaderFrag = @"
			//#ifndef GL_ES
			//#version 330
			//#endif

			#extension GL_EXT_gpu_shader4 : enable

			#define GU_TFX_MODULATE  0
			#define GU_TFX_DECAL     1
			#define GU_TFX_BLEND     2
			#define GU_TFX_REPLACE   3
			#define GU_TFX_ADD       4
								    
			#define GU_TCC_RGB       0
			#define GU_TCC_RGBA      1
								    
			#define GU_NEVER         0
			#define GU_ALWAYS        1
			#define GU_EQUAL         2
			#define GU_NOTEQUAL      3
			#define GU_LESS          4
			#define GU_LEQUAL        5
			#define GU_GREATER       6
			#define GU_GEQUAL        7

			#define GU_CLEAR         0
			#define GU_AND           1
			#define GU_AND_REVERSE   2
			#define GU_COPY          3
			#define GU_AND_INVERTED  4
			#define GU_NOOP          5
			#define GU_XOR           6
			#define GU_OR            7
			#define GU_NOR           8
			#define GU_EQUIV         9
			#define GU_INVERTED      10
			#define GU_OR_REVERSE    11
			#define GU_COPY_INVERTED 12
			#define GU_OR_INVERTED   13
			#define GU_NAND          14
			#define GU_SET           15

			uniform vec4 uniformColor;
			uniform vec4 TEC; // TextureEnviromentColor, Cc, sceGuTexEnvColor()

			uniform int tfx;
			uniform int tcc;

			uniform bool lopEnabled;
			uniform int lop;

			uniform bool hasPerVertexColor;
			uniform bool hasTexture;
			uniform bool clearingMode;

			uniform bool colorTest;

			// ALPHA TEST
			uniform bool alphaTest;
			uniform int alphaFunction;
			uniform int alphaValue;
			uniform int alphaMask;

			uniform sampler2D backtex;
			uniform sampler2D texture0;

			varying vec4 v_color;
			varying vec4 v_normal;
			varying vec2 v_texCoords;
			varying vec2 v_backtexCoords;

			ivec4 convertToByte(vec4 v) {
				return ivec4(v * 255.0);
			}

			vec4 convertToFloat(ivec4 v) {
				return vec4(v) / 255.0;
			}

			void main() {

				if (hasPerVertexColor) {
					gl_FragColor = v_color;
				} else {
					gl_FragColor = uniformColor;
				}

				if (!clearingMode && hasTexture) {
					vec4 texColor = texture2D(texture0, v_texCoords);

					if (alphaTest) {
						//int alphaInt = int(gl_FragColor.a * 255.0) & alphaMask;
						//int alphaInt = int(gl_FragColor.a * 255.0);
						int alphaInt = int(texColor.a * 255.0) & alphaMask;
						if (alphaFunction == GU_NEVER   ) { discard; }
						else if (alphaFunction == GU_EQUAL   ) { if (!(alphaInt == alphaValue)) { discard; return; } }
						else if (alphaFunction == GU_NOTEQUAL) { if (!(alphaInt != alphaValue)) { discard; return; } }
						else if (alphaFunction == GU_LESS    ) { if (!(alphaInt <  alphaValue)) { discard; return; } }
						else if (alphaFunction == GU_LEQUAL  ) { if (!(alphaInt <= alphaValue)) { discard; return; } }
						else if (alphaFunction == GU_GREATER ) { if (!(alphaInt >  alphaValue)) { discard; return; } }
						else if (alphaFunction == GU_GEQUAL  ) { if (!(alphaInt >= alphaValue)) { discard; return; } }
					}

					if (tfx == GU_TFX_MODULATE) {
						gl_FragColor.rgb = texColor.rgb * gl_FragColor.rgb;
						gl_FragColor.a = (tcc == GU_TCC_RGBA) ? (gl_FragColor.a * texColor.a) : texColor.a;
					} else if (tfx == GU_TFX_DECAL) {
						if (tcc == GU_TCC_RGB) {
							gl_FragColor.rgba = texColor.rgba;
						} else {
							gl_FragColor.rgb = texColor.rgb * gl_FragColor.rgb;
							gl_FragColor.a = texColor.a;
						}
					} else if (tfx == GU_TFX_BLEND) {
						gl_FragColor.rgba = mix(texColor, gl_FragColor, 0.5);
					} else if (tfx == GU_TFX_REPLACE) {
						gl_FragColor.rgb = texColor.rgb;
						gl_FragColor.a = (tcc == GU_TCC_RGB) ? gl_FragColor.a : texColor.a;
					} else if (tfx == GU_TFX_ADD) {
						gl_FragColor.rgb += texColor.rgb;
						gl_FragColor.a = (tcc == GU_TCC_RGB) ? gl_FragColor.a : (texColor.a * gl_FragColor.a);
					} else {
						gl_FragColor = vec4(1, 0, 1, 1);
					}
				}

				if (lopEnabled) {
					ivec4 s = convertToByte(gl_FragColor);
					ivec4 d = convertToByte(texture2D(backtex, v_backtexCoords));
					ivec4 o = ivec4(0x77);

					// http://www.opengl.org/sdk/docs/man/xhtml/glLogicOp.xml
					     if (lop == GU_CLEAR        ) o = ivec4(0x00);
					else if (lop == GU_AND          ) o = s & d;
					else if (lop == GU_AND_REVERSE  ) o = s & ~d;
					else if (lop == GU_COPY         ) o = s;
					else if (lop == GU_AND_INVERTED ) o = ~s & d;
					else if (lop == GU_NOOP         ) o = d;
					else if (lop == GU_XOR          ) o = s ^ d;
					else if (lop == GU_OR           ) o = s | d;
					else if (lop == GU_NOR          ) o = ~(s | d);
					else if (lop == GU_EQUIV        ) o = ~(s ^ d);
					else if (lop == GU_INVERTED     ) o = ~d;
					else if (lop == GU_OR_REVERSE   ) o = s | ~d;
					else if (lop == GU_COPY_INVERTED) o = ~s;
					else if (lop == GU_OR_INVERTED  ) o = ~s | d;
					else if (lop == GU_NAND         ) o = ~(s & d);
					else if (lop == GU_SET          ) o = ivec4(0xFF);

					gl_FragColor = convertToFloat(o);
				}

				//if (colorTest) {
				//	discard; return;
				//}
				//discard; return;
			}
        ";

	    //language=c++
	    static public string ShaderVert = @"
			//#ifndef GL_ES
			//#version 330
			//#endif

			uniform mat4 matrixWorldViewProjection;
			uniform mat4 matrixTexture;
			uniform mat4 matrixBones[8];
			uniform int weightCount;
			uniform bool hasReversedNormal;

			attribute vec4 vertexTexCoords;
			attribute vec4 vertexColor;
			attribute vec4 vertexNormal;
			attribute vec4 vertexPosition;
			attribute float vertexWeight0;
			attribute float vertexWeight1;
			attribute float vertexWeight2;
			attribute float vertexWeight3;
			attribute float vertexWeight4;
			attribute float vertexWeight5;
			attribute float vertexWeight6;
			attribute float vertexWeight7;


			//uniform bool hasPerVertexColor;

			varying vec4 v_color;
			varying vec2 v_texCoords;
			varying vec2 v_backtexCoords;
			varying vec4 v_normal;

			#define DO_WEIGHT(n) 

			vec4 performSkinning(vec4 In) {
				if (weightCount == 0) {
					return In;
				}

				vec4 Out = vec4(0.0, 0.0, 0.0, 0.0);
				
				float totalWeight = 0.0;
				if (weightCount > 0) { totalWeight += vertexWeight0;
				if (weightCount > 1) { totalWeight += vertexWeight1;
				if (weightCount > 2) { totalWeight += vertexWeight2;
				if (weightCount > 3) { totalWeight += vertexWeight3;
				if (weightCount > 4) { totalWeight += vertexWeight4;
				if (weightCount > 5) { totalWeight += vertexWeight5;
				if (weightCount > 6) { totalWeight += vertexWeight6;
				if (weightCount > 7) { totalWeight += vertexWeight7;
				}}}}}}}}

				if (weightCount > 0) { Out += (matrixBones[0] * (vertexWeight0 / totalWeight)) * In;
				if (weightCount > 1) { Out += (matrixBones[1] * (vertexWeight1 / totalWeight)) * In;
				if (weightCount > 2) { Out += (matrixBones[2] * (vertexWeight2 / totalWeight)) * In;
				if (weightCount > 3) { Out += (matrixBones[3] * (vertexWeight3 / totalWeight)) * In;
				if (weightCount > 4) { Out += (matrixBones[4] * (vertexWeight4 / totalWeight)) * In;
				if (weightCount > 5) { Out += (matrixBones[5] * (vertexWeight5 / totalWeight)) * In;
				if (weightCount > 6) { Out += (matrixBones[6] * (vertexWeight6 / totalWeight)) * In;
				if (weightCount > 7) { Out += (matrixBones[7] * (vertexWeight7 / totalWeight)) * In;
				}}}}}}}}

				return Out;
			}

			vec4 prepareNormal(vec4 normal) {
				return hasReversedNormal ? -normal : normal;
			}

			void main() {
				
				gl_Position = matrixWorldViewProjection * performSkinning(vertexPosition);
				v_backtexCoords = (gl_Position.xy + vec2(1.0, 1.0)) / 2.0;
				v_normal = performSkinning(prepareNormal(vertexNormal));
				v_color = vertexColor;
				v_texCoords = (matrixTexture * vertexTexCoords).xy;
			}
		";
    }
}