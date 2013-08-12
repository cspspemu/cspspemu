#define GU_TFX_MODULATE	0
#define GU_TFX_DECAL	1
#define GU_TFX_BLEND	2
#define GU_TFX_REPLACE	3
#define GU_TFX_ADD		4

#define GU_TCC_RGB		(0)
#define GU_TCC_RGBA		(1)

uniform vec4 uniformColor;
uniform vec4 TEC; // TextureEnviromentColor, Cc, sceGuTexEnvColor()

uniform int tfx;
uniform int tcc;

uniform bool hasPerVertexColor;
uniform bool hasTexture;
uniform bool clearingMode;

uniform sampler2D texture0;

varying vec4 v_color;
varying vec2 v_texCoords;

void main() {
	if (hasPerVertexColor) {
		gl_FragColor = v_color;
	} else {
		gl_FragColor = uniformColor;
	}

	if (!clearingMode && hasTexture) {
		vec4 texColor = texture2D(texture0, v_texCoords);
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

	//discard; return;
}