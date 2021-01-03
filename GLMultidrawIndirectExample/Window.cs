﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GLMultidrawIndirectExample
{
    internal class Window : GameWindow
    {
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private void generateGeometry()
        {
            var gQuad = new[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.1f, 0.0f),
                new Vector2(0.0f, 0.1f),
                new Vector2(0.1f, 0.1f)
            };
            var gIndex = new uint[] { 0, 1, 2, 1, 3, 2 };

            var vVertex = new float[100 * 4 * 2];

            var index = 0;
            var xOffset = -0.95f;
            var yOffset = 0.85f;
            for (var i=0; i<10; i++)
            {
                for (var j=0; j<10; j++)
                {
                    for (var k=0; k<4; k++)
                    {
                        vVertex[index++] = gQuad[k].X + xOffset;
                        vVertex[index++] = gQuad[k].Y + yOffset;
                    }

                    xOffset += 0.2f;
                }

                yOffset -= 0.2f;
                xOffset = -0.95f;
            }


            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            //Create a vertex buffer object
            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vVertex.SizeInBytes(), vVertex, BufferUsageHint.StaticDraw);

            //Specify vertex attributes for the shader
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0); // Positions

            //Create an element buffer
            var ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, gIndex.SizeInBytes(), gIndex, BufferUsageHint.StaticDraw);

            //Generate draw commands
            var vDrawCommands = new DrawElementsIndirectData[100];
            for (uint i = 0; i < vDrawCommands.Length; i++)
                vDrawCommands[i] = new DrawElementsIndirectData(6, 1, 0, i * 4, 0);
            var indirectBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, indirectBuffer);
            GL.BufferData(BufferTarget.DrawIndirectBuffer, vDrawCommands.SizeInBytes(), vDrawCommands, BufferUsageHint.StaticDraw);
        }

        private void generateArrayTexture()
        {
            var arrayTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, arrayTexture);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, 1, 1, 100);

            var random = new Random();
            var color = new byte[4];
            for (var i=0; i<100; i++)
            {
                random.NextBytes(color);

                GL.TexSubImage3D(
                    TextureTarget.Texture2DArray,
                    0,
                    0, 0, i,
                    1, 1, 1,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    color
                );
            }
            var nearest = Enumerable.Repeat((int)TextureMinFilter.Nearest, 100).ToArray();
            var clapToEdge = Enumerable.Repeat((int)TextureWrapMode.ClampToEdge, 100).ToArray();
            GL.TexParameterI(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, nearest);
            GL.TexParameterI(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, nearest);
            GL.TexParameterI(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, clapToEdge);
            GL.TexParameterI(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, clapToEdge);
        }

        private int CompileShaders(string gVertexShaderSource, string gFragmentShaderSource)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, gVertexShaderSource);
            CompileShader(vertexShader);

            var fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, gFragmentShaderSource);
            CompileShader(fragShader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragShader);

            LinkProgram(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragShader);

            return program;
        }

        protected static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var error = GL.GetShaderInfoLog(shader);
                Console.WriteLine(error);
                throw new Exception($"Error occurred whilst compiling Shader({shader})");
            }
        }

        protected static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                var error = GL.GetProgramInfoLog(program);
                Console.WriteLine(error);
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        protected override void OnLoad()
        {
            GL.ClearColor(1, 1, 1, 0);

            var gProgram = CompileShaders(File.ReadAllText("vertex.glsl"), File.ReadAllText("fragment.glsl"));
            GL.UseProgram(gProgram);

            generateGeometry();
            generateArrayTexture();
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MultiDrawElementsIndirect(
                PrimitiveType.Triangles, 
                DrawElementsType.UnsignedInt, 
                (IntPtr)0, 
                100,
                0
            );

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
        }
    }
}