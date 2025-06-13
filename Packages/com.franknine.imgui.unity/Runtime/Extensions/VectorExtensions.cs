namespace ImGui.Unity.Extensions
{
    public static class VectorExtensions
    {
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v) => new(v.X, v.Y);
        public static System.Numerics.Vector2 ToSystem(this UnityEngine.Vector2 v) => new(v.x, v.y);
        
        public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 v) => new(v.X, v.Y, v.Z, v.W);
        public static System.Numerics.Vector4 ToSystem(this UnityEngine.Vector4 v) => new(v.x, v.y, v.y, v.z);
    }
}