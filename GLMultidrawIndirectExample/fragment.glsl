#version 430 core

out vec4 color;
in vec2 uv;
flat in uint drawID;

layout (std140, binding = 0) uniform InstanceData {
	vec2 PositionOffsets[100];
	vec4 Colors[100];
};

void main(void)
{
	color = Colors[drawID];
}