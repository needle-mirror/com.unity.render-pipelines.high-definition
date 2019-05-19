using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using CED = CoreEditorDrawer<SerializedHDRaytracingEnvironment>;

    [CustomEditor(typeof(HDRaytracingEnvironment))]
    public class HDRaytracingEnvironmentInspector : Editor
    {
#if ENABLE_RAYTRACING
        protected static class Styles
        {
            // Generic
            public static readonly GUIContent genericSectionText = EditorGUIUtility.TrTextContent("Generic Attributes");
            public static readonly GUIContent rayBiasText = EditorGUIUtility.TrTextContent("Ray Bias");

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Ambient Occlusion
            public static readonly GUIContent aoSectionText = EditorGUIUtility.TrTextContent("Ray-traced Ambient Occlusion");
            public static readonly GUIContent aoEnableText = EditorGUIUtility.TrTextContent("Enable");
            public static readonly GUIContent aoLayerMaskText = EditorGUIUtility.TrTextContent("AO Layer Mask");
            public static readonly GUIContent aoRayLengthText = EditorGUIUtility.TrTextContent("Max AO Ray Length");
            public static readonly GUIContent aoNumSamplesText = EditorGUIUtility.TrTextContent("AO Number of Samples");
            public static readonly GUIContent aoFilterModeText = EditorGUIUtility.TrTextContent("AO Filter Mode");

            // AO Bilateral Filter Data
            public static GUIContent aoBilateralRadius = new GUIContent("Fitler Radius");

            // Nvidia Filter Data
            public static GUIContent aoNvidiaMaxFilterWidth = new GUIContent("AO Nvidia Max Filter Width");
            public static GUIContent aoNvidiaFilterRadius = new GUIContent("AO Nvidia Filter Radius");
            public static GUIContent aoNvidiaNormalSharpness = new GUIContent("AO Nvidia Normal Sharpness");

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Reflections
            public static GUIContent reflSectionText = new GUIContent("Ray-traced Reflections");
            public static GUIContent reflEnableText = new GUIContent("Enable");
            public static GUIContent reflLayerMaskText = EditorGUIUtility.TrTextContent("Reflection Layer Mask");
            public static GUIContent reflRayLengthText = new GUIContent("Max Reflections Ray Length");
            public static GUIContent reflBlendDistanceText = new GUIContent("Reflection Blend Distance");
            public static GUIContent reflMinSmoothnessText = new GUIContent("Reflections Min Smoothness");
            public static GUIContent reflClampValueText = new GUIContent("Reflections Clamp Value");
            public static GUIContent reflQualityText = new GUIContent("Reflections Quality");
            public static GUIContent reflFilerModeText = new GUIContent("Reflections Filter Mode");

            // Reflections Quarter Res
            public static GUIContent reflTemporalAccumulationWeight = new GUIContent("Reflections Temporal Accumulation Weight");

            // Relections Integration
            public static GUIContent reflNumMaxSamplesText = new GUIContent("Reflections Num Samples");

            // Filter data
            public static GUIContent reflFilterRadius = new GUIContent("Filter Radius");
            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Area Light Shadow
            public static GUIContent shadowEnableText = new GUIContent("Enable");
            public static GUIContent shadowLayerMaskText = EditorGUIUtility.TrTextContent("Shadow Layer Mask");
            public static GUIContent shadowSectionText = new GUIContent("Ray-traced Shadows");
            public static GUIContent shadowBilateralRadius = new GUIContent("Shadows Bilateral Radius");
            public static GUIContent shadowNumSamplesText = new GUIContent("Shadows Num Samples");
            public static GUIContent splitIntegrationText = new GUIContent("Split Integration");

            // Shadow Bilateral Filter Data
            public static GUIContent numAreaLightShadows = new GUIContent("Max Num Shadows");

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Light Cluster
            public static readonly GUIContent lightClusterSectionText = EditorGUIUtility.TrTextContent("Light Cluster");
            public static GUIContent maxNumLightsText = new GUIContent("Cluster Cell Max Lights");
            public static GUIContent cameraClusterRangeText = new GUIContent("Cluster Range");

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Primary visibility
            public static readonly GUIContent primaryRaytracingSectionText = EditorGUIUtility.TrTextContent("Primary Visiblity Raytracing");
            public static readonly GUIContent raytracingEnableText = new GUIContent("Enable");
            public static readonly GUIContent raytracedLayerMaskText = EditorGUIUtility.TrTextContent("Primary Visibility Layer Mask");
            public static readonly GUIContent rayMaxDepth = new GUIContent("Raytracing Maximal Depth");
            public static readonly GUIContent raytracingRayLength = new GUIContent("Raytracing Ray Length");

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // Indirect Diffuse
            public static readonly GUIContent indirectDiffuseSectionText = EditorGUIUtility.TrTextContent("Indirect Diffuse Raytracing");
            public static readonly GUIContent indirectDiffuseEnableText = new GUIContent("Enable");
            public static readonly GUIContent indirectDiffuseLayerMaskText = EditorGUIUtility.TrTextContent("Indirect Diffuse Layer Mask");
            public static readonly GUIContent indirectDiffuseNumSamplesText = new GUIContent("Indirect Diffuse Num Samples");
            public static readonly GUIContent indirectDiffuseRayLengthText = new GUIContent("Indirect Diffuse Ray Length");
            public static readonly GUIContent indirectDiffuseClampText = new GUIContent("Indirect Diffuse Clamp Value");
            public static readonly GUIContent indirectDiffuseFilterModeText = new GUIContent("Indirect Diffuse Filter Mode");
            public static readonly GUIContent indirectDiffuseFilterRadiusText = new GUIContent("Filter Radius");
        }

        SerializedHDRaytracingEnvironment m_SerializedHDRaytracingEnvironment;

        static readonly CED.IDrawer Inspector;

        enum Expandable
        {
            Generic = 1 << 0,
            AmbientOcclusion = 1 << 1,
            Reflection = 1 << 2,
            LightCluster = 1 << 3,
            AreaShadow = 1 << 4,
            PrimaryRaytracing = 1 << 5,
            IndirectDiffuse = 1 << 6
        }
        static ExpandedState<Expandable, HDRaytracingEnvironment> k_ExpandedState;

        static HDRaytracingEnvironmentInspector()
        {
            Inspector = CED.Group(CED.FoldoutGroup(Styles.genericSectionText, Expandable.Generic, k_ExpandedState, GenericSubMenu),
                        CED.FoldoutGroup(Styles.aoSectionText, Expandable.AmbientOcclusion, k_ExpandedState, AmbientOcclusionSubMenu),
                        CED.FoldoutGroup(Styles.reflSectionText, Expandable.Reflection, k_ExpandedState, ReflectionsSubMenu),
                        CED.FoldoutGroup(Styles.shadowSectionText, Expandable.AreaShadow, k_ExpandedState, AreaShadowSubMenu),
                        CED.FoldoutGroup(Styles.primaryRaytracingSectionText, Expandable.PrimaryRaytracing, k_ExpandedState, RaytracingSubMenu),
                        CED.FoldoutGroup(Styles.indirectDiffuseSectionText, Expandable.IndirectDiffuse, k_ExpandedState, IndirectDiffuseSubMenu),
                        CED.FoldoutGroup(Styles.lightClusterSectionText, Expandable.LightCluster, k_ExpandedState, LightClusterSubMenu));
        }
        static void GenericSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            // AO Specific fields
            EditorGUILayout.PropertyField(rtEnv.rayBias, Styles.rayBiasText);
        }

        static void UpdateEnvironmentSubScenes(SerializedHDRaytracingEnvironment rtEnv)
        {
            rtEnv.Apply();
            HDRenderPipeline hdPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            if (hdPipeline != null)
            {
                hdPipeline.m_RayTracingManager.UpdateEnvironmentSubScenes();
            }
        }

        static void AmbientOcclusionSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            // AO Specific fields
            EditorGUILayout.PropertyField(rtEnv.raytracedAO, Styles.aoEnableText);

            if(rtEnv.raytracedAO.boolValue)
            {
                EditorGUI.indentLevel++;

                // For the layer masks, we want to make sure the matching resources will be available during the following draw call. So we need to force a propagation to
                // the non serialized object and update the subscenes
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rtEnv.aoLayerMask, Styles.aoLayerMaskText);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateEnvironmentSubScenes(rtEnv);
                }

                EditorGUILayout.IntSlider(rtEnv.aoNumSamples, 1, 32, Styles.aoNumSamplesText);
                EditorGUILayout.PropertyField(rtEnv.aoRayLength, Styles.aoRayLengthText);
                EditorGUILayout.PropertyField(rtEnv.aoFilterMode, Styles.aoFilterModeText);

                EditorGUI.indentLevel++;
                switch ((HDRaytracingEnvironment.AOFilterMode)rtEnv.aoFilterMode.enumValueIndex)
                {
                    case HDRaytracingEnvironment.AOFilterMode.SpatioTemporal:
                        {
                            EditorGUILayout.PropertyField(rtEnv.aoBilateralRadius, Styles.aoBilateralRadius);
                        }
                        break;
                    case HDRaytracingEnvironment.AOFilterMode.Nvidia:
                        {
                            EditorGUILayout.PropertyField(rtEnv.maxFilterWidthInPixels, Styles.aoNvidiaMaxFilterWidth);
                            EditorGUILayout.PropertyField(rtEnv.filterRadiusInMeters, Styles.aoNvidiaFilterRadius);
                            EditorGUILayout.PropertyField(rtEnv.normalSharpness, Styles.aoNvidiaNormalSharpness);
                        }
                        break;
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        static void ReflectionsSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            // AO Specific fields
            EditorGUILayout.PropertyField(rtEnv.raytracedReflections, Styles.reflEnableText);

            if (rtEnv.raytracedReflections.boolValue)
            {
                EditorGUI.indentLevel++;

                // For the layer masks, we want to make sure the matching resources will be available during the following draw call. So we need to force a propagation to
                // the non serialized object and update the sub-scenes
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rtEnv.reflLayerMask, Styles.reflLayerMaskText);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateEnvironmentSubScenes(rtEnv);
                }

                EditorGUILayout.PropertyField(rtEnv.reflRayLength, Styles.reflRayLengthText);
                EditorGUILayout.PropertyField(rtEnv.reflBlendDistance, Styles.reflBlendDistanceText);
                EditorGUILayout.PropertyField(rtEnv.reflMinSmoothness, Styles.reflMinSmoothnessText);
                EditorGUILayout.PropertyField(rtEnv.reflClampValue, Styles.reflClampValueText);

                EditorGUILayout.PropertyField(rtEnv.reflQualityMode, Styles.reflQualityText);

                EditorGUI.indentLevel++;
                switch ((HDRaytracingEnvironment.ReflectionsQuality)rtEnv.reflQualityMode.enumValueIndex)
                {
                    case HDRaytracingEnvironment.ReflectionsQuality.QuarterRes:
                        {
                            EditorGUILayout.PropertyField(rtEnv.reflTemporalAccumulationWeight, Styles.reflTemporalAccumulationWeight);
                            EditorGUILayout.PropertyField(rtEnv.reflSpatialFilterRadius, Styles.reflFilterRadius);
                        }
                    break;
                    case HDRaytracingEnvironment.ReflectionsQuality.Integration:
                        {
                            EditorGUILayout.PropertyField(rtEnv.reflNumMaxSamples, Styles.reflNumMaxSamplesText);
                            EditorGUILayout.PropertyField(rtEnv.reflFilterMode, Styles.reflFilerModeText);

                            switch ((HDRaytracingEnvironment.ReflectionsFilterMode)rtEnv.reflFilterMode.enumValueIndex)
                            {
                                case HDRaytracingEnvironment.ReflectionsFilterMode.SpatioTemporal:
                                {
                                    EditorGUILayout.PropertyField(rtEnv.reflFilterRadius, Styles.reflFilterRadius);
                                }
                                break;
                            }
                        }
                    break;
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        static void RaytracingSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            // Primary Visibility Specific fields
            EditorGUILayout.PropertyField(rtEnv.raytracedObjects, Styles.raytracingEnableText);

            if (rtEnv.raytracedObjects.boolValue)
            {
                // For the layer masks, we want to make sure the matching resources will be available during the following draw call. So we need to force a propagation to
                // the non serialized object and update the sub-scenes
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rtEnv.raytracedLayerMask, Styles.raytracedLayerMaskText);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateEnvironmentSubScenes(rtEnv);
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rtEnv.rayMaxDepth, Styles.rayMaxDepth);
                EditorGUILayout.PropertyField(rtEnv.raytracingRayLength, Styles.raytracingRayLength);
                EditorGUI.indentLevel--;
            }
        }

        static void LightClusterSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            EditorGUILayout.PropertyField(rtEnv.maxNumLightsPercell, Styles.maxNumLightsText);
            EditorGUILayout.PropertyField(rtEnv.cameraClusterRange, Styles.cameraClusterRangeText);
        }

        static void AreaShadowSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            EditorGUILayout.PropertyField(rtEnv.raytracedShadows, Styles.shadowEnableText);

            if (rtEnv.raytracedShadows.boolValue)
            {
                // For the layer masks, we want to make sure the matching resources will be available during the following draw call. So we need to force a propagation to
                // the non serialized object and update the sub-scenes
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rtEnv.shadowLayerMask, Styles.shadowLayerMaskText);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateEnvironmentSubScenes(rtEnv);
                }

                EditorGUILayout.PropertyField(rtEnv.shadowNumSamples, Styles.shadowNumSamplesText);
                EditorGUILayout.PropertyField(rtEnv.numAreaLightShadows, Styles.numAreaLightShadows);
                EditorGUILayout.PropertyField(rtEnv.shadowFilterRadius, Styles.shadowBilateralRadius);
                EditorGUILayout.PropertyField(rtEnv.splitIntegration, Styles.splitIntegrationText);
            }
        }

        static void IndirectDiffuseSubMenu(SerializedHDRaytracingEnvironment rtEnv, Editor owner)
        {
            EditorGUILayout.PropertyField(rtEnv.raytracedIndirectDiffuse, Styles.indirectDiffuseEnableText);
            if (rtEnv.raytracedIndirectDiffuse.boolValue)
            {
                // For the layer masks, we want to make sure the matching resources will be available during the following draw call. So we need to force a propagation to
                // the non serialized object and update the sub-scenes
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rtEnv.indirectDiffuseLayerMask, Styles.indirectDiffuseLayerMaskText);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateEnvironmentSubScenes(rtEnv);
                }

                EditorGUILayout.PropertyField(rtEnv.indirectDiffuseNumSamples, Styles.indirectDiffuseNumSamplesText);
                EditorGUILayout.PropertyField(rtEnv.indirectDiffuseRayLength, Styles.indirectDiffuseRayLengthText);
                EditorGUILayout.PropertyField(rtEnv.indirectDiffuseClampValue, Styles.indirectDiffuseClampText);

                EditorGUILayout.PropertyField(rtEnv.indirectDiffuseFilterMode, Styles.indirectDiffuseFilterModeText);
                switch ((HDRaytracingEnvironment.IndirectDiffuseFilterMode)rtEnv.indirectDiffuseFilterMode.enumValueIndex)
                {
                    case HDRaytracingEnvironment.IndirectDiffuseFilterMode.SpatioTemporal:
                    {
                        EditorGUILayout.PropertyField(rtEnv.indirectDiffuseFilterRadius, Styles.indirectDiffuseFilterRadiusText);
                    }
                    break;
                }
            }
        }

        protected void OnEnable()
        {
            HDRaytracingEnvironment rtEnv = (HDRaytracingEnvironment)target;

            // Get & automatically add additional HD data if not present
            m_SerializedHDRaytracingEnvironment = new SerializedHDRaytracingEnvironment(rtEnv);

            k_ExpandedState = new ExpandedState<Expandable, HDRaytracingEnvironment>(~(-1), "HDRP");

        }

        public override void OnInspectorGUI()
        {
            m_SerializedHDRaytracingEnvironment.Update();
            Inspector.Draw(m_SerializedHDRaytracingEnvironment, this);
            m_SerializedHDRaytracingEnvironment.Apply();
        }
#endif
    }
}
