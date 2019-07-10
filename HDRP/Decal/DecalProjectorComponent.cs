﻿using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [ExecuteInEditMode]
    public class DecalProjectorComponent : MonoBehaviour
    {
        public Material m_Material = null;
		public float m_DrawDistance = 1000.0f;
        public float m_FadeScale = 0.9f;
        private Material m_OldMaterial = null;
        public const int kInvalidIndex = -1;  
        private int m_CullIndex = kInvalidIndex;

        public int CullIndex
        {
            get
            {
                return this.m_CullIndex;
            }
            set
            {
                this.m_CullIndex = value;
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
                var hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
                m_Material = hdrp != null ? hdrp.GetDefaultDecalMaterial() : null;
            }

            DecalSystem.instance.AddDecal(this);
        }

        public void Start()
        {
            DecalSystem.instance.AddDecal(this);
        }

        public void OnDisable()
        {
            DecalSystem.instance.RemoveDecal(this);
        }

		// Declare the method signature of the delegate to call.	
		public delegate void OnMaterialChangeDelegate();

		// Declare the event to which editor code will hook itself.
		public event OnMaterialChangeDelegate OnMaterialChange;

        public void OnValidate()
        {
            // handle material changes
            if (m_OldMaterial != m_Material)
            {
                Material tempMaterial = m_Material;
                m_Material = m_OldMaterial;
                if(m_Material != null)
                    DecalSystem.instance.RemoveDecal(this);
                m_Material = tempMaterial;
                DecalSystem.instance.AddDecal(this);
                m_OldMaterial = m_Material;

                // notify the editor that material has changed so it can update the shader foldout
				if (OnMaterialChange != null)
				{
					OnMaterialChange();
				}
            }

            if (m_Material != null)
            {
                Shader shader = m_Material.shader;
                if((shader != null) &&  (shader.name != "HDRenderPipeline/Decal"))
                {
                    Debug.LogWarning("Decal projector component material is not using HDRenderPipeline/Decal shader.", this);
                }
            }
        }

        private void DrawGizmo(bool selected)
        {
            var col = new Color(0.0f, 0.7f, 1f, 1.0f);
            col.a = selected ? 0.3f : 0.1f;
            Gizmos.color = col;
            Matrix4x4 offset = Matrix4x4.Translate(new Vector3(0.0f, -0.5f, 0.0f));
            Gizmos.matrix = transform.localToWorldMatrix * offset;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            col.a = selected ? 0.5f : 0.2f;
            Gizmos.color = col;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        public void OnDrawGizmosSelected()
        {
            DrawGizmo(true);
            // if this object is selected there is a chance the transform was changed so update culling info
            DecalSystem.instance.UpdateCachedData(this);
        }

        public void OnDrawGizmos()
        {
            DrawGizmo(false);
        }

        public bool IsValid()
        {
            // don't draw if no material or if material is the default decal material (empty)
            if (m_Material == null)
                return false;

            var hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
            if ((hdrp != null) && (m_Material == hdrp.GetDefaultDecalMaterial()))
                return false;

            return true;
        }
    }
}
