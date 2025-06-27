using System;
using System.Runtime.InteropServices;

namespace ImGui.Unity.FreeType
{
    internal static class ImFreetypeNative
    {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBuilderForFreeType();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAllocatorFunctions(IntPtr alloc_func, IntPtr free_func, IntPtr user_data);
    }
}