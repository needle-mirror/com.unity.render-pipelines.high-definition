namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // The HDRenderPipeline assumes linear lighting. Doesn't work with gamma.
    public class HDRenderPipelineAsset : RenderPipelineAsset
    {
        HDRenderPipelineAsset()
        {
        }

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new HDRenderPipeline(this);
        }

        [SerializeField]
        RenderPipelineResources m_RenderPipelineResources;
        public RenderPipelineResources renderPipelineResources
        {
            get { return m_RenderPipelineResources; }
            set { m_RenderPipelineResources = value; }
        }

        // NOTE: All those properties are public because of how HDRenderPipelineInspector retrieves those properties via serialization/reflection
        // Doing it this way allows to change parameters name and still retrieve correct serialized values

        // Global Renderer Settings
        public GlobalRenderingSettings globalRenderingSettings = new GlobalRenderingSettings();
        public GlobalTextureSettings globalTextureSettings = new GlobalTextureSettings();
        public SubsurfaceScatteringSettings sssSettings;
        public LightLoopSettings lightLoopSettings = new LightLoopSettings();

        // Shadow Settings
        public ShadowInitParameters shadowInitParams = new ShadowInitParameters();

        // Default Material / Shader
        [SerializeField]
        Material m_DefaultDiffuseMaterial;
        [SerializeField]
        Shader m_DefaultShader;

        public Material defaultDiffuseMaterial
        {
            get { return m_DefaultDiffuseMaterial; }
            set { m_DefaultDiffuseMaterial = value; }
        }

        public Shader defaultShader
        {
            get { return m_DefaultShader; }
            set { m_DefaultShader = value; }
        }

        public override Shader GetDefaultShader()
        {
            return m_DefaultShader;
        }

        public override Material GetDefaultMaterial()
        {
            return m_DefaultDiffuseMaterial;
        }

        public override Material GetDefaultParticleMaterial()
        {
            return null;
        }

        public override Material GetDefaultLineMaterial()
        {
            return null;
        }

        public override Material GetDefaultTerrainMaterial()
        {
            return null;
        }

        public override Material GetDefaultUIMaterial()
        {
            return null;
        }

        public override Material GetDefaultUIOverdrawMaterial()
        {
            return null;
        }

        public override Material GetDefaultUIETC1SupportedMaterial()
        {
            return null;
        }

        public override Material GetDefault2DMaterial()
        {
            return null;
        }

        public void OnValidate()
        {
        }
    }
}
