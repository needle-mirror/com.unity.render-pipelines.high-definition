namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public class HDRISkyRenderer : SkyRenderer
    {
        Material m_SkyHDRIMaterial; // Renders a cubemap into a render texture (can be cube or 2D)
        MaterialPropertyBlock m_PropertyBlock;
        HDRISkySettings m_HdriSkyParams;

        public HDRISkyRenderer(HDRISkySettings hdriSkyParams)
        {
            m_HdriSkyParams = hdriSkyParams;
            m_PropertyBlock = new MaterialPropertyBlock();
        }

        public override void Build()
        {
            m_SkyHDRIMaterial = CoreUtils.CreateEngineMaterial("Hidden/HDRenderPipeline/Sky/SkyHDRI");
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_SkyHDRIMaterial);
        }

        public override void SetRenderTargets(BuiltinSkyParameters builtinParams)
        {
            if (builtinParams.depthBuffer == BuiltinSkyParameters.nullRT)
            {
                CoreUtils.SetRenderTarget(builtinParams.commandBuffer, builtinParams.colorBuffer);
            }
            else
            {
                CoreUtils.SetRenderTarget(builtinParams.commandBuffer, builtinParams.colorBuffer, builtinParams.depthBuffer);
            }
        }

        public override void RenderSky(BuiltinSkyParameters builtinParams, bool renderForCubemap)
        {
            m_SkyHDRIMaterial.SetTexture(HDShaderIDs._Cubemap, m_HdriSkyParams.skyHDRI);
            m_SkyHDRIMaterial.SetVector(HDShaderIDs._SkyParam, new Vector4(m_HdriSkyParams.exposure, m_HdriSkyParams.multiplier, m_HdriSkyParams.rotation, 0.0f));

            // This matrix needs to be updated at the draw call frequency.
            m_PropertyBlock.SetMatrix(HDShaderIDs._PixelCoordToViewDirWS, builtinParams.pixelCoordToViewDirMatrix);

            CoreUtils.DrawFullScreen(builtinParams.commandBuffer, m_SkyHDRIMaterial, m_PropertyBlock, renderForCubemap ? 0 : 1);
        }

        public override bool IsSkyValid()
        {
            return m_HdriSkyParams != null && m_SkyHDRIMaterial != null;
        }
    }
}
