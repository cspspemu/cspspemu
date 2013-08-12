uniform mat4 matrixWorldViewProjection;
uniform mat4 matrixTexture;
uniform mat4 matrixBones[8];

attribute vec4 vertexTexCoords;
attribute vec4 vertexColor;
attribute vec3 vertexNormal;
attribute vec4 vertexPosition;
attribute vec4 vertexWeight_0_3;
attribute vec4 vertexWeight_4_7;

//uniform bool hasPerVertexColor;

varying vec4 v_color;
varying vec2 v_texCoords;
varying vec3 v_normal;

void main() {
	gl_Position = matrixWorldViewProjection * vertexPosition;
	v_color = vertexColor;
	v_texCoords = (matrixTexture * vertexTexCoords).xy;
	v_normal = vertexNormal;
}