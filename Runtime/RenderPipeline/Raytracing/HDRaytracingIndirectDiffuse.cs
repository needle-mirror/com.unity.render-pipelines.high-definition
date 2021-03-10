using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    public partial class HDRenderPipeline
    {
        // String values
        const string m_RayGenIndirectDiffuseIntegrationName = "RayGenIntegration";

        // Kernels
        int m_RaytracingIndirectDiffuseFullResKernel;
        int m_RaytracingIndirectDiffuseHalfResKernel;
        int m_IndirectDiffuseUpscaleFullResKernel;
        int m_IndirectDiffuseUpscaleHalfResKernel;
        int m_AdjustIndirectDiffuseWeightKernel;

        void InitRayTracedIndirectDiffuse()
        {
            ComputeShader indirectDiffuseShaderCS = m_Asset.renderPipelineRayTracingResources.indirectDiffuseRaytracingCS;

            // Grab all the kernels we shall be using
            m_RaytracingIndirectDiffuseFullResKernel = indirectDiffuseShaderCS.FindKernel("RaytracingIndirectDiffuseFullRes");
            m_RaytracingIndirectDiffuseHalfResKernel = indirectDiffuseShaderCS.FindKernel("RaytracingIndirectDiffuseHalfRes");
            m_IndirectDiffuseUpscaleFullResKernel = indirectDiffuseShaderCS.FindKernel("IndirectDiffuseIntegrationUpscaleFullRes");
            m_IndirectDiffuseUpscaleHalfResKernel = indirectDiffuseShaderCS.FindKernel("IndirectDiffuseIntegrationUpscaleHalfRes");
            m_AdjustIndirectDiffuseWeightKernel = indirectDiffuseShaderCS.FindKernel("AdjustIndirectDiffuseWeight");
        }

        void ReleaseRayTracedIndirectDiffuse()
        {
        }

        static RTHandle IndirectDiffuseHistoryBufferAllocatorFunction(string viewName, int frameIndex, RTHandleSystem rtHandleSystem)
        {
            return rtHandleSystem.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureXR.dimension,
                enableRandomWrite: true, useMipMap: false, autoGenerateMips: false,
                name: string.Format("{0}_IndirectDiffuseHistoryBuffer{1}", viewName, frameIndex));
        }

        DeferredLightingRTParameters PrepareIndirectDiffuseDeferredLightingRTParameters(HDCamera hdCamera)
        {
            DeferredLightingRTParameters deferredParameters = new DeferredLightingRTParameters();

            // Fetch the GI volume component
            var settings = hdCamera.volumeStack.GetComponent<GlobalIllumination>();
            RayTracingSettings rTSettings = hdCamera.volumeStack.GetComponent<RayTracingSettings>();

            // Make sure the binning buffer has the right size
            CheckBinningBuffersSize(hdCamera);

            // Generic attributes
            deferredParameters.rayBinning = true;
            deferredParameters.layerMask.value = (int)RayTracingRendererFlag.GlobalIllumination;
            deferredParameters.diffuseLightingOnly = true;
            deferredParameters.halfResolution = !settings.fullResolution;
            deferredParameters.rayCountType = (int)RayCountValues.DiffuseGI_Deferred;

            // Camera data
            deferredParameters.width = hdCamera.actualWidth;
            deferredParameters.height = hdCamera.actualHeight;
            deferredParameters.viewCount = hdCamera.viewCount;

            // Compute buffers
            deferredParameters.rayBinResult = m_RayBinResult;
            deferredParameters.rayBinSizeResult = m_RayBinSizeResult;
            deferredParameters.accelerationStructure = RequestAccelerationStructure();
            deferredParameters.lightCluster = RequestLightCluster();

            // Shaders
            deferredParameters.gBufferRaytracingRT = m_Asset.renderPipelineRayTracingResources.gBufferRaytracingRT;
            deferredParameters.deferredRaytracingCS = m_Asset.renderPipelineRayTracingResources.deferredRaytracingCS;
            deferredParameters.rayBinningCS = m_Asset.renderPipelineRayTracingResources.rayBinningCS;

            // XRTODO: add ray binning support for single-pass
            if (deferredParameters.viewCount > 1 && deferredParameters.rayBinning)
            {
                deferredParameters.rayBinning = false;
            }

            // Make a copy of the previous values that were defined in the CB
            deferredParameters.raytracingCB = m_ShaderVariablesRayTracingCB;
            // Override the ones we need to
            deferredParameters.raytracingCB._RaytracingRayMaxLength = settings.rayLength;
            deferredParameters.raytracingCB._RaytracingIntensityClamp = settings.clampValue;
            deferredParameters.raytracingCB._RaytracingIncludeSky = 1;
            deferredParameters.raytracingCB._RaytracingPreExposition = 1;
            deferredParameters.raytracingCB._RayTracingDiffuseLightingOnly = 1;

            return deferredParameters;
        }

        struct RTIndirectDiffuseDirGenParameters
        {
            // Camera parameters
            public int texWidth;
            public int texHeight;
            public int viewCount;

            // Generation parameters
            public bool fullResolution;

            // Additional resources
            public BlueNoise.DitheredTextureSet ditheredTextureSet;
            public int dirGenKernel;
            public ComputeShader directionGenCS;
            public ShaderVariablesRaytracing shaderVariablesRayTracingCB;
        }

        RTIndirectDiffuseDirGenParameters PrepareRTIndirectDiffuseDirGenParameters(HDCamera hdCamera, GlobalIllumination settings)
        {
            RTIndirectDiffuseDirGenParameters rtidDirGenParams = new RTIndirectDiffuseDirGenParameters();

            // Set the camera parameters
            rtidDirGenParams.texWidth = hdCamera.actualWidth;
            rtidDirGenParams.texHeight = hdCamera.actualHeight;
            rtidDirGenParams.viewCount = hdCamera.viewCount;

            // Set the generation parameters
            rtidDirGenParams.fullResolution = settings.fullResolution;

            // Grab the right kernel
            rtidDirGenParams.directionGenCS = m_Asset.renderPipelineRayTracingResources.indirectDiffuseRaytracingCS;
            rtidDirGenParams.dirGenKernel = settings.fullResolution ? m_RaytracingIndirectDiffuseFullResKernel : m_RaytracingIndirectDiffuseHalfResKernel;

            // Grab the additional parameters
            BlueNoise blueNoise = GetBlueNoiseManager();
            rtidDirGenParams.ditheredTextureSet = blueNoise.DitheredTextureSet8SPP();
            rtidDirGenParams.shaderVariablesRayTracingCB = m_ShaderVariablesRayTracingCB;

            return rtidDirGenParams;
        }

        struct RTIndirectDiffuseDirGenResources
        {
            // Input buffers
            public RTHandle depthStencilBuffer;
            public RTHandle normalBuffer;

            // Output buffers
            public RTHandle outputBuffer;
        }

        static void RTIndirectDiffuseDirGen(CommandBuffer cmd, RTIndirectDiffuseDirGenParameters rtidDirGenParams, RTIndirectDiffuseDirGenResources rtidDirGenResources)
        {
            // Inject the ray-tracing sampling data
            BlueNoise.BindDitheredTextureSet(cmd, rtidDirGenParams.ditheredTextureSet);

            // Bind all the required textures
            cmd.SetComputeTextureParam(rtidDirGenParams.directionGenCS, rtidDirGenParams.dirGenKernel, HDShaderIDs._DepthTexture, rtidDirGenResources.depthStencilBuffer);
            cmd.SetComputeTextureParam(rtidDirGenParams.directionGenCS, rtidDirGenParams.dirGenKernel, HDShaderIDs._NormalBufferTexture, rtidDirGenResources.normalBuffer);

            // Bind the output buffers
            cmd.SetComputeTextureParam(rtidDirGenParams.directionGenCS, rtidDirGenParams.dirGenKernel, HDShaderIDs._RaytracingDirectionBuffer, rtidDirGenResources.outputBuffer);

            int numTilesXHR, numTilesYHR;
            if (rtidDirGenParams.fullResolution)
            {
                // Evaluate the dispatch parameters
                numTilesXHR = (rtidDirGenParams.texWidth + (rtReflectionsComputeTileSize - 1)) / rtReflectionsComputeTileSize;
                numTilesYHR = (rtidDirGenParams.texHeight + (rtReflectionsComputeTileSize - 1)) / rtReflectionsComputeTileSize;
            }
            else
            {
                // Evaluate the dispatch parameters
                numTilesXHR = (rtidDirGenParams.texWidth / 2 + (rtReflectionsComputeTileSize - 1)) / rtReflectionsComputeTileSize;
                numTilesYHR = (rtidDirGenParams.texHeight / 2 + (rtReflectionsComputeTileSize - 1)) / rtReflectionsComputeTileSize;
            }

            // Compute the directions
            cmd.DispatchCompute(rtidDirGenParams.directionGenCS, rtidDirGenParams.dirGenKernel, numTilesXHR, numTilesYHR, rtidDirGenParams.viewCount);
        }

        struct RTIndirectDiffuseUpscaleParameters
        {
            // Camera parameters
            public int texWidth;
            public int texHeight;
            public int viewCount;

            // Generation parameters
            public int upscaleRadius;

            // Additional resources
            public Texture2DArray blueNoiseTexture;
            public Texture2D scramblingTexture;
            public int upscaleKernel;
            public ComputeShader upscaleCS;
        }

        RTIndirectDiffuseUpscaleParameters PrepareRTIndirectDiffuseUpscaleParameters(HDCamera hdCamera, GlobalIllumination settings)
        {
            RTIndirectDiffuseUpscaleParameters rtidUpscaleParams = new RTIndirectDiffuseUpscaleParameters();

            // Set the camera parameters
            rtidUpscaleParams.texWidth = hdCamera.actualWidth;
            rtidUpscaleParams.texHeight = hdCamera.actualHeight;
            rtidUpscaleParams.viewCount = hdCamera.viewCount;

            // Set the generation parameters
            rtidUpscaleParams.upscaleRadius = settings.upscaleRadius;

            // Grab the right kernel
            rtidUpscaleParams.upscaleCS = m_Asset.renderPipelineRayTracingResources.indirectDiffuseRaytracingCS;
            rtidUpscaleParams.upscaleKernel = settings.fullResolution ? m_IndirectDiffuseUpscaleFullResKernel : m_IndirectDiffuseUpscaleHalfResKernel;

            // Grab the additional parameters
            BlueNoise blueNoise = GetBlueNoiseManager();
            rtidUpscaleParams.blueNoiseTexture = blueNoise.textureArray16RGB;
            rtidUpscaleParams.scramblingTexture = m_Asset.renderPipelineResources.textures.scramblingTex;

            return rtidUpscaleParams;
        }

        struct RTIndirectDiffuseUpscaleResources
        {
            // Input buffers
            public RTHandle depthStencilBuffer;
            public RTHandle normalBuffer;
            public RTHandle indirectDiffuseBuffer;
            public RTHandle directionBuffer;

            // Output buffers
            public RTHandle outputBuffer;
        }

        static void RTIndirectDiffuseUpscale(CommandBuffer cmd, RTIndirectDiffuseUpscaleParameters rtidUpscaleParams, RTIndirectDiffuseUpscaleResources rtidUpscaleResources)
        {
            // Inject all the parameters for the compute
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._DepthTexture, rtidUpscaleResources.depthStencilBuffer);
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._NormalBufferTexture, rtidUpscaleResources.normalBuffer);
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._IndirectDiffuseTexture, rtidUpscaleResources.indirectDiffuseBuffer);
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._RaytracingDirectionBuffer, rtidUpscaleResources.directionBuffer);
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._BlueNoiseTexture, rtidUpscaleParams.blueNoiseTexture);
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._ScramblingTexture, rtidUpscaleParams.scramblingTexture);
            cmd.SetComputeIntParam(rtidUpscaleParams.upscaleCS, HDShaderIDs._SpatialFilterRadius, rtidUpscaleParams.upscaleRadius);

            // Output buffer
            cmd.SetComputeTextureParam(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, HDShaderIDs._UpscaledIndirectDiffuseTextureRW, rtidUpscaleResources.outputBuffer);

            // Texture dimensions
            int texWidth = rtidUpscaleParams.texWidth;
            int texHeight = rtidUpscaleParams.texHeight;

            // Evaluate the dispatch parameters
            int areaTileSize = 8;
            int numTilesXHR = (texWidth + (areaTileSize - 1)) / areaTileSize;
            int numTilesYHR = (texHeight + (areaTileSize - 1)) / areaTileSize;

            // Compute the texture
            cmd.DispatchCompute(rtidUpscaleParams.upscaleCS, rtidUpscaleParams.upscaleKernel, numTilesXHR, numTilesYHR, rtidUpscaleParams.viewCount);
        }

        struct AdjustRTIDWeightParameters
        {
            // Camera parameters
            public int texWidth;
            public int texHeight;
            public int viewCount;

            // Additional resources
            public int adjustWeightKernel;
            public ComputeShader adjustWeightCS;
        }

        AdjustRTIDWeightParameters PrepareAdjustRTIDWeightParametersParameters(HDCamera hdCamera)
        {
            AdjustRTIDWeightParameters parameters = new AdjustRTIDWeightParameters();

            // Set the camera parameters
            parameters.texWidth = hdCamera.actualWidth;
            parameters.texHeight = hdCamera.actualHeight;
            parameters.viewCount = hdCamera.viewCount;

            // Grab the right kernel
            parameters.adjustWeightCS = m_Asset.renderPipelineRayTracingResources.indirectDiffuseRaytracingCS;
            parameters.adjustWeightKernel = m_AdjustIndirectDiffuseWeightKernel;

            return parameters;
        }

        static void AdjustRTIDWeight(CommandBuffer cmd, AdjustRTIDWeightParameters parameters, RTHandle indirectDiffuseTexture, RTHandle depthPyramid, RTHandle stencilBuffer)
        {
            // Input data
            cmd.SetComputeTextureParam(parameters.adjustWeightCS, parameters.adjustWeightKernel, HDShaderIDs._DepthTexture, depthPyramid);
            cmd.SetComputeTextureParam(parameters.adjustWeightCS, parameters.adjustWeightKernel, HDShaderIDs._StencilTexture, stencilBuffer, 0, RenderTextureSubElement.Stencil);
            cmd.SetComputeIntParams(parameters.adjustWeightCS, HDShaderIDs._SsrStencilBit, (int)StencilUsage.TraceReflectionRay);

            // In/Output buffer
            cmd.SetComputeTextureParam(parameters.adjustWeightCS, parameters.adjustWeightKernel, HDShaderIDs._IndirectDiffuseTextureRW, indirectDiffuseTexture);

            // Texture dimensions
            int texWidth = parameters.texWidth;
            int texHeight = parameters.texHeight;

            // Evaluate the dispatch parameters
            int areaTileSize = 8;
            int numTilesXHR = (texWidth + (areaTileSize - 1)) / areaTileSize;
            int numTilesYHR = (texHeight + (areaTileSize - 1)) / areaTileSize;

            // Compute the texture
            cmd.DispatchCompute(parameters.adjustWeightCS, parameters.adjustWeightKernel, numTilesXHR, numTilesYHR, parameters.viewCount);
        }

        struct QualityRTIndirectDiffuseParameters
        {
            // Camera parameters
            public int texWidth;
            public int texHeight;
            public int viewCount;

            // Evaluation parameters
            public float rayLength;
            public int sampleCount;
            public float clampValue;
            public int bounceCount;

            // Other parameters
            public RayTracingShader indirectDiffuseRT;
            public RayTracingAccelerationStructure accelerationStructure;
            public HDRaytracingLightCluster lightCluster;
            public Texture skyTexture;
            public ShaderVariablesRaytracing shaderVariablesRayTracingCB;
            public BlueNoise.DitheredTextureSet ditheredTextureSet;
        }

        QualityRTIndirectDiffuseParameters PrepareQualityRTIndirectDiffuseParameters(HDCamera hdCamera, GlobalIllumination settings)
        {
            QualityRTIndirectDiffuseParameters qrtidParams = new QualityRTIndirectDiffuseParameters();

            // Set the camera parameters
            qrtidParams.texWidth = hdCamera.actualWidth;
            qrtidParams.texHeight = hdCamera.actualHeight;
            qrtidParams.viewCount = hdCamera.viewCount;

            // Evaluation parameters
            qrtidParams.rayLength = settings.rayLength;
            qrtidParams.sampleCount = settings.sampleCount.value;
            qrtidParams.clampValue = settings.clampValue;
            qrtidParams.bounceCount = settings.bounceCount.value;

            // Grab the additional parameters
            qrtidParams.indirectDiffuseRT = m_Asset.renderPipelineRayTracingResources.indirectDiffuseRaytracingRT;
            qrtidParams.accelerationStructure = RequestAccelerationStructure();
            qrtidParams.lightCluster = RequestLightCluster();
            qrtidParams.skyTexture = m_SkyManager.GetSkyReflection(hdCamera);
            qrtidParams.shaderVariablesRayTracingCB = m_ShaderVariablesRayTracingCB;
            BlueNoise blueNoise = GetBlueNoiseManager();
            qrtidParams.ditheredTextureSet = blueNoise.DitheredTextureSet8SPP();
            return qrtidParams;
        }

        struct QualityRTIndirectDiffuseResources
        {
            // Input buffer
            public RTHandle depthBuffer;
            public RTHandle normalBuffer;

            // Debug buffer
            public RTHandle rayCountTexture;

            // Ouput buffer
            public RTHandle outputBuffer;
        }

        static void RenderQualityRayTracedIndirectDiffuse(CommandBuffer cmd, QualityRTIndirectDiffuseParameters qrtidParameters, QualityRTIndirectDiffuseResources qrtidResources)
        {
            // Define the shader pass to use for the indirect diffuse pass
            cmd.SetRayTracingShaderPass(qrtidParameters.indirectDiffuseRT, "IndirectDXR");

            // Set the acceleration structure for the pass
            cmd.SetRayTracingAccelerationStructure(qrtidParameters.indirectDiffuseRT, HDShaderIDs._RaytracingAccelerationStructureName, qrtidParameters.accelerationStructure);

            // Inject the ray-tracing sampling data
            BlueNoise.BindDitheredTextureSet(cmd, qrtidParameters.ditheredTextureSet);

            // Set the data for the ray generation
            cmd.SetRayTracingTextureParam(qrtidParameters.indirectDiffuseRT, HDShaderIDs._IndirectDiffuseTextureRW, qrtidResources.outputBuffer);
            cmd.SetRayTracingTextureParam(qrtidParameters.indirectDiffuseRT, HDShaderIDs._DepthTexture, qrtidResources.depthBuffer);
            cmd.SetRayTracingTextureParam(qrtidParameters.indirectDiffuseRT, HDShaderIDs._NormalBufferTexture, qrtidResources.normalBuffer);

            // Set ray count texture
            cmd.SetRayTracingTextureParam(qrtidParameters.indirectDiffuseRT, HDShaderIDs._RayCountTexture, qrtidResources.rayCountTexture);

            // LightLoop data
            qrtidParameters.lightCluster.BindLightClusterData(cmd);

            // Set the data for the ray miss
            cmd.SetRayTracingTextureParam(qrtidParameters.indirectDiffuseRT, HDShaderIDs._SkyTexture, qrtidParameters.skyTexture);

            // Update global constant buffer
            qrtidParameters.shaderVariablesRayTracingCB._RaytracingIntensityClamp = qrtidParameters.clampValue;
            qrtidParameters.shaderVariablesRayTracingCB._RaytracingIncludeSky = 1;
            qrtidParameters.shaderVariablesRayTracingCB._RaytracingRayMaxLength = qrtidParameters.rayLength;
            qrtidParameters.shaderVariablesRayTracingCB._RaytracingNumSamples = qrtidParameters.sampleCount;
            qrtidParameters.shaderVariablesRayTracingCB._RaytracingMaxRecursion = qrtidParameters.bounceCount;
            qrtidParameters.shaderVariablesRayTracingCB._RayTracingDiffuseLightingOnly = 1;
            ConstantBuffer.PushGlobal(cmd, qrtidParameters.shaderVariablesRayTracingCB, HDShaderIDs._ShaderVariablesRaytracing);

            // Only use the shader variant that has multi bounce if the bounce count > 1
            CoreUtils.SetKeyword(cmd, "MULTI_BOUNCE_INDIRECT", qrtidParameters.bounceCount > 1);

            // Run the computation
            cmd.DispatchRays(qrtidParameters.indirectDiffuseRT, m_RayGenIndirectDiffuseIntegrationName, (uint)qrtidParameters.texWidth, (uint)qrtidParameters.texHeight, (uint)qrtidParameters.viewCount);

            // Disable the keywords we do not need anymore
            CoreUtils.SetKeyword(cmd, "MULTI_BOUNCE_INDIRECT", false);
        }
    }
}
