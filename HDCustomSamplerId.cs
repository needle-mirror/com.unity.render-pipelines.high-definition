namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public enum CustomSamplerId
    {
        PushGlobalParameters,
        CopySetDepthBuffer,
        CopyDepthStencilbuffer,
        HTileForSSS,
        Forward,
        RenderSSAO,
        RenderShadows,
        RenderDeferredDirectionalShadow,
        BuildLightList,
        BlitToFinalRT,
        Distortion,
        ApplyDistortion,
        DepthPrepass,
        TransparentDepthPrepass,
        GBuffer,
        DBuffer,
        DisplayDebugViewMaterial,
        DebugViewMaterialGBuffer,
        BlitDebugViewMaterialDebug,
        SubsurfaceScattering,
        ForwardPassName,
        ForwardTransparentDepthPrepass,
        RenderForwardError,
        TransparentDepthPostPass,
        Velocity,
        GaussianPyramidColor,
        PyramidDepth,
        PostProcessing,
        RenderDebug,
        InitAndClearBuffer,
        InitGBuffersAndClearDepthStencil,
        ClearSSSDiffuseTarget,
        ClearSSSFilteringTarget,
        ClearAndCopyStencilTexture,
        ClearHTile,
        ClearHDRTarget,
        ClearGBuffer,
        HDRenderPipelineRender,
        CullResultsCull,

        // Profile sampler for tile pass
        TPPrepareLightsForGPU,
        TPPushGlobalParameters,
        TPTiledLightingDebug,
        TPDeferredDirectionalShadow,
        TPTileSettingsEnableTileAndCluster,
        TPForwardPass,
        TPForwardTiledClusterpass,
        TPDisplayShadows,
        TPRenderDeferredLighting,

        // Misc
        VolumeUpdate,

        Max
    }
}
