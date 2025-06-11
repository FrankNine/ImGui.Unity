using Vector4 = System.Numerics.Vector4;

using UnityEngine;

namespace ImGui.Unity.Extensions
{
    public static class ColorExtensions
    {
        public static Vector4 ToVector4(this Color c) => new(c.r, c.g, c.b, c.a);
        public static Color ToColor(this Vector4 v) => new(v.X, v.Y, v.Z, v.W);
    }
}