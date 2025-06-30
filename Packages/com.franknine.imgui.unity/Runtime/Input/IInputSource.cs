using ImGuiNET;

namespace ImGui.Unity.Input
{
    /// <summary>
    /// Input bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
    /// </summary>
    internal interface IInputSource
    {
        void Initialize(ImGuiIOPtr io);
        void PrepareFrame(ImGuiIOPtr io);
        void Shutdown(ImGuiIOPtr io);
    }
}