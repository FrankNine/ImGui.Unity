using System;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Assertions;

using ImGuiNET;
using ImGui.Unity.Renderer;
using ImGui.Unity.Texture;
using ImGui.Unity.Extensions;

internal class DearImGuiPass : ScriptableRenderPass
{
    public ImDrawDataPtr DrawDataPtr;
    public Material Material;
    public MaterialPropertyBlock MaterialPropertyBlock;
    public TextureManager TextureManager;
    public int TextureID;
    public Mesh Mesh;

    private readonly ProfilingSampler _profilingSampler = new("Dear ImGui");

    private class PassData
    {
        public ImDrawDataPtr DrawDataPtr;
        public Material Material;
        public MaterialPropertyBlock MaterialPropertyBlock;
        public TextureManager TextureManager;
        public int TextureID;
        public Mesh Mesh;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>
                   ("Dear ImGui", out var passData, _profilingSampler))
        {
            passData.DrawDataPtr = DrawDataPtr;
            passData.Material = Material;
            passData.MaterialPropertyBlock = MaterialPropertyBlock;
            passData.TextureManager = TextureManager;
            passData.TextureID = TextureID;
            passData.Mesh = Mesh;

            var resourceData = frameData.Get<UniversalResourceData>();
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                ExecutePass(data, rgContext));
        }
    }

    private static void ExecutePass(PassData data, RasterGraphContext rgContext)
    {
        ImDrawDataPtr drawData = data.DrawDataPtr;
        var commandBuffer = rgContext.cmd;
        Vector2 fbSize = (drawData.DisplaySize * drawData.FramebufferScale).ToUnity();
        TextureManager textureManager = data.TextureManager;

        IntPtr prevTextureId = IntPtr.Zero;
        Vector4 clipOffset = new Vector4(drawData.DisplayPos.X, drawData.DisplayPos.Y,
            drawData.DisplayPos.X, drawData.DisplayPos.Y);
        Vector4 clipScale = new Vector4(drawData.FramebufferScale.X, drawData.FramebufferScale.Y,
            drawData.FramebufferScale.X, drawData.FramebufferScale.Y);

        commandBuffer.SetViewport(new Rect(0f, 0f, fbSize.x, fbSize.y));
        commandBuffer.SetViewProjectionMatrices(
            Matrix4x4.Translate(new Vector3(0.5f / fbSize.x, 0.5f / fbSize.y, 0f)), // Small adjustment to improve text.
            Matrix4x4.Ortho(0f, fbSize.x, fbSize.y, 0f, 0f, 1f));

        int subOf = 0;
        for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
        {
            ImDrawListPtr drawList = drawData.CmdLists[n];
            for (int i = 0, iMax = drawList.CmdBuffer.Size; i < iMax; ++i, ++subOf)
            {
                ImDrawCmdPtr drawCmd = drawList.CmdBuffer[i];
                if (drawCmd.UserCallback != IntPtr.Zero)
                {
                    UserDrawCallback userDrawCallback =
                        Marshal.GetDelegateForFunctionPointer<UserDrawCallback>(drawCmd.UserCallback);
                    userDrawCallback(drawList, drawCmd);
                }
                else
                {
                    // Project scissor rectangle into framebuffer space and skip if fully outside.
                    Vector4 clipSize = drawCmd.ClipRect.ToUnity() - clipOffset;
                    Vector4 clip = Vector4.Scale(clipSize, clipScale);

                    if (clip.x >= fbSize.x || clip.y >= fbSize.y || clip.z < 0f || clip.w < 0f) continue;

                    if (prevTextureId != drawCmd.TextureId)
                    {
                        prevTextureId = drawCmd.TextureId;

                        // TODO: Implement ImDrawCmdPtr.GetTexID().
                        bool hasTexture = textureManager.TryGetTexture(prevTextureId, out Texture texture);
                        Assert.IsTrue(hasTexture, "Texture does not exist. Try to use UImGuiUtility.GetTextureID().");

                        data.MaterialPropertyBlock.SetTexture(data.TextureID, texture);
                    }

                    commandBuffer.EnableScissorRect(new Rect(clip.x, fbSize.y - clip.w, clip.z - clip.x,
                        clip.w - clip.y)); // Invert y.
                    commandBuffer.DrawMesh(data.Mesh, Matrix4x4.identity, data.Material, subOf, -1,
                        data.MaterialPropertyBlock);
                }
            }
        }

        commandBuffer.DisableScissorRect();
    }
}