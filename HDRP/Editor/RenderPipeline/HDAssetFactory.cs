using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using UnityObject = UnityEngine.Object;

    static class HDAssetFactory
    {
        static string s_RenderPipelineResourcesPath
        {
            get { return HDUtils.GetHDRenderPipelinePath() + "RenderPipelineResources/HDRenderPipelineResources.asset"; }
        }

        class DoCreateNewAssetHDRenderPipeline : ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var newAsset = CreateInstance<HDRenderPipelineAsset>();
                newAsset.name = Path.GetFileName(pathName);
                // Load default renderPipelineResources / Material / Shader
                newAsset.renderPipelineResources = AssetDatabase.LoadAssetAtPath<RenderPipelineResources>(s_RenderPipelineResourcesPath);
                AssetDatabase.CreateAsset(newAsset, pathName);
                ProjectWindowUtil.ShowCreatedAsset(newAsset);
            }
        }

        [MenuItem("Assets/Create/Rendering/High Definition Render Pipeline Asset", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateHDRenderPipeline()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateNewAssetHDRenderPipeline>(), "New HDRenderPipelineAsset.asset", icon, null);
        }

        // Note: move this to a static using once we can target C#6+
        static T Load<T>(string path) where T : UnityObject
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        class DoCreateNewAssetHDRenderPipelineResources : ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var newAsset = CreateInstance<RenderPipelineResources>();
                newAsset.name = Path.GetFileName(pathName);

                newAsset.Init();
                
                AssetDatabase.CreateAsset(newAsset, pathName);
                ProjectWindowUtil.ShowCreatedAsset(newAsset);
            }
        }

        [MenuItem("Assets/Create/Rendering/High Definition Render Pipeline Resources", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateRenderPipelineResources()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateNewAssetHDRenderPipelineResources>(), "New HDRenderPipelineResources.asset", icon, null);
        }
    }
}
