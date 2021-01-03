#version 430 core

out vec3 color;
in vec2 uv;
flat in uint drawID;

layout (std140, binding = 0) uniform ObjectColours {
	vec3 Colors[100];
};

void main(void)
{
	color = Colors[drawID];
}