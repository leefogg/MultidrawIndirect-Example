using OpenTK.Graphics.OpenGL4;
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
        private int frameNumber;
        private const int numPositionOffsets = 4 * 100;
        private const int numColors = 4 * 100;
        private float[] positionsAndColours = new float[numPositionOffsets + numColors];
        private Random random = new Random();

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private void generateGeometry()
        {
            var quadVertcies = new[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.1f, 0.0f),
                new Vector2(0.0f, 0.1f),
                new Vector2(0.1f, 0.1f)
            };
            var indexBuffer = new uint[] { 0, 1, 2, 1, 3, 2 };

            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            //Create a vertex buffer object
            var vbo = GL.GenBuffer();
            var flattenedQuadVertcies = new float[]
            {
                quadVertcies[0].X,
                quadVertcies[0].Y,
                quadVertcies[1].X,
                quadVertcies[1].Y,
                quadVertcies[2].X,
                quadVertcies[2].Y,
                quadVertcies[3].X,
                quadVertcies[3].Y,
            };
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, flattenedQuadVertcies.SizeInBytes(), flattenedQuadVertcies, BufferUsageHint.StaticDraw);

            //Specify vertex attributes for the shader
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0); // Positions

            //Create an element buffer
            var ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.SizeInBytes(), indexBuffer, BufferUsageHint.StaticDraw);

            //Generate draw commands
            var drawCommand = new DrawElementsIndirectData(6, 1, 0, 0, 0);
            var drawCommands = Enumerable.Repeat(drawCommand, 100).ToArray(); // Draw the same quad 100 times
            var indirectBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, indirectBuffer);
            GL.BufferData(BufferTarget.DrawIndirectBuffer, drawCommands.SizeInBytes(), drawCommands, BufferUsageHint.StaticDraw);
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
            setupShader();
            generateGeometry();

            GL.ClearColor(1, 1, 1, 0);
        }

        private void setupShader()
        {
            var program = CompileShaders(File.ReadAllText("vertex.glsl"), File.ReadAllText("fragment.glsl"));
            GL.UseProgram(program);

            // Create and bind UBO
            var uniformBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, uniformBuffer);
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, uniformBuffer, (IntPtr)0, positionsAndColours.SizeInBytes());

            // Set PositionOffsets in UBO
            var index = 0;
            var xOffset = -0.95f;
            var yOffset = 0.85f;
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    positionsAndColours[index++] = xOffset;
                    positionsAndColours[index++] = yOffset;
                    // UBO's have to be vec4 aligned so add 2 padding floats
                    positionsAndColours[index++] = 0;
                    positionsAndColours[index++] = 0;

                    xOffset += 0.2f;
                }

                yOffset -= 0.2f;
                xOffset = -0.95f;
            }

            updateColors();
        }

        private void updateColors()
        {
            for (var i = 0; i < numColors; i++)
                positionsAndColours[numPositionOffsets + i] = (float)random.NextDouble();
            // Update the whole UBO for code simplicity, but should only update the colours.
            GL.BufferData(BufferTarget.UniformBuffer, positionsAndColours.SizeInBytes(), positionsAndColours, BufferUsageHint.StaticDraw);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Update colors (optional)
            if (frameNumber % 60 == 0)
                updateColors();

            GL.MultiDrawElementsIndirect(
                PrimitiveType.Triangles, 
                DrawElementsType.UnsignedInt, 
                (IntPtr)0, 
                100,
                0
            );

            SwapBuffers();
            frameNumber++;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
        }
    }
}
