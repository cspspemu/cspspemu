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
	v_normal = performSkinning(prepareNormal(vertexNormal));
	v_color = vertexColor;
	v_texCoords = (matrixTexture * vertexTexCoords).xy;
}