using System;

using ImGuiNET;
using ImGui.Unity.Extensions;

namespace ImGui.Unity.Input
{
    internal class InputSourceBase : IInputSource
    {
        private readonly InputCallbacks _callbacks = new();

        public virtual void Initialize(ImGuiIOPtr io)
        {
            var platformIo = ImGuiNET.ImGui.GetPlatformIO();

            io.SetBackendPlatformName("Unity Input System");
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;

            if (io.ConfigNavMoveSetMousePos)
            {
                io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
                io.WantSetMousePos = true;
            }
            else
            {
                io.BackendFlags &= ~ImGuiBackendFlags.HasSetMousePos;
                io.WantSetMousePos = false;
            }

            unsafe
            {
                InputCallbacks.SetClipboardFunctions
                (
                    InputCallbacks.GetClipboardTextCallback,
                    InputCallbacks.SetClipboardTextCallback
                );
            }

            _callbacks.Assign(io);
            platformIo.Platform_ClipboardUserData = IntPtr.Zero;
        }

        public virtual void PrepareFrame(ImGuiIOPtr io)
        {
            throw new NotImplementedException();
        }

        public virtual void Shutdown(ImGuiIOPtr io)
        {
            io.SetBackendPlatformName(null);

            _callbacks.Unset(io);
        }
    }
}