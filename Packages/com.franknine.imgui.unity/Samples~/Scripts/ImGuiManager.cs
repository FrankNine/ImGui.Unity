using UnityEngine;

using ImGui.Unity;

public class ImGuiManager : MonoBehaviour
{
    private void OnEnable()
    {
        DearImGuiRendererFeature.OnLayout += OnLayout;
    }

    private void OnDisable()
    {
        DearImGuiRendererFeature.OnLayout -= OnLayout;
    }

    private void OnLayout()
    {
        ImGuiNET.ImGui.ShowDemoWindow();
    }
}