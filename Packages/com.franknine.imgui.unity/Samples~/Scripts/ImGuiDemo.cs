using UnityEngine;

using ImGui.Unity;

public class ImGuiDemo : MonoBehaviour
{
    private void OnEnable()
        => DearImGuiRendererFeature.OnLayout += OnLayout;

    private void OnDisable()
        => DearImGuiRendererFeature.OnLayout -= OnLayout;

    private static void OnLayout()
        => ImGuiNET.ImGui.ShowDemoWindow();
}