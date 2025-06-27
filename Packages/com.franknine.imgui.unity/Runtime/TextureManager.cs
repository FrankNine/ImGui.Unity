using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using ImGui.Unity.Utilities;

namespace ImGui.Unity.Texture
{
    internal class TextureManager
    {
        private readonly Dictionary<IntPtr, Texture2D> _textures = new();

        public IntPtr Create(int width, int height, IntPtr sourcePixels)
        {
            var texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            var nativeTexture2DPtr = texture2D.GetNativeTexturePtr();
            _textures.Add(nativeTexture2DPtr, texture2D);
            
            var size = width * height * 4;
            unsafe
            {
                NativeArray<byte> srcData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>
                (
                    sourcePixels.ToPointer(),
                    size,
                    Allocator.None
                );
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref srcData, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
                // Invert y while copying the atlas texture.
                NativeArray<byte> dstData = texture2D.GetRawTextureData<byte>();
                int stride = width * 4;
                for (int y = 0; y < height; ++y)
                {
                    NativeArray<byte>.Copy(srcData, y * stride, dstData, (height - y - 1) * stride, stride);
                }
            } 
            texture2D.Apply();

            return nativeTexture2DPtr;
        }

        public bool TryGetTexture(IntPtr nativeTexture2DPtr, out Texture2D texture2D)
            => _textures.TryGetValue(nativeTexture2DPtr, out texture2D);

        public void Destroy(IntPtr nativeTexture2DPtr)
        {
            if (_textures.TryGetValue(nativeTexture2DPtr, out var texture2D))
            {
                UnityUtilities.Destroy(texture2D);
                _textures.Remove(nativeTexture2DPtr);
            } 
        }

        public void Shutdown()
        {
            foreach (var texture in _textures)
            {
                UnityUtilities.Destroy(texture.Value);
            }
            _textures.Clear();
        }
    }
}