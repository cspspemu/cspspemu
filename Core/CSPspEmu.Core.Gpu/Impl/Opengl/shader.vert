uniform mat4 matrixWorldViewProjection;
uniform mat4 matrixBones[8];

attribute vec4 vertexTexture;
attribute vec4 vertexColor;
attribute vec3 vertexNormal;
attribute vec4 vertexPosition;
attribute vec4 vertexWeight_0_3;
attribute vec4 vertexWeight_4_7;

void main() {
	//gl_Position = matrixWorldViewProjection * vertexPosition;
	gl_Position = vertexPosition;
}