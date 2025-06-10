using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

internal class DearImGuiPass : ScriptableRenderPass
{
    public Material Material;

    private readonly ProfilingSampler _profilingSampler = new("Dear ImGui");

    private class PassData
    {
        public Material Material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>
                   ("Dear ImGui", out var passData, _profilingSampler))
        {
            passData.Material = Material;
            
            var resourceData = frameData.Get<UniversalResourceData>();
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            
            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                ExecutePass(data, rgContext));
        }
    }

    private static void ExecutePass(PassData data, RasterGraphContext rgContext)
    {
        var mesh = new Mesh {
            name = "Dear ImGui Mesh",
            vertices = new[] { Vector3.zero, Vector3.right, Vector3.up},
            triangles = new[] { 0, 2, 1 }
        };

        rgContext.cmd.DrawMesh(mesh, Matrix4x4.identity, data.Material);
    }
}