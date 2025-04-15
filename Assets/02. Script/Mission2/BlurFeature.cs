using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurFeature : ScriptableRendererFeature
{
    class BlurPass : ScriptableRenderPass
    {
        private Material blurMaterial;
        private RTHandle source;
        private RenderTargetHandle tempTexture;

        public BlurPass(Material material)
        {
            this.blurMaterial = material;
            tempTexture.Init("_TemporaryColorTexture");
        }

        public void Setup(RTHandle cameraColorHandle)
        {
            this.source = cameraColorHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("BlurPass");

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            int tempID = Shader.PropertyToID("_TempColorTex");
            cmd.GetTemporaryRT(tempID, descriptor, FilterMode.Bilinear);

            // RTHandle은 Blit 시 .nameID로 접근
            cmd.Blit(source.nameID, tempID, blurMaterial, 0); // Horizontal
            cmd.Blit(tempID, source.nameID, blurMaterial, 1); // Vertical

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material blurMaterial;
    private BlurPass blurPass;

    public override void Create()
    {
        blurPass = new BlurPass(blurMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Unity 2022.3 이상에서는 RTHandle 사용
        blurPass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(blurPass);
    }
}
