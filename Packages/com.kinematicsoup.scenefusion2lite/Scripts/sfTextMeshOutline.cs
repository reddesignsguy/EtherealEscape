﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KS.SceneFusion
{
    /**
     * Creates an outline for a text mesh by creating 4 copies of the text. You must call CreateOutline to create the
     * outline. You can call it again to refresh the outline if the properties of the TextMesh change.
     */
    [RequireComponent(typeof(TextMesh))]
    public class sfTextMeshOutline : MonoBehaviour
    {
        /**
         * Thickness of the outline. For this to take affect, you must call CreateOutline after setting it.
         */
        public float Thickness = .01f;

        /**
         * Z-offset of the outline, to make the outline appear behind the text. For this to take affect, you must call
         * CreateOutline after setting it.
         */
        public float OffsetZ = .001f;

        /**
         * Color of the outline.
         */
        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                if (m_outlineTexts != null)
                {
                    foreach (TextMesh outline in m_outlineTexts)
                    {
                        if (outline != null)
                        {
                            outline.color = value;
                        }
                    }
                }
            }
        }
        [SerializeField]
        private Color m_color = Color.black;

        private TextMesh[] m_outlineTexts;

        /**
         * Creates the child TextMeshes that render the outline. If the outline was already created, destroys the
         * existing outline TextMeshes and recreates them.
         */
        public void CreateOutline()
        {
            if (m_outlineTexts != null)
            {
                foreach (TextMesh outline in m_outlineTexts)
                {
                    if (outline != null)
                    {
                        DestroyImmediate(outline.gameObject);
                    }
                }
            }
            TextMesh text = GetComponent<TextMesh>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (text == null || renderer == null)
            {
                return;
            }
            m_outlineTexts = new TextMesh[4];
            m_outlineTexts[0] = CreateOutlineCopy(text, renderer.sharedMaterial, new Vector2(1f, 1f) * Thickness);
            m_outlineTexts[1] = CreateOutlineCopy(text, renderer.sharedMaterial, new Vector2(-1f, 1f) * Thickness);
            m_outlineTexts[2] = CreateOutlineCopy(text, renderer.sharedMaterial, new Vector2(1f, -1f) * Thickness);
            m_outlineTexts[3] = CreateOutlineCopy(text, renderer.sharedMaterial, new Vector2(-1f, -1f) * Thickness);
        }

        /**
         * Creates a child copy of a text mesh to be part of the outline.
         * 
         * @param   TextMesh text to copy.
         * @param   Material material for the mesh renderer.
         * @param   Vector2 offset to add to the text position to get the outline's position.
         * @return  TextMesh outline copy.
         */
        private TextMesh CreateOutlineCopy(TextMesh text, Material material, Vector2 offset)
        {
            GameObject child = new GameObject("outline");
            child.transform.parent = transform;
            child.transform.localPosition = new Vector3(offset.x, offset.y, Thickness);
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            child.hideFlags = HideFlags.HideAndDontSave;

            TextMesh copy = child.AddComponent<TextMesh>();
            copy.alignment = text.alignment;
            copy.anchor = text.anchor;
            copy.characterSize = text.characterSize;
            copy.color = Color;
            copy.font = text.font;
            copy.fontSize = text.fontSize;
            copy.fontStyle = text.fontStyle;
            copy.lineSpacing = text.lineSpacing;
            copy.richText = text.richText;
            copy.text = text.text;

            child.GetComponent<MeshRenderer>().sharedMaterial = material;

            return copy;
        }
    }
}
