using ImGuiNET;
using ImGui.Unity.Data;

namespace ImGui.Unity.Input
{
    /// <summary>
    /// Input bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
    /// </summary>
    internal interface IInputSource
    {
        bool Initialize(ImGuiIOPtr io, UIOConfig config, string platformName);
        void PrepareFrame(ImGuiIOPtr io);
        void Shutdown(ImGuiIOPtr io);
    }
}