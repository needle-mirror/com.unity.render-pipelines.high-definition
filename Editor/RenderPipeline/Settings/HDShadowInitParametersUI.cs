using UnityEngine.Experimental.Rendering;
using UnityEditor.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using CED = CoreEditorDrawer<SerializedHDShadowInitParameters>;
    
    static class HDShadowInitParametersUI
    {
        enum ShadowResolution
        {
            ShadowResolution128 = 128,
            ShadowResolution256 = 256,
            ShadowResolution512 = 512,
            ShadowResolution1024 = 1024,
            ShadowResolution2048 = 2048,
            ShadowResolution4096 = 4096,
            ShadowResolution8192 = 8192,
            ShadowResolution16384 = 16384
        }
        
        enum Expandable
        {
            ShadowSettings = 1 << 0
        }

        readonly static ExpandedState<Expandable, HDShadowInitParameters> k_ExpandedState = new ExpandedState<Expandable, HDShadowInitParameters>(Expandable.ShadowSettings, "HDRP");

        static readonly GUIContent k_HeaderContent = EditorGUIUtility.TrTextContent("Shadows");

        static readonly GUIContent k_AtlasContent = EditorGUIUtility.TrTextContent("Atlas");
        static readonly GUIContent k_ResolutionContent = EditorGUIUtility.TrTextContent("Resolution");
        static readonly GUIContent k_Map16bContent = EditorGUIUtility.TrTextContent("16-bit");
        static readonly GUIContent k_DynamicRescaleContent = EditorGUIUtility.TrTextContent("Dynamic Rescale", "Scale the shadow map size using the screen size of the light to leave more space for other shadows in the atlas");
        static readonly GUIContent k_MaxRequestContent = EditorGUIUtility.TrTextContent("Max Shadow on Screen", "Max shadow on screen (S) per frame, 1 point light = 6 S, 1 spot light = 1 S and the directional is 4 S");
        static readonly GUIContent k_FilteringQualityContent = EditorGUIUtility.TrTextContent("Filtering Qualities");
        static readonly GUIContent k_QualityContent = EditorGUIUtility.TrTextContent("ShadowQuality");

        public static readonly CED.IDrawer Inspector = CED.FoldoutGroup(
            k_HeaderContent,
            Expandable.ShadowSettings,
            k_ExpandedState,
            Drawer_FieldHDShadows
            );
        
        static void Drawer_FieldHDShadows(SerializedHDShadowInitParameters serialized, Editor owner)
        {
            EditorGUILayout.LabelField(k_AtlasContent, EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            serialized.shadowAtlasResolution.intValue = (int)(ShadowResolution)EditorGUILayout.EnumPopup(k_ResolutionContent, (ShadowResolution)serialized.shadowAtlasResolution.intValue);
            
            bool shadowMap16Bits = (DepthBits)serialized.shadowMapDepthBits.intValue	== DepthBits.Depth16;
            shadowMap16Bits = EditorGUILayout.Toggle(k_Map16bContent, shadowMap16Bits);
            serialized.shadowMapDepthBits.intValue = (shadowMap16Bits) ? (int)DepthBits.Depth16 : (int)DepthBits.Depth32;
            EditorGUILayout.PropertyField(serialized.useDynamicViewportRescale, k_DynamicRescaleContent);
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();
            
            EditorGUILayout.DelayedIntField(serialized.maxShadowRequests, k_MaxRequestContent);
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(k_FilteringQualityContent, EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(serialized.shadowQuality, k_QualityContent);
            --EditorGUI.indentLevel;
        }
    }
}
