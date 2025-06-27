using System;

using UnityEngine;
using UnityEngine.Assertions;

using ImGuiNET;
using ImGui.Unity.Assets;
using ImGui.Unity.Data;
using ImGui.Unity.Extensions;

namespace ImGui.Unity.Input
{
    internal class InputSourceBase : IInputSource
    {
        private readonly IniSettingsAsset _iniSettings;
        private readonly CursorShapesAsset _cursorShapes;

        private readonly InputCallbacks _callbacks = new();

        private ImGuiMouseCursor _lastCursor = ImGuiMouseCursor.COUNT;

        internal InputSourceBase(CursorShapesAsset cursorShapes, IniSettingsAsset iniSettings)
        {
            _cursorShapes = cursorShapes;
            _iniSettings = iniSettings;
        }

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

            if (_iniSettings)
            {
                io.SetIniFilename(null);
                ImGuiNET.ImGui.LoadIniSettingsFromMemory(_iniSettings.Load());
            }
        }

        public virtual void PrepareFrame(ImGuiIOPtr io)
        {
            if (_iniSettings && io.WantSaveIniSettings)
            {
                _iniSettings.Save(ImGuiNET.ImGui.SaveIniSettingsToMemory());
                io.WantSaveIniSettings = false;
            }
        }
        
        protected void UpdateCursor(ImGuiIOPtr io, ImGuiMouseCursor cursor)
        {
            if (io.MouseDrawCursor)
            {
                cursor = ImGuiMouseCursor.None;
            }

            if (_lastCursor == cursor) return;
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0) return;

            _lastCursor = cursor;
            // Hide cursor if ImGui is drawing it or if it wants no cursor.
            Cursor.visible = cursor != ImGuiMouseCursor.None; 
            if (_cursorShapes)
            {
                Cursor.SetCursor(_cursorShapes[cursor].Texture, _cursorShapes[cursor].Hotspot, CursorMode.Auto);
            }
        }
        
        public virtual void Shutdown(ImGuiIOPtr io)
        {
            io.SetBackendPlatformName(null);

            _callbacks.Unset(io);
        }
    }
}