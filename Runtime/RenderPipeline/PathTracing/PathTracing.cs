using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    [Serializable, VolumeComponentMenu("Path Tracing")]
    public sealed class PathTracing : VolumeComponent
    {
        [Tooltip("Enables path tracing (thus disabling most other passes).")]
        public BoolParameter enable = new BoolParameter(false);

        [Tooltip("Defines the maximum number of paths cast within each pixel.")]
        public ClampedIntParameter maxSamples = new ClampedIntParameter(256, 1, 512);

        [Tooltip("Defines the minimum number of bounces for each path.")]
        public ClampedIntParameter minDepth = new ClampedIntParameter(1, 1, 10);

        [Tooltip("Defines the maximum number of bounces for each path.")]
        public ClampedIntParameter maxDepth = new ClampedIntParameter(3, 1, 10);

        [Tooltip("Defines the maximum intensity value computed for a path.")]
        public ClampedFloatParameter maxIntensity = new ClampedFloatParameter(10f, 0f, 100f);
    }

#if ENABLE_RAYTRACING
    public partial class HDRenderPipeline
    {
        // String values
        const string m_PathTracingRayGenShaderName = "RayGen";

        public void InitPathTracing()
        {
        }

        public void ReleasePathTracing()
        {
        }

        static RTHandle PathTracingHistoryBufferAllocatorFunction(string viewName, int frameIndex, RTHandleSystem rtHandleSystem)
        {
            return rtHandleSystem.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureXR.dimension,
                                        enableRandomWrite: true, useMipMap: false, autoGenerateMips: false,
                                        name: string.Format("PathTracingHistoryBuffer{0}", frameIndex));
        }

        public void PathTracingRender(HDCamera hdCamera, CommandBuffer cmd, RTHandle outputTexture, ScriptableRenderContext renderContext, int frameCount)
        {
            // First thing to check is: Do we have a valid ray-tracing environment?
            HDRaytracingEnvironment rtEnvironment = m_RayTracingManager.CurrentEnvironment();
            HDRenderPipeline renderPipeline = m_RayTracingManager.GetRenderPipeline();
            RayTracingShader pathTracingShader = m_Asset.renderPipelineRayTracingResources.pathTracing;

            PathTracing pathTracingSettings = VolumeManager.instance.stack.GetComponent<PathTracing>();
            LightCluster lightClusterSettings = VolumeManager.instance.stack.GetComponent<LightCluster>();

            // Check the validity of the state before computing the effect
            bool invalidState = rtEnvironment == null || !pathTracingSettings.enable.value || pathTracingShader == null
                || m_Asset.renderPipelineResources.textures.owenScrambled256Tex == null
                || m_Asset.renderPipelineResources.textures.scramblingTex == null;

            // If any resource or game-object is missing We stop right away
            if (invalidState)
                return;

            // Grab the history buffer (hijack the reflections one)
            RTHandle history = hdCamera.GetCurrentFrameRT((int)HDCameraFrameHistoryType.PathTracing)
                ?? hdCamera.AllocHistoryFrameRT((int)HDCameraFrameHistoryType.PathTracing, PathTracingHistoryBufferAllocatorFunction, 1);

            // Grab the acceleration structure and the list of HD lights for the target camera
            RayTracingAccelerationStructure accelerationStructure = m_RayTracingManager.RequestAccelerationStructure(rtEnvironment.raytracedLayerMask);
            HDRaytracingLightCluster lightCluster = m_RayTracingManager.RequestLightCluster(rtEnvironment.raytracedLayerMask);

            // Define the shader pass to use for the path tracing pass
            cmd.SetRayTracingShaderPass(pathTracingShader, "PathTracingDXR");

            // Set the acceleration structure for the pass
            cmd.SetRayTracingAccelerationStructure(pathTracingShader, HDShaderIDs._RaytracingAccelerationStructureName, accelerationStructure);

            // Inject the ray-tracing sampling data
            cmd.SetGlobalTexture(HDShaderIDs._OwenScrambledTexture, m_Asset.renderPipelineResources.textures.owenScrambled256Tex);
            cmd.SetGlobalTexture(HDShaderIDs._ScramblingTexture, m_Asset.renderPipelineResources.textures.scramblingTex);

            // Inject the ray generation data
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingRayBias, rtEnvironment.rayBias);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingNumSamples, pathTracingSettings.maxSamples.value);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingMinRecursion, pathTracingSettings.minDepth.value);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingMaxRecursion, pathTracingSettings.maxDepth.value);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingIntensityClamp, pathTracingSettings.maxIntensity.value);
            cmd.SetGlobalFloat(HDShaderIDs._RaytracingCameraNearPlane, hdCamera.camera.nearClipPlane);

            // Set the data for the ray generation
            //cmd.SetRayTracingTextureParam(pathTracingShader, HDShaderIDs._DepthTexture, m_SharedRTManager.GetDepthStencilBuffer());
            cmd.SetRayTracingTextureParam(pathTracingShader, HDShaderIDs._CameraColorTextureRW, outputTexture);
            cmd.SetGlobalInt(HDShaderIDs._RaytracingFrameIndex, frameCount);

            // Compute an approximate pixel spread angle value (in radians)
            float pixelSpreadAngle = hdCamera.camera.fieldOfView * (Mathf.PI / 180.0f) / Mathf.Min(hdCamera.actualWidth, hdCamera.actualHeight);
            cmd.SetRayTracingFloatParam(pathTracingShader, HDShaderIDs._RaytracingPixelSpreadAngle, pixelSpreadAngle);

            // LightLoop data
            cmd.SetGlobalBuffer(HDShaderIDs._RaytracingLightCluster, lightCluster.GetCluster());
            cmd.SetGlobalBuffer(HDShaderIDs._LightDatasRT, lightCluster.GetLightDatas());
            cmd.SetGlobalVector(HDShaderIDs._MinClusterPos, lightCluster.GetMinClusterPos());
            cmd.SetGlobalVector(HDShaderIDs._MaxClusterPos, lightCluster.GetMaxClusterPos());
            cmd.SetGlobalInt(HDShaderIDs._LightPerCellCount, lightClusterSettings.maxNumLightsPercell.value);
            cmd.SetGlobalInt(HDShaderIDs._PunctualLightCountRT, lightCluster.GetPunctualLightCount());
            cmd.SetGlobalInt(HDShaderIDs._AreaLightCountRT, lightCluster.GetAreaLightCount());

            // Note: directional lights are not supported atm
            //cmd.SetGlobalBuffer(HDShaderIDs._DirectionalLightDatas, renderPipeline.directionalLightDatas);
            //cmd.SetGlobalInt(HDShaderIDs._DirectionalLightCount, renderPipeline.m_lightList.directionalLights.Count);

            // Set the data for the ray miss
            cmd.SetRayTracingTextureParam(pathTracingShader, HDShaderIDs._SkyTexture, m_SkyManager.skyReflection);

            // Additional data for path tracing
            cmd.SetRayTracingTextureParam(pathTracingShader, HDShaderIDs._AccumulatedFrameTexture, history);
            cmd.SetRayTracingMatrixParam(pathTracingShader, HDShaderIDs._PixelCoordToViewDirWS, hdCamera.mainViewConstants.pixelCoordToViewDirWS);

            // Run the computation
            cmd.DispatchRays(pathTracingShader, m_PathTracingRayGenShaderName, (uint)hdCamera.actualWidth, (uint)hdCamera.actualHeight, 1);
        }
    }
#endif
}
