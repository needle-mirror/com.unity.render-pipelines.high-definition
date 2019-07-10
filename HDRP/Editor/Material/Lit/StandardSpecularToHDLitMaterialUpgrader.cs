using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public class StandardSpecularToHDLitMaterialUpgrader : MaterialUpgrader
    {
        public StandardSpecularToHDLitMaterialUpgrader() : this("Standard (Specular setup)", "HDRenderPipeline/Lit", LitGUI.SetupMaterialKeywordsAndPass) {}

        public StandardSpecularToHDLitMaterialUpgrader(string sourceShaderName, string destShaderName, MaterialFinalizer finalizer)
        {
            RenameShader(sourceShaderName, destShaderName, finalizer);

            RenameTexture("_MainTex", "_BaseColorMap");
            RenameColor("_Color", "_BaseColor");
            RenameFloat("_Glossiness", "_Smoothness");
            RenameTexture("_BumpMap", "_NormalMap");
            RenameFloat("_BumpScale", "_NormalScale");
            RenameColor("_EmissionColor", "_EmissiveColor");
            RenameFloat("_DetailNormalMapScale", "_DetailNormalScale");
            RenameFloat("_Cutoff", "_AlphaCutoff");
            RenameKeywordToFloat("_ALPHATEST_ON", "_AlphaCutoffEnable", 1f, 0f);

            // the HD renderloop packs detail albedo and detail normals into a single texture.
            // mapping the detail normal map, if any, to the detail map, should do the right thing if
            // there is no detail albedo.
            RenameTexture("_DetailNormalMap", "_DetailMap");

            // Anything reasonable that can be done here?
            //RenameFloat("_SpecColor", ...);

            //@TODO: Seb. Why do we multiply color by intensity
            //       in shader when we can just store a color?
            // builtinData.emissiveColor * builtinData.emissiveIntensity
        }

        public override void Convert(Material srcMaterial, Material dstMaterial)
        {
            base.Convert(srcMaterial, dstMaterial);
            //@TODO: Find a good way of setting up keywords etc from properties.
            // Code should be shared with material UI code.
        }
    }
}
