using UnityEngine.Serialization;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [RequireComponent(typeof(ReflectionProbe))]
    public class HDAdditionalReflectionData : HDProbe, ISerializationCallbackReceiver
    {
        enum Version
        {
            First,
            Second,
            HDProbeChild,
            UseInfluenceVolume,
            MergeEditors,
            // Add new version here and they will automatically be the Current one
            Max,
            Current = Max - 1
        }

        [SerializeField, FormerlySerializedAs("version")]
        int m_Version;

        ReflectionProbe m_LegacyProbe;
        /// <summary>Get the sibling component ReflectionProbe</summary>
        public ReflectionProbe reflectionProbe
        {
            get
            {
                if(m_LegacyProbe == null || m_LegacyProbe.Equals(null))
                {
                    m_LegacyProbe = GetComponent<ReflectionProbe>();
                }
                return m_LegacyProbe;
            }
        }

#pragma warning disable 649 //never assigned
        //data only kept for migration, to be removed in future version
        [SerializeField, System.Obsolete("influenceShape is deprecated, use influenceVolume parameters instead")]
        InfluenceShape influenceShape;
        [SerializeField, System.Obsolete("influenceSphereRadius is deprecated, use influenceVolume parameters instead")]
        float influenceSphereRadius = 3.0f;
        [SerializeField, System.Obsolete("blendDistancePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 blendDistancePositive = Vector3.zero;
        [SerializeField, System.Obsolete("blendDistanceNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 blendDistanceNegative = Vector3.zero;
        [SerializeField, System.Obsolete("blendNormalDistancePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 blendNormalDistancePositive = Vector3.zero;
        [SerializeField, System.Obsolete("blendNormalDistanceNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 blendNormalDistanceNegative = Vector3.zero;
        [SerializeField, System.Obsolete("boxSideFadePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 boxSideFadePositive = Vector3.one;
        [SerializeField, System.Obsolete("boxSideFadeNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 boxSideFadeNegative = Vector3.one;
#pragma warning restore 649 //never assigned

        bool needMigrateToHDProbeChild = false;
        bool needMigrateToUseInfluenceVolume = false;
        bool needMigrateToMergeEditors = false;

        public void CopyTo(HDAdditionalReflectionData data)
        {
            influenceVolume.CopyTo(data.influenceVolume);
            data.influenceVolume.shape = influenceVolume.shape; //force the legacy probe to refresh its size

            data.mode = mode;
            data.refreshMode = refreshMode;
            data.multiplier = multiplier;
            data.weight = weight;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (m_Version != (int)Version.Current)
            {
                // Add here data migration code that use other component
                // Note impossible to access other component at deserialization time
                if (m_Version < (int)Version.HDProbeChild)
                {
                    needMigrateToHDProbeChild = true;
                }
                else if (m_Version < (int)Version.UseInfluenceVolume)
                {
                    needMigrateToUseInfluenceVolume = true;
                }
                else if (m_Version < (int)Version.MergeEditors)
                {
                    needMigrateToMergeEditors = true;
                }
                else
                {
                    // Add here data migration code that do not use other component
                    m_Version = (int)Version.Current;
                }
            }
        }

        void OnEnable()
        {
            if (needMigrateToHDProbeChild)
                MigrateToHDProbeChild();
            if (needMigrateToUseInfluenceVolume)
                MigrateToUseInfluenceVolume();
            if (needMigrateToMergeEditors)
                MigrateToMergeEditors();
            
            ReflectionSystem.RegisterProbe(this);
        }

        void OnDisable()
        {
            ReflectionSystem.UnregisterProbe(this);
        }

        void OnValidate()
        {
            ReflectionSystem.UnregisterProbe(this);

            if (isActiveAndEnabled)
                ReflectionSystem.RegisterProbe(this);
        }

        void MigrateToHDProbeChild()
        {
            mode = reflectionProbe.mode;
            refreshMode = reflectionProbe.refreshMode;
            m_Version = (int)Version.HDProbeChild;
            needMigrateToHDProbeChild = false;
            OnAfterDeserialize();   //continue migrating if needed
        }

        void MigrateToUseInfluenceVolume()
        {
            influenceVolume.boxSize = reflectionProbe.size;
#pragma warning disable CS0618 // Type or member is obsolete
            influenceVolume.sphereRadius = influenceSphereRadius;
            influenceVolume.shape = influenceShape; //must be done after each size transfert
            influenceVolume.boxBlendDistancePositive = blendDistancePositive;
            influenceVolume.boxBlendDistanceNegative = blendDistanceNegative;
            influenceVolume.boxBlendNormalDistancePositive = blendNormalDistancePositive;
            influenceVolume.boxBlendNormalDistanceNegative = blendNormalDistanceNegative;
            influenceVolume.boxSideFadePositive = boxSideFadePositive;
            influenceVolume.boxSideFadeNegative = boxSideFadeNegative;
#pragma warning restore CS0618 // Type or member is obsolete
            m_Version = (int)Version.UseInfluenceVolume;
            needMigrateToUseInfluenceVolume = false;
            OnAfterDeserialize();   //continue migrating if needed

            //Note: former editor parameters will be recreated as if non existent.
            //User will lose parameters corresponding to non used mode between simplified and advanced
        }

        void MigrateToMergeEditors()
        {
            infiniteProjection = !reflectionProbe.boxProjection;
            reflectionProbe.boxProjection = false;
            m_Version = (int)Version.MergeEditors;
            needMigrateToMergeEditors = false;
            OnAfterDeserialize();   //continue migrating if needed
        }

        public override ReflectionProbeMode mode
        {
            set
            {
                base.mode = value;
                reflectionProbe.mode = value; //ensure compatibility till we capture without the legacy component
                if(value == ReflectionProbeMode.Realtime)
                {
                    refreshMode = ReflectionProbeRefreshMode.EveryFrame;
                }
            }
        }

        public override ReflectionProbeRefreshMode refreshMode
        {
            set
            {
                base.refreshMode = value;
                reflectionProbe.refreshMode = value; //ensure compatibility till we capture without the legacy component
            }
        }

        internal override void UpdatedInfluenceVolumeShape(Vector3 size, Vector3 offset)
        {
            reflectionProbe.size = size;
            reflectionProbe.center = transform.rotation*offset;
        }
    }
}
