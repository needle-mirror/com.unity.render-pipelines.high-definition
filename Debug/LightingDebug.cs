﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL]
    public enum DebugLightingMode
    {
        None,
        DiffuseLighting,
        SpecularLighting,
        VisualizeCascade,
        IndirectDiffuseOcclusionFromSsao,
        IndirectDiffuseGtaoFromSsao,
        IndirectSpecularOcclusionFromSsao,
        IndirectSpecularGtaoFromSsao
    }

    public enum ShadowMapDebugMode
    {
        None,
        VisualizeAtlas,
        VisualizeShadowMap
    }

    [Serializable]
    public class LightingDebugSettings
    {
        public bool IsDebugDisplayEnabled()
        {
            return debugLightingMode != DebugLightingMode.None;
        }

        public DebugLightingMode    debugLightingMode = DebugLightingMode.None;
        public bool                 enableShadows = true;
        public ShadowMapDebugMode   shadowDebugMode = ShadowMapDebugMode.None;
        public bool                 shadowDebugUseSelection = false;
        public uint                 shadowMapIndex = 0;
        public uint                 shadowAtlasIndex = 0;
        public float                shadowMinValue = 0.0f;
        public float                shadowMaxValue = 1.0f;

        public bool                 overrideSmoothness = false;
        public float                overrideSmoothnessValue = 0.5f;
        public Color                debugLightingAlbedo = new Color(0.5f, 0.5f, 0.5f);

        public bool                 displaySkyReflection = false;
        public float                skyReflectionMipmap = 0.0f;

        public LightLoopSettings.TileClusterDebug tileClusterDebug = LightLoopSettings.TileClusterDebug.None;
        public LightLoopSettings.TileClusterCategoryDebug tileClusterDebugByCategory = LightLoopSettings.TileClusterCategoryDebug.Punctual;

        public void OnValidate()
        {
            overrideSmoothnessValue = Mathf.Clamp(overrideSmoothnessValue, 0.0f, 1.0f);
            skyReflectionMipmap = Mathf.Clamp(skyReflectionMipmap, 0.0f, 1.0f);
        }
    }
}
