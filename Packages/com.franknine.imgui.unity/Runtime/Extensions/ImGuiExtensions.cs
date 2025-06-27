using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ImGuiNET;
using ImGui.Unity.Utilities;

namespace ImGui.Unity.Extensions
{
    public static unsafe class ImGuiExtensions
    {
        private static readonly HashSet<IntPtr> _managedAllocations = new();

        internal static void SetBackendPlatformName(this ImGuiIOPtr io, string name)
        {
            if (io.NativePtr->BackendPlatformName != (byte*)0)
            {
                if (_managedAllocations.Contains((IntPtr)io.NativePtr->BackendPlatformName))
                {
                    Marshal.FreeHGlobal(new IntPtr(io.NativePtr->BackendPlatformName));
                }

                io.NativePtr->BackendPlatformName = (byte*)0;
            }

            if (name != null)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* nativeName = (byte*)Marshal.AllocHGlobal(byteCount + 1);
                int offset = ImGuiUtilities.GetUtf8(name, nativeName, byteCount);

                nativeName[offset] = 0;

                io.NativePtr->BackendPlatformName = nativeName;
                _managedAllocations.Add((IntPtr)nativeName);
            }
        }

        internal static void SetIniFilename(this ImGuiIOPtr io, string name)
        {
            if (io.NativePtr->IniFilename != (byte*)0)
            {
                if (_managedAllocations.Contains((IntPtr)io.NativePtr->IniFilename))
                {
                    Marshal.FreeHGlobal((IntPtr)io.NativePtr->IniFilename);
                }

                io.NativePtr->IniFilename = (byte*)0;
            }

            if (name != null)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* nativeName = (byte*)Marshal.AllocHGlobal(byteCount + 1);
                int offset = ImGuiUtilities.GetUtf8(name, nativeName, byteCount);

                nativeName[offset] = 0;

                io.NativePtr->IniFilename = nativeName;
                _managedAllocations.Add((IntPtr)nativeName);
            }
        }
    }
}