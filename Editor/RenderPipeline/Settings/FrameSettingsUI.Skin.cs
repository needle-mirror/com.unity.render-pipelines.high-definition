using UnityEditor.Rendering;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    partial class FrameSettingsUI
    {
        static readonly GUIContent frameSettingsHeaderContent = EditorGUIUtility.TrTextContent("Frame Settings Override", "Default FrameSettings are defined in HDRenderPipelineAsset.");

        const string renderingSettingsHeaderContent = "Rendering";
        const string lightSettingsHeaderContent = "Lighting";
        const string asyncComputeSettingsHeaderContent = "Async Compute";
        const string lightLoopSettingsHeaderContent = "Light Loop";

        // Rendering Passes
        static readonly GUIContent transparentPrepassContent = EditorGUIUtility.TrTextContent("Transparent Prepass");
        static readonly GUIContent transparentPostpassContent = EditorGUIUtility.TrTextContent("Transparent Postpass");
        static readonly GUIContent motionVectorContent = EditorGUIUtility.TrTextContent("Motion Vectors");
        static readonly GUIContent objectMotionVectorsContent = EditorGUIUtility.TrTextContent("Object Motion Vectors");
        static readonly GUIContent decalsContent = EditorGUIUtility.TrTextContent("Decals");
        static readonly GUIContent roughRefractionContent = EditorGUIUtility.TrTextContent("Rough Refraction");
        static readonly GUIContent distortionContent = EditorGUIUtility.TrTextContent("Distortion");
        static readonly GUIContent postprocessContent = EditorGUIUtility.TrTextContent("Postprocess");

        // Rendering Settings
        static readonly GUIContent litShaderModeContent = EditorGUIUtility.TrTextContent("Lit Shader Mode");
        static readonly GUIContent depthPrepassWithDeferredRenderingContent = EditorGUIUtility.TrTextContent("Depth Prepass With Deferred Rendering");
        static readonly GUIContent opaqueObjectsContent = EditorGUIUtility.TrTextContent("Opaque Objects");
        static readonly GUIContent transparentObjectsContent = EditorGUIUtility.TrTextContent("Transparent Objects");
        static readonly GUIContent realtimePlanarReflectionContent = EditorGUIUtility.TrTextContent("Enable Realtime Planar Reflection"); 
        static readonly GUIContent msaaContent = EditorGUIUtility.TrTextContent("MSAA");
        static readonly GUIContent shadowContent = EditorGUIUtility.TrTextContent("Shadow");
        static readonly GUIContent contactShadowContent = EditorGUIUtility.TrTextContent("Contact Shadows");
        static readonly GUIContent shadowMaskContent = EditorGUIUtility.TrTextContent("Shadow Masks");
        static readonly GUIContent ssrContent = EditorGUIUtility.TrTextContent("SSR");
        static readonly GUIContent ssaoContent = EditorGUIUtility.TrTextContent("SSAO");
        static readonly GUIContent subsurfaceScatteringContent = EditorGUIUtility.TrTextContent("Subsurface Scattering");
        static readonly GUIContent transmissionContent = EditorGUIUtility.TrTextContent("Transmission");
        static readonly GUIContent atmosphericScatteringContent = EditorGUIUtility.TrTextContent("Atmospheric Scattering");
        static readonly GUIContent volumetricContent = EditorGUIUtility.TrTextContent("Volumetrics");
        static readonly GUIContent reprojectionForVolumetricsContent = EditorGUIUtility.TrTextContent("Reprojection For Volumetrics");
        static readonly GUIContent lightLayerContent = EditorGUIUtility.TrTextContent("LightLayers");
        static readonly GUIContent exposureControlContent = EditorGUIUtility.TrTextContent("Exposure Control");

        // Async compute
        static readonly GUIContent asyncComputeContent = EditorGUIUtility.TrTextContent("Async Compute", "This will have an effect only if target platform supports async compute.");
        static readonly GUIContent lightListAsyncContent = EditorGUIUtility.TrTextContent("Build Light List in Async");
        static readonly GUIContent SSRAsyncContent = EditorGUIUtility.TrTextContent("SSR in Async");
        static readonly GUIContent SSAOAsyncContent = EditorGUIUtility.TrTextContent("SSAO in Async");
        static readonly GUIContent contactShadowsAsyncContent = EditorGUIUtility.TrTextContent("Contact Shadows in Async");
        static readonly GUIContent volumeVoxelizationAsyncContent = EditorGUIUtility.TrTextContent("Volumetrics Voxelization in Async");
        
        // Light Loop
        // Uncomment if you re-enable LIGHTLOOP_SINGLE_PASS multi_compile in lit*.shader
        //static readonly GUIContent tileAndClusterContent = EditorGUIUtility.TrTextContent("Enable Tile And Cluster");
        static readonly GUIContent fptlForForwardOpaqueContent = EditorGUIUtility.TrTextContent("FPTL For Forward Opaque");
        static readonly GUIContent bigTilePrepassContent = EditorGUIUtility.TrTextContent("Big Tile Prepass");
        static readonly GUIContent computeLightEvaluationContent = EditorGUIUtility.TrTextContent("Compute Light Evaluation");
        static readonly GUIContent computeLightVariantsContent = EditorGUIUtility.TrTextContent("Compute Light Variants");
        static readonly GUIContent computeMaterialVariantsContent = EditorGUIUtility.TrTextContent("Compute Material Variants");
    }
}
