using System;
using System.IO;
using Vector2 = System.Numerics.Vector2;

using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using ImGuiNET;
using ImGui.Unity.Assets;
using ImGui.Unity.Data;
using ImGui.Unity.Events;
using ImGui.Unity.Texture;
using ImGui.Unity.Extensions;
using ImGui.Unity.Input;

namespace ImGui.Unity
{
    public class DearImGuiRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material _material;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [SerializeField] private FontInitializerEvent _fontCustomInitializer = new();
        [SerializeField] private FontAtlasConfigAsset _fontAtlasConfiguration;
        [SerializeField] private UIOConfig _initialConfiguration = new()
        {
            ImGuiConfig = ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable,

            DoubleClickTime = 0.30f,
            DoubleClickMaxDist = 6.0f,

            DragThreshold = 6.0f,

            KeyRepeatDelay = 0.250f,
            KeyRepeatRate = 0.050f,

            FontGlobalScale = 1.0f,
            FontAllowUserScaling = false,

            DisplayFramebufferScale = Vector2.One,

            MouseDrawCursor = false,
            TextCursorBlink = false,

            ResizeFromEdges = true,
            MoveFromTitleOnly = true,
            ConfigMemoryCompactTimer = 1f,
        };
        [Header("Customization")]
        [SerializeField] private StyleAsset _style;
        
        [Header("Custom Shader Properties")]
        [SerializeField] private string textureProperty = "_Texture";
        
        [SerializeField] private CursorShapesAsset _cursorShapes;
        [Tooltip("Null value uses default imgui.ini file.")]
        [SerializeField] private IniSettingsAsset _iniSettings;

        [SerializeField] private InputSourceType _inputSourceType;
        

        private DearImGuiPass _dearImGuiPass;
        
        private Mesh _mesh;
        private int _prevSubMeshCount = 1;

        private IntPtr _imGuiContext;
        private TextureManager _textureManager;
        private IInputSource _inputSource;
        
        // Skip all checks and validation when updating the mesh.
        private const MeshUpdateFlags NoMeshChecks = MeshUpdateFlags.DontNotifyMeshUsers |
                                                     MeshUpdateFlags.DontRecalculateBounds |
                                                     MeshUpdateFlags.DontResetBoneBounds |
                                                     MeshUpdateFlags.DontValidateIndices;

        // Color sent with TexCoord1 semantics because otherwise Color attribute would be reordered to come before UVs.
        private static readonly VertexAttributeDescriptor[] _vertexAttributes = {
            new(VertexAttribute.Position , VertexAttributeFormat.Float32, 2), // Position.
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2), // UV.
            new(VertexAttribute.TexCoord1, VertexAttributeFormat.UInt32 , 1), // Color.
        };

        public static event Action OnLayout;
        
        public override void Create()
        {
            if (!PlayerLoop.GetCurrentPlayerLoop()
                           .HasPlayerLoopSystem(typeof(UnityEngine.PlayerLoop.Update), _ImGuiUpdateLoop))
            {
                PlayerLoop.GetCurrentPlayerLoop()
                          .AppendToPlayerLoop(typeof(UnityEngine.PlayerLoop.Update), _ImGuiUpdateLoop);
            }

            if (!_mesh)
            {
                _mesh = new Mesh
                {
                    name = "DearImGui Mesh"
                };
                _mesh.MarkDynamic();
            }

            if (_imGuiContext == IntPtr.Zero)
            {
                _imGuiContext = ImGuiNET.ImGui.CreateContext();
                
                ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
                // Disable writing ini file
                unsafe { io.NativePtr->IniFilename = null; }

                // Supports ImDrawCmd::VtxOffset to output large meshes while still using 16-bits indices.
                io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

                // C++ exceptions will crash the editor and may cause user to lose unsaved changes.
                // Try to have some elegant recovery so that things don't just break.
                io.ConfigErrorRecovery = true;
                
                io.DisplaySize = new Vector2(Screen.width, Screen.height);
                
                _textureManager = new TextureManager();
                _textureManager.BuildFontAtlas(io, _fontAtlasConfiguration, _fontCustomInitializer);
                _textureManager.Initialize(io);
                
                _initialConfiguration.ApplyTo(io);
                _style?.ApplyTo(ImGuiNET.ImGui.GetStyle());

                IInputSource inputSource = InputUtility.Create(_inputSourceType, _cursorShapes, _iniSettings);
                _inputSource?.Shutdown(io);
                _inputSource = inputSource;
                _inputSource?.Initialize(io, _initialConfiguration, "Unity " + _inputSource);
            }

            _dearImGuiPass = new DearImGuiPass
            {
                Material = _material,
                MaterialPropertyBlock = new MaterialPropertyBlock(),
                TextureManager = _textureManager,
                TextureID = Shader.PropertyToID(textureProperty),
                Mesh = _mesh,
                
                renderPassEvent = _renderPassEvent
            };
        }

        // https://docs.unity3d.com/6000.1/Documentation/Manual/urp/renderer-features/create-custom-renderer-feature.html
        // AddRenderPasses: Unity calls this method every frame, once for each camera.
        // This method lets you inject ScriptableRenderPass instances into the scriptable Renderer.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!_material) return;
            if (_imGuiContext == IntPtr.Zero) return; 
            
            ImDrawDataPtr drawDataPtr = ImGuiNET.ImGui.GetDrawData();
            unsafe
            {
                if(drawDataPtr.NativePtr == null || !drawDataPtr.Valid) return; 
            }
            Vector2 fbOSize = drawDataPtr.DisplaySize * drawDataPtr.FramebufferScale;
            // Avoid rendering when minimized.
            if (fbOSize.X <= 0f || fbOSize.Y <= 0f || drawDataPtr.TotalVtxCount == 0) return;
            
            _UpdateMesh(drawDataPtr, _mesh);
            
            _dearImGuiPass.DrawDataPtr = drawDataPtr;
            renderer.EnqueuePass(_dearImGuiPass);
        }

        private void _ImGuiUpdateLoop()
        {
            if (_imGuiContext == IntPtr.Zero) return;
            ImGuiNET.ImGui.SetCurrentContext(_imGuiContext);
            
            ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            
            _textureManager.PrepareFrame(io);
            _inputSource.PrepareFrame(io);
            
            // Time.unscaledDeltaTime can be 0 in rare occasions. For example, when using the Frame Debugger.
            io.DeltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
            io.DisplaySize = new Vector2(Screen.width, Screen.height);

            ImGuiNET.ImGui.NewFrame();
            
            try
            {
                OnLayout?.Invoke();

                // C++ asserts can't be caught in C# and will crash the editor. 
                io.ConfigErrorRecoveryEnableAssert = false;

                // Only try rendering if no errors happen in OnLayout.
                // If an exception is thrown there, it's very likely the program is in an invalid state.
                ImGuiNET.ImGui.Render();

                // Turn off asserts only for user code.
                // For EchoImGui's code, fail fast so it can be fixed.
                io.ConfigErrorRecoveryEnableAssert = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                // New frame has a sanity check that will bring up a popup in Unity and force the user to restart without saving.
                // Just stopping the Render call isn't enough.
                // Should destroy the context to start fresh and to allow user to save data.
                Debug.LogError("Exception thrown in layout!");
            }
        }
        
        private void _UpdateMesh(ImDrawDataPtr drawDataPtr, Mesh mesh) 
        {
            int subMeshCount = 0;
            for (int n = 0; n < drawDataPtr.CmdListsCount; n++)
            {
                subMeshCount += drawDataPtr.CmdLists[n].CmdBuffer.Size;
            }
            
            if (_prevSubMeshCount != subMeshCount)
            {
                // Occasionally crashes when changing subMeshCount without clearing first.
                mesh.Clear(true);
                mesh.subMeshCount = _prevSubMeshCount = subMeshCount;
            }
            
            mesh.SetVertexBufferParams(drawDataPtr.TotalVtxCount, _vertexAttributes);
            mesh.SetIndexBufferParams(drawDataPtr.TotalIdxCount, IndexFormat.UInt16);
            
            //  Upload data into mesh.
            int vtxOf = 0;
            int idxOf = 0;

            int subMeshDescriptorSize = 0;
            for (int n = 0; n < drawDataPtr.CmdListsCount; n++)
            {
                ImDrawListPtr drawList = drawDataPtr.CmdLists[n];
                subMeshDescriptorSize += drawList.CmdBuffer.Size; 
            }
            var subMeshDescriptorArray = new NativeArray<SubMeshDescriptor>(subMeshDescriptorSize, Allocator.Temp);
            int subMeshDescriptorIndex = 0;
            for (int n = 0; n < drawDataPtr.CmdListsCount; n++)
            {
                ImDrawListPtr drawList = drawDataPtr.CmdLists[n];

                unsafe
                {
                    // TODO: Convert NativeArray to C# array or list (remove collections).
                    NativeArray<ImDrawVert> vtxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ImDrawVert>(
                        (void*)drawList.VtxBuffer.Data, drawList.VtxBuffer.Size, Allocator.None);
                    NativeArray<ushort> idxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ushort>(
                        (void*)drawList.IdxBuffer.Data, drawList.IdxBuffer.Size, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility
                        .SetAtomicSafetyHandle(ref vtxArray, AtomicSafetyHandle.GetTempMemoryHandle());
                    NativeArrayUnsafeUtility
                        .SetAtomicSafetyHandle(ref idxArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
                    // Upload vertex/index data.
                    mesh.SetVertexBufferData(vtxArray, 0, vtxOf, vtxArray.Length, 0, NoMeshChecks);
                    mesh.SetIndexBufferData(idxArray, 0, idxOf, idxArray.Length, NoMeshChecks);

                    // Define subMeshes.
                    for (int i = 0, iMax = drawList.CmdBuffer.Size; i < iMax; ++i)
                    {
                        ImDrawCmdPtr cmd = drawList.CmdBuffer[i];
                        SubMeshDescriptor descriptor = new SubMeshDescriptor
                        {
                            topology = MeshTopology.Triangles,
                            indexStart = idxOf + (int)cmd.IdxOffset,
                            indexCount = (int)cmd.ElemCount,
                            baseVertex = vtxOf + (int)cmd.VtxOffset,
                        };
                        subMeshDescriptorArray[subMeshDescriptorIndex] = descriptor;
                        subMeshDescriptorIndex++;
                    }

                    vtxOf += vtxArray.Length;
                    idxOf += idxArray.Length;
                }
            }
            
            mesh.SetSubMeshes(subMeshDescriptorArray, NoMeshChecks);
            mesh.UploadMeshData(false);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (PlayerLoop.GetCurrentPlayerLoop()
                          .HasPlayerLoopSystem(typeof(UnityEngine.PlayerLoop.Update), _ImGuiUpdateLoop))
            {
                PlayerLoop.GetCurrentPlayerLoop()
                          .RemovePlayerLoopSystem(typeof(UnityEngine.PlayerLoop.Update), _ImGuiUpdateLoop);
            }

            _inputSource?.Shutdown(ImGuiNET.ImGui.GetIO());
            _textureManager?.Shutdown();

            if (_imGuiContext != IntPtr.Zero)
            {
                ImGuiNET.ImGui.DestroyContext();
                _imGuiContext = IntPtr.Zero;
            }

            CoreUtils.Destroy(_mesh);
            _mesh = null;
        }
        
#if UNITY_EDITOR
        private const string PACKAGE_PATH = "Packages/com.franknine.imgui.unity/";
        private void Reset()
        {
            if (!_material)
            {
                var materialPath = Path.Combine(PACKAGE_PATH, "Runtime/Resources/Materials/DearImGui-Mesh.mat");
                _material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                UnityEditor.EditorUtility.SetDirty(this);
            }
            
            if (!_fontAtlasConfiguration)
            {
                var fontAtlasConfigAssetPath 
                    = Path.Combine(PACKAGE_PATH, "Runtime/DefaultSettings/Default Font Atlas Config Asset.asset");
                _fontAtlasConfiguration 
                    = UnityEditor.AssetDatabase.LoadAssetAtPath<FontAtlasConfigAsset>(fontAtlasConfigAssetPath);
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (!_style)
            {
                var styleAssetPath = Path.Combine(PACKAGE_PATH, "Runtime/DefaultSettings/Default Style Asset.asset"); 
                _style = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleAsset>(styleAssetPath); 
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}