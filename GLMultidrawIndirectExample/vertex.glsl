#version 460

layout (location = 0) in vec2 position;
layout (location = 1) in uint drawid;
out vec2 uv;
flat out uint drawID;

layout (std140, binding = 0) uniform InstanceData {
    vec2 PositionOffsets[100];
    vec4 Colors[100];
};

void main(void)
{
    drawID = gl_DrawID;
    gl_Position = vec4(position + PositionOffsets[drawID], 0.0, 1.0);
}