#define GU_TFX_MODULATE	0
#define GU_TFX_DECAL	1
#define GU_TFX_BLEND	2
#define GU_TFX_REPLACE	3
#define GU_TFX_ADD		4

#define GU_TCC_RGB		(0)
#define GU_TCC_RGBA		(1)

#define GU_NEVER    0
#define GU_ALWAYS   1
#define GU_EQUAL    2
#define GU_NOTEQUAL 3
#define GU_LESS     4
#define GU_LEQUAL   5
#define GU_GREATER  6
#define GU_GEQUAL   7

uniform vec4 uniformColor;
uniform vec4 TEC; // TextureEnviromentColor, Cc, sceGuTexEnvColor()

uniform int tfx;
uniform int tcc;

uniform bool hasPerVertexColor;
uniform bool hasTexture;
uniform bool clearingMode;

uniform bool colorTest;

// ALPHA TEST
uniform bool alphaTest;
uniform int alphaFunction;
uniform int alphaValue;
uniform int alphaMask;

uniform sampler2D texture0;

varying vec4 v_color;
varying vec4 v_normal;
varying vec2 v_texCoords;

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
			int alphaInt = int(texColor.a * 255.0);
			if (alphaMask == 0xFF) {
				switch (alphaFunction) {
					case GU_NEVER   : discard;
					case GU_EQUAL   : if (!(alphaInt == alphaValue)) { discard; return; } break;
					case GU_NOTEQUAL: if (!(alphaInt != alphaValue)) { discard; return; } break;
					case GU_LESS    : if (!(alphaInt <  alphaValue)) { discard; return; } break;
					case GU_LEQUAL  : if (!(alphaInt <= alphaValue)) { discard; return; } break;
					case GU_GREATER : if (!(alphaInt >  alphaValue)) { discard; return; } break;
					case GU_GEQUAL  : if (!(alphaInt >= alphaValue)) { discard; return; } break;
				}
			}
		}

		switch (tfx) {
			case GU_TFX_MODULATE:
				gl_FragColor.rgb = texColor.rgb * gl_FragColor.rgb;
				gl_FragColor.a = (tcc == GU_TCC_RGBA) ? (gl_FragColor.a * texColor.a) : texColor.a;
				break;
			case GU_TFX_DECAL:
				if (tcc == GU_TCC_RGB) {
					gl_FragColor.rgba = texColor.rgba;
				} else {
					gl_FragColor.rgb = texColor.rgb * gl_FragColor.rgb;
					gl_FragColor.a = texColor.a;
				}
				break;
			case GU_TFX_BLEND:
				gl_FragColor.rgba = mix(texColor, gl_FragColor, 0.5);
				break;
			case GU_TFX_REPLACE:
				gl_FragColor.rgb = texColor.rgb;
				gl_FragColor.a = (tcc == GU_TCC_RGB) ? gl_FragColor.a : texColor.a;
				break;
			case GU_TFX_ADD:
				gl_FragColor.rgb += texColor.rgb;
				gl_FragColor.a = (tcc == GU_TCC_RGB) ? gl_FragColor.a : (texColor.a * gl_FragColor.a);
				break;
			default:
				gl_FragColor = vec4(1, 0, 1, 1);
				break;
		}
	}

	//if (colorTest) {
	//	discard; return;
	//}
	//discard; return;
}