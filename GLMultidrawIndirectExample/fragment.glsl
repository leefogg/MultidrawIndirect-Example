#version 430 core

out vec4 color;
in vec2 uv;
flat in uint drawID;
layout (binding = 0) uniform sampler2DArray textureArray;

void main(void)
{
	color = texture(textureArray, vec3(uv.x,uv.y,drawID) );
}