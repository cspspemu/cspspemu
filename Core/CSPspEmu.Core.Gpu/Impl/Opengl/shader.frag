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

#define GU_CLEAR		(0)
#define GU_AND			(1)
#define GU_AND_REVERSE		(2)
#define GU_COPY			(3)
#define GU_AND_INVERTED		(4)
#define GU_NOOP			(5)
#define GU_XOR			(6)
#define GU_OR			(7)
#define GU_NOR			(8)
#define GU_EQUIV		(9)
#define GU_INVERTED		(10)
#define GU_OR_REVERSE		(11)
#define GU_COPY_INVERTED	(12)
#define GU_OR_INVERTED		(13)
#define GU_NAND			(14)
#define GU_SET			(15)

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
			int alphaInt = int(texColor.a * 255.0);
			if (alphaMask == 0xFF) {
				if (alphaFunction == GU_NEVER   ) { discard; }
				else if (alphaFunction == GU_EQUAL   ) { if (!(alphaInt == alphaValue)) { discard; return; } }
				else if (alphaFunction == GU_NOTEQUAL) { if (!(alphaInt != alphaValue)) { discard; return; } }
				else if (alphaFunction == GU_LESS    ) { if (!(alphaInt <  alphaValue)) { discard; return; } }
				else if (alphaFunction == GU_LEQUAL  ) { if (!(alphaInt <= alphaValue)) { discard; return; } }
				else if (alphaFunction == GU_GREATER ) { if (!(alphaInt >  alphaValue)) { discard; return; } }
				else if (alphaFunction == GU_GEQUAL  ) { if (!(alphaInt >= alphaValue)) { discard; return; } }
			}
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
		ivec4 o;

		// http://www.opengl.org/sdk/docs/man/xhtml/glLogicOp.xml
		if (lop == GU_CLEAR) {                           // GL_CLEAR	 0
			o = ivec4(0, 0, 0, 0);
		} else if (lop == GU_AND) {                      // GL_AND	 s & d
			o = s & d;
		} else if (lop == GU_AND_REVERSE) {              // GL_AND_REVERSE	 s & ~d
			o = s & ~d;
		} else if (lop == GU_COPY) {                     // GL_COPY	 s
			o = s;
		} else if (lop == GU_AND_INVERTED) {             // GL_AND_INVERTED	 ~s & d
			o = ~s & d;
		} else if (lop == GU_NOOP) {                     // GL_NOOP	 d
			o = d;
		} else if (lop == GU_XOR) {                      // GL_XOR	 s ^ d
			o = s ^ d;
		} else if (lop == GU_OR) {                       // GL_OR	 s | d
			o = s | d;
		} else if (lop == GU_NOR) {                      // GL_NOR	 ~(s | d)
			o = ~(s | d);
		} else if (lop == GU_EQUIV) {                    // GL_EQUIV	 ~(s ^ d)
			o = ~(s ^ d);
		} else if (lop == GU_INVERTED) {                 // GL_INVERT	 ~d
			o = ~d;
		} else if (lop == GU_OR_REVERSE) {               // GL_OR_REVERSE	 s | ~d
			o = s | ~d;
		} else if (lop == GU_COPY_INVERTED) {            // GL_COPY_INVERTED	 ~s
			o = ~s;
		} else if (lop == GU_OR_INVERTED) {              // GL_OR_INVERTED	 ~s | d
			o = ~s | d;
		} else if (lop == GU_NAND) {                     // GL_NAND	 ~(s & d)
			o = ~(s & d);
		} else if (lop == GU_SET) {                      // GL_SET	 1
			o = ivec4(0xFF, 0xFF, 0xFF, 0xFF);
		}

		gl_FragColor = convertToFloat(o);
	}

	//if (colorTest) {
	//	discard; return;
	//}
	//discard; return;
}