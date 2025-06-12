using UnityEngine;

namespace ImGui.Unity.Utilities
{
    public static class UnityUtilities
    {
        public static void Destroy(Object obj)
        {
            if (Application.isEditor)
            {
                Object.DestroyImmediate(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }
    }
}