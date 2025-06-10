using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ImGui.Unity
{
    public class DearImGuiRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material _material;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        private DearImGuiPass _dearImGuiPass;

        public override void Create() 
            => _dearImGuiPass = new DearImGuiPass
            {
                Material = _material,
                renderPassEvent = _renderPassEvent
            };

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) 
            => renderer.EnqueuePass(_dearImGuiPass);
    }
}