using System;
using System.Text;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace ImGui.Unity.Utilities
{
    internal static unsafe class ImGuiUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 ScreenToImGui(in Vector2 point)
            => new(point.x, ImGuiNET.ImGui.GetIO().DisplaySize.Y - point.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 ImGuiToScreen(in Vector2 point)
            => new(point.x, ImGuiNET.ImGui.GetIO().DisplaySize.Y - point.y);

        internal static string StringFromPtr(byte* ptr)
        {
            int characters = 0;
            while (ptr[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(ptr, characters);
        }

        internal static int GetUtf8(string text, byte* utf8Bytes, int utf8ByteCount)
        {
            fixed (char* utf16Ptr = text)
            {
                return Encoding.UTF8.GetBytes(utf16Ptr, text.Length, utf8Bytes, utf8ByteCount);
            }
        }

        internal static int GetUtf8(string text, int start, int length, byte* utf8Bytes, int utf8ByteCount)
        {
            if (start < 0 || length < 0 || start + length > text.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            fixed (char* utf16Ptr = text)
            {
                return Encoding.UTF8.GetBytes(utf16Ptr + start, length, utf8Bytes, utf8ByteCount);
            }
        }
    }
}