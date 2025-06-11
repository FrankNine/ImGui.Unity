using UnityEngine.Events;

using ImGuiNET;

namespace ImGui.Unity.Events
{
    [System.Serializable]
    public class FontInitializerEvent : UnityEvent<ImGuiIOPtr> { }
}