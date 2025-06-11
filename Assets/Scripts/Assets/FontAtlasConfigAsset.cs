using UnityEngine;

using ImGui.Unity.Data.Font;

namespace ImGui.Unity.Assets
{
    // TODO: Make a default resource file as sample.
    [CreateAssetMenu(menuName = "Dear ImGui/Font Atlas Configuration")]
    internal sealed class FontAtlasConfigAsset : ScriptableObject
    {
        public uint RasterizerFlags;
        public FontDefinition[] Fonts;
    }
}