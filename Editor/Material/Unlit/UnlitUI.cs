using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    class UnlitGUI : BaseUnlitGUI
    {
        protected override uint defaultExpandedState { get { return (uint)(Expandable.Base | Expandable.Input | Expandable.Transparency); }  }

        protected static class Styles
        {
            public static string InputsText = "Surface Inputs";

            public static GUIContent colorText = new GUIContent("Color", " Albedo (RGB) and Transparency (A).");

            public static string emissiveLabelText = "Emission Inputs";

            public static GUIContent emissiveText = new GUIContent("Emissive Color", "Emissive Color (RGB).");            

            // Emissive
            public static GUIContent albedoAffectEmissiveText = new GUIContent("Emission multiply with Base", "Specifies whether or not the emission color is multiplied by the albedo.");
            public static GUIContent useEmissiveIntensityText = new GUIContent("Use Emission Intensity", "Specifies whether to use to a HDR color or a LDR color with a separate multiplier.");
            public static GUIContent emissiveIntensityText = new GUIContent("Emission Intensity", "");
            public static GUIContent emissiveIntensityFromHDRColorText = new GUIContent("The emission intensity is from the HDR color picker in luminance", "");
            public static GUIContent emissiveExposureWeightText = new GUIContent("Exposure weight", "Control the percentage of emission to expose.");
        }

        protected MaterialProperty color = null;
        protected const string kColor = "_UnlitColor";
        protected MaterialProperty colorMap = null;
        protected const string kColorMap = "_UnlitColorMap";
        protected MaterialProperty emissiveColor = null;
        protected const string kEmissiveColor = "_EmissiveColor";
        protected MaterialProperty emissiveColorMap = null;
        protected const string kEmissiveColorMap = "_EmissiveColorMap";

        protected MaterialProperty emissiveColorLDR = null;
        protected const string kEmissiveColorLDR = "_EmissiveColorLDR";
        protected MaterialProperty emissiveExposureWeight = null;
        protected const string kemissiveExposureWeight = "_EmissiveExposureWeight";
        protected MaterialProperty useEmissiveIntensity = null;
        protected const string kUseEmissiveIntensity = "_UseEmissiveIntensity";
        protected MaterialProperty emissiveIntensityUnit = null;
        protected const string kEmissiveIntensityUnit = "_EmissiveIntensityUnit";
        protected MaterialProperty emissiveIntensity = null;
        protected const string kEmissiveIntensity = "_EmissiveIntensity";

        protected const string kStencilRef = "_StencilRef";
        protected const string kStencilWriteMask = "_StencilWriteMask";
        protected const string kStencilRefDepth = "_StencilRefDepth";
        protected const string kStencilWriteMaskDepth = "_StencilWriteMaskDepth";
        protected const string kStencilRefMV = "_StencilRefMV";
        protected const string kStencilWriteMaskMV = "_StencilWriteMaskMV";

        override protected void FindMaterialProperties(MaterialProperty[] props)
        {
            color = FindProperty(kColor, props);
            colorMap = FindProperty(kColorMap, props);

            emissiveColor = FindProperty(kEmissiveColor, props);
            emissiveColorMap = FindProperty(kEmissiveColorMap, props);
            emissiveIntensityUnit = FindProperty(kEmissiveIntensityUnit, props);
            emissiveIntensity = FindProperty(kEmissiveIntensity, props);
            emissiveExposureWeight = FindProperty(kemissiveExposureWeight, props);
            emissiveColorLDR = FindProperty(kEmissiveColorLDR, props);
            useEmissiveIntensity = FindProperty(kUseEmissiveIntensity, props);
        }

        protected override void MaterialPropertiesGUI(Material material)
        {
            using (var header = new HeaderScope(Styles.InputsText, (uint)Expandable.Input, this))
            {
                if (header.expanded)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.colorText, colorMap, color);
                    m_MaterialEditor.TextureScaleOffsetProperty(colorMap);
                }
            }

            var surfaceTypeValue = (SurfaceType)surfaceType.floatValue;
            if (surfaceTypeValue == SurfaceType.Transparent)
            {
                using (var header = new HeaderScope(StylesBaseUnlit.TransparencyInputsText, (uint)Expandable.Transparency, this))
                {
                    if (header.expanded)
                    {
                        DoDistortionInputsGUI();
                    }
                }
            }

            using (var header = new HeaderScope(Styles.emissiveLabelText, (uint)Expandable.Emissive, this))
            {
                if (header.expanded)
                {
                    EditorGUI.BeginChangeCheck();
                    m_MaterialEditor.ShaderProperty(useEmissiveIntensity, Styles.useEmissiveIntensityText);
                    bool updateEmissiveColor = EditorGUI.EndChangeCheck();
                        
                    if (useEmissiveIntensity.floatValue == 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        DoEmissiveTextureProperty(material, emissiveColor);
                        if (EditorGUI.EndChangeCheck() || updateEmissiveColor)
                            emissiveColor.colorValue = emissiveColor.colorValue;
                        EditorGUILayout.HelpBox(Styles.emissiveIntensityFromHDRColorText.text, MessageType.Info, true);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            DoEmissiveTextureProperty(material, emissiveColorLDR);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EmissiveIntensityUnit unit = (EmissiveIntensityUnit)emissiveIntensityUnit.floatValue;

                                if (unit == EmissiveIntensityUnit.Luminance)
                                    m_MaterialEditor.ShaderProperty(emissiveIntensity, Styles.emissiveIntensityText);
                                else
                                {
                                    float evValue = LightUtils.ConvertLuminanceToEv(emissiveIntensity.floatValue);
                                    evValue = EditorGUILayout.FloatField(Styles.emissiveIntensityText, evValue);
                                    emissiveIntensity.floatValue = LightUtils.ConvertEvToLuminance(evValue);
                                }
                                emissiveIntensityUnit.floatValue = (float)(EmissiveIntensityUnit)EditorGUILayout.EnumPopup(unit);
                            }
                        }
                        if (EditorGUI.EndChangeCheck() || updateEmissiveColor)
                            emissiveColor.colorValue = emissiveColorLDR.colorValue * emissiveIntensity.floatValue;
                    }

                    m_MaterialEditor.ShaderProperty(emissiveExposureWeight, Styles.emissiveExposureWeightText);

                    DoEmissionArea(material);
                }
            }
        }

        void DoEmissiveTextureProperty(Material material, MaterialProperty color)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.emissiveText, emissiveColorMap, color);
            m_MaterialEditor.TextureScaleOffsetProperty(emissiveColorMap);
        }

        protected override void MaterialPropertiesAdvanceGUI(Material material)
        {
        }

        protected override void VertexAnimationPropertiesGUI()
        {
        }

        protected override bool ShouldEmissionBeEnabled(Material material)
        {
            return (material.GetColor(kEmissiveColor) != Color.black) || material.GetTexture(kEmissiveColorMap);
        }

        protected override void SetupMaterialKeywordsAndPassInternal(Material material)
        {
            SetupMaterialKeywordsAndPass(material);
        }

        // All Setup Keyword functions must be static. It allow to create script to automatically update the shaders with a script if code change
        static public void SetupMaterialKeywordsAndPass(Material material)
        {
            SetupBaseUnlitKeywords(material);
            SetupBaseUnlitMaterialPass(material);

            CoreUtils.SetKeyword(material, "_EMISSIVE_COLOR_MAP", material.GetTexture(kEmissiveColorMap));

            // Stencil usage rules:
            // DoesntReceiveSSR and DecalsForwardOutputNormalBuffer need to be tagged during depth prepass
            // LightingMask need to be tagged during either GBuffer or Forward pass
            // ObjectVelocity need to be tagged in velocity pass.
            // As velocity pass can be use as a replacement of depth prepass it also need to have DoesntReceiveSSR and DecalsForwardOutputNormalBuffer
            // As GBuffer pass can have no depth prepass, it also need to have DoesntReceiveSSR and DecalsForwardOutputNormalBuffer
            // Object velocity is always render after a full depth buffer (if there is no depth prepass for GBuffer all object motion vectors are render after GBuffer)
            // so we have a guarantee than when we write object velocity no other object will be draw on top (and so would have require to overwrite velocity).
            // Final combination is:
            // Prepass: DoesntReceiveSSR,  DecalsForwardOutputNormalBuffer
            // Motion vectors: DoesntReceiveSSR,  DecalsForwardOutputNormalBuffer, ObjectVelocity
            // Forward: LightingMask

            int stencilRef = (int)StencilLightingUsage.NoLighting;
            int stencilWriteMask = (int)HDRenderPipeline.StencilBitMask.LightingMask;
            int stencilRefDepth = (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR;
            int stencilWriteMaskDepth = (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR | (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;
            int stencilRefMV = (int)HDRenderPipeline.StencilBitMask.ObjectVelocity | (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR;
            int stencilWriteMaskMV = (int)HDRenderPipeline.StencilBitMask.ObjectVelocity | (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR | (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;

            // As we tag both during velocity pass and Gbuffer pass we need a separate state and we need to use the write mask
            material.SetInt(kStencilRef, stencilRef);
            material.SetInt(kStencilWriteMask, stencilWriteMask);
            material.SetInt(kStencilRefDepth, stencilRefDepth);
            material.SetInt(kStencilWriteMaskDepth, stencilWriteMaskDepth);
            material.SetInt(kStencilRefMV, stencilRefMV);
            material.SetInt(kStencilWriteMaskMV, stencilWriteMaskMV);
        }
    }
} // namespace UnityEditor
