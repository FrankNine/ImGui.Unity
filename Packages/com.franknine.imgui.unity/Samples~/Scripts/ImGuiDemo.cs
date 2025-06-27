using UnityEngine;

using ImGui.Unity;

public class ImGuiDemo : MonoBehaviour
{
    private void OnEnable()
        => DearImGuiRendererFeature.OnLayout += OnLayout;

    private void OnDisable()
        => DearImGuiRendererFeature.OnLayout -= OnLayout;

    private static float _fontSize = 1.0f;
    private static void OnLayout()
    {
        ImGuiNET.ImGui.Begin("Font size");
        ImGuiNET.ImGui.SliderFloat("Size", ref _fontSize, 0.1f, 2.0f);
        ImGuiNET.ImGui.End();

        ImGuiNET.ImGui.GetStyle().FontScaleDpi = _fontSize;;
        
        ImGuiNET.ImGui.ShowDemoWindow();
    }
}