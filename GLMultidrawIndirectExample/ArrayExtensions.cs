using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GLMultidrawIndirectExample
{
    public static class ArrayExtensions
    {
        public static int SizeInBytes<T>(this T[] self) => Marshal.SizeOf<T>() * self.Length;
    }
}
