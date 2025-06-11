using UnityEngine;
using UTexture = UnityEngine.Texture;

namespace ImGui.Unity.Data
{
    internal sealed class SpriteInfo
    {
        public UTexture Texture;
        public Vector2 Size;
        public Vector2 UV0;
        public Vector2 UV1;
    }
}