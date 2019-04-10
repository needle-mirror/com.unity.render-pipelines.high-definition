using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [ExecuteAlways]
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class DecalProjectorComponent : MonoBehaviour
    {
        [Tooltip("Specifies the Material this component projects as a decal.")]
        public Material m_Material = null;
        [Tooltip("Sets the distance from the Camera at which HDRP stop rendering the decal.")]
        public float m_DrawDistance = 1000.0f;
        [Tooltip("Controls the distance from the Camera at which this component begins to fade the decal out. Expressed as a percentage of Fade Distance.")]
        public float m_FadeScale = 0.9f;
        [Tooltip("Sets the scale for the decal Material. Scales the decal along its UV axes.")]
        public Vector2 m_UVScale = new Vector2(1, 1);
        [Tooltip("Sets the offset for the decal Material. Moves the decal along its UV axes.")]
        public Vector2 m_UVBias = new Vector2(0, 0);
        [Tooltip("When enabled, HDRP draws this projector's decal on top of transparent surfaces.")]
        public bool m_AffectsTransparency = false;
        public Vector3 m_Offset = new Vector3(0, -0.5f, 0);
        [Tooltip("Sets the size of the projector.")]
        public Vector3 m_Size = new Vector3(1, 1, 1);
        private Material m_OldMaterial = null;
        private DecalSystem.DecalHandle m_Handle = null;
        [Tooltip("When enabled, the Scene view gizmo crops the decal instead of scaling it.")]
        public bool m_IsCropModeEnabled = false;
        public float m_FadeFactor = 1.0f;

        public DecalSystem.DecalHandle Handle
        {
            get
            {
                return this.m_Handle;
            }
            set
            {
                this.m_Handle = value;
            }
        }

        public Material Mat
        {
            get { return this.m_Material; }
        }

        public void OnEnable()
        {
            if (m_Material == null)
            {
#if UNITY_EDITOR
                var hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
                m_Material = hdrp != null ? hdrp.GetDefaultDecalMaterial() : null;
#else
                m_Material = null;
#endif
            }

            if (m_Handle != null)
            {
                DecalSystem.instance.RemoveDecal(m_Handle);
                m_Handle = null;
            }

            Vector4 uvScaleBias = new Vector4(m_UVScale.x, m_UVScale.y, m_UVBias.x, m_UVBias.y);
            Matrix4x4 sizeOffset = Matrix4x4.Translate(m_Offset) * Matrix4x4.Scale(m_Size);
            m_Handle = DecalSystem.instance.AddDecal(transform, sizeOffset, m_DrawDistance, m_FadeScale, uvScaleBias, m_AffectsTransparency, m_Material, gameObject.layer, m_FadeFactor);
        }

        public void OnDisable()
        {
            if (m_Handle != null)
            {
                DecalSystem.instance.RemoveDecal(m_Handle);
                m_Handle = null;
            }
        }

        // Declare the method signature of the delegate to call.
        public delegate void OnMaterialChangeDelegate();

        // Declare the event to which editor code will hook itself.
        public event OnMaterialChangeDelegate OnMaterialChange;

        public void OnValidate()
        {
            if (m_Handle != null) // don't do anything if OnEnable hasn't been called yet when scene is loading.
            {
                Vector4 uvScaleBias = new Vector4(m_UVScale.x, m_UVScale.y, m_UVBias.x, m_UVBias.y);
                Matrix4x4 sizeOffset = Matrix4x4.Translate(m_Offset) * Matrix4x4.Scale(m_Size);
                // handle material changes, because decals are stored as sets sorted by material, if material changes decal needs to be removed and re-added to that it goes into correct set
                if (m_OldMaterial != m_Material)
                {
                    DecalSystem.instance.RemoveDecal(m_Handle);
                    m_Handle = DecalSystem.instance.AddDecal(transform, sizeOffset, m_DrawDistance, m_FadeScale, uvScaleBias, m_AffectsTransparency, m_Material, gameObject.layer, m_FadeFactor);
                    m_OldMaterial = m_Material;

                    if(!DecalSystem.IsHDRenderPipelineDecal(m_Material.shader.name)) // non HDRP/decal shaders such as shader graph decal do not affect transparency
                    {
                        m_AffectsTransparency = false;
                    }

                    // notify the editor that material has changed so it can update the shader foldout
                    if (OnMaterialChange != null)
                    {
                        OnMaterialChange();
                    }
                }
                else // no material change, just update whatever else changed
                {
                    DecalSystem.instance.UpdateCachedData(transform, sizeOffset, m_DrawDistance, m_FadeScale, uvScaleBias, m_AffectsTransparency, m_Handle, gameObject.layer, m_FadeFactor);
                }
            }
        }

        public void LateUpdate()
        {
            if (m_Handle != null)
            {
                if (transform.hasChanged == true)
                {
                    Vector4 uvScaleBias = new Vector4(m_UVScale.x, m_UVScale.y, m_UVBias.x, m_UVBias.y);
                    Matrix4x4 sizeOffset = Matrix4x4.Translate(m_Offset) * Matrix4x4.Scale(m_Size);
                    DecalSystem.instance.UpdateCachedData(transform, sizeOffset, m_DrawDistance, m_FadeScale, uvScaleBias, m_AffectsTransparency, m_Handle, gameObject.layer, m_FadeFactor);
                    transform.hasChanged = false;
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            // if this object is selected there is a chance the transform was changed so update culling info
            Vector4 uvScaleBias = new Vector4(m_UVScale.x, m_UVScale.y, m_UVBias.x, m_UVBias.y);
            Matrix4x4 sizeOffset = Matrix4x4.Translate(m_Offset) * Matrix4x4.Scale(m_Size);
            DecalSystem.instance.UpdateCachedData(transform, sizeOffset, m_DrawDistance, m_FadeScale, uvScaleBias, m_AffectsTransparency, m_Handle, gameObject.layer, m_FadeFactor);
        }

        public void OnDrawGizmos()
        {
            var col = new Color(0.0f, 0.7f, 1f, 0.5f);
            Matrix4x4 offsetScale = Matrix4x4.Translate(m_Offset) * Matrix4x4.Scale(m_Size);
            Gizmos.matrix = transform.localToWorldMatrix * offsetScale;
            Gizmos.color = col;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        public bool IsValid()
        {
            // don't draw if no material or if material is the default decal material (empty)
            if (m_Material == null)
                return false;

#if UNITY_EDITOR
            var hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
            if ((hdrp != null) && (m_Material == hdrp.GetDefaultDecalMaterial()))
                return false;
#endif

            return true;
        }
    }
}
