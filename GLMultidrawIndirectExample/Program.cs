using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace GLMultidrawIndirectExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = new GameWindowSettings();
            var nativeWindowSettings = new NativeWindowSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.Debug,
                Size = new Vector2i(1920, 1080),
                IsEventDriven = false,
                Title = "MultidrawIndirect Example"
            };
            using var window = new Window(gameWindowSettings, nativeWindowSettings);
            window.Run();
        }
    }
}
