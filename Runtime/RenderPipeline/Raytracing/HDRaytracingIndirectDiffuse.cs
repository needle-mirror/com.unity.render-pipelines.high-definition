using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
#if ENABLE_RAYTRACING
    public class HDRaytracingIndirectDiffuse
    {
        // External structures
        HDRenderPipelineAsset m_PipelineAsset = null;
        RenderPipelineResources m_PipelineResources = null;
        SkyManager m_SkyManager = null;
        HDRaytracingManager m_RaytracingManager = null;
        SharedRTManager m_SharedRTManager = null;

        // Intermediate buffer that stores the indirect diffuse pre-denoising
        RTHandleSystem.RTHandle m_IndirectDiffuseTexture = null;

        // String values
        const string m_RayGenIndirectDiffuseName = "RayGenIndirectDiffuse";
        const string m_MissShaderName = "MissShaderIndirectDiffuse";
        const string m_ClosestHitShaderName = "ClosestHitMain";

        public HDRaytracingIndirectDiffuse()
        {
        }

        public void Init(HDRenderPipelineAsset asset, SkyManager skyManager, HDRaytracingManager raytracingManager, SharedRTManager sharedRTManager)
        {
            // Keep track of the pipeline asset
            m_PipelineAsset = asset;
            m_PipelineResources = asset.renderPipelineResources;

            // Keep track of the sky manager
            m_SkyManager = skyManager;

            // keep track of the ray tracing manager
            m_RaytracingManager = raytracingManager;

            // Keep track of the shared rt manager
            m_SharedRTManager = sharedRTManager;

            m_IndirectDiffuseTexture = RTHandles.Alloc(Vector2.one, filterMode: FilterMode.Point, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, enableRandomWrite: true, useDynamicScale: true, useMipMap: false, name: "IndirectDiffuseBuffer");
        }

        public void Release()
        {
            RTHandles.Release(m_IndirectDiffuseTexture);
        }

        void BindIndirectDiffuseTexture(CommandBuffer cmd)
        {
            cmd.SetGlobalTexture(HDShaderIDs._IndirectDiffuseTexture, m_IndirectDiffuseTexture);
        }

        public RTHandleSystem.RTHandle GetIndirectDiffuseTexture()
        {
            return m_IndirectDiffuseTexture;
        }

        public bool RenderIndirectDiffuse(HDCamera hdCamera, CommandBuffer cmd, ScriptableRenderContext renderContext, uint frameCount)
        {
            // Bind the indirect diffuse texture
            BindIndirectDiffuseTexture(cmd);

            // First thing to check is: Do we have a valid ray-tracing environment?
            HDRaytracingEnvironment rtEnvironement = m_RaytracingManager.CurrentEnvironment();
            RaytracingShader indirectDiffuseShader = m_PipelineAsset.renderPipelineResources.shaders.indirectDiffuseRaytracing;

            bool invalidState = rtEnvironement == null || !rtEnvironement.raytracedIndirectDiffuse
                || indirectDiffuseShader == null 
                || m_PipelineResources.textures.owenScrambledTex == null || m_PipelineResources.textures.scramblingTex == null;

            // If no acceleration structure available, end it now
            if (invalidState)
                return false;

            // Grab the acceleration structures and the light cluster to use
            RaytracingAccelerationStructure accelerationStructure = m_RaytracingManager.RequestAccelerationStructure(rtEnvironement.indirectDiffuseLayerMask);
            HDRaytracingLightCluster lightCluster = m_RaytracingManager.RequestLightCluster(rtEnvironement.indirectDiffuseLayerMask);

            // Compute the actual resolution that is needed base on the quality
            string targetRayGen = m_RayGenIndirectDiffuseName;

            // Define the shader pass to use for the indirect diffuse pass
            cmd.SetRaytracingShaderPass(indirectDiffuseShader, "IndirectDXR");

            // Set the acceleration structure for the pass
            cmd.SetRaytracingAccelerationStructure(indirectDiffuseShader, HDShaderIDs._RaytracingAccelerationStructureName, accelerationStructure);

            // Inject the ray-tracing sampling data
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._OwenScrambledTexture, m_PipelineResources.textures.owenScrambledTex);
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._ScramblingTexture, m_PipelineResources.textures.scramblingTex);

            // Inject the ray generation data
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingRayBias, rtEnvironement.rayBias);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingRayMaxLength, rtEnvironement.indirectDiffuseRayLength);
            cmd.SetRaytracingIntParams(indirectDiffuseShader, HDShaderIDs._RaytracingNumSamples, rtEnvironement.indirectDiffuseNumSamples);
            int frameIndex = hdCamera.IsTAAEnabled() ? hdCamera.taaFrameIndex : (int)frameCount % 8;
            cmd.SetGlobalInt(HDShaderIDs._RaytracingFrameIndex, frameIndex);

            // Set the data for the ray generation
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._IndirectDiffuseTextureRW, m_IndirectDiffuseTexture);
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._DepthTexture, m_SharedRTManager.GetDepthStencilBuffer());
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._NormalBufferTexture, m_SharedRTManager.GetNormalBuffer());

            // Set the indirect diffuse parameters
            cmd.SetRaytracingFloatParams(indirectDiffuseShader, HDShaderIDs._RaytracingIntensityClamp, rtEnvironement.indirectDiffuseClampValue);

            // Set ray count tex
            cmd.SetRaytracingIntParam(indirectDiffuseShader, HDShaderIDs._RayCountEnabled, m_RaytracingManager.rayCountManager.RayCountIsEnabled());
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, targetRayGen, HDShaderIDs._RayCountTexture, m_RaytracingManager.rayCountManager.rayCountTexture);

            // Compute the pixel spread value
            float pixelSpreadAngle = Mathf.Atan(2.0f * Mathf.Tan(hdCamera.camera.fieldOfView * Mathf.PI / 360.0f) / Mathf.Min(hdCamera.actualWidth, hdCamera.actualHeight));
            cmd.SetRaytracingFloatParam(indirectDiffuseShader, HDShaderIDs._RaytracingPixelSpreadAngle, pixelSpreadAngle);

            // LightLoop data
            cmd.SetGlobalBuffer(HDShaderIDs._RaytracingLightCluster, lightCluster.GetCluster());
            cmd.SetGlobalBuffer(HDShaderIDs._LightDatasRT, lightCluster.GetLightDatas());
            cmd.SetGlobalVector(HDShaderIDs._MinClusterPos, lightCluster.GetMinClusterPos());
            cmd.SetGlobalVector(HDShaderIDs._MaxClusterPos, lightCluster.GetMaxClusterPos());
            cmd.SetGlobalInt(HDShaderIDs._LightPerCellCount, rtEnvironement.maxNumLightsPercell);
            cmd.SetGlobalInt(HDShaderIDs._PunctualLightCountRT, lightCluster.GetPunctualLightCount());
            cmd.SetGlobalInt(HDShaderIDs._AreaLightCountRT, lightCluster.GetAreaLightCount());

            // Set the data for the ray miss
            cmd.SetRaytracingTextureParam(indirectDiffuseShader, m_MissShaderName, HDShaderIDs._SkyTexture, m_SkyManager.skyReflection);

            // Compute the actual resolution that is needed base on the quality
            uint widthResolution = (uint)hdCamera.actualWidth;
            uint heightResolution = (uint)hdCamera.actualHeight;

            // Run the calculus
            CoreUtils.SetKeyword(cmd, "DIFFUSE_LIGHTNG_ONLY", true);
            cmd.DispatchRays(indirectDiffuseShader, targetRayGen, widthResolution, heightResolution, 1);
            CoreUtils.SetKeyword(cmd, "DIFFUSE_LIGHTNG_ONLY", false);

            return true;
        }
    }
#endif
}
