#version 460

layout (location = 0) in vec2 position;
layout (location = 1) in uint drawid;
out vec2 uv;
flat out uint drawID;

void main(void)
{
    gl_Position = vec4(position,0.0,1.0);
    drawID = gl_DrawID;
}